namespace Heroes.StormData.DepotCache;

internal interface IDepotCache
{
    /// <summary>
    /// Gets the map data found in the <c>depotcache</c> directory. This is empty until <see cref="LoadDepotCache"/> is called.
    /// </summary>
    DepotCacheData Data { get; }

    /// <summary>
    /// Finds and loads the <c>.s2mv</c> and <c>.s2ma</c> files in the <c>depotcache</c> directory.
    /// </summary>
    void LoadDepotCache();
}
