using Heroes.LocaleText;

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
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$YrelSacredGroundArmorBonus" value="50" />
            """));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string result = parser.ParseText(text, StormLocale.ENUS);

        // assert
        result.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseText_CastIntroTimeHasDefaultValue_ParsedGameString()
    {
        // arrange
        string text = "After <c val=\"#TooltipNumbers\"><d ref=\"Abil,GuldanHorrify, CastIntroTime + Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]\" precision=\"2\"/></c> seconds, deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,GuldanHorrifyDamage,Amount\"/></c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,GuldanHorrifyFearDuration,Duration\"/></c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbil"), "Abil");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectCreatePersistent"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithNoId(XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """));
        dataStorage.AddElementWithNoId(XmlReaderHelper.GetStormXmlElement(
            """
            <CAbilEffectTarget default="1" />
            """));
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectTarget id="GuldanHorrify" />
                """),
            "Abil");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectCreatePersistent id="GuldanHorrifyAbilityStartCreatePersistent">
                  <PeriodicPeriodArray value="0.5" />
                </CEffectCreatePersistent>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="GuldanHorrifyDamage">
                  <Amount value="120" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="GuldanHorrifyFearDuration">
                  <Duration value="2" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="Guldan">
              <LevelScalingArray Ability="GuldanHorrify">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="GuldanHorrifyDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("After <c val=\"#TooltipNumbers\">0.5</c> seconds, deal <c val=\"#TooltipNumbers\">120<c val=\"#ColorGray\">~~0.04~~</c></c> damage to enemy Heroes in an area and Fear them for <c val=\"#TooltipNumbers\">2</c> seconds. While Feared, Heroes are Silenced and are forced to run away from Horrify's center.");
    }

    [TestMethod]
    public void ParseText_NormalGameString2_ParsedGameString()
    {
        // arrange
        string text = "Rain a small army of Demonic Grunts down on enemies, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionImpactDamage,Amount\"/></c> damage per impact. Grunts deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionDemonGruntWeaponDamage,Amount\"/></c> damage, have <c val=\"#TooltipNumbers\"><d ref=\"Unit,AzmodanDemonicInvasionDemonGrunt,LifeMax\"/></c> Health and last up to <c val=\"#TooltipNumbers\"><d ref=\"-Unit,AzmodanDemonicInvasionDemonGrunt,LifeMax/Unit,AzmodanDemonicInvasionDemonGrunt,LifeRegenRate\"/></c> seconds. When Grunts die they explode, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanDemonicInvasionExplodeDamage,Amount\"/></c> damage to nearby enemies, doubled against enemy Heroes.<n/><n/>Usable while Channeling All Shall Burn.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CUnit"), "Unit");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AzmodanDemonicInvasionImpactDamage">
                  <Amount value="85" />
                  <PeriodicPeriodArray value="0" />
                  <PeriodicPeriodArray value="0.25" />
                  <PeriodicPeriodArray value="0.25" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AzmodanDemonicInvasionDemonGruntWeaponDamage">
                  <Amount value="39" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CUnit id="AzmodanDemonicInvasionDemonGrunt">
                  <LifeMax value="750" />
                  <LifeRegenRate value="-75" />
                </CUnit>
                """),
            "Unit");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AzmodanDemonicInvasionExplodeDamage">
                  <Amount value="40" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="Azmodan">
              <LevelScalingArray Ability="AzmodanDemonicInvasion">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="AzmodanDemonicInvasionImpactDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="AzmodanDemonicInvasionDemonGruntWeaponDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Unit" />
                  <Entry value="AzmodanDemonicInvasionDemonGrunt" />
                  <Field value="LifeMax" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Unit" />
                  <Entry value="AzmodanDemonicInvasionDemonGrunt" />
                  <Field value="LifeRegenRate" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
              <LevelScalingArray Ability="AzmodanDemonicInvasion">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="AzmodanDemonicInvasionExplodeDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Rain a small army of Demonic Grunts down on enemies, dealing <c val=\"#TooltipNumbers\">85<c val=\"#ColorGray\">~~0.04~~</c></c> damage per impact. Grunts deal <c val=\"#TooltipNumbers\">39<c val=\"#ColorGray\">~~0.04~~</c></c> damage, have <c val=\"#TooltipNumbers\">750<c val=\"#ColorGray\">~~0.04~~</c></c> Health and last up to <c val=\"#TooltipNumbers\">10</c> seconds. When Grunts die they explode, dealing <c val=\"#TooltipNumbers\">40<c val=\"#ColorGray\">~~0.04~~</c></c> damage to nearby enemies, doubled against enemy Heroes.<n/><n/>Usable while Channeling All Shall Burn.");
    }

    [TestMethod]
    public void ParseText_HasParenthesisAndBrackets_ParsedGameString()
    {
        // arrange
        string text = "Toxic Nests deal <c val=\"#TooltipNumbers\"><d ref=\"(Effect,AbathurToxicNestEnvenomedNestDamage,Amount* [d ref='Behavior,AbathurToxicNestEnvenomedNest,PeriodCount' player='0'/])/Effect,ToxicNestDamage,Amount*100\"/>%</c> more damage over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNest,Duration\" player=\"0\"/></c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\"><d ref=\"-Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,ArmorModification.AllArmorBonus\"/></c> for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AbathurToxicNestEnvenomedNestArmorDebuff,Duration\"/></c> seconds.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AbathurToxicNestEnvenomedNestDamage">
                  <Amount value="38.25" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="AbathurToxicNestEnvenomedNest">
                  <PeriodCount value="3" />
                  <Duration value="3" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ToxicNestDamage">
                  <Amount value="153" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="AbathurToxicNestEnvenomedNestArmorDebuff">
                  <Duration value="4" />
                  <ArmorModification>
                    <AllArmorBonus value="-10" />
                  </ArmorModification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="Abathur">
              <LevelScalingArray Ability="AbathurToxicNest">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="AbathurToxicNestEnvenomedNestDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="ToxicNestDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Toxic Nests deal <c val=\"#TooltipNumbers\">75%</c> more damage over <c val=\"#TooltipNumbers\">3</c> seconds and reduce the Armor of enemy Heroes hit by <c val=\"#TooltipNumbers\">10</c> for <c val=\"#TooltipNumbers\">4</c> seconds.");
    }

    [TestMethod]
    public void ParseText_CostHasAnArrayIndex_ParsedGameString()
    {
        // arrange
        string text = "If Sand Blast travels at least <c val=\"bfd4fd\"><d ref=\"Validator,ChromieFastForwardDistanceCheck,Range/Effect,ChromieSandBlastLaunchMissile,ImpactLocation.ProjectionDistanceScale*100\"/>%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\"><d ref=\"Effect,ChromieSandBlastFastForwardCooldownReduction,Cost[0].CooldownTimeUse\" precision=\"2\"/></c> seconds.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CValidatorLocationCompareRange"), "Validator");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectLaunchMissile"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CValidatorLocationCompareRange id="ChromieFastForwardDistanceCheck">
                  <Range value="7.5" />
                </CValidatorLocationCompareRange>
                """),
            "Validator");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectLaunchMissile id="ChromieSandBlastLaunchMissile">
                  <ImpactLocation>
                    <ProjectionDistanceScale value="15" />
                  </ImpactLocation>
                </CEffectLaunchMissile>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyUnit id="ChromieSandBlastFastForwardCooldownReduction">
                  <Cost>
                    <Abil value="ChromieSandBlast,Execute" />
                    <CooldownTimeUse value="0.5" />
                  </Cost>
                </CEffectModifyUnit>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("If Sand Blast travels at least <c val=\"bfd4fd\">50%</c> of its base distance and hits a Hero, its cooldown is reduced to <c val=\"bfd4fd\">0.5</c> seconds.");
    }

    [TestMethod]
    public void ParseText_NegativeValueFromDRef_ParsedGameString()
    {
        // arrange
        string text = "reduces its cooldown by <c val=\"#TooltipNumbers\"><d ref=\"(1-Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse)*-1\"/></c>";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyUnit id="AnduinHolyWordSalvationLightOfStormwindCooldownReduction">
                  <Cost>
                    <CooldownTimeUse value="-60" />
                  </Cost>
                </CEffectModifyUnit>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("reduces its cooldown by <c val=\"#TooltipNumbers\">60</c>");
    }

    [TestMethod]
    [DataRow(StormLocale.ENUS, "3.75")]
    [DataRow(StormLocale.DEDE, "3,75")]
    [DataRow(StormLocale.FRFR, "3,75")]
    [DataRow(StormLocale.KOKR, "3.75")]
    [DataRow(StormLocale.RURU, "3,75")]
    public void ParseText_HasStormLocale_WritesValueInLocaleAndScalingInvariant(StormLocale stormLocale, string expectedValue)
    {
        // arrange
        string text = "deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,LocaleDamage,Amount*1.5\" precision=\"2\"/></c> damage";

        GameStringParser parser = new(CreateLocaleDamageStorage());

        // act
        string parsed = parser.ParseText(text, stormLocale);

        // assert
        parsed.Should().Be($"deals <c val=\"#TooltipNumbers\">{expectedValue}<c val=\"#ColorGray\">~~0.045~~</c></c> damage");
    }

    [TestMethod]
    public void ParseTextt_HasConstInValues_ParsedGameString()
    {
        // arrange
        string text = "Globe of Annihilation deals <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,AzmodanGlobeOfAnnihilationDamage,MultiplicativeModifierArray[Greed].Modifier\"player=\"0\"/>%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedTalentTokenCounter,Max\"/></c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\"><d ref=\"Effect,AzmodanGlobeOfAnnihilationGreedAllShallBurnRangeIncreaseModifyCatalog,CatalogModifications[0].Value/Abil,AzmodanAllShallBurn,Range*100\" player=\"0\"/>%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AzmodanGlobeOfAnnihilationGreedMovementSpeed,Modification.UnifiedMoveSpeedFactor*100\"/>%</c> Attack Speed and Movement Speed.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyCatalogNumeric"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectTarget"), "Abil");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorTokenCounter"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AzmodanAllShallBurnCastRange" value="6" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AzmodanAllShallBurnGreedBonusRange" value="1.5" />
            """));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AzmodanGlobeOfAnnihilationDamage">
                  <MultiplicativeModifierArray index="Greed" Modifier="0.15" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorTokenCounter id="AzmodanGlobeOfAnnihilationGreedTalentTokenCounter">
                  <Max value="200" />
                </CBehaviorTokenCounter>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyCatalogNumeric id="AzmodanGlobeOfAnnihilationGreedAllShallBurnRangeIncreaseModifyCatalog">
                  <CatalogModifications Reference="Abil,AzmodanAllShallBurn,Range">
                    <Value value="$AzmodanAllShallBurnGreedBonusRange" />
                  </CatalogModifications>
                  <CatalogModifications Reference="Actor,AzmodanAllShallBurnRangeSplat,Scale">
                    <Value value="$AzmodanAllShallBurnGreedBonusRange" />
                  </CatalogModifications>
                </CEffectModifyCatalogNumeric>
                """,
                layeredDataStorage),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectTarget id="AzmodanAllShallBurn">
                  <Range value="$AzmodanAllShallBurnCastRange" />
                </CAbilEffectTarget>
                """,
                layeredDataStorage),
            "Abil");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="AzmodanGlobeOfAnnihilationGreedMovementSpeed">
                  <Modification>
                    <UnifiedMoveSpeedFactor value="0.2" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Globe of Annihilation deals <c val=\"#TooltipNumbers\">15%</c> more damage to non-Heroic targets.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> After gaining <c val=\"#TooltipNumbers\">200</c> Annihilation, increase the range of All Shall Burn by <c val=\"#TooltipNumbers\">25%</c> and Demon Warriors gain <c val=\"#TooltipNumbers\">20%</c> Attack Speed and Movement Speed.");
    }

    //[TestMethod]
    //public void ParseText_HasConstInValues_ParsedGameString()
    //{
    //    // arrange
    //    string text = "<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodCount\"/></c> shots over <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[1]+Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[2]\" precision=\"2\"/></c> seconds. Each shot deals <c val=\"#TooltipNumbers\"><d ref=\"1+Effect,DVaPilotBigShotDamage,MultiplicativeModifierArray[PewPewPew].Modifier*100\"player=\"0\"/>%</c> damage.";

    //    LayeredDataStorage layeredDataStorage = new();
    //    DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

    //    dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectCreatePersistent"), "Effect");
    //    dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

    //    dataStorage.AddElementWithId(
    //        XmlReaderHelper.GetStormXmlElement(
    //            """
    //            <CEffectCreatePersistent id="DVaBigShotPewPewPewOffsetPeriodic">
    //              <PeriodCount value="3" />
    //              <PeriodicPeriodArray value="0" />
    //              <PeriodicPeriodArray value="0.25" />
    //              <PeriodicPeriodArray value="0.25" />
    //            </CEffectCreatePersistent>
    //            """),
    //        "Effect");
    //    dataStorage.AddElementWithId(
    //        XmlReaderHelper.GetStormXmlElement(
    //            """
    //            <CEffectDamage id="DVaPilotBigShotDamage">
    //              <MultiplicativeModifierArray index="PewPewPew" Modifier="-0.25" />
    //              <MultiplicativeModifierArray index="Headshot"Modifier="0.75" />
    //            </CEffectDamage>
    //            """),
    //        "Effect");

    //    // TODO: level scaling array

    //    GameStringParser parser = new(layeredDataStorage);

    //    // act
    //    string parsed = parser.ParseText(text, StormLocale.ENUS);

    //    // assert
    //    parsed.Should().Be("<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\">3</c> shots over <c val=\"#TooltipNumbers\">0.5</c> seconds. Each shot deals <c val=\"#TooltipNumbers\">75%</c> damage.");
    //}

    private static StormXmlElement CreateBaseElement(string elementName)
    {
        return XmlReaderHelper.GetStormXmlElement($"""<{elementName} />""");
    }

    // Effect,LocaleDamage,Amount of 2.5 with a 0.045 level scaling
    private static LayeredDataStorage CreateLocaleDamageStorage()
    {
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="LocaleDamage">
                  <Amount value="2.5" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="LocaleHero">
              <LevelScalingArray Ability="LocaleAbil">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="LocaleDamage" />
                  <Field value="Amount" />
                  <Value value="0.045000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        return layeredDataStorage;
    }
}
