namespace Heroes.XmlData.StormData;

public class StormElementData
{
    private static readonly HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications", "ConditionalEvents"];

    private string? _value;
    private string? _constValue;

    public StormElementData(XElement rootElement)
    {
        Parse(rootElement);
    }

    public StormElementData(string value)
    {
        Value = value;
    }

    public StormElementData(string value, string constValue)
    {
        Value = value;
        ConstValue = constValue;
    }

    public StormElementData(XElement rootElement, bool isInnerArray = false)
    {
        Parse(rootElement, isInnerArray);
    }

    public StormElementData(XElement element, string index, bool isInnerArray = false)
    {
        KeyValueDataPairs[index] = new StormElementData(element, isInnerArray);
    }

    public StormElementData(XAttribute attribute, string index)
    {
        KeyValueDataPairs[index] = new StormElementData(attribute.Value);
    }

    public Dictionary<string, StormElementData> KeyValueDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    [MemberNotNullWhen(true, nameof(ConstValue))]
    public bool HasConstValue => ConstValue is not null;

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
                return _constValue;
            else if (KeyValueDataPairs.Keys.Count == 1 && KeyValueDataPairs.TryGetValue("0", out StormElementData? data) && data.HasConstValue)
                return data.ConstValue;
            else
                return null;
        }
        private set => _constValue = value;
    }

    public bool HasNumericalIndex { get; init; }

    public bool HasTextIndex { get; init; }

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
                KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(attribute, "0")
                {
                    HasNumericalIndex = true,
                };
            }
            else
            {
                KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(attribute.Value);
            }
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;
            string? valueAtt = element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value;

            if (!string.IsNullOrEmpty(indexAtt))
            {
                bool numericalIndex = int.TryParse(indexAtt, out _);
                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? existingElementData))
                {
                    if (existingElementData.KeyValueDataPairs.TryGetValue(indexAtt, out StormElementData? existingIndexedData))
                    {
                        existingIndexedData.AddXElement(element, true);
                    }
                    else
                    {
                        existingElementData.KeyValueDataPairs[indexAtt] = new StormElementData(element, true);
                    }
                }
                else
                {
                    KeyValueDataPairs[elementName] = new StormElementData(element, indexAtt, true)
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

                    existingData.KeyValueDataPairs[nextIndex] = new StormElementData(element, true);
                }
                else
                {
                    KeyValueDataPairs[elementName] = new StormElementData(element, "0", true)
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
                KeyValueDataPairs[elementName] = new StormElementData(element);
            }
            else
            {
                string? constValueAtt = element.Attribute($"{StormModStorage.SelfNameConst}value")?.Value ?? element.Attribute($"{StormModStorage.SelfNameConst}Value")?.Value;

                if (string.IsNullOrEmpty(constValueAtt))
                    KeyValueDataPairs[elementName] = new StormElementData(valueAtt);
                else
                    KeyValueDataPairs[elementName] = new StormElementData(valueAtt, constValueAtt);
            }
        }
    }
}
//[DebuggerDisplay("{DebuggerDisplay,nq}")]
//public class StormElementData
//{
//    private static readonly HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications"];

//    public StormElementData(string value)
//    {
//        Value = value;
//    }

//    public StormElementData(XElement rootElement)
//    {
//       Parse(rootElement);
//    }

//    public StormElementData(XElement rootElement, bool innerArray)
//    {
//        Parse(rootElement, innerArray);
//    }

//    public StormElementData(string index, XElement element)
//    {
//        KeyValueDataPairs[index] = new StormElementData(element);
//    }

//    public StormElementData(string index, XElement element, bool innerArray)
//    {
//        KeyValueDataPairs[index] = new StormElementData(element, innerArray);
//    }

//    public Dictionary<string, StormElementData> KeyValueDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

//    [MemberNotNullWhen(true, nameof(Value))]
//    public bool HasValue => Value is not null;

//    [MemberNotNullWhen(true, nameof(ConstValue))]
//    public bool HasConstValue => ConstValue is not null;

//    public string? Value { get; private set; }

//    public string? ConstValue { get; private set; }

//    public bool IsArray { get; private set; }

//    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
//    private string DebuggerDisplay
//    {
//        get
//        {
//            string display = $"Count = {KeyValueDataPairs.Count}";

//            if (HasValue)
//                return $"Value = \"{Value}\", {display}";
//            else
//                return display;
//        }
//    }

//    public void AddXElement(XElement element)
//    {
//        Parse(element);
//    }

//    private void Parse(XElement rootElement, bool innerArray = false)
//    {
//        IEnumerable<XAttribute> attributes = rootElement.Attributes();
//        IEnumerable<XElement> elements = rootElement.Elements();

//        foreach (XAttribute attribute in attributes)
//        {
//            if (attribute.Name.LocalName.Equals("index", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(attribute.Value))
//                continue;

//            if (attribute.Name.LocalName.Equals("value", StringComparison.OrdinalIgnoreCase))
//            {
//                Value = attribute.Value;
//                continue;
//            }

//            if (attribute.Name.LocalName.Equals($"{StormModStorage.SelfNameConst}value", StringComparison.OrdinalIgnoreCase))
//            {
//                ConstValue = attribute.Value;
//                continue;
//            }

//            KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(attribute.Value);
//        }

//        foreach (XElement element in elements)
//        {
//            string elementName = element.Name.LocalName;
//            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;

//            if (!string.IsNullOrEmpty(indexAtt))
//            {
//                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? outerValue))
//                {
//                    if (outerValue.KeyValueDataPairs.TryGetValue(indexAtt, out StormElementData? innerValue))
//                        innerValue.AddXElement(element);
//                    else
//                        outerValue.KeyValueDataPairs[indexAtt] = new StormElementData(element);
//                }
//                else
//                {
//                    KeyValueDataPairs[elementName] = new StormElementData(indexAtt, element, true)
//                    {
//                        IsArray = true,
//                    };
//                }
//            }
//            else
//            {
//                // check if existing
//                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? value))
//                {
//                    if (value.IsArray)
//                    {
//                        string nextIndex = value.KeyValueDataPairs.Keys.Count.ToString();

//                        value.KeyValueDataPairs[nextIndex] = new StormElementData(element);
//                    }
//                    else
//                    {
//                        value.AddXElement(element);
//                    }
//                }
//                else
//                {
//                    if (innerArray || elementName.EndsWith("array", StringComparison.OrdinalIgnoreCase) || _otherElementArrays.Contains(elementName))
//                    {
//                        KeyValueDataPairs[elementName] = new StormElementData("0", element, true)
//                        {
//                            IsArray = true,
//                        };
//                    }
//                    else
//                    {
//                        KeyValueDataPairs[elementName] = new StormElementData(element);
//                    }
//                }
//            }
//        }
//    }
//}
