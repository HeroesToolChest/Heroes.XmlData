using Heroes.LocaleText;
using Heroes.XmlData.StormData;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
//using System.Xml;
//using System.Xml.Linq;
using U8Xml;

namespace Heroes.XmlData.GameStrings;

internal class GameStringParser
{
    public const int MaxNumberLength = 9;
    public const double MaxValueSize = 999_999_999;

    private readonly HeroesData _heroesData;

    private readonly List<TextSection> _textStack = [];
    private int _startingIndex = 0;
    private int _index = 0;

    //private CultureInfo? _culture;

    public GameStringParser(HeroesData heroesData, StormLocale gameStringLocale = StormLocale.ENUS)
    {
        _heroesData = heroesData;
        GameStringLocale = gameStringLocale;
    }

    public StormLocale GameStringLocale { get; }

    public string ParseTooltipDescription(ReadOnlySpan<char> description)
    {
        ConstructTextStack(description);

        return BuildDescription(description);
    }

    private string BuildDescription(ReadOnlySpan<char> description)
    {
        if (_textStack.Count < 1)
            return string.Empty;

        int totalSize = GetSizeOfBuffer();

        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : new char[totalSize];

        int currentOffset = 0;

        // loop through and build string
        foreach (TextSection item in _textStack)
        {
            if (item.IsText)
            {
                ReadOnlySpan<char> itemText = description[item.Range.Value];

                itemText.CopyTo(buffer[currentOffset..]);
                currentOffset += itemText.Length;
            }
            else if (item.IsValue)
            {
                //_culture ??= StormLocaleData.GetCultureInfo(GameStringLocale);

                ValueScale value = item.ValueScale.Value;

                value.Value.TryFormat(buffer[currentOffset..], out int charsWritten);
                currentOffset += charsWritten;

                if (value.Scaling.HasValue)
                {
                    value.Scaling.Value.TryFormat(buffer[currentOffset..], out charsWritten, format: $"~~{value.Scaling.Value}~~");
                    currentOffset += charsWritten;
                }
            }
        }

        // remove any null chars at the end
        return buffer.TrimEnd('\0').ToString();
    }

    private void ConstructTextStack(ReadOnlySpan<char> description)
    {
        _startingIndex = _index;

        while (_index < description.Length)
        {
            if (description[_index] == '[' && _index + 1 < description.Length && description[_index + 1] == ' ')
            {
            }
            else if (description[_index] == '<' && _index + 1 < description.Length && description[_index + 1] == 'd')
            {
#if DEBUG
                PushNormalText(description);
#else
                PushNormalText();
#endif
                if (TryParseDataTag(description, out Range? tag))
                {
                    PushValueFromTag(description, tag.Value);
                }

                _startingIndex = _index;
            }
            else
            {
                _index++;
            }
        }

        if (_index <= description.Length)
        {
#if DEBUG
            PushNormalText(description);
#else
            PushNormalText();
#endif
        }
    }

#if DEBUG
    private void PushNormalText(ReadOnlySpan<char> description)
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            ReadOnlySpan<char> temp = description.Slice(_startingIndex, normalTextLength);

            _textStack.Add(new TextSection(new Range(_startingIndex, _index)));
        }
    }
#else
    private void PushNormalText()
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
            _textStack.Add(new TextSection(new Range(_startingIndex, _index), TextSectionType.Text));

        }
    }
