namespace Heroes.XmlData;

/// <summary>
/// Class to load the heroes data from a given source.
/// </summary>
public class HeroesXmlLoader
{
    /// <summary>
    /// The product name for Heroes of the Storm.
    /// </summary>
    public const string ProductName = "hero";

    /// <summary>
    /// The product name for Heroes of the Storm PTR.
    /// </summary>
    public const string ProductPtrName = "herot";

    private static readonly JsonSerializerOptions _modsInfoFileJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly IHeroesSource _heroesSource;

    private bool _baseStormModsLoaded = false;
    private string _loadedMapTitle = string.Empty;

    // for testing
    private HeroesXmlLoader(IHeroesSource heroesSource)
    {
        StormStorage stormStorage = new();
        _heroesSource = heroesSource;

        HeroesData = new HeroesData(stormStorage);

        LoadedType = HeroesXmlLoaderType.File;
        RootDirectory = _heroesSource.ModsBaseDirectoryPath;
    }

    private HeroesXmlLoader()
        : this(string.Empty, modsInfoFile: null, progressReporter: null)
    {
    }

    private HeroesXmlLoader(string pathToModsDirectory, ModsInfoFile? modsInfoFile, IProgressReporter? progressReporter)
    {
        StormStorage stormStorage = new();
        FileHeroesSource fileHeroesSource = new(stormStorage, new StormModFactory(), new DepotCacheFactory(), pathToModsDirectory, progressReporter);

        modsInfoFile ??= GetModsInfoFile(pathToModsDirectory);

        if (modsInfoFile is not null)
        {
            HeroesVersion = modsInfoFile.Version;
            IsPtr = modsInfoFile.IsPtr;
        }

        _heroesSource = fileHeroesSource;

        HeroesData = new HeroesData(stormStorage);

        LoadedType = HeroesXmlLoaderType.File;
        RootDirectory = _heroesSource.ModsBaseDirectoryPath;
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage, CASCConfig cascConfig, IProgressReporter? progressReporter)
    {
        StormStorage stormStorage = new();
        _heroesSource = new CASCHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), cascHeroesStorage, progressReporter);

        HeroesData = new HeroesData(stormStorage);

        LoadedType = HeroesXmlLoaderType.CASC;
        RootDirectory = _heroesSource.ModsBaseDirectoryPath;
        HeroesVersion = cascConfig.VersionName;
        IsPtr = cascConfig.Product == ProductPtrName;
    }

    /// <summary>
    /// <para>Gets the loaded internal heroes build number from the buildid.txt file.</para>
    /// <para>This is sometimes not updated to the (latest) correct build.</para>
    /// </summary>
    public int? BuildId => _heroesSource.StormStorage.GetBuildId();

    /// <summary>
    /// Gets the version of the loaded Heroes of the Storm data.
    /// </summary>
    public string? HeroesVersion { get; }

    /// <summary>
    /// Gets a value indicating whether the data is from the PTR.
    /// </summary>
    public bool IsPtr { get; }

    /// <summary>
    /// Gets the type of the loaded source data.
    /// </summary>
    public HeroesXmlLoaderType LoadedType { get; }

    /// <summary>
    /// Gets the root directory.
    /// </summary>
    public string RootDirectory { get; }

    /// <summary>
    /// Gets the <see cref="HeroesData"/> which contain the xml and gamestrings data.
    /// </summary>
    public HeroesData HeroesData { get; }

    /// <summary>
    /// Gets the current loaded map title.
    /// </summary>
    public string GetLoadedMapTitle => _loadedMapTitle;

    /// <summary>
    /// Gets the current set storm locale.
    /// </summary>
    public StormLocale? GetCurrentStormLocale => HeroesData.HeroesLocalization;

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. The source of data will be emtpy.
    /// </summary>
    /// <returns>A <see cref="HeroesXmlLoader"/>.</returns>
    public static HeroesXmlLoader LoadWithEmpty()
    {
        return new HeroesXmlLoader();
    }

    /// <summary>
    /// Gets the properties from a .info file from a local mods directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory path, usually the mods directory.</param>
    /// <returns>The <see cref="ModsInfoFile"/> or <see langword="null"/> if file was not found.</returns>
    public static ModsInfoFile? GetModsInfoFile(string rootDirectory)
    {
        return GetModsInfoFileInternal(rootDirectory);
    }

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
    /// <param name="isPtr">Set to <see langword="true"/> to download from ptr.</param>
    /// <param name="httpClient">Pass in a instance <see cref="HttpClient"/> or a default one will be used.</param>
    /// <param name="loggerOptions">Logging options for the casclib.</param>
    /// <returns>A <see cref="CASCConfig"/> instance.</returns>
    public static CASCConfig GetOnlineCASCConfig(bool isPtr = false, HttpClient? httpClient = null, ILoggerOptions? loggerOptions = null)
    {
        return CASCConfig.LoadOnlineStorageConfig(isPtr ? ProductPtrName : ProductName, "us", false, httpClient, loggerOptions ?? new HeroesLoggerOptions());
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from an extracted file source.
    /// </summary>
    /// <param name="rootDirectory">The root directory path, usually the mods directory.</param>
    /// <param name="modsInfoFile">File properties of the extracted file source.</param>
    /// <param name="progressReporter">Used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    /// <remarks>On linux and macos, all directories and files should be in lowercase characters, otherwise some files and directories may not be found.</remarks>
    public static HeroesXmlLoader LoadWithFile(string rootDirectory, ModsInfoFile? modsInfoFile = null, IProgressReporter? progressReporter = null)
    {
        return new HeroesXmlLoader(rootDirectory, modsInfoFile, progressReporter);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of data to be loaded locally or online.
    /// </summary>
    /// <param name="cascConfig">The <see cref="CASCConfig"/> to determine on how to load data from the data files.</param>
    /// <param name="httpClient">Pass in a instance <see cref="HttpClient"/> or a default one will be used.</param>
    /// <param name="progressReporter">Used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadWithCASC(CASCConfig cascConfig, HttpClient? httpClient = null, IProgressReporter? progressReporter = null)
    {
        return LoadAsCASCInternal(cascConfig, httpClient, progressReporter);
    }

    /// <summary>
    /// Loads the base stormmods.
    /// </summary>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadStormMods()
    {
        if (_baseStormModsLoaded is false)
        {
            _heroesSource.LoadStormData();
            _heroesSource.LoadDepotCache();
        }

        _baseStormModsLoaded = true;

        return this;
    }

    /// <summary>
    /// Loads a map mod. Only one can be loaded at a time. Will automatically load the base stormmods if not already loaded.
    /// All Gamestrings will be reloaded if <see cref="LoadGameStrings(StormLocale)"/> was already called.
    /// </summary>
    /// <param name="mapTitle">A map's title. Can be found from <see cref="GetMapTitles"/>. Is not case-sensitive.</param>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadMapMod(string mapTitle)
    {
        LoadBaseStormMods();

        if (GetLoadedMapTitle.Equals(mapTitle, StringComparison.OrdinalIgnoreCase) is false)
        {
            _loadedMapTitle = mapTitle;
            _heroesSource.LoadStormMapData(mapTitle);

            if (HeroesData.HeroesLocalization is not null)
                LoadGameStrings(HeroesData.HeroesLocalization.Value);
        }

        return this;
    }

    /// <summary>
    /// Unloads the current map mod.
    /// </summary>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader UnloadMapMod()
    {
        _heroesSource.LoadStormMapData(string.Empty);

        return this;
    }

    /// <summary>
    /// Loads a specific localization for gamestrings. Only one can be loaded at a time. Will automatically load the base stormmods if not already loaded.
    /// </summary>
    /// <param name="localization">The <see cref="StormLocale"/>.</param>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadGameStrings(StormLocale localization = StormLocale.ENUS)
    {
        LoadBaseStormMods();

        HeroesData.SetHeroesLocalization(localization);
        _heroesSource.LoadGamestrings(localization);

        return this;
    }

    /// <summary>
    /// Loads a mod programmatically.
    /// </summary>
    /// <param name="manualModLoader">The data of the custom mod.</param>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadCustomMod(ManualModLoader manualModLoader)
    {
        CustomStormMod customStormMod = new(_heroesSource, manualModLoader);

        _heroesSource.LoadCustomMod(customStormMod);

        return this;
    }

    /// <summary>
    /// Loads a stormmod from a directory.
    /// </summary>
    /// <param name="stormmodDirectoryPath">The directory of the stormmod.</param>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadCustomMod(string stormmodDirectoryPath)
    {
        _heroesSource.LoadCustomMod(stormmodDirectoryPath);

        return this;
    }

    /// <summary>
    /// Unloads all custom mods.
    /// </summary>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader UnloadCustomMods()
    {
        _heroesSource.UnloadCustomMods();

        return this;
    }

    /// <summary>
    /// Gets a collection of the map titles to be used for <see cref="LoadMapMod(string)"/>.
    /// </summary>
    /// <returns>A collection of map titles.</returns>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public IEnumerable<string> GetMapTitles()
    {
        return _heroesSource.S2MAPropertiesByTitle.Select(x => x.Key);
    }

    /// <summary>
    /// Gets a map's data from the s2ma and s2mv files. Does not contain data from the xml or gamestring files.
    /// </summary>
    /// <param name="mapTitle">A map's title. Can be found from <see cref="GetMapTitles"/>.</param>
    /// <returns>Map data for the specified map or <see langword="null"/> if map not found.</returns>
    public StormMap? GetStormMap(string mapTitle)
    {
        if (_heroesSource.S2MAPropertiesByTitle.TryGetValue(mapTitle, out S2MAProperties? s2maProperties) && s2maProperties.S2MVProperties is not null)
        {
            return new()
            {
                Name = mapTitle,
                NameByLocale = s2maProperties.S2MVProperties.NameByStormLocale.AsReadOnly(),
                LoadingScreenImagePath = s2maProperties.S2MVProperties.LoadingImage ?? string.Empty,
                MapId = s2maProperties.MapId ?? string.Empty,
                MapLink = s2maProperties.S2MVProperties.MapLink,
                MapSize = s2maProperties.S2MVProperties.MapSize.GetAsTuple(),
                ReplayPreviewImagePath = s2maProperties.S2MVProperties.PreviewLargeImage,
                LayoutFilePath = s2maProperties.S2MVProperties.CustomLayout,
                LayoutLoadingScreenFrame = s2maProperties.S2MVProperties.CustomFrame,
                S2MVFilePath = s2maProperties.S2MVProperties.DirectoryPath,
                S2MAFilePath = s2maProperties.DirectoryPath,
            };
        }

        return null;
    }

    /// <summary>
    /// Gets the collection of the loaded map mod dependencies. These are in ascending order (the order in which they are loaded), where the last mod is currently loaded map mod.
    /// </summary>
    /// <returns>A collection of all the map dependencies.</returns>
    public IEnumerable<StormMapDependency> GetLoadedStormMapDependencies()
    {
        return _heroesSource.GetMapDependencies();
    }

    /// <summary>
    /// Gets whether a map mod is currently loaded.
    /// </summary>
    /// <returns><see langword="true"/> if a map mod is loaded, otherwise <see langword="false"/>.</returns>
    public bool IsMapModLoaded()
    {
        return _heroesSource.IsMapModLoaded();
    }

    /// <summary>
    /// Gets the title of the map mods that is currently loaded (in enUS).
    /// </summary>
    /// <returns>The map mods title, otherwise <see langword="null"/>.</returns>
    public string? GetLoadedMapModTitle()
    {
        return _heroesSource.LoadedStormMapTitle();
    }

    /// <summary>
    /// Determines if a file exists.
    /// <para>
    /// If <paramref name="mpqPath"/> is <see langword="null"/>, then <paramref name="path"/> must be a path relative to the <see cref="RootDirectory"/>.
    /// </para>
    /// <para>
    /// If <paramref name="mpqPath"/> is not <see langword="null"/>, then <paramref name="path"/> must be relative to the <paramref name="mpqPath"/>.
    /// </para>
    /// </summary>
    /// <param name="path">The relative path of the file to check.</param>
    /// <param name="mpqPath">The relative path of the mpq file.</param>
    /// <returns><see langword="true"/> if the file exists, otherwise <see langword="false"/>. Will return <see langword="false"/> if <paramref name="mpqPath"/> was not found.</returns>
    public bool FileExists(string? path, string? mpqPath = null) => _heroesSource.FileExists(path, mpqPath);

    /// <summary>
    /// Determines if a file exists.
    /// </summary>
    /// <param name="stormFile">The file to check.</param>
    /// <returns><see langword="true"/> if the file exists, otherwise <see langword="false"/>.</returns>
    public bool FileExists(StormFile stormFile) => _heroesSource.FileExists(stormFile);

    /// <summary>
    /// Opens a file for reading.
    /// <para>
    /// If <paramref name="mpqPath"/> is <see langword="null"/>, then <paramref name="path"/> must be a path relative to the <see cref="RootDirectory"/>.
    /// </para>
    /// <para>
    /// If <paramref name="mpqPath"/> is not <see langword="null"/>, then <paramref name="path"/> must be relative to the <paramref name="mpqPath"/>.
    /// </para>
    /// </summary>
    /// <param name="path">The relative path of the file to open.</param>
    /// <param name="mpqPath">The relative path of the mpq file to open.</param>
    /// <returns>Returns a <see cref="Stream"/> or else throws an exception.</returns>
    /// <exception cref="FileNotFoundException">File was not found, or <paramref name="mpqPath"/> was not found.</exception>
    public Stream GetFile(string path, string? mpqPath = null) => _heroesSource.GetFile(path, mpqPath);

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="stormFile">The file to open.</param>
    /// <returns>Returns a <see cref="Stream"/> or else throws an exception.</returns>
    /// <exception cref="FileNotFoundException">File was not found.</exception>
    public Stream GetFile(StormFile stormFile) => _heroesSource.GetFile(stormFile);

    /// <summary>
    /// Gets the total number of directories that were not found during the loading process.
    /// </summary>
    /// <returns>The total number of not found directories.</returns>
    public int GetCountOfNotFoundDirectories()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfNotFoundDirectories);
    }

    /// <summary>
    /// Gets the total number of files that were not found during the loading process.
    /// </summary>
    /// <returns>The total number of not found files.</returns>
    public int GetCountOfNotFoundFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfNotFoundFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded xml data files.
    /// </summary>
    /// <remarks>The .xml files.</remarks>
    /// <returns>The total number of xml files.</returns>
    public int GetCountOfXmlDataFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfXmlDataFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded font style files.
    /// </summary>
    /// <remarks>The .stormstyle files.</remarks>
    /// <returns>The total number of font style files.</returns>
    public int GetCountOfFontStyleFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfXmlFontStyleFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded gamestring files.
    /// </summary>
    /// <remarks>The gamestrings.txt files.</remarks>
    /// <returns>The total number of gamestring files.</returns>
    public int GetCountOfGameStringsFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfGameStringFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded assets text files.
    /// </summary>
    /// <remarks>The assets.txt files.</remarks>
    /// <returns>The total number of assets text files.</returns>
    public int GetCountOfAssetsTextFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfAssetsTextFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded layout files.
    /// </summary>
    /// <remarks>The .stormlayout files.</remarks>
    /// <returns>The total number of layout files.</returns>
    public int GetCountOfLayoutFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfLayoutFiles);
    }

    /// <summary>
    /// Gets the total number of currently loaded asset (image) files.
    /// </summary>
    /// <remarks>Primarly .dds files.</remarks>
    /// <returns>The total number of asset files.</returns>
    public int GetCountOfAssetFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.Sum(x => x.NumberOfAssetFiles);
    }

    /// <summary>
    /// Gets a collection of the directories that were not found during the loading process.
    /// </summary>
    /// <returns>An enumerable of <see cref="StormPath"/>s.</returns>
    public IEnumerable<StormPath> GetNotFoundDirectories()
    {
        return _heroesSource.StormStorage.StormModStorages.SelectMany(x => x.NotFoundDirectories);
    }

    /// <summary>
    /// Gets a collection of the files that were not found during the loading process.
    /// </summary>
    /// <returns>An enumerable of <see cref="StormPath"/>s.</returns>
    public IEnumerable<StormPath> GetNotFoundFiles()
    {
        return _heroesSource.StormStorage.StormModStorages.SelectMany(x => x.NotFoundFiles);
    }

    /// <summary>
    /// Gets a collection of the loaded data file paths.
    /// </summary>
    /// <remarks>
    /// Contains the .xml, .stormstyle, .stormlayout, gamestrings.txt, and assets.txt files.
    /// </remarks>
    /// <returns>An enumerable of <see cref="StormPath"/>s.</returns>
    public IEnumerable<StormPath> GetDataFilePaths()
    {
        return _heroesSource.StormStorage.StormModStorages.SelectMany(x => x.AddedXmlDataFilePaths
            .Concat(x.AddedXmlFontStyleFilePaths)
            .Concat(x.FoundLayoutFilePaths)
            .Concat(x.AddedGameStringFilePaths)
            .Concat(x.AddedAssetsTextFilePaths));
    }

    /// <summary>
    /// Gets a collection of all the available asset (the images) file paths.
    /// </summary>
    /// <returns>An enumerable of <see cref="StormPath"/>s.</returns>
    public IEnumerable<StormPath> GetAssetFilePaths()
    {
        return _heroesSource.StormStorage.StormModStorages.SelectMany(x => x.FoundAssetFilePaths);
    }

    internal static HeroesXmlLoader LoadWithInternal(IHeroesSource heroesSource)
    {
        return new HeroesXmlLoader(heroesSource);
    }

    private static HeroesXmlLoader LoadAsCASCInternal(CASCConfig cascConfig, HttpClient? httpClient = null, IProgressReporter? progressReporter = null)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, httpClient: httpClient, worker: (ProgressReporter?)progressReporter);
        cascHandler.Root.LoadListFile(string.Empty, (ProgressReporter?)progressReporter);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        return new HeroesXmlLoader(new CASCHeroesStorage(cascHandler, cascFolderRoot), cascConfig, progressReporter);
    }

    private static ModsInfoFile? GetModsInfoFileInternal(string modsDirectoryPath)
    {
        string infoFilePath = Path.Join(modsDirectoryPath, ".info");

        if (!File.Exists(infoFilePath))
            return null;

        using Stream fileStream = File.OpenRead(infoFilePath);

        return JsonSerializer.Deserialize<ModsInfoFile>(fileStream, _modsInfoFileJsonSerializerOptions);
    }

    private void LoadBaseStormMods()
    {
        if (_baseStormModsLoaded is false)
            LoadStormMods();
    }
}