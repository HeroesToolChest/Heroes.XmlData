namespace Heroes.XmlData;

public class HeroesXmlFileLoader
{
    private readonly string _pathToModsDirectory;
    private readonly int? _hotsBuild;
    private readonly HeroesData _heroesData;
    private readonly FileHeroesSource _fileHeroesSource;

    public HeroesXmlFileLoader(string pathToModsDirectory, int? hotsBuild = null)
    {
        _pathToModsDirectory = pathToModsDirectory;
        _hotsBuild = hotsBuild;

        _heroesData = new(_hotsBuild);
        _fileHeroesSource = new(_heroesData, pathToModsDirectory);
    }

    public void LoadStormMods()
    {
        _fileHeroesSource.LoadStormData();
        _fileHeroesSource.LoadDepotCache();
    }

    public void LoadMapMod(string mapTitle)
    {
        _fileHeroesSource.LoadStormMapData(mapTitle);
    }

    public void LoadGameStrings(HeroesLocalization localization = HeroesLocalization.ENUS)
    {
        HeroesData.SetHeroesLocalization(localization);

        _fileHeroesSource.LoadGamestrings(localization);
    }

    public IEnumerable<string> GetMapTitles()
    {
        return _fileHeroesSource.S2MAPropertiesByTitle.Select(x => x.Key).Order();
    }

    public HeroesData HeroesData => _heroesData;
}