#endif

    private bool TryParseDataTag(ReadOnlySpan<char> description, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = description[_index..];
        int lengthOffset = description.Length - currentTextSpan.Length;

        int startTagIndex = 0; // index of <
        int endTagIndex = -1;

        for (int i = 1; i < currentTextSpan.Length; i++)
        {
            if (currentTextSpan[i] == '>')
            {
                endTagIndex = i;
                break;
            }
            else if (currentTextSpan[i] == '<')
            {
                startTagIndex = i;
                break;
            }
        }

        if (endTagIndex > 0)
        {
            tag = new Range(startTagIndex + lengthOffset, endTagIndex + lengthOffset + 1);

            _index += endTagIndex - startTagIndex + 1;

            return true;
        }

        _index += startTagIndex;

        return false;
    }


    private void PushValueFromTag(ReadOnlySpan<char> description, Range tag)
    {
        ReadOnlySpan<char> span = description[tag];

        ValueScale valueScale = ParseDataTag(span);

        _textStack.Add(new TextSection(valueScale));
    }
    //private bool TryParseDataTag(ReadOnlySpan<char> description)
    //{
    //    ReadOnlySpan<char> currentTextSpan = description[_index..];
    //    int lengthOffset = description.Length - currentTextSpan.Length;

    //    int startTagIndex = 0; // index of <
    //    int endTagIndex = -1;

    //    for (int i = 1; i < currentTextSpan.Length; i++)
    //    {
    //        if (currentTextSpan[i] == '>' && currentTextSpan[i - 1] == '/')
    //        {
    //            endTagIndex = i;
    //            break;
    //        }
    //        else if (currentTextSpan[i] == '<')
    //        {
    //            startTagIndex = i;
    //            break;
    //        }
    //    }

    //    if (endTagIndex > 0)
    //    {
    //        ReadOnlySpan<char> tagSpan = currentTextSpan[startTagIndex..(endTagIndex + 1)];

    //        //// check if its a start tag
    //        //if (tagSpan[1] != '/' && tagSpan[^2] != '/')
    //        //    isStartTag = true;
    //        //else
    //        //    isStartTag = false;

    //        //tag = new Range(startTagIndex + lengthOffset, endTagIndex + lengthOffset + 1);

    //        _index += endTagIndex - startTagIndex + 1;

    //        return true;
    //    }

    //    _index += startTagIndex;

    //    return false;
    //}

    // <d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/>
    // <d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>
    private ValueScale ParseDataTag(ReadOnlySpan<char> dataTag)
    {
        double resultValue = 0;
        double? scaling = null;

        using XmlObject xmlDataTag = XmlParser.Parse(dataTag);
        XmlNode xmlRoot = xmlDataTag.Root;
        XmlAttributeList xmlAttributes = xmlRoot.Attributes;

        if (!((xmlAttributes.TryFind("precision", out XmlAttribute precisionAttribute) || xmlAttributes.TryFind("Precision", out precisionAttribute)) &&
            precisionAttribute.Value.TryToInt32(out int precision)))
        {
            precision = 0;
        }

        if (xmlAttributes.TryFind("const", out XmlAttribute constAttribute))
        {
            Span<char> buffer = stackalloc char[constAttribute.Value.GetCharCount()];

            Encoding.UTF8.TryGetChars(constAttribute.Value.AsSpan(), buffer, out int charsWritten);

            if (_heroesData.TryGetConstantXElement(buffer, out StormXElementValuePath? stormXElementValue))
            {
                resultValue = _heroesData.EvaluateConstantElement(stormXElementValue.Value);
            }
        }
        else if (xmlAttributes.TryFind("ref", out XmlAttribute refAttribute))
        {
            Span<char> buffer = stackalloc char[refAttribute.Value.GetCharCount()];

            Encoding.UTF8.TryGetChars(refAttribute.Value.AsSpan(), buffer, out int charsWritten);

            ValueScale valueScale = DataRefParser.Parse(buffer, _heroesData);
            resultValue = valueScale.Value;
            scaling = valueScale.Scaling;
        }

        if (scaling.HasValue)
            return new ValueScale(Math.Round(resultValue, precision), scaling.Value);
        else
            return new ValueScale(Math.Round(resultValue, precision));
    }

    private int GetSizeOfBuffer()
    {
        int sum = 0;

        foreach (TextSection current in _textStack)
        {
            if (current.IsValue)
            {
                sum += MaxNumberLength;

                if (current.ValueScale.Value.Scaling.HasValue)
                    sum += 6;
            }
            else if (current.IsText)
            {
                sum += current.Range!.Value.End.Value - current.Range.Value.Start.Value;
            }
        }

        return sum;
    }

    //private double ParseDataRef(ReadOnlySpan<char> buffer)
    //{
    //    List<TextSection> dataRefParts = [];
    //    // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    //    // tgus - sdkjf + klsdf * sdflkj(234 + 3345)

    //    //int operatorCount = buffer.Count('+');
    //    //operatorCount += buffer.Count('-');
    //    //operatorCount += buffer.Count('*');
    //    //operatorCount += buffer.Count('/');
    //    //operatorCount += buffer.Count('(');
    //    //operatorCount += buffer.Count(')');

    //    //Span<Range> ranges = stackalloc Range[operatorCount + 1];
    //    int startIndex = 0;
    //    while (startIndex < buffer.Length)
    //    {
    //        ReadOnlySpan<char> part = GetNextPart(buffer[startIndex..]);

    //        if (part.IsEmpty)
    //            continue;

    //        if (double.TryParse(part.Trim(), out double value))
    //            dataRefParts.Add(new TextSection(value)); // hardcoded value
    //        else
    //            dataRefParts.Add(new TextSection(10));

    //        dataRefParts.Add(new TextSection(new Range(part.Length - 1, part.Length)));

    //        startIndex += part.Length + 1;
    //    }

    //    return 0;
    //}

    //private static ReadOnlySpan<char> GetNextPart(ReadOnlySpan<char> text)
    //{
    //    int indexOfOperator = text.IndexOfAny(_gameStringOps);
    //    if (indexOfOperator > -1)
    //        return text[..indexOfOperator];
    //    else
    //        return text;
    //}
}
