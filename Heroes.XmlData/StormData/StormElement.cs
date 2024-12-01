using U8Xml;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an xml element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElement
{
    private const string IdAttribute = "id";
    private const string ParentAttribute = "parent";
    private readonly List<StormXmlValuePath> _originalXmlElements = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXmlValuePath"/>.</param>
    internal StormElement(StormXmlValuePath baseValue)
    {
        _originalXmlElements.Add(baseValue);

        using XmlObject xmlObject = XmlParser.Parse(baseValue.Value);
        ElementType = xmlObject.Root.Name.ToString();

        DataValues = new StormElementData(xmlObject.Root);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    internal StormElement(StormElement baseValue)
    {
        _originalXmlElements = [.. baseValue._originalXmlElements];

        using (XmlObject xmlObject = XmlParser.Parse(OriginalXmlElements[0].Value))
        {
            DataValues = new StormElementData(xmlObject.Root);
        }

        for (int i = 1; i < OriginalXmlElements.Count; i++)
        {
            using XmlObject xmlObject = XmlParser.Parse(OriginalXmlElements[i].Value);
            AddToDataValues(xmlObject.Root);
        }

        ElementType = baseValue.ElementType;
    }

    /// <summary>
    /// Gets a collection of all the original xml elements.
    /// </summary>
    public IReadOnlyList<StormXmlValuePath> OriginalXmlElements => _originalXmlElements.AsReadOnly();

    /// <summary>
    /// Gets the data values.
    /// </summary>
    public StormElementData DataValues { get; private set; }

    /// <summary>
    /// Gets the element type.
    /// </summary>
    public string ElementType { get; }

    /// <summary>
    /// Gets the value of the id attribute.
    /// </summary>
    public string? Id
    {
        get
        {
            if (HasId)
                return DataValues.ElementDataPairs[IdAttribute].RawValue;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets the value of the parent attribute.
    /// </summary>
    public string? ParentId
    {
        get
        {
            if (HasParentId)
                return DataValues.ElementDataPairs[ParentAttribute].RawValue;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Id"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Id))]
    public bool HasId => DataValues.ElementDataPairs.ContainsKey(IdAttribute);

    /// <summary>
    /// Gets a value indicating whether <see cref="ParentId"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ParentId))]
    public bool HasParentId => DataValues.ElementDataPairs.ContainsKey(ParentAttribute);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasId && HasParentId)
                display = $"{{<{ElementType} id=\"{Id}\" parent=\"{ParentId}\">}}";
            else if (HasId && !HasParentId)
                display = $"{{<{ElementType} id=\"{Id}\">}}";
            else if (!HasId && HasParentId)
                display = $"{{<{ElementType} parent=\"{ParentId}\">}}";
            else
                display = $"{{<{ElementType}>}}";

            return display;
        }
    }

    /// <summary>
    /// Gets a collection of all the inner elements and attributes.
    /// </summary>
    /// <returns>A collection of all the inner xml data as <see cref="StormElementData"/>.</returns>
    public IEnumerable<StormElementData> GetElements()
    {
        return DataValues.GetElements();
    }

    /// <summary>
    /// Merges a xml element into this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormXmlValuePath">An xml element along with it's file path.</param>
    public void AddValue(StormXmlValuePath stormXmlValuePath)
    {
        _originalXmlElements.Add(stormXmlValuePath);

        using XmlObject xmlObject = XmlParser.Parse(stormXmlValuePath.Value);
        AddToDataValues(xmlObject.Root);
    }

    /// <summary>
    /// Merges another <see cref="StormElement"/> in this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormElement">Another <see cref="StormElement"/>.</param>
    public void AddValue(StormElement stormElement)
    {
        _originalXmlElements.AddRange(stormElement.OriginalXmlElements);

        foreach (StormXmlValuePath item in stormElement.OriginalXmlElements)
        {
            using XmlObject xmlObject = XmlParser.Parse(item.Value);
            AddToDataValues(xmlObject.Root);
        }
    }

    private void AddToDataValues(XmlNode xmlNode)
    {
        DataValues.AddElement(xmlNode);
    }
}
