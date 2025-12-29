using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementTests
{
    [TestMethod]
    public void AddValue_MergingSingleElement_DataValuesAreMerged()
    {
        SetElementsForMerginSingleElement(out XElement element, out XElement mergingElement);
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        AssertMergingSingleElement(stormElement);
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
        stormElement.DataValues.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/##id##");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("color").RawValue.Should().Be("255,0,255,0");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("AllowMovement").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").RawValue.Should().Be("Spell");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").RawValue.Should().Be("1");

        stormElement.OriginalXElements.Count.Should().Be(2);
        stormElement.IsDefault.Should().BeTrue();
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
        stormElement.DataValues.GetElementDataAt("ResponseFlags").GetElementDataAt("Acquire").IsIndexed.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").RawValue.Should().Be("SelfHealing");
        stormElement.DataValues.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").IsIndexed.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("ImpactLocation").RawValue.Should().Be("TargetUnitOrPoint");
        stormElement.DataValues.GetElementDataAt("ImpactLocation").GetElementDataAt("History").RawValue.Should().Be("Damage");
        stormElement.DataValues.GetElementDataAt("DamageModifierSource").RawValue.Should().Be("Caster");
        stormElement.DataValues.GetElementDataAt("DamageModifierSource").HasValue.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").RawValue.Should().Be("MinionDamage");
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").HasValue.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").GetElementDataAt("Validator").RawValue.Should().Be("TargetMinion");
        stormElement.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").IsIndexed.Should().BeTrue();
        stormElement.DataValues.GetElementDataAt("MultiplicativeModifierArray").GetElementDataAt("MuradinStormboltSledgehammer").GetElementDataAt("Modifier").RawValue.Should().Be("2.5");

        stormElement.DataValues.GetElementDataAt("SplashHistory").IsIndexed.Should().BeFalse();
        stormElement.DataValues.GetElementDataAt("ResponseFlags").IsIndexed.Should().BeFalse();

        stormElement.IsDefault.Should().BeFalse();
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

        // act
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element3, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element4, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.ElementType.Should().Be("CWeaponLegacy");
        stormElement.ParentId.Should().Be("StormHeroFastWeapon");
        stormElement.DataValues["Cost"]["Vital"]["Energy"].RawValue.Should().Be("2");
        stormElement.DataValues["Cost"].Value.GetString().Should().BeEmpty();
        stormElement.DataValues["Cost"]["Cooldown"]["Link"].Value.GetString().Should().Be("Weapon/TracerHeroWeapon");
        stormElement.DataValues["Cost"]["Cooldown"].Value.GetString().Should().Be("Weapon/TracerHeroWeapon");
    }

    [TestMethod]
    public void AddValue_MergeCostElement_ReturnsTimeUse()
    {
        XElement element1 = XElement.Parse(@"
  <CAbilBehavior default=""1"">
    <Cost>
      <Charge Location=""Unit""/>
      <Cooldown Location=""Unit""/>
    </Cost>
  </CAbilBehavior>
");

        XElement element2 = XElement.Parse(@"
  <CAbilBehavior id=""LucioCrossfade"">
    <Cost>
      <Cooldown TimeUse=""0.5"" />
    </Cost>
  </CAbilBehavior>
");

        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.DataValues["Cost"]["Cooldown"]["TimeUse"].RawValue.Should().Be("0.5");
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

        stormElement.IsDefault.Should().BeTrue();
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
        stormElementData.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Field.Should().Be("CmdButtonArray[Execute].AutoQueueId");
                },
                second =>
                {
                    second.Field.Should().Be("CmdButtonArray[Execute].Flags[Continuous]");
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

    [TestMethod]
    public void AddValue_DefaultElementWithTwoAddedNonDefaults_ReturnsCorrectReplacementValue()
    {
        XElement element1 = XElement.Parse(
            """
            <CButton default="1">
              <Tooltip value="Button/Tooltip/##id##"/>
            </CButton>
            """);

        XElement element2 = XElement.Parse(
            """
            <CButton default="1" id="StormButtonParent">
            </CButton>
            """);

        XElement element3 = XElement.Parse(
            """
            <CButton default="1" id="StormButtonParentTrait" parent="StormButtonParent"/>
            """);

        XElement element4 = XElement.Parse(
            """
            <CButton id="MuradinSecondWind" parent="StormButtonParentTrait">
            </CButton>
            """);

        XElement element5 = XElement.Parse(
            """
            <CButton id="MuradinSecondWindActivateable" parent="MuradinSecondWind">
            </CButton>
            """);
        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element3, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element4, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element5, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.DataValues["Tooltip"].Value.GetString().Should().Be("Button/Tooltip/MuradinSecondWind");
        stormElement.DataValues.ElementDataCount.Should().Be(3);
        stormElement.DefaultDataValues.ElementDataCount.Should().Be(2);
        stormElement.DefaultDataValues["id"].RawValue.Should().Be("MuradinSecondWind");
        stormElement.DefaultDataValues["parent"].RawValue.Should().Be("StormButtonParentTrait");

        stormElement.IsDefault.Should().BeFalse();
    }

    [TestMethod]
    public void AddValue_NoDefault_Returns()
    {
        XElement element1 = XElement.Parse(
            """
            <CUser id="EndOfMatchMapSpecificAward">
                <Instances Id="[Default]">
                    <Fixed Fixed="1">
                        <Field Id="Base"/>
                    </Fixed>
                    <Fixed Fixed="1">
                        <Field Id="Weight Modifier"/>
                    </Fixed>
                    <String String="false">
                        <Field Id="Gated by Base"/>
                    </String>
                    <String String="false">
                        <Field Id="Present as Ratio"/>
                    </String>
                </Instances>
                <Instances Id="Generic Instance"/>
                <Instances Id="Shriner">
                    <Fixed Fixed="2">
                        <Field Id="Base"/>
                    </Fixed>
                    <GameLink GameLink="EndOfMatchAwardMostDragonShrinesCapturedBoolean">
                        <Field Id="Score Value Boolean"/>
                    </GameLink>
                    <String String="09">
                        <Field Id="Award Badge Index"/>
                    </String>
                    <String String="true">
                        <Field Id="Gated by Base"/>
                    </String>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Shriner_Award Name">
                        <Field Id="Award Name"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Shriner_Description">
                        <Field Id="Description"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Shriner_Tooltip Text">
                        <Field Id="Tooltip Text"/>
                    </Text>
                </Instances>
                <Instances Id="Master of the Curse">
                    <Fixed Fixed="1498">
                        <Field Id="Base"/>
                    </Fixed>
                    <GameLink GameLink="EndOfMatchAwardMostCurseDamageDoneBoolean">
                        <Field Id="Score Value Boolean"/>
                    </GameLink>
                    <String String="16">
                        <Field Id="Award Badge Index"/>
                    </String>
                    <String String="true">
                        <Field Id="Present as Ratio"/>
                    </String>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Award Name">
                        <Field Id="Award Name"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Description">
                        <Field Id="Description"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Tooltip Text">
                        <Field Id="Tooltip Text"/>
                    </Text>
                </Instances>
            </CUser>
            """);

        XElement element2 = XElement.Parse(
            """
            <CUser id="EndOfMatchMapSpecificAward">
                <Instances Id="[Override]Generic Instance">
                    <Fixed Fixed="6">
                        <Field Id="Base"/>
                    </Fixed>
                    <GameLink GameLink="EndOfMatchAwardMostInterruptedCageUnlocksBoolean">
                        <Field Id="Score Value Boolean"/>
                    </GameLink>
                    <String String="true">
                        <Field Id="Gated by Base"/>
                    </String>
                    <String String="true">
                        <Field Id="Present as Ratio"/>
                    </String>
                    <String String="36">
                        <Field Id="Award Badge Index"/>
                    </String>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/[Override]Generic Instance_Award Name">
                        <Field Id="Award Name"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/[Override]Generic Instance_Description">
                        <Field Id="Description"/>
                    </Text>
                    <Text Text="UserData/EndOfMatchMapSpecificAward/[Override]Generic Instance_Tooltip Text">
                        <Field Id="Tooltip Text"/>
                    </Text>
                </Instances>
            </CUser>
            """);

        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.IsDefault.Should().BeFalse();
    }

    [TestMethod]
    public void AddValue_TooltipIdWithDefaultElement_ReturnsCorrectReplacementValue()
    {
        XElement element1 = XElement.Parse(
            """
            <CButton default="1">
              <Tooltip value="Button/Tooltip/##id##"/>
            </CButton>
            """);

        XElement element2 = XElement.Parse(
            """
            <CButton default="1" id="StormButtonParent">
            </CButton>
            """);

        XElement element3 = XElement.Parse(
            """
            <CButton default="1" id="StormButtonParentTrait" parent="StormButtonParent"/>
            """);

        XElement element4 = XElement.Parse(
            """
            <CButton id="MuradinSecondWind" parent="StormButtonParentTrait">
            </CButton>
            """);

        XElement element5 = XElement.Parse(
            """
            <CButton default="1" id="MuradinSecondWindActivateable" parent="MuradinSecondWind">
            </CButton>
            """);
        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element3, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element4, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element5, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.DataValues["Tooltip"].Value.GetString().Should().Be("Button/Tooltip/MuradinSecondWindActivateable");
    }

    [TestMethod]
    public void StormElement_Buttons_ShouldBeInArray()
    {
        XElement element = XElement.Parse(
            """
            <CBehaviorAbility id="NecromancerBoneArmor">
              <Buttons Face="NecromancerBoneArmor" Type="AbilCmd" AbilCmd="NecromancerBoneArmor,Execute" ShowValidator="DoesNotHaveNecromancerBoneArmorTalentsOrHasBoneArmorShadeTalent" />
              <Buttons Face="NecromancerBoneArmorAbilBacklash" Type="AbilCmd" AbilCmd="NecromancerBoneArmor,Execute" ShowValidator="HasNecromancerTalentBacklash" />
              <Buttons Face="NecromancerBoneArmorAbilShackler" Type="AbilCmd" AbilCmd="NecromancerBoneArmor,Execute" ShowValidator="HasNecromancerTalentShackler" />
            </CBehaviorAbility>
            """);

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // assert
        stormElement.DataValues["Buttons"].ElementDataCount.Should().Be(3);
        stormElement.DataValues["Buttons"]["0"]["Face"].RawValue.Should().Be("NecromancerBoneArmor");
        stormElement.DataValues["Buttons"]["1"]["Face"].RawValue.Should().Be("NecromancerBoneArmorAbilBacklash");
        stormElement.DataValues["Buttons"]["2"]["Face"].RawValue.Should().Be("NecromancerBoneArmorAbilShackler");
    }

    [TestMethod]
    public void StormElement_Cost_ShouldBeInArray()
    {
        XElement element = XElement.Parse(
            """
            <CEffectModifyUnit id="AzmodanHeroWeaponBattlebornTalentModifyCooldown">
              <Cost Abil="AzmodanSummonDemonWarrior,Execute" CooldownOperation="Add" CooldownTimeUse="-0.75" />
              <Cost Abil="AzmodanDemonLieutenant,Execute" CooldownOperation="Add" CooldownTimeUse="-1.5" />
            </CEffectModifyUnit>
            """);

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // assert
        stormElement.DataValues["Cost"].ElementDataCount.Should().Be(2);
        stormElement.DataValues["Cost"]["0"]["CooldownTimeUse"].RawValue.Should().Be("-0.75");
        stormElement.DataValues["Cost"]["1"]["CooldownTimeUse"].RawValue.Should().Be("-1.5");
    }

    [TestMethod]
    public void AddValue_ArrayWithNonInnerArrays_DataValuesAreMerged()
    {
        XElement element = XElement.Parse(@"
<CEffectEnumArea id=""ZaryaWeaponSplashTargetSearch"">
  <AreaArray Effect=""ZaryaWeaponSplashDamageSet"">
    <Radius value=""0.5"" />
    <RectangleWidth value=""1"" />
    <RectangleHeight value=""6.25"" />
  </AreaArray>
</CEffectEnumArea>
");

        XElement mergingElement = XElement.Parse(@"
<CEffectEnumArea id=""ZaryaWeaponToTheLimitSearch"" parent=""ZaryaWeaponSplashTargetSearch"">
  <AreaArray index=""0"" Effect=""ZaryaWeaponSplashDamageSet"">
    <RectangleWidth value=""1.35"" />
    <RectangleHeight value=""8.4375"" />
  </AreaArray>
  <AreaRelativeOffset Y=""5.4438"" />
</CEffectEnumArea>
");
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // assert
        stormElement.ElementType.Should().Be("CEffectEnumArea");
        stormElement.DataValues["AreaArray"]["0"]["RectangleWidth"].RawValue.Should().Be("1.35");
        stormElement.DataValues["AreaArray"]["0"]["RectangleHeight"].RawValue.Should().Be("8.4375");
        stormElement.DataValues["AreaArray"]["0"]["Radius"].RawValue.Should().Be("0.5");
    }

    [TestMethod]
    public void ToXElement_NoId_XElementShouldBeCorrect()
    {
        // arrange
        SetElementsForMerginSingleElement(out XElement element, out XElement mergingElement);
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement, TestHelpers.GetStormPath("some\\other\\path")));

        // act
        XElement xElement = stormElement.ToXElement();

        // assert
        AssertMergingSingleElement(new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path"))));
    }

    [TestMethod]
    public void ToXElement_HasIdWithParents_XElementShouldBeCorrect()
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
        stormElement.AddValue(new StormXElementValuePath(mergingElement1, TestHelpers.GetStormPath("some\\other1\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement2, TestHelpers.GetStormPath("some\\other2\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement3, TestHelpers.GetStormPath("some\\other3\\path")));
        stormElement.AddValue(new StormXElementValuePath(mergingElement4, TestHelpers.GetStormPath("some\\other4\\path")));

        // act
        XElement xElement = stormElement.ToXElement();

        // assert
        StormElement stormElementResult = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        stormElementResult.DataValues.GetElementDataAt("Marker").Value.GetString().Should().Be("Effect/StormBoltDamage");
        stormElementResult.DataValues.GetElementDataAt("ResponseFlags").GetElementDataAt("Acquire").RawValue.Should().Be("1");
        stormElementResult.DataValues.GetElementDataAt("ResponseFlags").GetElementDataAt("Acquire").IsIndexed.Should().BeTrue();
        stormElementResult.DataValues.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").RawValue.Should().Be("SelfHealing");
        stormElementResult.DataValues.GetElementDataAt("LeechScoreArray").GetElementDataAt("0").IsIndexed.Should().BeTrue();
        stormElementResult.DataValues.GetElementDataAt("ImpactLocation").GetElementDataAt("Value").RawValue.Should().Be("TargetUnitOrPoint");
        stormElementResult.DataValues.GetElementDataAt("ImpactLocation").RawValue.Should().Be("TargetUnitOrPoint");
        stormElementResult.DataValues.GetElementDataAt("ImpactLocation").GetElementDataAt("History").RawValue.Should().Be("Damage");
        stormElementResult.DataValues.GetElementDataAt("DamageModifierSource").RawValue.Should().Be("Caster");
        stormElementResult.DataValues.GetElementDataAt("DamageModifierSource").HasValue.Should().BeTrue();
        stormElementResult.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").RawValue.Should().Be("MinionDamage");
        stormElementResult.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").HasValue.Should().BeTrue();
        stormElementResult.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").GetElementDataAt("Validator").RawValue.Should().Be("TargetMinion");
        stormElementResult.DataValues.GetElementDataAt("AmountScoreArray").GetElementDataAt("3").IsIndexed.Should().BeTrue();
        stormElementResult.DataValues.GetElementDataAt("MultiplicativeModifierArray").GetElementDataAt("MuradinStormboltSledgehammer").GetElementDataAt("Modifier").RawValue.Should().Be("2.5");
        stormElementResult.DataValues["LeechRecipientArray"]["0"].Value.GetString().Should().BeEmpty();

        stormElementResult.DataValues.GetElementDataAt("SplashHistory").IsIndexed.Should().BeFalse();
        stormElementResult.DataValues.GetElementDataAt("ResponseFlags").IsIndexed.Should().BeFalse();
    }

    [TestMethod]
    public void ToXElement_MergeCostElement_XElementShouldBeCorrect()
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
        XElement xElement = stormElement.ToXElement();

        // assert
        StormElement stormElementResult = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));

        stormElementResult.Id.Should().Be("TracerHeroWeapon");
        stormElementResult.ElementType.Should().Be("CWeaponLegacy");
        stormElementResult.ParentId.Should().BeNull();
        stormElementResult.DataValues["Cost"]["Vital"]["Energy"].RawValue.Should().Be("2");
        stormElementResult.DataValues["Cost"].Value.GetString().Should().BeEmpty();
        stormElementResult.DataValues["Cost"]["Cooldown"]["Link"].Value.GetString().Should().Be("Weapon/TracerHeroWeapon");
        stormElementResult.DataValues["Cost"]["Cooldown"].Value.GetString().Should().Be("Weapon/TracerHeroWeapon");
    }

    [TestMethod]
    public void ToXElement_MergeCostElement2_XElementShouldBeCorrect()
    {
        XElement element1 = XElement.Parse(@"
  <CAbilBehavior default=""1"">
    <Cost>
      <Charge Location=""Unit""/>
      <Cooldown Location=""Unit""/>
    </Cost>
  </CAbilBehavior>
");

        XElement element2 = XElement.Parse(@"
  <CAbilBehavior id=""LucioCrossfade"">
    <Cost>
      <Cooldown TimeUse=""0.5"" />
    </Cost>
  </CAbilBehavior>
");

        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));

        // act
        XElement xElement = stormElement.ToXElement();

        // assert
        StormElement stormElementResult = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));

        stormElementResult.Id.Should().Be("LucioCrossfade");
        stormElementResult.ParentId.Should().BeNull();
        stormElementResult.DataValues["Cost"]["Cooldown"]["TimeUse"].RawValue.Should().Be("0.5");
        stormElementResult.DataValues["Cost"]["Charge"]["Location"].Value.GetString().Should().Be("Unit");
        stormElementResult.DataValues["Cost"]["Charge"].Value.GetString().Should().Be("Unit");
    }

    [TestMethod]
    public void ToXElement_HasProcessingInstructions_XElementShouldBeCorrect()
    {
        XElement element1 = XElement.Parse(@"
  <CVoiceLine default=""1"">
    <Name value=""VoiceLine/Name/##id##"" />
    <SortName value=""VoiceLine/SortName/##id##"" />
    <Description value=""VoiceLine/Description/##id##"" />
    <ReleaseDate>
      <Month value=""1"" />
      <Day value=""1"" />
      <Year value=""2014"" />
    </ReleaseDate>
    <AttributeId value=""TODO"" />
    <ProductId value=""11089"" />
    <LootChestRewardCutsceneFile value=""Cutscenes/UI_LootChest_Reward_BG.StormCutscene"" />
    <TileCutsceneFile value=""Cutscenes/UI_LootChest_Reward_BG.StormCutscene"" />
  </CVoiceLine>
");

        XElement element2 = XElement.Parse(@"
  <CVoiceLine default=""1"" id=""StormVoiceLineCommonBase"">
    <?token id=""heroid"" type=""CHeroLink"" value=""Bogus""?>
    <Hero value=""##heroid##"" />
    <TileTexture value=""Assets\Textures\Storm_UI_Voice_##heroid##.dds"" />
  </CVoiceLine>
");

        XElement element3 = XElement.Parse(@"
  <CVoiceLine default=""1"" id=""StormVoiceLine01Common"" parent=""StormVoiceLineCommonBase"">
    <Flags index=""FreePlay"" value=""1"" />
    <HyperlinkId value=""##heroid##VoiceLine01"" />
    <ProductId value=""0"" />
  </CVoiceLine>
");

        XElement element4 = XElement.Parse(@"
  <CVoiceLine default=""1"" id=""Abathur_VoiceLine01Common"" parent=""StormVoiceLine01Common"">
    <?token id=""heroid"" type=""CHeroLink"" value=""Abathur""?>
    <AttributeId value=""AB01"" />
    <Sound value=""AbathurHero_VoiceLineOne"" />
  </CVoiceLine>
");
        StormElement stormElement = new(new StormXElementValuePath(element1, TestHelpers.GetStormPath("some\\path")));
        stormElement.AddValue(new StormXElementValuePath(element2, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element3, TestHelpers.GetStormPath("some\\other\\path")));
        stormElement.AddValue(new StormXElementValuePath(element4, TestHelpers.GetStormPath("some\\other\\path")));

        // act
        XElement xElement = stormElement.ToXElement();

        // assert
        StormElement stormElementResult = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));

        stormElementResult.Id.Should().Be("Abathur_VoiceLine01Common");
        stormElementResult.DataValues["Hero"].Value.GetString().Should().Be("Abathur");
        stormElementResult.ProcessingInstructionsById.Dictionary.Should().BeEmpty();
    }

    private static void SetElementsForMerginSingleElement(out XElement element, out XElement mergingElement)
    {
        element = XElement.Parse(@"
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
        mergingElement = XElement.Parse(@"
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
    }

    private static void AssertMergingSingleElement(StormElement stormElement)
    {
        stormElement.ElementType.Should().Be("CAbilEffectInstant");
        stormElement.DataValues.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/##id##");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0".AsSpan()).GetElementDataAt("color").RawValue.Should().Be("255,0,255,0");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("Model").RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        stormElement.DataValues.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("disableWhileDead").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("SharedFlags").GetElementDataAt("TargetCursorVisibleInBlackMask").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("AllowMovement".AsSpan()).RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("WaitToSpend").RawValue.Should().Be("0");
        stormElement.DataValues.GetElementDataAt("Flags").GetElementDataAt("ValidateButtonState").RawValue.Should().Be("1");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("AutoQueueId").RawValue.Should().Be("Spell");
        stormElement.DataValues.GetElementDataAt("CmdButtonArray").GetElementDataAt("Execute").GetElementDataAt("Flags").GetElementDataAt("Continuous").RawValue.Should().Be("1");
    }
}
