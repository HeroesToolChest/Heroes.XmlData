using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementTests
{
    [TestMethod]
    public void AddValue_MergingSingleElement_DataValuesAreMerged()
    {
        // arrange
        using XmlObject element = XmlParser.Parse(
            """
            <CAbil default="1">
              <Name value="Abil/Name/##id##" />
              <TechPlayer value="Upkeep" />
              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
              <OrderArray>
                <Color value="255,0,255,0" />
                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
                <LineTexture value="Assets\Textures\WayPointLine.dds" />
              </OrderArray>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
            </CAbil>
            """);

        using XmlObject mergingElement = XmlParser.Parse(
            """
            <CAbilEffectInstant default="1">
              <CmdButtonArray index="Execute" AutoQueueId="Spell">
                <Flags index="Continuous" value="1" />
              </CmdButtonArray>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
              <Flags index="AllowMovement" value="1" />
              <Flags index="WaitToSpend" value="0" />
              <Flags index="ValidateButtonState" value="1" />
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbilEffectInstant>
            """);

        StormElement stormElement = new(new StormXmlValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXmlValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.ElementType.Should().Be("CAbil");
        stormElement.DataValues.GetElementDataAt("default").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/##id##");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0".AsSpan()).GetElementDataAt("color").GetElementDataAt("0").RawValue.Should().Be("255,0,255,0");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").GetElementDataAt("0").RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("AllowMovement".AsSpan()).RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").GetElementDataAt("0").RawValue.Should().Be("Spell");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").RawValue.Should().Be("1");
    }

    [TestMethod]
    public void AddValue_MergingOtherStormElement_DataValuesAreMerged()
    {
        // arrange
        using XmlObject element = XmlParser.Parse(
            """
            <CAbil default="1">
              <Name value="Abil/Name/##id##" />
              <TechPlayer value="Upkeep" />
              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
              <OrderArray>
                <Color value="255,0,255,0" />
                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
                <LineTexture value="Assets\Textures\WayPointLine.dds" />
              </OrderArray>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
            </CAbil>
            """);

        using XmlObject mergingElement = XmlParser.Parse(
            """
            <CAbilEffectInstant default="1">
              <CmdButtonArray index="Execute" AutoQueueId="Spell">
                <Flags index="Continuous" value="1" />
              </CmdButtonArray>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
              <Flags index="AllowMovement" value="1" />
              <Flags index="WaitToSpend" value="0" />
              <Flags index="ValidateButtonState" value="1" />
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbilEffectInstant>
            """);

        StormElement stormElement = new(new StormXmlValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElement otherStormElement = new(new StormXmlValuePath(mergingElement, TestHelpers.GetStormPath("some\\path\\two")));

        // act
        stormElement.AddValue(otherStormElement);

        // assert
        stormElement.ElementType.Should().Be("CAbil");
        stormElement.DataValues.GetElementDataAt("default").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/##id##");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("color").GetElementDataAt("0").RawValue.Should().Be("255,0,255,0");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").GetElementDataAt("0").RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("AllowMovement").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").GetElementDataAt("0").RawValue.Should().Be("Spell");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").RawValue.Should().Be("1");

        stormElement.OriginalXmlElements.Count.Should().Be(2);
    }

    [TestMethod]
    public void AddValue_AddMultipleValuesWithIdThatHasParents_DataValuesAreMerged()
    {
        // arrange
        using XmlObject element = XmlParser.Parse(
            """
            <CEffect default="1">
              <Chance value="1" />
              <Marker Link="Effect/##id##" />
              <DamageModifierSource Value="Unknown" />
            </CEffect>
            """);

        using XmlObject mergingElement1 = XmlParser.Parse(
            """
            <CEffectDamage default="1">
              <Visibility value="Snapshot" />
              <MaxCount value="4294967295" />
              <MinCountError value="CantFindEnoughTargets" />
              <LaunchLocation Value="SourceUnit" />
              <ImpactLocation Value="TargetUnitOrPoint" />
              <SearchFlags index="SameCliff" value="1" />
              <Kind value="Basic" />
              <KindSplash value="Basic" />
            </CEffectDamage>
            """);

        using XmlObject mergingElement2 = XmlParser.Parse(
            """
            <CEffectDamage default="1" id="StormDamage">
                <ValidatorArray value="TargetNotInvulnerable" />
              <ResponseFlags index="Acquire" value="1" />
              <LeechScoreArray Value="SelfHealing" />
              <Visibility value="Visible" />
              <ImpactLocation History="Damage" />
              <AmountScoreArray Validator="IsHeroAndNotVehicleAndNotHallucination" Value="HeroDamage" />
              <AmountScoreArray Validator="IsStructureAndNotDestructible" Value="StructureDamage" />
              <AmountScoreArray Validator="IsStructureAndNotDestructible" Value="SiegeDamage" />
              <AmountScoreArray Validator="TargetMinion" Value="MinionDamage" />
              <AmountScoreArray Validator="TargetMinion" Value="SiegeDamage" />
              <AmountScoreArray Validator="TargetIsMercLaner" Value="SiegeDamage" />
              <AmountScoreArray Validator="TargetIsMercDefender" Value="CreepDamage" />
              <AmountScoreArray Validator="IsSummonedUnit" Value="SummonDamage" />
              <AmountScoreArray Validator="TargetIsSummonedandNotHeroic" Value="SiegeDamage" />
              <SplashHistory value="Damage" />
              <DamageModifierSource Value="Caster" />
              <LeechRecipientArray />
            </CEffectDamage>
            """);

        using XmlObject mergingElement3 = XmlParser.Parse(
            """
            <CEffectDamage default="1" id="StormSpell" parent="StormDamage">
              <CritValidatorArray value="CritAliasSpellPower" />
              <Kind value="Ability" />
              <KindSplash value="Ability" />
            </CEffectDamage>
            """);

        using XmlObject mergingElement4 = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage" parent="StormSpell">
              <Amount value="110" />
              <CritValidatorArray index="0" value="CritAliasSpellPowerOrMuradinSledgehammerCombine" />
              <FlatModifierArray index="MuradinStormboltPerfectStorm" Accumulator="MuradinStormboltPerfectStormAccumulator" />
              <MultiplicativeModifierArray index="MuradinStormboltSledgehammer" Validator="HasMuradinStormhammerSledgehammerAndTargetNotHeroic" Modifier="2.5" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXmlValuePath(mergingElement1, TestHelpers.GetStormPath("some\\other1\\path")));
        stormElement.AddValue(new StormXmlValuePath(mergingElement2, TestHelpers.GetStormPath("some\\other2\\path")));
        stormElement.AddValue(new StormXmlValuePath(mergingElement3, TestHelpers.GetStormPath("some\\other3\\path")));
        stormElement.AddValue(new StormXmlValuePath(mergingElement4, TestHelpers.GetStormPath("some\\other4\\path")));

        // assert
        stormElement.DataValues.GetElementDataAt("ResponseFlags").GetElementDataAt("Acquire").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").RawValue.Should().Be("SelfHealing");
        stormElement.DataValues.GetElementDataAt("ImpactLocation").RawValue.Should().Be("TargetUnitOrPoint");
        stormElement.DataValues.GetElementDataAt("ImpactLocation").GetElementDataAt("History").RawValue.Should().Be("Damage");
        stormElement.DataValues.GetElementDataAt("DamageModifierSource").RawValue.Should().Be("Caster");
        stormElement.DataValues.GetElementDataAt("DamageModifierSource").HasValue.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").RawValue.Should().Be("MinionDamage");
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").HasValue.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").GetElementDataAt("Validator").GetElementDataAt("0").RawValue.Should().Be("TargetMinion");
        stormElement.DataValues.GetElementDataAt("MultiplicativeModifierArray").GetElementDataAt("MuradinStormboltSledgehammer").GetElementDataAt("Modifier").GetElementDataAt("0").RawValue.Should().Be("2.5");
    }

    [TestMethod]
    public void AddValue_AddingElement_ReturnOriginalElements()
    {
        // arrange
        using XmlObject element = XmlParser.Parse(
            """
            <CAbil default="1">
              <Name value="Abil/Name/##id##" />
              <TechPlayer value="Upkeep" />
              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
              <OrderArray>
                <Color value="255,0,255,0" />
                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
                <LineTexture value="Assets\Textures\WayPointLine.dds" />
              </OrderArray>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
            </CAbil>
            """);

        using XmlObject mergingElement = XmlParser.Parse(
            """
            <CAbilEffectInstant default="1">
              <CmdButtonArray index="Execute" AutoQueueId="Spell">
                <Flags index="Continuous" value="1" />
              </CmdButtonArray>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
              <Flags index="AllowMovement" value="1" />
              <Flags index="WaitToSpend" value="0" />
              <Flags index="ValidateButtonState" value="1" />
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbilEffectInstant>
            """);

        StormElement stormElement = new(new StormXmlValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXmlValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.OriginalXmlElements.Should().HaveCount(2);

        stormElement.OriginalXmlElements[0].Value.Equals(element);
        stormElement.OriginalXmlElements[0].StormPath.Path.Equals("some\\path");

        stormElement.OriginalXmlElements[1].Value.Equals(mergingElement);
        stormElement.OriginalXmlElements[1].Value.Equals("some\\other\\path");
    }

    [TestMethod]
    public void Id_HasIdAttribute_ReturnsId()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage" parent="StormSpell">
              <Amount value="110" />
            </CEffectDamage>
            """);
        XmlNode node = xmlObject.Root;
        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        xmlObject.Dispose();
        // act
        string? resultValue = stormElement.Id;
        bool result = stormElement.HasId;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("StormBoltDamage");
    }

    [TestMethod]
    public void Id_HasNoId_ReturnsNull()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage parent="StormSpell">
              <Amount value="110" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormElement.Id;
        bool result = stormElement.HasId;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }

    [TestMethod]
    public void ParentId_HasParentAttribute_ReturnsParentId()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage" parent="StormSpell">
              <Amount value="110" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormElement.ParentId;
        bool result = stormElement.HasParentId;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("StormSpell");
    }

    [TestMethod]
    public void ParentId_HasNoParentAttribute_ReturnsNull()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage">
              <Amount value="110" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormElement.ParentId;
        bool result = stormElement.HasParentId;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }

    [TestMethod]
    public void GetDescendants_GetAllInnerElements_ReturnsAll()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CAbilEffectInstant default="1">
                <CmdButtonArray index="Execute" AutoQueueId="Spell">
                    <Flags index="Continuous" value="1" />
                </CmdButtonArray>
            </CAbilEffectInstant>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        List<StormElementData> stormElementData = stormElement.GetElements().ToList();

        // assert
        stormElementData.Should().HaveCount(3)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Field.Should().Be("default");
                },
                second =>
                {
                    second.Field.Should().Be("CmdButtonArray[Execute].AutoQueueId[0]");
                },
                third =>
                {
                    third.Field.Should().Be("CmdButtonArray[Execute].Flags[Continuous]");
                });
    }

    [TestMethod]
    public void TryGetElementDataAt_HasData_ReturnsStormElementData()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage">
                <Amount value="110" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.DataValues.TryGetElementDataAt("amount", out StormElementData? _);
        bool resultAsSpan = stormElement.DataValues.TryGetElementDataAt("amount".AsSpan(), out StormElementData? _);

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
    }

    [TestMethod]
    public void TryGetElementDataAt_HasNoData_ReturnsNull()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="StormBoltDamage">
                <Damage value="110" />
            </CEffectDamage>
            """);

        StormElement stormElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.DataValues.TryGetElementDataAt("amount", out StormElementData? stormElementData);
        bool resultAsSpan = stormElement.DataValues.TryGetElementDataAt("amount".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
        stormElementData.Should().BeNull();
        stormElementDataAsSpan.Should().BeNull();
    }
}
