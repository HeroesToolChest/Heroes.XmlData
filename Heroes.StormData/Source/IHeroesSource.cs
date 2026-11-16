namespace Heroes.StormData.Source;

internal interface IHeroesSource
{
    /// <summary>
    /// Gets the full path to the <c>mods</c> directory.
    /// </summary>
    string ModsDirectoryPath { get; }

    /// <summary>
    /// Gets the container that holds all data across all the mods.
    /// </summary>
    SourceContainer SourceContainer { get; }

    IStormModFactory StormModFactory { get; }

    /// <summary>
    /// Gets the depot cache, which holds the map data found in the <c>depotcache</c> directory.
    /// </summary>
    IDepotCache DepotCache { get; }

    void LoadStormMods();
}
