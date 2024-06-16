using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data that represents an <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData : IXmlData
{
    private static readonly HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications", "ConditionalEvents", "CardLayouts"];

    private string? _value;
    private string? _constValue;

    internal StormElementData(XElement rootElement)
    {
        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, bool isArray = false)
    {
        Parent = parent;

        if (!string.IsNullOrWhiteSpace(parent.Field))
        {
            if (isArray)
                Field = $"{parent.Field}[{field}]";
            else
                Field = $"{parent.Field}.{field}";
        }
        else
        {
            Field = field;
        }
    }

    internal StormElementData(StormElementData parent, string field, XElement rootElement)
        : this(parent, field)
    {
        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, string value, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        Value = value;
    }

    internal StormElementData(StormElementData parent, string field, string value, string constValue)
        : this(parent, field)
    {
        Value = value;
        ConstValue = constValue;
    }

    internal StormElementData(StormElementData parent, string field, XElement rootElement, bool isInnerArray = false, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        Parse(rootElement, isInnerArray);
    }

    internal StormElementData(StormElementData parent, string field, XElement element, string index, bool isInnerArray = false)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, element, isInnerArray, true);
    }

    internal StormElementData(StormElementData parent, string field, XAttribute attribute, string index)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, attribute.Value, true);
    }

    /// <summary>
    /// Gets the parent data.
    /// </summary>
    public StormElementData? Parent { get; }

    /// <summary>
    /// Gets the representation of the current data reference field.
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    /// <summary>
    /// Gets a value indicating whether <see cref="ConstValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ConstValue))]
    public bool HasConstValue => ConstValue is not null;

    /// <summary>
    /// Gets a value indicating whether <see cref="ScaleValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ScaleValue))]
    public bool HasHxdScale => ElementDataPairs.Count == 1 && ElementDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    /// <summary>
    /// Gets the value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? Value
    {
        get
        {
            if (_value is not null)
            {
                return _value;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    return data.Value;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    if (firstElementData.HasValue)
                        return firstElementData.Value;
                }
            }

            return null;
        }

        private set => _value = value;
    }

    /// <summary>
    /// Gets the evaluated value of the constant if <see cref="Value"/> contains a constant (starts with $).
    /// </summary>
    public string? ConstValue
    {
        get
        {
            if (_constValue is not null)
            {
                return _constValue;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasConstValue)
                {
                    return data.ConstValue;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    if (firstElementData.HasConstValue)
                        return firstElementData.ConstValue;
                }
            }

            return null;
        }
        private set => _constValue = value;
    }

    /// <summary>
    /// Gets the scaling value.
    /// </summary>
    public string? ScaleValue
    {
        get
        {
            if (HasHxdScale)
            {
                return ElementDataPairs[ScaleValueParser.ScaleAttributeName].Value;
            }

            return null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of numerical keys.
    /// </summary>
    public bool HasNumericalIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of text keys.
    /// </summary>
    public bool HasTextIndex { get; init; }

    /// <inheritdoc/>
    public int XmlDataCount => ElementDataPairs.Count;

    /// <summary>
    /// Gets the inner data.
    /// </summary>
    internal Dictionary<string, StormElementData> ElementDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display = $"Count = {ElementDataPairs.Count}";

            if (HasValue)
            {
                return $"Value = \"{Value}\", {display}";
            }
            else if (HasConstValue)
            {
                return $"ConstValue = \"{ConstValue}\", {display}";
            }
            else
            {
                if (HasNumericalIndex)
                    return $"{display}, IsNumericalIndex";
                else if (HasTextIndex)
                    return $"{display}, IsTextIndex";
                else
                    return $"{display}";
            }
        }
    }

    /// <inheritdoc/>
    public StormElementData GetXmlData(ReadOnlySpan<char> index)
    {
        if (ElementDataPairs.TryGetValue(index.ToString(), out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException();
    }

    /// <inheritdoc/>
    public StormElementData GetXmlData(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        if (ElementDataPairs.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException();
    }

    /// <inheritdoc/>
    public bool TryGetXmlData(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        return TryGetXmlData(index.ToString(), out stormElementData);
    }

    /// <inheritdoc/>
    public bool TryGetXmlData(string index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.TryGetValue(index, out stormElementData);
    }

    /// <inheritdoc/>
    public IEnumerable<StormElementData> GetXmlData()
    {
        foreach (StormElementData data in ElementDataPairs.Values)
        {
            yield return data;
        }
    }

    internal void AddXElement(XElement element, bool isInnerArray = false)
    {
        Parse(element, isInnerArray);
    }

    private void Parse(XElement rootElement, bool isInnerArray = false)
    {
        IEnumerable<XAttribute> attributes = rootElement.Attributes();
        IEnumerable<XElement> elements = rootElement.Elements();

        foreach (XAttribute attribute in attributes)
        {
            if (attribute.Name.LocalName.Equals("index", StringComparison.OrdinalIgnoreCase))
                continue;

            if (attribute.Name.LocalName.Equals("value", StringComparison.OrdinalIgnoreCase))
            {
                Value = attribute.Value;
                continue;
            }

            if (attribute.Name.LocalName.Equals($"{StormModStorage.SelfNameConst}value", StringComparison.OrdinalIgnoreCase))
            {
                ConstValue = attribute.Value;
                continue;
            }

            if (isInnerArray)
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute, "0")
                {
                    HasNumericalIndex = true,
                };
            }
            else
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute.Value);
            }
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;
            string? valueAtt = element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value;

            if (!string.IsNullOrEmpty(indexAtt))
            {
                if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingElementData))
                {
                    if (existingElementData.ElementDataPairs.TryGetValue(indexAtt, out StormElementData? existingIndexedData))
                    {
                        existingIndexedData.AddXElement(element, true);
                    }
                    else
                    {
                        existingElementData.ElementDataPairs[indexAtt] = new StormElementData(existingElementData, indexAtt, element, true, true);
                    }
                }
                else
                {
                    bool numericalIndex = int.TryParse(indexAtt, out _);

                    ElementDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element, indexAtt, true)
                    {
                        HasNumericalIndex = numericalIndex,
                        HasTextIndex = !numericalIndex,
                    };
                }
            }
            else if (isInnerArray || elementName.AsSpan().EndsWith("array", StringComparison.OrdinalIgnoreCase) || _otherElementArrays.Contains(elementName))
            {
                if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
                {
                    string nextIndex = existingData.ElementDataPairs.Keys.Count.ToString();

                    existingData.ElementDataPairs[nextIndex] = new StormElementData(existingData, nextIndex, element, true, true);
                }
                else
                {
                    ElementDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element, "0", true)
                    {
                        HasNumericalIndex = true,
                    };
                }
            }
            else if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
            {
                existingData.AddXElement(element);
            }
            else if (string.IsNullOrEmpty(valueAtt))
            {
                ElementDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element);
            }
            else
            {
                string? constValueAtt = element.Attribute($"{StormModStorage.SelfNameConst}value")?.Value ?? element.Attribute($"{StormModStorage.SelfNameConst}Value")?.Value;

                if (string.IsNullOrEmpty(constValueAtt))
                    ElementDataPairs[elementName] = new StormElementData(this, elementName, valueAtt);
                else
                    ElementDataPairs[elementName] = new StormElementData(this, elementName, valueAtt, constValueAtt);
            }
        }
    }
}
