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
        stormElement.ElementType.Should().Be("CAbilEffectInstant");
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
        stormElement.ElementType.Should().Be("CAbilEffectInstant");
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

        stormElement.OriginalXElements.Count.Should().Be(2);
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
    public void AddValue_MergeCostElement_MergedAsNonIndexed()
    {
        XElement element1 = XElement.Parse(@"
<CWeapon default=""1"">
  <Cost>
    <Cooldown Link=""Weapon/##id##""/>
  </Cost>
</CWeapon>
");

        XElement element2 = XElement.Parse(@"
<CWeaponLegacy default=""1"" id=""StormHeroWeapon"">
</CWeaponLegacy>
");

        XElement element3 = XElement.Parse(@"
<CWeaponLegacy default=""1"" id=""StormHeroFastWeapon"" parent=""StormHeroWeapon"">
</CWeaponLegacy>
");

        XElement element4 = XElement.Parse(@"
<CWeaponLegacy id=""TracerHeroWeapon"" parent=""StormHeroFastWeapon"">
  <Cost>
    <Vital index=""Energy"" value=""2"" />
  </Cost>
</CWeaponLegacy>
");
        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element3, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element4, TestHelpers.GetStormPath("some\\other\\path")));

        // act

        // assert
        stormElement.ElementType.Should().Be("CWeaponLegacy");
        stormElement.DataValues["Cost"]["0"]["Vital"]["Energy"].RawValue.Should().Be("2");
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
        stormElement.OriginalXElements.Should().HaveCount(2);

        stormElement.OriginalXElements[0].Value.Equals(element);
        stormElement.OriginalXElements[0].StormPath.Path.Equals("some\\path");

        stormElement.OriginalXElements[1].Value.Equals(mergingElement);
        stormElement.OriginalXElements[1].Value.Equals("some\\other\\path");
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
        List<StormElementData> stormElementData = [.. stormElement.GetElements()];

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

    [TestMethod]
    public void ContainsIndex_HasIndex_ReturnsTrue()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Amount value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.DataValues.ContainsIndex("amount");
        bool resultAsSpan = stormElement.DataValues.ContainsIndex("amount".AsSpan());

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsIndex_DoesNotHaveIndex_ReturnsFalse()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CEffectDamage id=""StormBoltDamage"">
  <Damage value=""110"" />
</CEffectDamage>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        bool result = stormElement.DataValues.ContainsIndex("amount");
        bool resultAsSpan = stormElement.DataValues.ContainsIndex("amount".AsSpan());

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
    }

    [TestMethod]
    public void AddValue_RemoveAttributeInArray_ArrayElementAreRemoved()
    {
        XElement element = XElement.Parse(@"
  <CUnit default=""1"" id=""StormHeroMountedCustom"" parent=""StormHero"">
    <CardLayouts>
      <!--total index 22-->
      <LayoutButtons Face=""LockedHeroicAbility"" Type=""Passive"" Requirements=""UltimateNotUnlocked"" Slot=""Heroic"" />
      <LayoutButtons Face=""Hearthstone"" Type=""AbilCmd"" AbilCmd=""Hearthstone,Execute"" Slot=""Hearth"" />
      <LayoutButtons Face=""Attack"" Type=""AbilCmd"" AbilCmd=""attack,Execute"" Slot=""Attack"" />
      <LayoutButtons Face=""AcquireMove"" Type=""AbilCmd"" AbilCmd=""move,AcquireMove"" Slot=""Attack"" />
      <LayoutButtons Face=""Stop"" Type=""AbilCmd"" AbilCmd=""HoldFire,Stop"" Slot=""Stop"" />
      <LayoutButtons Face=""MoveHoldPosition"" Type=""AbilCmd"" AbilCmd=""HoldFire,HoldFire"" Slot=""Hold"" />
      <LayoutButtons Face=""Cancel"" Type=""CancelTargetMode"" Slot=""Cancel"" />
      <LayoutButtons Face=""MoonwellDrink"" Type=""AbilCmd"" AbilCmd=""FountainDrink,Execute"" Slot=""Interact"" />
      <LayoutButtons Face=""MoonwellDrink"" Type=""AbilCmd"" AbilCmd=""FountainDrinkOnCooldown,Execute"" Slot=""Interact"" />
      <LayoutButtons Face=""CaptureMacGuffin"" Type=""AbilCmd"" AbilCmd=""CaptureMacGuffin,Execute"" Slot=""Interact"" />
      <LayoutButtons Face=""SmartCommandUnitInteraction"" Type=""AbilCmd"" AbilCmd=""SmartCommandUnitInteraction,Execute"" Slot=""Interact"" />
      <LayoutButtons Face=""MountCabooseSmartCommandUnitInteraction"" Type=""AbilCmd"" AbilCmd=""MountCabooseSmartCommandUnitInteraction,Execute"" Slot=""Interact"" />
      <LayoutButtons Face=""Move"" Type=""AbilCmd"" AbilCmd=""move,Move"" Slot=""Cancel"" />
      <LayoutButtons Face=""Tease"" Type=""AbilCmd"" AbilCmd=""stop,Tease"" Slot=""Taunt"" />
      <LayoutButtons Face=""Dance"" Type=""AbilCmd"" AbilCmd=""stop,Dance"" Slot=""Dance"" />
      <LayoutButtons Face=""MapMechanicAbilityInstant"" Type=""AbilCmd"" AbilCmd=""MapMechanicAbilityInstant,Execute"" Slot=""MapMechanic"" />
      <LayoutButtons Face=""MapMechanicAbility"" Type=""AbilCmd"" AbilCmd=""MapMechanicAbilityTarget,Execute"" Slot=""MapMechanic"" />
      <LayoutButtons Face=""MapMechanicAbility2"" Type=""AbilCmd"" AbilCmd=""MapMechanicAbilityTarget2,Execute"" Slot=""MapMechanic"" />
      <LayoutButtons Face=""LockedMapMechanicAbility"" Type=""Passive"" Slot=""MapMechanic"" />
      <LayoutButtons Face=""MapMechanicAbilityParent"" Type=""AbilCmd"" AbilCmd=""MapMechanicAbilityParent,Execute"" Slot=""MapMechanic"" />
      <LayoutButtons Face=""LootSpray"" Type=""AbilCmd"" AbilCmd=""LootSpray,Execute"" Slot=""Spray"" />
      <LayoutButtons Face=""LootYellVoiceLine"" Type=""AbilCmd"" AbilCmd=""LootYellVoiceLine,Execute"" Slot=""Voice"" />
      <LayoutButtons Face=""Move"" Type=""AbilCmd"" AbilCmd=""move,Move"" Slot=""ForceMove"" />
    </CardLayouts>
  </CUnit>
");

        XElement mergingElement = XElement.Parse(@"
  <CUnit id=""HeroGall"" parent=""StormHeroMountedCustom"">
    <CardLayouts index=""0"">
      <LayoutButtons index=""1"" Face=""Dance"" Type=""AbilCmd"" AbilCmd=""stop,Dance"" Slot=""Dance"" />
      <LayoutButtons index=""2"" Face=""LootSpray"" Type=""AbilCmd"" AbilCmd=""LootSpray,Execute"" Slot=""Spray"" />
      <LayoutButtons index=""3"" Face=""LootYellVoiceLine"" Type=""AbilCmd"" AbilCmd=""LootYellVoiceLine,Execute"" Slot=""Voice"" />
      <LayoutButtons index=""4"" Face=""GallTwistingNetherActivated"" Type=""AbilCmd"" AbilCmd=""GallTwistingNetherActivated,Execute"" Slot=""Heroic"" />
      <LayoutButtons index=""5"" Face=""GallTwistingNether"" Type=""AbilCmd"" AbilCmd=""GallShiftingNether,Execute"" Slot=""Heroic"" />
      <LayoutButtons index=""6"" Face=""GallTwistingNether"" Type=""AbilCmd"" AbilCmd=""GallTwistingNether,Execute"" Slot=""Heroic"" />
      <LayoutButtons index=""7"" Face=""GallShadowboltVolley"" Type=""AbilCmd"" AbilCmd=""GallShadowBoltVolleyMoltenBlockDummy,Execute"" Slot=""Heroic"" />
      <LayoutButtons index=""8"" Face=""GallShadowflame"" Type=""AbilCmd"" AbilCmd=""GallShadowflame,Execute"" Slot=""Ability1"" />
      <LayoutButtons index=""9"" Face=""GallDoubleBackAbility"" Type=""AbilCmd"" AbilCmd=""GallDoubleBack,Execute"" Slot=""Ability2"" />
      <LayoutButtons index=""10"" Face=""GallDreadOrb"" Type=""AbilCmd"" AbilCmd=""GallDreadOrb,Execute"" Slot=""Ability2"" />
      <LayoutButtons index=""11"" Face=""GallRunicBlast"" Type=""AbilCmd"" AbilCmd=""GallRunicBlast,Execute"" Slot=""Ability3"" />
      <LayoutButtons index=""12"" Face=""GallShoveHotbar"" Type=""AbilCmd"" AbilCmd=""GallShove,Execute"" Slot=""Mount"" />
      <LayoutButtons index=""13"" Face=""GallOgreRageActivated"" Type=""AbilCmd"" AbilCmd=""GallOgreRage,Execute"" Slot=""Trait"" />
      <LayoutButtons index=""14"" Face=""GallOgreRagePassive"" Type=""Passive"" AbilCmd="""" Slot=""Trait"" />
      <LayoutButtons index=""15"" Face=""GallShadowboltVolley"" Type=""AbilCmd"" AbilCmd=""GallShadowboltVolley,Execute"" Slot=""Heroic"" />
      <LayoutButtons index=""16"" removed=""1"" />
      <LayoutButtons index=""17"" removed=""1"" />
      <LayoutButtons index=""18"" removed=""1"" />
      <LayoutButtons index=""19"" removed=""1"" />
      <LayoutButtons index=""20"" removed=""1"" />
      <LayoutButtons index=""21"" removed=""1"" />
      <LayoutButtons index=""22"" removed=""1"" />
    </CardLayouts>
  </CUnit>
");

        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.DataValues["CardLayouts"]["0"]["LayoutButtons"].ElementDataCount.Should().Be(16);
    }
}
