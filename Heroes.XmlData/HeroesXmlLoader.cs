namespace Heroes.XmlData;

/// <summary>
/// Class to load the xml data and gamestrings from a given source.
/// </summary>
public class HeroesXmlLoader
{
    private readonly IHeroesSource _heroesSource;

    private bool _baseStormModsLoaded = false;

    private HeroesXmlLoader()
        : this(string.Empty)
    {
    }

    private HeroesXmlLoader(string pathToModsDirectory)
    {
        StormStorage stormStorage = new();
        _heroesSource = new FileHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), pathToModsDirectory);
        HeroesData = new HeroesData(stormStorage);
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage)
    {
        StormStorage stormStorage = new();
        _heroesSource = new CASCHeroesSource(stormStorage, new StormModFactory(), new DepotCacheFactory(), cascHeroesStorage);
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
    public static HeroesXmlLoader LoadAsEmpty()
    {
        return new HeroesXmlLoader();
    }

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from an extracted file source.
    /// </summary>
    /// <param name="pathToModsDirectory">A mods directory.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    /// <remarks>On linux and macos, all directories and files must be in lowercase characters.</remarks>
    public static HeroesXmlLoader LoadAsFile(string pathToModsDirectory)
    {
        return new HeroesXmlLoader(pathToModsDirectory);
    }

    ///// <summary>
    ///// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from the Heroes of the Storm directory.
    ///// </summary>
    ///// <param name="cascHeroesStorage">A <paramref name="cascHeroesStorage"/>.</param>
    ///// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    //public static HeroesXmlLoader LoadAsCASC(CASCHeroesStorage cascHeroesStorage)
    //{
    //    return new HeroesXmlLoader(cascHeroesStorage);
    //}

    /// <summary>
    /// Gets an instance of the <see cref="HeroesXmlLoader"/> class. Sets the source of the data to be loaded from the Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The Heroes of the storm directory.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/> instance.</returns>
    public static HeroesXmlLoader LoadAsCASC(string pathToHeroesDirectory)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;
        CASCConfig config = CASCConfig.LoadLocalStorageConfig(pathToHeroesDirectory, "hero");
        CASCHandler cascHandler = CASCHandler.OpenStorage(config);

        cascHandler.Root.LoadListFile(Path.Combine(Environment.CurrentDirectory, "listfile.txt"));

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        return new HeroesXmlLoader(new CASCHeroesStorage(cascHandler, cascFolderRoot));
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

    public HeroesXmlLoader LoadCustomMod(ManualModLoader manualModLoader)
    {
        CustomStormMod customStormMod = new(_heroesSource, manualModLoader);

        _heroesSource.LoadCustomMod(customStormMod);

        return this;
    }

    public HeroesXmlLoader LoadCustomMod(string directoryPath)
    {
        _heroesSource.LoadCustomMod(directoryPath);

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

    private void LoadBaseStormMods()
    {
        if (_baseStormModsLoaded is false)
            LoadStormMods();
    }
}