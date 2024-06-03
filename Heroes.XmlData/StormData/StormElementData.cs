using System.Diagnostics;

namespace Heroes.XmlData.StormData;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData
{
    private static readonly HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications", "ConditionalEvents"];

    private string? _value;
    private string? _constValue;

    public StormElementData(XElement rootElement)
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
        KeyValueDataPairs[index] = new StormElementData(this, index, element, isInnerArray, true);
    }

    internal StormElementData(StormElementData parent, string field, XAttribute attribute, string index)
        : this(parent, field)
    {
        KeyValueDataPairs[index] = new StormElementData(this, index, attribute.Value, true);
    }

    public Dictionary<string, StormElementData> KeyValueDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

    public StormElementData? Parent { get; }

    public string? Field { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    [MemberNotNullWhen(true, nameof(ConstValue))]
    public bool HasConstValue => ConstValue is not null;

    [MemberNotNullWhen(true, nameof(ScaleValue))]
    public bool HasHxdScale => KeyValueDataPairs.Count == 1 && KeyValueDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    public string? Value
    {
        get
        {
            if (_value is not null)
            {
                return _value;
            }
            else if (KeyValueDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && KeyValueDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    return data.Value;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = KeyValueDataPairs.First().Value;
                    if (firstElementData.HasValue)
                        return firstElementData.Value;
                }
            }

            return null;
        }

        private set => _value = value;
    }

    public string? ConstValue
    {
        get
        {
            if (_constValue is not null)
            {
                return _constValue;
            }
            else if (KeyValueDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && KeyValueDataPairs.TryGetValue("0", out StormElementData? data) && data.HasConstValue)
                {
                    return data.ConstValue;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = KeyValueDataPairs.First().Value;
                    if (firstElementData.HasConstValue)
                        return firstElementData.ConstValue;
                }
            }

            return null;
        }
        private set => _constValue = value;
    }

    public string? ScaleValue
    {
        get
        {
            if (HasHxdScale)
            {
                return KeyValueDataPairs[ScaleValueParser.ScaleAttributeName].Value;
            }

            return null;
        }
    }

    public bool HasNumericalIndex { get; init; }

    public bool HasTextIndex { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display = $"Count = {KeyValueDataPairs.Count}";

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

    public void AddXElement(XElement element, bool isInnerArray = false)
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
                KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute, "0")
                {
                    HasNumericalIndex = true,
                };
            }
            else
            {
                KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute.Value);
            }
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;
            string? valueAtt = element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value;

            if (!string.IsNullOrEmpty(indexAtt))
            {
                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? existingElementData))
                {
                    if (existingElementData.KeyValueDataPairs.TryGetValue(indexAtt, out StormElementData? existingIndexedData))
                    {
                        existingIndexedData.AddXElement(element, true);
                    }
                    else
                    {
                        existingElementData.KeyValueDataPairs[indexAtt] = new StormElementData(existingElementData, indexAtt, element, true, true);
                    }
                }
                else
                {
                    bool numericalIndex = int.TryParse(indexAtt, out _);

                    KeyValueDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element, indexAtt, true)
                    {
                        HasNumericalIndex = numericalIndex,
                        HasTextIndex = !numericalIndex,
                    };
                }
            }
            else if (isInnerArray || elementName.AsSpan().EndsWith("array", StringComparison.OrdinalIgnoreCase) || _otherElementArrays.Contains(elementName))
            {
                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? existingData))
                {
                    string nextIndex = existingData.KeyValueDataPairs.Keys.Count.ToString();

                    existingData.KeyValueDataPairs[nextIndex] = new StormElementData(existingData, nextIndex, element, true, true);
                }
                else
                {
                    KeyValueDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element, "0", true)
                    {
                        HasNumericalIndex = true,
                    };
                }
            }
            else if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? existingData))
            {
                existingData.AddXElement(element);
            }
            else if (string.IsNullOrEmpty(valueAtt))
            {
                KeyValueDataPairs[elementName] = new StormElementData(this, element.Name.LocalName, element);
            }
            else
            {
                string? constValueAtt = element.Attribute($"{StormModStorage.SelfNameConst}value")?.Value ?? element.Attribute($"{StormModStorage.SelfNameConst}Value")?.Value;

                if (string.IsNullOrEmpty(constValueAtt))
                    KeyValueDataPairs[elementName] = new StormElementData(this, elementName, valueAtt);
                else
                    KeyValueDataPairs[elementName] = new StormElementData(this, elementName, valueAtt, constValueAtt);
            }
        }
    }
}
