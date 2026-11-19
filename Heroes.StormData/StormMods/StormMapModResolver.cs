namespace Heroes.StormData.StormMods;

/// <summary>
/// Resolves the ordered list of <c>stormmods</c> that make up a map, by walking the map's dependencies and the dependencies listed in each dependency's <c>documentinfo</c> file.
/// </summary>
internal sealed class StormMapModResolver
{
    private readonly Func<string, IStormMod> _createMapStormMod;

    /// <summary>
    /// Initializes a new instance of the <see cref="StormMapModResolver"/> class.
    /// </summary>
    /// <param name="createMapStormMod">Creates a <see cref="StormModType.Map"/> <c>stormmod</c> from a <see cref="MapDependency.LocalFile"/> path (relative to the <c>mods</c> directory).</param>
    public StormMapModResolver(Func<string, IStormMod> createMapStormMod)
    {
        _createMapStormMod = createMapStormMod;
    }

    /// <summary>
    /// Gets the <c>stormmods</c> of a map in load order. The dependencies come first and the <paramref name="mapRootStormMod"/> is last.
    /// The <c>heroesdata.stormmod</c> is excluded (it is part of the base layer) and only the first occurrence of a duplicated <c>stormmod</c> is kept, so a <c>stormmod</c> is loaded before anything that depends on it.
    /// </summary>
    /// <param name="mapRootStormMod">The map's own <c>stormmod</c>.</param>
    /// <param name="mapDependencies">The dependencies of the map.</param>
    /// <returns>The <c>stormmods</c> in load order.</returns>
    public IEnumerable<IStormMod> Resolve(IStormMod mapRootStormMod, IEnumerable<MapDependency> mapDependencies)
    {
        List<IStormMod> stormMods = CollectDependencies(mapDependencies);
        stormMods.Add(mapRootStormMod); // add the map root last

        return stormMods
            .Where(x => !x.DirectoryPath.EndsWith(SourceConstants.HeroesDataStormModDirectory, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(x => x.DirectoryPath, StringComparer.OrdinalIgnoreCase);
    }

    private List<IStormMod> CollectDependencies(IEnumerable<MapDependency> mapDependencies)
    {
        List<IStormMod> stormMods = [];
        List<IStormMod> currentDependencies = [];

        foreach (MapDependency mapDependency in mapDependencies)
        {
            IStormMod stormMod = _createMapStormMod(mapDependency.LocalFile);
            currentDependencies.Add(stormMod);

            stormMods.AddRange(CollectDependencies(stormMod.GetMapDependencies()));
        }

        currentDependencies.Reverse(); // flip-flop
        stormMods.AddRange(currentDependencies);

        return stormMods;
    }
}
