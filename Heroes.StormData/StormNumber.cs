namespace Heroes.StormData;

/// <summary>
/// A number from the data, with both the value as written and the value the game stores for it.
/// <para>
/// The game stores numbers as 20.12 fixed-point, in steps of 1/4096, so most decimals are stored slightly differently than they are written,
/// e.g. a level scaling of 0.04 is stored as 0.0400390625 and 0.045 as 0.044921875.
/// Use <see cref="Value"/> to show the number and <see cref="FixedValue"/> to calculate with it, such as a value at a level.
/// </para>
/// </summary>
public readonly record struct StormNumber
{
    private StormNumber(double value, double fixedValue)
    {
        Value = value;
        FixedValue = fixedValue;
    }

    /// <summary>
    /// Gets the number as written in the data (e.g. 0.04).
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the number the game stores for <see cref="Value"/>, rounded to the nearest step of 1/4096 (e.g. 0.0400390625 for 0.04).
    /// It is always a multiple of 1/4096, so it is exact as a <see cref="double"/>.
    /// </summary>
    public double FixedValue { get; }

    /// <summary>
    /// Creates a <see cref="StormNumber"/> from a number as written in the data.
    /// </summary>
    /// <param name="value">The number as written in the data.</param>
    /// <returns>The <see cref="StormNumber"/> of <paramref name="value"/>.</returns>
    internal static StormNumber FromDataValue(double value) => new(value, MathHelpers.ToStormFixedLiteralValue(value));
}
