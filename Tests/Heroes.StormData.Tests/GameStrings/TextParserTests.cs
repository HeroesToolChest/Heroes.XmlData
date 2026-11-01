namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class TextParserTests
{
    private const string Text = "Deal X damage";

    [TestMethod]
    public void BuildDescription_ValueHasScaling_WritesValueAndScaling()
    {
        // arrange
        List<TextSectionItem> textStack = CreateTextStack(new ValueScale(120, 0.04));

        // act
        string result = BuildDescription(textStack);

        // assert
        result.Should().Be("Deal 120<c val=\"#ColorGray\">~~0.04~~</c> damage");
    }

    [TestMethod]
    public void BuildDescription_ValueHasScalingAndIsPercent_WritesPercentBeforeScaling()
    {
        // arrange
        List<TextSectionItem> textStack = CreateTextStack(new ValueScale(25, 0.04), isPercent: true);

        // act
        string result = BuildDescription(textStack);

        // assert
        result.Should().Be("Deal 25%<c val=\"#ColorGray\">~~0.04~~</c> damage");
    }

    [TestMethod]
    public void BuildDescription_ValueRoundedToNegativeZero_WritesZero()
    {
        // arrange
        List<TextSectionItem> textStack = CreateTextStack(new ValueScale(Math.Round(-0.4), 0.04));

        // act
        string result = BuildDescription(textStack);

        // assert
        result.Should().Be("Deal 0<c val=\"#ColorGray\">~~0.04~~</c> damage");
    }

    [TestMethod]
    public void BuildDescription_ValueSmallEnoughForExponentNotation_WritesFixedPoint()
    {
        // arrange
        List<TextSectionItem> textStack = CreateTextStack(new ValueScale(0.00001));

        // act
        string result = BuildDescription(textStack);

        // assert
        result.Should().Be("Deal 0.00001 damage");
    }

    [TestMethod]
    public void BuildDescription_CurrentCultureUsesCommaDecimalSeparator_WritesInvariantNumbers()
    {
        // arrange
        List<TextSectionItem> textStack = CreateTextStack(new ValueScale(1.5, 0.025));
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            string result = BuildDescription(textStack);

            // assert
            result.Should().Be("Deal 1.5<c val=\"#ColorGray\">~~0.025~~</c> damage");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    // "Deal " + value + " damage"
    private static List<TextSectionItem> CreateTextStack(ValueScale valueScale, bool isPercent = false) =>
    [
        new TextSectionItem(new Range(0, 5)),
        new TextSectionItem(valueScale, isPercent),
        new TextSectionItem(new Range(6, Text.Length)),
    ];

    private static string BuildDescription(List<TextSectionItem> textStack)
    {
        TextParser textParser = new(new DataTagParser(new LayeredDataStorage()), Text);

        return textParser.BuildDescription(textStack);
    }
}
