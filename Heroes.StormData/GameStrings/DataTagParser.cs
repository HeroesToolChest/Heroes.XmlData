using System.Text;
using U8Xml;

namespace Heroes.StormData.GameStrings;

internal sealed class DataTagParser
{
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
        double resultValue = 0;
        double? scaling = null;

        try
        {
            using XmlObject xmlDataTag = XmlParser.Parse(dataTag);
            U8Xml.XmlNode xmlRoot = xmlDataTag.Root;
            XmlAttributeList xmlAttributes = xmlRoot.Attributes;

            if (!((xmlAttributes.TryFind("precision", out U8Xml.XmlAttribute precisionAttribute) || xmlAttributes.TryFind("Precision", out precisionAttribute)) &&
                precisionAttribute.Value.TryToInt32(out int precision)))
            {
                precision = 0;
            }

            if (xmlAttributes.TryFind("const", out U8Xml.XmlAttribute constAttribute))
            {
                Span<char> valueBuffer = stackalloc char[constAttribute.Value.GetCharCount()];

                Encoding.UTF8.TryGetChars(constAttribute.Value.AsSpan(), valueBuffer, out int charsWritten);

                if (_layeredDataStorage.TryGetFirstStormXmlConstElementById(valueBuffer, out StormXmlConstElement? stormXmlConstElement))
                {
                    resultValue = stormXmlConstElement.GetValueAsDouble();
                }
            }
            else if (xmlAttributes.TryFind("ref", out U8Xml.XmlAttribute refAttribute))
            {
                Span<char> buffer = stackalloc char[refAttribute.Value.GetCharCount()];

                Encoding.UTF8.TryGetChars(refAttribute.Value.AsSpan(), buffer, out int charsWritten);

                ValueScale valueScale = _dataRefAttributeParser.Parse(buffer);
                resultValue = valueScale.Value;
                scaling = valueScale.Scaling;
            }

            if (scaling.HasValue)
                return new ValueScale(Math.Round(resultValue, precision), Math.Round(scaling.Value, StormCalculator.MaxFractionalDigits));
            else
                return new ValueScale(Math.Round(resultValue, precision));
        }
        catch (Exception)
        {
            return new ValueScale(0);
        }
    }
}
