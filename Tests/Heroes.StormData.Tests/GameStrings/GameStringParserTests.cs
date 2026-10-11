namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class GameStringParserTests
{
    [TestMethod]
    public void ParseText_HasConstDataRef_ReturnsParsedText()
    {
        // arrange
        string text = "Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\"><d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/></c> Armor until she leaves the area.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Base);

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$YrelSacredGroundArmorBonus" value="50" />
            """));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string result = parser.ParseText(text);

        // assert
        result.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }
}
