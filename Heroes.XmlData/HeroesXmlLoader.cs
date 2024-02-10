namespace Heroes.XmlData;

public class HeroesXmlLoader
{
    private readonly IStormStorage _stormStorage;
    private readonly IHeroesSource _heroesSource;

    private HeroesXmlLoader(string pathToModsDirectory)
    {
        _stormStorage = new StormStorage();
        _heroesSource = new FileHeroesSource(_stormStorage, pathToModsDirectory);
        HeroesData = new HeroesData(_stormStorage);
    }

    private HeroesXmlLoader(CASCHeroesStorage cascHeroesStorage)
    {
        _stormStorage = new StormStorage();
        _heroesSource = new CASCHeroesSource(_stormStorage, cascHeroesStorage);
        HeroesData = new HeroesData(_stormStorage);
    }

    public HeroesData HeroesData { get; }

    public static HeroesXmlLoader LoadAsFile(string pathToModsDirectory)
    {
        return new HeroesXmlLoader(pathToModsDirectory);
    }

    public static HeroesXmlLoader LoadAsCASC(CASCHeroesStorage cascHeroesStorage)
    {
        return new HeroesXmlLoader(cascHeroesStorage);
    }

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

    public void LoadStormMods()
    {
        _heroesSource.LoadStormData();
        _heroesSource.LoadDepotCache();
    }

    public void LoadMapMod(string mapTitle)
    {
        _heroesSource.LoadStormMapData(mapTitle);

        if (HeroesData.HeroesLocalization is not null)
            LoadGameStrings(HeroesData.HeroesLocalization.Value);
    }

    public void LoadGameStrings(StormLocale localization = StormLocale.ENUS)
    {
        HeroesData.SetHeroesLocalization(localization);

        _heroesSource.LoadGamestrings(localization);
    }

    public IEnumerable<string> GetMapTitles()
    {
        return _heroesSource.S2MAPropertiesByTitle.Select(x => x.Key).Order();
    }
}