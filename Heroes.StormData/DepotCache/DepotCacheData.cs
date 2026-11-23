namespace Heroes.StormData.DepotCache;

/// <summary>
/// Holds the map data found in the <c>depotcache</c> directory.
/// </summary>
internal sealed class DepotCacheData
{
    /// <summary>
    /// Gets the <see cref="S2MVProperties"/> by their dependency key (<see cref="S2MapPropertiesBase.GetDependencyKey"/>). The key is used to match a <see cref="S2MAProperties"/> to its <see cref="S2MVProperties"/>.
    /// <para>
    /// If more than one <c>.s2mv</c> file has the same key, the first one added is kept.
    /// </para>
    /// </summary>
    public Dictionary<string, S2MVProperties> S2MVPropertiesByDependencyKey { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the file paths of the loaded <c>.s2mv</c> files.
    /// </summary>
    public List<string> S2MVFilePaths { get; } = [];

    /// <summary>
    /// Gets all the loaded <see cref="S2MAProperties"/>.
    /// </summary>
    public List<S2MAProperties> S2MAProperties { get; } = [];

    /// <summary>
    /// Gets the <see cref="S2MAProperties"/> that have a matching <see cref="S2MVProperties"/>, by the map title.
    /// <para>
    /// The key is the map title (<c>enUS</c>) and is case-insensitive.
    /// If more than one map has the same title, the first one added is kept. Maps with an empty title are not added.
    /// </para>
    /// </summary>
    public Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the file paths of the loaded <c>.s2ma</c> files.
    /// </summary>
    public List<string> S2MAFilePaths { get; } = [];

    /// <summary>
    /// Adds a loaded <c>.s2mv</c> file.
    /// </summary>
    /// <param name="filePath">The path of the <c>.s2mv</c> file.</param>
    /// <param name="s2mvProperties">The properties loaded from the file.</param>
    public void AddS2MV(string filePath, S2MVProperties s2mvProperties)
    {
        // a duplicate would otherwise fail the whole depot cache load
        S2MVPropertiesByDependencyKey.TryAdd(s2mvProperties.GetDependencyKey(), s2mvProperties);
        S2MVFilePaths.Add(filePath);
    }

    /// <summary>
    /// Adds a loaded <c>.s2ma</c> file and links it to its <see cref="S2MVProperties"/>, if one was found.
    /// All the <c>.s2mv</c> files should be added before this is called.
    /// </summary>
    /// <param name="filePath">The path of the <c>.s2ma</c> file.</param>
    /// <param name="s2maProperties">The properties loaded from the file.</param>
    public void AddS2MA(string filePath, S2MAProperties s2maProperties)
    {
        if (S2MVPropertiesByDependencyKey.TryGetValue(s2maProperties.GetDependencyKey(), out S2MVProperties? s2mvProperties))
            s2maProperties.S2MVProperties = s2mvProperties;

        S2MAProperties.Add(s2maProperties);
        S2MAFilePaths.Add(filePath);

        // an empty title can't be looked up, and a duplicate would otherwise fail the whole depot cache load
        if (s2maProperties.S2MVProperties is not null && !string.IsNullOrWhiteSpace(s2maProperties.S2MVProperties.HeaderTitle))
            S2MAPropertiesByTitle.TryAdd(s2maProperties.S2MVProperties.HeaderTitle, s2maProperties);
    }

    /// <summary>
    /// Removes all the data.
    /// </summary>
    public void Clear()
    {
        S2MVPropertiesByDependencyKey.Clear();
        S2MVFilePaths.Clear();
        S2MAProperties.Clear();
        S2MAPropertiesByTitle.Clear();
        S2MAFilePaths.Clear();
    }
}
