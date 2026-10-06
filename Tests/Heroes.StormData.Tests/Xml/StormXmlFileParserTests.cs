namespace Heroes.StormData.Tests.Xml;

[TestClass]
public class StormXmlFileParserTests
{
    [TestMethod]
    public void Parse_GeneralElements_ReturnsItems()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
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
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        result.Should().HaveCount(4);
        result[0].ElementName.Should().Be("Marker");
        result[1].ElementName.Should().Be("Cost");
        result[2].ElementName.Should().Be("Abil");
        result[2].IsDefault.Should().BeTrue();
        result[3].ElementName.Should().Be("Button");
        result[3].Id.Should().Be("JainaConeOfCold");
        result[3].Parent.Should().Be("StormButtonParent");
    }

    [TestMethod]
    public void Parse_ConstNoExpression_ShouldReturnConstElement()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <const id="$XalatathLife" value="1330" />
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("const");
        element.ElementType.Should().Be(StormXmlElementType.Const);
        element.Id.Should().Be("$XalatathLife");
        element.Data.Value.Value.RawValue.Should().Be("1330");
    }

    [TestMethod]
    public void Parse_HasIdAttribute_ShouldReturnId()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CEffectDamage id="StormBoltDamage"></CEffectDamage>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CEffectDamage></CEffectDamage>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CEffectDamage parent="StormSpell"></CEffectDamage>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CEffectDamage></CEffectDamage>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil default="1"></CAbil>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil default="0"></CAbil>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil></CAbil>
            </Catalog>
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
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil>
                <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
              </CAbil>
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.Data["OrderArray"].ByIndex["0"]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine.dds");
    }

    [TestMethod]
    public void Parse_HasNumericalIndexes_ShouldReturnLineTexture()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil>
                <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine_0.dds" />
                <OrderArray LineTexture="Assets\Textures\Storm_WayPointLine_1.dds" />
                <OrderArray index="2" LineTexture="Assets\Textures\Storm_WayPointLine_2.dds" />
              </CAbil>
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.Data["OrderArray"].ByIndex[0]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_0.dds");
        element.Data["OrderArray"].ByIndex[1]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_1.dds");
        element.Data["OrderArray"].ByIndex[2]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_2.dds");
    }

    [TestMethod]
    public void Parse_HasNamedArrayIndex_ShouldReturnValues()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CAbil default="1">
                <SharedFlags index="DisableWhileDead" value="1" />
                <SharedFlags index="AllowQuickCastCustomization" value="1" />
              </CAbil>
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Abil");
        element.IsDefault.Should().BeTrue();

        element.Data["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("1");
        element.Data["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
    }

    [TestMethod]
    public void Parse_HasNumericalIndexInnerArray_ShouldReturnValues()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
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
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("Hero");
        element.Id.Should().Be("KelThuzad");
        element.IsDefault.Should().BeFalse();

        element.Data["HeroAbilArray"].ByIndex["0"]["Abil"].Value.ToString().Should().Be("KelThuzadDeathAndDecay");
        element.Data["HeroAbilArray"].ByIndex["0"]["Flags"].ByIndex["0"].Value.ToString().Should().Be("1");

        element.Data["HeroAbilArray"].ByIndex["2"]["Abil"].Value.ToString().Should().Be("KelThuzadChains");
        element.Data["HeroAbilArray"].ByIndex["2"]["Flags"].ByIndex["2"].Value.ToString().Should().Be("3");
    }

    [TestMethod]
    public void Parse_HasElementWithInnerElement_ShouldReturnValues()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
              <CBehaviorTokenCounter id="KelThuzadMasterOfTheColdDarkToken">
                <Max value="30" />
                <ConditionalEvents Compare="GE" CompareValue="15">
                <Event Effect="KelThuzadMasterOfTheColdDarkTier1ModifyPlayer" />
                </ConditionalEvents>
              </CBehaviorTokenCounter>
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.ElementName.Should().Be("BehaviorTokenCounter");
        element.Id.Should().Be("KelThuzadMasterOfTheColdDarkToken");
        element.IsDefault.Should().BeFalse();

        element.Data["ConditionalEvents"].ByIndex["0"]["Compare"].Value.ToString().Should().Be("GE");
        element.Data["ConditionalEvents"].ByIndex["0"]["CompareValue"].Value.ToString().Should().Be("15");
        element.Data["ConditionalEvents"].ByIndex["0"]["Event"].ByProperty["Effect"].Value.ToString().Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
    }

    [TestMethod]
    public void Parse_HasAbilityModificationArray_ShouldReturnValues()
    {
        // arrange
        ElementCache elementCache = new();
        StormXmlFileParser stormXmlFileParser = new(elementCache);

        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
            """
            <Catalog>
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
            </Catalog>
            """);

        // act
        List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

        // assert
        StormXmlElement element = result[0];
        element.Id.Should().Be("AnubarakMasteryEpicenterBurrowCharge");
        element.IsDefault.Should().BeFalse();

        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Field"].Value.ToString().Should().Be("AreaArray[0].Radius");
        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Value"].Value.ToString().Should().Be("1.600000");
        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Field"].Value.ToString().Should().Be("Chance");
        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Value"].Value.ToString().Should().Be("1.000000");
        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Field"].Value.ToString().Should().Be("AnubarakBurrowChargeCursorSplat");
        element.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Value"].Value.ToString().Should().Be("0.600000");
    }

    //[TestMethod]
    //public void Parse_NonIdChildIntoBase_ShouldMergeProperties()
    //{
    //    // arrange
    //    ElementCache elementCache = new();
    //    StormXmlFileParser stormXmlFileParser = new(elementCache);

    //    using XmlReader reader = XmlReaderHelper.CreateXmlReader(
    //        """
    //        <Catalog>
    //          <CAbil default="1">
    //            <Name value="Abil/Name/##id##" />
    //            <TechPlayer value="Upkeep" />
    //            <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
    //            <OrderArray>
    //              <Color value="255,0,255,0" />
    //              <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
    //              <LineTexture value="Assets\Textures\WayPointLine.dds" />
    //            </OrderArray>
    //            <SharedFlags index="DisableWhileDead" value="1" />
    //            <SharedFlags index="AllowQuickCastCustomization" value="1" />
    //            <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
    //          </CAbil>
    //          <CAbilEffectInstant default="1">
    //            <CmdButtonArray index="Execute" AutoQueueId="Spell">
    //             <Flags index="Continuous" value="1" />
    //            </CmdButtonArray>
    //            <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
    //            <Flags index="AllowMovement" value="1" />
    //            <Flags index="WaitToSpend" value="0" />
    //            <Flags index="ValidateButtonState" value="1" />
    //            <SharedFlags index="DisableWhileDead" value="0" />
    //          </CAbilEffectInstant>
    //        </Catalog>
    //        """);

    //    // act
    //    List<StormXmlElement> result = [.. stormXmlFileParser.Parse(reader, StormFileHelper.CreateStormFile())];

    //    // assert
    //    StormXmlElement element = result[0];
    //    element.ElementName.Should().Be("CAbilEffectInstant");
    //    element.Data["Name"].Value.ToString().Should().Be("Abil/Name/##id##");
    //    element.Data["OrderArray"].ByIndex["0"]["Color"].Value.ToString().Should().Be("255,0,255,0");
    //    element.Data["OrderArray"].ByIndex["0"]["Model"].Value.ToString().Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
    //    element.Data["OrderArray"].ByIndex["0"]["LineTexture"].Value.ToString().Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
    //    element.Data["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("0");
    //    element.Data["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
    //    element.Data["SharedFlags"].ByIndex["TargetCursorVisibleInBlackMask"].Value.ToString().Should().Be("1");
    //    element.Data["Flags"].ByIndex["AllowMovement"].Value.ToString().Should().Be("1");
    //    element.Data["Flags"].ByIndex["WaitToSpend"].Value.ToString().Should().Be("0");
    //    element.Data["Flags"].ByIndex["ValidateButtonState"].Value.ToString().Should().Be("1");
    //    element.Data["CmdButtonArray"].ByIndex["Execute"]["AutoQueueId"].Value.ToString().Should().Be("Spell");
    //    element.Data["CmdButtonArray"].ByIndex["Execute"]["Flags"].ByIndex["Continuous"].Value.ToString().Should().Be("1");
    //}
}
