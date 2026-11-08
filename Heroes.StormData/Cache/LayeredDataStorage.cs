using System.Runtime.InteropServices;

namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a storage container for different types of data storage, including base, map, and custom data storage.
/// <para>The <c>TryGetFirst</c> methods get a reference to the first data storage.</para>
/// <para>The <c>Get</c> merges from all the data storages (if it can).</para>
/// </summary>
internal sealed class LayeredDataStorage
{
    private readonly DataStorage _baseDataStorage;
    private readonly DataStorage _mapDataStorage;
    private readonly DataStorage _customDataStorage;

    private readonly ElementCache _baseElementCache;
    private readonly ElementCache _mapElementCache;
    private readonly ElementCache _customElementCache;

    private readonly MergedStormXmlElementCache _mergedStormXmlElementCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredDataStorage"/> class.
    /// </summary>
    public LayeredDataStorage()
    {
        _baseDataStorage = new DataStorage();
        _mapDataStorage = new DataStorage();
        _customDataStorage = new DataStorage();

        _baseElementCache = _baseDataStorage.ElementCache;
        _mapElementCache = _mapDataStorage.ElementCache;
        _customElementCache = _customDataStorage.ElementCache;

        _mergedStormXmlElementCache = new MergedStormXmlElementCache(new StormXmlElementMerger(this));
    }

    /// <summary>
    /// Gets the appropriate <see cref="DataStorage"/> instance based on the specified <see cref="StormModType"/>.
    /// </summary>
    /// <param name="stormModType">The type of the <c>stormmod</c>.</param>
    /// <returns>The corresponding <see cref="DataStorage"/> instance.</returns>
    public DataStorage GetDataStorage(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseDataStorage,
        StormModType.Map => _mapDataStorage,
        StormModType.Custom => _customDataStorage,
        _ => _customDataStorage,
    };

    /// <summary>
    /// Tries to retrieve the first <c>const</c> XML element by its <c>id</c> from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <c>const</c> XML element to retrieve. This is case-sensitive.</param>
    /// <param name="stormXmlConstElement">When this method returns, contains the <c>const</c> XML element associated with the specified <c>id</c>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <c>const</c> XML element was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstStormXmlConstElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXmlConstElement? stormXmlConstElement)
    {
        // custom first
        if (_customElementCache.StormXmlConstElementByIdAltLookup.TryGetValue(id, out stormXmlConstElement))
            return true;

        if (_mapElementCache.StormXmlConstElementByIdAltLookup.TryGetValue(id, out stormXmlConstElement))
            return true;

        if (_baseElementCache.StormXmlConstElementByIdAltLookup.TryGetValue(id, out stormXmlConstElement))
            return true;

        return false;
    }

    /// <summary>
    /// Tries to retrieve the first catalog name associated with a given class element name from the data storages.
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <param name="catalogName">When this method returns, contains the catalog name (e.g. <c>Effect</c>) associated with the specified class element name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the catalog name was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstCatalogNameByElementName(ReadOnlySpan<char> elementName, [NotNullWhen(true)] out string? catalogName)
    {
        // custom first
        if (_customElementCache.CatalogByClassElementAltLookup.TryGetValue(elementName, out catalogName))
            return true;

        if (_mapElementCache.CatalogByClassElementAltLookup.TryGetValue(elementName, out catalogName))
            return true;

        if (_baseElementCache.CatalogByClassElementAltLookup.TryGetValue(elementName, out catalogName))
            return true;

        return false;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlElement"/> by its class element name (without type letter declaration) from the data storages.
    /// <para>
    /// The returned element can be the stored instance, so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementByClassElement(ReadOnlySpan<char> elementName)
    {
        return GetStormXmlElementByClassElement(elementName, out _);
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlElement"/> by its class element name (without type letter declaration) from the data storages.
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <param name="isCopy">
    /// When this method returns, <see langword="true"/> if the returned element is a new copy (more than one data storage had it, so they were merged), which the caller can modify;
    /// <see langword="false"/> if it is the stored instance, which must not be modified.
    /// </param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementByClassElement(ReadOnlySpan<char> elementName, out bool isCopy)
    {
        StormXmlElement? stormXmlElement = null;
        isCopy = false;

        // base first
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_baseElementCache, elementName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_mapElementCache, elementName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_customElementCache, elementName));

        return stormXmlElement;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlElement"/> by its <c>id</c> from the specified catalog name from the data storages.
    /// <para>
    /// The returned element can be the stored instance, so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>. This is case-insensitive.</param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        return GetStormXmlElementById(id, catalogName, out _);
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlElement"/> by its <c>id</c> from the specified catalog name from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>. This is case-insensitive.</param>
    /// <param name="isCopy">
    /// When this method returns, <see langword="true"/> if the returned element is a new copy (more than one data storage had it, so they were merged), which the caller can modify;
    /// <see langword="false"/> if it is the stored instance, which must not be modified.
    /// </param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName, out bool isCopy)
    {
        StormXmlElement? stormXmlElement = null;
        isCopy = false;

        // base first
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_baseElementCache, id, catalogName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_mapElementCache, id, catalogName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_customElementCache, id, catalogName));

