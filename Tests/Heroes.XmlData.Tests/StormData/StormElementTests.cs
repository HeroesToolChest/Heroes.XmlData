using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementTests
{
    [TestMethod]
    public void AddValue_MergingSingleElement_DataValuesAreMerged()
    {
        XElement element = XElement.Parse(@"
<CAbil default=""1"">
  <Name value=""Abil/Name/##id##"" />
  <TechPlayer value=""Upkeep"" />
  <TargetMessage value=""Abil/TargetMessage/DefaultTargetMessage"" />
  <OrderArray>
    <Color value=""255,0,255,0"" />
    <Model value=""Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3"" />
    <LineTexture value=""Assets\Textures\WayPointLine.dds"" />
  </OrderArray>
  <SharedFlags index=""DisableWhileDead"" value=""1"" />
  <SharedFlags index=""AllowQuickCastCustomization"" value=""1"" />
  <SharedFlags index=""TargetCursorVisibleInBlackMask"" value=""1"" />
</CAbil>
");

        XElement mergingElement = XElement.Parse(@"
<CAbilEffectInstant default=""1"">
  <CmdButtonArray index=""Execute"" AutoQueueId=""Spell"">
    <Flags index=""Continuous"" value=""1"" />
  </CmdButtonArray>
  <OrderArray index=""0"" LineTexture=""Assets\Textures\Storm_WayPointLine.dds"" />
  <Flags index=""AllowMovement"" value=""1"" />
  <Flags index=""WaitToSpend"" value=""0"" />
  <Flags index=""ValidateButtonState"" value=""1"" />
  <SharedFlags index=""DisableWhileDead"" value=""0"" />
</CAbilEffectInstant>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.ElementType.Should().Be("CAbil");
        stormElement.GetXmlData("default").Value.Should().Be("1");
        stormElement.GetXmlData("name").Value.Should().Be("Abil/Name/##id##");
        stormElement.GetXmlData("OrderArray").GetXmlData("0".AsSpan()).GetXmlData("color").GetXmlData("0").Value.Should().Be("255,0,255,0");
        stormElement.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("Model").GetXmlData("0").Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.GetXmlData("SharedFlags").GetXmlData("disableWhileDead").Value.Should().Be("0");
        stormElement.GetXmlData("SharedFlags").GetXmlData("AllowQuickCastCustomization").Value.Should().Be("1");
        stormElement.GetXmlData("SharedFlags").GetXmlData("TargetCursorVisibleInBlackMask").Value.Should().Be("1");
        stormElement.GetXmlData("Flags").GetXmlData("AllowMovement".AsSpan()).Value.Should().Be("1");
        stormElement.GetXmlData("Flags").GetXmlData("WaitToSpend").Value.Should().Be("0");
        stormElement.GetXmlData("Flags").GetXmlData("ValidateButtonState").Value.Should().Be("1");
        stormElement.GetXmlData("CmdButtonArray").GetXmlData("Execute").GetXmlData("AutoQueueId").GetXmlData("0").Value.Should().Be("Spell");
        stormElement.GetXmlData("CmdButtonArray").GetXmlData("Execute").GetXmlData("Flags").GetXmlData("Continuous").Value.Should().Be("1");
    }

    [TestMethod]
    public void AddValue_MergingOtherStormElement_DataValuesAreMerged()
    {
        XElement element = XElement.Parse(@"
<CAbil default=""1"">
  <Name value=""Abil/Name/##id##"" />
  <TechPlayer value=""Upkeep"" />
  <TargetMessage value=""Abil/TargetMessage/DefaultTargetMessage"" />
  <OrderArray>
    <Color value=""255,0,255,0"" />
    <Model value=""Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3"" />
    <LineTexture value=""Assets\Textures\WayPointLine.dds"" />
  </OrderArray>
  <SharedFlags index=""DisableWhileDead"" value=""1"" />
  <SharedFlags index=""AllowQuickCastCustomization"" value=""1"" />
  <SharedFlags index=""TargetCursorVisibleInBlackMask"" value=""1"" />
</CAbil>
");

        XElement mergingElement = XElement.Parse(@"
<CAbilEffectInstant default=""1"">
  <CmdButtonArray index=""Execute"" AutoQueueId=""Spell"">
    <Flags index=""Continuous"" value=""1"" />
  </CmdButtonArray>
  <OrderArray index=""0"" LineTexture=""Assets\Textures\Storm_WayPointLine.dds"" />
  <Flags index=""AllowMovement"" value=""1"" />
  <Flags index=""WaitToSpend"" value=""0"" />
  <Flags index=""ValidateButtonState"" value=""1"" />
  <SharedFlags index=""DisableWhileDead"" value=""0"" />
</CAbilEffectInstant>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElement otherStormElement = new(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\path\\two")));

        // act
        stormElement.AddValue(otherStormElement);

        // assert
        stormElement.ElementType.Should().Be("CAbil");
        stormElement.GetXmlData("default").Value.Should().Be("1");
        stormElement.GetXmlData("name").Value.Should().Be("Abil/Name/##id##");
        stormElement.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("color").GetXmlData("0").Value.Should().Be("255,0,255,0");
        stormElement.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("Model").GetXmlData("0").Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.GetXmlData("SharedFlags").GetXmlData("disableWhileDead").Value.Should().Be("0");
        stormElement.GetXmlData("SharedFlags").GetXmlData("AllowQuickCastCustomization").Value.Should().Be("1");
        stormElement.GetXmlData("SharedFlags").GetXmlData("TargetCursorVisibleInBlackMask").Value.Should().Be("1");
        stormElement.GetXmlData("Flags").GetXmlData("AllowMovement").Value.Should().Be("1");
        stormElement.GetXmlData("Flags").GetXmlData("WaitToSpend").Value.Should().Be("0");
        stormElement.GetXmlData("Flags").GetXmlData("ValidateButtonState").Value.Should().Be("1");
        stormElement.GetXmlData("CmdButtonArray").GetXmlData("Execute").GetXmlData("AutoQueueId").GetXmlData("0").Value.Should().Be("Spell");
        stormElement.GetXmlData("CmdButtonArray").GetXmlData("Execute").GetXmlData("Flags").GetXmlData("Continuous").Value.Should().Be("1");

        stormElement.OriginalStormXElementValues.Count.Should().Be(2);
    }

    [TestMethod]
    public void AddValue_AddMultipleValuesWithIdThatHasParents_DataValuesAreMerged()
    {
        XElement element = XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
  <Marker Link=""Effect/##id##"" />
  <DamageModifierSource Value=""Unknown"" />
</CEffect>
");

        XElement mergingElement1 = XElement.Parse(@"
<CEffectDamage default=""1"">
  <Visibility value=""Snapshot"" />
  <MaxCount value=""4294967295"" />
  <MinCountError value=""CantFindEnoughTargets"" />
  <LaunchLocation Value=""SourceUnit"" />
  <ImpactLocation Value=""TargetUnitOrPoint"" />
  <SearchFlags index=""SameCliff"" value=""1"" />
  <Kind value=""Basic"" />
  <KindSplash value=""Basic"" />
</CEffectDamage>
");

        XElement mergingElement2 = XElement.Parse(@"
<CEffectDamage default=""1"" id=""StormDamage"">
  <ValidatorArray value=""TargetNotInvulnerable"" />
  <ResponseFlags index=""Acquire"" value=""1"" />
  <LeechScoreArray Value=""SelfHealing"" />
  <Visibility value=""Visible"" />
  <ImpactLocation History=""Damage"" />
  <AmountScoreArray Validator=""IsHeroAndNotVehicleAndNotHallucination"" Value=""HeroDamage"" />
  <AmountScoreArray Validator=""IsStructureAndNotDestructible"" Value=""StructureDamage"" />
  <AmountScoreArray Validator=""IsStructureAndNotDestructible"" Value=""SiegeDamage"" />
  <AmountScoreArray Validator=""TargetMinion"" Value=""MinionDamage"" />
  <AmountScoreArray Validator=""TargetMinion"" Value=""SiegeDamage"" />
  <AmountScoreArray Validator=""TargetIsMercLaner"" Value=""SiegeDamage"" />
  <AmountScoreArray Validator=""TargetIsMercDefender"" Value=""CreepDamage"" />
  <AmountScoreArray Validator=""IsSummonedUnit"" Value=""SummonDamage"" />
  <AmountScoreArray Validator=""TargetIsSummonedandNotHeroic"" Value=""SiegeDamage"" />
  <SplashHistory value=""Damage"" />
  <DamageModifierSource Value=""Caster"" />
  <LeechRecipientArray />
</CEffectDamage>
");

        XElement mergingElement3 = XElement.Parse(@"
<CEffectDamage default=""1"" id=""StormSpell"" parent=""StormDamage"">
  <CritValidatorArray value=""CritAliasSpellPower"" />
  <Kind value=""Ability"" />
  <KindSplash value=""Ability"" />
</CEffectDamage>
");

        XElement mergingElement4 = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"" parent=""StormSpell"">
  <Amount value=""110"" />
  <CritValidatorArray index=""0"" value=""CritAliasSpellPowerOrMuradinSledgehammerCombine"" />
  <FlatModifierArray index=""MuradinStormboltPerfectStorm"" Accumulator=""MuradinStormboltPerfectStormAccumulator"" />
  <MultiplicativeModifierArray index=""MuradinStormboltSledgehammer"" Validator=""HasMuradinStormhammerSledgehammerAndTargetNotHeroic"" Modifier=""2.5"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement1, TestHelpers.GetStormPath("some\\other1\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement2, TestHelpers.GetStormPath("some\\other2\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement3, TestHelpers.GetStormPath("some\\other3\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement4, TestHelpers.GetStormPath("some\\other4\\path")));

        // assert
        stormElement.GetXmlData("ResponseFlags").GetXmlData("Acquire").Value.Should().Be("1");
        stormElement.GetXmlData("LeechScoreArray").GetXmlData("0").Value.Should().Be("SelfHealing");
        stormElement.GetXmlData("ImpactLocation").Value.Should().Be("TargetUnitOrPoint");
        stormElement.GetXmlData("ImpactLocation").GetXmlData("History").Value.Should().Be("Damage");
        stormElement.GetXmlData("DamageModifierSource").Value.Should().Be("Caster");
        stormElement.GetXmlData("DamageModifierSource").HasValue.Should().BeTrue();
        stormElement.GetXmlData("AmountScoreArray").GetXmlData("3").Value.Should().Be("MinionDamage");
        stormElement.GetXmlData("AmountScoreArray").GetXmlData("3").HasValue.Should().BeTrue();
        stormElement.GetXmlData("AmountScoreArray").GetXmlData("3").GetXmlData("Validator").GetXmlData("0").Value.Should().Be("TargetMinion");
        stormElement.GetXmlData("MultiplicativeModifierArray").GetXmlData("MuradinStormboltSledgehammer").GetXmlData("Modifier").GetXmlData("0").Value.Should().Be("2.5");
    }

    [TestMethod]
    public void AddValue_AddingElement_ReturnOriginalElements()
    {
        XElement element = XElement.Parse(@"
<CAbil default=""1"">
  <Name value=""Abil/Name/##id##"" />
  <TechPlayer value=""Upkeep"" />
  <TargetMessage value=""Abil/TargetMessage/DefaultTargetMessage"" />
  <OrderArray>
    <Color value=""255,0,255,0"" />
    <Model value=""Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3"" />
    <LineTexture value=""Assets\Textures\WayPointLine.dds"" />
  </OrderArray>
  <SharedFlags index=""DisableWhileDead"" value=""1"" />
  <SharedFlags index=""AllowQuickCastCustomization"" value=""1"" />
  <SharedFlags index=""TargetCursorVisibleInBlackMask"" value=""1"" />
</CAbil>
");

        XElement mergingElement = XElement.Parse(@"
<CAbilEffectInstant default=""1"">
  <CmdButtonArray index=""Execute"" AutoQueueId=""Spell"">
    <Flags index=""Continuous"" value=""1"" />
  </CmdButtonArray>
  <OrderArray index=""0"" LineTexture=""Assets\Textures\Storm_WayPointLine.dds"" />
  <Flags index=""AllowMovement"" value=""1"" />
  <Flags index=""WaitToSpend"" value=""0"" />
  <Flags index=""ValidateButtonState"" value=""1"" />
  <SharedFlags index=""DisableWhileDead"" value=""0"" />
</CAbilEffectInstant>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.OriginalStormXElementValues.Should().HaveCount(2);

        stormElement.OriginalStormXElementValues[0].Value.Equals(element);
        stormElement.OriginalStormXElementValues[0].StormPath.Path.Equals("some\\path");

        stormElement.OriginalStormXElementValues[1].Value.Equals(mergingElement);
        stormElement.OriginalStormXElementValues[1].Value.Equals("some\\other\\path");
    }

    [TestMethod]
    public void Id_HasIdAttribute_ReturnsId()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"" parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
        XElement element = XElement.Parse(@"
<CEffectDamage parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"" parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormElement.ParentId;
        bool result = stormElement.HasParentId;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }

    [TestMethod]
    public void GetXmlData_GetAllInnerData_ReturnsAll()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        List<StormElementData> stormElementData = stormElement.GetXmlData().ToList();

        // assert
        stormElementData.Should().HaveCount(2);
    }

    [TestMethod]
    public void TryGetXmlData_HasData_ReturnsStormElementData()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.TryGetXmlData("amount", out StormElementData? stormElementData);
        bool resultAsSpan = stormElement.TryGetXmlData("amount".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
        stormElementData!.GetXmlData().ToList().Should().BeEmpty();
        stormElementDataAsSpan!.GetXmlData().ToList().Should().BeEmpty();
    }

    [TestMethod]
    public void TryGetXmlData_HasNoData_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Damage value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.TryGetXmlData("amount", out StormElementData? stormElementData);
        bool resultAsSpan = stormElement.TryGetXmlData("amount".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
        stormElementData.Should().BeNull();
        stormElementDataAsSpan.Should().BeNull();
    }
}
