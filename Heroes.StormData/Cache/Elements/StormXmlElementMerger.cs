namespace Heroes.StormData.Cache.Elements;

internal class StormXmlElementMerger
{
    private readonly LayeredElementCache _layeredElementCache;

    public StormXmlElementMerger(LayeredElementCache layeredElementCache)
    {
        _layeredElementCache = layeredElementCache;
    }

    /// <summary>
    /// Retrieves a merged <see cref="StormXmlElement"/> for the specified identifier from the given catalog name.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>.</param>
    /// <returns>The merged <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetMerged(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        return Merge(id, catalogName, out _);
    }

    /// <summary>
    /// Retrieves the merged element with no <c>id</c> (the default element) of a class, merged from the data storages and over its catalog's base class
    /// default element (e.g. <c>CEffect</c> for <c>CEffectDamage</c>). This is the same element that <see cref="GetMerged(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
    /// merges an element at the top of its parent chain into.
    /// <para>
    /// The returned element can be the stored instance (when there is nothing to merge), so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <returns>The merged default element if the class has one; otherwise, <see langword="null"/>, even if its catalog's base class has one.</returns>
    public StormXmlElement? GetMergedElementOfNoId(ReadOnlySpan<char> elementName)
    {
        return MergeUpElementOfNoId(elementName, out _);
    }

    // the stored elements must never be merged into, so the parent is cloned first unless it is already a copy.
    // with no parent, the element is returned as is, so it keeps its own copy state
    private static StormXmlElement MergeIntoParent(StormXmlElement? parentElement, bool isParentCopy, StormXmlElement stormXmlElement, bool isElementCopy, out bool isCopy)
    {
        if (parentElement is null)
        {
            isCopy = isElementCopy;
            return stormXmlElement;
        }

        if (!isParentCopy)
            parentElement = parentElement.Clone();

        parentElement.MergeChild(stormXmlElement);

        isCopy = true;
        return parentElement;
    }

    // recursively travels up the StormXmlElement's parents, then merges back down so child values override parent values
    private StormXmlElement? Merge(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName, out bool isCopy)
    {
        isCopy = false;

        if (id.IsEmpty)
            return null;

        StormXmlElement? stormXmlElement = _layeredElementCache.GetStormXmlElementById(id, catalogName, out bool isElementCopy);

        if (stormXmlElement is null)
            return null;

        // id and parent check is to prevent infinite recursion
        if (stormXmlElement.HasParent && !id.Equals(stormXmlElement.Parent, StringComparison.Ordinal))
        {
            StormXmlElement? parentElement = Merge(stormXmlElement.Parent, catalogName, out bool isParentCopy);

            return MergeIntoParent(parentElement, isParentCopy, stormXmlElement, isElementCopy, out isCopy);
        }

        // top of the parent chain, so continue with the element-of-no-id
        StormXmlElement? elementOfNoId = MergeUpElementOfNoId(stormXmlElement.ElementName, out bool isElementOfNoIdCopy);

        return MergeIntoParent(elementOfNoId, isElementOfNoIdCopy, stormXmlElement, isElementCopy, out isCopy);
    }

    // elements with no id attribute
    private StormXmlElement? MergeUpElementOfNoId(ReadOnlySpan<char> elementName, out bool isCopy)
    {
        isCopy = false;

        StormXmlElement? stormXmlElement = _layeredElementCache.GetStormXmlElementByClassElement(elementName, out bool isElementCopy);

        if (stormXmlElement is null)
            return null;

        // the base element may not be the correct one, but close enough
        StormXmlElement? baseElement = GetBaseStormElement(stormXmlElement.ElementName, out bool isBaseCopy);

        if (baseElement is null)
        {
            isCopy = isElementCopy;
            return stormXmlElement;
        }

        // the element-of-no-id can be the base element itself, in which case there is nothing to merge
        if (baseElement.ElementName.Equals(stormXmlElement.ElementName, StringComparison.OrdinalIgnoreCase))
        {
            isCopy = isBaseCopy;
            return baseElement;
        }

        return MergeIntoParent(baseElement, isBaseCopy, stormXmlElement, isElementCopy, out isCopy);
    }

    private StormXmlElement? GetBaseStormElement(ReadOnlySpan<char> elementName, out bool isCopy)
    {
        if (_layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName))
        {
            return _layeredElementCache.GetStormXmlElementByClassElement(catalogName, out isCopy);
        }

        isCopy = false;
        return null;
    }
}
