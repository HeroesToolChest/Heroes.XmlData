//using Heroes.StormLoader.StormData.Xml;

//namespace Heroes.StormLoader.StormData.StormXml.Tests;

//[TestClass]
//public class StormXmlElementTests
//{
//    [TestMethod]
//    public void Create_HasIdAttribute_ShouldReturnId()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CEffectDamage id="StormBoltDamage"></CEffectDamage>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.ElementName.Should().Be("CEffectDamage");
//        stormXmlElement.Id.Should().Be("StormBoltDamage");
//    }

//    [TestMethod]
//    public void Create_HasNoIdAttribute_ShouldReturnNull()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CEffectDamage></CEffectDamage>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.Id.Should().BeNull();
//    }

//    [TestMethod]
//    public void Create_HasParentAttribute_ShouldReturnParent()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CEffectDamage parent="StormSpell"></CEffectDamage>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.Parent.Should().Be("StormSpell");
//    }

//    [TestMethod]
//    public void Create_HasNoParentAttribute_ShouldReturnNull()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CEffectDamage></CEffectDamage>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.Parent.Should().BeNull();
//    }

//    [TestMethod]
//    public void Create_HasDefaultAttribute_ShouldReturnTrue()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil default="1"></CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.IsDefault.Should().BeTrue();
//    }

//    [TestMethod]
//    public void Create_HasDefaultAttribute_ShouldReturnFalse()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil default="0"></CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.IsDefault.Should().BeFalse();
//    }

//    [TestMethod]
//    public void Create_HasNoDefaultAttribute_ShouldReturnFalse()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil></CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.IsDefault.Should().BeFalse();
//    }

//    [TestMethod]
//    public void Create_HasOrderArrayProperty_ShouldReturnLineTexture()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil>
//              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
//            </CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.ElementName.Should().Be("CAbil");
//        stormXmlElement.Data["OrderArray"].ByIndex["0"]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine.dds");
//    }

//    [TestMethod]
//    public void Create_HasNumericalIndexes_ShouldReturnLineTexture()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil>
//              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine_0.dds" />
//              <OrderArray LineTexture="Assets\Textures\Storm_WayPointLine_1.dds" />
//              <OrderArray index="2" LineTexture="Assets\Textures\Storm_WayPointLine_2.dds" />
//            </CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.Data["OrderArray"].ByIndex[0]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_0.dds");
//        stormXmlElement.Data["OrderArray"].ByIndex[1]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_1.dds");
//        stormXmlElement.Data["OrderArray"].ByIndex[2]["LineTexture"].Value.ToString().Should().Be(@"Assets\Textures\Storm_WayPointLine_2.dds");
//    }

//    [TestMethod]
//    public void Create_HasNamedArrayIndex_ShouldReturnValues()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil default="1">
//              <SharedFlags index="DisableWhileDead" value="1" />
//              <SharedFlags index="AllowQuickCastCustomization" value="1" />
//            </CAbil>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.ElementName.Should().Be("CAbil");
//        stormXmlElement.IsDefault.Should().BeTrue();

//        stormXmlElement.Data["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("1");
//        stormXmlElement.Data["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
//    }

//    [TestMethod]
//    public void Create_HasNumericalIndexInnerArray_ShouldReturnValues()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CHero id="KelThuzad">
//              <HeroAbilArray Abil="KelThuzadDeathAndDecay" Button="KelThuzadDeathAndDecay">
//                <Flags index="0" value="1" />
//                <Flags value="2" />
//                <Flags value="3" />
//              </HeroAbilArray>
//              <HeroAbilArray Abil="KelThuzadFrostNova" Button="KelThuzadFrostNova">
//                <Flags index="0" value="1" />
//                <Flags value="2" />
//                <Flags value="3" />
//              </HeroAbilArray>
//              <HeroAbilArray Abil="KelThuzadChains" Button="KelThuzadChains">
//                <Flags index="0" value="1" />
//                <Flags value="2" />
//                <Flags value="3" />
//              </HeroAbilArray>
//            </CHero>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.ElementName.Should().Be("CHero");
//        stormXmlElement.Id.Should().Be("KelThuzad");
//        stormXmlElement.IsDefault.Should().BeFalse();

//        stormXmlElement.Data["HeroAbilArray"].ByIndex["0"]["Abil"].Value.ToString().Should().Be("KelThuzadDeathAndDecay");
//        stormXmlElement.Data["HeroAbilArray"].ByIndex["0"]["Flags"].ByIndex["0"].Value.ToString().Should().Be("1");

//        stormXmlElement.Data["HeroAbilArray"].ByIndex["2"]["Abil"].Value.ToString().Should().Be("KelThuzadChains");
//        stormXmlElement.Data["HeroAbilArray"].ByIndex["2"]["Flags"].ByIndex["2"].Value.ToString().Should().Be("3");
//    }

//    [TestMethod]
//    public void Create_HasElementWithInnerElement_ShouldReturnValues()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CBehaviorTokenCounter id="KelThuzadMasterOfTheColdDarkToken">
//              <Max value="30" />
//              <ConditionalEvents Compare="GE" CompareValue="15">
//                <Event Effect="KelThuzadMasterOfTheColdDarkTier1ModifyPlayer" />
//              </ConditionalEvents>
//            </CBehaviorTokenCounter>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.ElementName.Should().Be("CBehaviorTokenCounter");
//        stormXmlElement.Id.Should().Be("KelThuzadMasterOfTheColdDarkToken");
//        stormXmlElement.IsDefault.Should().BeFalse();

