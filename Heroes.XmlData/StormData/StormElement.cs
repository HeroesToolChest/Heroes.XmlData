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
    private readonly Dictionary<string, XElement> _processingInstructionsById = [];

    private bool _isDefault = true;


    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    internal StormElement(StormXElementValuePath baseValue)
    {
        ProcessingInstructionsById = _processingInstructionsById.GetAlternateLookup<ReadOnlySpan<char>>();

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
        ProcessingInstructionsById = _processingInstructionsById.GetAlternateLookup<ReadOnlySpan<char>>();

        _originalXElements = [.. baseValue.OriginalXElements];

        ElementType = baseValue.ElementType;
        DataValues = new StormElementData(this, OriginalXElements[0].Value);

        for (int i = 1; i < OriginalXElements.Count; i++)
        {
            AddToDataValues(OriginalXElements[i].Value);
        }

        DefaultDataValues = DataValues;
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

    /// <summary>
    /// Gets a value indicating whether the default attribute is set to 1.
    /// </summary>
    public bool IsDefault => _isDefault;

    /// <summary>
    /// Gets the processing instructions by the id in <see cref="XElement"/> form.
    /// </summary>
    public Dictionary<string, XElement>.AlternateLookup<ReadOnlySpan<char>> ProcessingInstructionsById { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToXElement().ToString();

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

    /// <summary>
    /// Creates an <see cref="XElement"/> that represents the merged values from <see cref="DataValues"/>.
    /// <para>
    /// This will not contain a parent attribute.
    /// </para>
    /// <para>
    /// All element replacements, e.g. ##id##, will be evaluated.
    /// </para>
    /// </summary>
    /// <returns>An <see cref="XElement"/> containing the merged data values.</returns>
    public XElement ToXElement()
    {
        XElement rootElement = new(ElementType);

        // Add id attribute first if it exists
        if (DataValues.ElementDataPairs.TryGetValue("id", out StormElementData? idData) && idData.HasValue)
        {
            rootElement.SetAttributeValue("id", idData.Value.GetString());
        }

        BuildXElementFromData(rootElement, DataValues, true);

        return rootElement;
    }

    private static void BuildXElementFromData(XElement parentElement, StormElementData stormElementData, bool isRootElement = false)
    {
        var elementData = stormElementData.GetElementData();

        foreach (KeyValuePair<string, StormElementData> element in elementData)
        {
            string key = element.Key;
            StormElementData childData = element.Value;

            // Skip id on root element as it's already added first and skip parent attribute
            if (isRootElement && (key.Equals("id", StringComparison.OrdinalIgnoreCase) || key.Equals("parent", StringComparison.OrdinalIgnoreCase)))
                continue;

            if (childData.HasValue && childData.ElementDataCount == 0)
            {
                // This is a simple attribute or element with a value
                if (childData.IsIndexed)
                {
                    // This is an indexed element within an array
                    XElement childElement = new(childData.Parent?.Field ?? key);
                    childElement.SetAttributeValue("value", childData.Value.GetString());
                    childElement.SetAttributeValue("index", key);
                    parentElement.Add(childElement);
                }
                else if (isRootElement && IsRootAttribute(key))
                {
                    // This is a root-level attribute that should remain an attribute
                    parentElement.SetAttributeValue(key, childData.Value.GetString());
                }
                else
                {
                    // Convert this attribute to a child element with a value attribute
                    XElement childElement = new(key);
                    childElement.SetAttributeValue("value", childData.Value.GetString());

                    parentElement.Add(childElement);
                }
            }
            else if (childData.ElementDataCount > 0)
            {
                // This is a complex element with nested data
                if (childData.IsIndexed)
                {
                    // This is an indexed array element
                    XElement childElement = new(childData.Parent?.Field ?? key);
                    childElement.SetAttributeValue("index", key);

                    BuildXElementFromData(childElement, childData);
                    parentElement.Add(childElement);
                }
                else if (IsArrayElement(childData))
                {
                    // This is an array container, add it's indexed children
                    var elementDataArray = childData.GetElementData();

                    foreach (KeyValuePair<string, StormElementData> elementArray in elementDataArray)
                    {
                        XElement arrayElement = new(key);
                        arrayElement.SetAttributeValue("index", elementArray.Key);

                        // check if no elements, if not and has value, then set value attribute
                        if (elementArray.Value.ElementDataCount == 0 && elementArray.Value.HasValue)
                            arrayElement.SetAttributeValue("value", elementArray.Value.Value.GetString());
                        else if (elementArray.Value.HasValue)
                            arrayElement.Add(CreateValueXElement(elementArray.Value.Value.GetString()));

                        BuildXElementFromData(arrayElement, elementArray.Value);

                        parentElement.Add(arrayElement);
                    }
                }
                else if (childData.HasValue)
                {
                    // This is a complex element with a value and nested data
                    XElement childElement = new(key);

                    XElement valueElement = CreateValueXElement(childData.Value.GetString());

                    childElement.Add(valueElement);

                    BuildXElementFromData(childElement, childData);
                    parentElement.Add(childElement);
                }
                else
                {
                    // This is a regular nested element
                    XElement childElement = new(key);

                    BuildXElementFromData(childElement, childData);
                    parentElement.Add(childElement);
                }
            }
        }
    }

    private static XElement CreateValueXElement(string? value)
    {
        XElement valueElement = new("Value");
        valueElement.SetAttributeValue("value", value);
        return valueElement;
    }

    // if root element, then keep it as an root attribute
    private static bool IsRootAttribute(string attributeName)
    {
        return attributeName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
               attributeName.Equals("parent", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsArrayElement(StormElementData data)
    {
        // Check if all children are indexed (numeric or string indices)
        foreach (string index in data.GetElementDataIndexes())
        {
            StormElementData indexedData = data.GetElementDataAt(index);
            if (!indexedData.IsIndexed)
                return false;
        }

        return data.ElementDataCount > 0;
    }

    private void AddToDataValues(XElement xElement)
    {
        string? defaultValue = xElement.Attribute("default")?.Value ?? xElement.Attribute("Default")?.Value;
        if (!string.IsNullOrWhiteSpace(defaultValue) && defaultValue == "1")
        {
            DefaultDataValues = new StormElementData(this, xElement);
            _isDefault = true;
        }
        else if (_isDefault)
        {
            DefaultDataValues = new StormElementData(this, xElement);
            _isDefault = false;
        }

        DataValues.AddXElement(xElement);
        ElementType = xElement.Name.LocalName;
    }
}
