namespace Heroes.StormData.GameStrings;

/// <summary>
/// A number from a gamestring data tag, with its per-level scaling if it has one.
/// </summary>
internal readonly struct ValueScale
{
    /// <summary>
    /// The largest value a <see cref="ValueScale"/> holds, either sign. <see cref="MathHelpers"/> also writes numbers no larger than this.
    /// </summary>
    public const double MaxValueSize = 999_999;

    /// <summary>
    /// The largest scaling a <see cref="ValueScale"/> holds, either sign. A scaling is a per-level increase such as 0.04 or 0.035,
    /// so it is always less than 1. It has six decimal places, the precision scaling is rounded to.
    /// </summary>
    public const double MaxScalingSize = 0.999999;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueScale"/> struct, for a value that has no scaling.
    /// </summary>
    /// <param name="value">The value. It is limited to <see cref="MaxValueSize"/>, either sign.</param>
    public ValueScale(double value)
    {
        Value = ClampValue(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueScale"/> struct, for a value that scales per level.
    /// </summary>
    /// <param name="value">The value. It is limited to <see cref="MaxValueSize"/>, either sign.</param>
    /// <param name="scaling">The per-level increase, e.g. 0.04. It is limited to <see cref="MaxScalingSize"/>, either sign.</param>
    public ValueScale(double value, double scaling)
    {
        Value = ClampValue(value);
        Scaling = ClampScaling(scaling);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueScale"/> struct, for a scaling value that was rounded for the tooltip.
    /// The value from before the rounding is kept, since it is needed to work out the value at a level.
    /// </summary>
    /// <param name="value">The value rounded to <paramref name="precision"/>, as the tooltip shows it. It is limited to <see cref="MaxValueSize"/>, either sign.</param>
    /// <param name="scaling">The per-level increase, e.g. 0.04. It is limited to <see cref="MaxScalingSize"/>, either sign.</param>
    /// <param name="unroundedValue">The value before it was rounded to <paramref name="precision"/>. It is limited to <see cref="MaxValueSize"/>, either sign.</param>
    /// <param name="precision">The number of decimal places the value is rounded to.</param>
    public ValueScale(double value, double scaling, double unroundedValue, int precision)
    {
        Value = ClampValue(value);
        Scaling = ClampScaling(scaling);
        UnroundedValue = ClampValue(unroundedValue);
        Precision = precision;
    }

    /// <summary>
    /// Gets the value, as the tooltip shows it.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the per-level increase, e.g. 0.04 for 4% per level, or <see langword="null"/> if the value does not scale.
    /// </summary>
    public double? Scaling { get; }

    /// <summary>
    /// Gets the value before it was rounded to <see cref="Precision"/>, if it is known.
    /// </summary>
    public double? UnroundedValue { get; }

    /// <summary>
    /// Gets the number of decimal places <see cref="Value"/> is rounded to, if it is known.
    /// </summary>
    public int? Precision { get; }

    /// <summary>
    /// Limits <paramref name="value"/> to <see cref="MaxValueSize"/>, either sign, with -0 turned into 0.
    /// </summary>
    /// <param name="value">The value to limit.</param>
    /// <returns>The limited value.</returns>
    private static double ClampValue(double value) => NormalizeZero(Math.Clamp(value, -MaxValueSize, MaxValueSize));

    /// <summary>
    /// Limits <paramref name="scaling"/> to <see cref="MaxScalingSize"/>, either sign, with -0 turned into 0.
    /// </summary>
    /// <param name="scaling">The scaling to limit.</param>
    /// <returns>The limited scaling.</returns>
    private static double ClampScaling(double scaling) => NormalizeZero(Math.Clamp(scaling, -MaxScalingSize, MaxScalingSize));

    /// <summary>
    /// Turns -0 into 0, since -0 (e.g. from rounding -0.4) would otherwise be written as "-0".
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><paramref name="value"/>, or 0 if it is -0.</returns>
    private static double NormalizeZero(double value) => value == 0 ? 0 : value;
}
