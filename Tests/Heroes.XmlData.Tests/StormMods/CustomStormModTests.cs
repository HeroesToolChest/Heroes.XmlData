using NSubstitute;

namespace Heroes.XmlData.Tests.StormMods;

[TestClass]
public class CustomStormModTests
{
    private readonly IHeroesSource _heroesSource;

    public CustomStormModTests()
    {
        _heroesSource = Substitute.For<IHeroesSource>();
    }

    [TestMethod]
    public void LoadStormData_HasAllData_AllAddsAreHit()
    {
        // arrange
        ManualModLoader manualModLoader = new ManualModLoader("test")
            .AddConstantXElements([XElement.Parse(@"<const id=""$ChromieBasicAttackRange"" value=""7"" />")])
            .AddBaseElementTypes([("Effect", "CEffectDamage")])
            .AddElements([XElement.Parse(@"
<CAbilEffectTarget id=""GuldanHorrify"">
  <PrepEffect value=""DismountDecloakCasterSet"" />
</CAbilEffectTarget>")])
            .AddLevelScalingArrayElements([XElement.Parse(@"
<LevelScalingArray Ability=""ChromieSandBlast"">
  <Modifications>
    <Catalog value=""Effect"" />
    <Entry value=""ChromieSandBlastDamage"" />
    <Field value=""Amount"" />
    <Value value=""0.040000"" />
    <AffectedByAbilityPower value=""1"" />
    <AffectedByOverdrive value=""1"" />
  </Modifications>
</LevelScalingArray>
")])
            .AddStormStyleElements([XElement.Parse(@"<Constant name=""ColorChatCustomMessageHyperlink"" val=""c60000"" />")])
            .AddAssetFilePaths([Path.Join("this", "is", "file", "path")]);

        _heroesSource.StormStorage.CreateModStorage(default!).ReturnsForAnyArgs(new StormModStorage(default!, default!));

        CustomStormMod customStormMod = new(_heroesSource, manualModLoader);

        // act
        customStormMod.LoadStormData();

        // assert
        _heroesSource.StormStorage.Received().AddConstantXElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.StormStorage.Received().AddBaseElementTypes(StormModType.Custom, Arg.Any<string>(), Arg.Any<string>());
        _heroesSource.StormStorage.Received().AddElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.StormStorage.Received().AddLevelScalingArrayElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.StormStorage.Received().AddStormStyleElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.StormStorage.Received().AddAssetFilePath(StormModType.Custom, Arg.Any<string>(), Arg.Any<StormPath>());
    }

    [TestMethod]
    public void LoadStormGameStrings_HasGameStringsToBeAdded_AddsGameStrings()
    {
        // arrange
        ManualModLoader manualModLoader = new ManualModLoader("test")
            .AddGameStrings(
            [
                "Gamestring1=value1",
                "Gamestring2=value2",
                "Gamestring3=value3",
            ],
            StormLocale.ENUS);

        CustomStormMod customStormMod = new(_heroesSource, manualModLoader);

        _heroesSource.StormStorage.GetGameStringWithId("Gamestring1=value1", Arg.Any<StormPath>()).Returns(("Gamestring1", new GameStringText("value1", TestHelpers.GetStormPath("custom"))));
        _heroesSource.StormStorage.GetGameStringWithId("Gamestring2=value2", Arg.Any<StormPath>()).Returns(("Gamestring2", new GameStringText("value2", TestHelpers.GetStormPath("custom"))));
        _heroesSource.StormStorage.GetGameStringWithId("Gamestring3=value3", Arg.Any<StormPath>()).Returns(("Gamestring3", new GameStringText("value3", TestHelpers.GetStormPath("custom"))));

        // act
        customStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        customStormMod.StormModStorage.Received().AddGameString("Gamestring1", Arg.Any<GameStringText>());
        customStormMod.StormModStorage.Received().AddGameString("Gamestring2", Arg.Any<GameStringText>());
        customStormMod.StormModStorage.Received().AddGameString("Gamestring3", Arg.Any<GameStringText>());
    }
}
