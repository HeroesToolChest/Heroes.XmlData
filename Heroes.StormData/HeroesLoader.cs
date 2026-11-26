namespace Heroes.StormData;

/// <summary>
/// Loads the Heroes of the Storm data (the <c>stormmods</c>, a <c>mapmod</c>, and gamestrings) from extracted files (<see cref="LoadWithFile"/>)
/// or from CASC storage (<see cref="LoadWithCASC"/>), and gives access to the source's files.
/// </summary>
/// <remarks>
/// <para>
/// The load methods only do work when the requested state differs from the current state, and they load
/// whatever they depend on (such as the base <c>stormmods</c>) themselves, so they can be called in any order.
/// </para>
/// <para>
/// This class is not thread-safe for loading: the load and unload methods, and the map methods (which may load the depot cache),
/// must not be called at the same time as each other. The file methods (<see cref="FileExists(string?, string?)"/>, <see cref="GetFile(string, string?)"/>,
/// <see cref="TryGetFile(string?, string?, out Stream?)"/>, and their overloads) only read from the source, so they can be called from more than one thread.
/// </para>
/// <para>
/// For a CASC source, the caller opens the storage with <see cref="OpenCASCStorage(CASCConfig, HttpClient, ProgressReporter?)"/>, owns it, and disposes of it.
/// An instance only reads from it, so it must not be used after the storage is disposed.
/// </para>
/// </remarks>
public sealed class HeroesLoader
{
    /// <summary>
    /// The product name for Heroes of the Storm.
    /// </summary>
    public const string ProductName = "hero";

    /// <summary>
    /// The product name for Heroes of the Storm PTR.
    /// </summary>
    public const string ProductPtrName = "herot";

    /// <summary>
    /// The name of the <c>hdp.info</c> file in a local <c>mods</c> directory.
    /// </summary>
    public const string ModsHdpInfoFileName = "hdp.info";

    private static readonly JsonSerializerOptions _modsInfoFileJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly IHeroesSource _heroesSource;

    private bool _isDepotCacheLoaded;
    private bool _stormModsLoadFailed;

    private HeroesLoader(IHeroesSource heroesSource, ModsInfoFile? modsInfoFile)
    {
        _heroesSource = heroesSource;

        RootDirectory = heroesSource.ModsDirectoryPath;
        ModsInfoFile = modsInfoFile;
        HeroesVersion = modsInfoFile?.Version;
        IsPtr = modsInfoFile?.IsPtr ?? false;

        StormCatalog = new StormCatalog(heroesSource.SourceContainer.LayeredDataStorage);
    }

    private HeroesLoader(IHeroesSource heroesSource, CASCConfig cascConfig)
    {
        _heroesSource = heroesSource;

        RootDirectory = heroesSource.ModsDirectoryPath;
        HeroesVersion = cascConfig.VersionName;
        IsPtr = cascConfig.Product == ProductPtrName;

        StormCatalog = new StormCatalog(heroesSource.SourceContainer.LayeredDataStorage);
    }

    /// <summary>
    /// Gets the root directory, the <c>mods</c> directory.
    /// <para>
    /// For a file source, this is the full path of the directory that was given, without a trailing directory separator.
    /// For a CASC source, this is the <c>mods</c> directory relative to the root of the CASC storage.
    /// </para>
    /// </summary>
    public string RootDirectory { get; }

    /// <summary>
    /// Gets the version of the loaded Heroes of the Storm data, or <see langword="null"/> if it is unknown.
    /// <para>
    /// For a CASC source, this is from the <see cref="CASCConfig"/>. For a file source, this is from the <see cref="ModsInfoFile"/>.
    /// </para>
    /// </summary>
    public string? HeroesVersion { get; }

    /// <summary>
    /// Gets a value indicating whether the data is from the PTR.
    /// <para>
    /// For a CASC source, this is from the <see cref="CASCConfig"/>. For a file source, this is from the <see cref="ModsInfoFile"/>,
    /// and is <see langword="false"/> if there is none.
    /// </para>
    /// </summary>
    public bool IsPtr { get; }

    /// <summary>
    /// Gets the <c>hdp.info</c> file properties of a file source, or <see langword="null"/> if there is none or the source is CASC.
    /// </summary>
    public ModsInfoFile? ModsInfoFile { get; }

    /// <summary>
    /// Gets the <see cref="StormData.StormCatalog"/> for reading the loaded data.
    /// <para>
    /// It is the same instance for the lifetime of this loader and always reflects what is currently loaded,
    /// so it is empty (other than the default data) until the base <c>stormmods</c> are loaded, and it changes as a <c>mapmod</c> or gamestrings are loaded or unloaded.
    /// </para>
    /// </summary>
    public StormCatalog StormCatalog { get; }

