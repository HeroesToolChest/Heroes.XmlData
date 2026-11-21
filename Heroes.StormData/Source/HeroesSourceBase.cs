namespace Heroes.StormData.Source;

internal abstract class HeroesSourceBase : IHeroesSource
{
    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    private IDepotCache? _depotCache;

    public HeroesSourceBase(string modsDirectoryPath, IStormModFactory stormModFactory)
    {
        ModsDirectoryPath = modsDirectoryPath;
        StormModFactory = stormModFactory;

        AddStormMods();
    }

    public string ModsDirectoryPath { get; }

    public SourceContainer SourceContainer { get; } = new();

    public IStormModFactory StormModFactory { get; }

    public void LoadStormMods()
    {
        foreach (IStormMod stormMod in _stormMods)
        {
            SourceContainer.AddModContainer(stormMod.ModContainer);

            stormMod.LoadStormFiles();
        }
    }

    public void LoadDepotCache()
    {
        _depotCache ??= CreateDepotCache();
        _depotCache.LoadDepotCache();
    }

    public void LoadGamestrings(StormLocale stormLocale)
    {
        SourceContainer.ClearGamestrings();

        foreach (IStormMod stormMod in _stormMods)
            stormMod.LoadStormGameStrings(stormLocale);

        foreach (IStormMod stormMod in _stormMapMods)
            stormMod.LoadStormGameStrings(stormLocale);

        foreach (IStormMod stormMod in _stormCustomMods)
            stormMod.LoadStormGameStrings(stormLocale);
    }

    public void LoadStormMapMod(string mapTitle)
    {
        if (_depotCache is null)
            return;

        _stormMapMods.Clear();
        SourceContainer.ClearStormMapMods();

        if (!_depotCache.Data.S2MAPropertiesByTitle.TryGetValue(mapTitle, out S2MAProperties? s2maProperties))
            return;

        IStormMod mapRootMod = GetMpqStormMod(mapTitle, Path.Join(ModsDirectoryPath, s2maProperties.DirectoryPath), StormModType.Map);

        StormMapModResolver stormMapModResolver = new(localFile => GetStormMod(Path.Join(ModsDirectoryPath, localFile), StormModType.Map));

        _stormMapMods.AddRange(stormMapModResolver.Resolve(mapRootMod, s2maProperties.MapDependencies));

        // the resolver returns them in load order, dependencies first and the map root last
        foreach (IStormMod stormMod in _stormMapMods)
        {
            SourceContainer.AddModContainer(stormMod.ModContainer);

            stormMod.LoadStormFiles();
        }
    }

    /// <summary>
    /// Gets the <see cref="IStormMod"/> for the specified directory path.
    /// </summary>
    /// <param name="directoryPath">The full path to the directory of the <c>stormmod</c>.</param>
    /// <param name="stormModType">The type of the <c>stormmod</c>.</param>
    /// <returns>The <see cref="IStormMod"/> instance.</returns>
    protected abstract IStormMod GetStormMod(string directoryPath, StormModType stormModType);

    protected abstract IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType);

    /// <summary>
    /// Creates the <see cref="IDepotCache"/> for this source.
    /// </summary>
    /// <returns>The <see cref="IDepotCache"/> instance.</returns>
    protected abstract IDepotCache CreateDepotCache();

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.CoreStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesDataStormModDirectory), StormModType.Base));
    }
}
