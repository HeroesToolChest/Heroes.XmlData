namespace Heroes.XmlData;

/// <summary>
/// Class to load the xml data and gamestrings from a given source.
/// </summary>
public class HeroesXmlLoader
{
    private readonly IStormStorage _stormStorage;
    private readonly IHeroesSource _heroesSource;

    private HeroesXmlLoader(string pathToModsDirectory)
    {
        _stormStorage = new StormStorage();
        _heroesSource = new FileHeroesSource(_stormStorage, new StormModFactory(), new DepotCacheFactory(), pathToModsDirectory);
        HeroesData = new HeroesData(_stormStorage);
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage)
    {
        _stormStorage = new StormStorage();
        _heroesSource = new CASCHeroesSource(_stormStorage, new StormModFactory(), new DepotCacheFactory(), cascHeroesStorage);
        HeroesData = new HeroesData(_stormStorage);
    }

    /// <summary>
    /// Gets the <see cref="HeroesData"/> which contain the xml and gamestrings data.
    /// </summary>
    public IHeroesData HeroesData { get; }

    /// <summary>
    /// Gets an instanace of the <see cref="HeroesXmlLoader"/> class. Loads the data from an extracted file source.
    /// </summary>
    /// <param name="pathToModsDirectory">A mods directory.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/>.</returns>
    public static HeroesXmlLoader LoadAsFile(string pathToModsDirectory)
    {
        return new HeroesXmlLoader(pathToModsDirectory);
    }

    /// <summary>
    /// Gets an instanace of the <see cref="HeroesXmlLoader"/> class. Loads the data from the Heroes of the Storm directory.
    /// </summary>
    /// <param name="cascHeroesStorage">A <paramref name="cascHeroesStorage"/>.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/>.</returns>
    public static HeroesXmlLoader LoadAsCASC(CASCHeroesStorage cascHeroesStorage)
    {
        return new HeroesXmlLoader(cascHeroesStorage);
    }

    /// <summary>
    /// Gets an instanace of the <see cref="HeroesXmlLoader"/> class. Loads the data from the Heroes of the Storm directory.
    /// </summary>
    /// <param name="pathToHeroesDirectory">The Heroes of the storm directory.</param>
    /// <returns>A <see cref="HeroesXmlLoader"/>.</returns>
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
    /// Loads the default stormmods.
    /// </summary>
    public void LoadStormMods()
    {
        _heroesSource.LoadStormData();
        _heroesSource.LoadDepotCache();
    }

    /// <summary>
    /// Loads a map mod.
    /// </summary>
    /// <param name="mapTitle">A map's title. Can be found from <see cref="GetMapTitles"/>.</param>
    public void LoadMapMod(string mapTitle)
    {
        _heroesSource.LoadStormMapData(mapTitle);

        if (HeroesData.HeroesLocalization is not null)
            LoadGameStrings(HeroesData.HeroesLocalization.Value);
    }

    /// <summary>
    /// Loads a specific localization for gamestrings.
    /// </summary>
    /// <param name="localization">The <see cref="StormLocale"/>.</param>
    public void LoadGameStrings(StormLocale localization = StormLocale.ENUS)
    {
        HeroesData.SetHeroesLocalization(localization);

        _heroesSource.LoadGamestrings(localization);
    }

    /// <summary>
    /// Gets a collection of the map titles.
    /// </summary>
    /// <returns>A collection of map titles.</returns>
    public IEnumerable<string> GetMapTitles()
    {
        return _heroesSource.S2MAPropertiesByTitle.Select(x => x.Key).Order();
    }
}