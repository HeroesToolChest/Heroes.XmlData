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
}
