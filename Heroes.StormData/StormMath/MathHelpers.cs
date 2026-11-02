namespace Heroes.StormData.StormMath;

internal static class MathHelpers
{
    /// <summary>
    /// The format numbers are written in, in gamestrings and in math expressions. It is fixed-point, so never exponent notation,
    /// with up to 6 decimal places (<see cref="StormCalculator.MaxFractionalDigits"/>) and trailing zeros dropped.
    /// Rounding to 6 decimal places also drops floating-point noise.
    /// </summary>
    public const string NumberFormat = "0.######";

    /// <summary>
    /// Gets the number of characters <see cref="WriteNumber"/> writes for <paramref name="number"/>.
    /// Includes the negative sign, decimal point, and fractional digits.
    /// </summary>
    /// <param name="number">The number to evaluate.</param>
    /// <returns>The number of characters, at most 14.</returns>
    public static int GetCountOfDigits(double number)
    {
        number = Normalize(number);

        // fast path for whole numbers, the most common case
        if (double.IsInteger(number))
        {
            uint magnitude = (uint)Math.Abs(number);
            int count = 1;

            while (magnitude >= 10)
            {
                magnitude /= 10;
                count++;
            }

            if (number < 0)
                count++; // negative sign

            return count;
        }

        // measure by writing it, so the count always matches WriteNumber
        Span<char> buffer = stackalloc char[StormCalculator.MaxNumberLength];
        int length = 0;

        WriteNumber(number, buffer, ref length);

        return length;
    }

    /// <summary>
    /// Writes <paramref name="number"/> in <see cref="NumberFormat"/> into <paramref name="destination"/> at <paramref name="offset"/>,
    /// then moves <paramref name="offset"/> past it. Numbers beyond <see cref="ValueScale.MaxValueSize"/>, either sign, are written as that limit,
    /// and numbers are rounded to 6 decimal places.
    /// </summary>
    /// <param name="number">The number to write.</param>
    /// <param name="destination">The buffer to write into.</param>
    /// <param name="offset">The position in <paramref name="destination"/> to write at.</param>
    /// <exception cref="ArgumentException"><paramref name="destination"/> doesn't have room for the number.</exception>
    public static void WriteNumber(double number, Span<char> destination, ref int offset)
    {
        if (!Normalize(number).TryFormat(destination[offset..], out int charsWritten, NumberFormat, CultureInfo.InvariantCulture))
            throw new ArgumentException("The destination is too small to hold the number.", nameof(destination));

        offset += charsWritten;
    }

    // a tiny negative number rounds to -0, which is turned into 0 so it isn't written as "-0"
    private static double Normalize(double number)
    {
        double clamped = Math.Clamp(number, -ValueScale.MaxValueSize, ValueScale.MaxValueSize);
        double rounded = Math.Round(clamped, StormCalculator.MaxFractionalDigits, StormCalculator.MaxFractionalMidpointRoundingMode);

        return rounded == 0 ? 0 : rounded;
    }
}
