using Heroes.LocaleText;

namespace Heroes.XmlData.GameStrings.Tests;

[TestClass]
public class GameStringParserTests
{
    public GameStringParserTests()
    {
    }

    [TestMethod]
    public void ParseTooltipDescription_ConstElement_ParsedGameString()
    {
        // arrange
        string description = "Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\"><d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/></c> Armor until she leaves the area.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddConstantXElements(new List<XElement>()
                {
                    new(
                        "const",
                        new XAttribute("id", "$YrelSacredGroundArmorBonus"),
                        new XAttribute("value", "50")),
                }));

        HeroesData heroesData = loader.HeroesData;

        GameStringParser gameStringParser = new(heroesData);

        // act
        string parsed = gameStringParser.ParseTooltipDescription(description);

        // assert
        parsed.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NormalGameString1_ParsedGameString()
    {
        // arrange
        string description = "After <c val=\"#TooltipNumbers\"><d ref=\"Abil,GuldanHorrify, CastIntroTime + Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]\" precision=\"2\"/></c> seconds, deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,GuldanHorrifyDamage,Amount\"/></c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,GuldanHorrifyFearDuration,Duration\"/></c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Abil", "CAbilEffectTarget"),
                    ("Effect", "CEffectCreatePersistent"),
                    ("Effect", "CEffectDamage"),
                    ("Behavior", "CBehaviorBuff"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CAbilEffectTarget",
                        new XAttribute("id", "GuldanHorrify")),
                    new(
                        "CEffectCreatePersistent",
                        new XAttribute("id", "GuldanHorrifyAbilityStartCreatePersistent"),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0.5"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "GuldanHorrifyDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "120"))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "GuldanHorrifyFearDuration"),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "2"))),
                })
                .AddLevelScalingElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "GuldanHorrify"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "GuldanHorrifyDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        GameStringParser gameStringParser = new(heroesData);

        // act
        string parsed = gameStringParser.ParseTooltipDescription(description);

        // assert
        parsed.Should().Be("After <c val=\"#TooltipNumbers\">0.5</c> seconds, deal <c val=\"#TooltipNumbers\">120~~0.04~~</c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\">2</c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.");
    }

    [TestMethod]
    public void ParseTooltipDescription2Test()
    {
        // arrange
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

        GameStringParser gameStringParser = new(heroesData);
        // act

        gameStringParser.ParseTooltipDescription(description);

        // assert
    }
}