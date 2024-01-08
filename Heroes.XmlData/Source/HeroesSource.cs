namespace Heroes.XmlData.Source;

internal abstract class HeroesSource : IHeroesSource
{
    private const string _defaultModsDirectory = "mods";
    private const string _gameDataDirectory = "GameData";
    private const string _baseStormDataDirectory = "base.stormdata";
    private const string _localizedDataDirectory = "LocalizedData";
    private const string _gameStringFile = "GameStrings.txt";
    private const string _gameDataXmlFile = "GameData.xml";
    private const string _includesXmlFile = "Includes.xml";
    private const string _documentInfoFile = "DocumentInfo";

    private const string _coreStormModDirectory = "core.stormmod";
    private const string _heroesStormModDirectory = "heroes.stormmod";
    private const string _heroesDataStormModDirectory = "heroesdata.stormmod";

    private const string _heroModsDirectory = "heromods";
    private const string _depotCacheDirectory = "core.stormmod\\base.stormdata\\DepotCache";
    private const string _battleMapModsDirectory = "heroesmapmods\\battlegroundmapmods";

    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];

    private bool _fileCasingIsUpper = true;

    public HeroesSource(IHeroesData heroesData, string modsDirectoryPath)
    {
        HeroesData = heroesData;
        HotsBuild = HeroesData.HotsBuild ?? int.MaxValue;
        ModsDirectoryPath = modsDirectoryPath;

        AddStormMods();
        DepotCache = GetDepotCache();
    }

    public int HotsBuild { get; }

    public string ModsDirectoryPath { get; }

    public string DefaultModsDirectory => _defaultModsDirectory;

    public string GameDataDirectory => _fileCasingIsUpper ? _gameDataDirectory : _gameDataDirectory.ToLowerInvariant();

    public string BaseStormDataDirectory => _baseStormDataDirectory;

    public string LocalizedDataDirectory => _fileCasingIsUpper ? _localizedDataDirectory : _localizedDataDirectory.ToLowerInvariant();

    public string GameStringFile => _fileCasingIsUpper ? _gameStringFile : _gameStringFile.ToLowerInvariant();

    public string GameDataXmlFile => _fileCasingIsUpper ? _gameDataXmlFile : _gameDataXmlFile.ToLowerInvariant();

    public string IncludesXmlFile => _fileCasingIsUpper ? _includesXmlFile : _includesXmlFile.ToLowerInvariant();

    public string DocumentInfoFile => _documentInfoFile;

    public string CoreStormModDirectory => _coreStormModDirectory;

    public string HeroesStormModDirectory => _heroesStormModDirectory;

    public string HeroesDataStormModDirectory => _heroesDataStormModDirectory;

    public string HeroModsDirectory => _heroModsDirectory;

    public string DepotCacheDirectory => _depotCacheDirectory;

    public string BattleMapModsDirectory => _battleMapModsDirectory;

    public IHeroesData HeroesData { get; }

    public IDepotCache DepotCache { get; }

    public Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; } = [];

    public List<string> S2MVPaths { get; } = [];

    public List<S2MAProperties> S2MAProperties { get; } = [];

    public Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; } = [];

    public List<string> S2MAPaths { get; } = [];

    protected static string TestCasingDirectoryPath => Path.Join(_defaultModsDirectory, _coreStormModDirectory, _baseStormDataDirectory, _gameDataDirectory);

    public void LoadStormData()
    {
        foreach (IStormMod stormMod in _stormMods)
        {
            stormMod.LoadStormData();
        }
    }

    public void LoadGamestrings(HeroesLocalization localization)
    {
        HeroesData.ClearGamestrings();

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

        IStormMod mapRootMod = GetMpqStormMod(s2maProperties.DirectoryPath, mapTitle);

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

    public void ValidateCasing()
    {
        if (!CasingExists())
        {
            _fileCasingIsUpper = false;
        }
    }

    protected abstract IStormMod GetStormMod(string directoryPath);

    protected abstract IStormMod GetMpqStormMod(string directoryPath, string name);

    protected abstract IDepotCache GetDepotCache();

    protected abstract bool CasingExists();

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(CoreStormModDirectory));
        _stormMods.Add(GetStormMod(HeroesStormModDirectory));
        _stormMods.Add(GetStormMod(HeroesDataStormModDirectory));
    }
}
