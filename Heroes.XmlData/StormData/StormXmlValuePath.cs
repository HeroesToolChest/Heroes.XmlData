using System.Text;
using U8Xml;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the xml of an element and it's relative path.
/// </summary>
public record StormXmlValuePath : StormValuePath<byte[]>
{
    private string? _xmlAsString;

    /// <summary>
    /// Initializes a new instance of the <see cref="StormXmlValuePath"/> class.
    /// </summary>
    /// <param name="xml">The xml in <see cref="byte"/> form.</param>
    /// <param name="stormPath">The relative path where the xml resides from.</param>
    public StormXmlValuePath(byte[] xml, StormPath stormPath)
        : base(xml, stormPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormXmlValuePath"/> class.
    /// </summary>
    /// <param name="xmlNode">The element node of an <see cref="XmlObject"/>.</param>
    /// <param name="stormPath">The relative path where the xml resides from.</param>
    public StormXmlValuePath(XmlNode xmlNode, StormPath stormPath)
        : this(xmlNode.AsRawString().ToArray(), stormPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormXmlValuePath"/> class.
    /// </summary>
    /// <param name="xmlObject">The <see cref="XmlObject"/>.</param>
    /// <param name="stormPath">The relative path where the xml resides from.</param>
    public StormXmlValuePath(XmlObject xmlObject, StormPath stormPath)
        : this(xmlObject.AsRawString().ToArray(), stormPath)
    {
    }

    /// <summary>
    /// Gets xml as a string.
    /// </summary>
    public string Xml => _xmlAsString ??= Encoding.UTF8.GetString(Value);
}
