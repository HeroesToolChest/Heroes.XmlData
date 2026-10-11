using System.Runtime.InteropServices;

namespace Heroes.StormData.GameStrings;

internal ref struct TextParser
{
    private const string ScalingNumbersTag = "<c val=\"#ColorGray\">";
    private const int ScalingNumbersTagLength = 20;

    private readonly DataTagParser _dataTagParser;
    private readonly ReadOnlySpan<char> _text;

    private int _startingIndex;
    private int _index;

    public TextParser(DataTagParser dataTagParser, ReadOnlySpan<char> text)
    {
        _dataTagParser = dataTagParser;
        _text = text;
    }

    public void ConstructTextStack(List<TextSectionItem> textStack)
    {
        _startingIndex = _index;

        while (_index < _text.Length)
        {
            if (_text[_index] == '<' && _index + 2 < _text.Length && (_text[_index + 1] == 'd' || _text[_index + 1] == 'D') && _text[_index + 2] == ' ')
            {
                PushNormalText(textStack);

                if (TryParseTagContents(out Range? tag))
                {
                    PushValueScaleFromTag(tag.Value, textStack);
                }

                _startingIndex = _index;
            }
            else
            {
                _index++;
            }
        }

        PushNormalText(textStack);
    }

    public readonly string BuildDescription(List<TextSectionItem> textStack)
    {
        if (textStack.Count < 1)
            return string.Empty;

        int totalSize = GetSizeOfBuffer(textStack);

        Span<char> buffer = totalSize < 1024 ? stackalloc char[totalSize] : new char[totalSize];

        int currentOffset = 0;

        // loop through and build string
        foreach (ref readonly TextSectionItem item in CollectionsMarshal.AsSpan(textStack))
        {
            if (item.Type == TextSectionType.Text)
            {
                ReadOnlySpan<char> itemText = _text[item.Range];

                itemText.CopyTo(buffer[currentOffset..]);
                currentOffset += itemText.Length;
            }
            else if (item.Type == TextSectionType.Value)
            {
                ValueScale value = item.ValueScale;

                value.Value.TryFormat(buffer[currentOffset..], out int charsWritten);
                currentOffset += charsWritten;

                if (value.Scaling.HasValue)
                {
                    if (item.IsPercent)
                    {
                        "%".CopyTo(buffer[currentOffset..]);
                        currentOffset += 1;
                    }

                    $"{ScalingNumbersTag}~~".CopyTo(buffer[currentOffset..]);
                    currentOffset += ScalingNumbersTagLength + 2;

                    value.Scaling.Value.TryFormat(buffer[currentOffset..], out charsWritten);
                    currentOffset += charsWritten;

                    "~~</c>".CopyTo(buffer[currentOffset..]);
                    currentOffset += 6;
                }
            }
        }

        // remove any null chars at the end
        return buffer[..currentOffset].ToString();
    }

    private static int GetSizeOfBuffer(List<TextSectionItem> textStack)
    {
        int sum = 0;

        foreach (ref readonly TextSectionItem current in CollectionsMarshal.AsSpan(textStack))
        {
            if (current.Type == TextSectionType.Value)
            {
                sum += current.ValueScale.TotalValueDigits();
                sum += current.ValueScale.TotalScalingDigits() + 4; // +4 for ~~ ~~
                sum += ScalingNumbersTagLength + 4; // +4 for close tag

                if (current.IsPercent)
                    sum += 1; // for %
            }
            else if (current.Type == TextSectionType.Text)
            {
                sum += current.Range.End.Value - current.Range.Start.Value;
            }
        }

        return sum;
    }

    private readonly void PushNormalText(List<TextSectionItem> textStack)
    {
        int normalTextLength = _index - _startingIndex;
        if (normalTextLength > 0)
        {
#if DEBUG
            ReadOnlySpan<char> temp = _text.Slice(_startingIndex, normalTextLength);
#endif
            textStack.Add(new TextSectionItem(new Range(_startingIndex, _index)));
        }
    }

    private bool TryParseTagContents([NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = _text[_index..];
        int lengthOffset = _text.Length - currentTextSpan.Length;

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

    private void PushValueScaleFromTag(Range tag, List<TextSectionItem> textStack)
    {
        ReadOnlySpan<char> span = _text[tag];

        ValueScale valueScale = _dataTagParser.ParseDataTag(span);

        if (valueScale.Scaling.HasValue && _index < _text.Length && _text[_index] == '%')
        {
            textStack.Add(new TextSectionItem(valueScale, true));
            _index++;
        }
        else
        {
            textStack.Add(new TextSectionItem(valueScale));
        }
    }
}
