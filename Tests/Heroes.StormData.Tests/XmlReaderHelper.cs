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
        StormXmlFileParser stormXmlFileParser = CreateStormXmlFileParser(layeredDataStorage);
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

    /// <summary>
    /// Parses the provided XML string into a <see cref="StormXmlStyleConstantElement"/> using <see cref="StormXmlFileParser.ParseStyleFile(XmlReader, StormFile)"/>.
    /// </summary>
    /// <param name="xml">A string containing a <c>Constant</c> element. Do not include the root element, as it will be added automatically.</param>
    /// <returns>A <see cref="StormXmlStyleConstantElement"/> instance.</returns>
    internal static StormXmlStyleConstantElement GetStormXmlStyleConstantElement(string xml)
    {
        return (StormXmlStyleConstantElement)GetStormXmlStyleFileElement(xml);
    }

    /// <summary>
    /// Parses the provided XML string into a <see cref="StormXmlStyleElement"/> using <see cref="StormXmlFileParser.ParseStyleFile(XmlReader, StormFile)"/>.
    /// </summary>
    /// <param name="xml">A string containing a <c>Style</c> element. Do not include the root element, as it will be added automatically.</param>
    /// <returns>A <see cref="StormXmlStyleElement"/> instance.</returns>
    internal static StormXmlStyleElement GetStormXmlStyleElement(string xml)
    {
        return (StormXmlStyleElement)GetStormXmlStyleFileElement(xml);
    }

    /// <summary>
    /// Creates a <see cref="StormXmlFileParser"/> that looks up <c>const</c> elements and asset texts from the given <see cref="LayeredDataStorage"/>.
    /// </summary>
    /// <param name="layeredDataStorage">An optional <see cref="LayeredDataStorage"/> instance. If <see langword="null"/>, an empty one is used.</param>
    /// <returns>A <see cref="StormXmlFileParser"/> instance.</returns>
    internal static StormXmlFileParser CreateStormXmlFileParser(LayeredDataStorage? layeredDataStorage = null)
    {
        layeredDataStorage ??= new LayeredDataStorage();

        return new StormXmlFileParser(layeredDataStorage.Elements, layeredDataStorage.Texts);
    }

    private static StormXmlElement GetStormXmlStyleFileElement(string xml)
    {
        StormXmlFileParser stormXmlFileParser = CreateStormXmlFileParser();
        using XmlReader reader = CreateXmlReader(xml);

        return stormXmlFileParser.ParseStyleFile(reader, StormFileHelper.CreateStormFile()).First();
    }
}
