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
    /// Retrieves the merged element with no <c>id</c> (the default element) of a class, merged from the data storages and over the default elements
    /// of the classes it derives from: its catalog's base class (e.g. <c>CValidator</c>), then each class in between (e.g. <c>CValidatorUnit</c> for <c>CValidatorUnitAbil</c>).
    /// This is the same element that <see cref="GetMerged(ReadOnlySpan{char}, ReadOnlySpan{char})"/> merges an element at the top of its parent chain into.
    /// <para>
    /// A class derives from each class of the same catalog whose name is the start of its name, up to a capital letter (e.g. <c>CValidatorUnitOrderQueue</c>
    /// derives from <c>CValidator</c>, <c>CValidatorUnit</c>, and <c>CValidatorUnitOrder</c>). The more derived class's values override the less derived class's.
    /// </para>
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

    // elements with no id attribute, merged over the default elements of the classes they derive from
    private StormXmlElement? MergeUpElementOfNoId(ReadOnlySpan<char> elementName, out bool isCopy)
    {
        isCopy = false;

        StormXmlElement? stormXmlElement = _layeredElementCache.GetStormXmlElementByClassElement(elementName, out bool isElementCopy);

        if (stormXmlElement is null)
            return null;

        if (!_layeredElementCache.TryGetFirstCatalogNameByElementName(stormXmlElement.ElementName, out string? catalogName))
        {
            isCopy = isElementCopy;
            return stormXmlElement;
        }

        StormXmlElement? merged = null;
        bool isMergedCopy = false;

        // the catalog's base class first (e.g. Validator), which is not always the start of the element name (e.g. RequirementNode for RequirementEq)
        if (!catalogName.Equals(stormXmlElement.ElementName, StringComparison.OrdinalIgnoreCase))
            MergeClassDefaultInto(ref merged, ref isMergedCopy, catalogName);

        // then each class in between, from the least to the most derived (e.g. ValidatorUnit, then ValidatorUnitOrder for ValidatorUnitOrderQueue).
        // a class is only a base of the element if it is in the same catalog (e.g. Achievement is a different catalog than AchievementTerm)
        string fullElementName = stormXmlElement.ElementName;

        for (int i = 1; i < fullElementName.Length; i++)
        {
            if (!char.IsUpper(fullElementName[i]))
                continue;

            ReadOnlySpan<char> className = fullElementName.AsSpan(0, i);

            if (className.Length <= catalogName.Length && className.Equals(catalogName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (_layeredElementCache.TryGetFirstCatalogNameByElementName(className, out string? classCatalogName) &&
                classCatalogName.Equals(catalogName, StringComparison.OrdinalIgnoreCase))
            {
                MergeClassDefaultInto(ref merged, ref isMergedCopy, className);
            }
        }

        return MergeIntoParent(merged, isMergedCopy, stormXmlElement, isElementCopy, out isCopy);
    }

    // merges a class's default element (if it has one) into the default elements merged so far, as their child
    private void MergeClassDefaultInto(ref StormXmlElement? merged, ref bool isMergedCopy, ReadOnlySpan<char> className)
    {
        StormXmlElement? classDefault = _layeredElementCache.GetStormXmlElementByClassElement(className, out bool isClassDefaultCopy);
        if (classDefault is null)
            return;

        if (merged is null)
        {
            merged = classDefault;
            isMergedCopy = isClassDefaultCopy;
            return;
        }

        merged = MergeIntoParent(merged, isMergedCopy, classDefault, isClassDefaultCopy, out isMergedCopy);
    }
}
