using System.Runtime.CompilerServices;

namespace Heroes.StormData.GameStrings;

/// <summary>
/// Replaces the data tags (&lt;d ... /&gt;) in a gamestring with their values, in one pass over the text.
/// </summary>
internal sealed class GameStringParser
{
    private const string ScalingStart = "<c val=\"#ColorGray\">~~";
    private const string ScalingEnd = "~~</c>";

    // most gamestrings fit in this, longer ones grow into a buffer rented from the array pool
    private const int StackBufferLength = 512;

    private readonly DataTagParser _dataTagParser;

    public GameStringParser(LayeredDataStorage layeredDataStorage)
    {
        _dataTagParser = new DataTagParser(layeredDataStorage);
    }

    /// <summary>
    /// Parses the data tags in <paramref name="text"/>, writing their numbers in the format of <paramref name="stormLocale"/>.
    /// </summary>
    /// <param name="text">The gamestring text.</param>
    /// <param name="stormLocale">The locale of the gamestring.</param>
    /// <returns>The parsed text.</returns>
    public string ParseText(ReadOnlySpan<char> text, StormLocale stormLocale)
    {
        int tagStart = IndexOfDataTag(text, 0);

        // nothing to replace
        if (tagStart < 0)
            return text.ToString();

        // the gamestring locale's format, used for the values and the scaling (~~x~~) written in the text
        DefaultInterpolatedStringHandler builder = new(0, 0, StormLocaleCulture.GetCultureInfo(stormLocale), stackalloc char[StackBufferLength]);
        int textStart = 0;

        while (tagStart >= 0)
        {
            builder.AppendFormatted(text[textStart..tagStart]);

            // the tag ends at the first '>', unless another tag starts first
            int endOffset = text[(tagStart + 1)..].IndexOfAny('<', '>');
            int end = endOffset < 0 ? text.Length : tagStart + 1 + endOffset;

            if (end < text.Length && text[end] == '>')
            {
                int tagEnd = end + 1;
                ValueScale valueScale = _dataTagParser.ParseDataTag(text[tagStart..tagEnd]);

                // a % right after a scaling value goes before the scaling
                bool isPercent = valueScale.Scaling.HasValue && tagEnd < text.Length && text[tagEnd] == '%';

                AppendValue(ref builder, valueScale, isPercent);

                textStart = isPercent ? tagEnd + 1 : tagEnd;
            }
            else
            {
                // an unfinished tag is dropped, up to where the next tag starts or the text ends
                textStart = end;
            }

            tagStart = IndexOfDataTag(text, textStart);
        }

        builder.AppendFormatted(text[textStart..]);

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

    // finds the next "<d " (or "<D ") in text, starting at start
    private static int IndexOfDataTag(ReadOnlySpan<char> text, int start)
    {
        while (start < text.Length)
        {
            int offset = text[start..].IndexOf('<');
            if (offset < 0)
                return -1;

            int index = start + offset;

            if (index + 2 < text.Length && (text[index + 1] == 'd' || text[index + 1] == 'D') && text[index + 2] == ' ')
                return index;

            start = index + 1;
        }

        return -1;
    }
}
