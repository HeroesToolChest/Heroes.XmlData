using System.Runtime.CompilerServices;

namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class TextParserTests
{
    [TestMethod]
    public void AppendValue_ValueHasScaling_WritesValueAndScaling()
    {
        // act
        string result = AppendValue(new ValueScale(120, 0.04));

        // assert
        result.Should().Be("120<c val=\"#ColorGray\">~~0.04~~</c>");
    }

    [TestMethod]
    public void AppendValue_ValueHasScalingAndIsPercent_WritesPercentBeforeScaling()
    {
        // act
        string result = AppendValue(new ValueScale(25, 0.04), isPercent: true);

        // assert
        result.Should().Be("25%<c val=\"#ColorGray\">~~0.04~~</c>");
    }

    [TestMethod]
    public void AppendValue_NoScalingAndIsPercent_WritesValueOnly()
    {
        // act
        string result = AppendValue(new ValueScale(25), isPercent: true);

        // assert
        result.Should().Be("25");
    }

    [TestMethod]
    public void AppendValue_ValueRoundedToNegativeZero_WritesZero()
    {
        // act
        string result = AppendValue(new ValueScale(Math.Round(-0.4), 0.04));

        // assert
        result.Should().Be("0<c val=\"#ColorGray\">~~0.04~~</c>");
    }

    [TestMethod]
    public void AppendValue_ValueSmallEnoughForExponentNotation_WritesFixedPoint()
    {
        // act
        string result = AppendValue(new ValueScale(0.00001));

        // assert
        result.Should().Be("0.00001");
    }

    [TestMethod]
    public void AppendValue_CurrentCultureUsesCommaDecimalSeparator_WritesInvariantNumbers()
    {
        // arrange
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            string result = AppendValue(new ValueScale(1.5, 0.025));

            // assert
            result.Should().Be("1.5<c val=\"#ColorGray\">~~0.025~~</c>");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    public void Parse_NoDataTags_ReturnsTextUnchanged()
    {
        // arrange
        string text = "Deal <c val=\"#TooltipNumbers\">50</c> damage.<n/>";

        // act
        string result = Parse(text);

        // assert
        result.Should().Be(text);
    }

    [TestMethod]
    public void Parse_HasDataTag_ReplacesTagWithValue()
    {
        // act
        string result = Parse("Deal <d ref=\"120\"/> damage");

        // assert
        result.Should().Be("Deal 120 damage");
    }

    [TestMethod]
    public void Parse_PercentAfterValueWithNoScaling_KeepsPercent()
    {
        // act
        string result = Parse("Deal <d ref=\"20\"/>% more damage");

        // assert
        result.Should().Be("Deal 20% more damage");
    }

    [TestMethod]
    public void Parse_DataTagIsNotClosed_DropsTheRestOfTheText()
    {
        // act
        string result = Parse("Deal <d ref=\"120\" damage");

        // assert
        result.Should().Be("Deal ");
    }

    [TestMethod]
    public void Parse_DataTagIsNotClosedBeforeTheNextTag_DropsItUpToTheNextTag()
    {
        // act
        string result = Parse("Deal <d ref=\"120\" <c val=\"#TooltipNumbers\"><d ref=\"5\"/></c> damage");

        // assert
        result.Should().Be("Deal <c val=\"#TooltipNumbers\">5</c> damage");
    }

    [TestMethod]
    public void Parse_TextLongerThanTheStackBuffer_ReturnsWholeText()
    {
        // arrange
        string part = "Deal <c val=\"#TooltipNumbers\"><d ref=\"120\"/></c> damage. ";
        string expectedPart = "Deal <c val=\"#TooltipNumbers\">120</c> damage. ";

        string text = string.Concat(Enumerable.Repeat(part, 40));

        // act
        string result = Parse(text);

        // assert
        result.Should().Be(string.Concat(Enumerable.Repeat(expectedPart, 40)));
    }

    private static string AppendValue(ValueScale valueScale, bool isPercent = false)
    {
        DefaultInterpolatedStringHandler builder = new(0, 0, CultureInfo.InvariantCulture);

        TextParser.AppendValue(ref builder, valueScale, isPercent);

        return builder.ToStringAndClear();
    }

    private static string Parse(string text)
    {
        return new TextParser(new DataTagParser(new LayeredDataStorage()), text, CultureInfo.InvariantCulture).Parse();
    }
}
