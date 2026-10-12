namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a cache for storing and retrieving elements from the data storage.
/// </summary>
internal sealed partial class ElementCache
{
    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlConstElement"/>s by their <c>id</c> attribute.
    /// <para>
    /// The dictionary key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlConstElement> StormXmlConstElementById { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets a dictionary of class element names (without type letter declaration) (e.g. <c>EffectDamage</c>) by catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, HashSet<string>> ClassElementsByCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of catalog names (e.g. <c>Effect</c>) by class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, string> CatalogByClassElement { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s with no <c>id</c> attribute by their class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlElement> StormXmlElementByClassElement { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s by their <c>id</c> attribute by their catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner dictionary key should be case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>>> StormXmlElementsByCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);
}