        return stormXmlElement;
    }

    /// <summary>
    /// Retrieves a merged <see cref="StormXmlElement"/> by its <c>id</c> from the specified catalog name.
    /// The element is merged from the data storages, its parents, and its element with no <c>id</c> (see <see cref="StormXmlElementMerger"/>).
    /// <para>
    /// The merged element is cached and the same instance is returned on later calls, so it must not be modified.
    /// The cache is cleared when a base class catalog name or an element is added to any of the data storages.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>. This is case-insensitive.</param>
    /// <returns>The merged <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetMergedStormXmlElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        return _mergedStormXmlElementCache.GetOrMerge(id, catalogName, GetElementsVersion());
    }

    /// <summary>
    /// Retrieves the <c>LevelScalingArray</c> modification <c>Field</c> and <c>Value</c> list by its <c>id</c> (the <c>Entry</c>) from the specified catalog name, merged from the data storages.
    /// <para>
    /// The lists are merged by <c>Field</c> (case-sensitive): a higher data storage replaces the <c>Value</c> of a matching <c>Field</c> and adds any new <c>Field</c>.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> (the <c>Entry</c> value) of the modification. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name (the <c>Catalog</c> value) of the modification. This is case-insensitive.</param>
    /// <returns>The read-only list of <see cref="LevelScalingFieldValue"/> if found; otherwise, <see langword="null"/>.</returns>
    public IReadOnlyList<LevelScalingFieldValue>? GetScalingFieldValuesById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        List<LevelScalingFieldValue>? scalingFieldValues = null;
        bool isCopy = false;

        // base first
        MergeScalingFieldValuesInto(ref scalingFieldValues, ref isCopy, FindScalingFieldValuesById(_baseElementCache, id, catalogName));
        MergeScalingFieldValuesInto(ref scalingFieldValues, ref isCopy, FindScalingFieldValuesById(_mapElementCache, id, catalogName));
        MergeScalingFieldValuesInto(ref scalingFieldValues, ref isCopy, FindScalingFieldValuesById(_customElementCache, id, catalogName));

        return scalingFieldValues;
    }

    /// <summary>
    /// Clears the cached merged elements from <see cref="GetMergedStormXmlElementById(ReadOnlySpan{char}, ReadOnlySpan{char})"/>.
    /// </summary>
    public void ClearMergedStormXmlElementCache()
    {
        _mergedStormXmlElementCache.Clear();
    }

    private static StormXmlElement? FindStormXmlElementByElementName(ElementCache elementCache, ReadOnlySpan<char> elementName)
    {
        if (elementCache.StormXmlElementByClassElementAltLookup.TryGetValue(elementName, out StormXmlElement? stormXmlElement))
            return stormXmlElement;

        return null;
    }

    private static StormXmlElement? FindStormXmlElementById(ElementCache elementCache, ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (elementCache.StormXmlElementsByCatalogAltLookup.TryGetValue(catalogName, out var stormXmlElementById) &&
            stormXmlElementById.TryGetValue(id, out StormXmlElement? stormXmlElement))
        {
            return stormXmlElement;
        }

        return null;
    }

    private static List<LevelScalingFieldValue>? FindScalingFieldValuesById(ElementCache elementCache, ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (elementCache.ScalingFieldValueByIdByCatalogAltLookup.TryGetValue(catalogName, out var scalingFieldValueById) &&
            scalingFieldValueById.TryGetValue(id, out List<LevelScalingFieldValue>? scalingFieldValues))
        {
            return scalingFieldValues;
        }

        return null;
    }

    private static void MergeInto(ref StormXmlElement? stormXmlElement, ref bool isCopy, StormXmlElement? layerElement)
    {
        if (layerElement is null)
            return;

        // first layer found: keep the cached instance for now, it is only copied if another layer has to be merged into it
        if (stormXmlElement is null)
        {
            stormXmlElement = layerElement;
            return;
        }

        if (!isCopy)
        {
            // need to merge, so clone the first layer to avoid modifying the cached instance
            stormXmlElement = stormXmlElement.Clone();
            isCopy = true;
        }

        stormXmlElement.Merge(layerElement);
    }

    private static void MergeScalingFieldValuesInto(ref List<LevelScalingFieldValue>? scalingFieldValues, ref bool isCopy, List<LevelScalingFieldValue>? layerScalingFieldValues)
    {
        if (layerScalingFieldValues is null)
            return;

        // first layer found: keep the cached list for now, it is only copied if another layer has to be merged into it
        if (scalingFieldValues is null)
        {
            scalingFieldValues = layerScalingFieldValues;
            return;
        }

        if (!isCopy)
        {
            // need to merge, so copy the first layer to avoid modifying the cached list
            scalingFieldValues = [.. scalingFieldValues];
            isCopy = true;
        }

        foreach (LevelScalingFieldValue layerScalingFieldValue in CollectionsMarshal.AsSpan(layerScalingFieldValues))
        {
            DataStorage.AddOrReplaceScalingFieldValue(scalingFieldValues, layerScalingFieldValue);
        }
    }

    // each data storage's version only goes up, so the sum changes whenever any of them does
    private int GetElementsVersion() => _baseDataStorage.ElementsVersion + _mapDataStorage.ElementsVersion + _customDataStorage.ElementsVersion;
}
