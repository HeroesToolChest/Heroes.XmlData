namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods. This partial class contains alternate lookup dictionaries.
/// </summary>
internal sealed partial class StormCache
{
    public StormCache()
    {
        ConstantXElementByIdAltLookup = ConstantXElementById.GetAlternateLookup<ReadOnlySpan<char>>();
        AssetTextsByIdAltLookup = AssetTextsById.GetAlternateLookup<ReadOnlySpan<char>>();
        StormStyleStyleElementsByNameAltLookup = StormStyleStyleElementsByName.GetAlternateLookup<ReadOnlySpan<char>>();
        StormStyleConstantElementsByNameAltLookup = StormStyleConstantElementsByName.GetAlternateLookup<ReadOnlySpan<char>>();
        StormElementsByDataObjectTypeAltLookup = StormElementsByDataObjectType.GetAlternateLookup<ReadOnlySpan<char>>();
        StormElementByElementTypeAltLookup = StormElementByElementType.GetAlternateLookup<ReadOnlySpan<char>>();
        DataObjectTypeByElementTypeAltLookup = DataObjectTypeByElementType.GetAlternateLookup<ReadOnlySpan<char>>();
        ScaleValueStormElementsByDataObjectTypeAltLookup = ScaleValueStormElementsByDataObjectType.GetAlternateLookup<ReadOnlySpan<char>>();
        ElementTypesByDataObjectTypeAltLookup = ElementTypesByDataObjectType.GetAlternateLookup<ReadOnlySpan<char>>();
        UnitNamesByDataObjectTypeAltLookup = UnitNamesByDataObjectType.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public Dictionary<string, StormXElementValuePath>.AlternateLookup<ReadOnlySpan<char>> ConstantXElementByIdAltLookup { get; }

    public Dictionary<string, AssetText>.AlternateLookup<ReadOnlySpan<char>> AssetTextsByIdAltLookup { get; }

    public Dictionary<string, StormStyleStyleElement>.AlternateLookup<ReadOnlySpan<char>> StormStyleStyleElementsByNameAltLookup { get; }

    public Dictionary<string, StormStyleConstantElement>.AlternateLookup<ReadOnlySpan<char>> StormStyleConstantElementsByNameAltLookup { get; }

    public Dictionary<string, Dictionary<string, StormElement>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> StormElementsByDataObjectTypeAltLookup { get; }

    public Dictionary<string, StormElement>.AlternateLookup<ReadOnlySpan<char>> StormElementByElementTypeAltLookup { get; }

    public Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> DataObjectTypeByElementTypeAltLookup { get; }

    public Dictionary<string, Dictionary<string, StormElement>>.AlternateLookup<ReadOnlySpan<char>> ScaleValueStormElementsByDataObjectTypeAltLookup { get; }

    public Dictionary<string, HashSet<string>>.AlternateLookup<ReadOnlySpan<char>> ElementTypesByDataObjectTypeAltLookup { get; }

    public Dictionary<string, Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>>>.AlternateLookup<ReadOnlySpan<char>> UnitNamesByDataObjectTypeAltLookup { get; }
}
