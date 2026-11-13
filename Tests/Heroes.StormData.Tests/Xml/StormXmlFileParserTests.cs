using Heroes.StormData.Cache.Strings;

namespace Heroes.StormData.Xml.Tests;

[TestClass]
public sealed class StormXmlFileParserTests
{
    [TestMethod]
    public void Parse_GeneralElements_ReturnsItems()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <const id="$XalatathLife" value="1330"/>
            <SMarker>
              <Link value="Abil/##id##"/>
              <MatchFlags index="Link" value="1"/>
              <MismatchFlags index="Id" value="1"/>
            </SMarker>
            <SCost>
              <Charge Link="Abil/##id##"/>
              <Cooldown Link="Abil/##id##"/>
            </SCost>
            <CAbil default="1">
              <Name value="Abil/Name/##id##"/>
              <TechPlayer value="Upkeep"/>
              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage"/>
              <OrderArray>
                <Color value="255,0,255,0"/>
                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3"/>
                <LineTexture value="Assets\Textures\WayPointLine.dds"/>
              </OrderArray>
              <SharedFlags index="DisableWhileDead" value="1"/>
              <SharedFlags index="AllowQuickCastCustomization" value="1"/>
              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1"/>
            </CAbil>
            <CButton id="JainaConeOfCold" parent="StormButtonParent">
              <Tooltip value="Button/Tooltip/JainaConeofCold" />
            </CButton>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result.Should().HaveCount(5);
        result[0].ElementName.Should().Be("const");
        result[1].ElementName.Should().Be("Marker");
        result[2].ElementName.Should().Be("Cost");
        result[3].ElementName.Should().Be("Abil");
        result[3].IsDefault.Should().BeTrue();
        result[4].ElementName.Should().Be("Button");
        result[4].Id.Should().Be("JainaConeOfCold");
        result[4].Parent.Should().Be("StormButtonParent");
    }

    [TestMethod]
    public void Parse_ConstNoExpression_ShouldReturnConstElement()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <const id="$XalatathLife" value="1330" />
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        StormXmlConstElement constElement = (StormXmlConstElement)element;

        constElement.ElementName.Should().Be("const");
        constElement.ElementType.Should().Be(StormXmlElementType.Const);
        constElement.Id.Should().Be("$XalatathLife");
        constElement.EvaluateAsExpression.Should().BeFalse();
        constElement.Value.Should().Be("1330");
        constElement.GetValueAsInt32().Should().Be(1330);
    }

    [TestMethod]
    public void Parse_ConstWithExpression_ShouldReturnEvaluatedConstElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Base);

        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$XalatathDarkHeartRitualLinkDamage" value="35" />
            """));

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <const id="$XalatathDarkHeartRitualSecondaryLinkDamage" value="/($XalatathDarkHeartRitualLinkDamage 2)" evaluateAsExpression="1" />
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        StormXmlConstElement constElement = (StormXmlConstElement)element;

        constElement.ElementName.Should().Be("const");
        constElement.ElementType.Should().Be(StormXmlElementType.Const);
        constElement.Id.Should().Be("$XalatathDarkHeartRitualSecondaryLinkDamage");
        constElement.EvaluateAsExpression.Should().BeTrue();
        constElement.Value.Should().Be("17.5");
        constElement.GetValueAsDouble().Should().Be(17.5);
    }

    [TestMethod]
    public void Parse_HasIdAttribute_ShouldReturnId()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage id="StormBoltDamage"></CEffectDamage>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("EffectDamage");
        element.ElementType.Should().Be(StormXmlElementType.Class);
        element.Id.Should().Be("StormBoltDamage");
    }

    [TestMethod]
    public void Parse_HasNoIdAttribute_ShouldReturnNull()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage></CEffectDamage>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Id.Should().BeNull();
    }

    [TestMethod]
    public void Parse_HasParentAttribute_ShouldReturnParent()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage parent="StormSpell"></CEffectDamage>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Parent.Should().Be("StormSpell");
    }

    [TestMethod]
    public void Parse_HasNoParentAttribute_ShouldReturnNull()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage></CEffectDamage>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Parent.Should().BeNull();
    }

    [TestMethod]
    public void Parse_HasDefaultAttribute_ShouldReturnTrue()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil default="1"></CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.IsDefault.Should().BeTrue();
    }

    [TestMethod]
    public void Parse_HasDefaultAttribute_ShouldReturnFalse()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil default="0"></CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.IsDefault.Should().BeFalse();
    }

    [TestMethod]
    public void Parse_HasNoDefaultAttribute_ShouldReturnFalse()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil></CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.IsDefault.Should().BeFalse();
    }

    [TestMethod]
    public void Parse_HasOrderArrayProperty_ShouldReturnLineTexture()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
            </CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.Members["OrderArray"].ByIndex["0"]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine.dds");
    }

    [TestMethod]
    public void Parse_HasNumericalIndexes_ShouldReturnLineTexture()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine_0.dds" />
              <OrderArray LineTexture="Assets\Textures\Storm_WayPointLine_1.dds" />
              <OrderArray index="2" LineTexture="Assets\Textures\Storm_WayPointLine_2.dds" />
            </CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.Members["OrderArray"].ByIndex[0]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_0.dds");
        element.Members["OrderArray"].ByIndex[1]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_1.dds");
        element.Members["OrderArray"].ByIndex[2]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_2.dds");
    }

    [TestMethod]
    public void Parse_HasNamedArrayIndex_ShouldReturnValues()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil default="1">
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
            </CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.IsDefault.Should().BeTrue();

        element.Members["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("1");
        element.Members["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
    }

    [TestMethod]
    public void Parse_HasNumericalIndexInnerArray_ShouldReturnValues()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="KelThuzad">
              <HeroAbilArray Abil="KelThuzadDeathAndDecay" Button="KelThuzadDeathAndDecay">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
                </HeroAbilArray>
              <HeroAbilArray Abil="KelThuzadFrostNova" Button="KelThuzadFrostNova">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
              </HeroAbilArray>
              <HeroAbilArray Abil="KelThuzadChains" Button="KelThuzadChains">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
              </HeroAbilArray>
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Hero");
        element.Id.Should().Be("KelThuzad");
        element.IsDefault.Should().BeFalse();

        element.Members["HeroAbilArray"].ByIndex["0"]["Abil"].Value.ToString().Should().Be("KelThuzadDeathAndDecay");
        element.Members["HeroAbilArray"].ByIndex["0"]["Flags"].ByIndex["0"].Value.ToString().Should().Be("1");

        element.Members["HeroAbilArray"].ByIndex["2"]["Abil"].Value.ToString().Should().Be("KelThuzadChains");
        element.Members["HeroAbilArray"].ByIndex["2"]["Flags"].ByIndex["2"].Value.ToString().Should().Be("3");
    }

    [TestMethod]
    public void Parse_HasElementWithInnerElement_ShouldReturnValues()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CBehaviorTokenCounter id="KelThuzadMasterOfTheColdDarkToken">
              <Max value="30" />
              <ConditionalEvents Compare="GE" CompareValue="15">
              <Event Effect="KelThuzadMasterOfTheColdDarkTier1ModifyPlayer" />
              </ConditionalEvents>
            </CBehaviorTokenCounter>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("BehaviorTokenCounter");
        element.Id.Should().Be("KelThuzadMasterOfTheColdDarkToken");
        element.IsDefault.Should().BeFalse();

        element.Members["ConditionalEvents"].ByIndex["0"]["Compare"].Value.ToString().Should().Be("GE");
        element.Members["ConditionalEvents"].ByIndex["0"]["CompareValue"].Value.ToString().Should().Be("15");
        element.Members["ConditionalEvents"].ByIndex["0"]["Event"].ByMember["Effect"].Value.ToString().Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
    }

    [TestMethod]
    public void Parse_HasAbilityModificationArray_ShouldReturnValues()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CTalent id="AnubarakMasteryEpicenterBurrowCharge">
              <AbilityModificationArray Compare="GE" CompareValue="15">
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
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Id.Should().Be("AnubarakMasteryEpicenterBurrowCharge");
        element.IsDefault.Should().BeFalse();

        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Field"].Value.ToString().Should().Be("AreaArray[0].Radius");
        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Value"].Value.ToString().Should().Be("1.600000");
        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Field"].Value.ToString().Should().Be("Chance");
        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Value"].Value.ToString().Should().Be("1.000000");
        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Field"].Value.ToString().Should().Be("AnubarakBurrowChargeCursorSplat");
        element.Members["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Value"].Value.ToString().Should().Be("0.600000");
    }

    [TestMethod]
    public void Parse_HasProcessingInstructions_ShouldReturnValues()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CVoiceLine default="1" id="StormVoiceLineCommonBase">
              <?token id="heroid" type="CHeroLink" value="Bogus"?>
              <Hero value="##heroid##" />
              <TileTexture value="Assets\Textures\Storm_UI_Voice_##heroid##.dds" />
            </CVoiceLine>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Id.Should().Be("StormVoiceLineCommonBase");
        element.IsDefault.Should().BeTrue();

        element.Members["heroid"].Value.GetString().Should().Be("Bogus");
    }

    [TestMethod]
    public void Parse_RepeatedNonArrayElement_MergesOverEarlierElement()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbilEffectTarget id="TestAbil">
              <?token id="amount" type="CFixed" value="2"?>
              <Cost>
                <Charge Link="TestCharge" />
                <Amount value="1" />
              </Cost>
              <Cost>
                <Amount value="##amount##" />
              </Cost>
            </CAbilEffectTarget>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlArray cost = result[0].Members["Cost"];

        cost.IsArray.Should().BeFalse();
        cost.ByMember["Charge"].ByMember["Link"].Value.RawValue.Should().Be("TestCharge");
        cost.ByMember["Amount"].Value.RawValue.Should().Be("##amount##");
        cost.ByMember["Amount"].Value.GetString().Should().Be("2");
    }

    [TestMethod]
    public void Parse_ValueAttributeIsAssetReference_ShouldReturnAssetTextValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@UI_Portrait_Alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().Be(@"Assets\Textures\ui_targetportrait_hero_alarak.dds");
    }

    [TestMethod]
    public void Parse_NonValueAttributeIsAssetReference_ShouldReturnAssetTextValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_WayPointLine", @"Assets\Textures\Storm_WayPointLine.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbil>
              <OrderArray index="0" LineTexture="@UI_WayPointLine" />
            </CAbil>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["OrderArray"].ByIndex["0"]["LineTexture"].Value.RawValue.Should().Be(@"Assets\Textures\Storm_WayPointLine.dds");
    }

    [TestMethod]
    public void Parse_RootElementAttributeIsAssetReference_ShouldReturnAssetTextValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Model_Alarak", @"Assets\Units\Heroes\Alarak\Alarak.m3");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CModel id="Alarak" Model="@UI_Model_Alarak" />
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Id.Should().Be("Alarak");
        result[0].Members["Model"].Value.RawValue.Should().Be(@"Assets\Units\Heroes\Alarak\Alarak.m3");
    }

    [TestMethod]
    public void Parse_AssetReferenceNotFound_ShouldReturnEmptyValue()
    {
        // arrange
        StormXmlFileParser stormXmlFileParser = new(new LayeredDataStorage());

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@UI_Portrait_Alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlValue value = result[0].Members["Portrait"].Value;
        value.HasValue.Should().BeTrue();
        value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AssetReferenceWithDifferentCasing_ShouldReturnEmptyValue()
    {
        // arrange - asset ids are case-sensitive
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@ui_portrait_alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AssetReferenceIsOnlyAtSign_ShouldReturnEmptyValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AssetReferenceAssetTextValueIsEmpty_ShouldReturnEmptyValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", string.Empty);

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@UI_Portrait_Alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_AssetReferenceInMultipleLayers_ShouldReturnCustomAssetTextValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", "base.dds");
        AddAssetText(layeredDataStorage, StormModType.Map, "UI_Portrait_Alarak", "map.dds");
        AddAssetText(layeredDataStorage, StormModType.Custom, "UI_Portrait_Alarak", "custom.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Alarak">
              <Portrait value="@UI_Portrait_Alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().Be("custom.dds");
    }

    [TestMethod]
    public void Parse_AssetTextValueContainsConstant_ShouldNotEvaluateConstant()
    {
        // arrange - the asset text value is used as is, it is not evaluated as a $ constant
        LayeredDataStorage layeredDataStorage = new();

        layeredDataStorage.GetDataStorage(StormModType.Base).AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AlarakLife" value="1330" />
            """));
        AddAssetText(layeredDataStorage, StormModType.Base, "AlarakLifeAsset", "$AlarakLife");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CUnit id="HeroAlarak">
              <LifeMax value="@AlarakLifeAsset" />
            </CUnit>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["LifeMax"].Value.RawValue.Should().Be("$AlarakLife");
    }

    [TestMethod]
    public void Parse_AssetReferenceContainsConstant_ShouldLookUpAssetNotConstant()
    {
        // arrange - a value starting with '@' is always an asset reference, even if it contains a '$'
        LayeredDataStorage layeredDataStorage = new();

        layeredDataStorage.GetDataStorage(StormModType.Base).AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$AlarakLife" value="1330" />
            """));

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CUnit id="HeroAlarak">
              <LifeMax value="@$AlarakLife" />
            </CUnit>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["LifeMax"].Value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("Alarak@Portrait")]
    [DataRow(" @UI_Portrait_Alarak")]
    [DataRow("UI_Portrait_Alarak@")]
    public void Parse_AtSignNotAtStartOfValue_ShouldReturnValueAsIs(string attributeValue)
    {
        // arrange - only a leading '@' makes it an asset reference
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            $"""
            <CHero id="Alarak">
              <Portrait value="{attributeValue}" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Members["Portrait"].Value.RawValue.Should().Be(attributeValue);
    }

    [TestMethod]
    public void Parse_ConstElementValueIsAssetReference_ShouldReturnAssetTextValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "UI_Portrait_Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <const id="$AlarakPortrait" value="@UI_Portrait_Alarak" />
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlConstElement constElement = (StormXmlConstElement)result[0];
        constElement.Id.Should().Be("$AlarakPortrait");
        constElement.Value.Should().Be(@"Assets\Textures\ui_targetportrait_hero_alarak.dds");
    }

    [TestMethod]
    public void Parse_IdAttributeStartsWithAtSign_ShouldReturnIdAsIs()
    {
        // arrange - the id attribute is never an asset reference
        LayeredDataStorage layeredDataStorage = new();

        AddAssetText(layeredDataStorage, StormModType.Base, "Alarak", @"Assets\Textures\ui_targetportrait_hero_alarak.dds");

        StormXmlFileParser stormXmlFileParser = new(layeredDataStorage);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="@Alarak">
              <Portrait value="@Alarak" />
            </CHero>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result[0].Id.Should().Be("@Alarak");
        result[0].Members["Portrait"].Value.RawValue.Should().Be(@"Assets\Textures\ui_targetportrait_hero_alarak.dds");
    }

    // adds the asset text directly to the text cache of the given layer
    private static void AddAssetText(LayeredDataStorage layeredDataStorage, StormModType stormModType, string id, string value)
    {
        layeredDataStorage.GetDataStorage(stormModType).TextCache.AssetTextsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }
}
