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
    private const string _buildIdFile = "buildid.txt";

    private const string _coreStormModDirectory = "core.stormmod";
    private const string _heroesStormModDirectory = "heroes.stormmod";
    private const string _heroesDataStormModDirectory = "heroesdata.stormmod";

    private const string _heroModsDirectory = "heromods";
    private const string _uiDirectory = "ui";

    private readonly string _depotCacheDirectory = Path.Join(_coreStormModDirectory, _baseStormDataDirectory, "depotcache");
    private readonly string _battleMapModsDirectory = Path.Join("heroesmapmods", "battlegroundmapmods");

    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    public HeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory)
        : this(stormStorage, stormModFactory, depotCacheFactory, _defaultModsDirectory)
    {
    }

    public HeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath)
    {
        StormStorage = stormStorage;
        StormModFactory = stormModFactory;
        DepotCacheFactory = depotCacheFactory;
        ModsBaseDirectoryPath = modsDirectoryPath;

        AddStormMods();

        DepotCache = GetDepotCache();
    }

    public IStormModFactory StormModFactory { get; }

    public string ModsBaseDirectoryPath { get; }

    public string DefaultModsDirectory => _defaultModsDirectory;

    public string GameDataDirectory => _gameDataDirectory;

    public string BaseStormDataDirectory => _baseStormDataDirectory;

    public string LocalizedDataDirectory => _localizedDataDirectory;

    public string GameStringFile => _gameStringFile;

    public string GameDataXmlFile => _gameDataXmlFile;

    public string IncludesXmlFile => _includesXmlFile;

    public string DocumentInfoFile => _documentInfoFile;

    public string FontStyleFile => _fontStyleFile;

    public string BuildIdFile => _buildIdFile;

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

    protected IDepotCacheFactory DepotCacheFactory { get; }

    public void LoadStormData()
    {
        foreach (IStormMod stormMod in _stormMods)
        {
            StormStorage.AddModStorage(stormMod.StormModStorage);

            stormMod.LoadStormData();
        }

        StormStorage.BuildDataForScalingAttributes(StormModType.Normal);
    }

    public void LoadGamestrings(StormLocale stormLocale)
    {
        StormStorage.ClearGamestrings();

        foreach (IStormMod stormMod in _stormMods)
        {
            stormMod.LoadStormGameStrings(stormLocale);
        }

        foreach (IStormMod stormMapMod in _stormMapMods)
        {
            stormMapMod.LoadStormGameStrings(stormLocale);
        }

        foreach (IStormMod stormMapMod in _stormCustomMods)
        {
            stormMapMod.LoadStormGameStrings(stormLocale);
        }
    }

    public void LoadDepotCache()
    {
        DepotCache.LoadDepotCache();
    }

    public void LoadStormMapData(string mapTitle)
    {
        _stormMapMods.Clear();
        StormStorage.ClearStormMapMods();

        if (!S2MAPropertiesByTitle.TryGetValue(mapTitle, out S2MAProperties? s2maProperties))
            return;

        IStormMod mapRootMod = GetMpqStormMod(mapTitle, s2maProperties.DirectoryPath, StormModType.Map);

        _stormMapMods.AddRange(mapRootMod.GetStormMapMods(s2maProperties));

        foreach (IStormMod stormMapMod in _stormMapMods)
        {
            stormMapMod.LoadStormData();

            StormStorage.AddModStorage(stormMapMod.StormModStorage);
        }

        StormStorage.BuildDataForScalingAttributes(StormModType.Map);
    }

    public void LoadCustomMod(IStormMod stormMod)
    {
        _stormCustomMods.Add(stormMod);

        stormMod.LoadStormData();

        StormStorage.BuildDataForScalingAttributes(StormModType.Custom);
    }

    public void LoadCustomMod(string directoryPath)
    {
        IStormMod stormMod = GetStormMod(directoryPath, StormModType.Custom);
        _stormCustomMods.Add(stormMod);

        stormMod.LoadStormData();

        StormStorage.BuildDataForScalingAttributes(StormModType.Custom);
    }

    public void UnloadCustomMods()
    {
        _stormCustomMods.Clear();
    }

    protected abstract IStormMod GetStormMod(string directoryPath, StormModType stormModType);

    protected abstract IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType);

    protected abstract IDepotCache GetDepotCache();

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(CoreStormModDirectory, StormModType.Normal));
        _stormMods.Add(GetStormMod(HeroesStormModDirectory, StormModType.Normal));
        _stormMods.Add(GetStormMod(HeroesDataStormModDirectory, StormModType.Normal));
    }
}
