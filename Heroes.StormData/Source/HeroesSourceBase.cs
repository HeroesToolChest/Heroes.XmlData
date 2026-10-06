namespace Heroes.StormData.Source;

internal abstract class HeroesSourceBase : IHeroesSource
{
    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    public HeroesSourceBase(string modsDirectoryPath, IStormModFactory stormModFactory)
    {
        ModsDirectoryPath = modsDirectoryPath;
        StormModFactory = stormModFactory;

        AddStormMods();
    }

    public string ModsDirectoryPath { get; }

    public SourceContainer SourceContainer { get; } = new();

    protected IStormModFactory StormModFactory { get; }

    public void LoadStormMods()
    {
        foreach (IStormMod stormMod in _stormMods)
        {

            // StormStorage.AddModStorage(stormMod.StormModStorage);

            stormMod.LoadStormFiles();
        }
    }

    public void LoadGamestrings(StormLocale stormLocale)
    {

    }

    /// <summary>
    /// Gets the <see cref="IStormMod"/> for the specified directory path.
    /// </summary>
    /// <param name="directoryPath">The full path to the directory of the <c>stormmod</c>.</param>
    /// <param name="stormModType">The type of the <c>stormmod</c>.</param>
    /// <returns>The <see cref="IStormMod"/> instance.</returns>
    protected abstract IStormMod GetStormMod(string directoryPath, StormModType stormModType);

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.CoreStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesDataStormModDirectory), StormModType.Base));
    }
}
