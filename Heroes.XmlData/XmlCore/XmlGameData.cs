namespace Heroes.XmlData.XmlCore;

internal class XmlGameData
{
    private readonly List<XmlStorage> _xmlStormMods = [];

    public void Add(XmlStorage xmlStormMod)
    {
        _xmlStormMods.Add(xmlStormMod);
    }

    public void ClearGamestrings()
    {
        _xmlStormMods.ForEach(x => x.ClearGameStrings());
    }
}

