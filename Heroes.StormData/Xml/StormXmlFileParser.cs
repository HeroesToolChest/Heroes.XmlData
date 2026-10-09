using Heroes.StormData.StormMath;

namespace Heroes.StormData.Xml;

internal class StormXmlFileParser
{
    private static readonly SearchValues<char> _constantVariableSearch = SearchValues.Create(" ,.;");

    private readonly LayeredDataStorage _layeredDataStorage;

    public StormXmlFileParser(LayeredDataStorage layeredDataStorage)
    {
        _layeredDataStorage = layeredDataStorage;
    }

    /// <summary>
    /// Parses the XML file and returns a collection of <see cref="StormXmlElement"/> objects.
    /// </summary>
    /// <param name="reader">The XML reader to read from.</param>
    /// <param name="stormFile">The file associated with the XML elements.</param>
    /// <returns>A collection of <see cref="StormXmlElement"/> objects.</returns>
    public IEnumerable<StormXmlElement> Parse(XmlReader reader, StormFile stormFile)
    {
        reader.MoveToContent(); // <Catalog>
        reader.Read();

        while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.Read();
                continue;
            }

            StormXmlMember stormXmlProperty = ParseElement(reader);

            if (stormXmlProperty.Name.Equals(StormXmlConstants.ConstElementName, StringComparison.Ordinal))
                yield return new StormXmlConstElement(stormXmlProperty, stormFile);
            else
                yield return new StormXmlElement(stormXmlProperty, stormFile);
        }
    }

    private StormXmlMember ParseElement(XmlReader reader)
    {
        string elementName = reader.LocalName;
        bool isArray = false;

        StormXmlMember stormXmlProperty = new(elementName);

        if (elementName.EndsWith("array", StringComparison.OrdinalIgnoreCase) || StormXmlConstants.ElementArrays.Contains(elementName))
            isArray = true;

        // for self-closing tags like <Element ... />
        bool isEmptyElement = reader.IsEmptyElement;

        if (reader.MoveToFirstAttribute())
        {
            do
            {
                bool isIdAttribute = false;
                string name = reader.LocalName;
                string value = reader.Value;

                if (name.Equals(StormXmlConstants.IdAttributeName, StringComparison.Ordinal))
                    isIdAttribute = true;
                else if (!isArray && name.Equals(StormXmlConstants.IndexAttributeName, StringComparison.OrdinalIgnoreCase))
                    isArray = true;

                if (value.Length > 0 && value[0] == '@')
                {
                    // TODO: asset text
                }
                else if (!isIdAttribute && value.IndexOf('$') is int indexOfConstant && indexOfConstant >= 0)
                {
                    stormXmlProperty.Add(name, ComputeConstantValue(value, indexOfConstant));
                }
                else
                {
                    stormXmlProperty.Add(name, value);
                }
            }
            while (reader.MoveToNextAttribute());

            reader.MoveToElement(); // back to the element node
        }

        stormXmlProperty.IsArray = isArray;

        if (isEmptyElement)
        {
            reader.Read(); // consume <Element ... />, done
            return stormXmlProperty;
        }

        reader.Read(); // consume the start tag <Element ...>

        while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // Peek the child's index attribute without moving the reader
                string? indexValue = reader.GetAttribute(StormXmlConstants.IndexAttributeName);

                stormXmlProperty.Add(ParseElement(reader), indexValue);
            }
            else
            {
                reader.Read();
            }
        }

        reader.Read(); // consume the end tag </Element>

        return stormXmlProperty;
    }

    private string ComputeConstantValue(string value, int indexOfConstant)
    {
        // should be a non-const element
        if (indexOfConstant == 0)
        {
            if (_layeredDataStorage.TryGetFirstStormXmlConstElementById(value, out StormXmlConstElement? stormXmlConstElement))
            {
                return "";
            }
        }

        return StormPrefixNotation.Compute(_layeredDataStorage, value).ToString();

        // here it should be a const element so we need to find the constant in the value
        //ReadOnlySpan<char> startOfConstant = value.AsSpan(indexOfConstant);

        //int endIndexOfConst = startOfConstant.IndexOfAny(_constantVariableSearch);

        //ReadOnlySpan<char> constant = endIndexOfConst < 0
        //    ? startOfConstant
        //    : startOfConstant[..endIndexOfConst];

        //if (constant.IsEmpty)
        //    return string.Empty;




        //if (constant[0] == '$')
        //{
        //    if (_layeredDataStorage.TryGetFirstStormXmlConstElementById(constant, out StormXmlConstElement? stormXmlConstElement))
        //    {
        //        //stormXmlConstElement
        //    }
        //}
        //else if (double.TryParse(constant, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
        //{
        //    return result.ToString();
        //}

        //return constant.ToString();
        //return _stormStorage.GetValueFromConstTextAsText(constant);



        //if (text.IsEmpty)
        //    return string.Empty;

        //if (text[0] == '$')
        //{
        //    if (TryGetFirstConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
        //    {
        //        return GetValueFromConstElement(stormXElementValuePath.Value);
        //    }
        //}
        //else if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
        //{
        //    return result.ToString();
        //}

        //return text.ToString();



        //public string GetValueFromConstElement(XElement constElement)
        //{
        //    (string? valueAttribute, bool isExpressionAttribute) = ReadConstAttributes(constElement);

        //    if (string.IsNullOrWhiteSpace(valueAttribute))
        //        return valueAttribute ?? string.Empty;

        //    if (isExpressionAttribute)
        //        return HeroesPrefixNotation.Compute(this, valueAttribute).ToString();

        //    if (double.TryParse(valueAttribute, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        //        return value.ToString();

        //    return valueAttribute;
        //}



    //public double GetValueFromConstTextAsNumber(ReadOnlySpan<char> text)
    //{
    //    if (text.IsEmpty)
    //        return 0;

    //    if (text[0] == '$')
    //    {
    //        if (TryGetFirstConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
    //        {
    //            return GetValueFromConstElementAsNumber(stormXElementValuePath.Value);
    //        }
    //    }
    //    else if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
    //    {
    //        return result;
    //    }

    //    return 0;
    //}
    }
}
