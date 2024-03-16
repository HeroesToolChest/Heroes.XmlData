namespace Heroes.XmlData;

/// <summary>
/// Class to load the xml data and gamestrings from a given source.
/// </summary>
public class HeroesXmlLoader
{
    private readonly IStormStorage _stormStorage;
    private readonly IHeroesSource _heroesSource;

    private bool _baseStormModsLoaded = false;

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
    /// Loads a map mod.
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
    /// Loads a specific localization for gamestrings.
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
    /// Gets a collection of the map titles.
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