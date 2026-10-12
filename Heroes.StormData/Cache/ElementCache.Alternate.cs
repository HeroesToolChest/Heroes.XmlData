namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a cache for storing and retrieving elements from the data storage.
/// </summary>
internal sealed partial class ElementCache
{
    public ElementCache()
    {
        StormXmlConstElementByIdAltLookup = StormXmlConstElementById.GetAlternateLookup<ReadOnlySpan<char>>();
        CatalogByClassElementAltLookup = CatalogByClassElement.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlConstElement"/>s by their <c>id</c> attribute.
    /// <para>
    /// The dictionary key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlConstElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlConstElementByIdAltLookup { get; }

    /// <summary>
    /// Gets a dictionary of class element names (without type letter declaration) (e.g. <c>EffectDamage</c>) by catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> CatalogByClassElementAltLookup { get; }
}
