using System.Runtime.CompilerServices;

namespace Heroes.StormData.StormMath;

internal static class MathHelpers
{
    /// <summary>
    /// The format numbers are written in, in gamestrings. It is fixed-point, so never exponent notation,
    /// with up to 6 decimal places (<see cref="StormCalculator.MaxFractionalDigits"/>) and trailing zeros dropped.
    /// Rounding to 6 decimal places also drops floating-point noise.
    /// </summary>
    public const string NumberFormat = "0.######";

    // the game stores numbers as fixed-point (20.12), so each step is 1/4096
    private const decimal StormFixedScale = 4096m;

    // 1/4096 has 12 decimal places, so every step can be written with at most 12
    private const int StormFixedMaxDecimalPlaces = 12;

    // larger than any game value, keeps the conversion to decimal and long from overflowing
    private const double StormFixedMaxMagnitude = 1e12;

    /// <summary>
    /// Appends <paramref name="number"/> in <see cref="NumberFormat"/> to <paramref name="builder"/>, in the culture the builder was created with.
    /// The number is first normalized (see <see cref="Normalize"/>).
    /// </summary>
    /// <param name="builder">The builder to append to. It must be created with a culture, otherwise the current culture is used.</param>
    /// <param name="number">The number to append.</param>
    public static void AppendNumber(ref DefaultInterpolatedStringHandler builder, double number)
    {
        builder.AppendFormatted(Normalize(number), NumberFormat);
    }

    /// <summary>
    /// Gets <paramref name="number"/> as <see cref="AppendNumber"/> writes it: limited to <see cref="ValueScale.MaxValueSize"/>, either sign,
    /// rounded to 6 decimal places, and with -0 turned into 0.
    /// </summary>
    /// <param name="number">The number to normalize.</param>
    /// <returns>The normalized number.</returns>
    public static double Normalize(double number)
    {
        double clamped = Math.Clamp(number, -ValueScale.MaxValueSize, ValueScale.MaxValueSize);
        double rounded = Math.Round(clamped, StormCalculator.MaxFractionalDigits, StormCalculator.MaxFractionalMidpointRoundingMode);

        return rounded == 0 ? 0 : rounded;
    }

    /// <summary>
    /// Gets the value the game stores for the result of a calculation, such as a <c>const</c> with an expression.
    /// The result is rounded down to a fixed-point step of 1/4096, then written as the shortest decimal that converts back
    /// (rounding to the nearest step) to that same step. For example, 1.1/33 (0.0333...) is 136/4096, which is 0.0332, and 0.3/100 (0.003) is 12/4096, which is still 0.003.
    /// </summary>
    /// <remarks>
    /// Numbers that are written as is in the data (not calculated) keep their exact value, so this is only for calculated values.
    /// </remarks>
    /// <param name="number">The calculated number.</param>
    /// <returns>The stored value of <paramref name="number"/>.</returns>
    public static double ToStormFixedValue(double number)
    {
        if (!double.IsFinite(number) || Math.Abs(number) >= StormFixedMaxMagnitude)
            return number;

        // rounded down (towards negative infinity), e.g. -0.3 is -1228.8 steps, which is -1229
        long step = (long)decimal.Floor((decimal)number * StormFixedScale);

        for (int decimalPlaces = 0; decimalPlaces <= StormFixedMaxDecimalPlaces; decimalPlaces++)
        {
            decimal candidate = Math.Round(step / StormFixedScale, decimalPlaces, MidpointRounding.AwayFromZero);

            if ((long)Math.Round(candidate * StormFixedScale, MidpointRounding.AwayFromZero) == step)
                return (double)candidate;
        }

        return (double)(step / StormFixedScale);
    }
}
