using System.Diagnostics;

namespace Heroes.XmlData.StormData;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData
{
    private static HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications"];

    public StormElementData(string value)
    {
        Value = value;
    }

    public StormElementData(XElement rootElement)
    {
       Parse(rootElement);
    }

    public StormElementData(string index, XElement element)
    {
        KeyValueDataPairs[index] = new StormElementData(element);
    }

    public Dictionary<string, StormElementData> KeyValueDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    [MemberNotNullWhen(true, nameof(ConstValue))]
    public bool HasConstValue => ConstValue is not null;

    public string? Value { get; private set; }

    public string? ConstValue { get; private set; }

    public bool IsArray { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display = $"Count = {KeyValueDataPairs.Count}";

            if (HasValue)
                return $"Value = \"{Value}\", {display}";
            else
                return display;
        }
    }

    public void AddXElement(XElement element)
    {
        Parse(element);
    }

    private void Parse(XElement rootElement)
    {
        IEnumerable<XAttribute> attributes = rootElement.Attributes();
        IEnumerable<XElement> elements = rootElement.Elements();

        foreach (XAttribute attribute in attributes)
        {
            if (attribute.Name.LocalName.Equals("index", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(attribute.Value))
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

            KeyValueDataPairs[attribute.Name.LocalName] = new StormElementData(attribute.Value);
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;

            if (!string.IsNullOrEmpty(indexAtt))
            {
                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? outerValue))
                {
                    if (outerValue.KeyValueDataPairs.TryGetValue(indexAtt, out StormElementData? innerValue))
                        innerValue.AddXElement(element);
                    else
                        outerValue.KeyValueDataPairs[indexAtt] = new StormElementData(element);
                }
                else
                {
                    KeyValueDataPairs[elementName] = new StormElementData(indexAtt, element)
                    {
                        IsArray = true,
                    };
                }
            }
            else
            {
                // check if existing
                if (KeyValueDataPairs.TryGetValue(elementName, out StormElementData? value))
                {
                    if (value.IsArray)
                    {
                        string nextIndex = value.KeyValueDataPairs.Keys.Count.ToString();

                        value.KeyValueDataPairs[nextIndex] = new StormElementData(element);
                    }
                    else
                    {
                        value.AddXElement(element);
                    }
                }
                else
                {
                    if (elementName.EndsWith("array", StringComparison.OrdinalIgnoreCase) || _otherElementArrays.Contains(elementName))
                    {
                        KeyValueDataPairs[elementName] = new StormElementData("0", element)
                        {
                            IsArray = true,
                        };
                    }
                    else
                    {
                        KeyValueDataPairs[elementName] = new StormElementData(element);
                    }
                }
            }
        }
    }
}
