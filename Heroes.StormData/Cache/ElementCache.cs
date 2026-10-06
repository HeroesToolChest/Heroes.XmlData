namespace Heroes.StormData.Cache;

internal class ElementCache
{
    /// <summary>
    /// Gets a dictionary of class element names (without type letter declaration) (e.g. <c>EffectDamage</c>) by catalog name (e.g. <c>Effect</c>).
    /// </summary>
    public Dictionary<string, HashSet<string>> ClassElementsByCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of catalog names (e.g. <c>Effect</c>) by class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// </summary>
    public Dictionary<string, string> CatalogByClassElement { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s with no <c>id</c> attribute by their class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// </summary>
    public Dictionary<string, StormXmlElement> StormXmlElementByClassElement { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s by their <c>id</c> attribute by their catalog name (e.g. <c>Effect</c>).
    /// </summary>
    public Dictionary<string, Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>>> StormXmlElementsByCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);
}
