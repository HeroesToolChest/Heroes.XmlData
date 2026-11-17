namespace Heroes.StormData.Cache.Strings;

/// <summary>
/// Looks up text data across the base, map, and custom <see cref="TextCache"/> layers.
/// <para>The <c>TryGetFirst</c> methods get a reference from the first layer that has it (custom, then map, then base).</para>
/// </summary>
internal sealed class LayeredTextCache
{
    private readonly TextCache _baseTextCache;
    private readonly TextCache _mapTextCache;
    private readonly TextCache _customTextCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredTextCache"/> class.
    /// </summary>
    /// <param name="baseTextCache">The <see cref="TextCache"/> of the base data storage.</param>
    /// <param name="mapTextCache">The <see cref="TextCache"/> of the map data storage.</param>
    /// <param name="customTextCache">The <see cref="TextCache"/> of the custom data storage.</param>
    public LayeredTextCache(TextCache baseTextCache, TextCache mapTextCache, TextCache customTextCache)
    {
        _baseTextCache = baseTextCache;
        _mapTextCache = mapTextCache;
        _customTextCache = customTextCache;
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
}
