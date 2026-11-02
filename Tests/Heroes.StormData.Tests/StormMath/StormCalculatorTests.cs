using System.Collections.Concurrent;
using System.Data;

namespace Heroes.StormData.StormMath.Tests;

[TestClass]
public sealed class StormCalculatorTests
{
    [TestMethod]
    [DataRow(100, "(12 + 6.000000) * (0.1875 + 0.062500) - (12 * 0.1875) / (12 * 0.1875) * 100")]
    [DataRow(50, "17 / 34 * 100")]
    [DataRow(70, "(57.8 / 34) * 100 - 100")]
    [DataRow(40, "-100*(1-1.400000)")]
    [DataRow(-100, "--100")]
    [DataRow(15, "-100*(-0.15)")]
    [DataRow(150, "-100 * (0.225/(-0.15))")]
    [DataRow(40, "(1+(-0.6)*100)")]
    [DataRow(30, "-(-0.6-(-0.3))*100")]
    [DataRow(70, "- (-0.7*100)")]
    [DataRow(-0.5, "-0.5")]
    [DataRow(0, "0")]
    [DataRow(100, "1+0*100")]
    [DataRow(100, "(1+0*100)")]
    [DataRow(60, "((5) + (3) / 5 - 1) * 100")]
    [DataRow(5, "(30/20)-1*10)")]
    [DataRow(60, "(1-(-60))*-1")]
    [DataRow(9, "--100*(-0.09)")]
    [DataRow(10, "5*/-+5")]
    [DataRow(0, "*+/-5+5")]
    [DataRow(1.5, ".0625 *24")]
    [DataRow(40, " 100 * (-(-0.4)) ")]
    [DataRow(20, "-(-(30)-(-10.000000)")]
    [DataRow(60, " 100(100*(-0.4))")]
    [DataRow(0.5, "+(0.5)")]
    [DataRow(10, "10/0")]
    public void Compute_ValidExpressions_ReturnsValue(double expected, string expression)
    {
        // arrange
        // act
        double result = StormCalculator.Compute(expression);
        double resultFromSpan = StormCalculator.Compute(expression.AsSpan());

        // assert
        result.Should().Be(expected);
        resultFromSpan.Should().Be(expected);
    }

    [TestMethod]
    public void Compute_AllMultiThreading_ReturnsValues()
    {
        // arrange
        List<string> expressions = [];
        ConcurrentBag<double> results = [];

        for (int i = 0; i < 100; i++)
        {
            expressions.Add("-100*(-0.15)");
        }

        // act
        Parallel.ForEach(expressions, (x) =>
        {
            results.Add(StormCalculator.Compute(x));
        });

        // assert
        results.Should().AllBeEquivalentTo(15);
    }

    [TestMethod]
    public void Compute_InvalidExpression_ThrowException()
    {
        // arrange
        string expression = "1(*a4/z)+0*100";

        // act
        Action act = () => StormCalculator.Compute(expression);

        // assert
        act.Should().Throw<SyntaxErrorException>();
    }

    [TestMethod]
    public void Compute_ExpressionLongerThanStackBuffer_ReturnsValue()
    {
        // arrange
        string sum = string.Join('+', Enumerable.Repeat("1.5", 200));
        string nested = $"{new string('(', 200)}2{new string(')', 200)}*(-3)";

        // act
        double sumResult = StormCalculator.Compute(sum);
        double nestedResult = StormCalculator.Compute(nested);

        // assert
        sumResult.Should().Be(300);
        nestedResult.Should().Be(-6);
    }

    [TestMethod]
    [DataRow("0")]
    [DataRow("-0")]
    [DataRow(".5")]
    [DataRow("5.")]
    [DataRow("-5.")]
    [DataRow("6.000000")]
    [DataRow("0.1")]
    [DataRow("0.1875")]
    [DataRow("-0.15")]
    [DataRow("999998.123456")]
    [DataRow("-999998.123456")]
    [DataRow(".1234567890123")]
    [DataRow("123456789012345")]
    [DataRow("1234567890123456")]
    [DataRow("0.0000000000000001")]
    [DataRow("00000000000000000000.5")]
    public void ParseNumber_Numbers_MatchesDoubleParse(string number)
    {
        // arrange
        double expected = double.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture);

        // act
        double result = StormCalculator.ParseNumber(number);

        // assert
        BitConverter.DoubleToInt64Bits(result).Should().Be(BitConverter.DoubleToInt64Bits(expected));
    }

    [TestMethod]
    [DataRow(".")]
    [DataRow("-")]
    [DataRow("")]
    [DataRow("1.2.3")]
    public void ParseNumber_NotANumber_ThrowsFormatException(string number)
    {
        // arrange
        // act
        Action act = () => StormCalculator.ParseNumber(number);

        // assert
        act.Should().Throw<FormatException>();
    }

    [TestMethod]
    public void Compute_CurrentCultureUsesCommaDecimalSeparator_ParsesWithInvariantCulture()
    {
        // arrange
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            double result = StormCalculator.Compute("1.5*2");

            // assert
            result.Should().Be(3);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}