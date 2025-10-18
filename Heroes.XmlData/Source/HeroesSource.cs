namespace Heroes.XmlData.Source;

internal abstract class HeroesSource : IHeroesSource
{
    private const string DefaultModsDirectoryConst = "mods";
    private const string GameDataDirectoryConst = "gamedata";
    private const string BaseStormDataDirectoryConst = "base.stormdata";
    private const string BaseStormAssetsDirectoryConst = "base.stormassets";
    private const string LocalizedDataDirectoryConst = "localizeddata";
    private const string AssetsDirectoryConst = "assets";
    private const string GameStringFileConst = "gamestrings.txt";
    private const string GameDataXmlFileConst = "gamedata.xml";
    private const string IncludesXmlFileConst = "includes.xml";
    private const string DocumentInfoFileConst = "documentinfo";
    private const string FontStyleFileConst = "fontstyles.stormstyle";
    private const string BuildIdFileConst = "buildid.txt";
    private const string AssetsTextFileConst = "assets.txt";
    private const string DescIndexStormLayoutFileConst = "descindex.stormlayout";

    private const string CoreStormModDirectoryConst = "core.stormmod";
    private const string HeroesStormModDirectoryConst = "heroes.stormmod";
    private const string HeroesDataStormModDirectoryConst = "heroesdata.stormmod";

