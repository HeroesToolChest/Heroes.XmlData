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

    /// <summary>
    /// Gets a lookup of element <c>id</c>s by their <c>unitName</c> attribute (the unit an actor is for, e.g. <c>HeroMuradin</c>) by their catalog name (e.g. <c>Actor</c>).
    /// <para>
    /// The key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner key (the unit name) is case-sensitive, like the unit's <c>id</c>.
    /// </para>
    /// <para>
    /// The inner value is every <c>id</c> that had the unit name, each once, in the order they were loaded (the last one is the one loaded last),
    /// as more than one element can have the same unit name (e.g. <c>DeadHeroResurrectSpirit</c>).
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, List<string>>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> IdsByUnitNameByCatalog { get; } =
        new Dictionary<string, Dictionary<string, List<string>>.AlternateLookup<ReadOnlySpan<char>>>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Removes all the elements and lookups.
    /// </summary>
    public void Clear()
    {
        StormXmlConstElementById.Dictionary.Clear();
        ClassElementsByCatalog.Clear();
        CatalogByClassElement.Dictionary.Clear();
        StormXmlElementByClassElement.Dictionary.Clear();
        StormXmlElementsByIdByCatalog.Dictionary.Clear();
        ScalingFieldValueByIdByCatalog.Dictionary.Clear();
        IdsByUnitNameByCatalog.Dictionary.Clear();
    }
}
