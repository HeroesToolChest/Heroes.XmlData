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
    private const string _documentInfoFile = "documentinfo";
    private const string _fontStyleFile = "fontstyles.stormstyle";

    private const string _coreStormModDirectory = "core.stormmod";
    private const string _heroesStormModDirectory = "heroes.stormmod";
    private const string _heroesDataStormModDirectory = "heroesdata.stormmod";

    private const string _heroModsDirectory = "heromods";
    private const string _uiDirectory = "ui";

    private readonly string _depotCacheDirectory = Path.Join("core.stormmod", "base.stormdata", "depotcache");
    private readonly string _battleMapModsDirectory = Path.Join("heroesmapmods", "battlegroundmapmods");

    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];

    public HeroesSource(IStormStorage stormStorage, string modsDirectoryPath)
    {
        StormStorage = stormStorage;
        HotsBuild = StormStorage.HotsBuild;
        ModsDirectoryPath = modsDirectoryPath;

        AddStormMods();
        DepotCache = GetDepotCache();
    }

    public int? HotsBuild { get; }

    public string ModsDirectoryPath { get; }

    public string DefaultModsDirectory => _defaultModsDirectory;

    public string GameDataDirectory => _gameDataDirectory;

    public string BaseStormDataDirectory => _baseStormDataDirectory;

    public string LocalizedDataDirectory => _localizedDataDirectory;

    public string GameStringFile => _gameStringFile;

    public string GameDataXmlFile => _gameDataXmlFile;

    public string IncludesXmlFile => _includesXmlFile;

    public string DocumentInfoFile => _documentInfoFile;

    public string FontStyleFile => _fontStyleFile;

    public string CoreStormModDirectory => _coreStormModDirectory;

    public string HeroesStormModDirectory => _heroesStormModDirectory;

    public string HeroesDataStormModDirectory => _heroesDataStormModDirectory;

    public string HeroModsDirectory => _heroModsDirectory;

    public string UIDirectory => _uiDirectory;

    public string DepotCacheDirectory => _depotCacheDirectory;

    public string BattleMapModsDirectory => _battleMapModsDirectory;

    public IStormStorage StormStorage { get; }

    public IDepotCache DepotCache { get; }

    public Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; } = [];

    public List<string> S2MVPaths { get; } = [];

    public List<S2MAProperties> S2MAProperties { get; } = [];

    public Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; } = [];

    public List<string> S2MAPaths { get; } = [];

    public void LoadStormData()
    {
        foreach (IStormMod stormMod in _stormMods)
        {
            stormMod.LoadStormData();
        }
    }

    public void LoadGamestrings(HeroesLocalization localization)
    {
        StormStorage.ClearGamestrings();

        foreach (IStormMod stormMod in _stormMods)
        {
            stormMod.LoadStormGameStrings(localization);
        }

        foreach (IStormMod stormMapMod in _stormMapMods)
        {
            stormMapMod.LoadStormGameStrings(localization);
        }
    }

    public void LoadDepotCache()
    {
        DepotCache.LoadDepotCache();
    }

    public bool LoadStormMapData(string mapTitle)
    {
        if (!S2MAPropertiesByTitle.TryGetValue(mapTitle, out S2MAProperties? s2maProperties))
            return false;

        _stormMapMods.Clear();

        IStormMod mapRootMod = GetMpqStormMod(mapTitle, s2maProperties.DirectoryPath, true);

        _stormMapMods.AddRange(mapRootMod.GetStormMapMods(s2maProperties));

        foreach (IStormMod stormMapMod in _stormMapMods)
        {
            stormMapMod.LoadStormData();
        }

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

    protected abstract IStormMod GetStormMod(string directoryPath, bool isMapMod);

    protected abstract IStormMod GetMpqStormMod(string name, string directoryPath, bool isMapMod);

    protected abstract IDepotCache GetDepotCache();

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(CoreStormModDirectory, false));
        _stormMods.Add(GetStormMod(HeroesStormModDirectory, false));
        _stormMods.Add(GetStormMod(HeroesDataStormModDirectory, false));
    }
}
