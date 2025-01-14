namespace Heroes.XmlData;

/// <summary>
/// Class to load the heroes data from a given source.
/// </summary>
public class HeroesXmlLoader
{
    private readonly IHeroesSource _heroesSource;

    private bool _baseStormModsLoaded = false;

    private HeroesXmlLoader()
        : this(string.Empty, null)
    {
    }

    private HeroesXmlLoader(string pathToModsDirectory, IBackgroundWorkerEx? backgroundWorkerEx)
    {
        StormStorage stormStorage = new();
        _heroesSource = new FileHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), pathToModsDirectory, backgroundWorkerEx);

        HeroesData = new HeroesData(stormStorage);

        LoadedType = HeroesXmlLoaderType.File;
        RootDirectory = _heroesSource.ModsBaseDirectoryPath;
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage, IBackgroundWorkerEx? backgroundWorkerEx)
    {
        StormStorage stormStorage = new();
        _heroesSource = new CASCHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), cascHeroesStorage, backgroundWorkerEx);

        HeroesData = new HeroesData(stormStorage);

        LoadedType = HeroesXmlLoaderType.CASC;
        RootDirectory = _heroesSource.ModsBaseDirectoryPath;
    }

    /// <summary>
    /// Gets the the type of the loaded source data.
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
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. The source of data will be emtpy.
    /// </summary>
    /// <returns>A <see cref="HeroesXmlLoader"/>.</returns>
    public static HeroesXmlLoader LoadWithEmpty()
    {
        return new HeroesXmlLoader();
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from an extracted file source.
    /// </summary>
    /// <param name="rootDirectory">The root directory, usually the mods directory.</param>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    /// <remarks>On linux and macos, all directories and files should be in lowercase characters, otherwise some files and directories may not be found.</remarks>
    public static HeroesXmlLoader LoadWithFile(string rootDirectory, IBackgroundWorkerEx? backgroundWorkerEx = null)
    {
        return new HeroesXmlLoader(rootDirectory, backgroundWorkerEx);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from the Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The Heroes of the storm directory.</param>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadWithCASC(string pathToHeroesDirectory, IBackgroundWorkerEx? backgroundWorkerEx = null)
    {
        return LoadAsCASCInternal(CASCConfig.LoadLocalStorageConfig(pathToHeroesDirectory, "hero"), backgroundWorkerEx);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be downloaded from Blizzard's online servers.
    /// </summary>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadWithOnlineCASC(IBackgroundWorkerEx? backgroundWorkerEx = null)
    {
        return LoadAsCASCInternal(CASCConfig.LoadOnlineStorageConfig("hero", "us"), backgroundWorkerEx);
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
    /// </summary>
    /// <param name="mapTitle">A map's title. Can be found from <see cref="GetMapTitles"/>.</param>
    /// <returns>The current <see cref="HeroesXmlLoader"/> instance.</returns>
    public HeroesXmlLoader LoadMapMod(string mapTitle)
    {
        LoadBaseStormMods();

        _heroesSource.LoadStormMapData(mapTitle);

        if (HeroesData.HeroesLocalization is not null)
            LoadGameStrings(HeroesData.HeroesLocalization.Value);

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
        return _heroesSource.IsMapMapLoaded();
    }

    /// <summary>
    /// Gets the title of the map mods that is currently loaded (in enUS).
    /// </summary>
    /// <returns>The map mods title, othewise <see langword="null"/>.</returns>
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

    private static HeroesXmlLoader LoadAsCASCInternal(CASCConfig cascConfig, IBackgroundWorkerEx? backgroundWorkerEx)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, (BackgroundWorkerEx?)backgroundWorkerEx);
        cascHandler.Root.LoadListFile(string.Empty, (BackgroundWorkerEx?)backgroundWorkerEx);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        return new HeroesXmlLoader(new CASCHeroesStorage(cascHandler, cascFolderRoot), backgroundWorkerEx);
    }

    private void LoadBaseStormMods()
    {
        if (_baseStormModsLoaded is false)
            LoadStormMods();
    }
}