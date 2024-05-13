using System.Globalization;
using System.Text;
using U8Xml;

namespace Heroes.XmlData.GameStrings;

internal class GameStringParser
{
    public const int MaxNumberLength = 9;
    public const int MaxScalingLength = 6;
    public const double MaxValueSize = 999_999_999;

    private readonly HeroesData _heroesData;
    private readonly DataRefParser _dataRefParser;

    private readonly List<ITextSection> _textStack = [];
    private int _startingIndex = 0;
    private int _index = 0;

    private GameStringParser(HeroesData heroesData)
    {
        _heroesData = heroesData;

        _dataRefParser = new DataRefParser(this, _heroesData);
    }

    public static string ParseTooltipDescription(ReadOnlySpan<char> description, HeroesData heroesData)
    {
        GameStringParser gameStringParser = new(heroesData);

        gameStringParser.ConstructTextStack(description);

        return gameStringParser.BuildDescription(description);
    }

    // <d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/>
    // <d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>
    public ValueScale ParseDataTag(ReadOnlySpan<char> dataTag)
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

            ValueScale valueScale = _dataRefParser.Parse(buffer);
            resultValue = valueScale.Value;
            scaling = valueScale.Scaling;
        }

        if (scaling.HasValue)
            return new ValueScale(Math.Round(resultValue, precision), Math.Round(scaling.Value, MaxScalingLength));
        else
            return new ValueScale(Math.Round(resultValue, precision));
    }

    // <c val=\"#TooltipNumbers\">
    private static bool TryParseAttributeValRange(ReadOnlySpan<char> styleTag, [NotNullWhen(true)] out Range? attributeValValue)
    {
        attributeValValue = null;

        int indexOfVal = styleTag.IndexOf("val=", StringComparison.OrdinalIgnoreCase);
        if (indexOfVal < 0)
            return false;

        int startIndexOfQuote = styleTag.IndexOf("\"");
        int endIndeoxOfQuote = styleTag[(startIndexOfQuote + 1)..].IndexOf("\"");

        attributeValValue = new Range(startIndexOfQuote + 1, startIndexOfQuote + endIndeoxOfQuote);

        return true;
    }

    private string BuildDescription(ReadOnlySpan<char> description)
    {
        if (_textStack.Count < 1)
            return string.Empty;

        int totalSize = GetSizeOfBuffer();

        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : new char[totalSize];

        int currentOffset = 0;

        // loop through and build string
        for (int i = 0; i < _textStack.Count; i++)
        {
            ITextSection item = _textStack[i];

            if (item.Type == TextSectionType.Text)
            {
                TextSection textSection = (TextSection)item;

                ReadOnlySpan<char> itemText = description[textSection.Range];

                itemText.CopyTo(buffer[currentOffset..]);
                currentOffset += itemText.Length;
            }
            else if (item.Type == TextSectionType.Value)
            {
                TextSectionValueScale textSectionValueScale = (TextSectionValueScale)item;

                ValueScale value = textSectionValueScale.ValueScale;

                value.Value.TryFormat(buffer[currentOffset..], out int charsWritten);
                currentOffset += charsWritten;

                if (value.Scaling.HasValue)
                {
                    value.Scaling.Value.TryFormat(buffer[currentOffset..], out charsWritten, format: $"~~{value.Scaling.Value}~~", CultureInfo.InvariantCulture);
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
            if (description[_index] == '<' && _index + 1 < description.Length && description[_index + 1] == 'd')
            {
#if DEBUG
                PushNormalText(description);
#else
                PushNormalText();
#endif
                if (TryParseTagContents(description, out Range? tag))
                {
                    PushValueScaleFromTag(description, tag.Value);
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
            _textStack.Add(new TextSection(new Range(_startingIndex, _index)));

        }
    }
#endif

    private bool TryParseTagContents(ReadOnlySpan<char> description, [NotNullWhen(true)] out Range? tag)
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

    private void PushValueScaleFromTag(ReadOnlySpan<char> description, Range tag)
    {
        ReadOnlySpan<char> span = description[tag];

        ValueScale valueScale = ParseDataTag(span);

        _textStack.Add(new TextSectionValueScale(valueScale));
    }

    private int GetSizeOfBuffer()
    {
        int sum = 0;

        foreach (ITextSection current in _textStack)
        {
            if (current.Type == TextSectionType.Value)
            {
                TextSectionValueScale textSectionValueScale = (TextSectionValueScale)current;

                sum += MaxNumberLength;

                if (textSectionValueScale.ValueScale.Scaling.HasValue)
                    sum += MaxScalingLength;
            }
            else if (current.Type == TextSectionType.Text)
            {
                TextSection textSection = (TextSection)current;

                sum += textSection.Range.End.Value - textSection.Range.Start.Value;
            }
        }

        return sum;
    }
}
