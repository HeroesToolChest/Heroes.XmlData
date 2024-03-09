using System.Collections.Concurrent;
using System.Data;

namespace Heroes.XmlData.Tests;

[TestClass]
public class HeroesMathTests
{
    [TestMethod]
    [DataRow(100, "(12 + 6.000000) * (0.1875 + 0.062500) - (12 * 0.1875) / (12 * 0.1875) * 100")]
    [DataRow(50, "17 / 34 * 100")]
    [DataRow(70, "(57.8 / 34) * 100 - 100")]
    [DataRow(39.99999999999999, "-100*(1-1.400000)")]
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
    [DataRow(60.00000000000001, "((5) + (3) / 5 - 1) * 100")]
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
        HeroesMath.Compute(expression).Should().Be(expected);
        HeroesMath.Compute(expression.AsSpan()).Should().Be(expected);
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
            results.Add(HeroesMath.Compute(x));
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
        Action act = () => HeroesMath.Compute(expression);

        // assert
        act.Should().Throw<SyntaxErrorException>();
    }
}