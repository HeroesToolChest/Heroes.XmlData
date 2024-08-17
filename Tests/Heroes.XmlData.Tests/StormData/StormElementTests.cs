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
        stormElement.GetElementDataAt("default").Value.Should().Be("1");
        stormElement.GetElementDataAt("name").Value.Should().Be("Abil/Name/##id##");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0".AsSpan()).GetElementDataAt("color").GetElementDataAt("0").Value.Should().Be("255,0,255,0");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").GetElementDataAt("0").Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").Value.Should().Be("0");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").Value.Should().Be("1");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").Value.Should().Be("1");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("AllowMovement".AsSpan()).Value.Should().Be("1");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").Value.Should().Be("0");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").Value.Should().Be("1");
        stormElement.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").GetElementDataAt("0").Value.Should().Be("Spell");
        stormElement.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").Value.Should().Be("1");
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
        stormElement.GetElementDataAt("default").Value.Should().Be("1");
        stormElement.GetElementDataAt("name").Value.Should().Be("Abil/Name/##id##");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("color").GetElementDataAt("0").Value.Should().Be("255,0,255,0");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").GetElementDataAt("0").Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").Value.Should().Be("0");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").Value.Should().Be("1");
        stormElement.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").Value.Should().Be("1");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("AllowMovement").Value.Should().Be("1");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").Value.Should().Be("0");
        stormElement.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").Value.Should().Be("1");
        stormElement.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").GetElementDataAt("0").Value.Should().Be("Spell");
        stormElement.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").Value.Should().Be("1");

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
        stormElement.GetElementDataAt("ResponseFlags").GetElementDataAt("Acquire").Value.Should().Be("1");
        stormElement.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").Value.Should().Be("SelfHealing");
        stormElement.GetElementDataAt("ImpactLocation").Value.Should().Be("TargetUnitOrPoint");
        stormElement.GetElementDataAt("ImpactLocation").GetElementDataAt("History").Value.Should().Be("Damage");
        stormElement.GetElementDataAt("DamageModifierSource").Value.Should().Be("Caster");
        stormElement.GetElementDataAt("DamageModifierSource").HasValue.Should().BeTrue();
        stormElement.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").Value.Should().Be("MinionDamage");
        stormElement.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").HasValue.Should().BeTrue();
        stormElement.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").GetElementDataAt("Validator").GetElementDataAt("0").Value.Should().Be("TargetMinion");
        stormElement.GetElementDataAt("MultiplicativeModifierArray").GetElementDataAt("MuradinStormboltSledgehammer").GetElementDataAt("Modifier").GetElementDataAt("0").Value.Should().Be("2.5");
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
        List<StormElementData> stormElementData = stormElement.GetAllElementData().ToList();

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
        bool result = stormElement.TryGetElementDataAt("amount", out StormElementData? stormElementData);
        bool resultAsSpan = stormElement.TryGetElementDataAt("amount".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
        stormElementData!.GetAllElementData().ToList().Should().BeEmpty();
        stormElementDataAsSpan!.GetAllElementData().ToList().Should().BeEmpty();
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
        bool result = stormElement.TryGetElementDataAt("amount", out StormElementData? stormElementData);
        bool resultAsSpan = stormElement.TryGetElementDataAt("amount".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
        stormElementData.Should().BeNull();
        stormElementDataAsSpan.Should().BeNull();
    }
}
