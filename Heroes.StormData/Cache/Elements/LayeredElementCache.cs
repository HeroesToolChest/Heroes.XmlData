using System.Runtime.InteropServices;

namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Looks up element data across the base, map, and custom <see cref="ElementCache"/> layers.
/// <para>The <c>TryGetFirst</c> methods get a reference from the first layer that has it (custom, then map, then base).</para>
/// <para>The <c>Get</c> methods merge from all the layers (base, then map, then custom).</para>
/// </summary>
internal sealed class LayeredElementCache
{
    private readonly ElementCache _baseElementCache;
    private readonly ElementCache _mapElementCache;
    private readonly ElementCache _customElementCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredElementCache"/> class.
    /// </summary>
    /// <param name="baseElementCache">The <see cref="ElementCache"/> of the base data storage.</param>
    /// <param name="mapElementCache">The <see cref="ElementCache"/> of the map data storage.</param>
    /// <param name="customElementCache">The <see cref="ElementCache"/> of the custom data storage.</param>
    public LayeredElementCache(ElementCache baseElementCache, ElementCache mapElementCache, ElementCache customElementCache)
    {
        _baseElementCache = baseElementCache;
        _mapElementCache = mapElementCache;
        _customElementCache = customElementCache;
    }

    /// <summary>
    /// Tries to retrieve the first <c>const</c> XML element by its <c>id</c> from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <c>const</c> XML element to retrieve. This is case-sensitive.</param>
    /// <param name="stormXmlConstElement">When this method returns, contains the <c>const</c> XML element associated with the specified <c>id</c>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <c>const</c> XML element was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstStormXmlConstElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXmlConstElement? stormXmlConstElement)
    {
        // custom first
        if (_customElementCache.StormXmlConstElementById.TryGetValue(id, out stormXmlConstElement))
            return true;

        if (_mapElementCache.StormXmlConstElementById.TryGetValue(id, out stormXmlConstElement))
            return true;

        if (_baseElementCache.StormXmlConstElementById.TryGetValue(id, out stormXmlConstElement))
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
        if (_customElementCache.CatalogByClassElement.TryGetValue(elementName, out catalogName))
            return true;

        if (_mapElementCache.CatalogByClassElement.TryGetValue(elementName, out catalogName))
            return true;

        if (_baseElementCache.CatalogByClassElement.TryGetValue(elementName, out catalogName))
            return true;

        return false;
    }

    /// <summary>
    /// Gets the class element names (without type letter declaration) (e.g. <c>EffectDamage</c>) of a catalog name from all the data storages.
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <returns>
    /// A new case-insensitive set of the class element names from the base, map, and custom data storages, or an empty set if none of them have the catalog name.
    /// </returns>
    public HashSet<string> GetClassElementNamesByCatalogName(ReadOnlySpan<char> catalogName)
    {
        HashSet<string> elementNames = new(StringComparer.OrdinalIgnoreCase);

        AddClassElementNames(_baseElementCache, catalogName, elementNames);
        AddClassElementNames(_mapElementCache, catalogName, elementNames);
        AddClassElementNames(_customElementCache, catalogName, elementNames);

        return elementNames;
    }

    /// <summary>
    /// Gets the <c>id</c>s of the <see cref="StormXmlElement"/>s of a catalog name, from all the data storages or from one of them.
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <param name="stormModType">The data storage to get the <c>id</c>s from, or <see langword="null"/> for all of them.</param>
    /// <returns>A new case-sensitive set of the <c>id</c>s, or an empty set if none of the data storages have the catalog name.</returns>
    public HashSet<string> GetStormXmlElementIdsByCatalogName(ReadOnlySpan<char> catalogName, StormModType? stormModType = null)
    {
        HashSet<string> ids = new(StringComparer.Ordinal);

        if (stormModType is null or StormModType.Base)
            AddStormXmlElementIds(_baseElementCache, catalogName, ids);

        if (stormModType is null or StormModType.Map)
            AddStormXmlElementIds(_mapElementCache, catalogName, ids);

        if (stormModType is null or StormModType.Custom)
            AddStormXmlElementIds(_customElementCache, catalogName, ids);

        return ids;
    }

