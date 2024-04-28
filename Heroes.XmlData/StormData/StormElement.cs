namespace Heroes.XmlData.StormData;

internal class StormElement
{
    public StormElement(StormXElementValuePath baseValue)
    {
        OriginalStormXElementValues.Add(baseValue);
        DataValues = new StormElementData(baseValue.Value);
    }

    public StormElement(StormElement baseValue)
    {
        OriginalStormXElementValues.AddRange(baseValue.OriginalStormXElementValues);

        foreach (StormXElementValuePath valuePath in baseValue.OriginalStormXElementValues)
        {
            DataValues = new StormElementData(valuePath.Value);
        }
    }

    public StormElementData? DataValues { get; private set; }

    public List<StormXElementValuePath> OriginalStormXElementValues { get; } = [];

    public void AddValue(StormXElementValuePath stormXElementValue)
    {
        OriginalStormXElementValues.Add(stormXElementValue);

        if (DataValues is null)
            DataValues = new StormElementData(stormXElementValue.Value);
        else
            DataValues.AddXElement(stormXElementValue.Value);
    }

    //private void Test(XElement rootElement)
    //{
    //    //Dictionary<string, string> keyValuePairs = [];

    //    IEnumerable<XAttribute> attributes = rootElement.Attributes();
    //    IEnumerable<XElement> elements = rootElement.Elements();

    //    foreach (XAttribute attribute in attributes)
    //    {
    //        _keyValueDataPairs[attribute.Name.LocalName] = new StormXElementData(attribute.Value);
    //    }

    //    foreach (XElement element in elements)
    //    {
    //        string elementName = element.Name.LocalName;
    //        string? value = element.Attribute("value")?.Value;
    //        string? index = element.Attribute("index")?.Value;

    //        if (!string.IsNullOrEmpty(index))
    //            _keyValueDataPairs[elementName] = new StormXElementData(index, element);
    //        else
    //            _keyValueDataPairs[elementName] = new StormXElementData("0", element);
    //    }
    //}


    //private static void MergeValues(XElement currentElement, XElement addingRootElement)
    //{
    //    MergeAttributes(currentElement, addingRootElement);
    //    MergeElements(currentElement, addingRootElement);
    //}

    //private static void MergeAttributes(XElement currentElement, XElement addingElement)
    //{
    //    IEnumerable<XAttribute> addingAttributes = addingElement.Attributes();

    //    foreach (XAttribute attribute in addingAttributes)
    //    {
    //        XAttribute? currentAttribute = currentElement.Attribute(attribute.Name);
    //        if (currentAttribute is null)
    //        {
    //            currentElement.Add(attribute);
    //        }
    //        else if (!string.IsNullOrEmpty(attribute.Value))
    //        {
    //            currentAttribute.SetValue(attribute.Value);
    //        }
    //    }
    //}

    //private static void MergeElements(XElement currentElement, XElement addingRootElement)
    //{
    //    IEnumerable<XElement> addingElements = addingRootElement.Elements();
    //    foreach (XElement addingElement in addingElements)
    //    {
    //        // check if the element name already exists
    //        XElement? element = currentElement.Element(addingElement.Name);
    //        if (element is null)
    //        {
    //            currentElement.Add(addingElement);
    //        }
    //        else
    //        {
    //            // check if the element already exists based on the index
    //            string? addingIndexValue = addingElement.Attribute("index")?.Value;
    //            string? currentIndexValue = element.Attribute("index")?.Value;

    //            if (currentIndexValue is not null && currentIndexValue.Equals(addingIndexValue, StringComparison.OrdinalIgnoreCase))
    //            {
    //                // it does, so merge the attributes
    //                MergeAttributes(element, addingElement);

    //                // if has child elements, keep merging
    //                if (addingElement.HasElements)
    //                {
    //                    MergeElements(element, addingElement);
    //                }
    //            }
    //            else
    //            {
    //                currentElement.Add(addingElement);
    //            }
    //        }
    //    }
    //}
}
