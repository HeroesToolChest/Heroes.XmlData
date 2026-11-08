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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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
    public void ParseText_ConditionalEventsIsAnArray_ParsedGameString()
    {
        // arrange
        string text = "<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Basic Attacks against Heroes while Windfury's Movement Speed bonus is active increase Attack Damage by <c val=\"#TooltipNumbers\"><d ref=\"Accumulator,ThrallMaelstromWeaponDamageAccumulator,Scale\"precision=\"2\"/></c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ThrallMaelstromWeaponTokenCounter,ConditionalEvents[0].CompareValue*Accumulator,ThrallMaelstromWeaponDamageAccumulator,Scale\"/></c> Attack Damage, increase the Movement Speed bonus of Windfury to <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer,EffectArray[0].Value\"/>%</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ThrallMaelstromWeaponTokenCounter,ConditionalEvents[1].CompareValue\"/></c> Attack Damage, Thrall permanently gains <c val=\"#TooltipNumbers\"><d ref=\"100*Behavior,ThrallWindfuryMaelstromWeapon2ndQuestCompletionMoveSpeedCarry,Modification.UnifiedMoveSpeedFactor\"/>%</c> increased Movement Speed.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAccumulatorToken"), "Accumulator");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorTokenCounter"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyPlayer"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAccumulatorToken default="1" id="BaseTokenAccumulator">
                  <Scale value="1" />
                </CAccumulatorToken>
                """),
            "Accumulator");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAccumulatorToken id="ThrallMaelstromWeaponDamageAccumulator" parent="BaseTokenAccumulator">
                  <MinAccumulation value="0" />
                  <MaxAccumulation value="3000" />
                </CAccumulatorToken>
                """),
            "Accumulator");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorTokenCounter id="ThrallMaelstromWeaponTokenCounter">
                  <ConditionalEvents CompareValue="20">
                    <Event Effect="ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer" />
                  </ConditionalEvents>
                  <ConditionalEvents CompareValue="40">
                    <Event Effect="ThrallWindfuryMaelstromWeaponTalent2ndQuestCompletionApplyCarry" />
                  </ConditionalEvents>
                </CBehaviorTokenCounter>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyPlayer id="ThrallWindfuryMaelstromWeaponTalent1stQuestCompletionModifyPlayer">
                  <EffectArray Reference="Behavior,WindfurySpeedBuff,Modification.UnifiedMoveSpeedFactor" Value=".4" />
                </CEffectModifyPlayer>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ThrallWindfuryMaelstromWeapon2ndQuestCompletionMoveSpeedCarry">
                  <Modification>
                    <UnifiedMoveSpeedFactor value="0.15" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("<img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Repeatable Quest:</c> Basic Attacks against Heroes while Windfury's Movement Speed bonus is active increase Attack Damage by <c val=\"#TooltipNumbers\">1</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">20</c> Attack Damage, increase the Movement Speed bonus of Windfury to <c val=\"#TooltipNumbers\">40%</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Reward:</c> After gaining <c val=\"#TooltipNumbers\">40</c> Attack Damage, Thrall permanently gains <c val=\"#TooltipNumbers\">15%</c> increased Movement Speed.");
    }

    [TestMethod]
    public void ParseText_DRefExpressionTrailingCommandAndQuote_ParsedGameString()
    {
        // arrange
        string text = "Increase the Slow amount of each Twin Cleave axe by <c val=\"#TooltipNumbers\"><d ref=\"(Behavior,ZuljinTwinCleaveLacerateFirstHitSlow,Modification.UnifiedMoveSpeedFactor-Behavior,ZuljinTwinCleaveFirstHitSlow,Modification.UnifiedMoveSpeedFactor)*(-100)\" player=\"0\" precision=\"2\"/>%</c> and its duration by <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ZuljinTwinCleaveLacerateFirstHitSlow,Duration-Behavior,ZuljinTwinCleaveFirstHitSlow,Duration,\"player=\"0\"precision=\"1\"/></c> seconds.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectTarget"), "Abil");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ZuljinTwinCleaveFirstHitSlow" Duration="2">
                  <Modification>
                    <UnifiedMoveSpeedFactor value="-0.15" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ZuljinTwinCleaveLacerateFirstHitSlow" Duration="2.5">
                  <Modification>
                    <UnifiedMoveSpeedFactor value="-0.25" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Increase the Slow amount of each Twin Cleave axe by <c val=\"#TooltipNumbers\">10%</c> and its duration by <c val=\"#TooltipNumbers\">0.5</c> seconds.");
    }

    [TestMethod]
    public void ParseText_LevelScalingArrayIsUsingNumericalIndexInsteadOfText_ParsedGameString()
    {
        // arrange
        string text = "Probius gains permanent Shields equal to <c val=\"#TooltipNumbers\"><d ref=\"(Behavior,ProbiusShieldCapacitorPassiveShieldBuff,Modification.VitalMaxArray[Shields] / Unit,HeroProbius,LifeMax) * 100\"/>% </c>of his max Health. Shields regenerate quickly as long as he hasn't taken damage recently.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CUnit"), "Unit");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ProbiusShieldCapacitorPassiveShieldBuff" Duration="2">
                  <Modification>
                    <VitalMaxArray index="Shields" value="126" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CUnit id="HeroProbius">
                  <LifeMax value="1260" />
                </CUnit>
                """),
            "Unit");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="ProbiusShieldCapacitorPassiveShieldBuff" />
                  <Field value="Modification.VitalMaxArray[1]" />
                  <Value value="0.040000" />
                </Modifications>
                <Modifications>
                  <Catalog value="Unit" />
                  <Entry value="HeroProbius" />
                  <Field value="LifeMax" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Probius gains permanent Shields equal to <c val=\"#TooltipNumbers\">10% </c>of his max Health. Shields regenerate quickly as long as he hasn't taken damage recently.");
    }

    [TestMethod]
    public void ParseText_ConstValuesWithNegateConstExpressions_ParsedGameString()
    {
        // arrange
        string text = "Launch a fireball, Burning enemies hit for <c val=\"#TooltipNumbers\"><d ref=\"Effect,AlexstraszaFlameBuffetDoTDamage,Amount*(1+Behavior,AlexstraszaFlameBuffetDoT,PeriodCount)\"/></c> damage over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AlexstraszaFlameBuffetDoT,Duration\" player=\"0\" precision=\"1\"/></c> seconds.<n/><n/>Hitting enemies that are already Burning deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,AlexstraszaFlameBuffetImpactBonusDamage,Amount\"/></c> bonus damage upon impact, Slows them by <c val=\"#TooltipNumbers\"><d ref=\"-Behavior,AlexstraszaFlameBuffetSlow,Modification.UnifiedMoveSpeedFactor*100\" player=\"0\"/>%</c> decaying over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,AlexstraszaFlameBuffetSlow,Duration\" player=\"0\" precision=\"2\"/></c> seconds, and refunds the Mana cost.<n/><n/><c val=\"#ColorViolet\">Dragonqueen: Wing Buffet</c><n/>Damage and Knockback enemies in an arc.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AlexstraszaFlameBuffetSlowDuration" value="2" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AlexstraszaFlameBuffetSlowAmountPos" value="0.4" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AlexstraszaFlameBuffetSlowAmount" value="negate($AlexstraszaFlameBuffetSlowAmountPos)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AlexstraszaFlameBuffetDoTDamage" parent="StormSpellDot">
                  <Amount value="15" />
                  <SourceButtonFace value="AlexstraszaFlameBuffet" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="AlexstraszaFlameBuffetDoT" parent="StormDot">
                  <Duration value="5.5" />
                  <Period value="1" />
                  <PeriodCount value="4" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="AlexstraszaFlameBuffetImpactBonusDamage" parent="StormSpell">
                  <Flags index="Crit" value="1" />
                  <Amount value="125" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="AlexstraszaFlameBuffetSlow" parent="StormSlowParent">
                  <Duration value="$AlexstraszaFlameBuffetSlowDuration" />
                  <Modification>
                    <UnifiedMoveSpeedFactor value="$AlexstraszaFlameBuffetSlowAmount">
                      <AccumulatorArray value="AlexstraszaFlameBuffetDecayAccumulator" />
                    </UnifiedMoveSpeedFactor>
                  </Modification>
                </CBehaviorBuff>
                """,
                layeredDataStorage),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Launch a fireball, Burning enemies hit for <c val=\"#TooltipNumbers\">75</c> damage over <c val=\"#TooltipNumbers\">5.5</c> seconds.<n/><n/>Hitting enemies that are already Burning deals <c val=\"#TooltipNumbers\">125</c> bonus damage upon impact, Slows them by <c val=\"#TooltipNumbers\">40%</c> decaying over <c val=\"#TooltipNumbers\">2</c> seconds, and refunds the Mana cost.<n/><n/><c val=\"#ColorViolet\">Dragonqueen: Wing Buffet</c><n/>Damage and Knockback enemies in an arc.");
    }

    [TestMethod]
    public void ParseText_InnerElementsWithConstValueAttributes_ParsedGameString()
    {
        // arrange
        string text = "Basic Attacks with Repeater Cannon reduce Ability cooldowns by <c val=\"#TooltipNumbers\"><d ref=\"-1*Effect,FenixArsenalOverchargeModifyCooldowns,Cost[0].CooldownTimeUse\" precision=\"2\"/></c> seconds. Basic Attacks with Phase Bomb active reduce Ability cooldowns by <c val=\"#TooltipNumbers\"><d ref=\"-1*Effect,FenixArsenalOverchargeModifyCooldowns,Cost[0].CooldownTimeUse\" precision=\"2\"/></c></c> seconds per Hero hit.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$FenixArsenalOverchargeCDRAmount" value="-0.5" />
            """));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyUnit id="FenixArsenalOverchargeModifyCooldowns">
                  <Cost Abil="FenixPlasmaCutter,Execute" CooldownOperation="Add" CooldownTimeUse="$FenixArsenalOverchargeCDRAmount" />
                  <Cost Abil="FenixWarp,Execute" CooldownOperation="Add" CooldownTimeUse="$FenixArsenalOverchargeCDRAmount" />
                  <Cost Abil="FenixPurificationSalvo,Execute" CooldownOperation="Add" CooldownTimeUse="$FenixArsenalOverchargeCDRAmount" />
                  <Cost Abil="FenixPlanetCracker,Execute" CooldownOperation="Add" CooldownTimeUse="$FenixArsenalOverchargeCDRAmount" />
                </CEffectModifyUnit>
                """,
                layeredDataStorage),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Basic Attacks with Repeater Cannon reduce Ability cooldowns by <c val=\"#TooltipNumbers\">0.5</c> seconds. Basic Attacks with Phase Bomb active reduce Ability cooldowns by <c val=\"#TooltipNumbers\">0.5</c></c> seconds per Hero hit.");
    }

    [TestMethod]
    public void ParseText_ArrayElementHasNoDefaultIndexer_ParsedGameString()
    {
        // arrange
        string text = "<s val=\"StandardTooltipDetails\"><d ref=\"Abil,GuldanLifeTap,Cost.Vital[Life]\"/></s>";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectInstant"), "Abil");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectInstant id="GuldanLifeTap">
                  <Effect value="GuldanLifeTapCastSet" />
                  <Cost>
                    <Vital index="Life" value="222" />
                    <Cooldown TimeUse="0.5" />
                  </Cost>
                  <CmdButtonArray index="Execute" DefaultButtonFace="GuldanLifeTap" Requirements="GuldanLifeTapManaControllerNotActive" />
                </CAbilEffectInstant>
                """),
            "Abil");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("<s val=\"StandardTooltipDetails\">222</s>");
    }

    [TestMethod]
    public void ParseText_FractionalMath_ParsedGameString()
    {
        // arrange
        string text = "Wall Ride's Movement Speed bonus gradually increases to <c val=\"#TooltipNumbers\"><d ref=\"(Accumulator,LucioWallRideAccelerandoTokenAccumulator,MaxAccumulation/Unit,HeroLucio,Speed)+(Behavior,LucioWallRideSpeed,Modification.MoveSpeedBonus/Unit,HeroLucio,Speed)*100\" player=\"0\"/>%</c> over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,LucioWallRideAccelerandoToken,Max*Behavior,LucioWallRideSpeed,Period\"/></c> seconds while Lúcio maintains its effect.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAccumulatorToken"), "Accumulator");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CUnit"), "Unit");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$HeroBaseMovementSpeed" value="4.8398" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideAcceleranceBonusMovespeedModifier" value="0.2" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideAccelerandoAccumulatorMaximum" value="*($HeroBaseMovementSpeed $LucioWallrideAcceleranceBonusMovespeedModifier)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideBonusMovespeedModifier" value="0.2" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideBonusMovespeed" value="*($HeroBaseMovementSpeed $LucioWallrideBonusMovespeedModifier)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideAccelerandoTimeToMaximum" value="6" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideUpdatePeriod" value="0.125" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$LucioWallrideAccelerandoMaximumStacks" value="/($LucioWallrideAccelerandoTimeToMaximum $LucioWallrideUpdatePeriod)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAccumulatorToken id="LucioWallRideAccelerandoTokenAccumulator" parent="BaseAccumulator">
                  <MaxAccumulation value="$LucioWallrideAccelerandoAccumulatorMaximum" />
                  <ApplicationRule value="Add" />
                  <TokenId value="LucioWallRideAccelerandoToken" />
                  <Scale value="$LucioWallrideAccelerandoAccumulatorScale" />
                </CAccumulatorToken>
                """,
                layeredDataStorage),
            "Accumulator");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CUnit id="HeroLucio" parent="StormHeroMountedCustom">
                  <Speed value="4.8398" />
                </CUnit>
                """),
            "Unit");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="LucioWallRideSpeed" parent="StormSprint">
                  <Duration value="2" />
                  <Modification>
                    <StateFlags index="SuppressCollision" value="1" />
                    <MoveSpeedBonus value="$LucioWallrideBonusMovespeed">
                      <AccumulatorArray value="LucioWallRideAccelerandoTokenAccumulator" />
                    </MoveSpeedBonus>
                  </Modification>
                  <Period value="$LucioWallrideUpdatePeriod" />
                </CBehaviorBuff>
                """,
                layeredDataStorage),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorTokenCounter id="LucioWallRideAccelerandoToken" parent="StormGenericToken">
                  <Alignment value="Positive" />
                  <Max value="$LucioWallrideAccelerandoMaximumStacks" />
                  <SortIndex value="10" />
                </CBehaviorTokenCounter>
                """,
                layeredDataStorage),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Wall Ride's Movement Speed bonus gradually increases to <c val=\"#TooltipNumbers\">40%</c> over <c val=\"#TooltipNumbers\">6</c> seconds while Lúcio maintains its effect.");
    }

    [TestMethod]
    public void ParseText_WithGermanQuoteInDRef_ParsedGameString()
    {
        // arrange
        string text = "Scharfschütze fügt Gegnern in der Nähe des Einschlagorts zusätzlich <c val=\"#TooltipNumbers\"><d ref=\"100*(1+Effect,SnipeExplosiveDamage,MultiplicativeModifierArray[ExplosiveSnipe].Modifier)“/>%</c> des Schadens zu.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="SnipeExplosiveDamage" parent="StormSpell">
                  <Amount value="230" />
                  <ValidatorArray value="noMarkers" />
                  <SourceButtonFace value="NovaSnipeExplosiveRoundTalent" />
                  <MultiplicativeModifierArray index="PrecisionSniperBase" Accumulator="NovaPrecisionSniperAccumulator" />
                  <MultiplicativeModifierArray index="PrecisionSniperMax" Validator="NovaHas5StacksOfPrecisionSniper" Modifier="0.25" />
                  <MultiplicativeModifierArray index="ExplosiveSnipe" Modifier="-0.1" />
                  <DamageModifierSource Value="Creator" />
                </CEffectDamage>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Scharfschütze fügt Gegnern in der Nähe des Einschlagorts zusätzlich <c val=\"#TooltipNumbers\">0%</c> des Schadens zu.");
    }

    [TestMethod]
    public void ParseText_ArthasFrostmourneHungersDamageScaling_ParsedGameString()
    {
        // arrange
        string text = "Activate to make Arthas's next Basic Attack strike immediately and deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,ArthasFrostmourneHungersDamage,FlatModifierArray[0].Modifier \"/></c> increased damage. Dealing damage restores <c val=\"#TooltipNumbers\"><d ref=\"Effect,ArthasFrostmourneHungersCreateManaHealer,RechargeVitalRate\" player=\"0\"/></c> Mana.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ArthasFrostmourneHungersDamage" parent="StormWeapon">
                  <Flags index="Crit" value="1" />
                  <Amount value="95">
                    <AccumulatorArray value="ArthasFrostmourneHungersEternalHungerDamageAccumulator" />
                  </Amount>
                  <FlatModifierArray index="0" Modifier="99" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectCreateHealer id="ArthasFrostmourneHungersCreateManaHealer" parent="StormManaRestore">
                  <ValidatorArray value="CasterNotBlindedAndTargetNotEvading" />
                  <FlatModifierArray index="ArthasFrostmourneHungersEternalHunger" Accumulator="ArthasFrostmourneHungersEternalHungerManaAccumulator" />
                  <ImpactUnit Value="Caster" />
                  <RechargeVitalRate value="30" />
                </CEffectCreateHealer>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="ArthasFrostmourneHungersDamage" />
                  <Field value="FlatModifierArray[0].Modifier" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Activate to make Arthas's next Basic Attack strike immediately and deal <c val=\"#TooltipNumbers\">99<c val=\"#ColorGray\">~~0.04~~</c></c> increased damage. Dealing damage restores <c val=\"#TooltipNumbers\">30</c> Mana.");
    }

    [TestMethod]
    public void ParseText_CrusaderDivineFortressNoIndex_ParsedGameString()
    {
        // arrange
        string text = "Basic Attacks grant <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CrusaderDivineFortressArmorBuff,ArmorModification.ArmorSet.ArmorMitigationTable[Basic]\"/></c> Physical Armor for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CrusaderDivineFortressArmorBuff,Duration\"Precision=\"2\"/></c> seconds, stacking up to <c val=\"#TooltipNumbers\"><d ref=\"Behavior,CrusaderDivineFortressArmorBuff,ArmorModification.ArmorSet.ArmorMitigationTable[Basic]*Behavior,CrusaderDivineFortressArmorBuff,MaxStackCount\"/></c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Gambit:</c> Gain <c val=\"#TooltipNumbers\"><d ref=\"100*Accumulator,CrusaderDivineFortressInstantHealthAccumulator,MaxAccumulation\"/>%</c> bonus maximum Health. Every death reduces this bonus by <c val=\"#TooltipNumbers\"><d ref=\"100*Accumulator,CrusaderDivineFortressInstantHealthAccumulator,Scale\"/>%</c>.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAccumulatorToken"), "Accumulator");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="CrusaderDivineFortressArmorBuff" parent="StormArmorPermanent">
                  <MaxStackCount value="4" />
                  <Duration value="4" />
                  <ArmorModification StackCount="4">
                    <ArmorSet index="Hero">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                    <ArmorSet index="Merc">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                    <ArmorSet index="Monster">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                    <ArmorSet index="Summon">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                    <ArmorSet index="Structure">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                    <ArmorSet index="Minion">
                      <ArmorMitigationTable index="Basic" value="10" />
                    </ArmorSet>
                  </ArmorModification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAccumulatorToken id="CrusaderDivineFortressInstantHealthAccumulator" parent="BaseAccumulator">
                  <MaxAccumulation value="0.25" />
                  <ApplicationRule value="Add" />
                  <TokenId value="CrusaderDivineFortressInstantTokenCounter" />
                  <Scale value="0.05" />
                </CAccumulatorToken>
                """),
            "Accumulator");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Basic Attacks grant <c val=\"#TooltipNumbers\">10</c> Physical Armor for <c val=\"#TooltipNumbers\">4</c> seconds, stacking up to <c val=\"#TooltipNumbers\">40</c>.<n/><n/><img path=\"@UI/StormTalentInTextQuestIcon\" alignment=\"uppermiddle\" color=\"B48E4C\" width=\"20\" height=\"22\"/><c val=\"#TooltipQuest\">Gambit:</c> Gain <c val=\"#TooltipNumbers\">25%</c> bonus maximum Health. Every death reduces this bonus by <c val=\"#TooltipNumbers\">5%</c>.");
    }

    [TestMethod]
    public void ParseText_Scaling_ParsedGameString()
    {
        // arrange
        string text = "Deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,GallShadowflameDamage,Amount\"/></c> damage to enemies in the area.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="GallShadowflameDamage" parent="StormSpell">
                  <Amount value="135" />
                  <MultiplicativeModifierArray index="0" Validator="GallTargetHasEdgeofMadnessTokenCounter" Accumulator="GallShadowflameEdgeofMadnessTalentDamageAccumulator" Crit="1" />
                  <MultiplicativeModifierArray index="PsychoticBreak" Validator="GallPsychoticBreakHeroDamageCombine" Modifier="0.1" />
                  <MultiplicativeModifierArray index="OgreRage" Validator="GallCasterHasOgreRageSpellDamageBuff" Accumulator="GallTheWillofGallDamageAccumulator" Modifier="$GallOgreRageBonus" />
                  <MultiplicativeModifierArray index="OgreRampage" Validator="GallOgreRampageCombine" Accumulator="GallOgreRampageSpellPowerAccumulator" Modifier="$GallOgreRampageBonus" />
                  <LeechRecipientArray index="0" Value="Creator" />
                  <DamageModifierSource Value="Creator" />
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
                  <Entry value="GallShadowflameDamage" />
                  <Field value="Amount" />
                  <Value value="0.050000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Deal <c val=\"#TooltipNumbers\">135<c val=\"#ColorGray\">~~0.05~~</c></c> damage to enemies in the area.");
    }

    [TestMethod]
    public void ParseText_HasNumericalAndTextIndex_ParsedGameString()
    {
        // arrange
        string text = "Increase Dread's damage by <C val=\"#TooltipNumbers\"><d ref=\"Effect,OrpheaDreadLineDamage,MultiplicativeModifierArray[GrowingNightmarePassive].Modifier * 100\"/>%</c>. <n/><n/><c val=\"#TooltipQuest\">Quest:</c> Hit <C val=\"#TooltipNumbers\"><d ref=\"Behavior,OrpheaDreadGrowingNightmareMarkerBehavior,MaxStackCount\"/></c> enemy Heroes with a single cast of Dread's eruption.<n/><n/><c val=\"#TooltipQuest\">Reward:</c> Permanently increase Dread's eruption damage by <C val=\"#TooltipNumbers\"><d ref=\"100*Effect,OrpheaDreadFinalDamage,MultiplicativeModifierArray[GrowingNightmareQuestComplete].Modifier\"/>%</c> and increase the Slow amount of Dread's eruption by <C val=\"#TooltipNumbers\"><d ref=\"-100*Effect,OrpheaDreadGrowingNightmareQuestCompletionModifyPlayer,EffectArray[0].Value\"/>%</c>.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="OrpheaDreadGrowingNightmareMarkerBehavior">
                  <MaxStackCount value="3" />
                  <Duration value="0.0625" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="OrpheaDreadLineDamage" parent="StormSpell">
                  <Amount value="85">
                    <AccumulatorArray value="OrpheaMindDevourerAccumulator" />
                  </Amount>
                  <MultiplicativeModifierArray index="0" Validator="OrpheaAncestralStrengthDamageValidator" Modifier="0.15" Crit="1" />
                  <MultiplicativeModifierArray index="GrowingNightmarePassive" Validator="OrpheaHasGrowingNightmareTalent" Modifier="0.5" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="OrpheaDreadFinalDamage" parent="StormSpell">
                  <Amount value="175">
                    <AccumulatorArray value="OrpheaMindDevourerAccumulator" />
                  </Amount>
                  <MultiplicativeModifierArray index="0" Validator="OrpheaAncestralStrengthDamageValidator" Modifier="0.15" Crit="1" />
                  <MultiplicativeModifierArray index="GrowingNightmareQuestComplete" Validator="OrpheaDreadHasMaxGrowingNightmareTokens" Modifier="0.5" />
                  <MultiplicativeModifierArray index="GrowingNightmarePassive" Validator="OrpheaHasGrowingNightmareTalent" Modifier="0.5" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyPlayer id="OrpheaDreadGrowingNightmareQuestCompletionModifyPlayer">
                  <WhichPlayer Value="Caster" />
                  <EffectArray Reference="Behavior,OrpheaDreadFinalSlow,Modification.UnifiedMoveSpeedFactor" Value="-0.15" />
                </CEffectModifyPlayer>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Increase Dread's damage by <C val=\"#TooltipNumbers\">50%</c>. <n/><n/><c val=\"#TooltipQuest\">Quest:</c> Hit <C val=\"#TooltipNumbers\">3</c> enemy Heroes with a single cast of Dread's eruption.<n/><n/><c val=\"#TooltipQuest\">Reward:</c> Permanently increase Dread's eruption damage by <C val=\"#TooltipNumbers\">50%</c> and increase the Slow amount of Dread's eruption by <C val=\"#TooltipNumbers\">15%</c>.");
    }

    [TestMethod]
    public void ParseText_CostIsAnArray_ParsedGameString()
    {
        // arrange
        string text = "Reduce the Mana cost of Summon Demon Warrior from <c val=\"#TooltipNumbers\"><d ref=\"Abil,AzmodanSummonDemonWarrior,Cost.Vital[Energy]\"Player=\"0\"/></c> to <c val=\"#TooltipNumbers\"><d ref=\"Abil,AzmodanSummonDemonWarrior,Cost.Vital[Energy]+Talent,AzmodanBattleborn,AbilityModificationArray[0].Modifications[0].Value\"Player=\"0\"/></c>. Azmodan's Basic Attacks reduce the cooldown of Summon Demon Warrior by <c val=\"#TooltipNumbers\"><d ref=\"-Effect,AzmodanHeroWeaponBattlebornTalentModifyCooldown,Cost[0].CooldownTimeUse\"precision=\"2\"/></c> seconds and Demon Lieutenant by <c val=\"#TooltipNumbers\"><d ref=\"-Effect,AzmodanHeroWeaponBattlebornTalentModifyCooldown,Cost[1].CooldownTimeUse\"precision=\"1\"/></c> seconds.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectTarget"), "Abil");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CTalent"), "Talent");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CAbilEffectTarget id="AzmodanSummonDemonWarrior">
                  <Cost>
                    <Vital index="Energy" value="25" />
                    <Cooldown TimeUse="10" />
                  </Cost>
                </CAbilEffectTarget>
                """),
            "Abil");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CTalent id="AzmodanBattleborn">
                  <Face value="AzmodanGlobOfAnnihilationBattlebornTalent" />
                  <AbilityModificationArray>
                    <Modifications>
                      <Type value="FlatModification" />
                      <Entry value="AzmodanSummonDemonWarrior" />
                      <Field value="Cost.Vital[Energy]" />
                      <Value value="-5.000000" />
                    </Modifications>
                  </AbilityModificationArray>
                </CTalent>
                """),
            "Talent");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyUnit id="AzmodanHeroWeaponBattlebornTalentModifyCooldown">
                  <ValidatorArray value="AzmodanHasBattleborn" />
                  <ValidatorArray value="CasterNotBlindedandTargetNotEvading" />
                  <ImpactUnit Value="Caster" />
                  <Cost Abil="AzmodanSummonDemonWarrior,Execute" CooldownOperation="Add" CooldownTimeUse="-0.75" />
                  <Cost Abil="AzmodanDemonLieutenant,Execute" CooldownOperation="Add" CooldownTimeUse="-1.5" />
                </CEffectModifyUnit>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Reduce the Mana cost of Summon Demon Warrior from <c val=\"#TooltipNumbers\">25</c> to <c val=\"#TooltipNumbers\">20</c>. Azmodan's Basic Attacks reduce the cooldown of Summon Demon Warrior by <c val=\"#TooltipNumbers\">0.75</c> seconds and Demon Lieutenant by <c val=\"#TooltipNumbers\">1.5</c> seconds.");
    }

    [TestMethod]
    public void ParseText_ArrayStartsAt1_ParsedGameString()
    {
        // arrange
        string text = "Activate to swing the Hammer of Twilight, dealing <c val=\"#TooltipNumbers\"><d ref=\"Effect,ChoGallTwilightHammerDamage,Amount\"/></c> damage, pushing enemies away, and Stunning them for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ChoHammerOfTwilightKnockbackBehaviorStun,Duration\" precision=\"2\"/></c> seconds.<n/><n/><c val=\"#AbilityPassive\">Passive:</c> Cho's Basic Attacks deal <c val=\"#TooltipNumbers\"><d ref=\"100*Effect,ChoHeroWeaponDamage,MultiplicativeModifierArray[1].Modifier\"/>%</c> increased damage.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CAbilEffectTarget"), "Abil");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CTalent"), "Talent");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyUnit"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ChoGallTwilightHammerDamage" parent="StormSpell">
                  <Amount value="150" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ChoHeroWeaponDamage" parent="StormWeapon">
                  <Amount value="130" />
                  <MultiplicativeModifierArray index="1" Validator="ChoTalentHasHammerOfTwilight" Modifier="0.25" />
                  <MultiplicativeModifierArray index="SearedFleshCrit" Validator="ChoSearedFleshCritCombine" Accumulator="ChoSearedFleshDamageAccumulator" Crit="1" />
                  <MultiplicativeModifierArray index="SearedFleshNoCrit" Validator="ChoSearedFleshNoCritCombine" Accumulator="ChoSearedFleshDamageAccumulator" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ChoHammerOfTwilightKnockbackBehaviorStun" parent="StormStun">
                  <Duration value="0.75" />
                  <Modification>
                    <ModifyFlags index="SuppressTurning" value="1" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="ChoGallTwilightHammerDamage" />
                  <Field value="Amount" />
                  <Value value="0.045000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Activate to swing the Hammer of Twilight, dealing <c val=\"#TooltipNumbers\">150<c val=\"#ColorGray\">~~0.045~~</c></c> damage, pushing enemies away, and Stunning them for <c val=\"#TooltipNumbers\">0.75</c> seconds.<n/><n/><c val=\"#AbilityPassive\">Passive:</c> Cho's Basic Attacks deal <c val=\"#TooltipNumbers\">25%</c> increased damage.");
    }

    [TestMethod]
    public void ParseText_CapitalDRef_ParsedGameString()
    {
        // arrange
        string text = "Deal <c val=\"#TooltipNumbers\"><d ref=\"Effect,ValeeraGarroteDamage,Amount\"/></c> damage to an enemy and an additional <c val=\"#TooltipNumbers\"><D ref=\"Behavior,ValeeraGarrote,Duration * Effect,ValeeraGarroteDoTDamage,Amount\"/></c> damage over <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ValeeraGarrote,Duration\"/></c> seconds, and Silence them for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,ValeeraGarroteSilence,Duration\" precision=\"2\"/></c> seconds.<n/><n/><c val=\"#GlowColorRed\">Awards 1 Combo Point.</c><n/><n/><c val=\"#ColorViolet\">Unstealth: Eviscerate<n/></c>High damage finishing move.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ValeeraGarroteDamage" parent="StormSpell">
                  <Amount value="20" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ValeeraGarroteDoTDamage" parent="StormSpellDot">
                  <ImpactLocation Value="SourceUnit" />
                  <Amount value="20" />
                  <MultiplicativeModifierArray index="ValeeraGarroteRuptureBonusDamage" Validator="ValeeraHasRupture" Modifier="1" />
                  <SourceButtonFace value="ValeeraGarrote" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ValeeraGarrote" parent="StormDot">
                  <Duration value="7" />
                  <Period value="1" />
                  <PeriodCount value="7" />
                  <InitialEffect value="ValeeraGarroteDamageReveal" />
                  <PeriodicEffect value="ValeeraGarroteDoTDamage" />
                  <PeriodicDisplayEffect value="ValeeraGarroteDoTDamage" />
                  <FinalEffect value="ValeeraGarroteRuptureRemoveTokenCounter" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="ValeeraGarroteSilence" parent="StormSilence">
                  <BuffFlags index="RemoveDamageResponseExhausted" value="0" />
                  <Duration value="2.5" />
                  <DamageResponse Handled="ValeeraGarroteSilenceRefresh">
                    <Cost>
                      <Charge CountMax="3" CountStart="3" CountUse="1" />
                    </Cost>
                    <RequireEffectArray value="ValeeraWeaponDamage" />
                  </DamageResponse>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Deal <c val=\"#TooltipNumbers\">20</c> damage to an enemy and an additional <c val=\"#TooltipNumbers\">140</c> damage over <c val=\"#TooltipNumbers\">7</c> seconds, and Silence them for <c val=\"#TooltipNumbers\">2.5</c> seconds.<n/><n/><c val=\"#GlowColorRed\">Awards 1 Combo Point.</c><n/><n/><c val=\"#ColorViolet\">Unstealth: Eviscerate<n/></c>High damage finishing move.");
    }

    [TestMethod]
    public void ParseText_BracketedDeadRush_ParsedGameString()
    {
        // arrange
        string text = "Zombie Wall deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,WitchDoctorZombieWallWeaponDamage,MultiplicativeModifierArray[DeadRush].Modifier*100\"/>%</c> more damage. When it expires up to <c val=\"#TooltipNumbers\">5</c> remaining Zombies uproot and attack nearby enemies for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,WitchDoctorZombieWallDeadRushTalentTimedLife,Duration\" player=\"0\"/></c> seconds.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");
        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectDamage"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="WitchDoctorZombieWallWeaponDamage" parent="StormSummonedUnitWeapon">
                  <Amount value="28" />
                  <MultiplicativeModifierArray index="DeadRush" Validator="WitchDoctorZombieWallDeadRushSourceHasTalentBuffBehavior" Modifier="1" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="WitchDoctorZombieWallDeadRushTalentTimedLife" parent="StormTimedLife">
                  <DisplayDuration index="Self" value="0" />
                  <DisplayDuration index="Ally" value="0" />
                  <DisplayDuration index="Enemy" value="0" />
                  <Duration value="3" />
                  <InitialEffect value="WitchDoctorZombieWallDeadRushApplyBehaviorSource" />
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Zombie Wall deals <c val=\"#TooltipNumbers\">100%</c> more damage. When it expires up to <c val=\"#TooltipNumbers\">5</c> remaining Zombies uproot and attack nearby enemies for <c val=\"#TooltipNumbers\">3</c> seconds.");
    }

    [TestMethod]
    public void ParseText_ModificationDamageDealtFraction_ParsedGameString()
    {
        // arrange
        string text = "Jaina gains <c val=\"#TooltipNumbers\"><d ref=\"100*Behavior,JainaArcaneIntellectSpellPower,Modification.DamageDealtFraction\"/>%</c> Spell Power.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithNoId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff default="1">
                  <MaxStackCount value="1" />
                  <DurationBonusMin value="-1" />
                  <DurationBonusMax value="-1" />
                  <TimeScaleSource Value="Target" />
                  <Player Value="Unknown" />
                  <AcquirePlayer Value="Unknown" />
                  <BuffFlags index="Countdown" value="1" />
                  <BuffFlags index="RemoveDamageResponseExhausted" value="1" />
                  <HealResponse Location="Receiver">
                    <Vital value="Life" />
                    <HealTypes index="CreateHealer" value="1" />
                    <HealTypes index="Leech" value="1" />
                  </HealResponse>
                  <DamageResponse Location="Defender" />
                  <KillCredit Value="Unknown" />
                  <RevealUnit Value="Unknown" />
                  <ShowInUI value="1" />
                </CBehaviorBuff>
                """));
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff default="1" id="CarryBehaviorParent">
                  <BehaviorFlags index="Permanent" value="1" />
                  <BehaviorFlags index="EnabledWhileDead" value="1" />
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="JainaArcaneIntellectSpellPower" parent="CarryBehaviorParent">
                  <DisableValidatorArray value="JainaHasGTE75PercentMana" />
                  <Modification>
                    <HealDealtAdditiveMultiplier index="Life" value="0.1" />
                    <HealDealtAdditiveMultiplier index="Shields" value="0.1" />
                    <DamageDealtFraction index="Ability" value="0.1" />
                  </Modification>
                  <Face value="JainaFrostbiteArcaneIntellectTalentSpellPower" />
                  <Alignment value="Positive" />
                  <InfoFlags index="Hidden" value="0" />
                  <SortIndex value="20" />
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Jaina gains <c val=\"#TooltipNumbers\">10%</c> Spell Power.");
    }

    [TestMethod]
    public void ParseText_DamageResponseModifyFraction_ParsedGameString()
    {
        // arrange
        string text = "<c val=\"#ColorCreamYellow\">Human</c> Basic Attacks splash for <c val=\"#TooltipNumbers\"><d ref=\"Behavior,GreymaneHuntersBlunderbussCarryBehavior,DamageResponse.ModifyFraction*100\"/>%</c> damage behind the target.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddElementWithNoId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff default="1">
                </CBehaviorBuff>
                """));
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff default="1" id="CarryBehaviorParent">
                </CBehaviorBuff>
                """),
            "Behavior");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="GreymaneHuntersBlunderbussCarryBehavior" parent="CarryBehaviorParent">
                  <DamageResponse Chance="1" Location="Attacker">
                    <RequireEffectArray value="GreymaneHuntersBlunderbussDamage" />
                  </DamageResponse>
                  <Modification>
                    <WeaponDisableArray value="HeroGreymaneMeleeWeapon" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("<c val=\"#ColorCreamYellow\">Human</c> Basic Attacks splash for <c val=\"#TooltipNumbers\">100%</c> damage behind the target.");
    }

    [TestMethod]
    public void ParseText_SameArrayIndexOverride_ParsedGameString()
    {
        // arrange
        string text = "While above <c val=\"#TooltipNumbers\">40</c> Energy, Zarya's Basic Attack size is increased by <c val=\"#TooltipNumbers\"><d ref=\"(Effect,ZaryaWeaponToTheLimitSearch,AreaArray[0].RectangleWidth)-(Effect,ZaryaWeaponSplashTargetSearch,AreaArray[0].RectangleWidth)*100\"/>%</c>.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectEnumArea"), "Effect");

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectEnumArea id="ZaryaWeaponSplashTargetSearch">
                  <AreaArray Effect="ZaryaWeaponSplashDamageSet">
                    <Radius value="0.5" />
                    <RectangleWidth value="1" />
                    <RectangleHeight value="6.25" />
                  </AreaArray>
                </CEffectEnumArea>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectEnumArea id="ZaryaWeaponToTheLimitSearch" parent="ZaryaWeaponSplashTargetSearch">
                  <AreaArray index="0" Effect="ZaryaWeaponSplashDamageSet">
                    <RectangleWidth value="1.35" />
                    <RectangleHeight value="8.4375" />
                  </AreaArray>
                  <AreaRelativeOffset Y="5.4438" />
                </CEffectEnumArea>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("While above <c val=\"#TooltipNumbers\">40</c> Energy, Zarya's Basic Attack size is increased by <c val=\"#TooltipNumbers\">35%</c>.");
    }

    [TestMethod]
    public void ParseText_KerriganRavageDrefContainsMoreThanNeeded_ParsedGameString()
    {
        // arrange
        string text = "Minions grant <c val=\"#TooltipNumbers\"><d ref=\"Effect,KerriganRavageSharpenedBladesModifyTokenCountMinions,Value,KerriganRavageSharpenedBladesDamageAccumulator,Scale\" precision=\"1\"/></c> stack and Heroes grant <c val=\"#TooltipNumbers\"><d ref=\"Effect,KerriganRavageSharpenedBladesModifyTokenCountHeroes,Value,KerriganRavageSharpenedBladesDamageAccumulator,Scale\" precision=\"1\"/></c> stacks.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CEffectModifyTokenCount"), "Effect");

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
                <CEffectModifyTokenCount id="KerriganRavageSharpenedBladesModifyTokenCountMinions" parent="BaseEffectModifyTokenCount">
                  <EffectCategory index="Quest" value="1" />
                </CEffectModifyTokenCount>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyTokenCount default="1" id="BaseEffectModifyTokenCount">
                  <Value value="1" />
                  <ContextUnit Value="Target" />
                  <CollationId value="##id##" />
                </CEffectModifyTokenCount>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectModifyTokenCount id="KerriganRavageSharpenedBladesModifyTokenCountHeroes" parent="BaseEffectModifyTokenCount">
                  <EffectCategory index="Quest" value="1" />
                  <TokenId value="KerriganRavageSharpenedBladesTokenCounter" />
                  <Value value="5" />
                </CEffectModifyTokenCount>
                """),
            "Effect");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Minions grant <c val=\"#TooltipNumbers\">1</c> stack and Heroes grant <c val=\"#TooltipNumbers\">5</c> stacks.");
    }

    [TestMethod]
    public void ParseText_IllidanDrefDoesNotExists_ParsedGameString()
    {
        // arrange
        string text = "If Sweeping Strike hits <c val=\"#TooltipNumbers\">2</c> Heroes its damage bonus is increased from <c val=\"#TooltipNumbers\"><d ref=\"Behavior,IllidanSweepingStrikesDamageBuffNoStacking,Modification.DamageDealtFraction[Basic]*100\"/>%</c>";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="IllidanSweepingStrikesDamageBuffNoStacking">
                  <Modification>
                    <AttackSpeedMultiplier value="1.35" />
                  </Modification>
                </CBehaviorBuff>
                """),
            "Behavior");

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("If Sweeping Strike hits <c val=\"#TooltipNumbers\">2</c> Heroes its damage bonus is increased from <c val=\"#TooltipNumbers\">0%</c>");
    }

    [TestMethod]
    public void ParseText_ConstsWithRound_ParsedGameString()
    {
        // arrange
        string text = "Basic Attacks against enemy Heroes afflicted by Shadow Mark grant a <c val=\"#TooltipNumbers\"><d ref=\"Behavior,XalatathMarkedResilienceShield,DamageResponse.ModifyLimit*Behavior,XalatathMarkedResilienceShield,MaxStackCount\"/></c> point Shield that rapidly decays.";

        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(CreateBaseElement("CBehaviorBuff"), "Behavior");

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceTotalShieldDuration" value="3" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceShieldFullHealthDuration" value="1" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceShieldDecayDuration" value="-($XalatathMarkedResilienceTotalShieldDuration $XalatathMarkedResilienceShieldFullHealthDuration)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceShieldTotalHealth" value="160" />
            """));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceShieldStackCount" value="round(/($XalatathMarkedResilienceShieldTotalHealth /($XalatathMarkedResilienceShieldDecayDuration 0.0625)))" evaluateAsExpression="1" />
            """,
            layeredDataStorage));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathMarkedResilienceShieldHealthPerStack" value="/($XalatathMarkedResilienceShieldTotalHealth $XalatathMarkedResilienceShieldStackCount)" evaluateAsExpression="1" />
            """,
            layeredDataStorage));

        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CBehaviorBuff id="XalatathMarkedResilienceShield">
                  <MaxStackCount value="$XalatathMarkedResilienceShieldStackCount" />
                  <DamageResponse ModifyLimit="$XalatathMarkedResilienceShieldHealthPerStack" />
                </CBehaviorBuff>
                """,
                layeredDataStorage),
            "Behavior");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero>
              <LevelScalingArray>
                <Modifications>
                  <Catalog value="Behavior" />
                  <Entry value="XalatathMarkedResilienceShield" />
                  <Field value="DamageResponse.ModifyLimit" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));

        GameStringParser parser = new(layeredDataStorage);

        // act
        string parsed = parser.ParseText(text, StormLocale.ENUS);

        // assert
        parsed.Should().Be("Basic Attacks against enemy Heroes afflicted by Shadow Mark grant a <c val=\"#TooltipNumbers\">160<c val=\"#ColorGray\">~~0.04~~</c></c> point Shield that rapidly decays.");
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
        SourceContainer sourceContainer = new();
        LayeredDataStorage layeredDataStorage = sourceContainer.LayeredDataStorage;
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