    /// <summary>
    /// Gets a value indicating whether the base <c>stormmods</c> have been loaded.
    /// </summary>
    public bool IsStormModsLoaded { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a <c>mapmod</c> is currently loaded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(LoadedMapTitle))]
    public bool IsStormMapModLoaded => LoadedMapTitle is not null;

    /// <summary>
    /// Gets the title of the currently loaded <c>mapmod</c>, or <see langword="null"/> if no <c>mapmod</c> is loaded.
    /// </summary>
    public string? LoadedMapTitle { get; private set; }

    /// <summary>
    /// Gets the locale of the currently loaded gamestrings, or <see langword="null"/> if no gamestrings are loaded.
    /// <para>This is the same as <see cref="StormCatalog.GameStringsLocale"/>.</para>
    /// </summary>
    public StormLocale? GameStringsLocale => StormCatalog.GameStringsLocale;

    /// <summary>
    /// Helper method to get a <see cref="CASCConfig"/> from a local Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The path to the Heroes of the Storm directory.</param>
    /// <param name="loggerOptions">Logging options for the casclib.</param>
    /// <returns>A <see cref="CASCConfig"/> instance.</returns>
    public static CASCConfig GetCASCConfig(string pathToHeroesDirectory, ILoggerOptions? loggerOptions = null)
    {
        return CASCConfig.LoadLocalStorageConfig(pathToHeroesDirectory, ProductName, loggerOptions ?? new HeroesLoggerOptions());
    }

    /// <summary>
    /// Helper method to get a <see cref="CASCConfig"/> from Blizzard's online servers.
    /// </summary>
    /// <param name="httpClient">An instance of <see cref="HttpClient"/>.</param>
    /// <param name="isPtr">Set to <see langword="true"/> to download from ptr.</param>
    /// <param name="loggerOptions">Logging options for the casclib.</param>
    /// <returns>A <see cref="CASCConfig"/> instance.</returns>
    public static CASCConfig GetOnlineCASCConfig(HttpClient httpClient, bool isPtr = false, ILoggerOptions? loggerOptions = null)
    {
        return CASCConfig.LoadOnlineStorageConfig(isPtr ? ProductPtrName : ProductName, "us", httpClient, false, loggerOptions ?? new HeroesLoggerOptions());
    }

    /// <summary>
    /// Opens a CASC storage, locally or online, to pass to <see cref="LoadWithCASC(CASCHandler, HeroesLoaderOptions?)"/>.
    /// </summary>
    /// <param name="cascConfig">The <see cref="CASCConfig"/> to determine on how to load data from the data files.</param>
    /// <param name="httpClient">An instance of <see cref="HttpClient"/>. Needed for both local and online.</param>
    /// <param name="progressReporter">Used to report the progress of opening the CASC storage.</param>
    /// <returns>
    /// The opened CASC storage, with its file names loaded. The caller owns it: a local storage keeps its data files open until it is disposed,
    /// so dispose of it when done with the <see cref="HeroesLoader"/> instances that use it.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="cascConfig"/> or <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public static CASCHandler OpenCASCStorage(CASCConfig cascConfig, HttpClient httpClient, ProgressReporter? progressReporter = null)
    {
        ArgumentNullException.ThrowIfNull(cascConfig);
        ArgumentNullException.ThrowIfNull(httpClient);

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, httpClient: httpClient, worker: progressReporter);

        try
        {
            cascHandler.Root.LoadListFile(string.Empty, progressReporter);
        }
        catch
        {
            // the caller never gets the handler, so close it here
            cascHandler.Dispose();
            throw;
        }

        return cascHandler;
    }

    /// <summary>
    /// Gets the properties from the <c>hdp.info</c> file of a local <c>mods</c> directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory path, usually the <c>mods</c> directory.</param>
    /// <returns>The <see cref="StormData.ModsInfoFile"/> or <see langword="null"/> if the file was not found.</returns>
    /// <exception cref="ArgumentException"><paramref name="rootDirectory"/> is <see langword="null"/>, empty, or white space.</exception>
    /// <exception cref="JsonException">The file is not valid JSON.</exception>
    public static ModsInfoFile? GetModsInfoFile(string rootDirectory)
    {
        return GetModsInfoFile(new FileSystem(), rootDirectory);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesLoader"/> class. Sets the source of the data to be loaded from an extracted file source.
    /// </summary>
    /// <param name="rootDirectory">
    /// The root directory path, usually the <c>mods</c> directory. A relative path is made a full path from the current directory, so the loader
    /// does not depend on the current directory afterwards.
    /// </param>
    /// <param name="modsInfoFile">
    /// The properties of the extracted file source. If <see langword="null"/>, they are read from the <c>hdp.info</c> file in <paramref name="rootDirectory"/>, if it exists.
    /// </param>
    /// <param name="options">
    /// If not <see langword="null"/>, the data is loaded as described by <see cref="Load(HeroesLoaderOptions)"/>. Otherwise nothing is loaded yet.
    /// They are checked before <paramref name="rootDirectory"/> is used.
    /// </param>
    /// <returns>A <see cref="HeroesLoader"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="rootDirectory"/> is <see langword="null"/>, empty, or white space, or <see cref="HeroesLoaderOptions.MapTitle"/> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="HeroesLoaderOptions.GameStringsLocale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    /// <exception cref="DirectoryNotFoundException"><paramref name="rootDirectory"/> does not exist.</exception>
    /// <exception cref="KeyNotFoundException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    /// <exception cref="JsonException">The <c>hdp.info</c> file is not valid JSON.</exception>
    /// <remarks>
    /// On Linux and macOS, all the files and directories in <paramref name="rootDirectory"/> must have lowercase names, because their paths are looked up in lowercase.
    /// The letter case of the <paramref name="rootDirectory"/> path itself is kept as given.
    /// </remarks>
    public static HeroesLoader LoadWithFile(string rootDirectory, ModsInfoFile? modsInfoFile = null, HeroesLoaderOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        // checked before anything is opened
        if (options is not null)
            ThrowIfOptionsNotValid(options);

        // a full path, so the loader does not depend on the current directory, which can change later
        rootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));

        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"The root directory was not found: {rootDirectory}");

        modsInfoFile ??= GetModsInfoFile(rootDirectory);

        return Create(new HeroesLoader(new FileHeroesSource(rootDirectory, new StormModFactory()), modsInfoFile), options);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesLoader"/> class. Sets the source of data to be loaded from a CASC storage, locally or online.
    /// </summary>
    /// <param name="cascHandler">
    /// The opened CASC storage, from <see cref="OpenCASCStorage(CASCConfig, HttpClient, ProgressReporter?)"/>.
    /// The caller owns it: dispose of it when done with the returned <see cref="HeroesLoader"/>, not before.
    /// </param>
    /// <param name="options">
    /// If not <see langword="null"/>, the data is loaded as described by <see cref="Load(HeroesLoaderOptions)"/>. Otherwise nothing is loaded yet.
    /// They are checked before anything is read from the storage, except whether <see cref="HeroesLoaderOptions.MapTitle"/> exists, which needs the storage.
    /// </param>
    /// <returns>A <see cref="HeroesLoader"/> instance that reads from <paramref name="cascHandler"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cascHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><see cref="HeroesLoaderOptions.MapTitle"/> is empty or white space.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="HeroesLoaderOptions.GameStringsLocale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    /// <exception cref="KeyNotFoundException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    /// <remarks>
    /// The returned instance does not dispose <paramref name="cascHandler"/>, including when this throws.
    /// After <paramref name="cascHandler"/> is disposed, the methods that read from a local storage throw an <see cref="ObjectDisposedException"/>.
    /// <code>
    /// using CASCHandler cascHandler = HeroesLoader.OpenCASCStorage(cascConfig, httpClient);
    /// HeroesLoader heroesLoader = HeroesLoader.LoadWithCASC(cascHandler);
    /// </code>
    /// </remarks>
    public static HeroesLoader LoadWithCASC(CASCHandler cascHandler, HeroesLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(cascHandler);

        // checked before the storage tree is built, which can take a while
        if (options is not null)
            ThrowIfOptionsNotValid(options);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        CASCHeroesSource cascHeroesSource = new(SourceConstants.DefaultModsDirectory, new StormModFactory(), new CASCHeroesStorage(cascHandler, cascFolderRoot));

        return Create(new HeroesLoader(cascHeroesSource, cascHandler.Config), options);
    }

    /// <summary>
    /// <para>Loads the base <c>stormmods</c>, then the <c>mapmod</c> and gamestrings described by <paramref name="options"/>, in that order.</para>
    /// <para>
    /// Compared to calling <see cref="LoadMapMod(string)"/> and <see cref="LoadGameStrings(StormLocale)"/> separately,
    /// the gamestrings are loaded at most once, even when both the map and the locale change.
    /// </para>
    /// <para>
    /// The options are checked before anything is loaded, so a map that is not found does not load the base <c>stormmods</c>.
    /// </para>
    /// </summary>
    /// <param name="options">What should be loaded.</param>
    /// <remarks>
    /// If loading the map's files throws, no map is loaded afterwards (the previous one was already unloaded).
    /// If the map loads but loading the gamestrings throws, the map stays loaded and no gamestrings are loaded (<see cref="GameStringsLocale"/> is <see langword="null"/>).
    /// </remarks>
    /// <exception cref="ArgumentException"><see cref="HeroesLoaderOptions.MapTitle"/> is empty or white space.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="HeroesLoaderOptions.GameStringsLocale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    /// <exception cref="KeyNotFoundException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    public void Load(HeroesLoaderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ThrowIfOptionsNotValid(options);

        // checked first, so nothing is loaded for a map that can't be. It only needs the depot cache.
        if (options.MapTitle is not null && !IsLoadedMapTitle(options.MapTitle) && !StormMapExists(options.MapTitle))
            throw new KeyNotFoundException($"Could not find the map: {options.MapTitle}");

        LoadStormMods();

        bool mapChanged = false;

        if (options.MapTitle is not null && !TryLoadMapModCore(options.MapTitle, out mapChanged))
            throw new KeyNotFoundException($"Could not find the map: {options.MapTitle}");

        StormLocale? locale = options.GameStringsLocale ?? GameStringsLocale;

        if (locale is not null && (mapChanged || locale != GameStringsLocale))
            LoadGameStringsCore(locale.Value);
    }

    /// <summary>
    /// Loads the base <c>stormmods</c>. Does nothing if they are already loaded.
    /// <para>
    /// The depot cache (the map files) is not loaded. It is loaded the first time a map is needed, such as by <see cref="LoadMapMod(string)"/> or <see cref="GetMapTitles"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The base <c>stormmods</c> cannot be loaded again after a failed attempt (it would add their data a second time),
    /// so if this throws, this and the other methods that load the base <c>stormmods</c> throw an <see cref="InvalidOperationException"/> afterwards.
    /// Create a new <see cref="HeroesLoader"/> instead. The methods that only use the depot cache, such as <see cref="GetMapTitles"/>, still work.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A previous attempt to load the base <c>stormmods</c> failed.</exception>
    public void LoadStormMods()
    {
        if (IsStormModsLoaded)
            return;

        if (_stormModsLoadFailed)
            throw new InvalidOperationException($"A previous attempt to load the base stormmods failed. Create a new {nameof(HeroesLoader)} instance.");

        try
        {
            _heroesSource.LoadStormMods();
        }
        catch
        {
            _stormModsLoadFailed = true;
            throw;
        }

        IsStormModsLoaded = true;
    }

    /// <summary>
    /// Loads a <c>mapmod</c>, replacing the currently loaded one.
    /// <para>
    /// Only one can be loaded at a time.
    /// Loads the base <c>stormmods</c> if they are not already loaded.
    /// If gamestrings are loaded, they are reloaded to include the map's gamestrings.
    /// </para>
    /// </summary>
    /// <param name="mapTitle">
    /// <para>A map's title. Can be found from <see cref="GetMapTitles"/>. Is case-insensitive.</para>
    /// <para>Only <c>enUS</c> map titles are accepted.</para>
    /// </param>
    /// <remarks>
    /// The title is looked up before the base <c>stormmods</c> are loaded, so a map that is not found does not load them.
    /// If loading the map's files throws, no map is loaded afterwards (the previous one was already unloaded).
    /// If the map loads but reloading the gamestrings throws, the map stays loaded and no gamestrings are loaded (<see cref="GameStringsLocale"/> is <see langword="null"/>).
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="mapTitle"/> is <see langword="null"/>, empty, or white space.</exception>
    /// <exception cref="KeyNotFoundException">The map was not found. The current <c>mapmod</c> stays loaded.</exception>
    public void LoadMapMod(string mapTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapTitle);

        if (!TryLoadMapMod(mapTitle))
            throw new KeyNotFoundException($"Could not find the map: {mapTitle}");
    }

    /// <summary>
    /// Loads a <c>mapmod</c>, replacing the currently loaded one.
    /// <para>
    /// Only one can be loaded at a time.
    /// Loads the base <c>stormmods</c> if they are not already loaded.
    /// If gamestrings are loaded, they are reloaded to include the map's gamestrings.
    /// </para>
    /// </summary>
    /// <param name="mapTitle">
    /// <para>A map's title. Can be found from <see cref="GetMapTitles"/>. Is case-insensitive.</para>
    /// <para>Only <c>enUS</c> map titles are accepted.</para>
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the map is loaded; <see langword="false"/> if the map was not found, in which case the current <c>mapmod</c> stays loaded
    /// and the base <c>stormmods</c> are not loaded.
    /// Returns <see langword="false"/> without loading anything if <paramref name="mapTitle"/> is <see langword="null"/>, empty, or white space.
    /// </returns>
    /// <remarks>
    /// Only a map that is not found returns <see langword="false"/>. Other failures still throw.
    /// If loading the map's files throws, no map is loaded afterwards (the previous one was already unloaded).
    /// If the map loads but reloading the gamestrings throws, the map stays loaded and no gamestrings are loaded (<see cref="GameStringsLocale"/> is <see langword="null"/>).
    /// </remarks>
    public bool TryLoadMapMod(string? mapTitle)
    {
        if (string.IsNullOrWhiteSpace(mapTitle))
            return false;

        // checked first, so an unknown title does not load the base stormmods. It only needs the depot cache.
        if (!IsLoadedMapTitle(mapTitle) && !StormMapExists(mapTitle))
            return false;

        LoadStormMods();

        if (!TryLoadMapModCore(mapTitle, out bool mapChanged))
            return false;

        if (mapChanged && GameStringsLocale is StormLocale locale)
            LoadGameStringsCore(locale);

        return true;
    }

    /// <summary>
    /// Unloads the current <c>mapmod</c>, removing all of its data (the xml, file, style, <c>assets.txt</c>, and gamestring data).
    /// <para>
    /// The base <c>stormmods</c> data stays loaded, including any values the map had overridden.
    /// The base gamestrings and <see cref="GameStringsLocale"/> are not changed, and nothing is reloaded.
    /// </para>
    /// </summary>
    /// <returns><see langword="true"/> if a <c>mapmod</c> was unloaded; <see langword="false"/> if no <c>mapmod</c> was loaded.</returns>
    public bool UnloadMapMod()
    {
        if (LoadedMapTitle is null)
            return false;

        _heroesSource.UnloadStormMapMod();
        LoadedMapTitle = null;

        return true;
    }

    /// <summary>
    /// Loads the gamestrings of a locale from the base and map <c>stormmods</c>, replacing the currently loaded gamestrings.
    /// <para>
    /// Only one locale can be loaded at a time. Does nothing if the locale is already loaded.
    /// Loads the base <c>stormmods</c> if they are not already loaded.
    /// </para>
    /// </summary>
    /// <param name="locale">The locale of the gamestrings to load.</param>
    /// <remarks>
    /// A <c>stormmod</c> that does not have a <c>gamestrings.txt</c> file for the locale is skipped.
    /// If this throws while loading, all the gamestrings are removed and <see cref="GameStringsLocale"/> is <see langword="null"/>, so it can be called again.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="locale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    public void LoadGameStrings(StormLocale locale = StormLocale.ENUS)
    {
        ThrowIfLocaleNotDefined(locale, nameof(locale));

        LoadStormMods();

        if (GameStringsLocale == locale)
            return;

        LoadGameStringsCore(locale);
    }

    /// <summary>
    /// Gets the map titles (<c>enUS</c>) to be used for <see cref="LoadMapMod(string)"/>, <see cref="TryLoadMapMod(string?)"/>, and <see cref="GetStormMap(string)"/>.
    /// <para>
    /// Loads the depot cache (the map files) if it is not already loaded. The base <c>stormmods</c> are not loaded.
    /// </para>
    /// </summary>
    /// <returns>
    /// A new list of the map titles, sorted by <see cref="StringComparer.OrdinalIgnoreCase"/>, so the order is the same on every platform.
    /// To check for a single title, use <see cref="StormMapExists(string?)"/>, which is case-insensitive.
    /// </returns>
    /// <remarks>
    /// If more than one map has the same title, only one of them is listed, the one with the first <c>.s2ma</c> file path (ordinal, case-insensitive).
    /// Maps with an empty title are not listed.
    /// </remarks>
    public IReadOnlyList<string> GetMapTitles()
    {
        EnsureDepotCacheLoaded();

        return _heroesSource.GetMapTitles();
    }

    /// <summary>
    /// Determines if a map exists.
    /// <para>
    /// Loads the depot cache (the map files) if it is not already loaded. The base <c>stormmods</c> are not loaded.
    /// </para>
    /// </summary>
    /// <param name="mapTitle">A map's title (<c>enUS</c>). Can be found from <see cref="GetMapTitles"/>. Is case-insensitive.</param>
    /// <returns>
    /// <see langword="true"/> if the map exists; otherwise <see langword="false"/>.
    /// Returns <see langword="false"/> if <paramref name="mapTitle"/> is <see langword="null"/>, empty, or white space.
    /// </returns>
    public bool StormMapExists(string? mapTitle) => TryGetMapProperties(mapTitle, out _, out _);

    /// <summary>
    /// Gets a map's data from the <c>.s2ma</c> and <c>.s2mv</c> files. Does not contain data from the xml or gamestring files.
    /// <para>
    /// Loads the depot cache (the map files) if it is not already loaded. The base <c>stormmods</c> are not loaded.
    /// </para>
    /// </summary>
    /// <param name="mapTitle">A map's title (<c>enUS</c>). Can be found from <see cref="GetMapTitles"/>. Is case-insensitive.</param>
    /// <returns>Map data for the specified map.</returns>
    /// <exception cref="ArgumentException"><paramref name="mapTitle"/> is <see langword="null"/>, empty, or white space.</exception>
    /// <exception cref="KeyNotFoundException">The map was not found.</exception>
    public StormMap GetStormMap(string mapTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapTitle);

        if (TryGetStormMap(mapTitle, out StormMap? stormMap))
            return stormMap;

        throw new KeyNotFoundException($"Could not find the map: {mapTitle}");
    }

    /// <summary>
    /// Gets a map's data from the <c>.s2ma</c> and <c>.s2mv</c> files. Does not contain data from the xml or gamestring files.
    /// <para>
    /// Loads the depot cache (the map files) if it is not already loaded. The base <c>stormmods</c> are not loaded.
    /// </para>
    /// </summary>
    /// <param name="mapTitle">A map's title (<c>enUS</c>). Can be found from <see cref="GetMapTitles"/>. Is case-insensitive.</param>
    /// <param name="stormMap">When this method returns, contains the map data, if the map was found; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the map was found; otherwise <see langword="false"/>.
    /// Returns <see langword="false"/> if <paramref name="mapTitle"/> is <see langword="null"/>, empty, or white space.
    /// </returns>
    public bool TryGetStormMap(string? mapTitle, [NotNullWhen(true)] out StormMap? stormMap)
    {
        if (!TryGetMapProperties(mapTitle, out S2MAProperties? s2maProperties, out S2MVProperties? s2mvProperties))
        {
            stormMap = null;
            return false;
        }

        stormMap = new StormMap()
        {
            Name = s2mvProperties.HeaderTitle,
            NameByLocale = s2mvProperties.NameByStormLocale.AsReadOnly(),
            MapId = s2maProperties.MapId ?? string.Empty,
            MapLink = s2mvProperties.MapLink,
            MapSize = (s2mvProperties.MapSize.Width, s2mvProperties.MapSize.Height),
            ReplayPreviewImagePath = s2mvProperties.PreviewLargeImage,
            LoadingScreenImagePath = s2mvProperties.LoadingImage,
            LayoutFilePath = s2mvProperties.CustomLayout,
            LayoutLoadingScreenFrame = s2mvProperties.CustomFrame,
            S2MAFilePath = s2maProperties.DirectoryPath,
            S2MVFilePath = s2mvProperties.DirectoryPath,
        };

        return true;
    }

    /// <summary>
    /// Gets the <c>stormmods</c> of the currently loaded <c>mapmod</c>, in the order they were loaded.
    /// The map's dependencies come first and the map's own <c>stormmod</c> is last, with <see cref="StormMapDependency.IsMap"/> set.
    /// <para>
    /// A <c>stormmod</c> that more than one dependency depends on is listed once, where it was loaded.
    /// The <c>heroesdata.stormmod</c> is not listed, even if the map depends on it, because it is one of the base <c>stormmods</c>.
    /// The <c>stormmods</c> included through an <c>includes.xml</c> file are not listed, but they are loaded and unloaded with the map.
    /// </para>
    /// </summary>
    /// <returns>A new list of the loaded <c>mapmod</c>'s <c>stormmods</c>, or an empty list if no <c>mapmod</c> is loaded.</returns>
    public IReadOnlyList<StormMapDependency> GetLoadedStormMapDependencies()
    {
        IReadOnlyList<IStormMod> stormMapMods = _heroesSource.StormMapMods;

        // the map root is always last
        return stormMapMods
            .Select((x, index) => new StormMapDependency()
            {
                Name = x.Name,
                RelativePath = x.RelativeStormModPath,
                IsMap = index == stormMapMods.Count - 1,
            })
            .ToList();
    }

    /// <summary>
    /// Gets the <see cref="StormModContainer"/>s of all the loaded <c>stormmods</c>, in their data layer order:
    /// the base <c>stormmods</c> first, then the <c>mapmod</c>'s, then any custom ones. Within a layer, they are in the order they were loaded.
    /// <para>
    /// This includes the <c>stormmods</c> loaded through an <c>includes.xml</c> file.
    /// </para>
    /// </summary>
    /// <returns>
    /// A new list of the loaded <see cref="StormModContainer"/>s, or an empty list if nothing is loaded.
    /// Loading or unloading afterwards does not change the returned list.
    /// </returns>
    public IReadOnlyList<StormModContainer> GetLoadedStormModContainers() => [.. _heroesSource.SourceContainer.StormModContainers];

    /// <summary>
    /// Determines if a file, or an entry in an MPQ archive file, exists.
    /// </summary>
    /// <param name="path">
    /// <para>The file path.</para>
    /// <para>
    /// It should be relative to the <c>mods</c> directory (it may start with <c>mods</c>) or start with the <see cref="RootDirectory"/>.
    /// For a file source, it may also be an absolute path.
    /// </para>
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/> (e.g. a <c>.s2ma</c> file), or <see langword="null"/> if <paramref name="path"/> is the file itself.</param>
    /// <returns>
    /// <see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="path"/> being <see langword="null"/> or whitespace,
    /// <paramref name="path"/> being a directory, and a file at <paramref name="path"/> that is not an MPQ archive when <paramref name="mpqEntryPath"/> is given.
    /// </returns>
    public bool FileExists(string? path, string? mpqEntryPath = null) => _heroesSource.FileExists(path, mpqEntryPath);

    /// <summary>
    /// Determines if a file exists. If the file is inside an MPQ archive, the entry in the archive is checked.
    /// </summary>
    /// <param name="stormFile">The file to check.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="stormFile"/> being <see langword="null"/>.</returns>
    public bool FileExists(StormFile? stormFile)
    {
        if (stormFile is null)
            return false;

        (string path, string? mpqEntryPath) = GetSourcePaths(stormFile);

        return FileExists(path, mpqEntryPath);
    }

    /// <summary>
    /// Opens a file, or an entry in an MPQ archive file, for reading.
    /// </summary>
    /// <param name="path">
    /// <para>The file path.</para>
    /// <para>
    /// It should be relative to the <c>mods</c> directory (it may start with <c>mods</c>) or start with the <see cref="RootDirectory"/>.
    /// For a file source, it may also be an absolute path.
    /// </para>
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/> (e.g. a <c>.s2ma</c> file), or <see langword="null"/> if <paramref name="path"/> is the file itself.</param>
    /// <returns>The <see cref="Stream"/> of the file. The caller is responsible for disposing it.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="FileNotFoundException">
    /// The file was not found (in the same cases that <see cref="FileExists(string?, string?)"/> returns <see langword="false"/>).
    /// </exception>
    /// <exception cref="MpqHeroesToolException">The entry in the MPQ archive file exists, but could not be decompressed.</exception>
    public Stream GetFile(string path, string? mpqEntryPath = null) => _heroesSource.GetFile(path, mpqEntryPath);

    /// <summary>
    /// Opens a file for reading. If the file is inside an MPQ archive, the entry in the archive is opened.
    /// </summary>
    /// <param name="stormFile">The file to open.</param>
    /// <returns>The <see cref="Stream"/> of the file. The caller is responsible for disposing it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stormFile"/> is <see langword="null"/>.</exception>
    /// <exception cref="FileNotFoundException">The file, or the entry in the MPQ archive file, was not found.</exception>
    /// <exception cref="MpqHeroesToolException">The entry in the MPQ archive file exists, but could not be decompressed.</exception>
    public Stream GetFile(StormFile stormFile)
    {
        ArgumentNullException.ThrowIfNull(stormFile);

        (string path, string? mpqEntryPath) = GetSourcePaths(stormFile);

        return GetFile(path, mpqEntryPath);
    }

    /// <summary>
    /// Opens a file for reading, if it exists.
    /// </summary>
    /// <param name="path">
    /// <para>The file path.</para>
    /// <para>
    /// It should be relative to the <c>mods</c> directory (it may start with <c>mods</c>) or start with the <see cref="RootDirectory"/>.
    /// For a file source, it may also be an absolute path.
    /// </para>
    /// </param>
    /// <param name="stream">
    /// When this method returns, contains the <see cref="Stream"/> of the file, if it exists; otherwise, <see langword="null"/>.
    /// The caller is responsible for disposing it.
    /// </param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>.</returns>
    public bool TryGetFile(string? path, [NotNullWhen(true)] out Stream? stream) => TryGetFile(path, null, out stream);

    /// <summary>
    /// Opens a file, or an entry in an MPQ archive file, for reading, if it exists.
    /// </summary>
    /// <param name="path">
    /// <para>The file path.</para>
    /// <para>
    /// It should be relative to the <c>mods</c> directory (it may start with <c>mods</c>) or start with the <see cref="RootDirectory"/>.
    /// For a file source, it may also be an absolute path.
    /// </para>
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/> (e.g. a <c>.s2ma</c> file), or <see langword="null"/> if <paramref name="path"/> is the file itself.</param>
    /// <param name="stream">
    /// When this method returns, contains the <see cref="Stream"/> of the file, if it exists; otherwise, <see langword="null"/>.
    /// The caller is responsible for disposing it.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the file exists; otherwise <see langword="false"/>, in the same cases as <see cref="FileExists(string?, string?)"/>.
    /// </returns>
    /// <exception cref="MpqHeroesToolException">The entry in the MPQ archive file exists, but could not be decompressed.</exception>
    public bool TryGetFile(string? path, string? mpqEntryPath, [NotNullWhen(true)] out Stream? stream) => _heroesSource.TryGetFile(path, mpqEntryPath, out stream);

    /// <summary>
    /// Opens a file for reading, if it exists. If the file is inside an MPQ archive, the entry in the archive is opened.
    /// </summary>
    /// <param name="stormFile">The file to open.</param>
    /// <param name="stream">
    /// When this method returns, contains the <see cref="Stream"/> of the file, if it exists; otherwise, <see langword="null"/>.
    /// The caller is responsible for disposing it.
    /// </param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="stormFile"/> being <see langword="null"/>.</returns>
    /// <exception cref="MpqHeroesToolException">The entry in the MPQ archive file exists, but could not be decompressed.</exception>
    public bool TryGetFile(StormFile? stormFile, [NotNullWhen(true)] out Stream? stream)
    {
        if (stormFile is null)
        {
            stream = null;
            return false;
        }

        (string path, string? mpqEntryPath) = GetSourcePaths(stormFile);

        return TryGetFile(path, mpqEntryPath, out stream);
    }

    internal static HeroesLoader LoadWithInternal(IHeroesSource heroesSource, HeroesLoaderOptions? options = null, ModsInfoFile? modsInfoFile = null)
    {
        return Create(new HeroesLoader(heroesSource, modsInfoFile), options);
    }

    internal static ModsInfoFile? GetModsInfoFile(IFileSystem fileSystem, string rootDirectory)
    {
        // an empty path would read the file from the current directory
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        string infoFilePath = fileSystem.Path.Join(rootDirectory, ModsHdpInfoFileName);

        if (!fileSystem.File.Exists(infoFilePath))
            return null;

        using FileSystemStream fileStream = fileSystem.File.OpenRead(infoFilePath);

        return JsonSerializer.Deserialize<ModsInfoFile>(fileStream, _modsInfoFileJsonSerializerOptions);
    }

    private static HeroesLoader Create(HeroesLoader heroesLoader, HeroesLoaderOptions? options)
    {
        if (options is not null)
            heroesLoader.Load(options);

        return heroesLoader;
    }

    /// <summary>
    /// Gets the paths to pass to the source for a <see cref="StormFile"/>: the archive and the entry inside it, or the file itself.
    /// </summary>
    /// <param name="stormFile">The file.</param>
    /// <returns>The path of the file, and the path of the entry if the file is inside an MPQ archive.</returns>
    private static (string Path, string? MpqEntryPath) GetSourcePaths(StormFile stormFile) => stormFile.IsInMpqArchive
        ? (stormFile.MpqFilePath, stormFile.MpqEntryPath)
        : (stormFile.FullPath, null);

    private static void ThrowIfLocaleNotDefined(StormLocale locale, string paramName)
    {
        if (!Enum.IsDefined(locale))
            throw new ArgumentOutOfRangeException(paramName, locale, $"The value is not a defined {nameof(StormLocale)}.");
    }

    // the checks that don't need anything loaded, so they can be done before a source is opened
    private static void ThrowIfOptionsNotValid(HeroesLoaderOptions options)
    {
        if (options.GameStringsLocale is StormLocale locale)
            ThrowIfLocaleNotDefined(locale, nameof(options));

        if (options.MapTitle is not null && string.IsNullOrWhiteSpace(options.MapTitle))
            throw new ArgumentException($"{nameof(HeroesLoaderOptions.MapTitle)} is empty or white space. Use null to keep the current map.", nameof(options));
    }

    private bool TryLoadMapModCore(string mapTitle, out bool mapChanged)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapTitle);

        mapChanged = false;

        if (IsLoadedMapTitle(mapTitle))
            return true;

        // loads the depot cache if needed, and finds the map the same way as the other map methods
        if (!TryGetMapProperties(mapTitle, out _, out S2MVProperties? s2mvProperties))
            return false;

        // use the map's own title, so LoadedMapTitle does not depend on the casing that was passed in
        string headerTitle = s2mvProperties.HeaderTitle;

        try
        {
            if (!_heroesSource.LoadStormMapMod(headerTitle))
                return false;
        }
        catch
        {
            // the previous map was already unloaded and the new one may be partially loaded, so remove what is there
            _heroesSource.UnloadStormMapMod();
            LoadedMapTitle = null;

            throw;
        }

        LoadedMapTitle = headerTitle;
        mapChanged = true;

        return true;
    }

    // the loaded map is not looked up again, so asking for it does nothing
    private bool IsLoadedMapTitle(string mapTitle) => string.Equals(LoadedMapTitle, mapTitle, StringComparison.OrdinalIgnoreCase);

    private void EnsureDepotCacheLoaded()
    {
        if (_isDepotCacheLoaded)
            return;

        _heroesSource.LoadDepotCache();
        _isDepotCacheLoaded = true;
    }

    private bool TryGetMapProperties(string? mapTitle, [NotNullWhen(true)] out S2MAProperties? s2maProperties, [NotNullWhen(true)] out S2MVProperties? s2mvProperties)
    {
        s2maProperties = null;
        s2mvProperties = null;

        if (string.IsNullOrWhiteSpace(mapTitle))
            return false;

        EnsureDepotCacheLoaded();

        // a map is only found by its title when it has s2mv properties, the check is kept so all the map methods agree
        if (!_heroesSource.TryGetS2MAProperties(mapTitle, out s2maProperties) || s2maProperties.S2MVProperties is null)
        {
            s2maProperties = null;
            return false;
        }

        s2mvProperties = s2maProperties.S2MVProperties;
        return true;
    }

    private void LoadGameStringsCore(StormLocale locale)
    {
        // cleared first, so a failed load is not seen as loaded, it is safe to call again as it clears all gamestrings before loading
        StormCatalog.GameStringsLocale = null;

        _heroesSource.LoadGameStrings(locale);
        StormCatalog.GameStringsLocale = locale;
    }
}
