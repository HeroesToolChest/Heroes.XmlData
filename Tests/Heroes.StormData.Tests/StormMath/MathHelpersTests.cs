using System.Runtime.CompilerServices;

namespace Heroes.StormData.StormMath.Tests;

[TestClass]
public sealed class MathHelpersTests
{
    [TestMethod]
    [DataRow(0, "0")]
    [DataRow(-0.0, "0")]
    [DataRow(-0.0000001, "0")] // rounds to -0, which is written as 0
    [DataRow(1235, "1235")]
    [DataRow(-1235, "-1235")]
    [DataRow(2.0000001, "2")]
    [DataRow(999_999.0, "999999")]
    [DataRow(-999_999.0, "-999999")]
    [DataRow(999_998.123456789, "999998.123457")] // rounded to 6 decimal places
    [DataRow(-999_998.123456, "-999998.123456")] // the longest a number can be
    [DataRow(999_999.5, "999999")] // clamped to ValueScale.MaxValueSize
    [DataRow(1_000_000.0, "999999")]
    [DataRow(-3e9, "-999999")]
    [DataRow(double.MaxValue, "999999")]
    [DataRow(double.MinValue, "-999999")]
    [DataRow(double.PositiveInfinity, "999999")]
    [DataRow(double.NegativeInfinity, "-999999")]
    [DataRow(-0.123456789012345, "-0.123457")]
    [DataRow(123.1, "123.1")]
    [DataRow(0.29, "0.29")]
    [DataRow(-999.9, "-999.9")]
    [DataRow(123.456789, "123.456789")]
    [DataRow(-123.456789, "-123.456789")]
    [DataRow(123.100000001, "123.1")]
    [DataRow(0.04, "0.04")]
    [DataRow(0.1234567, "0.123457")]
    [DataRow(0.30000000000000004, "0.3")] // floating-point noise is dropped
    [DataRow(1.1400000000000001, "1.14")]
    [DataRow(0.00001, "0.00001")] // never exponent notation
    [DataRow(0.000001, "0.000001")]
    [DataRow(0.000000001, "0")] // past 6 decimal places
    [DataRow(double.NaN, "NaN")]
    public void AppendNumber_HasNumber_AppendsNumber(double number, string expected)
    {
        // act
        string result = AppendNumber(number, CultureInfo.InvariantCulture);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void AppendNumber_BuilderHasText_AppendsAfterText()
    {
        // arrange
        DefaultInterpolatedStringHandler builder = new(0, 0, CultureInfo.InvariantCulture);
        builder.AppendLiteral("ab");

        // act
        MathHelpers.AppendNumber(ref builder, 1.5);

        // assert
        builder.ToStringAndClear().Should().Be("ab1.5");
    }

    [TestMethod]
    [DataRow("de-DE", -1.5, "-1,5")]
    [DataRow("fr-FR", 2.25, "2,25")]
    [DataRow("en-US", -1.5, "-1.5")]
    [DataRow("de-DE", 120, "120")]
    public void AppendNumber_BuilderHasCulture_AppendsInCulture(string cultureName, double number, string expected)
    {
        // act
        string result = AppendNumber(number, CultureInfo.GetCultureInfo(cultureName));

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(-5, "--5")]
    [DataRow(-999998.123456, "--999998,,123456")]
    public void AppendNumber_CultureHasMultiCharSigns_AppendsSigns(double number, string expected)
    {
        // arrange
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "--";
        culture.NumberFormat.NumberDecimalSeparator = ",,";

        // act
        string result = AppendNumber(number, culture);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(31, 31)] // whole numbers are exact steps
    [DataRow(-48, -48)]
    [DataRow(17.5, 17.5)]
    [DataRow(2.0625, 2.0625)]
    [DataRow(0.000244140625, 0.0002)] // 1/4096, the smallest step
    [DataRow(0.033333, 0.0332)] // 1.1/33 is 136.53 steps, rounded down to 136
    [DataRow(0.022222, 0.0222)] // 0.8/36 is 91.02 steps, rounded down to 91
    [DataRow(0.003, 0.003)] // 0.3/100 is 12.288 steps, rounded down to 12, which 0.003 still converts back to
    [DataRow(33.333333, 33.3333)] // 200/6
    [DataRow(-0.3, -0.3)] // -1228.8 steps, rounded down to -1229, which -0.3 converts back to
    [DataRow(0.3, 0.2998)] // 1228.8 steps, rounded down to 1228
    [DataRow(1.35, 1.3499)]
    [DataRow(double.NaN, double.NaN)]
    [DataRow(double.PositiveInfinity, double.PositiveInfinity)]
    [DataRow(1e12, 1e12)] // too large, kept as is
    public void ToStormFixedValue_HasNumber_ReturnsStoredValue(double number, double expected)
    {
        // act
        double result = MathHelpers.ToStormFixedValue(number);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(0.5, 0.5)] // already a step
    [DataRow(2, 2)]
    [DataRow(0.04, 0.0400390625)] // 163.84 steps, rounded to 164
    [DataRow(0.045, 0.044921875)] // 184.32 steps, rounded to 184
    [DataRow(0.05, 0.050048828125)] // 204.8 steps, rounded to 205
    [DataRow(0.035, 0.034912109375)] // 143.36 steps, rounded to 143
    [DataRow(-0.04, -0.0400390625)]
    [DataRow(0.3, 0.300048828125)] // 1228.8 steps, rounded to 1229
    [DataRow(0.0001220703125, 0.000244140625)] // half a step, rounded away from zero
    [DataRow(double.NaN, double.NaN)]
    [DataRow(1e12, 1e12)] // too large, kept as is
    public void ToStormFixedLiteralValue_HasNumber_ReturnsStoredValue(double number, double expected)
    {
        // act
        double result = MathHelpers.ToStormFixedLiteralValue(number);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(1.5, "1.5")]
    [DataRow(-110.6, "-110.6")]
    [DataRow(0.1234567, "0.123457")] // normalized to 6 decimal places
    [DataRow(1_000_000.0, "999999")]
    public void AppendInvariantNumber_BuilderHasCommaDecimalSeparator_AppendsInInvariantCulture(double number, string expected)
    {
        // arrange
        DefaultInterpolatedStringHandler builder = new(0, 0, CultureInfo.GetCultureInfo("de-DE"));

        // act
        MathHelpers.AppendInvariantNumber(ref builder, number);

        // assert
        builder.ToStringAndClear().Should().Be(expected);
    }

    [TestMethod]
    [DataRow(0.04, "0.0400390625")] // more than 6 decimal places, so the stored value is written exactly
    [DataRow(-0.035, "-0.034912109375")]
    [DataRow(0.5, "0.5")]
    [DataRow(-0.00001, "0")] // rounds to -0, which is written as 0
    public void AppendInvariantStormFixedNumber_BuilderHasCommaDecimalSeparator_AppendsStoredValueInInvariantCulture(double number, string expected)
    {
        // arrange
        DefaultInterpolatedStringHandler builder = new(0, 0, CultureInfo.GetCultureInfo("de-DE"));

        // act
        MathHelpers.AppendInvariantStormFixedNumber(ref builder, number);

        // assert
        builder.ToStringAndClear().Should().Be(expected);
    }

    private static string AppendNumber(double number, IFormatProvider formatProvider)
    {
        DefaultInterpolatedStringHandler builder = new(0, 0, formatProvider);

        MathHelpers.AppendNumber(ref builder, number);

        return builder.ToStringAndClear();
    }
}
