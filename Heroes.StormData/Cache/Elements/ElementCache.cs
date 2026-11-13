namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents a cache for storing and retrieving elements from the data storage.
/// </summary>
internal sealed class ElementCache
{
    /// <summary>
    /// Gets a lookup of <see cref="StormXmlConstElement"/>s by their <c>id</c> attribute.
    /// <para>
    /// The key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlConstElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlConstElementById { get; } =
        new Dictionary<string, StormXmlConstElement>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a dictionary of class element names (without type letter declaration) (e.g. <c>EffectDamage</c>) by catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, HashSet<string>> ClassElementsByCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a lookup of catalog names (e.g. <c>Effect</c>) by class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> CatalogByClassElement { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a lookup of <see cref="StormXmlElement"/>s with no <c>id</c> attribute by their class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlElementByClassElement { get; } =
        new Dictionary<string, StormXmlElement>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a lookup of <see cref="StormXmlElement"/>s by their <c>id</c> attribute by their catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner key should be case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> StormXmlElementsByIdByCatalog { get; } =
        new Dictionary<string, Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>>>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a lookup of <c>LevelScalingArray</c> <c>Field</c> and <c>Value</c>s by their id attribute (<c>Entry</c>) by their catalog name (e.g <c>Effect</c>).
    /// <para>
    /// The key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner key should be case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, List<LevelScalingFieldValue>>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> ScalingFieldValueByIdByCatalog { get; } =
        new Dictionary<string, Dictionary<string, List<LevelScalingFieldValue>>.AlternateLookup<ReadOnlySpan<char>>>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();
}
