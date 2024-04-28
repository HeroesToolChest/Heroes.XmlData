using Heroes.LocaleText;

namespace Heroes.XmlData.Extensions.GameStrings.Tests;

[TestClass]
public class GameStringParserTests
{
    public GameStringParserTests()
    {
    }

    [TestMethod]
    public void ParseTooltipDescriptionTest()
    {
        // arrange
        string id = "Button/Tooltip/YrelSacredGround";
        string description = "Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\"><d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/></c> Armor until she leaves the area.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .AddConstantElements(new List<XElement>()
            {
                new(
                    "const",
                    new XAttribute("id", "$YrelSacredGroundArmorBonus"),
                    new XAttribute("value", "50")),
            });

        HeroesData heroesData = loader.HeroesData;

        GameStringParser gameStringParser = GameStringParser.GetInstance(heroesData);
        // act

        gameStringParser.ParseTooltipDescription(id, description);

        // assert
    }

    [TestMethod]
    public void ParseTooltipDescription2Test()
    {
        // arrange
        string id = "Button/Tooltip/AbathurSymbioteCarapace";
        string description = "Shields the assisted ally for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CarapaceEvolutionShieldTooltipDummy,DamageResponse.ModifyLimit\"/></c>. Allied Heroes are healed for <c val=\"#TooltipNumbers\"><d ref=\"Effect,RegenerativeMicrobesCreateHealer,RechargeVitalRate\"/></c> Health per second while the Shield is active. Lasts for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CarapaceEvolutionShield,Duration\" player=\"0\"/></c> seconds.";

        TooltipDescription tooltipDescription = new TooltipDescription(description);
        string a =tooltipDescription.RawDescription;

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .AddConstantElements(new List<XElement>()
            {
                new(
                    "const",
                    new XAttribute("id", "$YrelSacredGroundArmorBonus"),
                    new XAttribute("value", "50")),
            });

        HeroesData heroesData = loader.HeroesData;

        GameStringParser gameStringParser = GameStringParser.GetInstance(heroesData);
        // act

        gameStringParser.ParseTooltipDescription(id, description);

        // assert
    }
}