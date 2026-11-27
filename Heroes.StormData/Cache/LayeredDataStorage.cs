namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a storage container for different types of data storage, including base, map, and custom data storage.
/// <para>
/// The lookups across the data storages are done through the layered caches (<see cref="Elements"/>, <see cref="Texts"/>, <see cref="Files"/>, and <see cref="Styles"/>).
/// Their <c>TryGetFirst</c> methods get a reference to the first data storage and their <c>Get</c> methods merge from all the data storages (if they can).
/// </para>
/// </summary>
internal sealed class LayeredDataStorage
{
    private readonly DataStorage _baseDataStorage;
    private readonly DataStorage _mapDataStorage;
    private readonly DataStorage _customDataStorage;

    private readonly StormXmlElementMerger _stormXmlElementMerger;
    private readonly MergedStormXmlElementCache _mergedStormXmlElementCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredDataStorage"/> class.
    /// </summary>
    public LayeredDataStorage()
    {
        _baseDataStorage = new DataStorage();
        _mapDataStorage = new DataStorage();
        _customDataStorage = new DataStorage();

        Elements = new LayeredElementCache(_baseDataStorage.ElementCache, _mapDataStorage.ElementCache, _customDataStorage.ElementCache);
        Texts = new LayeredTextCache(_baseDataStorage.TextCache, _mapDataStorage.TextCache, _customDataStorage.TextCache);
        Files = new LayeredFileCache(_baseDataStorage.FileCache, _mapDataStorage.FileCache, _customDataStorage.FileCache);
        Styles = new LayeredStyleCache(_baseDataStorage.StyleCache, _mapDataStorage.StyleCache, _customDataStorage.StyleCache);

        _stormXmlElementMerger = new StormXmlElementMerger(Elements);
        _mergedStormXmlElementCache = new MergedStormXmlElementCache(_stormXmlElementMerger);
    }

    /// <summary>
    /// Gets the element lookups across the data storages.
    /// </summary>
    public LayeredElementCache Elements { get; }

    /// <summary>
    /// Gets the text lookups across the data storages.
    /// </summary>
    public LayeredTextCache Texts { get; }

    /// <summary>
    /// Gets the file lookups across the data storages.
    /// </summary>
    public LayeredFileCache Files { get; }

    /// <summary>
    /// Gets the <c>.stormstyle</c> lookups across the data storages.
    /// </summary>
    public LayeredStyleCache Styles { get; }

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
    /// Retrieves the merged default element (the element with no <c>id</c>) of a class, merged from the data storages and over its catalog's base class
    /// default element (e.g. <c>CEffect</c> for <c>CEffectDamage</c>), the same as an element with an <c>id</c> is merged into (see <see cref="StormXmlElementMerger"/>).
    /// <para>
    /// It is not cached, and the returned element can be the stored instance (when there is nothing to merge), so it must not be modified.
    /// </para>
    /// </summary>
    /// <param name="elementName">The name of the class element (without type letter declaration) (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <returns>The merged default element if the class has one; otherwise, <see langword="null"/>.</returns>
    public StormXmlElement? GetMergedDefaultStormXmlElement(ReadOnlySpan<char> elementName)
    {
        return _stormXmlElementMerger.GetMergedElementOfNoId(elementName);
    }

    /// <summary>
    /// Removes all the data from the <see cref="DataStorage"/> of the specified <see cref="StormModType"/> and the cached merged elements.
    /// The other data storages are not changed.
    /// </summary>
    /// <param name="stormModType">The type of the <c>stormmod</c>.</param>
    public void ClearDataStorage(StormModType stormModType)
    {
        GetDataStorage(stormModType).Clear();

        // the merged elements may include the removed elements. The version change would clear them on the next lookup, but this releases them now.
        _mergedStormXmlElementCache.Clear();
    }

    /// <summary>
    /// Removes all the gamestrings from the base, map, and custom data storages.
    /// </summary>
    public void ClearGameStrings()
    {
        _baseDataStorage.TextCache.ClearGameStrings();
        _mapDataStorage.TextCache.ClearGameStrings();
        _customDataStorage.TextCache.ClearGameStrings();
    }

    /// <summary>
    /// Clears the cached merged elements from <see cref="GetMergedStormXmlElementById(ReadOnlySpan{char}, ReadOnlySpan{char})"/>.
    /// </summary>
    public void ClearMergedStormXmlElementCache()
    {
        _mergedStormXmlElementCache.Clear();
    }

    // each data storage's version only goes up, so the sum changes whenever any of them does
    private int GetElementsVersion() => _baseDataStorage.ElementsVersion + _mapDataStorage.ElementsVersion + _customDataStorage.ElementsVersion;
}
