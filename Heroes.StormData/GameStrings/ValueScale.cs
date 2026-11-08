namespace Heroes.StormData.GameStrings;

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

    public ValueScale(double value)
    {
        Value = ClampValue(value);
    }

    public ValueScale(double value, double scaling)
    {
        Value = ClampValue(value);
        Scaling = ClampScaling(scaling);
    }

    public double Value { get; }

    public double? Scaling { get; }

    private static double ClampValue(double value) => NormalizeZero(Math.Clamp(value, -MaxValueSize, MaxValueSize));

    private static double ClampScaling(double scaling) => NormalizeZero(Math.Clamp(scaling, -MaxScalingSize, MaxScalingSize));

    // -0 (e.g. from rounding -0.4) would otherwise be written as "-0"
    private static double NormalizeZero(double value) => value == 0 ? 0 : value;
}
