namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a storage container for different types of data storage, including base, map, and custom data storage.
/// </summary>
internal class LayeredDataStorage
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
}
