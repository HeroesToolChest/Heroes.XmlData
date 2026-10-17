namespace Heroes.StormData.Cache;

internal class StormElementMerger
{
    private readonly LayeredDataStorage _layeredDataStorage;

    public StormElementMerger(LayeredDataStorage layeredDataStorage)
    {
        _layeredDataStorage = layeredDataStorage;
    }

    /// <summary>
    /// Retrieves a merged <see cref="StormXmlElement"/> for the specified identifier from the given catalog name.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>.</param>
    /// <returns>The merged <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetMerged(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        return Merge(id, catalogName);
    }

    private static StormXmlElement MergeIntoParent(StormXmlElement? parentElement, StormXmlElement stormXmlElement)
    {
        if (parentElement is null)
            return stormXmlElement;

        parentElement.Merge(stormXmlElement);

        return parentElement;
    }

    // recursively travels up the StormXmlElement's parents, then merges back down so child values override parent values
    private StormXmlElement? Merge(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (id.IsEmpty)
            return null;

        StormXmlElement? stormXmlElement = _layeredDataStorage.GetStormXmlElementById(id, catalogName);

        if (stormXmlElement is null)
            return null;

        // id and parent check is to prevent infinite recursion
        if (stormXmlElement.HasParent && !id.Equals(stormXmlElement.Parent, StringComparison.Ordinal))
            return MergeIntoParent(Merge(stormXmlElement.Parent, catalogName), stormXmlElement);

        // top of the parent chain, so continue with the element-of-no-id
        return MergeIntoParent(MergeUpElementOfNoId(stormXmlElement.ElementName), stormXmlElement);
    }

    // elements with no id attribute
    private StormXmlElement? MergeUpElementOfNoId(ReadOnlySpan<char> elementName)
    {
        StormXmlElement? stormXmlElement = _layeredDataStorage.GetStormXmlElementByClassElement(elementName);

        if (stormXmlElement is null)
            return null;

        // the base element may not be the correct one, but close enough
        StormXmlElement? baseElement = GetBaseStormElement(stormXmlElement.ElementName);

        if (baseElement is null)
            return stormXmlElement;

        // the element-of-no-id can be the base element itself, in which case there is nothing to merge
        if (!baseElement.ElementName.Equals(stormXmlElement.ElementName, StringComparison.OrdinalIgnoreCase))
            baseElement.Merge(stormXmlElement);

        return baseElement;
    }

    private StormXmlElement? GetBaseStormElement(ReadOnlySpan<char> elementName)
    {
        if (_layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName))
        {
            return _layeredDataStorage.GetStormXmlElementByClassElement(catalogName);
        }

        return null;
    }
}
