namespace Heroes.StormData.Cache.Styles;

/// <summary>
/// Represents a cache for storing and retrieving <c>.stormstyle</c> elements from the data storage.
/// </summary>
internal sealed class StyleCache
{
    /// <summary>
    /// Gets a lookup of <see cref="StormXmlStyleConstantElement"/>s by their <c>name</c> attribute.
    /// <para>
    /// The key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlStyleConstantElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlStyleConstantElementByName { get; } =
        new Dictionary<string, StormXmlStyleConstantElement>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a lookup of <see cref="StormXmlStyleElement"/>s by their <c>name</c> attribute.
    /// <para>
    /// The key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlStyleElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlStyleElementByName { get; } =
        new Dictionary<string, StormXmlStyleElement>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
}
