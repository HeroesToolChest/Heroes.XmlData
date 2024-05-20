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
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, "some\\other\\path"));

        // assert
        stormElement.DataValues.KeyValueDataPairs["default"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["name"].Value.Should().Be("Abil/Name/##id##");
        stormElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["color"].KeyValueDataPairs["0"].Value.Should().Be("255,0,255,0");
        stormElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Model"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["disableWhileDead"].Value.Should().Be("0");
        stormElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["AllowQuickCastCustomization"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["TargetCursorVisibleInBlackMask"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["AllowMovement"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["WaitToSpend"].Value.Should().Be("0");
        stormElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["ValidateButtonState"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["CmdButtonArray"].KeyValueDataPairs["Execute"].KeyValueDataPairs["AutoQueueId"].KeyValueDataPairs["0"].Value.Should().Be("Spell");
        stormElement.DataValues.KeyValueDataPairs["CmdButtonArray"].KeyValueDataPairs["Execute"].KeyValueDataPairs["Flags"].KeyValueDataPairs["Continuous"].Value.Should().Be("1");
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
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement1, "some\\other1\\path"));
        stormElement.AddValue(new StormXElementValuePath(mergingElement2, "some\\other2\\path"));
        stormElement.AddValue(new StormXElementValuePath(mergingElement3, "some\\other3\\path"));
        stormElement.AddValue(new StormXElementValuePath(mergingElement4, "some\\other4\\path"));

        // assert
        stormElement.DataValues.KeyValueDataPairs["ResponseFlags"].KeyValueDataPairs["Acquire"].Value.Should().Be("1");
        stormElement.DataValues.KeyValueDataPairs["LeechScoreArray"].KeyValueDataPairs["0"].Value.Should().Be("SelfHealing");
        stormElement.DataValues.KeyValueDataPairs["ImpactLocation"].Value.Should().Be("TargetUnitOrPoint");
        stormElement.DataValues.KeyValueDataPairs["ImpactLocation"].KeyValueDataPairs["History"].Value.Should().Be("Damage");
        stormElement.DataValues.KeyValueDataPairs["DamageModifierSource"].Value.Should().Be("Caster");
        stormElement.DataValues.KeyValueDataPairs["DamageModifierSource"].HasValue.Should().BeTrue();
        stormElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].Value.Should().Be("MinionDamage");
        stormElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].HasValue.Should().BeTrue();
        stormElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].KeyValueDataPairs["Validator"].KeyValueDataPairs["0"].Value.Should().Be("TargetMinion");
        stormElement.DataValues.KeyValueDataPairs["MultiplicativeModifierArray"].KeyValueDataPairs["MuradinStormboltSledgehammer"].KeyValueDataPairs["Modifier"].KeyValueDataPairs["0"].Value.Should().Be("2.5");
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
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, "some\\other\\path"));

        // assert
        stormElement.OriginalStormXElementValues.Should().HaveCount(2);

        stormElement.OriginalStormXElementValues[0].Value.Equals(element);
        stormElement.OriginalStormXElementValues[0].Path.Equals("some\\path");

        stormElement.OriginalStormXElementValues[1].Value.Equals(mergingElement);
        stormElement.OriginalStormXElementValues[1].Value.Equals("some\\other\\path");
    }

    [TestMethod]
    public void StormElement_HasIdAttribute_ReturnsId()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"" parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // assert
        stormElement.HasId.Should().BeTrue();
        stormElement.Id.Should().Be("StormBoltDamage");
    }

    [TestMethod]
    public void StormElement_HasNoIdAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // assert
        stormElement.HasId.Should().BeFalse();
        stormElement.Id.Should().BeNull();
    }

    [TestMethod]
    public void StormElement_HasParentAttribute_ReturnsParent()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"" parent=""StormSpell"">
  <Amount value=""110"" />
</CEffectDamage>
");

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // assert
        stormElement.HasParentId.Should().BeTrue();
        stormElement.ParentId.Should().Be("StormSpell");
    }

    [TestMethod]
    public void StormElement_HasNoParentAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, "some\\path"));

        // assert
        stormElement.HasParentId.Should().BeFalse();
        stormElement.ParentId.Should().BeNull();
    }
}
