using System.Text;
using U8Xml;

namespace Heroes.StormData.GameStrings;

internal sealed class DataTagParser
{
    public const string AttributeNameConst = "const";
    public const string AttributeNameRef = "ref";
    public const string AttributeNamePrecision = "precision";

    private readonly LayeredDataStorage _layeredDataStorage;
    private readonly DataRefAttributeParser _dataRefAttributeParser;

    public DataTagParser(LayeredDataStorage layeredDataStorage)
    {
        _layeredDataStorage = layeredDataStorage;
        _dataRefAttributeParser = new DataRefAttributeParser(layeredDataStorage, this);
    }

    // <d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/>
    // <d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>
    public ValueScale ParseDataTag(ReadOnlySpan<char> dataTag)
    {
        // a simple tag has its attributes read directly, which is much faster than parsing it as xml.
        if (dataTag.Length >= 4 && dataTag[0] == '<' && dataTag.EndsWith("/>") &&
            SimpleDataTagReader.TryRead(dataTag[1..^2], out DataTagAttributes attributes))
        {
            return GetValueScale(attributes);
        }

        return ParseXmlDataTag(dataTag);
    }

    // [d ref='Behavior,AbathurToxicNestEnvenomedNest,PeriodCount' player='0'/], a data tag inside a dref
    public ValueScale ParseBracketDataTag(ReadOnlySpan<char> bracketDataTag)
    {
        if (bracketDataTag.IsEmpty)
            return new ValueScale(0);

        if (bracketDataTag.Length >= 4 && bracketDataTag[0] == '[' && bracketDataTag.EndsWith("/]") &&
            SimpleDataTagReader.TryRead(bracketDataTag[1..^2], out DataTagAttributes attributes))
        {
            return GetValueScale(attributes);
        }

        // not simple, so parse it as the xml tag it would be, [d .../] to <d .../>
        Span<char> xmlDataTag = stackalloc char[bracketDataTag.Length];
        bracketDataTag.CopyTo(xmlDataTag);
        xmlDataTag[0] = '<';
        xmlDataTag[^1] = '>';

        return ParseXmlDataTag(xmlDataTag);
    }

    // parses the tag as xml, for the tags that aren't simple.
    // like SimpleDataTagReader, attribute names ignore case and the first attribute of a name is used
    internal ValueScale ParseXmlDataTag(ReadOnlySpan<char> dataTag)
    {
        try
        {
            using XmlObject xmlDataTag = XmlParser.Parse(dataTag);

            RawString constValue = default;
            RawString refValue = default;
            bool hasConst = false;
            bool hasRef = false;
            bool hasPrecision = false;
            int precision = 0;

            foreach (U8Xml.XmlAttribute attribute in xmlDataTag.Root.Attributes)
            {
                // the name is utf-8, which Ascii compares with the char constants directly
                ReadOnlySpan<byte> name = attribute.Name.AsSpan();

                if (!hasConst && Ascii.EqualsIgnoreCase(name, AttributeNameConst))
                {
                    constValue = attribute.Value;
                    hasConst = true;
                }
                else if (!hasRef && Ascii.EqualsIgnoreCase(name, AttributeNameRef))
                {
                    refValue = attribute.Value;
                    hasRef = true;
                }
                else if (!hasPrecision && Ascii.EqualsIgnoreCase(name, AttributeNamePrecision))
                {
                    if (!attribute.Value.TryToInt32(out precision))
                        precision = 0;

                    hasPrecision = true;
                }
            }

            if (hasConst)
            {
                Span<char> valueBuffer = stackalloc char[constValue.GetCharCount()];

                Encoding.UTF8.TryGetChars(constValue.AsSpan(), valueBuffer, out int charsWritten);

                return GetValueScale(DataTagAttributes.FromConst(valueBuffer[..charsWritten], precision));
            }
            else if (hasRef)
            {
                Span<char> buffer = stackalloc char[refValue.GetCharCount()];

                Encoding.UTF8.TryGetChars(refValue.AsSpan(), buffer, out int charsWritten);

                return GetValueScale(DataTagAttributes.FromRef(buffer[..charsWritten], precision));
            }

            return new ValueScale(0);
        }
        catch (Exception)
        {
            return new ValueScale(0);
        }
    }

    // the const is used over the ref when a tag has both
    private ValueScale GetValueScale(DataTagAttributes attributes)
    {
        try
        {
            if (attributes.HasConst)
            {
                double value = 0;

                if (_layeredDataStorage.TryGetFirstStormXmlConstElementById(attributes.ConstId, out StormXmlConstElement? stormXmlConstElement))
                    value = stormXmlConstElement.GetValueAsDouble();

                return new ValueScale(Math.Round(value, attributes.Precision));
            }

            if (attributes.HasRef)
            {
                ValueScale valueScale = _dataRefAttributeParser.Parse(attributes.Ref);

                if (valueScale.Scaling.HasValue)
                    return new ValueScale(Math.Round(valueScale.Value, attributes.Precision), Math.Round(valueScale.Scaling.Value, StormCalculator.MaxFractionalDigits));
                else
                    return new ValueScale(Math.Round(valueScale.Value, attributes.Precision));
            }

            return new ValueScale(0);
        }
        catch (Exception)
        {
            // e.g. a precision that Math.Round doesn't allow, or a ref that isn't a valid expression
            return new ValueScale(0);
        }
    }
}
