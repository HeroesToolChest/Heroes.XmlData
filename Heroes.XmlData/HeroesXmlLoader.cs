namespace Heroes.XmlData;

/// <summary>
/// Class to load the xml data and gamestrings from a given source.
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
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage, IBackgroundWorkerEx? backgroundWorkerEx)
    {
        StormStorage stormStorage = new();
        _heroesSource = new CASCHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), cascHeroesStorage, backgroundWorkerEx);
        HeroesData = new HeroesData(stormStorage);
    }

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
    /// <param name="pathToModsDirectory">A mods directory.</param>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    /// <remarks>On linux and macos, all directories and files should be in lowercase characters, otherwise some files and directories may not be found.</remarks>
    public static HeroesXmlLoader LoadWithFile(string pathToModsDirectory, IBackgroundWorkerEx? backgroundWorkerEx = null)
    {
        return new HeroesXmlLoader(pathToModsDirectory, backgroundWorkerEx);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from the Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The Heroes of the storm directory.</param>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadWithCASC(string pathToHeroesDirectory, BackgroundWorkerEx? backgroundWorkerEx = null)
    {
        return LoadAsCASCInternal(CASCConfig.LoadLocalStorageConfig(pathToHeroesDirectory, "hero"), backgroundWorkerEx);
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be downloaded from Blizzard's online servers.
    /// </summary>
    /// <param name="backgroundWorkerEx">A background worker used to report loading progress.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadWithOnlineCASC(BackgroundWorkerEx backgroundWorkerEx)
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
        return _heroesSource.S2MAPropertiesByTitle.Select(x => x.Key).Order();
    }

    /// <summary>
    /// Gets a collection of not found files.
    /// </summary>
    /// <returns>A collection of not found <see cref="StormPath"/>s that represents not found files.</returns>
    public IEnumerable<StormPath> GetNotFoundFiles()
    {
        foreach (StormPath item in _heroesSource.StormStorage.StormCache.NotFoundFilesList)
        {
            yield return item;
        }

        foreach (StormPath item in _heroesSource.StormStorage.StormMapCache.NotFoundFilesList)
        {
            yield return item;
        }

        foreach (StormPath item in _heroesSource.StormStorage.StormCustomCache.NotFoundFilesList)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Gets a collection of not found directories.
    /// </summary>
    /// <returns>A collection of not found <see cref="StormPath"/>s that represents not found directories.</returns>
    public IEnumerable<StormPath> GetNotFoundDirectories()
    {
        foreach (StormPath item in _heroesSource.StormStorage.StormCache.NotFoundDirectoriesList)
        {
            yield return item;
        }

        foreach (StormPath item in _heroesSource.StormStorage.StormMapCache.NotFoundDirectoriesList)
        {
            yield return item;
        }

        foreach (StormPath item in _heroesSource.StormStorage.StormCustomCache.NotFoundDirectoriesList)
        {
            yield return item;
        }
    }

    private static HeroesXmlLoader LoadAsCASCInternal(CASCConfig cascConfig, BackgroundWorkerEx? backgroundWorkerEx)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, backgroundWorkerEx);
        cascHandler.Root.LoadListFile(string.Empty, backgroundWorkerEx);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        return new HeroesXmlLoader(new CASCHeroesStorage(cascHandler, cascFolderRoot), backgroundWorkerEx);
    }

    private void LoadBaseStormMods()
    {
        if (_baseStormModsLoaded is false)
            LoadStormMods();
    }
}