    /// <summary>
    /// Determines if any of the data storages has a <see cref="StormXmlElement"/> with no <c>id</c> for the class element name. Nothing is merged.
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <returns><see langword="true"/> if a data storage has the element; otherwise, <see langword="false"/>.</returns>
    public bool ContainsStormXmlElementByClassElement(ReadOnlySpan<char> elementName)
    {
        // custom first
        return FindStormXmlElementByElementName(_customElementCache, elementName) is not null ||
            FindStormXmlElementByElementName(_mapElementCache, elementName) is not null ||
            FindStormXmlElementByElementName(_baseElementCache, elementName) is not null;
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
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_baseElementCache, elementName));
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_mapElementCache, elementName));
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_customElementCache, elementName));

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
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_baseElementCache, id, catalogName));
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_mapElementCache, id, catalogName));
        LayerMerger.MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_customElementCache, id, catalogName));

        return stormXmlElement;
    }

    /// <summary>
    /// Determines if any of the data storages has a <see cref="StormXmlElement"/> with the <c>id</c> in the specified catalog name.
    /// Nothing is merged, so this is cheaper than <see cref="GetStormXmlElementById(ReadOnlySpan{char}, ReadOnlySpan{char})"/>.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>. This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>. This is case-insensitive.</param>
    /// <returns><see langword="true"/> if a data storage has the element; otherwise, <see langword="false"/>.</returns>
    public bool ContainsStormXmlElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        // custom first
        return FindStormXmlElementById(_customElementCache, id, catalogName) is not null ||
            FindStormXmlElementById(_mapElementCache, id, catalogName) is not null ||
            FindStormXmlElementById(_baseElementCache, id, catalogName) is not null;
    }

    /// <summary>
    /// Tries to retrieve the first <c>id</c> of an element in the specified catalog name by its <c>unitName</c> attribute from the data storages.
    /// </summary>
    /// <param name="unitName">The value of the <c>unitName</c> attribute (e.g. <c>HeroMuradin</c>). This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Actor</c>). This is case-insensitive.</param>
    /// <param name="id">When this method returns, contains the <c>id</c> of the element, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <c>id</c> was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstIdByUnitName(ReadOnlySpan<char> unitName, ReadOnlySpan<char> catalogName, [NotNullWhen(true)] out string? id)
    {
        // custom first
        return TryGetIdByUnitName(_customElementCache, unitName, catalogName, out id) ||
            TryGetIdByUnitName(_mapElementCache, unitName, catalogName, out id) ||
            TryGetIdByUnitName(_baseElementCache, unitName, catalogName, out id);
    }

    /// <summary>
    /// Tries to retrieve the <c>id</c> of an element in the specified catalog name by its <c>unitName</c> attribute from one data storage.
    /// <para>
    /// This is the unit name a definition of the element had, which a later definition of the same element can have changed.
    /// </para>
    /// </summary>
    /// <param name="stormModType">The data storage to look in.</param>
    /// <param name="unitName">The value of the <c>unitName</c> attribute (e.g. <c>HeroMuradin</c>). This is case-sensitive.</param>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Actor</c>). This is case-insensitive.</param>
    /// <param name="id">When this method returns, contains the <c>id</c> of the element, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <c>id</c> was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIdByUnitNameInLayer(StormModType stormModType, ReadOnlySpan<char> unitName, ReadOnlySpan<char> catalogName, [NotNullWhen(true)] out string? id)
    {
        ElementCache? elementCache = stormModType switch
        {
            StormModType.Base => _baseElementCache,
            StormModType.Map => _mapElementCache,
            StormModType.Custom => _customElementCache,
            _ => null,
        };

        if (elementCache is null)
        {
            id = null;
            return false;
        }

        return TryGetIdByUnitName(elementCache, unitName, catalogName, out id);
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

    private static void AddStormXmlElementIds(ElementCache elementCache, ReadOnlySpan<char> catalogName, HashSet<string> ids)
    {
        if (elementCache.StormXmlElementsByIdByCatalog.TryGetValue(catalogName, out var stormXmlElementById))
            ids.UnionWith(stormXmlElementById.Dictionary.Keys);
    }

    private static void AddClassElementNames(ElementCache elementCache, ReadOnlySpan<char> catalogName, HashSet<string> elementNames)
    {
        // the dictionary's comparer is StringComparer.OrdinalIgnoreCase, which supports span lookups
        if (elementCache.ClassElementsByCatalog.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(catalogName, out HashSet<string>? foundElementNames))
            elementNames.UnionWith(foundElementNames);
    }

    private static StormXmlElement? FindStormXmlElementByElementName(ElementCache elementCache, ReadOnlySpan<char> elementName)
    {
        if (elementCache.StormXmlElementByClassElement.TryGetValue(elementName, out StormXmlElement? stormXmlElement))
            return stormXmlElement;

        return null;
    }

    private static bool TryGetIdByUnitName(ElementCache elementCache, ReadOnlySpan<char> unitName, ReadOnlySpan<char> catalogName, [NotNullWhen(true)] out string? id)
    {
        if (elementCache.IdByUnitNameByCatalog.TryGetValue(catalogName, out var idByUnitName) && idByUnitName.TryGetValue(unitName, out id))
            return true;

        id = null;
        return false;
    }

    private static StormXmlElement? FindStormXmlElementById(ElementCache elementCache, ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (elementCache.StormXmlElementsByIdByCatalog.TryGetValue(catalogName, out var stormXmlElementById) &&
            stormXmlElementById.TryGetValue(id, out StormXmlElement? stormXmlElement))
        {
            return stormXmlElement;
        }

        return null;
    }

    private static List<LevelScalingFieldValue>? FindScalingFieldValuesById(ElementCache elementCache, ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (elementCache.ScalingFieldValueByIdByCatalog.TryGetValue(catalogName, out var scalingFieldValueById) &&
            scalingFieldValueById.TryGetValue(id, out List<LevelScalingFieldValue>? scalingFieldValues))
        {
            return scalingFieldValues;
        }

        return null;
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
}
