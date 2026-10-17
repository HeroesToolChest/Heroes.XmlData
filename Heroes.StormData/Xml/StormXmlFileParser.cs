namespace Heroes.StormData.Xml;

internal sealed class StormXmlFileParser
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

            StormXmlMember stormXmlMember = ParseElement(reader);

            if (stormXmlMember.Name.Equals(StormXmlConstants.ConstElementName, StringComparison.Ordinal))
                yield return new StormXmlConstElement(stormXmlMember, stormFile);
            else
                yield return new StormXmlElement(stormXmlMember, stormFile);
        }
    }

    private static void ParseTokenInstruction(ReadOnlySpan<char> content, StormXmlMember rootMember)
    {
        string? id = null;
        string? type = null;
        string? value = null;

        while (TryReadPseudoAttribute(ref content, out ReadOnlySpan<char> name, out string attributeValue))
        {
            if (name.SequenceEqual(StormXmlConstants.IdAttributeName))
                id = attributeValue;
            else if (name.SequenceEqual(StormXmlConstants.TokenTypeAttributeName))
                type = attributeValue;
            else if (name.SequenceEqual(StormXmlConstants.ValueAttributeName))
                value = attributeValue;
        }

        rootMember.AddToken(new StormXmlToken(id ?? string.Empty, type ?? string.Empty, value ?? string.Empty));
    }

    // Reads the next name="value" pair and advances 'text' past it.
    private static bool TryReadPseudoAttribute(ref ReadOnlySpan<char> text, out ReadOnlySpan<char> name, out string value)
    {
        name = default;
        value = string.Empty;

        text = text.TrimStart();

        int equalsIndex = text.IndexOf('=');

        if (equalsIndex < 0)
            return false;

        ReadOnlySpan<char> remainingText = text[(equalsIndex + 1)..].TrimStart();

        if (remainingText.IsEmpty || (remainingText[0] != '"'))
            return false;

        char quote = remainingText[0];

        remainingText = remainingText[1..];

        int closingIndex = remainingText.IndexOf(quote);

        if (closingIndex < 0)
            return false;

        name = text[..equalsIndex].Trim();
        text = remainingText[(closingIndex + 1)..];
        value = remainingText[..closingIndex].ToString();

        return true;
    }

    private StormXmlMember ParseElement(XmlReader reader, StormXmlMember? rootMember = null)
    {
        string elementName = reader.LocalName;
        bool isArray = false;

        StormXmlMember stormXmlMember = new(elementName);
        rootMember ??= stormXmlMember;

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
                    stormXmlMember.Add(name, ComputeConstantValue(value, indexOfConstant), rootMember);
                }
                else
                {
                    stormXmlMember.Add(name, value, rootMember);
                }
            }
            while (reader.MoveToNextAttribute());

            reader.MoveToElement(); // back to the element node
        }

        stormXmlMember.IsArray = isArray;

        if (isEmptyElement)
        {
            reader.Read(); // consume <Element ... />, done
            return stormXmlMember;
        }

        reader.Read(); // consume the start tag <Element ...>

        while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // Peek the child's index attribute without moving the reader
                string? indexValue = reader.GetAttribute(StormXmlConstants.IndexAttributeName);

                stormXmlMember.Add(ParseElement(reader, rootMember), indexValue);
            }
            else if (reader.NodeType == XmlNodeType.ProcessingInstruction && reader.Name.Equals(StormXmlConstants.TokenInstructionName, StringComparison.Ordinal))
            {
                // <?token id="heroid" type="CHeroLink" value="Bogus"?>
                ParseTokenInstruction(reader.Value, rootMember);

                reader.Read();
            }
            else
            {
                reader.Read();
            }
        }

        reader.Read(); // consume the end tag </Element>

        return stormXmlMember;
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
