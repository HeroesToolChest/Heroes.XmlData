using Heroes.XmlData.StormData;

namespace Heroes.XmlData.Tests.StormData;

[TestClass]
public class StormXElementTests
{
    [TestMethod]
    public void AddStormXElementValue_AddValue_DataValuesAreMerged()
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
        StormElement stormXElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormXElement.AddValue(new StormXElementValuePath(mergingElement, "some\\other\\path"));

        // assert
        stormXElement.DataValues.KeyValueDataPairs["default"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["name"].Value.Should().Be("Abil/Name/##id##");
        stormXElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["color"].KeyValueDataPairs["0"].Value.Should().Be("255,0,255,0");
        stormXElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Model"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormXElement.DataValues.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormXElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["disableWhileDead"].Value.Should().Be("0");
        stormXElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["AllowQuickCastCustomization"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["TargetCursorVisibleInBlackMask"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["AllowMovement"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["WaitToSpend"].Value.Should().Be("0");
        stormXElement.DataValues.KeyValueDataPairs["Flags"].KeyValueDataPairs["ValidateButtonState"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["CmdButtonArray"].KeyValueDataPairs["Execute"].KeyValueDataPairs["AutoQueueId"].Value.Should().Be("Spell");
        stormXElement.DataValues.KeyValueDataPairs["CmdButtonArray"].KeyValueDataPairs["Execute"].KeyValueDataPairs["Flags"].KeyValueDataPairs["Continuous"].Value.Should().Be("1");
    }

    [TestMethod]
    public void AddStormXElementValue_AddMultipleValuesWithIdThatHasParents_DataValuesAreMerged()
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
        StormElement stormXElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormXElement.AddValue(new StormXElementValuePath(mergingElement1, "some\\other1\\path"));
        stormXElement.AddValue(new StormXElementValuePath(mergingElement2, "some\\other2\\path"));
        stormXElement.AddValue(new StormXElementValuePath(mergingElement3, "some\\other3\\path"));
        stormXElement.AddValue(new StormXElementValuePath(mergingElement4, "some\\other4\\path"));

        // assert
        stormXElement.DataValues.KeyValueDataPairs["ResponseFlags"].KeyValueDataPairs["Acquire"].Value.Should().Be("1");
        stormXElement.DataValues.KeyValueDataPairs["ResponseFlags"].IsArray.Should().BeTrue();
        stormXElement.DataValues.KeyValueDataPairs["LeechScoreArray"].KeyValueDataPairs["0"].Value.Should().Be("SelfHealing");
        stormXElement.DataValues.KeyValueDataPairs["LeechScoreArray"].IsArray.Should().BeTrue();
        stormXElement.DataValues.KeyValueDataPairs["ImpactLocation"].Value.Should().Be("TargetUnitOrPoint");
        stormXElement.DataValues.KeyValueDataPairs["ImpactLocation"].KeyValueDataPairs["History"].Value.Should().Be("Damage");
        stormXElement.DataValues.KeyValueDataPairs["ImpactLocation"].IsArray.Should().BeFalse();
        stormXElement.DataValues.KeyValueDataPairs["DamageModifierSource"].Value.Should().Be("Caster");
        stormXElement.DataValues.KeyValueDataPairs["DamageModifierSource"].IsArray.Should().BeFalse();
        stormXElement.DataValues.KeyValueDataPairs["DamageModifierSource"].HasValue.Should().BeTrue();
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].Value.Should().Be("MinionDamage");
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].HasValue.Should().BeTrue();
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].KeyValueDataPairs["Validator"].Value.Should().Be("TargetMinion");
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].IsArray.Should().BeTrue();
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].IsArray.Should().BeFalse();
        stormXElement.DataValues.KeyValueDataPairs["AmountScoreArray"].KeyValueDataPairs["3"].KeyValueDataPairs["Validator"].IsArray.Should().BeFalse();
        stormXElement.DataValues.KeyValueDataPairs["MultiplicativeModifierArray"].KeyValueDataPairs["MuradinStormboltSledgehammer"].KeyValueDataPairs["Modifier"].Value.Should().Be("2.5");
    }

    [TestMethod]
    public void StormXElementValues_AddingElement_ReturnOriginalElements()
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
        StormElement stormXElement = new(new StormXElementValuePath(element, "some\\path"));

        // act
        stormXElement.AddValue(new StormXElementValuePath(mergingElement, "some\\other\\path"));

        // assert
        stormXElement.OriginalStormXElementValues.Should().HaveCount(2);

        stormXElement.OriginalStormXElementValues[0].Value.Equals(element);
        stormXElement.OriginalStormXElementValues[0].Path.Equals("some\\path");

        stormXElement.OriginalStormXElementValues[1].Value.Equals(mergingElement);
        stormXElement.OriginalStormXElementValues[1].Value.Equals("some\\other\\path");
    }
}