//        stormXmlElement.Data["ConditionalEvents"].ByIndex["0"]["Compare"].Value.ToString().Should().Be("GE");
//        stormXmlElement.Data["ConditionalEvents"].ByIndex["0"]["CompareValue"].Value.ToString().Should().Be("15");
//        stormXmlElement.Data["ConditionalEvents"].ByIndex["0"]["Event"].ByProperty["Effect"].Value.ToString().Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
//    }

//    [TestMethod]
//    public void Create_HasAbilityModificationArray_ShouldReturnValues()
//    {
//        // arrange
//        using XmlReader reader = XmlReaderHelper.CreateXmlReader(
//            """
//            <CTalent id="AnubarakMasteryEpicenterBurrowCharge">
//              <AbilityModificationArray Compare="GE" CompareValue="15">
//                <Modifications>
//                  <Field value="AreaArray[0].Radius" />
//                  <Value value="1.600000" />
//                </Modifications>
//                <Modifications>
//                  <Field value="Chance" />
//                  <Value value="1.000000" />
//                </Modifications>
//                <Modifications>
//                  <Field value="AnubarakBurrowChargeCursorSplat" />
//                  <Value value="0.600000" />
//                </Modifications>
//              </AbilityModificationArray>
//            </CTalent>
//            """);

//        // act
//        StormXmlElement stormXmlElement = StormXmlElement.Create(reader);

//        // assert
//        stormXmlElement.Id.Should().Be("AnubarakMasteryEpicenterBurrowCharge");
//        stormXmlElement.IsDefault.Should().BeFalse();

//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Field"].Value.ToString().Should().Be("AreaArray[0].Radius");
//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["0"]["Value"].Value.ToString().Should().Be("1.600000");
//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Field"].Value.ToString().Should().Be("Chance");
//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["1"]["Value"].Value.ToString().Should().Be("1.000000");
//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Field"].Value.ToString().Should().Be("AnubarakBurrowChargeCursorSplat");
//        stormXmlElement.Data["AbilityModificationArray"].ByIndex["0"]["Modifications"].ByIndex["2"]["Value"].Value.ToString().Should().Be("0.600000");
//    }

//    [TestMethod]
//    public void Merge_NonIdChildIntoBase_ShouldMergeProperties()
//    {
//        // arrange
//        using XmlReader reader1 = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbil default="1">
//              <Name value="Abil/Name/##id##" />
//              <TechPlayer value="Upkeep" />
//              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
//              <OrderArray>
//                <Color value="255,0,255,0" />
//                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
//                <LineTexture value="Assets\Textures\WayPointLine.dds" />
//              </OrderArray>
//              <SharedFlags index="DisableWhileDead" value="1" />
//              <SharedFlags index="AllowQuickCastCustomization" value="1" />
//              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
//            </CAbil>
//            """);

//        using XmlReader reader2 = XmlReaderHelper.CreateXmlReader(
//            """
//            <CAbilEffectInstant default="1">
//              <CmdButtonArray index="Execute" AutoQueueId="Spell">
//                <Flags index="Continuous" value="1" />
//              </CmdButtonArray>
//              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
//              <Flags index="AllowMovement" value="1" />
//              <Flags index="WaitToSpend" value="0" />
//              <Flags index="ValidateButtonState" value="1" />
//              <SharedFlags index="DisableWhileDead" value="0" />
//            </CAbilEffectInstant>
//            """);

//        StormXmlElement parentEement = StormXmlElement.Create(reader1);
//        StormXmlElement childElement1 = StormXmlElement.Create(reader2);

//        // act
//        parentEement.Merge(childElement1);

//        // assert
//        parentEement.ElementName.Should().Be("CAbilEffectInstant");
//        parentEement.Data["Name"].Value.ToString().Should().Be("Abil/Name/##id##");
//        parentEement.Data["OrderArray"].ByIndex["0"]["Color"].Value.ToString().Should().Be("255,0,255,0");
//        parentEement.Data["OrderArray"].ByIndex["0"]["Model"].Value.ToString().Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
//        parentEement.Data["OrderArray"].ByIndex["0"]["LineTexture"].Value.ToString().Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
//        parentEement.Data["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("0");
//        parentEement.Data["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
//        parentEement.Data["SharedFlags"].ByIndex["TargetCursorVisibleInBlackMask"].Value.ToString().Should().Be("1");
//        parentEement.Data["Flags"].ByIndex["AllowMovement"].Value.ToString().Should().Be("1");
//        parentEement.Data["Flags"].ByIndex["WaitToSpend"].Value.ToString().Should().Be("0");
//        parentEement.Data["Flags"].ByIndex["ValidateButtonState"].Value.ToString().Should().Be("1");
//        parentEement.Data["CmdButtonArray"].ByIndex["Execute"]["AutoQueueId"].Value.ToString().Should().Be("Spell");
//        parentEement.Data["CmdButtonArray"].ByIndex["Execute"]["Flags"].ByIndex["Continuous"].Value.ToString().Should().Be("1");
//    }
//}
