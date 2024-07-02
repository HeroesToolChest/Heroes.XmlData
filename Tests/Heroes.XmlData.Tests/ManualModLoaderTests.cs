namespace Heroes.XmlData.Tests;

[TestClass]
public class ManualModLoaderTests
{
    [TestMethod]
    public void AddGameStrings_AddingNewLocale_AddsNewLocaleWithGamestrings()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        List<string> gameStringsENUS =
        [
            "id1=value1",
            "id2=value1",
            "id3=value2",
            "id3=value4",
            "id3",
        ];
        List<string> gameStringsDEDE =
        [
            "id1=value1",
            "id2=value1",
        ];

        // act
        manualModLoader.AddGameStrings(gameStringsENUS, StormLocale.ENUS);
        manualModLoader.AddGameStrings(gameStringsDEDE, StormLocale.DEDE);

        // assert
        manualModLoader.GameStringsByLocale.Should().BeEquivalentTo(new Dictionary<StormLocale, List<string>>
        {
            { StormLocale.ENUS, ["id1=value1", "id2=value1", "id3=value2", "id3=value4", "id3"] },
            { StormLocale.DEDE, ["id1=value1", "id2=value1"] },
        });
    }

    [TestMethod]
    public void AddGameStrings_AddingExistingLocales_AddsGamestrings()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        List<string> gameStrings =
        [
            "id1=value1",
            "id2=value1",
            "id3=value2",
            "id3=value4",
            "id3",
        ];

        manualModLoader.AddGameStrings(gameStrings, StormLocale.ENUS);

        // act
        manualModLoader.AddGameStrings(gameStrings, StormLocale.ENUS);

        // assert
        manualModLoader.GameStringsByLocale.Should().BeEquivalentTo(new Dictionary<StormLocale, List<string>>
        {
            { StormLocale.ENUS, ["id1=value1", "id2=value1", "id3=value2", "id3=value4", "id3", "id1=value1", "id2=value1", "id3=value2", "id3=value4", "id3"] },
        });
    }

    [TestMethod]
    public void AddConstantXElements_AddingTwoCollection_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddConstantXElements([
            XElement.Parse(@"
 <const id=""$ChromieBasicAttackRange"" value=""7"" />
"),
            XElement.Parse(@"
<const id=""$ChromieBasicAttackDamage"" value=""82"" />
")
]);

        manualModLoader.AddConstantXElements([
            XElement.Parse(@"
<const id=""$ChromieWeaponPreSwing"" value=""0.25"" />
"),
            XElement.Parse(@"
<const id=""$ChromieSandBlastDamage"" value=""305"" />
")
]);

        // assert
        manualModLoader.ConstantXElements.Should().BeEquivalentTo([
            XElement.Parse(@"
 <const id=""$ChromieBasicAttackRange"" value=""7"" />
"),
            XElement.Parse(@"
<const id=""$ChromieBasicAttackDamage"" value=""82"" />
"),
            XElement.Parse(@"
<const id=""$ChromieWeaponPreSwing"" value=""0.25"" />
"),
            XElement.Parse(@"
<const id=""$ChromieSandBlastDamage"" value=""305"" />
")
]);
    }

    [TestMethod]
    public void AddBaseElementTypes_AddingNewBaseTypes_AddsBaseTypes()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddBaseElementTypes([("Effect", "CEffectDamage"), ("Unit", "CUnit")]);

        // assert
        manualModLoader.ElementNamesByDataObjectType.Should().BeEquivalentTo(new Dictionary<string, HashSet<string>>
        {
            { "Effect", ["CEffectDamage"] },
            { "Unit", ["CUnit"] },
        });
    }

    [TestMethod]
    public void AddBaseElementTypes_AddingExistingBaseTypes_AddsBaseTypes()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddBaseElementTypes([("Effect", "CEffectDamage"), ("Effect", "CEffectAbility")]);

        // assert
        manualModLoader.ElementNamesByDataObjectType.Should().BeEquivalentTo(new Dictionary<string, HashSet<string>>
        {
            { "Effect", ["CEffectDamage", "CEffectAbility"] },
        });
    }

    [TestMethod]
    public void AddElements_AddingTwoCollections_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddElements([
            XElement.Parse(@"
<CAbilEffectTarget id=""GuldanHorrify"">
  <PrepEffect value=""DismountDecloakCasterSet"" />
</CAbilEffectTarget>
"),
            XElement.Parse(@"
<CAbilEffectTarget id=""GuldanCorruption"">
  <PrepEffect value=""DismountDecloakCasterSet"" />
</CAbilEffectTarget>
")
]);

        manualModLoader.AddElements([
            XElement.Parse(@"
<CAbilEffectTarget id=""GuldanRainOfDestruction"">
  <Activity value=""Abil/Name/GuldanRainOfDestruction"" />
</CAbilEffectTarget>
")]);

        // assert
        manualModLoader.Elements.Should().SatisfyRespectively(
            first =>
            {
                first.Should().BeEquivalentTo(XElement.Parse(@"
<CAbilEffectTarget id=""GuldanHorrify"">
  <PrepEffect value=""DismountDecloakCasterSet"" />
</CAbilEffectTarget>
"));
            },
            second =>
            {
                second.Should().BeEquivalentTo(XElement.Parse(@"
<CAbilEffectTarget id=""GuldanCorruption"">
  <PrepEffect value=""DismountDecloakCasterSet"" />
</CAbilEffectTarget>
"));
            },
            third =>
            {
                third.Should().BeEquivalentTo(XElement.Parse(@"
<CAbilEffectTarget id=""GuldanRainOfDestruction"">
  <Activity value=""Abil/Name/GuldanRainOfDestruction"" />
</CAbilEffectTarget>
"));
            });
    }

    [TestMethod]
    public void AddLevelScalingArrayElements_AddingTwoCollections_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddLevelScalingArrayElements([
            XElement.Parse(@"
<LevelScalingArray Ability=""GuldanDrainLife"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanDrainLifeDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.040000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
"),
            XElement.Parse(@"
<LevelScalingArray Ability=""GuldanHorrify"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanHorrifyDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.040000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
")
]);

        manualModLoader.AddLevelScalingArrayElements([
            XElement.Parse(@"
<LevelScalingArray Ability=""GuldanCorruption"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanCorruptionDoTDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.045000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
")]);

        // assert
        manualModLoader.LevelScalingArrayElements.Should().SatisfyRespectively(
            first =>
            {
                first.Should().BeEquivalentTo(XElement.Parse(@"
<LevelScalingArray Ability=""GuldanDrainLife"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanDrainLifeDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.040000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
"));
            },
            second =>
            {
                second.Should().BeEquivalentTo(XElement.Parse(@"
<LevelScalingArray Ability=""GuldanHorrify"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanHorrifyDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.040000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
"));
            },
            third =>
            {
                third.Should().BeEquivalentTo(XElement.Parse(@"
<LevelScalingArray Ability=""GuldanCorruption"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""GuldanCorruptionDoTDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.045000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
"));
            });
    }

    [TestMethod]
    public void AddStormStyleElements_AddingTwoCollections_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddStormStyleElements([
            XElement.Parse(@"
<Constant name=""ColorSocialPlayerRank"" val=""ffffff"" />
"),
            XElement.Parse(@"
<Constant name=""ColorSocialPlayerOffline"" val=""170,171,197,255"" />
")
]);

        manualModLoader.AddStormStyleElements([
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
")]);

        // assert
        manualModLoader.StormStyleElements.Should().SatisfyRespectively(
            first =>
            {
                first.Should().BeEquivalentTo(XElement.Parse(@"
<Constant name=""ColorSocialPlayerRank"" val=""ffffff"" />
"));
            },
            second =>
            {
                second.Should().BeEquivalentTo(XElement.Parse(@"
<Constant name=""ColorSocialPlayerOffline"" val=""170,171,197,255"" />
"));
            },
            third =>
            {
                third.Should().BeEquivalentTo(XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
"));
            });
    }

    [TestMethod]
    public void Name_SetName_GetName()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        string result = manualModLoader.Name;

        // assert
        result.Should().Be("custom");
    }
}