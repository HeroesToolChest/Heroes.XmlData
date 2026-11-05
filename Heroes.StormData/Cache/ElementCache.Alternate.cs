namespace Heroes.StormData.Cache;

/// <content>
/// Represents the alternate lookup dictionaries for the <see cref="ElementCache"/> class.
/// </content>
internal sealed partial class ElementCache
{
    public ElementCache()
    {
        StormXmlConstElementByIdAltLookup = StormXmlConstElementById.GetAlternateLookup<ReadOnlySpan<char>>();
        CatalogByClassElementAltLookup = CatalogByClassElement.GetAlternateLookup<ReadOnlySpan<char>>();
        StormXmlElementByClassElementAltLookup = StormXmlElementByClassElement.GetAlternateLookup<ReadOnlySpan<char>>();
        StormXmlElementsByCatalogAltLookup = StormXmlElementsByIdByCatalog.GetAlternateLookup<ReadOnlySpan<char>>();
        ScalingFieldValueByIdByCatalogAltLookup = ScalingFieldValueByIdByCatalog.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlConstElement"/>s by their <c>id</c> attribute.
    /// <para>
    /// The dictionary key is case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlConstElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlConstElementByIdAltLookup { get; }

    /// <summary>
    /// Gets a dictionary of catalog names (e.g. <c>Effect</c>) by class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> CatalogByClassElementAltLookup { get; }

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s with no <c>id</c> attribute by their class element name (without type letter declaration) (e.g. <c>EffectDamage</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlElementByClassElementAltLookup { get; }

    /// <summary>
    /// Gets a dictionary of <see cref="StormXmlElement"/>s by their <c>id</c> attribute by their catalog name (e.g. <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner dictionary key should be case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> StormXmlElementsByCatalogAltLookup { get; }

    /// <summary>
    /// Gets a dictionary of <c>LevelScalingArray</c> <c>Field</c> and <c>Value</c>s by their id attribute (<c>Entry</c>) by their catalog name (e.g <c>Effect</c>).
    /// <para>
    /// The dictionary key is case-insensitive.
    /// </para>
    /// <para>
    /// The inner dictionary key should be case-sensitive.
    /// </para>
    /// </summary>
    public Dictionary<string, Dictionary<string, List<LevelScalingFieldValue>>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> ScalingFieldValueByIdByCatalogAltLookup { get; }
}
