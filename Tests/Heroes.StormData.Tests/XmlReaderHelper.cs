namespace Heroes.StormData.Tests;

internal static class XmlReaderHelper
{
    internal static XmlReader CreateXmlReader(string xml)
    {
        return XmlReader.Create(new StringReader(xml));
    }

    internal static StormXmlElement GetStormXmlElement(string xml, ElementCache? elementCache = null)
    {
        StormXmlFileParser stormXmlFileParser = new(elementCache ?? new ElementCache());
        using XmlReader reader = CreateXmlReader(xml);

        return stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile()).First();
    }
}
