namespace Heroes.StormData.Tests;

internal static class XmlReaderHelper
{
    /// <summary>
    /// Creates an <see cref="XmlReader"/> from a string containing XML.
    /// </summary>
    /// <param name="xml">A string containing XML. Do not include the root &lt;Catalog&gt; element, as it will be added automatically.</param>
    /// <returns>An <see cref="XmlReader"/> instance.</returns>
    internal static XmlReader CreateXmlReader(string xml)
    {
        return XmlReader.Create(new StringReader($"<Catalog>{xml}</Catalog>"));
    }

    /// <summary>
    /// Parses the provided XML string into a <see cref="StormXmlElement"/> using the <see cref="StormXmlFileParser"/>.
    /// </summary>
    /// <param name="xml">A string containing XML. Do not include the root &lt;Catalog&gt; element, as it will be added automatically.</param>
    /// <param name="layeredDataStorage">An optional <see cref="LayeredDataStorage"/> instance.</param>
    /// <returns>A <see cref="StormXmlElement"/> instance.</returns>
    internal static StormXmlElement GetStormXmlElement(string xml, LayeredDataStorage? layeredDataStorage = null)
    {
        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage ?? new LayeredDataStorage());
        using XmlReader reader = CreateXmlReader(xml);

        return stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile()).First();
    }

    /// <summary>
    /// Parses the provided XML string into a <see cref="StormXmlConstElement"/> using the <see cref="StormXmlFileParser"/>.
    /// </summary>
    /// <param name="xml">A string containing XML. Do not include the root &lt;Catalog&gt; element, as it will be added automatically.</param>
    /// <param name="layeredDataStorage">An optional <see cref="LayeredDataStorage"/> instance.</param>
    /// <returns>A <see cref="StormXmlConstElement"/> instance.</returns>
    internal static StormXmlConstElement GetStormXmlConstElement(string xml, LayeredDataStorage? layeredDataStorage = null)
    {
        return (StormXmlConstElement)GetStormXmlElement(xml, layeredDataStorage);
    }
}
