namespace Heroes.XmlData.Source;

internal abstract class HeroesSource : IHeroesSource
{
    private const string DefaultModsDirectoryConst = "mods";
    private const string GameDataDirectoryConst = "gamedata";
    private const string BaseStormDataDirectoryConst = "base.stormdata";
    private const string LocalizedDataDirectoryConst = "localizeddata";
    private const string GameStringFileConst = "gamestrings.txt";
    private const string GameDataXmlFileConst = "gamedata.xml";
    private const string IncludesXmlFileConst = "includes.xml";
    private const string DocumentInfoFileConst = "documentinfo";
    private const string FontStyleFileConst = "fontstyles.stormstyle";
    private const string BuildIdFileConst = "buildid.txt";

    private const string CoreStormModDirectoryConst = "core.stormmod";
    private const string HeroesStormModDirectoryConst = "heroes.stormmod";
    private const string HeroesDataStormModDirectoryConst = "heroesdata.stormmod";

    private const string HeroModsDirectoryConst = "heromods";
    private const string UiDirectoryConst = "ui";

    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    public HeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory)
        : this(stormStorage, stormModFactory, depotCacheFactory, DefaultModsDirectoryConst)
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

    public string DefaultModsDirectory => DefaultModsDirectoryConst;

    public string GameDataDirectory => GameDataDirectoryConst;

    public string BaseStormDataDirectory => BaseStormDataDirectoryConst;

    public string LocalizedDataDirectory => LocalizedDataDirectoryConst;

    public string GameStringFile => GameStringFileConst;

    public string GameDataXmlFile => GameDataXmlFileConst;

    public string IncludesXmlFile => IncludesXmlFileConst;

    public string DocumentInfoFile => DocumentInfoFileConst;

    public string FontStyleFile => FontStyleFileConst;

    public string BuildIdFile => BuildIdFileConst;

    public string CoreStormModDirectory => CoreStormModDirectoryConst;

    public string HeroesStormModDirectory => HeroesStormModDirectoryConst;

    public string HeroesDataStormModDirectory => HeroesDataStormModDirectoryConst;

    public string HeroModsDirectory => HeroModsDirectoryConst;

    public string UIDirectory => UiDirectoryConst;

    public string DepotCacheDirectory => Path.Join(CoreStormModDirectory, BaseStormDataDirectory, "depotcache");

    public string BattleMapModsDirectory => Path.Join("heroesmapmods", "battlegroundmapmods");

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
