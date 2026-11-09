namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Caches merged <see cref="StormXmlElement"/>s by catalog name and <c>id</c>.
/// </summary>
internal sealed class MergedStormXmlElementCache
{
    private readonly StormXmlElementMerger _stormXmlElementMerger;

    // merged elements by catalog name, then by id (null when the element doesn't exist)
    private readonly Dictionary<string, Dictionary<string, StormXmlElement?>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> _mergedStormXmlElementsByIdByCatalog =
        new Dictionary<string, Dictionary<string, StormXmlElement?>.AlternateLookup<ReadOnlySpan<char>>>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    // the elements version the merged elements were cached at
    private int _elementsVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergedStormXmlElementCache"/> class.
    /// </summary>
    /// <param name="stormXmlElementMerger">The merger that creates the elements that aren't cached yet.</param>
    public MergedStormXmlElementCache(StormXmlElementMerger stormXmlElementMerger)
    {
        _stormXmlElementMerger = stormXmlElementMerger;
    }

    /// <summary>
    /// Gets the merged <see cref="StormXmlElement"/> by its <c>id</c> from the specified catalog name, merging and caching it if it isn't cached yet.
    /// <para>
    /// The same instance is returned on later calls, so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>. This is case-insensitive.</param>
    /// <param name="elementsVersion">
    /// The current elements version of the data. If it differs from the version the elements were cached at, the cache is cleared first, since they may be out of date.
    /// </param>
    /// <returns>The merged <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetOrMerge(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName, int elementsVersion)
    {
        if (elementsVersion != _elementsVersion)
        {
            Clear();
            _elementsVersion = elementsVersion;
        }

        if (!_mergedStormXmlElementsByIdByCatalog.TryGetValue(catalogName, out var mergedStormXmlElementsById))
        {
            // case-sensitive for ids
            mergedStormXmlElementsById = new Dictionary<string, StormXmlElement?>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();

            _mergedStormXmlElementsByIdByCatalog[catalogName] = mergedStormXmlElementsById;
        }

        if (!mergedStormXmlElementsById.TryGetValue(id, out StormXmlElement? mergedStormXmlElement))
        {
            mergedStormXmlElement = _stormXmlElementMerger.GetMerged(id, catalogName);
            mergedStormXmlElementsById[id] = mergedStormXmlElement;
        }

        return mergedStormXmlElement;
    }

    /// <summary>
    /// Clears the cached merged elements.
    /// </summary>
    public void Clear()
    {
        _mergedStormXmlElementsByIdByCatalog.Dictionary.Clear();
    }
}
