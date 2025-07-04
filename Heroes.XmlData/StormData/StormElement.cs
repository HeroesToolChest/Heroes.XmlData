namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an top level <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElement
{
    private const string IdAttribute = "id";
    private const string ParentAttribute = "parent";
    private readonly List<StormXElementValuePath> _originalXElements = [];

    private bool _isCurrentDefault = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    internal StormElement(StormXElementValuePath baseValue)
    {
        _originalXElements.Add(baseValue);
        ElementType = baseValue.Value.Name.LocalName;

        DataValues = new StormElementData(this, baseValue.Value);
        DefaultDataValues = DataValues;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    internal StormElement(StormElement baseValue)
    {
        _originalXElements = [.. baseValue.OriginalXElements];
        DataValues = new StormElementData(this, OriginalXElements[0].Value);
        DefaultDataValues = DataValues;

        for (int i = 1; i < OriginalXElements.Count; i++)
        {
            AddToDataValues(OriginalXElements[i].Value);
        }

        ElementType = baseValue.ElementType;
    }

    /// <summary>
    /// Gets a collection of all the original <see cref="XElement"/>s.
    /// </summary>
    public IReadOnlyList<StormXElementValuePath> OriginalXElements => _originalXElements.AsReadOnly();

    /// <summary>
    /// Gets the data values.
    /// </summary>
    public StormElementData DataValues { get; private set; }

    /// <summary>
    /// <para>
    /// Gets the data values associated with the last set element with the "default" attribute set to "1".
    /// </para>
    /// <para>
    /// This can be either the element with the "default" attribute set to "1" if there is no child element or the first child element that inherits it.
    /// </para>
    /// </summary>
    public StormElementData DefaultDataValues { get; private set; }

    /// <summary>
    /// Gets the element type.
    /// </summary>
    public string ElementType { get; private set; }

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
    /// Merges a <see cref="XElement"/> into this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormXElementValue">An <see cref="XElement"/> along with its file path.</param>
    public void AddValue(StormXElementValuePath stormXElementValue)
    {
        _originalXElements.Add(stormXElementValue);

        AddToDataValues(stormXElementValue.Value);
    }

    /// <summary>
    /// Merges another <see cref="StormElement"/> in this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormElement">Another <see cref="StormElement"/>.</param>
    public void AddValue(StormElement stormElement)
    {
        _originalXElements.AddRange(stormElement.OriginalXElements);

        foreach (StormXElementValuePath item in stormElement.OriginalXElements)
        {
            AddToDataValues(item.Value);
        }
    }

    private void AddToDataValues(XElement xElement)
    {
        string? defaultValue = xElement.Attribute("default")?.Value ?? xElement.Attribute("Default")?.Value;
        if (!string.IsNullOrWhiteSpace(defaultValue) && defaultValue == "1")
        {
            DefaultDataValues = new StormElementData(this, xElement);
            _isCurrentDefault = true;
        }
        else if (_isCurrentDefault)
        {
            DefaultDataValues = new StormElementData(this, xElement);
            _isCurrentDefault = false;
        }

        DataValues.AddXElement(xElement);
        ElementType = xElement.Name.LocalName;
    }
}
