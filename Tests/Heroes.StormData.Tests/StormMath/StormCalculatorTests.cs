using System.Collections.Concurrent;
using System.Data;
using System.Text;

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
        double result = StormCalculator.Compute(expression, []);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("(12 + 6.000000) * (0.1875 + 0.062500) - (12 * 0.1875) / (12 * 0.1875) * 100")]
    [DataRow("17 / 34 * 100")]
    [DataRow("(57.8 / 34) * 100 - 100")]
    [DataRow("-100*(1-1.400000)")]
    [DataRow("--100")]
    [DataRow("-100*(-0.15)")]
    [DataRow("-100 * (0.225/(-0.15))")]
    [DataRow("(1+(-0.6)*100)")]
    [DataRow("-(-0.6-(-0.3))*100")]
    [DataRow("- (-0.7*100)")]
    [DataRow("-0.5")]
    [DataRow("0")]
    [DataRow("1+0*100")]
    [DataRow("((5) + (3) / 5 - 1) * 100")]
    [DataRow("(30/20)-1*10)")]
    [DataRow("(1-(-60))*-1")]
    [DataRow("--100*(-0.09)")]
    [DataRow("5*/-+5")]
    [DataRow("*+/-5+5")]
    [DataRow(".0625 *24")]
    [DataRow(" 100 * (-(-0.4)) ")]
    [DataRow("-(-(30)-(-10.000000)")]
    [DataRow(" 100(100*(-0.4))")]
    [DataRow("+(0.5)")]
    [DataRow("10/0")]
    [DataRow("(5)*(1.04)+(3)")]
    [DataRow("-(120*1.04)/(0.5)")]
    public void Compute_NumbersAsValuePlaceholders_ReturnsSameAsNumbersInText(string expression)
    {
        // arrange
        (string placeholderExpression, double[] values) = ToValuePlaceholders(expression);

        // act
        double expected = StormCalculator.Compute(expression, []);
        double result = StormCalculator.Compute(placeholderExpression, values);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void Compute_ValuePlaceholders_UsesValuesInOrder()
    {
        // arrange
        // act
        double result = StormCalculator.Compute("(#)-(#)", [10, 4]);

        // assert
        result.Should().Be(6);
    }

    [TestMethod]
    public void Compute_ValuePlaceholderValueIsNegative_UsesNegativeValue()
    {
        // arrange
        // act
        double result = StormCalculator.Compute("(#*#)", [-5, 1.04]);

        // assert
        result.Should().Be(-5.2);
    }

    [TestMethod]
    public void Compute_ValuePlaceholderAfterNegativeSign_NegatesValue()
    {
        // arrange
        // act
        double result = StormCalculator.Compute("(-#)*#", [3, 4]);

        // assert
        result.Should().Be(-12);
    }

    [TestMethod]
    public void Compute_MoreValuePlaceholdersThanValues_ThrowsSyntaxErrorException()
    {
        // arrange
        // act
        Action act = () => StormCalculator.Compute("(#)+(#)", [1]);

        // assert
        act.Should().Throw<SyntaxErrorException>();
    }

    [TestMethod]
    public void Compute_NumbersAndValuePlaceholdersMixed_ReturnsValue()
    {
        // arrange
        // act
        double result = StormCalculator.Compute("2*(#)-0.5", [3]);

        // assert
        result.Should().Be(5.5);
    }

    [TestMethod]
    public void Compute_ValuePlaceholderWithNoValues_ThrowsSyntaxErrorException()
    {
        // arrange
        // act
        Action act = () => StormCalculator.Compute("2*#", []);

        // assert
        act.Should().Throw<SyntaxErrorException>();
    }

    [TestMethod]
    public void Compute_MoreValuesThanValuePlaceholders_IgnoresExtraValues()
    {
        // arrange
        // act
        double result = StormCalculator.Compute("(#)+(#)", [1, 2, 100]);

        // assert
        result.Should().Be(3);
    }

    [TestMethod]
    public void Compute_ValuePlaceholdersLongerThanStackBuffer_ReturnsValue()
    {
        // arrange
        string expression = string.Join('+', Enumerable.Repeat("(#)", 300));
        double[] values = [.. Enumerable.Repeat(1.5, 300)];

        // act
        double result = StormCalculator.Compute(expression, values);

        // assert
        result.Should().Be(450);
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
            results.Add(StormCalculator.Compute(x, []));
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
        Action act = () => StormCalculator.Compute(expression, []);

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
        double sumResult = StormCalculator.Compute(sum, []);
        double nestedResult = StormCalculator.Compute(nested, []);

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
            double result = StormCalculator.Compute("1.5*2", []);

            // assert
            result.Should().Be(3);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    // replaces each number (a run of digits and decimal points, without its sign) with a value placeholder
    private static (string Expression, double[] Values) ToValuePlaceholders(string expression)
    {
        StringBuilder placeholderExpression = new();
        List<double> values = [];

        for (int i = 0; i < expression.Length; i++)
        {
            if (!char.IsAsciiDigit(expression[i]) && expression[i] != '.')
            {
                placeholderExpression.Append(expression[i]);
                continue;
            }

            int start = i;

            while (i + 1 < expression.Length && (char.IsAsciiDigit(expression[i + 1]) || expression[i + 1] == '.'))
                i++;

            values.Add(double.Parse(expression.AsSpan(start, i - start + 1), NumberStyles.Float, CultureInfo.InvariantCulture));
            placeholderExpression.Append(StormCalculator.ValuePlaceholder);
        }

        return (placeholderExpression.ToString(), [.. values]);
    }
}