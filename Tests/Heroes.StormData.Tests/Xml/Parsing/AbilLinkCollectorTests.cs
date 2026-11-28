namespace Heroes.StormData.Xml.Parsing.Tests;

[TestClass]
public sealed class AbilLinkCollectorTests
{
    private readonly AbilLinkCache _abilLinkCache = new();
    private readonly AbilLinkCollector _abilLinkCollector;

    public AbilLinkCollectorTests()
    {
        _abilLinkCollector = new AbilLinkCollector(_abilLinkCache);
    }

    [TestMethod]
    public void Parse_AbilElementsWithId_AddsIdsInOrder()
    {
        // act
        Parse(
            """
            <CAbilEffectTarget id="AlarakDiscordStrike" />
            <CAbilEffectInstant id="AlarakSadism" />
            <CAbilEffectTarget id="AlarakDiscordStrike" />
            """);

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("AlarakDiscordStrike", false), new AbilLinkEntry("AlarakSadism", false));
    }

    [TestMethod]
    public void Parse_AbilElementsWithNoId_AddsDefaultElementOfEachClassOnce()
    {
        // act
        Parse(
            """
            <CAbil default="1" />
            <CAbilEffectTarget default="1" />
            <CAbilEffectTarget default="1" id="" />
            <CAbilEffectTarget id="AlarakDiscordStrike" />
            <CAbilEffectTarget default="1" />
            """);

        // assert
        _abilLinkCache.Entries.Should().Equal(
            new AbilLinkEntry("CAbil", true),
            new AbilLinkEntry("CAbilEffectTarget", true),
            new AbilLinkEntry("AlarakDiscordStrike", false));
    }

    [TestMethod]
    public void Parse_ElementsThatAreNotAbilElements_AddsNothing()
    {
        // act
        Parse(
            """
            <const id="$AlarakDiscordStrikeDamage" value="175" />
            <CEffectDamage id="AlarakDiscordStrikeDamage" />
            <CEffectDamage default="1" />
            <CUnit id="HeroAlarak" />
            """);

        // assert
        _abilLinkCache.Entries.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AbilLinkFields_AddsIdsInXmlOrder()
    {
        // act
        Parse(
            """
            <CUnit id="HeroAlarak">
              <AbilArray Link="AlarakDiscordStrike" />
              <CardLayouts>
                <LayoutButtons Face="AlarakSadism" Type="AbilCmd" AbilCmd="AlarakSadism,Execute" Slot="Trait" />
              </CardLayouts>
              <AbilArray Link="AlarakLightningSurge" />
            </CUnit>
            <CAbilEffectTarget id="AlarakLightningSurge" />
            <CAbilEffectTarget id="AlarakCounterStrike" />
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike", "AlarakSadism", "AlarakLightningSurge", "AlarakCounterStrike");
    }

    [TestMethod]
    public void Parse_AbilLinkFieldsAsValueElements_AddsIds()
    {
        // act
        Parse(
            """
            <CBehaviorBuff id="AlarakDeadlyChargeChannel">
              <Modification>
                <AbilLinkEnableArray value="AlarakDeadlyChargeCancel" />
                <AbilLinkDisableArray value="AlarakDiscordStrike" />
              </Modification>
            </CBehaviorBuff>
            <CTalent id="AlarakChaosReigns">
              <Abil value="AlarakSadism" />
            </CTalent>
            <CValidatorUnitOrderQueue id="AlarakNotQueued">
              <AbilLink value="AlarakCounterStrike" />
            </CValidatorUnitOrderQueue>
            <CUnit id="HeroAlarak">
              <AbilArray>
                <Link value="AlarakLightningSurge" />
              </AbilArray>
            </CUnit>
            """);

        // assert
        GetIds().Should().Equal("AlarakDeadlyChargeCancel", "AlarakDiscordStrike", "AlarakSadism", "AlarakCounterStrike", "AlarakLightningSurge");
    }

    [TestMethod]
    [DataRow("""<CBehaviorBuff id="AlarakTelekinesis"><ParentAbil value="AlarakDiscordStrike" /></CBehaviorBuff>""")]
    [DataRow("""<CHero id="Alarak"><HeroAbilArray Abil="AlarakDiscordStrike" Button="AlarakButton" /></CHero>""")]
    [DataRow("""<CHero id="Alarak"><LevelScalingArray Ability="AlarakDiscordStrike" /></CHero>""")]
    [DataRow("""<CEffectModifyUnit id="AlarakModifyCooldown"><Cost Abil="AlarakDiscordStrike,Execute" CooldownTimeUse="1" /></CEffectModifyUnit>""")]
    [DataRow("""<CBehaviorAbility id="AlarakAbility"><Buttons Face="AlarakButton" AbilCmd="AlarakDiscordStrike,Execute" /></CBehaviorAbility>""")]
    [DataRow("""<CActorRange id="AlarakRange"><Abil Link="AlarakDiscordStrike" /></CActorRange>""")]
    [DataRow("""<CUnit id="HeroAlarak"><TauntAbil index="Dance" value="AlarakDiscordStrike" /></CUnit>""")]
    [DataRow("""<CSound id="AlarakSound"><AbilSoundArray AbilCmd="AlarakDiscordStrike,Execute" /></CSound>""")]
    public void Parse_FieldThatIsAnAbilLink_AddsId(string xml)
    {
        // act
        Parse(xml);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    [TestMethod]
    [DataRow("""<CAbilEffectTarget default="1"><CmdButtonArray index="Execute" DefaultButtonFace="AlarakButton" AutoQueueId="AlarakQueue" /></CAbilEffectTarget>""")]
    [DataRow("""<CEffectModifyUnit id="AlarakModifyCooldown"><Cost><Cooldown Link="Abil/AlarakDiscordStrike" /></Cost></CEffectModifyUnit>""")]
    [DataRow("""<CUnit id="HeroAlarak"><BehaviorArray Link="AlarakSadism" /><WeaponArray Link="AlarakWeapon" /></CUnit>""")]
    [DataRow("""<CActorRange id="AlarakRange"><On Terms="Abil.AlarakDiscordStrike.Guide" Send="Create" /></CActorRange>""")]
    [DataRow("""<CTalent id="AlarakTalent"><AbilityModificationArray><Modifications><Catalog value="Abil" /><Entry value="AlarakDiscordStrike" /></Modifications></AbilityModificationArray></CTalent>""")]
    [DataRow("""<CUnit id="HeroAlarak"><LayoutButtons index="AlarakDiscordStrike" removed="1" /></CUnit>""")]
    [DataRow("""<CUnit id="HeroAlarak"><AbilArray Link="" /><AbilArray Link="  " /></CUnit>""")]
    [DataRow("""<CEffectDamage id="AlarakDamage"><Count Link="AlarakDiscordStrike" /></CEffectDamage>""")]
    public void Parse_FieldThatIsNotAnAbilLink_AddsNothing(string xml)
    {
        // act
        Parse(xml);

        // assert
        GetIds().Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AbilCommand_AddsAbilityWithoutCommand()
    {
        // act
        Parse(
            """
            <CUnit id="HeroAlarak">
              <CardLayouts>
                <LayoutButtons AbilCmd="AlarakDiscordStrike,Execute" />
                <LayoutButtons AbilCmd=" AlarakSadism , Execute" />
                <LayoutButtons AbilCmd="AlarakDiscordStrike,Cancel" />
              </CardLayouts>
            </CUnit>
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike", "AlarakSadism");
    }

    [TestMethod]
    public void Parse_SubmenuAbilState_AddsWholeValue()
    {
        // act
        Parse(
            """
            <CUnit id="HeroButcher">
              <CardLayouts>
                <LayoutButtons AbilCmd="ButcherFreshMeatDummy,Execute" SubmenuAbilState="ButcherFreshMeat" />
                <LayoutButtons AbilCmd="RuthlessOnslaughtCancel,Execute" SubmenuAbilState="RuthlessOnslaughtCancel,Execute" />
                <LayoutButtons>
                  <AbilCmd value="SamuroMirrorImage,Execute" />
                  <SubmenuAbilState value="SamuroMirrorImage,Execute" />
                </LayoutButtons>
              </CardLayouts>
            </CUnit>
            """);

        // assert
        GetIds().Should().Equal(
            "ButcherFreshMeatDummy",
            "ButcherFreshMeat",
            "RuthlessOnslaughtCancel",
            "RuthlessOnslaughtCancel,Execute",
            "SamuroMirrorImage",
            "SamuroMirrorImage,Execute");
    }

    [TestMethod]
    public void Parse_AbilLinkBeforeAbilElement_IdKeepsItsFirstPosition()
    {
        // act
        Parse(
            """
            <CValidatorUnitOrderQueue id="AttackNotQueued">
              <AbilLink value="attack" />
            </CValidatorUnitOrderQueue>
            <CAbilMove id="move" />
            <CAbilAttack id="attack" />
            """);

        // assert
        GetIds().Should().Equal("attack", "move");
    }

    [TestMethod]
    public void Parse_IdsThatDifferInCase_AddsBoth()
    {
        // act
        Parse(
            """
            <CAbilAttack id="attack" />
            <CAbilAttack id="Attack" />
            """);

        // assert
        GetIds().Should().Equal("attack", "Attack");
    }

    [TestMethod]
    [DataRow("CRequirementCountAbil", """<Count Link="AlarakDiscordStrike" State="CompleteOnly" />""", true)]
    [DataRow("CRequirementCountBehavior", """<Count Link="AlarakDiscordStrike" State="CompleteOnly" />""", false)]
    [DataRow("CRequirementAllowAbil", """<Link value="AlarakDiscordStrike" />""", true)]
    [DataRow("CRequirementAllowBehavior", """<Link value="AlarakDiscordStrike" />""", false)]
    public void Parse_LinkOfRequirementNode_AddsIdForAbilRequirementNodes(string elementName, string innerXml, bool isAbilLink)
    {
        // act
        Parse($"""<{elementName} id="AlarakRequirement">{innerXml}</{elementName}>""");

        // assert
        if (isAbilLink)
            GetIds().Should().Equal("AlarakDiscordStrike");
        else
            GetIds().Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_CatalogReference_AddsIdOfAbilReferenceOnly()
    {
        // act
        Parse(
            """
            <CAccumulatorUserData id="AlarakAccumulator">
              <EffectArray Reference="Behavior,AlarakSadism,Modification.DamageDealtFraction" />
              <EffectArray Reference="Abil,AlarakDiscordStrike,Cost[0].Cooldown.TimeUse" />
            </CAccumulatorUserData>
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    [TestMethod]
    public void Parse_ParentFieldHasToken_AddsTokenValueOfChildAtChild()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CAbilEffectTarget id="AlarakSadism" />
            <CActorRange id="AlarakDiscordStrikeRange" parent="RangeAbil" abil="AlarakDiscordStrike" />
            <CAbilEffectTarget id="AlarakLightningSurge" />
            """);

        // assert
        GetIds().Should().Equal("AlarakSadism", "AlarakDiscordStrike", "AlarakLightningSurge");
    }

    [TestMethod]
    public void Parse_ParentOfParentFieldHasToken_AddsTokenValueOfChild()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CActorRange id="RangeAbilPlusSplat" parent="RangeAbil" />
            <CActorRange id="AlarakDiscordStrikeRange" parent="RangeAbilPlusSplat" abil="AlarakDiscordStrike" />
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    [TestMethod]
    public void Parse_TokenHasDefaultValue_AddsDefaultValueWhenChildDoesNotSetIt()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink" value="attack"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CActorRange id="AlarakAttackRange" parent="RangeAbil" />
            <CActorRange id="AlarakDiscordStrikeRange" parent="RangeAbil" abil="AlarakDiscordStrike" />
            """);

        // assert
        // the element with the token is seen with its default value too
        GetIds().Should().Equal("attack", "AlarakDiscordStrike");
    }

    [TestMethod]
    public void Parse_TokenValueHasIdToken_AddsValueWithIdOfChild()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink" value="##id##"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CActorRange id="AlarakDiscordStrike" parent="RangeAbil" />
            <CActorRange id="AlarakRange" parent="RangeAbil" abil="##id##Cast" />
            """);

        // assert
        GetIds().Should().Equal("RangeAbil", "AlarakDiscordStrike", "AlarakRangeCast");
    }

    [TestMethod]
    public void Parse_TokenIsNotInAnAbilLinkField_AddsNothing()
    {
        // act
        Parse(
            """
            <CActorSplat id="StormGuideVisualCircle">
              <?token id="abil" type="CAbilLink"?>
              <On Terms="Abil.##abil##.Guide" Send="Create" />
            </CActorSplat>
            <CActorSplat id="ButcherLambToTheSlaughterCursorSplat" parent="StormGuideVisualCircle" abil="LambToTheSlaughter" />
            """);

        // assert
        GetIds().Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_TokenHasNoValue_AddsNothing()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
              <Abil Link="##other##" />
            </CActorRange>
            <CActorRange id="AlarakRange" parent="RangeAbil" />
            """);

        // assert
        GetIds().Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_ParentInOtherCatalogHasSameId_IsNotTheParent()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CEffectDamage id="AlarakDamage" parent="RangeAbil" abil="AlarakDiscordStrike" />
            """);

        // assert
        GetIds().Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_LaterDefinitionWithNoParentHasTokenField_AddsToTheElement()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CActorRange id="RangeAbil">
              <?token id="queryAbil" type="CAbilLink"?>
              <QueryAbilCmd value="##queryAbil##,Execute" />
            </CActorRange>
            <CActorRange id="AlarakRange" parent="RangeAbil" abil="AlarakDiscordStrike" queryAbil="AlarakSadism" />
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike", "AlarakSadism");
    }

    [TestMethod]
    public void Parse_ParentIsItsOwnParent_DoesNotLoop()
    {
        // act
        Parse(
            """
            <CActorRange id="RangeAbil" parent="RangeAbilPlusSplat">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CActorRange id="RangeAbilPlusSplat" parent="RangeAbil" />
            <CActorRange id="AlarakRange" parent="RangeAbilPlusSplat" abil="AlarakDiscordStrike" />
            """);

        // assert
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    [TestMethod]
    public void Parse_ParentIsInEarlierCache_AddsTokenValueToItsOwnCache()
    {
        // arrange
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            """);

        AbilLinkCache mapAbilLinkCache = new();
        AbilLinkCollector mapAbilLinkCollector = new(mapAbilLinkCache, _abilLinkCache);

        // act
        Parse(
            """
            <CActorRange id="MapRange" parent="RangeAbil" abil="MapAbility" />
            """,
            mapAbilLinkCollector);

        // assert
        _abilLinkCache.Entries.Should().BeEmpty();
        mapAbilLinkCache.Entries.Should().Equal(new AbilLinkEntry("MapAbility", false));
    }

    [TestMethod]
    public void Parse_LaterDefinitionInLaterCache_DoesNotChangeEarlierCache()
    {
        // arrange
        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            """);

        AbilLinkCache mapAbilLinkCache = new();
        AbilLinkCollector mapAbilLinkCollector = new(mapAbilLinkCache, _abilLinkCache);

        Parse(
            """
            <CActorRange id="RangeAbil">
              <?token id="queryAbil" type="CAbilLink"?>
              <QueryAbilCmd value="##queryAbil##,Execute" />
            </CActorRange>
            <CActorRange id="MapRange" parent="RangeAbil" abil="MapAbility" queryAbil="MapQueryAbility" />
            """,
            mapAbilLinkCollector);

        // act
        Parse(
            """
            <CActorRange id="AlarakRange" parent="RangeAbil" abil="AlarakDiscordStrike" queryAbil="AlarakSadism" />
            """);

        // assert
        mapAbilLinkCache.Entries.Select(x => x.Value).Should().Equal("MapAbility", "MapQueryAbility");
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    [TestMethod]
    public void Parse_NoCollector_AddsNothing()
    {
        // arrange
        using XmlReader reader = XmlReaderHelper.CreateXmlReader("""<CAbilEffectTarget id="AlarakDiscordStrike" />""");

        // act
        List<StormXmlElement> elements = [.. XmlReaderHelper.CreateStormXmlFileParser().Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        elements.Should().ContainSingle();
        _abilLinkCache.Entries.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_WithCollector_ReturnsSameElements()
    {
        // arrange
        const string Xml =
            """
            <const id="$AlarakRange" value="5" />
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CUnit id="HeroAlarak">
              <AbilArray Link="AlarakDiscordStrike" />
            </CUnit>
            """;

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(Xml);
        using XmlReader collectorReader = XmlReaderHelper.CreateXmlReader(Xml);

        // act
        List<StormXmlElement> elements = [.. XmlReaderHelper.CreateStormXmlFileParser().Parse(reader, StormFileHelper.CreateStormFile())];
        List<StormXmlElement> collectorElements = [.. XmlReaderHelper.CreateStormXmlFileParser().Parse(collectorReader, StormFileHelper.CreateStormFile(), _abilLinkCollector)];

        // assert
        collectorElements.Select(x => (x.ElementName, x.Id, x.Parent)).Should().Equal(elements.Select(x => (x.ElementName, x.Id, x.Parent)));
        GetIds().Should().Equal("AlarakDiscordStrike");
    }

    private string[] GetIds() => [.. _abilLinkCache.Entries.Where(x => !x.IsDefaultElement).Select(x => x.Value)];

    private void Parse(string xml, AbilLinkCollector? abilLinkCollector = null)
    {
        using XmlReader reader = XmlReaderHelper.CreateXmlReader(xml);

        // the elements are parsed as they are enumerated
        foreach (StormXmlElement element in XmlReaderHelper.CreateStormXmlFileParser().Parse(reader, StormFileHelper.CreateStormFile(), abilLinkCollector ?? _abilLinkCollector))
        {
            _ = element;
        }
    }
}
