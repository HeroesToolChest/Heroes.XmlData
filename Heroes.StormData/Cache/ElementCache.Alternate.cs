namespace Heroes.StormData.Cache;

internal partial class ElementCache
{
    public ElementCache()
    {
        StormXmlConstElementByIdAltLookup = StormXmlConstElementById.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public Dictionary<string, StormXmlConstElement>.AlternateLookup<ReadOnlySpan<char>> StormXmlConstElementByIdAltLookup { get; }
}