    private const string HeroModsDirectoryConst = "heromods";
    private const string UiDirectoryConst = "ui";
    private const string LayoutDirectoryConst = "layout";

    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    public HeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, IProgressReporter? progressReporter)
        : this(stormStorage, stormModFactory, depotCacheFactory, DefaultModsDirectoryConst, progressReporter)
    {
    }

    public HeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IProgressReporter? progressReporter)
    {
        StormStorage = stormStorage;
        StormModFactory = stormModFactory;
        DepotCacheFactory = depotCacheFactory;
        ModsBaseDirectoryPath = modsDirectoryPath;

        ProgressReporter = progressReporter;

        AddStormMods();

        DepotCache = GetDepotCache();
    }

    public IStormModFactory StormModFactory { get; }

    public IProgressReporter? ProgressReporter { get; }

    public string ModsBaseDirectoryPath { get; }

    public string DefaultModsDirectory => DefaultModsDirectoryConst;

    public string GameDataDirectory => GameDataDirectoryConst;

    public string BaseStormDataDirectory => BaseStormDataDirectoryConst;

    public string BaseStormAssetsDirectory => BaseStormAssetsDirectoryConst;

    public string LocalizedDataDirectory => LocalizedDataDirectoryConst;

    public string AssetsDirectory => AssetsDirectoryConst;

    public string GameStringFile => GameStringFileConst;

    public string GameDataXmlFile => GameDataXmlFileConst;

    public string IncludesXmlFile => IncludesXmlFileConst;

    public string DocumentInfoFile => DocumentInfoFileConst;

    public string FontStyleFile => FontStyleFileConst;

    public string BuildIdFile => BuildIdFileConst;

    public string AssetsTextFile => AssetsTextFileConst;

    public string DescIndexStormLayoutFile => DescIndexStormLayoutFileConst;

    public string CoreStormModDirectory => CoreStormModDirectoryConst;

    public string HeroesStormModDirectory => HeroesStormModDirectoryConst;

    public string HeroesDataStormModDirectory => HeroesDataStormModDirectoryConst;

    public string HeroModsDirectory => HeroModsDirectoryConst;

    public string UIDirectory => UiDirectoryConst;

    public string LayoutDirectory => LayoutDirectoryConst;

    public string DepotCacheDirectory => Path.Join(CoreStormModDirectory, BaseStormDataDirectory, "depotcache");

    public string BattleMapModsDirectory => Path.Join("heroesmapmods", "battlegroundmapmods");

    public IStormStorage StormStorage { get; }

    public IDepotCache DepotCache { get; }

    public Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; } = [];

    public List<string> S2MVPaths { get; } = [];

    public List<S2MAProperties> S2MAProperties { get; } = [];

    public Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> S2MAPaths { get; } = [];

    protected IDepotCacheFactory DepotCacheFactory { get; }

    public string GetLocaleStormDataDirectory(StormLocale stormLocale) => $"{stormLocale.ToString().ToLowerInvariant()}.stormdata";

    public void LoadStormData()
    {
        ProgressReporter?.Start(0, "Loading storm data...");

        for (int i = 0; i < _stormMods.Count; i++)
        {
            IStormMod stormMod = _stormMods[i];
            StormStorage.AddModStorage(stormMod.StormModStorage);

            stormMod.LoadStormData();

            ProgressReporter?.Report((int)((i + 1) / (float)_stormMods.Count * 100));
        }

        StormStorage.BuildDataForScalingAttributes(StormModType.Normal);
    }

    public void LoadGamestrings(StormLocale stormLocale)
    {
        StormStorage.ClearGamestrings();

        ProgressReporter?.Start(0, "Loading gamestrings...");

        int totalModsCount = _stormMods.Count + _stormMapMods.Count + _stormCustomMods.Count;

        for (int i = 0; i < _stormMods.Count; i++)
        {
            _stormMods[i].LoadStormGameStrings(stormLocale);

            ProgressReporter?.Report((int)((i + 1) / (float)totalModsCount * 100));
        }

        for (int i = 0; i < _stormMapMods.Count; i++)
        {
            _stormMapMods[i].LoadStormGameStrings(stormLocale);

            ProgressReporter?.Report((int)((_stormMods.Count + i + 1) / (float)totalModsCount * 100));
        }

        for (int i = 0; i < _stormCustomMods.Count; i++)
        {
            _stormCustomMods[i].LoadStormGameStrings(stormLocale);

            ProgressReporter?.Report((int)((_stormMods.Count + _stormMapMods.Count + i + 1) / (float)totalModsCount * 100));
        }
    }

    public void LoadDepotCache()
    {
        ProgressReporter?.Start(0, "Loading depot cache...");

        DepotCache.LoadDepotCache();

        ProgressReporter?.Report(100);
    }

    public void LoadStormMapData(string mapTitle)
    {
        _stormMapMods.Clear();
        StormStorage.ClearStormMapMods();

        if (!S2MAPropertiesByTitle.TryGetValue(mapTitle, out S2MAProperties? s2maProperties))
            return;

        IStormMod mapRootMod = GetMpqStormMod(mapTitle, s2maProperties.DirectoryPath, StormModType.Map);

        _stormMapMods.AddRange(mapRootMod.GetStormMapMods(s2maProperties));

        ProgressReporter?.Start(0, $"Loading map '{mapTitle}' mods...");

        // load up all the maps stormmods
        for (int i = 0; i < _stormMapMods.Count; i++)
        {
            IStormMod stormMapMod = _stormMapMods[i];
            stormMapMod.LoadStormData();

            StormStorage.AddModStorage(stormMapMod.StormModStorage);

            ProgressReporter?.Report((int)((i + 1) / (float)_stormMapMods.Count * 100));
        }

        StormStorage.BuildDataForScalingAttributes(StormModType.Map);
    }

    public void LoadCustomMod(IStormMod stormMod)
    {
        _stormCustomMods.Add(stormMod);

        stormMod.LoadStormData();

        StormStorage.AddModStorage(stormMod.StormModStorage);
        StormStorage.BuildDataForScalingAttributes(StormModType.Custom);
    }

    public void LoadCustomMod(string directoryPath)
    {
        IStormMod stormMod = GetStormMod(directoryPath, StormModType.Custom);

        LoadCustomMod(stormMod);
    }

    public void UnloadCustomMods()
    {
        _stormCustomMods.Clear();
    }

    public IEnumerable<StormMapDependency> GetMapDependencies()
    {
        return _stormMapMods.Select(x => new StormMapDependency() { Name = x.Name, DirectoryPath = x.DirectoryPath });
    }

    public bool IsMapModLoaded()
    {
        return _stormMapMods.Count > 0;
    }

    public string? LoadedStormMapTitle()
    {
        if (_stormMapMods.Count > 0)
            return _stormMapMods[^1].Name;
        else
            return null;
    }

    public abstract bool FileExists(string? path, string? mpqPath = null);

    public abstract bool FileExists(StormFile stormFile);

    public abstract Stream GetFile(string path, string? mpqPath = null);

    public abstract Stream GetFile(StormFile stormFile);

    protected abstract IStormMod GetStormMod(string directoryPath, StormModType stormModType, IProgressReporter? progressReporter = null);

    protected abstract IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType);

    protected abstract IDepotCache GetDepotCache();

    protected string GetValidatedPath(string path)
    {
        if (!path.StartsWith(DefaultModsDirectory))
            path = Path.Join(ModsBaseDirectoryPath, path);

        return path;
    }

    protected bool IsMpqFileEntryExists(string mpqPath, string filePath)
    {
        if (!FileExists(mpqPath))
            return false;

        using MpqHeroesArchive mpqFile = MpqHeroesFile.Open(GetFile(mpqPath));
        return mpqFile.FileEntryExists(filePath);
    }

    protected Stream GetMpqFileEntry(string mpqPath, string filePath)
    {
        if (!FileExists(mpqPath))
            throw new FileNotFoundException($"Could not find mpq file", mpqPath);

        using MpqHeroesArchive mpqFile = MpqHeroesFile.Open(GetFile(mpqPath));
        if (mpqFile.TryGetEntry(filePath, out MpqHeroesArchiveEntry? mpqHeroesArchiveEntry))
        {
            return mpqFile.DecompressEntry(mpqHeroesArchiveEntry.Value);
        }

        throw new FileNotFoundException($"Could not find file in mpq (mpq path: {mpqPath}", filePath);
    }

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(CoreStormModDirectory, StormModType.Normal));
        _stormMods.Add(GetStormMod(HeroesStormModDirectory, StormModType.Normal));
        _stormMods.Add(GetStormMod(HeroesDataStormModDirectory, StormModType.Normal));
    }
}
