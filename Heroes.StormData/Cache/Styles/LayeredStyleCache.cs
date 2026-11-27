namespace Heroes.StormData.Cache.Styles;

/// <summary>
/// Looks up <c>.stormstyle</c> data across the base, map, and custom <see cref="StyleCache"/> layers.
/// <para>The <c>Get</c> methods merge from all the layers (base, then map, then custom).</para>
/// </summary>
internal sealed class LayeredStyleCache
{
    private readonly StyleCache _baseStyleCache;
    private readonly StyleCache _mapStyleCache;
    private readonly StyleCache _customStyleCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredStyleCache"/> class.
    /// </summary>
    /// <param name="baseStyleCache">The <see cref="StyleCache"/> of the base data storage.</param>
    /// <param name="mapStyleCache">The <see cref="StyleCache"/> of the map data storage.</param>
    /// <param name="customStyleCache">The <see cref="StyleCache"/> of the custom data storage.</param>
    public LayeredStyleCache(StyleCache baseStyleCache, StyleCache mapStyleCache, StyleCache customStyleCache)
    {
        _baseStyleCache = baseStyleCache;
        _mapStyleCache = mapStyleCache;
        _customStyleCache = customStyleCache;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlStyleConstantElement"/> (from <c>.stormstyle</c> files) by its <c>name</c> from the data storages.
    /// <para>
    /// The returned element can be the stored instance, so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant. This is case-sensitive.</param>
    /// <returns>The <see cref="StormXmlStyleConstantElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlStyleConstantElement? GetStormXmlStyleConstantElementByName(ReadOnlySpan<char> name)
    {
        return GetStormXmlStyleConstantElementByName(name, out _);
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlStyleConstantElement"/> (from <c>.stormstyle</c> files) by its <c>name</c> from the data storages.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant. This is case-sensitive.</param>
    /// <param name="isCopy">
    /// When this method returns, <see langword="true"/> if the returned element is a new copy (more than one data storage had it, so they were merged), which the caller can modify;
    /// <see langword="false"/> if it is the stored instance, which must not be modified.
    /// </param>
    /// <returns>The <see cref="StormXmlStyleConstantElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlStyleConstantElement? GetStormXmlStyleConstantElementByName(ReadOnlySpan<char> name, out bool isCopy)
    {
        StormXmlStyleConstantElement? stormXmlStyleConstantElement = null;
        isCopy = false;

        // base first
        LayerMerger.MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_baseStyleCache, name));
        LayerMerger.MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_mapStyleCache, name));
        LayerMerger.MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_customStyleCache, name));

        return stormXmlStyleConstantElement;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlStyleElement"/> (from <c>.stormstyle</c> files) by its <c>name</c> from the data storages.
    /// <para>
    /// The returned element can be the stored instance, so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style. This is case-sensitive.</param>
    /// <returns>The <see cref="StormXmlStyleElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlStyleElement? GetStormXmlStyleElementByName(ReadOnlySpan<char> name)
    {
        return GetStormXmlStyleElementByName(name, out _);
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlStyleElement"/> (from <c>.stormstyle</c> files) by its <c>name</c> from the data storages.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style. This is case-sensitive.</param>
    /// <param name="isCopy">
    /// When this method returns, <see langword="true"/> if the returned element is a new copy (more than one data storage had it, so they were merged), which the caller can modify;
    /// <see langword="false"/> if it is the stored instance, which must not be modified.
    /// </param>
    /// <returns>The <see cref="StormXmlStyleElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlStyleElement? GetStormXmlStyleElementByName(ReadOnlySpan<char> name, out bool isCopy)
    {
        StormXmlStyleElement? stormXmlStyleElement = null;
        isCopy = false;

        // base first
        LayerMerger.MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_baseStyleCache, name));
        LayerMerger.MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_mapStyleCache, name));
        LayerMerger.MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_customStyleCache, name));

        return stormXmlStyleElement;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlStyleElement"/> (from <c>.stormstyle</c> files) by its <c>name</c>, merged from the data storages and then over its
    /// <c>template</c> chain (its template, that template's template, and so on), so the attributes it inherits are included.
    /// <para>
    /// The style's own attributes override its template's, and the result keeps the style's own <c>name</c> and <c>template</c>.
    /// A template that is not found, or that is already in the chain (a cycle), ends the chain.
    /// The returned element can be the stored instance (when there is nothing to merge), so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style. This is case-sensitive.</param>
    /// <returns>The merged <see cref="StormXmlStyleElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlStyleElement? GetMergedStormXmlStyleElementByName(ReadOnlySpan<char> name)
    {
        StormXmlStyleElement? stormXmlStyleElement = GetStormXmlStyleElementByName(name);
        if (stormXmlStyleElement is null || !stormXmlStyleElement.HasTemplate)
            return stormXmlStyleElement;

        // the style, then its template, then that template's template, and so on
        List<StormXmlStyleElement> chain = [stormXmlStyleElement];
        HashSet<string> chainNames = new(StringComparer.Ordinal) { stormXmlStyleElement.Name };

        string? template = stormXmlStyleElement.Template;

        while (template is not null && chainNames.Add(template))
        {
            StormXmlStyleElement? templateElement = GetStormXmlStyleElementByName(template);
            if (templateElement is null)
                break;

            chain.Add(templateElement);
            template = templateElement.Template;
        }

        if (chain.Count == 1)
            return stormXmlStyleElement;

        // merged from the top of the chain down, so each style's attributes override its template's.
        // the top is cloned, so the stored elements are never modified
        StormXmlStyleElement merged = (StormXmlStyleElement)chain[^1].Clone();

        for (int i = chain.Count - 2; i >= 0; i--)
            merged.MergeChild(chain[i]);

        return merged;
    }

    /// <summary>
    /// Determines if any of the data storages has a <see cref="StormXmlStyleConstantElement"/> with the <c>name</c>. Nothing is merged.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant. This is case-sensitive.</param>
    /// <returns><see langword="true"/> if a data storage has the style constant; otherwise, <see langword="false"/>.</returns>
    public bool ContainsStormXmlStyleConstantElementByName(ReadOnlySpan<char> name)
    {
        return FindStormXmlStyleConstantElementByName(_customStyleCache, name) is not null ||
            FindStormXmlStyleConstantElementByName(_mapStyleCache, name) is not null ||
            FindStormXmlStyleConstantElementByName(_baseStyleCache, name) is not null;
    }

    /// <summary>
    /// Determines if any of the data storages has a <see cref="StormXmlStyleElement"/> with the <c>name</c>. Nothing is merged.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style. This is case-sensitive.</param>
    /// <returns><see langword="true"/> if a data storage has the style; otherwise, <see langword="false"/>.</returns>
    public bool ContainsStormXmlStyleElementByName(ReadOnlySpan<char> name)
    {
        return FindStormXmlStyleElementByName(_customStyleCache, name) is not null ||
            FindStormXmlStyleElementByName(_mapStyleCache, name) is not null ||
            FindStormXmlStyleElementByName(_baseStyleCache, name) is not null;
    }

    private static StormXmlStyleConstantElement? FindStormXmlStyleConstantElementByName(StyleCache styleCache, ReadOnlySpan<char> name)
    {
        if (styleCache.StormXmlStyleConstantElementByName.TryGetValue(name, out StormXmlStyleConstantElement? stormXmlStyleConstantElement))
            return stormXmlStyleConstantElement;

        return null;
    }

    private static StormXmlStyleElement? FindStormXmlStyleElementByName(StyleCache styleCache, ReadOnlySpan<char> name)
    {
        if (styleCache.StormXmlStyleElementByName.TryGetValue(name, out StormXmlStyleElement? stormXmlStyleElement))
            return stormXmlStyleElement;

        return null;
    }
}
