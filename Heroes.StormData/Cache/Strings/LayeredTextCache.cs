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
    /// Tries to retrieve the first gamestring value (from a <c>gamestrings.txt</c> file) by its <c>id</c> from the data storages.
    /// </summary>
    /// <param name="id">The <c>id</c> (the key) of the gamestring. This is case-sensitive.</param>
    /// <param name="value">
    /// When this method returns, contains the unparsed gamestring value associated with the specified <c>id</c>, if found; otherwise, <see langword="null"/>.
    /// The value can be <see cref="string.Empty"/> if the gamestring has no value.
    /// </param>
    /// <returns><see langword="true"/> if the gamestring was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstGameStringById(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value) =>
        TryGetFirst(id, static textCache => textCache.GameStringsById, out value);

    /// <summary>
    /// Gets all the gamestrings (from the <c>gamestrings.txt</c> files) of the data storages, by their <c>id</c>.
    /// If more than one data storage has a gamestring, the value from the highest one is used (custom, then map, then base).
    /// </summary>
    /// <returns>A new case-sensitive dictionary of the unparsed gamestring values by their <c>id</c>.</returns>
    public Dictionary<string, string> GetGameStrings()
    {
        Dictionary<string, StringValue> baseGameStrings = _baseTextCache.GameStringsById.Dictionary;
        Dictionary<string, StringValue> mapGameStrings = _mapTextCache.GameStringsById.Dictionary;
        Dictionary<string, StringValue> customGameStrings = _customTextCache.GameStringsById.Dictionary;

        Dictionary<string, string> gameStrings = new(baseGameStrings.Count + mapGameStrings.Count + customGameStrings.Count, StringComparer.Ordinal);

        // base first, so the higher layers replace its values
        foreach ((string id, StringValue stringValue) in baseGameStrings)
            gameStrings[id] = stringValue.Value;

        foreach ((string id, StringValue stringValue) in mapGameStrings)
            gameStrings[id] = stringValue.Value;

        foreach ((string id, StringValue stringValue) in customGameStrings)
            gameStrings[id] = stringValue.Value;

        return gameStrings;
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
    public bool TryGetFirstAssetTextById(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value) =>
        TryGetFirst(id, static textCache => textCache.AssetTextsById, out value);

    // looks in the custom layer first, then map, then base
    private bool TryGetFirst(
        ReadOnlySpan<char> id,
        Func<TextCache, Dictionary<string, StringValue>.AlternateLookup<ReadOnlySpan<char>>> getLookup,
        [NotNullWhen(true)] out string? value)
    {
        if (getLookup(_customTextCache).TryGetValue(id, out StringValue stringValue) ||
            getLookup(_mapTextCache).TryGetValue(id, out stringValue) ||
            getLookup(_baseTextCache).TryGetValue(id, out stringValue))
        {
            value = stringValue.Value;
            return true;
        }

        value = null;
        return false;
    }
}
