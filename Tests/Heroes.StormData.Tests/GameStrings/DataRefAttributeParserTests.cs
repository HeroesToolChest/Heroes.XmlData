using Heroes.StormData.GameStrings;

namespace Heroes.StormData.Tests.GameStrings;

[TestClass]
public class DataRefAttributeParserTests
{
    [TestMethod]
    public void Test()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        DataTagParser dataTagParser = new(layeredDataStorage);

        DataRefAttributeParser parser = new(layeredDataStorage, dataTagParser);

        // act
        parser.Parse("Abil,GuldanHorrify, CastIntroTime + Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]");

        // assert
    }

    [TestMethod]
    public void Parse_HasValueSmallEnoughForExponentNotation_ComputesValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();

        // act
        ValueScale result = parser.Parse("0.00001*100000");

        // assert
        result.Value.Should().Be(1);
    }

    [TestMethod]
    public void Parse_CurrentCultureUsesCommaDecimalSeparator_ComputesValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            ValueScale result = parser.Parse("1.5*2");

            // assert
            result.Value.Should().Be(3);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    [DataRow(0.04)]
    [DataRow(0.14)] // 0.14 + 1 is 1.1400000000000001
    [DataRow(0.57)] // 0.57 + 1 is 1.5699999999999998
    [DataRow(-0.04)] // multiplier is 0.96
    public void CalculateExpression_ValueHasScaling_ReturnsValueAndScaling(double scaling)
    {
        // arrange
        List<TextSectionItem> dataRefParts = [new TextSectionItem(new ValueScale(5, scaling))];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(string.Empty, dataRefParts);

        // assert
        result.Value.Should().Be(5);
        result.Scaling.Should().BeApproximately(scaling, 0.000001);
    }

    [TestMethod]
    public void CalculateExpression_OperatorBetweenValuesWithScaling_ReturnsCombinedScaling()
    {
        // arrange
        string dRef = "a+b";
        List<TextSectionItem> dataRefParts =
        [
            new TextSectionItem(new ValueScale(5, 0.14)),
            new TextSectionItem(new Range(1, 2)), // +
            new TextSectionItem(new ValueScale(3)),
        ];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(dRef, dataRefParts);

        // assert
        result.Value.Should().Be(8);
        result.Scaling.Should().BeApproximately(0.0875, 0.000001); // (5 * 1.14 + 3) / 8 - 1
    }

    [TestMethod]
    public void CalculateExpression_BaseValueIsZero_ReturnsNoScaling()
    {
        // arrange
        List<TextSectionItem> dataRefParts = [new TextSectionItem(new ValueScale(0, 0.04))];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(string.Empty, dataRefParts);

        // assert
        result.Value.Should().Be(0);
        result.Scaling.Should().BeNull();
    }

    private static DataRefAttributeParser CreateParser()
    {
        LayeredDataStorage layeredDataStorage = new();

        return new DataRefAttributeParser(layeredDataStorage, new DataTagParser(layeredDataStorage));
    }
}
