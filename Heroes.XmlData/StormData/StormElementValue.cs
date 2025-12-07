namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents a storm element's value.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly ref struct StormElementValue
{
    private readonly StormElementData _defaultStormElementData;

    internal StormElementValue(StormElementData stormElementData)
    {
        // move to parent
        _defaultStormElementData = stormElementData.StormElement.DefaultDataValues;
    }

    internal ReadOnlySpan<char> Value { get; init; }

    internal bool IsNull { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? DebuggerDisplay => IsNull ? null : $"\"{Value}\"";

    /// <summary>
    /// Gets the current value as a <see cref="string"/>.
    /// </summary>
    /// <returns>The value as a <see cref="string"/>.</returns>
    public readonly string GetString()
    {
        return ConstructString(ParseElementValues());
    }

    /// <summary>
    /// Gets the current value as a <see cref="int"/>.
    /// </summary>
    /// <returns>The value as a <see cref="int"/>.</returns>
    /// <exception cref="HeroesXmlDataException">The value is not convertable to a <see cref="int"/>.</exception>
    public readonly int GetInt()
    {
        try
        {
            return int.Parse(Value);
        }
        catch (FormatException ex)
        {
            throw new HeroesXmlDataException($"Could not convert value '{Value}' to an int32.", ex);
        }
    }

    /// <summary>
    /// Get the current value as an <see cref="int"/>. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="value">When this method returns, contains the value as an <see cref="int"/>.</param>
    /// <returns><see langword="true"/> if the value can be converted to an <see cref="int"/>, otherwise <see langword="false"/>.</returns>
    public readonly bool TryGetInt32(out int value)
    {
        return int.TryParse(Value, out value);
    }

    /// <summary>
    /// Gets the current value as a <see cref="double"/>.
    /// </summary>
    /// <returns>The value as a <see cref="double"/>.</returns>
    /// <exception cref="HeroesXmlDataException">The value is not convertable to a <see cref="double"/>.</exception>
    public readonly double GetDouble()
    {
        try
        {
            return double.Parse(Value);
        }
        catch (FormatException ex)
        {
            throw new HeroesXmlDataException($"Could not convert value '{Value}' to a double.", ex);
        }
    }

    /// <summary>
    /// Get the current value as an <see cref="double"/>. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="value">When this method returns, contains the value as an <see cref="double"/>.</param>
    /// <returns><see langword="true"/> if the value can be converted to an <see cref="double"/>, otherwise <see langword="false"/>.</returns>
    public readonly bool TryGetDouble(out double value)
    {
        return double.TryParse(Value, out value);
    }

    /// <inheritdoc/>
    public override readonly string? ToString()
    {
        return GetString();
    }

#if DEBUG
    private readonly void PushNormalText(ref int index, ref int startingIndex, List<(Range Text, bool Replace)> elementNameList)
#else
    private static void PushNormalText(ref int index, ref int startingIndex, List<(Range Text, bool Replace)> elementNameList)
#endif
    {
        int normalTextLength = index - startingIndex;
        if (normalTextLength > 0)
        {
#if DEBUG
            ReadOnlySpan<char> temp = Value.Slice(startingIndex, normalTextLength);
#endif
            elementNameList.Add((new Range(startingIndex, index), false));
        }
    }

    private readonly List<(Range Text, bool Replace)> ParseElementValues()
    {
        int index = 0;
        int startingIndex = 0;

        List<(Range Text, bool Replace)> elementNameList = [];

        while (index < Value.Length)
        {
            // ##name##
            if (Value[index] == '#' && index + 1 < Value.Length && Value[index + 1] == '#')
            {
                PushNormalText(ref index, ref startingIndex, elementNameList);

                if (TryParseElementTag(ref index, out Range? tag))
                {
                    elementNameList.Add((tag.Value, true));

                    startingIndex = index;
                }
            }
            else
            {
                index++;
            }
        }

        if (index <= Value.Length)
        {
            PushNormalText(ref index, ref startingIndex, elementNameList);
        }

        return elementNameList;
    }

    private readonly string ConstructString(List<(Range Text, bool Replace)> elementNameList)
    {
        int bufferSize = GetBufferSize(elementNameList);

        int indexOfBuffer = 0;
        Span<char> buffer = stackalloc char[bufferSize];

        foreach ((Range indexOfText, bool replace) in elementNameList)
        {
            if (replace && _defaultStormElementData.TryGetElementDataAt(Value[indexOfText].Trim('#'), out StormElementData? stormElementData) && stormElementData.RawValue is not null)
            {
                stormElementData.RawValue.CopyTo(buffer[indexOfBuffer..]);
                indexOfBuffer += stormElementData.RawValue.Length;
            }
            else
            {
                Value[indexOfText].CopyTo(buffer[indexOfBuffer..]);
                indexOfBuffer += Value[indexOfText].Length;
            }
        }

        return buffer.ToString();
    }

    private readonly int GetBufferSize(List<(Range Text, bool Replace)> elementNameList)
    {
        int count = 0;
        foreach ((Range indexOfText, bool replace) in elementNameList)
        {
            if (replace && _defaultStormElementData.TryGetElementDataAt(Value[indexOfText].Trim('#'), out StormElementData? stormElementData))
                count += stormElementData.RawValue?.Length ?? 0;
            else
                count += indexOfText.End.Value - indexOfText.Start.Value;
        }

        return count;
    }

    private readonly bool TryParseElementTag(ref int index, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        ReadOnlySpan<char> currentTextSpan = Value[index..];
        int lengthOffset = Value.Length - currentTextSpan.Length;

        int startElementIndex = currentTextSpan.IndexOf("##") + 1; // the second char of the ## (first batch)
        int endElementIndex = -1; // first char of the ## (second batch)

        for (int i = startElementIndex; i < currentTextSpan.Length; i++)
        {
            // find next occurrence of ##
            if (currentTextSpan[i] == '#' && i + 1 < currentTextSpan.Length && currentTextSpan[i + 1] == '#')
            {
                endElementIndex = i;

                break;
            }
        }

        if (startElementIndex > 0 && endElementIndex > 0)
        {
#if DEBUG
            ReadOnlySpan<char> value = currentTextSpan[(startElementIndex + 1)..endElementIndex];
#endif
            tag = new Range(startElementIndex - 1 + lengthOffset, endElementIndex + 1 + lengthOffset + 1);

            index += endElementIndex - startElementIndex + 3;

            return true;
        }

        index += 2;

        return false;
    }
}
