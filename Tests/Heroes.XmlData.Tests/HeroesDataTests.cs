using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.ObjectModel;

namespace Heroes.XmlData.Tests;

[TestClass]
public class HeroesDataTests
{
    [TestMethod]
    public void ParseGameString_ConstElement_ParsedGameString()
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

        // act
        TooltipDescription parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawDescription.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseGameString_HasAScalingValuePercent_ParsedGameString()
    {
        // arrange
        string description = "Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\"><d ref=\"Effect,OctoGrabPokeMasteryDamage,Amount * 100\"/>%</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "OctoGrabPokeMasteryDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "137"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "MurkyOctoGrab"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "OctoGrabPokeMasteryDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        TooltipDescription parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawDescription.Should().Be("Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\">13700%~~0.04~~</c>");
    }

    [TestMethod]
    [TestCategory("GameStrings")]
    public void ParseGameString_ConstantsWithExpressions_ParsedGameString()
    {
        // arrange
        string description = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\"><d ref=\"Behavior,DVaMechSelfDestructMechDetonationCountdown,Duration\" player=\"0\"/></c> seconds. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount+Accumulator,DVaSelfDestructDistanceAccumulator,MinAccumulation\"/></c> to <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount\"/></c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,AttributeFactor[Structure]*(-100)\"/>%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("Effect", "CEffectDamage"),
                    ("Accumulator", "CAccumulatorDistance"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "DVaMechSelfDestructMechDetonationCountdown"),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "4"))),
                    new(
                        "CAccumulatorDistance",
                        new XAttribute("id", "DVaSelfDestructDistanceAccumulator"),
                        new XElement(
                            "MinAccumulation",
                            new XAttribute("value", "$DVaMechSelfDestructDetonationMinimumAccumulation"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "DVaMechSelfDestructDetonationSearchDamage"),
                        new XElement(
                            "AttributeFactor",
                            new XAttribute("index", "Structure"),
                            new XAttribute("value", "-0.5")),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "$DVaMechSelfDestructDetonationMaximumDamage"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "DVaMechSelfDestruct"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "DVaMechSelfDestructDetonationSearchDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000"))),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Accumulator")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "DVaSelfDestructDistanceAccumulator")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "MinAccumulation")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                })
                .AddConstantXElements(new List<XElement>()
                {
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMaximumDamage"),
                        new XAttribute("value", "1100")),
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMinimumDamage"),
                        new XAttribute("value", "400")),
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMinimumAccumulation"),
                        new XAttribute("value", "-($DVaMechSelfDestructDetonationMinimumDamage $DVaMechSelfDestructDetonationMaximumDamage)"),
                        new XAttribute("evaluateAsExpression", "1")),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        TooltipDescription parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawDescription.Should().Be("Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400~~0.04~~</c> to <c val=\"#TooltipNumbers\">1100~~0.04~~</c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.<n/><n/><c val=\"FF8000\">Gain </c><c val=\"#TooltipNumbers\">1%</c><c val=\"FF8000\"> Charge for every </c><c val=\"#TooltipNumbers\">2</c><c val=\"FF8000\"> seconds spent Basic Attacking, and </c><c val=\"#TooltipNumbers\">25%</c><c val=\"FF8000\"> Charge per </c><c val=\"#TooltipNumbers\">100%</c><c val=\"FF8000\"> of Mech Health lost.</c>");
    }
}