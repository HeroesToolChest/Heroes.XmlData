using Heroes.XmlData.Tests;
using System.Configuration;

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
    public void GetDescendants_GetAllInnerElements_ReturnsAll()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CAbilEffectInstant default=""1"">
  <CmdButtonArray index=""Execute"" AutoQueueId=""Spell"">
    <Flags index=""Continuous"" value=""1"" />
  </CmdButtonArray>
</CAbilEffectInstant>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Damage value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

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
