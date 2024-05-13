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

        // act
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NormalGamestring_ParsedGameString()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("After <c val=\"#TooltipNumbers\">0.5</c> seconds, deal <c val=\"#TooltipNumbers\">120~~0.04~~</c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\">2</c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasParenthesisAndBrackets_ParsedGameString()
    {
        // arrange
        string description = "Toxic Nests deal <c val=\"#TooltipNumbers\"><d ref=\"(Effect,AbathurToxicNestEnvenomedNestDamage,Amount* [d ref='Behavior,AbathurToxicNestEnvenomedNest,PeriodCount' player='0'/])/Effect,ToxicNestDamage,Amount*100\"/>%</c> more damage over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNest,Duration\" player=\"0\"/></c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\"><d ref=\"-Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,ArmorModification.AllArmorBonus\"/></c> for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,Duration\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Toxic Nests deal <c val=\"#TooltipNumbers\">75%</c> more damage over <c val=\"#TooltipNumbers\">3</c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\">10</c> for <c val=\"#TooltipNumbers\">4</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NoScalingWithSingleDigitPercent_ParsedGameString()
    {
        // arrange
        string description = "Zarya's Basic Attack deals <c val=\"bfd4fd\"><d ref=\"(Effect,ZaryaWeaponFeelTheHeatDamage,Amount/Effect,ZaryaWeaponDamage,Amount)-1*10)\" />0%</c> additional damage to enemies in melee range.";

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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Zarya's Basic Attack deals <c val=\"bfd4fd\">50%</c> additional damage to enemies in melee range.");
    }

    [TestMethod]
    public void ParseTooltipDescription_CostIsAnArray_ParsedGameString()
    {
        // arrange
        string description = "If Sand Blast travels at least <c val=\"bfd4fd\"><d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\"><d ref=\"Effect,ChromieSandBlastFastForwardCooldownReduction,Cost[0].CooldownTimeUse\" precision=\"2\"/></c> seconds.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
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
                            new XAttribute("CooldownTimeUse", "0.5"))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("If Sand Blast travels at least <c val=\"bfd4fd\">50%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\">0.5</c> seconds.");
    }

    [TestMethod]
    public void ParseTooltipDescription_NegativeValueFromDRef_ParsedGameString()
    {
        // arrange
        string description = "reduces its cooldown by <c val=\"#TooltipNumbers\"><d ref=\"(1-Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse)*-1\"/></c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("reduces its cooldown by <c val=\"#TooltipNumbers\">60</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasAScalingValuePercent_ParsedGameString()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\">13700~~0.04~~%</c>");
    }

    [TestMethod]
    public void ParseTooltipDescription_HasConstInValues_ParsedGameString()
    {
        // arrange
        string description = "Globe of Annihilation deals <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,AzmodanGlobeOfAnnihilationDamage,MultiplicativeModifierArray[Greed].Modifier\"player=\"0\"/>%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedTalentTokenCounter,Max\"/></c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanGlobeOfAnnihilationGreedAllShallBurnRangeIncreaseModifyCatalog,CatalogModifications[0].Value/Abil,AzmodanAllShallBurn,Range*100\" player=\"0\"/>%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedMovementSpeed,Modification.UnifiedMoveSpeedFactor*100\"/>%</c> Attack Speed and Movement Speed.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Globe of Annihilation deals <c val=\"#TooltipNumbers\">15%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\">200</c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\">25%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\">20%</c> Attack Speed and Movement Speed.");
    }

    [TestMethod]
    public void ParseTooltipDescription_SpacingInDRef_ParsedGameString()
    {
        // arrange
        string description = "Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\"><d ref=\"Unit,RagnarosBigRag,LifeMax\"/></c> Health that burns away over <c val=\"#TooltipNumbers\"><d ref=\"(Unit,RagnarosBigRag,LifeMax / Unit,RagnarosBigRag,LifeRegenRate) * (-1)\"/></c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\">3996~~0.04~~</c> Health that burns away over <c val=\"#TooltipNumbers\">18</c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.");
    }

    [TestMethod]
    public void ParseTooltipDescription_DRefHasDefaultIndexerForScaling_ParsedGameString()
    {
        // arrange
        string description = "Deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,MultishotDamage,AmountArray\"/></c> damage to enemies within the target area.";

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
        string parsed = GameStringParser.ParseTooltipDescription(description, heroesData);

        // assert
        parsed.Should().Be("Deal <c val=\"#TooltipNumbers\">159~~0.04~~</c> damage to enemies within the target area.");
    }
    //
    //[TestMethod]
    //public void ParseTooltipDescription2Test()
    //{
    //    // arrange
    //    string description = "Shields the assisted ally for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CarapaceEvolutionShieldTooltipDummy,DamageResponse.ModifyLimit\"/></c>. Allied Heroes are healed for <c val=\"#TooltipNumbers\"><d ref=\"Effect,RegenerativeMicrobesCreateHealer,RechargeVitalRate\"/></c> Health per second while the Shield is active. Lasts for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CarapaceEvolutionShield,Duration\" player=\"0\"/></c> seconds.";

    //    TooltipDescription tooltipDescription = new TooltipDescription(description);
    //    string a =tooltipDescription.RawDescription;

    //    HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
    //        .AddConstantElements(new List<XElement>()
    //        {
    //            new(
    //                "const",
    //                new XAttribute("id", "$YrelSacredGroundArmorBonus"),
    //                new XAttribute("value", "50")),
    //        });

    //    HeroesData heroesData = loader.HeroesData;

    //    GameStringParser gameStringParser = new(heroesData);
    //    // act

    //    gameStringParser.ParseTooltipDescription(description);

    //    // assert
    //}
}