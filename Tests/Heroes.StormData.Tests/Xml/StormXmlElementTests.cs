namespace Heroes.StormData.Xml.Tests;

[TestClass]
public sealed class StormXmlElementTests
{
    [TestMethod]
    public void Id_HasId_ReturnsValue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" default="1" />
            """);

        // act
        string? result = element.Id;

        // assert
        result.Should().Be("TestId");
    }

    [TestMethod]
    public void Id_NoId_ReturnsValue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        string? result = element.Id;

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void HasId_HasId_ReturnsTrue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" default="1" />
            """);

        // act
        bool result = element.HasId;

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void HasId_NoId_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        bool result = element.HasId;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Parent_NoParent_ReturnsValue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        string? result = element.Parent;

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void HasParent_NoParent_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        bool result = element.HasParent;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void HasParent_HasParent_ReturnsTrue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" default="1" />
            """); 

        // act  
        bool result = element.HasParent;

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Parent_HasParent_ReturnsValue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" default="1" />
            """);

        // act
        string? result = element.Parent;

        // assert
        result.Should().Be("TestParent");
    }

    [TestMethod]
    public void IsDefault_HasDefault1_ReturnsTrue()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" default="1" />
            """);

        // act
        bool result = element.IsDefault;

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsDefault_HasDefault0_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" default="0" />
            """);

        // act
        bool result = element.IsDefault;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsDefault_HasNoDefault_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" />

            """);

        // act
        bool result = element.IsDefault;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ElementType_ValidType_ReturnsType()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestId" parent="TestParent" />

            """);

        // act
        StormXmlElementType result = element.ElementType;

        // assert
        result.Should().Be(StormXmlElementType.Class);
    }

    [TestMethod]
    public void ElementName_ValidValue_ReturnsCorrectString()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        string result = element.ElementName;

        // assert
        result.Should().Be("Abil");
    }

    [TestMethod]
    public void ElementNameWithDeclaration_ValidValue_ReturnsCorrectString()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        string result = element.ElementNameWithDeclaration;

        // assert
        result.Should().Be("CAbil");
    }

    [TestMethod]
    public void StormFiles_ValidValue_ReturnsCorrectList()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1" />
            """);

        // act
        IReadOnlyList<StormFile> result = element.StormFiles;

        // assert
        result.Should().ContainSingle();
    }

    [TestMethod]
    public void Members_AttributeAndElementSame_SameMemberAccess()
    {
        // arrange
        StormXmlElement element1 = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1">
              <OrderArray>
                <LineTexture value="Assets\Textures\WayPointLine.dds" />
              </OrderArray>
            </CAbil>
            """);

        StormXmlElement element2 = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1">
              <OrderArray index="0" LineTexture="Assets\Textures\WayPointLine.dds" />
            </CAbil>
            """);

        // act
        StormXmlMember result1 = element1.Members;
        StormXmlMember result2 = element2.Members;

        // assert
        result1["OrderArray"].ByIndex[0]["LineTexture"].Value.RawValue.Should().Be("Assets\\Textures\\WayPointLine.dds");
        result2["OrderArray"].ByIndex[0]["LineTexture"].Value.RawValue.Should().Be("Assets\\Textures\\WayPointLine.dds");
    }

    [TestMethod]
    public void Members_NonArrayValueAttributeAndElementSame_SameMemberAccess()
    {
        // arrange
        StormXmlElement element1 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage default="1">
              <LaunchLocation Value="SourceUnit" />
            </CEffectDamage>
            """);

        StormXmlElement element2 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage default="1">
              <LaunchLocation>
                <Value value="SourceUnit" />
              </LaunchLocation>
            </CEffectDamage>
            """);

        // act
        StormXmlMember result1 = element1.Members;
        StormXmlMember result2 = element2.Members;

        // assert
        result1["LaunchLocation"].ByMember["Value"].Value.RawValue.Should().Be("SourceUnit");
        result2["LaunchLocation"].ByMember["Value"].Value.RawValue.Should().Be("SourceUnit");
    }

    [TestMethod]
    public void Members_ArrayValueAttributeAndElementSame_SameMemberAccess()
    {
        // arrange
        StormXmlElement element1 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage default="1">
              <LeechScoreArray Value="SelfHealing" />
            </CEffectDamage>
            """);

        StormXmlElement element2 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage default="1">
              <LeechScoreArray>
                <Value value="SelfHealing" />
              </LeechScoreArray>
            </CEffectDamage>
            """);

        // act
        StormXmlMember result1 = element1.Members;
        StormXmlMember result2 = element2.Members;

        // assert
        result1["LeechScoreArray"].ByIndex[0]["Value"].Value.RawValue.Should().Be("SelfHealing");
        result2["LeechScoreArray"].ByIndex[0]["Value"].Value.RawValue.Should().Be("SelfHealing");
    }

    [TestMethod]
    public void Merge_SingleElement_MembersAreMerged()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
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

        StormXmlElement mergingElement = XmlReaderHelper.GetStormXmlElement(
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

        // act
        element.Merge(mergingElement);

        // assert
        element.ElementName.Should().Be("AbilEffectInstant");
        element.ElementType.Should().Be(StormXmlElementType.Class);
        element.Id.Should().BeNull();
        element.Parent.Should().BeNull();
        element.IsDefault.Should().BeTrue();
        element.Members["Name"].Value.RawValue.Should().Be("Abil/Name/##id##");
        element.Members["OrderArray"].ByIndex[0]["Color"].Value.RawValue.Should().Be("255,0,255,0");
        element.Members["OrderArray"].ByIndex[0]["Model"].Value.RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        element.Members["OrderArray"].ByIndex[0]["LineTexture"].Value.RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        element.Members["SharedFlags"].ByIndex["DisableWhileDead"].Value.RawValue.Should().Be("0");
        element.Members["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.RawValue.Should().Be("1");
        element.Members["SharedFlags"].ByIndex["TargetCursorVisibleInBlackMask"].Value.RawValue.Should().Be("1");
        element.Members["Flags"].ByIndex["AllowMovement"].Value.RawValue.Should().Be("1");
        element.Members["Flags"].ByIndex["WaitToSpend"].Value.RawValue.Should().Be("0");
        element.Members["Flags"].ByIndex["ValidateButtonState"].Value.RawValue.Should().Be("1");
        element.Members["CmdButtonArray"].ByIndex["Execute"]["AutoQueueId"].Value.RawValue.Should().Be("Spell");
        element.Members["CmdButtonArray"].ByIndex["Execute"]["Flags"].ByIndex["Continuous"].Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void Merge_MultipleValuesWithIdThatHasParents_MembersAreMerged()
    {
        StormXmlElement baseElement = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffect default="1">
              <Chance value="1" />
              <Marker Link="Effect/##id##" />
              <DamageModifierSource Value="Unknown" />
            </CEffect>
            """);

        StormXmlElement element2 = XmlReaderHelper.GetStormXmlElement(
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

        StormXmlElement element3 = XmlReaderHelper.GetStormXmlElement(
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

        StormXmlElement element4 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage default="1" id="StormSpell" parent="StormDamage">
              <CritValidatorArray value="CritAliasSpellPower" />
              <Kind value="Ability" />
              <KindSplash value="Ability" />
            </CEffectDamage>
            """);

        StormXmlElement element5 = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectDamage id="StormBoltDamage" parent="StormSpell">
              <Amount value="110" />
              <CritValidatorArray index="0" value="CritAliasSpellPowerOrMuradinSledgehammerCombine" />
              <FlatModifierArray index="MuradinStormboltPerfectStorm" Accumulator="MuradinStormboltPerfectStormAccumulator" />
              <MultiplicativeModifierArray index="MuradinStormboltSledgehammer" Validator="HasMuradinStormhammerSledgehammerAndTargetNotHeroic" Modifier="2.5" />
            </CEffectDamage>
            """);

        // act
        baseElement.Merge(element2);
        baseElement.Merge(element3);
        baseElement.Merge(element4);
        baseElement.Merge(element5);

        // assert
        baseElement.ElementName.Should().Be("EffectDamage");
        baseElement.Parent.Should().Be("StormSpell");
        baseElement.Members["ResponseFlags"].ByIndex["Acquire"].Value.RawValue.Should().Be("1");
        baseElement.Members["ResponseFlags"].ByIndex["Acquire"].IsArray.Should().BeTrue();
        baseElement.Members["LeechScoreArray"].ByIndex["0"]["Value"].Value.RawValue.Should().Be("SelfHealing");
        baseElement.Members["LeechScoreArray"].ByIndex["0"].IsArray.Should().BeTrue();
        baseElement.Members["ImpactLocation"].ByMember["Value"].Value.RawValue.Should().Be("TargetUnitOrPoint");
        baseElement.Members["ImpactLocation"].ByMember["History"].Value.RawValue.Should().Be("Damage");
        baseElement.Members["DamageModifierSource"].ByMember["Value"].Value.RawValue.Should().Be("Caster");
        baseElement.Members["AmountScoreArray"].ByIndex["3"]["Value"].Value.RawValue.Should().Be("MinionDamage");
        baseElement.Members["AmountScoreArray"].ByIndex["3"]["Validator"].Value.RawValue.Should().Be("TargetMinion");
        baseElement.Members["AmountScoreArray"].ByIndex["3"].IsArray.Should().BeTrue();
        baseElement.Members["MultiplicativeModifierArray"].ByIndex["MuradinStormboltSledgehammer"]["Modifier"].Value.RawValue.Should().Be("2.5");

        baseElement.Members["SplashHistory"].IsArray.Should().BeFalse();
        baseElement.Members["ResponseFlags"].IsArray.Should().BeTrue();

        baseElement.IsDefault.Should().BeFalse();
    }

    [TestMethod]
    public void Merge_CostElmeent_MergedAsNonIndexed()
    {
        // arrange
        StormXmlElement baseElement = XmlReaderHelper.GetStormXmlElement(
            """
            <CWeapon default="1">
              <Cost>
                <Cooldown Link="Weapon/##id##"/>
              </Cost>
            </CWeapon>
            """);

        StormXmlElement element2 = XmlReaderHelper.GetStormXmlElement(
            """
            <CWeaponLegacy default="1" id="StormHeroWeapon">
            </CWeaponLegacy>
            """);

        StormXmlElement element3 = XmlReaderHelper.GetStormXmlElement(
            """
            <CWeaponLegacy default="1" id="StormHeroFastWeapon" parent="StormHeroWeapon">
            </CWeaponLegacy>
            """);

        StormXmlElement element4 = XmlReaderHelper.GetStormXmlElement(
            """
            <CWeaponLegacy id="TracerHeroWeapon" parent="StormHeroFastWeapon">
              <Cost>
                <Vital index="Energy" value="2" />
              </Cost>
            </CWeaponLegacy>
            """);

        // act
        baseElement.Merge(element2);
        baseElement.Merge(element3);
        baseElement.Merge(element4);

        // assert
        baseElement.ElementType.Should().Be(StormXmlElementType.Class);
        baseElement.ElementName.Should().Be("WeaponLegacy");
        baseElement.Parent.Should().Be("StormHeroFastWeapon");
        baseElement.Members["Cost"].ByMember["Vital"].ByIndex["Energy"].Value.Should().Be("2");
        baseElement.Members["Cost"].Value.GetString().Should().BeEmpty();
        baseElement.Members["Cost"].ByMember["Cooldown"].ByMember["Link"].Value.RawValue.Should().Be("Weapon/##id##");
        baseElement.Members["Cost"].ByMember["Cooldown"].ByMember["Link"].Value.GetString().Should().Be("Weapon/TracerHeroWeapon");
    }
}
