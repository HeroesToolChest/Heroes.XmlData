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

    private static string AppendNumber(double number, IFormatProvider formatProvider)
    {
        DefaultInterpolatedStringHandler builder = new(0, 0, formatProvider);

        MathHelpers.AppendNumber(ref builder, number);

        return builder.ToStringAndClear();
    }
}
