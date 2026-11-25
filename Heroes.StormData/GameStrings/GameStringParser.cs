using System.Runtime.CompilerServices;

namespace Heroes.StormData.GameStrings;

/// <summary>
/// Replaces the data tags (&lt;d ... /&gt;) in a gamestring with their values, in one pass over the text.
/// </summary>
internal sealed class GameStringParser
{
    // the scaling is written as <c val="#ColorGray" value="55.3" precision="0" scaling="0.0400390625">~~0.04~~</c>, the attributes are only written if the unrounded value is known
    private const string ScalingTagStart = "<c val=\"#ColorGray\"";
    private const string ValueAttributeStart = " value=\"";
    private const string PrecisionAttributeStart = "\" precision=\"";
    private const string ScalingAttributeStart = "\" scaling=\"";
    private const string AttributeEnd = "\"";
    private const string ScalingStart = ">~~";
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

        // the gamestring locale's format, used for the values written in the text (the scaling tag is always in the invariant culture)
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
    /// Appends <paramref name="valueScale"/> to <paramref name="builder"/>, with the scaling (if any) after the value.
    /// </summary>
    /// <remarks>
    /// The value is what the tooltip shows, in the builder's format. The scaling (~~x~~) is not shown by the game, it is for other tools to read,
    /// so it is always in the invariant culture. To work out the value at a level, the scaling tag also has the unrounded value (value),
    /// the decimal places the value is rounded to (precision), and the scaling the game stores (scaling), also in the invariant culture.
    /// These are only written if the unrounded value is known.
    /// </remarks>
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

        builder.AppendLiteral(ScalingTagStart);

        if (valueScale.UnroundedValue is double unroundedValue && valueScale.Precision is int precision)
        {
            builder.AppendLiteral(ValueAttributeStart);
            MathHelpers.AppendInvariantNumber(ref builder, unroundedValue);
            builder.AppendLiteral(PrecisionAttributeStart);
            MathHelpers.AppendInvariantNumber(ref builder, precision);
            builder.AppendLiteral(ScalingAttributeStart);
            MathHelpers.AppendInvariantStormFixedNumber(ref builder, scaling);
            builder.AppendLiteral(AttributeEnd);
        }

        builder.AppendLiteral(ScalingStart);
        MathHelpers.AppendInvariantNumber(ref builder, scaling);
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
