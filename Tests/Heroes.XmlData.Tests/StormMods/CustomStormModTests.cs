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
            .AddStormStyleElements([XElement.Parse(@"<Constant name=""ColorChatCustomMessageHyperlink"" val=""c60000"" />")]);

        _heroesSource.StormStorage.CreateModStorage(default!).ReturnsForAnyArgs(new StormModStorage(default!, default!));

        CustomStormMod customStormMod = new(_heroesSource, manualModLoader);

        // act
        customStormMod.LoadStormData();

        // assert
        _heroesSource.Received().StormStorage.AddConstantXElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.Received().StormStorage.AddBaseElementTypes(StormModType.Custom, Arg.Any<string>(), Arg.Any<string>());
        _heroesSource.Received().StormStorage.AddElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.Received().StormStorage.AddLevelScalingArrayElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
        _heroesSource.Received().StormStorage.AddStormStyleElement(StormModType.Custom, Arg.Any<XElement>(), Arg.Any<StormPath>());
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

        // act
        customStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        _heroesSource.Received().StormStorage.AddGameString(StormModType.Custom, "Gamestring1", Arg.Any<GameStringText>());
        _heroesSource.Received().StormStorage.AddGameString(StormModType.Custom, "Gamestring2", Arg.Any<GameStringText>());
        _heroesSource.Received().StormStorage.AddGameString(StormModType.Custom, "Gamestring3", Arg.Any<GameStringText>());
    }
}
