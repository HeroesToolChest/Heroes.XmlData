namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a storage container for different types of data storage, including base, map, and custom data storage.
/// </summary>
internal sealed class LayeredDataStorage
{
    private readonly DataStorage _baseDataStorage;
    private readonly DataStorage _mapDataStorage;
    private readonly DataStorage _customDataStorage;

    private readonly ElementCache _baseElementCache;
    private readonly ElementCache _mapElementCache;
    private readonly ElementCache _customElementCache;

    public LayeredDataStorage()
    {
        _baseDataStorage = new DataStorage();
        _mapDataStorage = new DataStorage();
        _customDataStorage = new DataStorage();

        _baseElementCache = _baseDataStorage.ElementCache;
        _mapElementCache = _mapDataStorage.ElementCache;
        _customElementCache = _customDataStorage.ElementCache;
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
    /// <param name="id">The <c>id</c> of the <c>const</c> XML element to retrieve.</param>
    /// <param name="stormXmlConstElement">When this method returns, contains the <c>const</c> XML element associated with the specified <c>id</c>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <c>const</c> XML element was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstStormXmlConstElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXmlConstElement? stormXmlConstElement)
    {
        // custom cache always first
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
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>).</param>
    /// <param name="catalogName">When this method returns, contains the catalog name (e.g. <c>Effect</c>) associated with the specified class element name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the catalog name was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstCatalogNameByElementName(ReadOnlySpan<char> elementName, [NotNullWhen(true)] out string? catalogName)
    {
        // custom cache always first
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
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>).</param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementByClassElement(ReadOnlySpan<char> elementName)
    {
        StormXmlElement? stormXmlElement = null;
        bool isCopy = false;

        // base cache first
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_baseElementCache, elementName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_mapElementCache, elementName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementByElementName(_customElementCache, elementName));

        return stormXmlElement;
    }

    /// <summary>
    /// Retrieves a <see cref="StormXmlElement"/> by its <c>id</c> from the specified catalog name from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> of the <see cref="StormXmlElement"/>.</param>
    /// <param name="catalogName">The catalog name of the <see cref="StormXmlElement"/>.</param>
    /// <returns>The <see cref="StormXmlElement"/> if found; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetStormXmlElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        StormXmlElement? stormXmlElement = null;
        bool isCopy = false;

        // base cache first
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_baseElementCache, id, catalogName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_mapElementCache, id, catalogName));
        MergeInto(ref stormXmlElement, ref isCopy, FindStormXmlElementById(_customElementCache, id, catalogName));

        return stormXmlElement;
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
}
