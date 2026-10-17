namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a value for an XML element or attribute.
/// </summary>
public readonly struct StormXmlValue
{
    private readonly string? _rawValue;
    private readonly StormXmlMember _rootMember;

    internal StormXmlValue(string rawValue, StormXmlMember rootMember)
    {
        _rawValue = rawValue;
        _rootMember = rootMember;
    }

    /// <summary>
    /// Gets a value indicating whether a value was set.
    /// <para>
    /// In terms of the XML, this corresponds to the <c>value</c> attribute (case-sensitive).
    /// </para>
    /// </summary>
    public bool HasValue => _rawValue is not null;

    /// <summary>
    /// Gets the raw value, or an empty string if there is no value.
    /// <para>
    /// This will contain the token replacement <c>##[name]##</c> if it exists.
    /// </para>
    /// </summary>
    public string RawValue => _rawValue ?? string.Empty;

    /// <summary>
    /// Implicitly converts a <see cref="StormXmlValue"/> to a <see cref="string"/> by returning the raw value.
    /// </summary>
    /// <param name="stormValue">The <see cref="StormXmlValue"/> to convert.</param>
    public static implicit operator string(StormXmlValue stormValue) => stormValue.RawValue;

    /// <summary>
    /// Determines whether two <see cref="StormXmlValue"/> instances are equal by comparing their raw values.
    /// </summary>
    /// <param name="left">The first <see cref="StormXmlValue"/> to compare.</param>
    /// <param name="right">The second <see cref="StormXmlValue"/> to compare.</param>
    /// <returns><see langword="true"/> if the raw values of both instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(StormXmlValue left, StormXmlValue right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="StormXmlValue"/> instances are not equal by comparing their raw values.
    /// </summary>
    /// <param name="left">The first <see cref="StormXmlValue"/> to compare.</param>
    /// <param name="right">The second <see cref="StormXmlValue"/> to compare.</param>
    /// <returns><see langword="true"/> if the raw values of both instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(StormXmlValue left, StormXmlValue right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Gets the value as a <see cref="string"/>, replacing any token replacements <c>##[name]##</c> with their corresponding values.
    /// </summary>
    /// <returns>The value as a <see cref="string"/> with token replacements applied.</returns>
    public string GetString()
    {
        int indexOfHash = RawValue.IndexOf('#');

        if (indexOfHash < 0)
            return RawValue;

        return ConstructString(ParseRawValue(indexOfHash));
    }

    /// <summary>
    /// Gets the value as a <see cref="double"/>.
    /// </summary>
    /// <returns>The value as a <see cref="double"/>.</returns>
    /// <exception cref="HeroesStormDataException">Thrown when the value cannot be converted to a <see cref="double"/>.</exception>
    public double GetDouble()
    {
        try
        {
            return double.Parse(RawValue, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new HeroesStormDataException($"Could not convert value '{RawValue}' to a double.", ex);
        }
    }

    /// <summary>
    /// Tries to get the value as a <see cref="double"/>. Returns <see langword="true"/> if the conversion was successful, otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="value">When this method returns, contains the value as a <see cref="double"/> if the conversion succeeded, otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> if the value can be converted to a <see cref="double"/>, otherwise <see langword="false"/>.</returns>
    public readonly bool TryGetDouble(out double value)
    {
        return double.TryParse(RawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Gets the value as an <see cref="int"/>.
    /// </summary>
    /// <returns>The value as an <see cref="int"/>.</returns>
    /// <exception cref="HeroesStormDataException">Thrown when the value cannot be converted to an <see cref="int"/>.</exception>
    public int GetInt32()
    {
        try
        {
            return int.Parse(RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new HeroesStormDataException($"Could not convert value '{RawValue}' to an int32.", ex);
        }
    }

    /// <summary>
    /// Tries to get the value as an <see cref="int"/>. Returns <see langword="true"/> if the conversion was successful, otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="value">When this method returns, contains the value as an <see cref="int"/> if the conversion succeeded, otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> if the value can be converted to an <see cref="int"/>, otherwise <see langword="false"/>.</returns>
    public bool TryGetInt32(out int value)
    {
        return int.TryParse(RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// When <paramref name="obj"/> is a <see cref="string"/>, it is compared against <see cref="RawValue"/>.
    /// When <paramref name="obj"/> is a <see cref="StormXmlValue"/>, field equality is used.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is string str)
            return string.Equals(RawValue, str, StringComparison.Ordinal);

        if (obj is StormXmlValue other)
            return string.Equals(_rawValue, other._rawValue, StringComparison.Ordinal);

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => RawValue.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Calls <see cref="GetString"/> to get the value as a <see cref="string"/> with token replacements applied.
    /// </summary>
    /// <returns>The value as a <see cref="string"/> with token replacements applied.</returns>
    public override string ToString() => GetString();

    private static void PushNormalText(ref int index, ref int startingIndex, List<(Range Text, bool Replace)> rawValueTextList)
    {
        int normalTextLength = index - startingIndex;

        if (normalTextLength > 0)
            rawValueTextList.Add((new Range(startingIndex, index), false));
    }

    private List<(Range Text, bool Replace)> ParseRawValue(int indexOfFirstHash)
    {
        List<(Range Text, bool Replace)> rawValueTextList = [];

        int index = indexOfFirstHash;
        int startingIndex = 0;

        while (index < RawValue.Length)
        {
            // ##name##
            if (RawValue[index] == '#' && index + 1 < RawValue.Length && RawValue[index + 1] == '#')
            {
                int tagStart = index;

                if (TryParseElementTag(ref index, out Range? tag))
                {
                    PushNormalText(ref tagStart, ref startingIndex, rawValueTextList);

                    rawValueTextList.Add((tag.Value, true));

                    startingIndex = index;
                }
            }
            else
            {
                index++;
            }
        }

        PushNormalText(ref index, ref startingIndex, rawValueTextList);

        return rawValueTextList;
    }

    // note: this method assumes that the caller has already confirmed that 'index' points at the first '#' of the opening ##.
    private bool TryParseElementTag(ref int index, [NotNullWhen(true)] out Range? tag)
    {
        tag = null;

        // ##name##
        int contentStart = index + 2;

        // scan for closing ##
        int closingHash = -1;
        for (int i = contentStart; i < RawValue.Length - 1; i++)
        {
            if (RawValue[i] == '#' && RawValue[i + 1] == '#')
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

    private string ConstructString(List<(Range Text, bool Replace)> rawValueTextList)
    {
        char[] rentedArrayPool = ArrayPool<char>.Shared.Rent(RawValue.Length);

        try
        {
            int written = 0;

            foreach ((Range indexOfText, bool replace) in rawValueTextList)
            {
                ReadOnlySpan<char> currentValue = RawValue[indexOfText];
                ReadOnlySpan<char> trimmedValue = currentValue.Trim('#');

                ReadOnlySpan<char> valueToCopy;

                if (replace && _rootMember.TryGetMember(trimmedValue, out StormXmlArray? stormXmlArray) && stormXmlArray.Value.HasValue)
                    valueToCopy = stormXmlArray.Value.RawValue;
                else
                    valueToCopy = currentValue;

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
}
