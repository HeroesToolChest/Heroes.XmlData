namespace Heroes.XmlData.GameStrings.Tests;

[TestClass]
public class GameStringParserTests
{
    [TestMethod]
    public void ParseTooltipDescription_ConstElement_ParsedGameString()
    {
        // arrange
        string description = "Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\"><d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/></c> Armor until she leaves the area.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseTooltipDescription_CastIntroTimeHasDefaultValue_ParsedGameString()
    {
        // arrange
        string description = "After <c val=\"#TooltipNumbers\"><d ref=\"Abil,GuldanHorrify, CastIntroTime + Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]\" precision=\"2\"/></c> seconds, deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,GuldanHorrifyDamage,Amount\"/></c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,GuldanHorrifyFearDuration,Duration\"/></c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
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
                        "CAbil",
                        new XAttribute("default", "1")),
                    new(
                        "CAbilEffectTarget",
                        new XAttribute("default", "1")),
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
                .AddLevelScalingArrayElements(new List<XElement>()
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

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("After <c val=\"#TooltipNumbers\">0.5</c> seconds, deal <c val=\"#TooltipNumbers\">120~~0.04~~</c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\">2</c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NormalGameString2_ParsedGameString()
    {
        // arrange
        string description = "Rain a small army of Demonic Grunts down on enemies, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionImpactDamage,Amount\"/></c> damage per impact. Grunts deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionDemonGruntWeaponDamage,Amount\"/></c> damage, have <c val=\"#TooltipNumbers\"><d ref=\"Unit,AzmodanDemonicInvasionDemonGrunt,LifeMax\"/></c> Health and last up to <c val=\"#TooltipNumbers\"><d ref=\"-Unit,AzmodanDemonicInvasionDemonGrunt,LifeMax/Unit,AzmodanDemonicInvasionDemonGrunt,LifeRegenRate\"/></c> seconds. When Grunts die they explode, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionExplodeDamage,Amount\"/></c> damage to nearby enemies, doubled against enemy Heroes.<n/><n/>Usable while Channeling All Shall Burn.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                    ("Unit", "CUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "AzmodanDemonicInvasionImpactDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "85")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0.25")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0.25"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "AzmodanDemonicInvasionDemonGruntWeaponDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "39"))),
                    new(
                        "CUnit",
                        new XAttribute("id", "AzmodanDemonicInvasionDemonGrunt"),
                        new XElement(
                            "LifeMax",
                            new XAttribute("value", "750")),
                        new XElement(
                            "LifeRegenRate",
                            new XAttribute("value", "-75"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "AzmodanDemonicInvasionExplodeDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "40"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "AzmodanDemonicInvasion"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AzmodanDemonicInvasionImpactDamage")),
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
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AzmodanDemonicInvasionDemonGruntWeaponDamage")),
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
                                new XAttribute("value", "Unit")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AzmodanDemonicInvasionDemonGrunt")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "LifeMax")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000"))),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Unit")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AzmodanDemonicInvasionDemonGrunt")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "LifeRegenRate")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "AzmodanDemonicInvasion"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AzmodanDemonicInvasionExplodeDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Rain a small army of Demonic Grunts down on enemies, dealing <c val=\"#TooltipNumbers\">85~~0.04~~</c> damage per impact. Grunts deal <c val=\"#TooltipNumbers\">39~~0.04~~</c> damage, have <c val=\"#TooltipNumbers\">750~~0.04~~</c> Health and last up to <c val=\"#TooltipNumbers\">10</c> seconds. When Grunts die they explode, dealing <c val=\"#TooltipNumbers\">40~~0.04~~</c> damage to nearby enemies, doubled against enemy Heroes.<n/><n/>Usable while Channeling All Shall Burn.");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasParenthesisAndBrackets_ParsedGameString()
    {
        // arrange
        string description = "Toxic Nests deal <c val=\"#TooltipNumbers\"><d ref=\"(Effect,AbathurToxicNestEnvenomedNestDamage,Amount* [d ref='Behavior,AbathurToxicNestEnvenomedNest,PeriodCount' player='0'/])/Effect,ToxicNestDamage,Amount*100\"/>%</c> more damage over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNest,Duration\" player=\"0\"/></c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\"><d ref=\"-Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,ArmorModification.AllArmorBonus\"/></c> for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,Duration\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                    ("Behavior", "CBehaviorBuff"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "AbathurToxicNestEnvenomedNestDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "38.25"))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "AbathurToxicNestEnvenomedNest"),
                        new XElement(
                            "PeriodCount",
                            new XAttribute("value", "3")),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "3"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "ToxicNestDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "153"))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "AbathurToxicNestEnvenomedNestArmorDebuff"),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "4")),
                        new XElement(
                            "ArmorModification",
                            new XElement(
                                "AllArmorBonus",
                                new XAttribute("value", "-10")))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "AbathurToxicNest"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "AbathurToxicNestEnvenomedNestDamage")),
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
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "ToxicNestDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Toxic Nests deal <c val=\"#TooltipNumbers\">75%</c> more damage over <c val=\"#TooltipNumbers\">3</c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\">10</c> for <c val=\"#TooltipNumbers\">4</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NoScalingWithSingleDigitPercent_ParsedGameString()
    {
        // arrange
        string description = "Zarya's Basic Attack deals <c val=\"bfd4fd\"><d ref=\"(Effect,ZaryaWeaponFeelTheHeatDamage,Amount/Effect,ZaryaWeaponDamage,Amount)-1*10)\" />0%</c> additional damage to enemies in melee range.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "ZaryaWeaponFeelTheHeatDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "30"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "ZaryaWeaponDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "20"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "ZaryaWeaponDamage")),
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
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "ZaryaWeaponFeelTheHeatDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Zarya's Basic Attack deals <c val=\"bfd4fd\">50%</c> additional damage to enemies in melee range.");
    }

    [TestMethod]
    public void ParseTooltipDescription_CostIsAnArray_ParsedGameString()
    {
        // arrange
        string description = "If Sand Blast travels at least <c val=\"bfd4fd\"><d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\"><d ref=\"Effect,ChromieSandBlastFastForwardCooldownReduction,Cost[0].CooldownTimeUse\" precision=\"2\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Validator", "CValidatorLocationCompareRange"),
                    ("Effect", "CEffectLaunchMissile"),
                    ("Effect", "CEffectModifyUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CValidatorLocationCompareRange",
                        new XAttribute("id", "ChromieFastForwardDistanceCheck"),
                        new XElement(
                            "Range",
                            new XAttribute("value", "7.5"))),
                    new(
                        "CEffectLaunchMissile",
                        new XAttribute("id", "ChromieSandBlastLaunchMissile"),
                        new XElement(
                            "ImpactLocation",
                            new XElement(
                                "ProjectionDistanceScale",
                                new XAttribute("value", "15")))),
                    new(
                        "CEffectModifyUnit",
                        new XAttribute("id", "ChromieSandBlastFastForwardCooldownReduction"),
                        new XElement(
                            "Cost",
                            new XAttribute("Abil", "ChromieSandBlast,Execute"),
                            new XAttribute("CooldownTimeUse", "0.5"))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("If Sand Blast travels at least <c val=\"bfd4fd\">50%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\">0.5</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NegativeValueFromDRef_ParsedGameString()
    {
        // arrange
        string description = "reduces its cooldown by <c val=\"#TooltipNumbers\"><d ref=\"(1-Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse)*-1\"/></c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectModifyUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectModifyUnit",
                        new XAttribute("id", "AnduinHolyWordSalvationLightOfStormwindCooldownReduction"),
                        new XElement(
                            "Cost",
                            new XAttribute("CooldownTimeUse", "-60"))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("reduces its cooldown by <c val=\"#TooltipNumbers\">60</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasAScalingValuePercent_ParsedGameString()
    {
        // arrange
        string description = "Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\"><d ref=\"Effect,OctoGrabPokeMasteryDamage,Amount * 100\"/>%</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\">13700~~0.04~~%</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasConstInValues_ParsedGameString()
    {
        // arrange
        string description = "Globe of Annihilation deals <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,AzmodanGlobeOfAnnihilationDamage,MultiplicativeModifierArray[Greed].Modifier\"player=\"0\"/>%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedTalentTokenCounter,Max\"/></c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanGlobeOfAnnihilationGreedAllShallBurnRangeIncreaseModifyCatalog,CatalogModifications[0].Value/Abil,AzmodanAllShallBurn,Range*100\" player=\"0\"/>%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedMovementSpeed,Modification.UnifiedMoveSpeedFactor*100\"/>%</c> Attack Speed and Movement Speed.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                    ("Effect", "CEffectModifyCatalogNumeric"),
                    ("Abil", "CAbilEffectTarget"),
                    ("Behavior", "CBehaviorTokenCounter"),
                    ("Behavior", "CBehaviorBuff"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "AzmodanGlobeOfAnnihilationDamage"),
                        new XElement(
                            "MultiplicativeModifierArray",
                            new XAttribute("index", "Greed"),
                            new XAttribute("Modifier", "0.15"))),
                    new(
                        "CBehaviorTokenCounter",
                        new XAttribute("id", "AzmodanGlobeOfAnnihilationGreedTalentTokenCounter"),
                        new XElement(
                            "Max",
                            new XAttribute("value", "200"))),
                    new(
                        "CEffectModifyCatalogNumeric",
                        new XAttribute("id", "AzmodanGlobeOfAnnihilationGreedAllShallBurnRangeIncreaseModifyCatalog"),
                        new XElement(
                            "CatalogModifications",
                            new XAttribute("Reference", "Abil,AzmodanAllShallBurn,Range"),
                            new XElement(
                                "Value",
                                new XAttribute("value", "$AzmodanAllShallBurnGreedBonusRange"))),
                        new XElement(
                            "CatalogModifications",
                            new XAttribute("Reference", "Actor,AzmodanAllShallBurnRangeSplat,Scale"),
                            new XElement(
                                "Value",
                                new XAttribute("value", "$AzmodanAllShallBurnGreedBonusRange")))),
                    new(
                        "CAbilEffectTarget",
                        new XAttribute("id", "AzmodanAllShallBurn"),
                        new XElement(
                            "Range",
                            new XAttribute("value", "$AzmodanAllShallBurnCastRange"))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "AzmodanGlobeOfAnnihilationGreedMovementSpeed"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "UnifiedMoveSpeedFactor",
                                new XAttribute("value", "0.2")))),
                })
                .AddConstantXElements(new List<XElement>()
                {
                    new(
                        "const",
                        new XAttribute("id", "$AzmodanAllShallBurnCastRange"),
                        new XAttribute("value", "6")),
                    new(
                        "const",
                        new XAttribute("id", "$AzmodanAllShallBurnGreedBonusRange"),
                        new XAttribute("value", "1.5")),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Globe of Annihilation deals <c val=\"#TooltipNumbers\">15%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\">200</c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\">25%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\">20%</c> Attack Speed and Movement Speed.");
    }

    [TestMethod]
    public void ParseTooltipDescription_SpacingInDRef_ParsedGameString()
    {
        // arrange
        string description = "Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\"><d ref=\"Unit,RagnarosBigRag,LifeMax\"/></c> Health that burns away over <c val=\"#TooltipNumbers\"><d ref=\"(Unit,RagnarosBigRag,LifeMax / Unit,RagnarosBigRag,LifeRegenRate) * (-1)\"/></c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Unit", "CUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CUnit",
                        new XAttribute("id", "RagnarosBigRag"),
                        new XElement(
                            "LifeMax",
                            new XAttribute("value", "3996")),
                        new XElement(
                            "LifeRegenRate",
                            new XAttribute("value", "-222"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "RagnarosMoltenCore"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Unit")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "RagnarosBigRag")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "LifeMax")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000"))),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Unit")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "RagnarosBigRag")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "LifeRegenRate")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\">3996~~0.04~~</c> Health that burns away over <c val=\"#TooltipNumbers\">18</c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.");
    }

    [TestMethod]
    public void ParseTooltipDescription_DRefHasDefaultIndexerForScaling_ParsedGameString()
    {
        // arrange
        string description = "Deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,MultishotDamage,AmountArray\"/></c> damage to enemies within the target area.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "MultishotDamage"),
                        new XElement(
                            "AmountArray",
                            new XAttribute("index", "Quest"),
                            new XAttribute("value", "159"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "DemonHunterMultishot"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "MultishotDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "AmountArray[Quest]")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Deal <c val=\"#TooltipNumbers\">159~~0.04~~</c> damage to enemies within the target area.");
    }

    [TestMethod]
    public void ParseTooltipDescription_IndexerLessInLevelScalingArray_ParsedGameString()
    {
        // arrange
        string description = "Shields Tyrael for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,TyraelRighteousnessShield,DamageResponse[0].ModifyLimit[0]\"/></c> damage and nearby allied Heroes and Minions for <c val=\"#TooltipNumbers\">40%</c> as much for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,TyraelRighteousnessShield,Duration[0]\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "TyraelRighteousnessShield"),
                        new XElement(
                            "DamageResponse",
                            new XAttribute("Exhausted", "RighteousnessSalvationCreateHealer"),
                            new XAttribute("ModifyLimit", "336")),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "4"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "TyraelRighteousness"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Behavior")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "TyraelRighteousnessShield")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "DamageResponse.ModifyLimit")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Shields Tyrael for <c val=\"#TooltipNumbers\">336~~0.04~~</c> damage and nearby allied Heroes and Minions for <c val=\"#TooltipNumbers\">40%</c> as much for <c val=\"#TooltipNumbers\">4</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasArraysWithAndWithoutIndex_ParsedGameString()
    {
        // arrange
        string description = "<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodCount\"/></c> shots over <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[1]+Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[2]\" precision=\"2\"/></c> seconds. Each shot deals <c val=\"#TooltipNumbers\"><d ref=\"1+Effect,DVaPilotBigShotDamage,MultiplicativeModifierArray[PewPewPew].Modifier*100\"player=\"0\"/>%</c> damage.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectCreatePersistent"),
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectCreatePersistent",
                        new XAttribute("id", "DVaBigShotPewPewPewOffsetPeriodic"),
                        new XElement(
                            "PeriodCount",
                            new XAttribute("value", "3")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0.25")),
                        new XElement(
                            "PeriodicPeriodArray",
                            new XAttribute("value", "0.25"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "DVaPilotBigShotDamage"),
                        new XElement(
                            "MultiplicativeModifierArray",
                            new XAttribute("index", "PewPewPew"),
                            new XAttribute("Modifier", "-0.25")),
                        new XElement(
                            "MultiplicativeModifierArray",
                            new XAttribute("index", "Headshot"),
                            new XAttribute("Modifier", "0.75"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "DVaPilotBigShot"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "DVaPilotBigShotDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\">3</c> shots over <c val=\"#TooltipNumbers\">0.5</c> seconds. Each shot deals <c val=\"#TooltipNumbers\">75%</c> damage.");
    }

    [TestMethod]
    public void ParseTooltipDescription_ConstantsWithExpressions_ParsedGameString()
    {
        // arrange
        string description = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\"><d ref=\"Behavior,DVaMechSelfDestructMechDetonationCountdown,Duration\" player=\"0\"/></c> seconds. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount+Accumulator,DVaSelfDestructDistanceAccumulator,MinAccumulation\"/></c> to <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount\"/></c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,AttributeFactor[Structure]*(-100)\"/>%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400~~0.04~~</c> to <c val=\"#TooltipNumbers\">1100~~0.04~~</c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasArrayOnLastFieldOnly_ParsedGameString()
    {
        // arrange
        string description = "Transform for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,MuradinAvatarHealthBuff,Duration\" player=\"0\" precision=\"2\"/></c> seconds, gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,MuradinAvatar,Modification.VitalMaxArray[0]\"/></c> Health.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("AbilEffect", "CAbilEffectInstant"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "MuradinAvatarHealthBuff"),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "20"))),
                    new(
                        "CAbilEffectInstant",
                        new XAttribute("id", "MuradinAvatar")),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "MuradinAvatar"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "VitalMaxArray",
                                new XAttribute("index", "Life"),
                                new XAttribute("value", "1000")))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "MuradinAvatar"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Behavior")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "MuradinAvatar")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Modification.VitalMaxArray[0]")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Transform for <c val=\"#TooltipNumbers\">20</c> seconds, gaining <c val=\"#TooltipNumbers\">1000~~0.04~~</c> Health.");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasIndexedArray_ParsedGameString()
    {
        // arrange
        string description = "While at or below <c val=\"#TooltipNumbers\">50</c> Brew, gain <c val=\"#TooltipNumbers\"><d ref=\"100*Behavior,ChenBrewmastersBalanceSpeedBuff,Modification.UnifiedMoveSpeedFactor\"/>%</c> Movement Speed. While at or above <c val=\"#TooltipNumbers\">50</c> Brew, regenerate an additional <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ChenBrewmastersBalanceHealthRegen,Modification.VitalRegenArray[Life]\"/></c> Health per second.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("AbilEffect", "CAbilEffectInstant"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ChenBrewmastersBalanceSpeedBuff"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "UnifiedMoveSpeedFactor",
                                new XAttribute("value", "0.2")))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ChenBrewmastersBalanceHealthRegen"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "VitalRegenArray",
                                new XAttribute("index", "Life"),
                                new XAttribute("value", "18")))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "ChenFortifyingBrew"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Behavior")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "ChenBrewmastersBalanceHealthRegen")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Modification.VitalRegenArray[Life]")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("While at or below <c val=\"#TooltipNumbers\">50</c> Brew, gain <c val=\"#TooltipNumbers\">20%</c> Movement Speed. While at or above <c val=\"#TooltipNumbers\">50</c> Brew, regenerate an additional <c val=\"#TooltipNumbers\">18~~0.04~~</c> Health per second.");
    }

    [TestMethod]
    public void ParseTooltipDescription_ElementArray_ParsedGameString()
    {
        // arrange
        string description = "Increase Hardened Carapace's Spell Armor by <c val=\"#TooltipNumbers\"><d ref=\"Talent,AnubarakNerubianArmor,AbilityModificationArray[0].Modifications[0].Value\"/></c>.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Talent", "CTalent"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CTalent",
                        new XAttribute("id", "AnubarakNerubianArmor"),
                        new XElement(
                            "AbilityModificationArray",
                            new XElement(
                                "Modifications",
                                new XElement(
                                    "Field",
                                    new XAttribute("value", "ArmorModification.ArmorSet[Hero].ArmorMitigationTable[Ability]")),
                                new XElement(
                                    "Value",
                                    new XAttribute("value", "25.000000"))),
                            new XElement(
                                "Modifications",
                                new XElement(
                                    "Field",
                                    new XAttribute("value", "ArmorModification.ArmorSet[Minion].ArmorMitigationTable[Ability]")),
                                new XElement(
                                    "Value",
                                    new XAttribute("value", "500"))))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Increase Hardened Carapace's Spell Armor by <c val=\"#TooltipNumbers\">25</c>.");
    }

    [TestMethod]
    public void ParseTooltipDescription_ArrayIndexOf2_ParsedGameString()
    {
        // arrange
        string description = "Increases Burrow Charge impact area by <c val=\"#TooltipNumbers\"><d ref=\"100*Talent,AnubarakMasteryEpicenterBurrowCharge,AbilityModificationArray[0].Modifications[2].Value\"player=\"0\"/>%</c> and lowers the cooldown by <c val=\"#TooltipNumbers\"><d ref=\"-Effect,AnubarakBurrowChargeEpicenterModifyCooldown,Cost[0].CooldownTimeUse\"precision=\"2\"/></c> second for each Hero hit.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Talent", "CTalent"),
                    ("Effect", "CEffectModifyUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CTalent",
                        new XAttribute("id", "AnubarakMasteryEpicenterBurrowCharge"),
                        new XElement(
                            "AbilityModificationArray",
                            new XElement(
                                "Modifications",
                                new XElement(
                                    "Field",
                                    new XAttribute("value", "AreaArray[0].Radius")),
                                new XElement(
                                    "Value",
                                    new XAttribute("value", "1.600000"))),
                            new XElement(
                                "Modifications",
                                new XElement(
                                    "Field",
                                    new XAttribute("value", "Chance")),
                                new XElement(
                                    "Value",
                                    new XAttribute("value", "1.000000"))),
                            new XElement(
                                "Modifications",
                                new XElement(
                                    "Field",
                                    new XAttribute("value", "AnubarakBurrowChargeCursorSplat")),
                                new XElement(
                                    "Value",
                                    new XAttribute("value", "0.600000"))))),
                    new(
                        "CEffectModifyUnit",
                        new XAttribute("id", "AnubarakBurrowChargeEpicenterModifyCooldown"),
                        new XElement(
                            "Cost",
                            new XAttribute("Abil", "AnubarakBurrowCharge,Execute"),
                            new XAttribute("CooldownTimeUse", "-1"))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Increases Burrow Charge impact area by <c val=\"#TooltipNumbers\">60%</c> and lowers the cooldown by <c val=\"#TooltipNumbers\">1</c> second for each Hero hit.");
    }

    [TestMethod]
    public void ParseTooltipDescription_ValueInParentElement_ParsedGameString()
    {
        // arrange
        string description = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> Gain <c val=\"#TooltipNumbers\"><d ref=\"Effect,KelThuzadMasterOfTheColdDarkModifyToken,Value\"/></c> Blight every time a Hero is Rooted by Frost Nova or hit by Chains of Kel'Thuzad.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,ConditionalEvents[0].CompareValue\"/></c> Blight, gain the Glacial Spike Ability.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,Max\"/></c> Blight, gain <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkSpellPower,Modification.DamageDealtFraction[Ability] * 100\"/>%</c> Spell Power.<n/><n/><c val=\"#TooltipQuest\">Blight:</c> <c val=\"#TooltipNumbers\" validator=\"True\"><d ref=\"$BehaviorStackCount:KelThuzadMasterOfTheColdDarkToken$\"/>/<d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,Max\"/></c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectModifyTokenCount"),
                    ("Behavior", "CBehaviorTokenCounter"),
                    ("Behavior", "CBehaviorBuff"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectModifyTokenCount",
                        new XAttribute("default", "1"),
                        new XAttribute("id", "BaseEffectModifyTokenCount"),
                        new XElement(
                            "Value",
                            new XAttribute("value", "1"))),
                    new(
                        "CEffectModifyTokenCount",
                        new XAttribute("id", "KelThuzadMasterOfTheColdDarkModifyToken"),
                        new XAttribute("parent", "BaseEffectModifyTokenCount")),
                    new(
                        "CBehaviorTokenCounter",
                        new XAttribute("id", "KelThuzadMasterOfTheColdDarkToken"),
                        new XElement(
                            "Max",
                            new XAttribute("value", "30")),
                        new XElement(
                            "ConditionalEvents",
                            new XAttribute("Compare", "GE"),
                            new XAttribute("CompareValue", "15"),
                            new XElement(
                                "Event",
                                new XAttribute("Effect", "KelThuzadMasterOfTheColdDarkTier1ModifyPlayer")))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "KelThuzadMasterOfTheColdDarkSpellPower"),
                        new XElement(
                            "Max",
                            new XAttribute("value", "30")),
                        new XElement(
                            "Modification",
                            new XElement(
                                "DamageDealtFraction",
                                new XAttribute("index", "Ability"),
                                new XAttribute("value", "0.75")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> Gain <c val=\"#TooltipNumbers\">1</c> Blight every time a Hero is Rooted by Frost Nova or hit by Chains of Kel'Thuzad.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">15</c> Blight, gain the Glacial Spike Ability.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">30</c> Blight, gain <c val=\"#TooltipNumbers\">75%</c> Spell Power.<n/><n/><c val=\"#TooltipQuest\">Blight:</c> <c val=\"#TooltipNumbers\" validator=\"True\">0/30</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_DrefDoesNotHaveIndexerForArrayFields_ParsedGameString()
    {
        // arrange
        string description = "Launch a grenade that explodes at the end of its path or upon hitting an enemy, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,JunkratFragLauncherExplosionDamage,Amount\"/></c> damage to nearby enemies. Grenades can ricochet off of terrain. Deals <c val=\"#TooltipNumbers\"><d ref=\"-Effect,JunkratFragLauncherExplosionDamage,AttributeFactor[Structure] * 100\"/>%</c> less damage to Structures.<n/><n/>Stores up to <c val=\"#TooltipNumbers\"><d ref=\"Abil,JunkratFragLauncher,Cost.Charge.CountMax\" player=\"0\"/></c> charges. Frag Launcher's cooldown replenishes all charges at the same time.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                    ("Abil", "CAbilEffectTarget"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "JunkratFragLauncherExplosionDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "124")),
                        new XElement(
                            "AttributeFactor",
                            new XAttribute("index", "Structure"),
                            new XAttribute("value", "-0.5"))),
                    new(
                        "CAbilEffectTarget",
                        new XAttribute("id", "JunkratFragLauncher"),
                        new XElement(
                            "Cost",
                            new XElement(
                                "Charge",
                                new XElement(
                                    "CountMax",
                                    new XAttribute("value", "4"))))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "JunkratFragLauncher"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "JunkratFragLauncherExplosionDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Launch a grenade that explodes at the end of its path or upon hitting an enemy, dealing <c val=\"#TooltipNumbers\">124~~0.04~~</c> damage to nearby enemies. Grenades can ricochet off of terrain. Deals <c val=\"#TooltipNumbers\">50%</c> less damage to Structures.<n/><n/>Stores up to <c val=\"#TooltipNumbers\">4</c> charges. Frag Launcher's cooldown replenishes all charges at the same time.");
    }

    [TestMethod]
    public void ParseTooltipDescription_ConditionalEventsIsAnArray_ParsedGameString()
    {
        // arrange
        string description = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Basic Attacks against Heroes while Windfury's Movement Speed bonus is active increase Attack Damage by <c val=\"#TooltipNumbers\"><d ref=\"Accumulator,ThrallMaelstromWeaponDamageAccumulator,Scale\"precision=\"2\"/></c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ThrallMaelstromWeaponTokenCounter,ConditionalEvents[0].CompareValue*Accumulator,ThrallMaelstromWeaponDamageAccumulator,Scale\"/></c> Attack Damage, increase the Movement Speed bonus of Windfury to <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer,EffectArray[0].Value\"/>%</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ThrallMaelstromWeaponTokenCounter,ConditionalEvents[1].CompareValue\"/></c> Attack Damage, Thrall permanently gains <c val=\"#TooltipNumbers\"><d ref=\"100*Behavior,ThrallWindfuryMaelstromWeapon2ndQuestCompletionMoveSpeedCarry,Modification.UnifiedMoveSpeedFactor\"/>%</c> increased Movement Speed.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Accumulator", "CAccumulatorToken"),
                    ("Behavior", "CBehaviorTokenCounter"),
                    ("Behavior", "CBehaviorBuff"),
                    ("Effect", "CEffectModifyPlayer"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CAccumulatorToken",
                        new XAttribute("default", "1"),
                        new XAttribute("id", "BaseTokenAccumulator"),
                        new XElement(
                            "Scale",
                            new XAttribute("value", "1"))),
                    new(
                        "CAccumulatorToken",
                        new XAttribute("id", "ThrallMaelstromWeaponDamageAccumulator"),
                        new XAttribute("parent", "BaseTokenAccumulator"),
                        new XElement(
                            "MinAccumulation",
                            new XAttribute("value", "0")),
                        new XElement(
                            "MaxAccumulation",
                            new XAttribute("value", "3000"))),
                    new(
                        "CBehaviorTokenCounter",
                        new XAttribute("id", "ThrallMaelstromWeaponTokenCounter"),
                        new XElement(
                            "ConditionalEvents",
                            new XAttribute("CompareValue", "20"),
                            new XElement(
                                "Event",
                                new XAttribute("Effect", "ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer"))),
                        new XElement(
                            "ConditionalEvents",
                            new XAttribute("CompareValue", "40"),
                            new XElement(
                                "Event",
                                new XAttribute("Effect", "ThrallWindfuryMaelstromWeaponTalent2ndQuestCompletionApplyCarry")))),
                    new(
                        "CEffectModifyPlayer",
                        new XAttribute("id", "ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer"),
                        new XElement(
                            "EffectArray",
                            new XAttribute("Reference", "Behavior,WindfurySpeedBuff,Modification.UnifiedMoveSpeedFactor"),
                            new XAttribute("Value", ".4"))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ThrallWindfuryMaelstromWeapon2ndQuestCompletionMoveSpeedCarry"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "UnifiedMoveSpeedFactor",
                                new XAttribute("value", "0.15")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Basic Attacks against Heroes while Windfury's Movement Speed bonus is active increase Attack Damage by <c val=\"#TooltipNumbers\">1</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">20</c> Attack Damage, increase the Movement Speed bonus of Windfury to <c val=\"#TooltipNumbers\">40%</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">40</c> Attack Damage, Thrall permanently gains <c val=\"#TooltipNumbers\">15%</c> increased Movement Speed.");
    }

    [TestMethod]
    public void ParseTooltipDescription_DRefExpressionTrailingCommandAndQuote_ParsedGameString()
    {
        // arrange
        string description = "Increase the Slow amount of each Twin Cleave axe by <c val=\"#TooltipNumbers\"><d ref=\"(Behavior,ZuljinTwinCleaveLacerateFirstHitSlow,Modification.UnifiedMoveSpeedFactor-Behavior,ZuljinTwinCleaveFirstHitSlow,Modification.UnifiedMoveSpeedFactor)*(-100)\" player=\"0\" precision=\"2\"/>%</c> and its duration by <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ZuljinTwinCleaveLacerateFirstHitSlow,Duration-Behavior,ZuljinTwinCleaveFirstHitSlow,Duration,\"player=\"0\"precision=\"1\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("Abil", "CAbilEffectTarget"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ZuljinTwinCleaveFirstHitSlow"),
                        new XAttribute("Duration", "2"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "UnifiedMoveSpeedFactor",
                                new XAttribute("value", "-0.15")))),
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ZuljinTwinCleaveLacerateFirstHitSlow"),
                        new XAttribute("Duration", "2.5"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "UnifiedMoveSpeedFactor",
                                new XAttribute("value", "-0.25")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Increase the Slow amount of each Twin Cleave axe by <c val=\"#TooltipNumbers\">10%</c> and its duration by <c val=\"#TooltipNumbers\">0.5</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_LevelScalingArrayIsUsingNumericalIndexInsteadOfText_ParsedGameString()
    {
        // arrange
        string description = "Probius gains permanent Shields equal to <c val=\"#TooltipNumbers\"><d ref=\"(Behavior,ProbiusShieldCapacitorPassiveShieldBuff,Modification.VitalMaxArray[Shields] / Unit,HeroProbius,LifeMax) * 100\"/>% </c>of his max Health. Shields regenerate quickly as long as he hasn't taken damage recently.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("Unit", "CUnit"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "ProbiusShieldCapacitorPassiveShieldBuff"),
                        new XAttribute("Duration", "2"),
                        new XElement(
                            "Modification",
                            new XElement(
                                "VitalMaxArray",
                                new XAttribute("index", "Shields"),
                                new XAttribute("value", "126")))),
                    new(
                        "CUnit",
                        new XAttribute("id", "HeroProbius"),
                        new XElement(
                            "LifeMax",
                            new XAttribute("value", "1260"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Behavior")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "ProbiusShieldCapacitorPassiveShieldBuff")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Modification.VitalMaxArray[1]")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000"))),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Unit")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "HeroProbius")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "LifeMax")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(heroesData.StormStorage, description);

        // assert
        parsed.Should().Be("Probius gains permanent Shields equal to <c val=\"#TooltipNumbers\">10% </c>of his max Health. Shields regenerate quickly as long as he hasn't taken damage recently.");
    }
}