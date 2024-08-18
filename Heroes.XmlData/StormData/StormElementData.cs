using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data that represents an <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData
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
        RawValue = value;
    }

    internal StormElementData(StormElementData parent, string field, string value, string constValue)
        : this(parent, field)
    {
        RawValue = value;
        _constValue = constValue;
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
    /// Gets a value indicating whether <see cref="Value"/> is evaluated from a const.
    /// </summary>
    public bool IsConstValue
    {
        get
        {
            if (_constValue is not null)
            {
                return true;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.IsConstValue)
                {
                    return data.IsConstValue;
                }
                else if (HasTextIndex)
                {
                    return ElementDataPairs.First().Value.IsConstValue;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="HxdScaleValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(HxdScaleValue))]
    public bool HasHxdScale => ElementDataPairs.Count == 1 && ElementDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    /// <summary>
    /// Gets the original value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? RawValue
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
                    return data.RawValue;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    if (firstElementData.HasValue)
                        return firstElementData.RawValue;
                }
            }

            return null;
        }

        private set => _value = value;
    }

    /// <summary>
    /// Gets the evaluated value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? Value
    {
        get
        {
            if (_constValue is not null)
            {
                return _constValue;
            }
            else if (_value is not null)
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
                    return ElementDataPairs.First().Value.Value;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the scaling value.
    /// </summary>
    public string? HxdScaleValue
    {
        get
        {
            if (HasHxdScale)
            {
                return ElementDataPairs[ScaleValueParser.ScaleAttributeName].RawValue;
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

    /// <summary>
    /// Gets the amount of elements.
    /// </summary>
    public int ElementDataCount => ElementDataPairs.Count;

    /// <summary>
    /// Gets the inner data.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal Dictionary<string, StormElementData> ElementDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display = $"Count = {ElementDataPairs.Count}";

            if (HasValue)
            {
                return $"{Field}, Value = \"{Value}\", {display}";
            }
            else
            {
                if (HasNumericalIndex)
                    return $"{Field}, {display}, IsNumericalIndex";
                else if (HasTextIndex)
                    return $"{Field}, {display}, IsTextIndex";
                else
                    return $"{Field}, {display}";
            }
        }
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(ReadOnlySpan<char> index)
    {
        if (ElementDataPairs.TryGetValue(index.ToString(), out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException();
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        if (ElementDataPairs.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException();
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetElementDataAt(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        return TryGetElementDataAt(index.ToString(), out stormElementData);
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    public bool TryGetElementDataAt(string index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.TryGetValue(index, out stormElementData);
    }

    internal IEnumerable<StormElementData> GetElements()
    {
        foreach (StormElementData data in ElementDataPairs.Values)
        {
            if (data.ElementDataPairs.Count == 0)
            {
                yield return data;
            }
            else
            {
                foreach (StormElementData innerData in data.GetElements())
                {
                    yield return innerData;
                }
            }
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
                RawValue = attribute.Value;
                continue;
            }

            if (attribute.Name.LocalName.Equals($"{StormModStorage.SelfNameConst}value", StringComparison.OrdinalIgnoreCase))
            {
                _constValue = attribute.Value;
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
