namespace Heroes.StormData.GameStrings;

/// <summary>
/// Reads the attributes of a simple data tag directly, which is much faster than parsing it as xml.
/// </summary>
internal static class SimpleDataTagReader
{
    // the characters xml counts as whitespace
    private const string XmlWhitespace = " \t\r\n";

    /// <summary>
    /// Reads the tag name and attributes of a simple data tag, e.g. <c>d ref='...' player='0'</c> from <c>&lt;d ref='...' player='0'/&gt;</c>.
    /// <para>
    /// Simple means a tag name of <c>d</c>, attribute names of only letters, and values in ' or " that have no &lt; or &amp; (which xml would treat specially).
    /// The tag and attribute names ignore case, and the first attribute of a name is used.
    /// </para>
    /// </summary>
    /// <param name="tagContent">The tag name and attributes, without the opening &lt; and the closing /&gt;.</param>
    /// <param name="attributes">When this method returns <see langword="true"/>, contains the attributes of the tag.</param>
    /// <returns><see langword="true"/> if it is a simple data tag; otherwise <see langword="false"/>, so it can be parsed as xml instead.</returns>
    public static bool TryRead(ReadOnlySpan<char> tagContent, out DataTagAttributes attributes)
    {
        attributes = default;

        int indexOfAttributes = tagContent.IndexOfAny(XmlWhitespace);

        ReadOnlySpan<char> tagName = indexOfAttributes < 0 ? tagContent : tagContent[..indexOfAttributes];
        if (!tagName.Equals("d", StringComparison.OrdinalIgnoreCase))
            return false;

        ReadOnlySpan<char> remainingAttributes = indexOfAttributes < 0 ? default : tagContent[indexOfAttributes..];

        ReadOnlySpan<char> constId = default;
        ReadOnlySpan<char> dRef = default;
        ReadOnlySpan<char> precisionText = default;
        bool hasConst = false;
        bool hasRef = false;
        bool hasPrecision = false;

        while (TryReadNextAttribute(ref remainingAttributes, out ReadOnlySpan<char> name, out ReadOnlySpan<char> value))
        {
            if (!IsSimpleAttribute(name, value))
                return false;

            if (!hasConst && name.Equals(DataTagParser.AttributeNameConst, StringComparison.OrdinalIgnoreCase))
            {
                constId = value;
                hasConst = true;
            }
            else if (!hasRef && name.Equals(DataTagParser.AttributeNameRef, StringComparison.OrdinalIgnoreCase))
            {
                dRef = value;
                hasRef = true;
            }
            else if (!hasPrecision && name.Equals(DataTagParser.AttributeNamePrecision, StringComparison.OrdinalIgnoreCase))
            {
                precisionText = value;
                hasPrecision = true;
            }
        }

        // stopped at something that isn't an attribute
        if (!remainingAttributes.IsEmpty)
            return false;

        if (!TryParseLeadingInt32(precisionText, out int precision))
            precision = 0;

        attributes = new DataTagAttributes(hasConst, constId, hasRef, dRef, precision);

        return true;
    }

    private static bool IsSimpleAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        foreach (char character in name)
        {
            if (!char.IsAsciiLetter(character))
                return false;
        }

        return !value.ContainsAny('<', '&');
    }

    // reads the next name='value' or name="value" attribute at the start of text, then moves text past it.
    // returns false when there are no more, and leaves text as it is if what's left isn't an attribute
    private static bool TryReadNextAttribute(scoped ref ReadOnlySpan<char> text, out ReadOnlySpan<char> name, out ReadOnlySpan<char> value)
    {
        name = default;
        value = default;

        ReadOnlySpan<char> remaining = text.TrimStart(XmlWhitespace);

        int indexOfEquals = remaining.IndexOf('=');
        if (indexOfEquals < 1)
        {
            text = remaining;
            return false;
        }

        ReadOnlySpan<char> attributeName = remaining[..indexOfEquals].TrimEnd(XmlWhitespace);
        if (attributeName.IsEmpty || attributeName.ContainsAny(XmlWhitespace))
        {
            text = remaining;
            return false;
        }

        ReadOnlySpan<char> quotedValue = remaining[(indexOfEquals + 1)..].TrimStart(XmlWhitespace);
        if (quotedValue.IsEmpty || quotedValue[0] is not ('\'' or '"'))
        {
            text = remaining;
            return false;
        }

        int indexOfClosingQuote = quotedValue[1..].IndexOf(quotedValue[0]);
        if (indexOfClosingQuote < 0)
        {
            text = remaining;
            return false;
        }

        name = attributeName;
        value = quotedValue[1..(indexOfClosingQuote + 1)];
        text = quotedValue[(indexOfClosingQuote + 2)..];

        return true;
    }

    // reads a number the same way as the xml parser's TryToInt32: an optional sign and the digits at the start, ignoring anything after them,
    // e.g. 2.7 is 2. it fails with no digits at the start (including a leading space), or if the number doesn't fit in an int
    private static bool TryParseLeadingInt32(ReadOnlySpan<char> text, out int value)
    {
        int index = 0;

        if (index < text.Length && text[index] is '+' or '-')
            index++;

        int indexOfDigits = index;

        while (index < text.Length && char.IsAsciiDigit(text[index]))
            index++;

        if (index == indexOfDigits)
        {
            value = 0;
            return false;
        }

        return int.TryParse(text[..index], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }
}
