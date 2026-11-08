using System.Runtime.CompilerServices;

namespace Heroes.StormData.GameStrings;

/// <summary>
/// Replaces the data tags (&lt;d ... /&gt;) in a gamestring with their values, in one pass over the text.
/// </summary>
internal readonly ref struct TextParser
{
    private const string ScalingStart = "<c val=\"#ColorGray\">~~";
    private const string ScalingEnd = "~~</c>";

    // most gamestrings fit in this, longer ones grow into a buffer rented from the array pool
    private const int StackBufferLength = 512;

    private readonly DataTagParser _dataTagParser;
    private readonly ReadOnlySpan<char> _text;

    // the gamestring locale's format, used for the values and the scaling (~~x~~) written in the text
    private readonly IFormatProvider _formatProvider;

    public TextParser(DataTagParser dataTagParser, ReadOnlySpan<char> text, IFormatProvider formatProvider)
    {
        _dataTagParser = dataTagParser;
        _text = text;
        _formatProvider = formatProvider;
    }

    /// <summary>
    /// Parses the text, replacing each data tag with its value and scaling.
    /// </summary>
    /// <returns>The parsed text.</returns>
    public string Parse()
    {
        int tagStart = IndexOfDataTag(0);

        // nothing to replace
        if (tagStart < 0)
            return _text.ToString();

        DefaultInterpolatedStringHandler builder = new(0, 0, _formatProvider, stackalloc char[StackBufferLength]);
        int textStart = 0;

        while (tagStart >= 0)
        {
            builder.AppendFormatted(_text[textStart..tagStart]);

            // the tag ends at the first '>', unless another tag starts first
            int endOffset = _text[(tagStart + 1)..].IndexOfAny('<', '>');
            int end = endOffset < 0 ? _text.Length : tagStart + 1 + endOffset;

            if (end < _text.Length && _text[end] == '>')
            {
                int tagEnd = end + 1;
                ValueScale valueScale = _dataTagParser.ParseDataTag(_text[tagStart..tagEnd]);

                // a % right after a scaling value goes before the scaling
                bool isPercent = valueScale.Scaling.HasValue && tagEnd < _text.Length && _text[tagEnd] == '%';

                AppendValue(ref builder, valueScale, isPercent);

                textStart = isPercent ? tagEnd + 1 : tagEnd;
            }
            else
            {
                // an unfinished tag is dropped, up to where the next tag starts or the text ends
                textStart = end;
            }

            tagStart = IndexOfDataTag(textStart);
        }

        builder.AppendFormatted(_text[textStart..]);

        return builder.ToStringAndClear();
    }

    /// <summary>
    /// Appends <paramref name="valueScale"/> to <paramref name="builder"/>, in the builder's format, with the scaling (if any) after the value.
    /// </summary>
    /// <param name="builder">The builder to append to.</param>
    /// <param name="valueScale">The value and scaling to append.</param>
    /// <param name="isPercent">Whether a % is written between the value and its scaling. Only used if there is a scaling.</param>
    internal static void AppendValue(ref DefaultInterpolatedStringHandler builder, ValueScale valueScale, bool isPercent)
    {
        MathHelpers.AppendNumber(ref builder, valueScale.Value);

        if (valueScale.Scaling is not double scaling)
            return;

        if (isPercent)
            builder.AppendLiteral("%");

        builder.AppendLiteral(ScalingStart);
        MathHelpers.AppendNumber(ref builder, scaling);
        builder.AppendLiteral(ScalingEnd);
    }

    // finds the next "<d " (or "<D "), starting at start
    private int IndexOfDataTag(int start)
    {
        while (start < _text.Length)
        {
            int offset = _text[start..].IndexOf('<');
            if (offset < 0)
                return -1;

            int index = start + offset;

            if (index + 2 < _text.Length && (_text[index + 1] == 'd' || _text[index + 1] == 'D') && _text[index + 2] == ' ')
                return index;

            start = index + 1;
        }

        return -1;
    }
}
