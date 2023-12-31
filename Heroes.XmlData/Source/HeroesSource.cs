namespace Heroes.XmlData.Source;

internal abstract class HeroesSource : IHeroesSource
{
    private const string _defaultModsDirectory = "mods";
    private const string _gameDataDirectory = "gamedata";
    private const string _baseStormDataDirectory = "base.stormdata";
    private const string _localizedDataDirectory = "localizeddata";
    private const string _gameStringFile = "gamestrings.txt";
    private const string _gameDataXmlFile = "gamedata.xml";
    private const string _includesXmlFile = "includes.xml";

    private const string _coreStormModDirectory = "core.stormmod";
    private const string _heroesStormModDirectory = "heroes.stormmod";
    private const string _heroesDataStormModDirectory = "heroesdata.stormmod";

    private const string _heroModsDirectory = "heromods";
    private const string _depotCacheDirectory = "core.stormmod\\base.stormdata\\DepotCache";
    private const string _battleMapModsDirectory = "heroesmapmods\\battlegroundmapmods";

    public HeroesSource(IHeroesData heroesData, string modsDirectoryPath)
    {
        HeroesData = heroesData;
        HotsBuild = HeroesData.HotsBuild ?? int.MaxValue;
        ModsDirectoryPath = modsDirectoryPath;

        AddStormMods(StormMods);
        AddStormMaps();
    }

    public int HotsBuild { get; }

    public string ModsDirectoryPath { get; }

    public string DefaultModsDirectory => _defaultModsDirectory;

    public string GameDataDirectory => _gameDataDirectory;

    public string BaseStormDataDirectory => _baseStormDataDirectory;

    public string LocalizedDataDirectory => _localizedDataDirectory;

    public string GameStringFile => _gameStringFile;

    public string GameDataXmlFile => _gameDataXmlFile;

    public string IncludesXmlFile => _includesXmlFile;

    public string CoreStormModDirectory => _coreStormModDirectory;

    public string HeroesStormModDirectory => _heroesStormModDirectory;

    public string HeroesDataStormModDirectory => _heroesDataStormModDirectory;

    public string HeroModsDirectory => _heroModsDirectory;

    public string DepotCacheDirectory => _depotCacheDirectory;

    public string BattleMapModsDirectory => _battleMapModsDirectory;

    public IHeroesData HeroesData { get; }

    public IList<IStormMod> StormMods { get; } = new List<IStormMod>();

    public IDepotCache DepotCache { get; protected set; }

    public Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; } = [];

    public List<string> S2MVPaths { get; } = [];

    public List<S2MAProperties> S2MAProperties { get; } = [];

    public List<string> S2MAPaths { get; } = [];

    public void LoadStormData()
    {
        foreach (IStormMod stormMod in StormMods)
        {
            stormMod.LoadStormData();
        }
    }

    public void LoadGamestrings(HeroesLocalization localization)
    {
        HeroesData.ClearGamestrings();

        foreach (IStormMod stormMod in StormMods)
        {
            stormMod.LoadStormGameStrings(localization);
        }
    }

    public void LoadDepotCache()
    {
        DepotCache.LoadDepotCache();
    }

    public bool LoadStormMapData(string mapLinkId)
    {
        //if (!MapDependencyByMapLink.IsValueCreated)
        //    AddStormMaps();

        //if (!MapDependencyByMapLink.TryGetValue(mapLinkId, out List<MapDependency>? mapDependencies))
        //    return false;

        //foreach (MapDependency mapDependency in mapDependencies)
        //{
        //    //mapDependency.LocalFile
        //}

        return true;
    }

    public IStormMod CreateStormModInstance<T>(params object?[]? args)
        where T : IStormMod
    {
        if (Activator.CreateInstance(typeof(T), args) is not IStormMod instance)
        {
            throw new InvalidOperationException();
        }

        return instance;
    }

    protected abstract void AddStormMods(IList<IStormMod> stormMods);

    [MemberNotNull(nameof(DepotCache))]
    protected abstract void AddStormMaps();
}
