namespace Heroes.StormData;

/// <summary>
/// Class to load the heroes data from a given source.
/// </summary>
/// <remarks>
/// The load methods only do work when the requested state differs from the current state, and they load
/// whatever they depend on (such as the base <c>stormmods</c>) themselves, so they can be called in any order.
/// </remarks>
public class HeroesLoader
{
    /// <summary>
    /// The product name for Heroes of the Storm.
    /// </summary>
    public const string ProductName = "hero";

    /// <summary>
    /// The product name for Heroes of the Storm PTR.
    /// </summary>
    public const string ProductPtrName = "herot";

    private readonly IHeroesSource _heroesSource;

    private HeroesLoader(IHeroesSource heroesSource)
    {
        _heroesSource = heroesSource;
    }

    /// <summary>
    /// Gets a value indicating whether the base <c>stormmods</c> have been loaded.
    /// </summary>
    public bool IsStormModsLoaded { get; private set; }

    /// <summary>
    /// Gets the title of the currently loaded <c>mapmod</c>, or <see langword="null"/> if no <c>mapmod</c> is loaded.
    /// </summary>
    public string? LoadedMapTitle { get; private set; }

    /// <summary>
    /// Gets the locale of the currently loaded gamestrings, or <see langword="null"/> if no gamestrings are loaded.
    /// </summary>
    public StormLocale? GameStringsLocale { get; private set; }

    /// <summary>
    /// Helper method to get a <see cref="CASCConfig"/> from a local Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The path to the Heroes of the storm directory.</param>
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
    /// Gets an instance of the <see cref="HeroesLoader"/> class. Sets the source of the data to be loaded from an extracted file source.
    /// </summary>
    /// <param name="rootDirectory">The root directory path, usually the <c>mods</c> directory.</param>
    /// <param name="options">If not <see langword="null"/>, the data is loaded as described by <see cref="Load(HeroesLoaderOptions)"/>. Otherwise nothing is loaded yet.</param>
    /// <returns>A <see cref="HeroesLoader"/> instance.</returns>
    /// <exception cref="HeroesStormDataException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    public static HeroesLoader LoadWithFile(string rootDirectory, HeroesLoaderOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        return Create(new FileHeroesSource(rootDirectory, new StormModFactory()), options);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesLoader"/> class. Sets the source of data to be loaded locally or online.
    /// </summary>
    /// <param name="cascConfig">The <see cref="CASCConfig"/> to determine on how to load data from the data files.</param>
    /// <param name="httpClient">An instance of <see cref="HttpClient"/>. Needed for both local and online.</param>
    /// <param name="progressReporter">Used to report the progress of opening the CASC storage.</param>
    /// <param name="options">If not <see langword="null"/>, the data is loaded as described by <see cref="Load(HeroesLoaderOptions)"/>. Otherwise nothing is loaded yet.</param>
    /// <returns>A <see cref="HeroesLoader"/> instance.</returns>
    /// <exception cref="HeroesStormDataException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    public static HeroesLoader LoadWithCASC(CASCConfig cascConfig, HttpClient httpClient, IProgressReporter? progressReporter = null, HeroesLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(cascConfig);
        ArgumentNullException.ThrowIfNull(httpClient);

        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, httpClient: httpClient, worker: (ProgressReporter?)progressReporter);
        cascHandler.Root.LoadListFile(string.Empty, (ProgressReporter?)progressReporter);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        CASCHeroesSource cascHeroesSource = new(SourceConstants.DefaultModsDirectory, new StormModFactory(), new CASCHeroesStorage(cascHandler, cascFolderRoot));

        return Create(cascHeroesSource, options);
    }

    /// <summary>
    /// <para>Loads the base <c>stormmods</c>, then the <c>mapmod</c> and gamestrings described by <paramref name="options"/>, in that order.</para>
    /// <para>
    /// Compared to calling <see cref="LoadMapMod(string)"/> and <see cref="LoadGameStrings(StormLocale)"/> separately,
    /// the gamestrings are loaded at most once, even when both the map and the locale change.
    /// </para>
    /// </summary>
    /// <param name="options">What should be loaded.</param>
    /// <exception cref="HeroesStormDataException"><see cref="HeroesLoaderOptions.MapTitle"/> was not found.</exception>
    public void Load(HeroesLoaderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        LoadStormMods();

        bool mapChanged = false;

        if (options.MapTitle is not null && !TryLoadMapModCore(options.MapTitle, out mapChanged))
            throw new HeroesStormDataException($"Map title '{options.MapTitle}' was not found.");

        StormLocale? locale = options.GameStringsLocale ?? GameStringsLocale;

        if (locale is not null && (mapChanged || locale != GameStringsLocale))
            LoadGameStringsCore(locale.Value);
    }

    /// <summary>
    /// Loads the base <c>stormmods</c> and the depot cache. Does nothing if they are already loaded.
    /// </summary>
    public void LoadStormMods()
    {
        if (IsStormModsLoaded)
            return;

        _heroesSource.LoadStormMods();
        _heroesSource.LoadDepotCache();

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
    /// <para>Only <c>enUS</c> maps titles are accepted.</para>
    /// </param>
    /// <returns><see langword="true"/> if the map is loaded; <see langword="false"/> if the map was not found, in which case the current <c>mapmod</c> stays loaded.</returns>
    public bool LoadMapMod(string mapTitle)
    {
        LoadStormMods();

        if (!TryLoadMapModCore(mapTitle, out bool mapChanged))
            return false;

        if (mapChanged && GameStringsLocale is StormLocale locale)
            LoadGameStringsCore(locale);

        return true;
    }

    /// <summary>
    /// Unloads the current <c>mapmod</c>, including its gamestrings.
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
    /// <param name="localization">The <see cref="StormLocale"/>.</param>
    public void LoadGameStrings(StormLocale localization = StormLocale.ENUS)
    {
        LoadStormMods();

        if (GameStringsLocale == localization)
            return;

        LoadGameStringsCore(localization);
    }

    /// <summary>
    /// Gets a collection of the map titles (<c>enUS</c>) to be used for <see cref="LoadMapMod(string)"/>.
    /// <para>
    /// This is empty until the base <c>stormmods</c> are loaded.
    /// </para>
    /// </summary>
    /// <returns>A collection of map titles.</returns>
    public IEnumerable<string> GetMapTitles()
    {
        return _heroesSource.GetMapTitles();
    }

    internal static HeroesLoader LoadWithInternal(IHeroesSource heroesSource, HeroesLoaderOptions? options = null)
    {
        return Create(heroesSource, options);
    }

    private static HeroesLoader Create(IHeroesSource heroesSource, HeroesLoaderOptions? options)
    {
        HeroesLoader heroesLoader = new(heroesSource);

        if (options is not null)
            heroesLoader.Load(options);

        return heroesLoader;
    }

    private bool TryLoadMapModCore(string mapTitle, out bool mapChanged)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mapTitle);

        mapChanged = false;

        if (string.Equals(LoadedMapTitle, mapTitle, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!_heroesSource.LoadStormMapMod(mapTitle))
            return false;

        LoadedMapTitle = mapTitle;
        mapChanged = true;

        return true;
    }

    private void LoadGameStringsCore(StormLocale localization)
    {
        _heroesSource.LoadGamestrings(localization);
        GameStringsLocale = localization;
    }
}
