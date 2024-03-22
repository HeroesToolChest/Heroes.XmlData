using System.Collections.Concurrent;
using System.Data;

namespace Heroes.XmlData.Extensions.Tests;

[TestClass]
public class HeroesPrefixNotationTests
{
    public HeroesPrefixNotationTests()
    {
    }

    [TestMethod]
    [DataRow(4, "4")]
    [DataRow(0, "d")]
    [DataRow(0, "*")]
    [DataRow(-90, "negate(90)")]
    [DataRow(1, "/(4 4)")]
    [DataRow(0, "/(0 4)")]
    [DataRow(4, "/(4 0)")]
    [DataRow(0.0125, "/(*(0.1 0.5) 4)")]
    [DataRow(2.125, "/(*(+(-(10.23 0.23) 7) 0.5) 4)")]
    [DataRow(77335.205, "/(*(+(-(-(1000.5675 234.12345) 0.232) 7.14) 0.5) 0.005)")]
    [DataRow(-1, "/(negate(4) 4)")]
    [DataRow(-0.125, "/(*(0.1 negate(5)) 4)")]
    [DataRow(-0.0125, "/(*(0.1 negate(0.5)) 4)")]
    [DataRow(200, "+(1000 negate(800))")]
    [DataRow(0.0062499999999999995, "/(0.3 *(3 16))")]
    [DataRow(82.5, "+(-(100 60) *(5 8.5))")]
    public void Compute_ValidNumberExpressions_ReturnsValue(double expected, string expression)
    {
        // arrange
        // act
        double result = HeroesPrefixNotation.Compute(null!, expression);

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void Compute_AllMultiThreading_ReturnsValues()
    {
        // arrange
        List<string> expressions = [];
        ConcurrentBag<double> results = [];

        for (int i = 0; i < 100; i++)
        {
            expressions.Add("/(*(+(-(10.23 0.23) 7) 0.5) 4)");
        }

        // act
        Parallel.ForEach(expressions, (x) =>
        {
            results.Add(HeroesPrefixNotation.Compute(null!, x));
        });

        // assert
        results.Should().AllBeEquivalentTo(2.125);
    }

    [TestMethod]
    public void Compute_InvalidExpression_ThrowException()
    {
        // arrange
        string expression = "*(1(*a4/z)+0*100";

        // act
        Action act = () => HeroesPrefixNotation.Compute(null!, expression);

        // assert
        act.Should().Throw<SyntaxErrorException>();
    }

    [TestMethod]
    public void Compute_WithConstantInExpression_ReturnsValue()
    {
        // arrange
        string expression = "-(/(0.75 $GazloweDethLazorLeechAmount) 1)";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .AddConstantElements(new List<XElement>()
            {
                new(
                    "const",
                    new XAttribute("id", "$GazloweDethLazorLeechAmount"),
                    new XAttribute("value", "0.25")),
            });

        IHeroesData heroesData = loader.HeroesData;

        // act
        double result = HeroesPrefixNotation.Compute(heroesData, expression);

        // assert
        result.Should().Be(2);
    }

    [TestMethod]
    public void Compute_WithConstantNotFound_ReturnsValue()
    {
        // arrange
        string expression = "-(/(0.75 $DoesNotExist) 1)";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .AddConstantElements(new List<XElement>()
            {
                new(
                    "const",
                    new XAttribute("id", "$GazloweDethLazorLeechAmount"),
                    new XAttribute("value", "0.25")),
            });

        IHeroesData heroesData = loader.HeroesData;

        // act
        double result = HeroesPrefixNotation.Compute(heroesData, expression);

        // assert
        result.Should().Be(-0.25);
    }
}