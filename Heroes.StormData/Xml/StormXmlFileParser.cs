using System.Diagnostics;

namespace Heroes.StormData.Xml;

internal class StormXmlFileParser
{
    private static readonly SearchValues<char> _constantVariableSearch = SearchValues.Create(" ,.;");

    private readonly ElementCache _elementCache;
    //private readonly StormAttributeVariables _stormAttributeVariables;

    public StormXmlFileParser(ElementCache elementCache)
    {
        _elementCache = elementCache;
       // _stormAttributeVariables = stormAttributeVariables;
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

            yield return StormXmlElement.Create(ParseElement(reader), stormFile);

            //if (reader.LocalName.Equals(StormXmlConstants.ConstElementName, StringComparison.Ordinal))
            //    ParseConstants(reader);
            //else
            //    ParseElement(reader);
        }
    }

    private StormXmlProperty ParseElement(XmlReader reader)
    {
        string elementName = reader.LocalName;
        bool isArray = false;

        StormXmlProperty stormXmlProperty = new(elementName);

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
        ReadOnlySpan<char> startOfConstant = value.AsSpan(indexOfConstant);

        int endIndexOfConst = startOfConstant.IndexOfAny(_constantVariableSearch);

        ReadOnlySpan<char> constant = endIndexOfConst < 0
            ? startOfConstant
            : startOfConstant[..endIndexOfConst];

        return "";
    }
}
