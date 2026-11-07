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
    public void ParseText_NoScalingWithSingleDigitPercent_ParsedGameString()
    {
        // arrange
        string text = "Zarya's Basic Attack deals <c val=\"bfd4fd\"><d ref=\"(Effect,ZaryaWeaponFeelTheHeatDamage,Amount/Effect,ZaryaWeaponDamage,Amount)-1*10)\" />0%</c> additional damage to enemies in melee range.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ZaryaWeaponFeelTheHeatDamage">
                  <Amount value="30" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ZaryaWeaponDamage">
                  <Amount value="20" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="Zarya">
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="ZaryaWeaponDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="ZaryaWeaponFeelTheHeatDamage" />
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
        parsed.Should().Be("Zarya's Basic Attack deals <c val=\"bfd4fd\">50%</c> additional damage to enemies in melee range.");
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
    public void ParseText_HasAScalingValuePercent_ParsedGameString()
    {
        // arrange
        string text = "Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\"><d ref=\"Effect,OctoGrabPokeMasteryDamage,Amount * 100\"/>%</c>";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="OctoGrabPokeMasteryDamage">
                  <Amount value="137" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="Murky">
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="OctoGrabPokeMasteryDamage" />
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
        parsed.Should().Be("Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\">13700%<c val=\"#ColorGray\">~~0.04~~</c></c>");
    }

    [TestMethod]
    public void ParseText_HasConstInValues_ParsedGameString()
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

    [TestMethod]
    public void ParseText_SpacingInDRef_ParsedGameString()
    {
        // arrange
        string text = "Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\"><d ref=\"Unit,RagnarosBigRag,LifeMax\"/></c> Health that burns away over <c val=\"#TooltipNumbers\"><d ref=\"(Unit,RagnarosBigRag,LifeMax / Unit,RagnarosBigRag,LifeRegenRate) * (-1)\"/></c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CUnit"), "Unit");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CUnit id="RagnarosBigRag">
                  <LifeMax value="3996" />
                  <LifeRegenRate value="-222" />
                </CUnit>
                """),
            "Unit");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Unit" />
                  <Entry value="RagnarosBigRag" />
                  <Field value="LifeMax" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Unit" />
                  <Entry value="RagnarosBigRag" />
                  <Field value="LifeRegenRate" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Channel on an allied or destroyed Fort or Keep to replace it with Ragnaros's ultimate form, temporarily gaining new Abilities, having <c val=\"#TooltipNumbers\">3996<c val=\"#ColorGray\">~~0.04~~</c></c> Health that burns away over <c val=\"#TooltipNumbers\">18</c> seconds.<n/><n/>Ragnaros returns to his normal form upon losing all Health in Molten Core.");
    }

    [TestMethod]
    public void ParseText_DRefHasDefaultIndexerForScaling_ParsedGameString()
    {
        // arrange
        string text = "Deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,MultishotDamage,AmountArray\"/></c> damage to enemies within the target area.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="MultishotDamage">
                  <AmountArray index="Quest" value="159" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="MultishotDamage" />
                  <Field value="AmountArray[Quest]" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Deal <c val=\"#TooltipNumbers\">159<c val=\"#ColorGray\">~~0.04~~</c></c> damage to enemies within the target area.");
    }

    [TestMethod]
    public void ParseText_IndexerLessInLevelScalingArray_ParsedGameString()
    {
        // arrange
        string text = "Shields Tyrael for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,TyraelRighteousnessShield,DamageResponse[0].ModifyLimit[0]\"/></c> damage and nearby allied Heroes and Minions for <c val=\"#TooltipNumbers\">40%</c> as much for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,TyraelRighteousnessShield,Duration[0]\"/></c> seconds.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="TyraelRighteousnessShield">
                  <DamageResponse Exhausted="RighteousnessSalvationCreateHealer" ModifyLimit="336" />
                  <Duration value="4" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="TyraelRighteousnessShield" />
                  <Field value="DamageResponse[0].ModifyLimit[0]" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Shields Tyrael for <c val=\"#TooltipNumbers\">336<c val=\"#ColorGray\">~~0.04~~</c></c> damage and nearby allied Heroes and Minions for <c val=\"#TooltipNumbers\">40%</c> as much for <c val=\"#TooltipNumbers\">4</c> seconds.");
    }

    [TestMethod]
    public void ParseText_HasArraysWithAndWithoutIndex_ParsedGameString()
    {
        // arrange
        string text = "<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodCount\"/></c> shots over <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[1]+Effect,DVaBigShotPewPewPewOffsetPeriodic,PeriodicPeriodArray[2]\" precision=\"2\"/></c> seconds. Each shot deals <c val=\"#TooltipNumbers\"><d ref=\"1+Effect,DVaPilotBigShotDamage,MultiplicativeModifierArray[PewPewPew].Modifier*100\"player=\"0\"/>%</c> damage.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectCreatePersistent"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectCreatePersistent id="DVaBigShotPewPewPewOffsetPeriodic">
                  <PeriodCount value="3" />
                  <PeriodicPeriodArray value="0" />
                  <PeriodicPeriodArray value="0.25" />
                  <PeriodicPeriodArray value="0.25" />
                </CEffectCreatePersistent>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="DVaPilotBigShotDamage">
                  <MultiplicativeModifierArray index="PewPewPew" Modifier="-0.25" />
                  <MultiplicativeModifierArray index="Headshot" Modifier="0.75" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="DVaPilotBigShotDamage" />
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
        parsed.Should().Be("<c val=\"#AbilityPassive\">Pilot Mode: </c>Instead of a single shot, Big Shot fires <c val=\"#TooltipNumbers\">3</c> shots over <c val=\"#TooltipNumbers\">0.5</c> seconds. Each shot deals <c val=\"#TooltipNumbers\">75%</c> damage.");
    }

    [TestMethod]
    public void ParseText_ConstantsWithExpressions_ParsedGameString()
    {
        // arrange
        string text = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\"><d ref=\"Behavior,DVaMechSelfDestructMechDetonationCountdown,Duration\" player=\"0\"/></c> seconds. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount+Accumulator,DVaSelfDestructDistanceAccumulator,MinAccumulation\"/></c> to <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount\"/></c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,AttributeFactor[Structure]*(-100)\"/>%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAccumulatorDistance"), "Accumulator");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$DVaMechSelfDestructDetonationMaximumDamage" value="1100" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$DVaMechSelfDestructDetonationMinimumDamage" value="400" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$DVaMechSelfDestructDetonationMinimumAccumulation" value="-($DVaMechSelfDestructDetonationMinimumDamage $DVaMechSelfDestructDetonationMaximumDamage)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="DVaMechSelfDestructMechDetonationCountdown">
                  <Duration value="4" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAccumulatorDistance id="DVaSelfDestructDistanceAccumulator">
                  <MinAccumulation value="$DVaMechSelfDestructDetonationMinimumAccumulation" />
                </CAccumulatorDistance>
                """,
                layeredDataStorage),
            "Accumulator");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="DVaMechSelfDestructDetonationSearchDamage">
                  <AttributeFactor index="Structure" value="-0.5" />
                  <Amount value="$DVaMechSelfDestructDetonationMaximumDamage" />
                </CEffectDamage>
                """,
                layeredDataStorage),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="DVaMechSelfDestructDetonationSearchDamage" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Accumulator" />
                  <Entry value="DVaSelfDestructDistanceAccumulator" />
                  <Field value="MinAccumulation" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400<c val=\"#ColorGray\">~~0.04~~</c></c> to <c val=\"#TooltipNumbers\">1100<c val=\"#ColorGray\">~~0.04~~</c></c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>");
    }

    [TestMethod]
    public void ParseText_HasArrayOnLastFieldOnly_ParsedGameString()
    {
        // arrange
        string text = "Transform for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,MuradinAvatarHealthBuff,Duration\" player=\"0\" precision=\"2\"/></c> seconds, gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,MuradinAvatar,Modification.VitalMaxArray[0]\"/></c> Health.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectInstant"), "AbilEffect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="MuradinAvatarHealthBuff">
                  <Duration value="20" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectInstant id="MuradinAvatar">
                </CAbilEffectInstant>
                """),
            "AbilEffect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="MuradinAvatar">
                  <Modification>
                    <VitalMaxArray index="0" value="1000" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="MuradinAvatar" />
                  <Field value="Modification.VitalMaxArray[0]" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Transform for <c val=\"#TooltipNumbers\">20</c> seconds, gaining <c val=\"#TooltipNumbers\">1000<c val=\"#ColorGray\">~~0.04~~</c></c> Health.");
    }

    [TestMethod]
    public void ParseText_HasIndexedArray_ParsedGameString()
    {
        // arrange
        string text = "While at or below <c val=\"#TooltipNumbers\">50</c> Brew, gain <c val=\"#TooltipNumbers\"><d ref=\"100*Behavior,ChenBrewmastersBalanceSpeedBuff,Modification.UnifiedMoveSpeedFactor\"/>%</c> Movement Speed. While at or above <c val=\"#TooltipNumbers\">50</c> Brew, regenerate an additional <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ChenBrewmastersBalanceHealthRegen,Modification.VitalRegenArray[Life]\"/></c> Health per second.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ChenBrewmastersBalanceSpeedBuff">
                  <Modification>
                    <UnifiedMoveSpeedFactor value="0.2" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ChenBrewmastersBalanceHealthRegen">
                  <Modification>
                    <VitalRegenArray index="Life" value="18" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="ChenBrewmastersBalanceHealthRegen" />
                  <Field value="Modification.VitalRegenArray[Life]" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("While at or below <c val=\"#TooltipNumbers\">50</c> Brew, gain <c val=\"#TooltipNumbers\">20%</c> Movement Speed. While at or above <c val=\"#TooltipNumbers\">50</c> Brew, regenerate an additional <c val=\"#TooltipNumbers\">18<c val=\"#ColorGray\">~~0.04~~</c></c> Health per second.");
    }

    [TestMethod]
    public void ParseText_ElementArray_ParsedGameString()
    {
        // arrange
        string text = "Increase Hardened Carapace's Spell Armor by <c val=\"#TooltipNumbers\"><d ref=\"Talent,AnubarakNerubianArmor,AbilityModificationArray[0].Modifications[0].Value\"/></c>.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CTalent"), "Talent");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CTalent id="AnubarakNerubianArmor">
                  <AbilityModificationArray>
                    <Modifications>
                      <Field value="ArmorModification.ArmorSet[Hero].ArmorMitigationTable[Ability]" />
                      <Value value="25.000000" />
                    </Modifications>
                    <Modifications>
                      <Field value="ArmorModification.ArmorSet[Minion].ArmorMitigationTable[Ability]" />
                      <Value value="500" />
                    </Modifications>
                  </AbilityModificationArray>
                </CTalent>
                """),
            "Talent");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Increase Hardened Carapace's Spell Armor by <c val=\"#TooltipNumbers\">25</c>.");
    }

    [TestMethod]
    public void ParseText_ArrayIndexOf2_ParsedGameString()
    {
        // arrange
        string text = "Increases Burrow Charge impact area by <c val=\"#TooltipNumbers\"><d ref=\"100*Talent,AnubarakMasteryEpicenterBurrowCharge,AbilityModificationArray[0].Modifications[2].Value\"player=\"0\"/>%</c> and lowers the cooldown by <c val=\"#TooltipNumbers\"><d ref=\"-Effect,AnubarakBurrowChargeEpicenterModifyCooldown,Cost[0].CooldownTimeUse\"precision=\"2\"/></c> second for each Hero hit.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CTalent"), "Talent");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CTalent id="AnubarakMasteryEpicenterBurrowCharge">
                  <AbilityModificationArray>
                    <Modifications>
                      <Field value="AreaArray[0].Radius" />
                      <Value value="1.600000" />
                    </Modifications>
                    <Modifications>
                      <Field value="Chance" />
                      <Value value="1.000000" />
                    </Modifications>
                    <Modifications>
                      <Field value="AnubarakBurrowChargeCursorSplat" />
                      <Value value="0.600000" />
                    </Modifications>
                  </AbilityModificationArray>
                </CTalent>
                """),
            "Talent");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyUnit id="AnubarakBurrowChargeEpicenterModifyCooldown">
                  <Cost Abil="AnubarakBurrowCharge,Execute" CooldownTimeUse="-1" />
                </CEffectModifyUnit>
                """),
            "Effect");
        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Increases Burrow Charge impact area by <c val=\"#TooltipNumbers\">60%</c> and lowers the cooldown by <c val=\"#TooltipNumbers\">1</c> second for each Hero hit.");
    }

    [TestMethod]
    public void ParseText_ValueInParentElement_ParsedGameString()
    {
        // arrange
        string text = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> Gain <c val=\"#TooltipNumbers\"><d ref=\"Effect,KelThuzadMasterOfTheColdDarkModifyToken,Value\"/></c> Blight every time a Hero is Rooted by Frost Nova or hit by Chains of Kel'Thuzad.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,ConditionalEvents[0].CompareValue\"/></c> Blight, gain the Glacial Spike Ability.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,Max\"/></c> Blight, gain <c val=\"#TooltipNumbers\"><d ref=\"Behavior,KelThuzadMasterOfTheColdDarkSpellPower,Modification.DamageDealtFraction[Ability] * 100\"/>%</c> Spell Power.<n/><n/><c val=\"#TooltipQuest\">Blight:</c> <c val=\"#TooltipNumbers\" validator=\"True\"><d ref=\"$BehaviorStackCount:KelThuzadMasterOfTheColdDarkToken$\"/>/<d ref=\"Behavior,KelThuzadMasterOfTheColdDarkToken,Max\"/></c>";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyTokenCount"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorTokenCounter"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyTokenCount default="1" id="BaseEffectModifyTokenCount">
                  <Value value="1" />
                </CEffectModifyTokenCount>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyTokenCount id="KelThuzadMasterOfTheColdDarkModifyToken" parent="BaseEffectModifyTokenCount">
                </CEffectModifyTokenCount>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorTokenCounter id="KelThuzadMasterOfTheColdDarkToken">
                  <Max value="30" />
                  <ConditionalEvents Compare="GE" CompareValue="15">
                    <Event Effect="KelThuzadMasterOfTheColdDarkTier1ModifyPlayer" />
                  </ConditionalEvents>
                </CBehaviorTokenCounter>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="KelThuzadMasterOfTheColdDarkSpellPower">
                  <Max value="30" />
                  <Modification>
                    <DamageDealtFraction index="Ability" value="0.75" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Quest:</c> Gain <c val=\"#TooltipNumbers\">1</c> Blight every time a Hero is Rooted by Frost Nova or hit by Chains of Kel'Thuzad.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">15</c> Blight, gain the Glacial Spike Ability.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">30</c> Blight, gain <c val=\"#TooltipNumbers\">75%</c> Spell Power.<n/><n/><c val=\"#TooltipQuest\">Blight:</c> <c val=\"#TooltipNumbers\" validator=\"True\">0/30</c>");
    }

    [TestMethod]
    public void ParseText_DrefDoesNotHaveIndexerForArrayFields_ParsedGameString()
    {
        // arrange
        string text = "Launch a grenade that explodes at the end of its path or upon hitting an enemy, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,JunkratFragLauncherExplosionDamage,Amount\"/></c> damage to nearby enemies. Grenades can ricochet off of terrain. Deals <c val=\"#TooltipNumbers\"><d ref=\"-Effect,JunkratFragLauncherExplosionDamage,AttributeFactor[Structure] * 100\"/>%</c> less damage to Structures.<n/><n/>Stores up to <c val=\"#TooltipNumbers\"><d ref=\"Abil,JunkratFragLauncher,Cost.Charge.CountMax\" player=\"0\"/></c> charges. Frag Launcher's cooldown replenishes all charges at the same time.";

        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectTarget"), "Abil");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="JunkratFragLauncherExplosionDamage">
                  <Amount value="124" />
                  <AttributeFactor index="Structure" value="-0.5" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectTarget id="JunkratFragLauncher">
                  <Cost>
                    <Charge>
                      <CountMax value="4" />
                    </Charge>
                  </Cost>
                </CAbilEffectTarget>
                """),
            "Abil");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="JunkratFragLauncherExplosionDamage" />
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
        parsed.Should().Be("Launch a grenade that explodes at the end of its path or upon hitting an enemy, dealing <c val=\"#TooltipNumbers\">124<c val=\"#ColorGray\">~~0.04~~</c></c> damage to nearby enemies. Grenades can ricochet off of terrain. Deals <c val=\"#TooltipNumbers\">50%</c> less damage to Structures.<n/><n/>Stores up to <c val=\"#TooltipNumbers\">4</c> charges. Frag Launcher's cooldown replenishes all charges at the same time.");
    }

    [TestMethod]
    [DataRow(StormLocale.ENUS, "3.75", "0.045")]
    [DataRow(StormLocale.DEDE, "3,75", "0,045")]
    [DataRow(StormLocale.FRFR, "3,75", "0,045")]
    [DataRow(StormLocale.KOKR, "3.75", "0.045")]
    [DataRow(StormLocale.RURU, "3,75", "0,045")]
    public void ParseText_HasStormLocale_WritesValueAndScalingInLocale(StormLocale stormLocale, string expectedValue, string expectedScaling)
    {
        // arrange
        string text = "deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,LocaleDamage,Amount*1.5\" precision=\"2\"/></c> damage";

        GameStringParser parser = new(CreateLocaleDamageStorage());

        // act
        string parsed = parser.ParseText(text, stormLocale);

        // assert
        parsed.Should().Be($"deals <c val=\"#TooltipNumbers\">{expectedValue}<c val=\"#ColorGray\">~~{expectedScaling}~~</c></c> damage");
    }

    [TestMethod]
    [DataRow(StormLocale.ENUS, "deals 3.75 (+4.5% per level) damage")]
    [DataRow(StormLocale.DEDE, "deals 3,75 (+4,5 % pro Stufe) damage")]
    public void ParseText_HasStormLocale_GameStringTextReadsLocaleScaling(StormLocale stormLocale, string expected)
    {
        // arrange
        string text = "deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,LocaleDamage,Amount*1.5\" precision=\"2\"/></c> damage";

        GameStringParser parser = new(CreateLocaleDamageStorage());

        // act
        GameStringText gameStringText = new(parser.ParseText(text, stormLocale), stormLocale, false);

        // assert
        gameStringText.PlainTextWithScaling.Should().Be(expected);
    }

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
