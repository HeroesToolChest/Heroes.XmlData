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

    private readonly TextCache _baseTextCache;
    private readonly TextCache _mapTextCache;
    private readonly TextCache _customTextCache;

    private readonly FileCache _baseFileCache;
    private readonly FileCache _mapFileCache;
    private readonly FileCache _customFileCache;

    private readonly StyleCache _baseStyleCache;
    private readonly StyleCache _mapStyleCache;
    private readonly StyleCache _customStyleCache;

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

        _baseTextCache = _baseDataStorage.TextCache;
        _mapTextCache = _mapDataStorage.TextCache;
        _customTextCache = _customDataStorage.TextCache;

        _baseFileCache = _baseDataStorage.FileCache;
        _mapFileCache = _mapDataStorage.FileCache;
        _customFileCache = _customDataStorage.FileCache;

        _baseStyleCache = _baseDataStorage.StyleCache;
        _mapStyleCache = _mapDataStorage.StyleCache;
        _customStyleCache = _customDataStorage.StyleCache;

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
        MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_baseStyleCache, name));
        MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_mapStyleCache, name));
        MergeInto(ref stormXmlStyleConstantElement, ref isCopy, FindStormXmlStyleConstantElementByName(_customStyleCache, name));

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
        MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_baseStyleCache, name));
        MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_mapStyleCache, name));
        MergeInto(ref stormXmlStyleElement, ref isCopy, FindStormXmlStyleElementByName(_customStyleCache, name));

        return stormXmlStyleElement;
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

    /// <summary>
    /// Tries to retrieve the first asset text value (from an <c>assets.txt</c> file) by its <c>id</c> from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> (the key) of the asset text, without the leading <c>@</c> that is used to reference it in XML. This is case-sensitive.</param>
    /// <param name="value">
    /// When this method returns, contains the asset text value associated with the specified <c>id</c>, if found; otherwise, <see langword="null"/>.
    /// The value can be <see cref="string.Empty"/> if the asset text has no value.
    /// </param>
    /// <returns><see langword="true"/> if the asset text was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstAssetTextById(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value)
    {
        value = null;

        // custom first
        if (_customTextCache.AssetTextsById.TryGetValue(id, out StringValue? stringValue))
        {
            value = stringValue.Value;
            return true;
        }

        if (_mapTextCache.AssetTextsById.TryGetValue(id, out stringValue))
        {
            value = stringValue.Value;
            return true;
        }

        if (_baseTextCache.AssetTextsById.TryGetValue(id, out stringValue))
        {
            value = stringValue.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to retrieve the first <c>.stormlayout</c> file by its relative <c>ui</c> path from the data storages.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormdata</c> directory, which starts with <c>ui</c> (e.g. <c>ui/layout/hero/herotalents.stormlayout</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/> associated with the specified path, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstStormLayoutFileByRelativePath(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile)
    {
        return TryGetFirstFileByRelativePath(relativePath, static fileCache => fileCache.StormLayoutFilesByRelativePath, out stormFile);
    }

    /// <summary>
    /// Tries to retrieve the first asset file (e.g. a <c>.dds</c> file) by its relative <c>assets</c> path from the data storages.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormassets</c> directory, which starts with <c>assets</c> (e.g. <c>assets/textures/storm_ui_icon_abathur.dds</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/> associated with the specified path, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstAssetFileByRelativePath(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile)
    {
        return TryGetFirstFileByRelativePath(relativePath, static fileCache => fileCache.AssetFilesByRelativePath, out stormFile);
    }

    private static StormXmlElement? FindStormXmlElementByElementName(ElementCache elementCache, ReadOnlySpan<char> elementName)
    {
        if (elementCache.StormXmlElementByClassElement.TryGetValue(elementName, out StormXmlElement? stormXmlElement))
            return stormXmlElement;

        return null;
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

    private static List<LevelScalingFieldValue>? FindScalingFieldValuesById(ElementCache elementCache, ReadOnlySpan<char> id, ReadOnlySpan<char> catalogName)
    {
        if (elementCache.ScalingFieldValueByIdByCatalog.TryGetValue(catalogName, out var scalingFieldValueById) &&
            scalingFieldValueById.TryGetValue(id, out List<LevelScalingFieldValue>? scalingFieldValues))
        {
            return scalingFieldValues;
        }

        return null;
    }

    private static void MergeInto<T>(ref T? stormXmlElement, ref bool isCopy, T? layerElement)
        where T : StormXmlElement
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
            stormXmlElement = (T)stormXmlElement.Clone();
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

    // the file caches are keyed by normalized paths (see PathNormalizer), so the path is normalized the same way before the lookup
    private bool TryGetFirstFileByRelativePath(
        ReadOnlySpan<char> relativePath,
        Func<FileCache, Dictionary<string, StormFile>.AlternateLookup<ReadOnlySpan<char>>> getFilesByRelativePath,
        [NotNullWhen(true)] out StormFile? stormFile)
    {
        const int maxStackLength = 256;

        char[]? rentedBuffer = null;

        Span<char> buffer = relativePath.Length <= maxStackLength
            ? stackalloc char[maxStackLength]
            : (rentedBuffer = ArrayPool<char>.Shared.Rent(relativePath.Length));

        try
        {
            Span<char> normalizedPath = buffer[..relativePath.Length];
            relativePath.CopyTo(normalizedPath);
            PathNormalizer.NormalizePath(normalizedPath);

            // custom first
            if (getFilesByRelativePath(_customFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            if (getFilesByRelativePath(_mapFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            if (getFilesByRelativePath(_baseFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            return false;
        }
        finally
        {
            if (rentedBuffer is not null)
                ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }

    // each data storage's version only goes up, so the sum changes whenever any of them does
    private int GetElementsVersion() => _baseDataStorage.ElementsVersion + _mapDataStorage.ElementsVersion + _customDataStorage.ElementsVersion;
}
