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
        if (Value.IndexOf('#') < 0)
            return Value.ToString();

        return ConstructString(ParseElementValues());
    }

    /// <summary>
    /// Gets the current value as a <see cref="int"/>.
    /// </summary>
    /// <returns>The value as a <see cref="int"/>.</returns>
    /// <exception cref="HeroesXmlDataException">The value is not convertable to a <see cref="int"/>.</exception>
    public readonly int GetInt32()
    {
        try
        {
            return int.Parse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
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
        return int.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
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
            return double.Parse(Value, NumberStyles.Float, CultureInfo.InvariantCulture);
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
        return double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
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

        PushNormalText(ref index, ref startingIndex, elementNameList);

        return elementNameList;
    }

    private readonly string ConstructString(List<(Range Text, bool Replace)> elementNameList)
    {
        char[] rentedArrayPool = ArrayPool<char>.Shared.Rent(Value.Length);

        try
        {
            int written = 0;

            foreach ((Range indexOfText, bool replace) in elementNameList)
            {
                ReadOnlySpan<char> currentValue = Value[indexOfText];
                ReadOnlySpan<char> trimmedValue = currentValue.Trim('#');

                ReadOnlySpan<char> valueToCopy;

                if (replace && _defaultStormElementData.StormElement.ProcessingInstructionsById.TryGetValue(trimmedValue, out XElement? piElement))
                {
                    string? piValue = piElement.Attribute("value")?.Value;
                    valueToCopy = piValue is not null ? piValue.AsSpan() : currentValue;
                }
                else if (replace && _defaultStormElementData.TryGetElementDataAt(trimmedValue, out StormElementData? stormElementData) && stormElementData.RawValue is not null)
                {
                    valueToCopy = stormElementData.RawValue.AsSpan();
                }
                else
                {
                    valueToCopy = currentValue;
                }

                // check size and grow array pool if needed
                if (written + valueToCopy.Length > rentedArrayPool.Length)
                {
                    int newSize = Math.Max(rentedArrayPool.Length * 2, written + valueToCopy.Length);
                    char[] largerArrayPool = ArrayPool<char>.Shared.Rent(newSize);

                    rentedArrayPool.AsSpan(0, written).CopyTo(largerArrayPool);

                    ArrayPool<char>.Shared.Return(rentedArrayPool);

                    rentedArrayPool = largerArrayPool;
                }

                valueToCopy.CopyTo(rentedArrayPool.AsSpan(written));
                written += valueToCopy.Length;
            }

            return new string(rentedArrayPool, 0, written);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArrayPool);
        }
    }

    // note: this method assumes that the caller has already confirmed that 'index' points at the first '#' of the opening ##.
    private readonly bool TryParseElementTag(ref int index, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        // ##name##
        int contentStart = index + 2;

        // scan for closing ##
        int closingHash = -1;
        for (int i = contentStart; i < Value.Length - 1; i++)
        {
            if (Value[i] == '#' && Value[i + 1] == '#')
            {
                closingHash = i;
                break;
            }
        }

        if (closingHash < 0)
        {
            index += 2;
            return false;
        }

        // tag range spans from the first '#' of opening ## to the last '#' of closing ##
        tag = new Range(index, closingHash + 2);

        index = closingHash + 2;

        return true;
    }
}
