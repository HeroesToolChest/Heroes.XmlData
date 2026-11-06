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
    public void WriteNumber_HasNumber_WritesNumberAndCountMatches(double number, string expected)
    {
        // arrange
        Span<char> buffer = stackalloc char[64];
        int offset = 0;

        // act
        MathHelpers.WriteNumber(number, buffer, ref offset);
        int count = MathHelpers.GetCountOfDigits(number);

        // assert
        buffer[..offset].ToString().Should().Be(expected);
        count.Should().Be(expected.Length);
    }

    [TestMethod]
    public void GetCountOfDigits_NumbersOfAnyMagnitude_AreAtMostFourteenCharacters()
    {
        // arrange
        Random random = new(1);
        List<double> numbers = [.. Enumerable.Range(0, 100_000).Select(_ => (random.NextDouble() - 0.5) * Math.Pow(10, random.Next(-20, 21)))];

        // act
        List<int> counts = [.. numbers.Select(number => MathHelpers.GetCountOfDigits(number))];

        // assert
        counts.Should().OnlyContain(x => x <= 14);
    }

    [TestMethod]
    public void WriteNumber_CurrentCultureHasDifferentSigns_WritesInvariantNumber()
    {
        // arrange
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "--";
        culture.NumberFormat.NumberDecimalSeparator = ",";

        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        char[] buffer = new char[16];
        int offset = 0;

        try
        {
            CultureInfo.CurrentCulture = culture;

            // act
            MathHelpers.WriteNumber(-1.5, buffer, ref offset);
            int count = MathHelpers.GetCountOfDigits(-1.5);

            // assert
            new string(buffer, 0, offset).Should().Be("-1.5");
            count.Should().Be(4);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    public void WriteNumber_HasOffset_WritesAtOffsetAndMovesOffsetPastNumber()
    {
        // arrange
        char[] buffer = "ab______".ToCharArray();
        int offset = 2;

        // act
        MathHelpers.WriteNumber(1.5, buffer, ref offset);

        // assert
        new string(buffer).Should().Be("ab1.5___");
        offset.Should().Be(5);
    }

    [TestMethod]
    public void WriteNumber_DestinationTooSmall_ThrowsArgumentException()
    {
        // arrange
        char[] buffer = new char[3];

        // act
        Action act = () =>
        {
            int offset = 0;
            MathHelpers.WriteNumber(123.5, buffer, ref offset);
        };

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    [DataRow("de-DE", -1.5, "-1,5")]
    [DataRow("fr-FR", 2.25, "2,25")]
    [DataRow("en-US", -1.5, "-1.5")]
    [DataRow("de-DE", 120, "120")]
    public void WriteNumber_HasFormatProvider_WritesInCultureAndCountMatches(string cultureName, double number, string expected)
    {
        // arrange
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        char[] buffer = new char[32];
        int offset = 0;

        // act
        MathHelpers.WriteNumber(number, buffer, ref offset, culture);
        int count = MathHelpers.GetCountOfDigits(number, culture);

        // assert
        new string(buffer, 0, offset).Should().Be(expected);
        count.Should().Be(expected.Length);
    }

    [TestMethod]
    [DataRow(-5, "--5")]
    [DataRow(-999998.123456, "--999998,,123456")]
    public void WriteNumber_FormatProviderHasMultiCharSigns_CountMatches(double number, string expected)
    {
        // arrange
        CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "--";
        culture.NumberFormat.NumberDecimalSeparator = ",,";

        char[] buffer = new char[32];
        int offset = 0;

        // act
        MathHelpers.WriteNumber(number, buffer, ref offset, culture);
        int count = MathHelpers.GetCountOfDigits(number, culture);

        // assert
        new string(buffer, 0, offset).Should().Be(expected);
        count.Should().Be(expected.Length);
    }
}
