using System.Diagnostics;

namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a top level XML element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormXmlElement
{
    private readonly List<StormFile> _stormFiles = [];
    private readonly StormXmlProperty _stormXmlProperties;

    internal StormXmlElement(StormXmlProperty stormXmlProperty, StormFile stormFile)
    {
        _stormXmlProperties = stormXmlProperty;
        _stormFiles.Add(stormFile);

        Initialize(stormXmlProperty);
    }

    /// <summary>
    /// Gets the properties of the XML element.
    /// </summary>
    public StormXmlProperty Data => _stormXmlProperties;

    /// <summary>
    /// Gets the value of the <c>id</c> attribute.
    /// </summary>
    public string? Id { get; private set; }

    /// <summary>
    /// Gets the value of the <c>parent</c> attribute.
    /// </summary>
    public string? Parent { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the <c>default</c> attribute is set to "1".
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the name of the XML element.
    /// </summary>
    public string ElementName { get; private set; }

    /// <summary>
    /// Gets the type of the XML element.
    /// </summary>
    public StormXmlElementType ElementType { get; private set; }
    /// <summary>
    /// Gets a list of <see cref="StormFile"/>s that this element exists in.
    /// <para>
    /// The list will contain more than one item if this element has been merged from multiple <see cref="StormXmlElement"/>s.
    /// </para>
    /// </summary>
    public IReadOnlyList<StormFile> StormFiles => _stormFiles.AsReadOnly();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string id = string.IsNullOrWhiteSpace(Id) ? string.Empty : $", Id = \"{Id}\"";
            string parent = string.IsNullOrWhiteSpace(Parent) ? string.Empty : $", Parent = \"{Parent}\"";

            return $"{{{ElementName}{id}{parent}, Elements = {_stormXmlProperties.Count}}}";
        }
    }

    ///// <summary>
    ///// Creates a <see cref="StormXmlElement"/> from an <see cref="XmlReader"/>.
    ///// </summary>
    ///// <param name="reader">The <see cref="XmlReader"/> to read from.</param>
    ///// <returns>A <see cref="StormXmlElement"/> instance.</returns>
    //internal static StormXmlElement Create(XmlReader reader, StormAttributeVariables stormAttributeVariables)
    //{
    //    reader.MoveToContent();
    //    return new StormXmlElement(Parse(reader, stormAttributeVariables));
    //}

    /// <summary>
    /// Creates a <see cref="StormXmlElement"/> from a <see cref="StormXmlProperty"/>.
    /// </summary>
    /// <param name="rootStormXmlProperty">The root <see cref="StormXmlProperty"/> to create the element from.</param>
    /// <param name="stormFile">The <see cref="StormFile"/> associated with the element.</param>
    /// <returns>A <see cref="StormXmlElement"/> instance.</returns>
    internal static StormXmlElement Create(StormXmlProperty rootStormXmlProperty, StormFile stormFile)
    {
        return new StormXmlElement(rootStormXmlProperty, stormFile);
    }

    /// <summary>
    /// Merges the properties of another <see cref="StormXmlElement"/> into this instance.
    /// </summary>
    /// <param name="other">The <see cref="StormXmlElement"/> to merge into the current instance.</param>
    internal void Merge(StormXmlElement other)
    {
        if (ReferenceEquals(this, other))
            return;

        _stormXmlProperties.Merge(other._stormXmlProperties);
        _stormFiles.AddRange(other._stormFiles);

        Initialize(other._stormXmlProperties);
    }

    [MemberNotNull(nameof(ElementName))]
    private void Initialize(StormXmlProperty stormXmlProperty)
    {
        if (stormXmlProperty.Name.Equals("const", StringComparison.Ordinal))
        {
            ElementName = stormXmlProperty.Name;
            ElementType = StormXmlElementType.Const;
        }
        else
        {
            ElementName = stormXmlProperty.Name[1..];

            if (stormXmlProperty.Name.StartsWith('C'))
                ElementType = StormXmlElementType.Class;
            else if (stormXmlProperty.Name.StartsWith('S'))
                ElementType = StormXmlElementType.Struct;
            else if (stormXmlProperty.Name.StartsWith('E'))
                ElementType = StormXmlElementType.Enum;
            else
                ElementType = StormXmlElementType.Unknown;
        }

        Id = stormXmlProperty.TryGetProperty("id", out StormXmlArray? idProperty) ? idProperty.Value : null;
        Parent = stormXmlProperty.TryGetProperty("parent", out StormXmlArray? parentProperty) ? parentProperty.Value : null;
        IsDefault = stormXmlProperty.TryGetProperty("default", out StormXmlArray? defaultProperty) && defaultProperty.Value is not null && defaultProperty.Value == "1";
    }

    //private static StormXmlProperty Parse(XmlReader reader, StormAttributeVariables stormAttributeVariables)
    //{
    //    string elementName = reader.LocalName;
    //    bool isArray = false;

    //    StormXmlProperty stormXmlProperty = new(elementName);

    //    if (elementName.EndsWith("array", StringComparison.OrdinalIgnoreCase) || StormXmlConstants.ElementArrays.Contains(elementName))
    //        isArray = true;

    //    // for self-closing tags like <Element ... />
    //    bool isEmptyElement = reader.IsEmptyElement;

    //    if (reader.MoveToFirstAttribute())
    //    {
    //        do
    //        {
    //            string name = reader.LocalName;
    //            string value = reader.Value;

    //            if (name.Equals(StormXmlConstants.IdAttributeName, StringComparison.Ordinal))
    //            {

    //            }
    //            else if (!isArray && name.Equals(StormXmlConstants.IndexAttributeName, StringComparison.OrdinalIgnoreCase))
    //            {
    //                isArray = true;
    //            }

    //            stormXmlProperty.Add(name, value);
    //        }
    //        while (reader.MoveToNextAttribute());

    //        reader.MoveToElement(); // back to the element node
    //    }

    //    stormXmlProperty.IsArray = isArray;

    //    if (isEmptyElement)
    //    {
    //        reader.Read(); // consume <Element ... />, done
    //        return stormXmlProperty;
    //    }

    //    reader.Read(); // consume the start tag <Element ...>

    //    while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
    //    {
    //        if (reader.NodeType == XmlNodeType.Element)
    //        {
    //            // Peek the child's index attribute without moving the reader
    //            string? indexValue = reader.GetAttribute(StormXmlConstants.IndexAttributeName);

    //            stormXmlProperty.Add(Parse(reader, stormAttributeVariables), indexValue);
    //        }
    //        else
    //        {
    //            reader.Read();
    //        }
    //    }

    //    reader.Read(); // consume the end tag </Element>

    //    return stormXmlProperty;
    //}
}
