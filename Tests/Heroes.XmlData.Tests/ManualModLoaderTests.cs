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

    [TestMethod]
    public void AddAssetFilePaths_AddingTwoCollections_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddAssetFilePaths([
            "path/to/file1",
            "path/to/file2"
            ]);

        manualModLoader.AddAssetFilePaths([
            "path/to/file4"
            ]);

        // assert
        manualModLoader.AssetFilePaths.Should().SatisfyRespectively(
            first =>
            {
                first.Should().Be("path/to/file1");
            },
            second =>
            {
                second.Should().Be("path/to/file2");
            },
            third =>
            {
                third.Should().Be("path/to/file4");
            });
    }

    [TestMethod]
    public void AddStormMaps_HasStormMaps_AddsStormMaps()
    {
        // arrange
        ManualModLoader manualModLoader = new("TestMod");

        StormMap stormMap1 = new()
        {
            Name = "Cursed Hollow",
            NameByLocale = new Dictionary<StormLocale, string>
            {
                { StormLocale.ENUS, "Cursed Hollow" },
                { StormLocale.DEDE, "Verfluchte Höhle" },
            },
            MapId = "CursedHollow",
            MapLink = "CursedHollowLink",
            MapSize = (200.0, 200.0),
            ReplayPreviewImagePath = "Assets/Textures/CursedHollow_Preview.dds",
            LoadingScreenImagePath = "Assets/Textures/CursedHollow_Loading.dds",
            LayoutFilePath = "UI/Layout/CursedHollow.StormLayout",
            LayoutLoadingScreenFrame = "LoadingScreenFrame",
            S2MAFilePath = "Maps/CursedHollow.s2ma",
            S2MVFilePath = "Maps/CursedHollow.s2mv",
        };

        StormMap stormMap2 = new()
        {
            Name = "Towers of Doom",
            NameByLocale = new Dictionary<StormLocale, string>
            {
                { StormLocale.ENUS, "Towers of Doom" },
                { StormLocale.FRFR, "Tours du destin" },
            },
            MapId = "TowersOfDoom",
            MapLink = "TowersOfDoomLink",
            MapSize = (180.0, 180.0),
            ReplayPreviewImagePath = "Assets/Textures/TowersOfDoom_Preview.dds",
            LoadingScreenImagePath = "Assets/Textures/TowersOfDoom_Loading.dds",
            LayoutFilePath = "UI/Layout/TowersOfDoom.StormLayout",
            LayoutLoadingScreenFrame = "LoadingScreenFrame",
            S2MAFilePath = "Maps/TowersOfDoom.s2ma",
            S2MVFilePath = "Maps/TowersOfDoom.s2mv",
        };

        List<StormMap> stormMaps = [stormMap1, stormMap2];

        // act
        ManualModLoader result = manualModLoader.AddStormMaps(stormMaps);

        // assert
        result.Should().BeSameAs(manualModLoader);
        manualModLoader.StormMaps.Should().HaveCount(2);
        manualModLoader.StormMaps.Should().BeEquivalentTo(stormMaps);
        manualModLoader.StormMaps.Should().HaveElementAt(0, stormMap1);
        manualModLoader.StormMaps.Should().HaveElementAt(1, stormMap2);
    }

    [TestMethod]
    public void AddStormMaps_EmptyCollection_AddsNothing()
    {
        // arrange
        ManualModLoader manualModLoader = new("TestMod");
        List<StormMap> stormMaps = [];

        // act
        ManualModLoader result = manualModLoader.AddStormMaps(stormMaps);

        // assert
        result.Should().BeSameAs(manualModLoader);
        manualModLoader.StormMaps.Should().BeEmpty();
    }

    [TestMethod]
    public void AddLayoutFilePaths_AddingTwoCollections_ShouldReturnTotal()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddLayoutFilePaths([
            "path/to/layout1.stormlayout",
            "path/to/layout2.stormlayout"
            ]);

        manualModLoader.AddLayoutFilePaths([
            "path/to/layout3.stormlayout"
            ]);

        // assert
        manualModLoader.LayoutFilePaths.Should().HaveCount(3).And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().Be("path/to/layout1.stormlayout");
                },
                second =>
                {
                    second.Should().Be("path/to/layout2.stormlayout");
                },
                third =>
                {
                    third.Should().Be("path/to/layout3.stormlayout");
                });
    }

    [TestMethod]
    public void AddLayoutFilePaths_AddingDuplicates_OnlyStoresUnique()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");

        // act
        manualModLoader.AddLayoutFilePaths([
            "path/to/layout1.stormlayout",
            "path/to/layout2.stormlayout"
            ]);

        manualModLoader.AddLayoutFilePaths([
            "path/to/layout1.stormlayout",
            "path/to/layout3.stormlayout"
            ]);

        // assert
        manualModLoader.LayoutFilePaths.Should().HaveCount(3);
        manualModLoader.LayoutFilePaths.Should().Contain("path/to/layout1.stormlayout");
        manualModLoader.LayoutFilePaths.Should().Contain("path/to/layout2.stormlayout");
        manualModLoader.LayoutFilePaths.Should().Contain("path/to/layout3.stormlayout");
    }

    [TestMethod]
    public void AddLayoutFilePaths_EmptyCollection_AddsNothing()
    {
        // arrange
        ManualModLoader manualModLoader = new("custom");
        List<string> layoutPaths = [];

        // act
        ManualModLoader result = manualModLoader.AddLayoutFilePaths(layoutPaths);

        // assert
        result.Should().BeSameAs(manualModLoader);
        manualModLoader.LayoutFilePaths.Should().BeEmpty();
    }
}