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
