namespace Heroes.XmlData.Tests;

[TestClass]
public class HeroesDataTests
{
    private readonly IStormStorage _stormStorage;
    private readonly HeroesData _heroesData;

    public HeroesDataTests()
    {
        _stormStorage = Substitute.For<IStormStorage>();

        _heroesData = new HeroesData(_stormStorage);
    }

    [TestMethod]
    public void ParseGameString_ConstElement_ParsedGameString()
    {
        // arrange
        string description = "Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\"><d const=\"$YrelSacredGroundArmorBonus\" precision=\"2\"/></c> Armor until she leaves the area.";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddConstantXElements(new List<XElement>()
                {
                    new(
                        "const",
                        new XAttribute("id", "$YrelSacredGroundArmorBonus"),
                        new XAttribute("value", "50")),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        GameStringText parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawText.Should().Be("Yrel sanctifies the ground around her, gaining <c val=\"#TooltipNumbers\">50</c> Armor until she leaves the area.");
    }

    [TestMethod]
    public void ParseGameString_HasAScalingValuePercent_ParsedGameString()
    {
        // arrange
        string description = "Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\"><d ref=\"Effect,OctoGrabPokeMasteryDamage,Amount * 100\"/>%</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "OctoGrabPokeMasteryDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "137"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "MurkyOctoGrab"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "OctoGrabPokeMasteryDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        GameStringText parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawText.Should().Be("Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\">13700%</c><c val=\"#ColorGray\">~~0.04~~</c>");
    }

    [TestMethod]
    [TestCategory("GameStrings")]
    public void ParseGameString_ConstantsWithExpressions_ParsedGameString()
    {
        // arrange
        string description = "Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\"><d ref=\"Behavior,DVaMechSelfDestructMechDetonationCountdown,Duration\" player=\"0\"/></c> seconds. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount+Accumulator,DVaSelfDestructDistanceAccumulator,MinAccumulation\"/></c> to <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,Amount\"/></c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,DVaMechSelfDestructDetonationSearchDamage,AttributeFactor[Structure]*(-100)\"/>%</c> damage against Structures.</n></n><c val=\"FF8000\">Gain <c val=\"#TooltipNumbers\">1%</c> Charge for every <c val=\"#TooltipNumbers\">2</c> seconds spent Basic Attacking, and <c val=\"#TooltipNumbers\">25%</c> Charge per <c val=\"#TooltipNumbers\">100%</c> of Mech Health lost.</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Behavior", "CBehaviorBuff"),
                    ("Effect", "CEffectDamage"),
                    ("Accumulator", "CAccumulatorDistance"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CBehaviorBuff",
                        new XAttribute("id", "DVaMechSelfDestructMechDetonationCountdown"),
                        new XElement(
                            "Duration",
                            new XAttribute("value", "4"))),
                    new(
                        "CAccumulatorDistance",
                        new XAttribute("id", "DVaSelfDestructDistanceAccumulator"),
                        new XElement(
                            "MinAccumulation",
                            new XAttribute("value", "$DVaMechSelfDestructDetonationMinimumAccumulation"))),
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "DVaMechSelfDestructDetonationSearchDamage"),
                        new XElement(
                            "AttributeFactor",
                            new XAttribute("index", "Structure"),
                            new XAttribute("value", "-0.5")),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "$DVaMechSelfDestructDetonationMaximumDamage"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "DVaMechSelfDestruct"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "DVaMechSelfDestructDetonationSearchDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000"))),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Accumulator")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "DVaSelfDestructDistanceAccumulator")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "MinAccumulation")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                })
                .AddConstantXElements(new List<XElement>()
                {
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMaximumDamage"),
                        new XAttribute("value", "1100")),
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMinimumDamage"),
                        new XAttribute("value", "400")),
                    new(
                        "const",
                        new XAttribute("id", "$DVaMechSelfDestructDetonationMinimumAccumulation"),
                        new XAttribute("value", "-($DVaMechSelfDestructDetonationMinimumDamage $DVaMechSelfDestructDetonationMaximumDamage)"),
                        new XAttribute("evaluateAsExpression", "1")),
                }));

        HeroesData heroesData = loader.HeroesData;

        // act
        GameStringText parsed = heroesData.ParseGameString(description, StormLocale.ENUS);

        // assert
        parsed.RawText.Should().Be("Eject from the Mech, setting it to self-destruct after <c val=\"#TooltipNumbers\">4</c> seconds. Deals <c val=\"#TooltipNumbers\">400</c><c val=\"#ColorGray\">~~0.04~~</c> to <c val=\"#TooltipNumbers\">1100</c><c val=\"#ColorGray\">~~0.04~~</c> damage in a large area, depending on distance from center. Deals <c val=\"#TooltipNumbers\">50%</c> damage against Structures.<n/><n/><c val=\"FF8000\">Gain </c><c val=\"#TooltipNumbers\">1%</c><c val=\"FF8000\"> Charge for every </c><c val=\"#TooltipNumbers\">2</c><c val=\"FF8000\"> seconds spent Basic Attacking, and </c><c val=\"#TooltipNumbers\">25%</c><c val=\"FF8000\"> Charge per </c><c val=\"#TooltipNumbers\">100%</c><c val=\"FF8000\"> of Mech Health lost.</c>");
    }

    [TestMethod]
    public void Build_HasBuildId_ReturnsBuildId()
    {
        // arrange
        _stormStorage.GetBuildId().Returns(90240);

        // act
        int? result = _heroesData.Build;

        // assert
        result.Should().Be(90240);
    }

    [TestMethod]
    public void Build_NoBuildId_ReturnsNull()
    {
        // arrange
        _stormStorage.GetBuildId().Returns((int?)null);

        // act
        int? result = _heroesData.Build;

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void HeroesLocalization_NotSet_ReturnsNull()
    {
        // arrange (none)

        // act
        StormLocale? result = _heroesData.HeroesLocalization;

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void SetHeroesLocalization_SetToENUS_HeroesLocalizationReturnsENUS()
    {
        // arrange
        _heroesData.SetHeroesLocalization(StormLocale.ENUS);

        // act
        StormLocale? result = _heroesData.HeroesLocalization;

        // assert
        result.Should().Be(StormLocale.ENUS);
    }

    [TestMethod]
    public void GetStormGameString_ByStringId_GameStringFound_ReturnsStormGameString()
    {
        // arrange
        StormGameString gameString = new("Abil/Name/AlarakDiscordStrike", "Discord Strike");
        _stormStorage.GetStormGameString("Abil/Name/AlarakDiscordStrike").Returns(gameString);

        // act
        StormGameString? result = _heroesData.GetStormGameString("Abil/Name/AlarakDiscordStrike");

        // assert
        result.Should().BeSameAs(gameString);
        result!.Id.Should().Be("Abil/Name/AlarakDiscordStrike");
        result.Value.Should().Be("Discord Strike");
    }

    [TestMethod]
    public void GetStormGameString_ByStringId_GameStringNotFound_ReturnsNull()
    {
        // arrange
        _stormStorage.GetStormGameString(Arg.Any<string>()).Returns((StormGameString?)null);

        // act
        StormGameString? result = _heroesData.GetStormGameString("Unknown");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormAssetString_ByStringId_AssetFound_ReturnsStormAssetString()
    {
        // arrange
        StormAssetString assetString = new("Assets/Textures/btn-alarak.dds", "Assets/Textures/btn-alarak.dds");
        _stormStorage.GetStormAssetString("Assets/Textures/btn-alarak.dds").Returns(assetString);

        // act
        StormAssetString? result = _heroesData.GetStormAssetString("Assets/Textures/btn-alarak.dds");

        // assert
        result.Should().BeSameAs(assetString);
        result!.Id.Should().Be("Assets/Textures/btn-alarak.dds");
    }

    [TestMethod]
    public void GetStormAssetString_ByStringId_AssetNotFound_ReturnsNull()
    {
        // arrange
        _stormStorage.GetStormAssetString(Arg.Any<string>()).Returns((StormAssetString?)null);

        // act
        StormAssetString? result = _heroesData.GetStormAssetString("Unknown.dds");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormGameStrings_HasGameStrings_ReturnsAllGameStrings()
    {
        // arrange
        List<StormGameString> gameStrings =
        [
            new("id1", "value1"),
            new("id2", "value2"),
            new("id3", "value3"),
        ];
        _stormStorage.GetStormGameStrings().Returns(gameStrings);

        // act
        IEnumerable<StormGameString> result = _heroesData.GetStormGameStrings();

        // assert
        result.Should().BeEquivalentTo(gameStrings);
    }

    [TestMethod]
    public void GetStormGameStrings_NoGameStrings_ReturnsEmpty()
    {
        // arrange
        _stormStorage.GetStormGameStrings().Returns([]);

        // act
        IEnumerable<StormGameString> result = _heroesData.GetStormGameStrings();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetStormElementIds_DefaultCacheTypeAll_ReturnsIds()
    {
        // arrange
        List<string> ids = ["Effect1", "Effect2", "Effect3"];
        _stormStorage.GetStormElementIds("Effect", StormCacheType.All).Returns(ids);

        // act
        IEnumerable<string> result = _heroesData.GetStormElementIds("Effect");

        // assert
        result.Should().BeEquivalentTo(ids);
    }

    [TestMethod]
    public void GetStormElementIds_NormalCacheType_ReturnsOnlyNormalCacheIds()
    {
        // arrange
        List<string> ids = ["Effect1"];
        _stormStorage.GetStormElementIds("Effect", StormCacheType.Normal).Returns(ids);

        // act
        IEnumerable<string> result = _heroesData.GetStormElementIds("Effect", StormCacheType.Normal);

        // assert
        result.Should().BeEquivalentTo(ids);
    }

    [TestMethod]
    public void StormAssetFileExists_FileExists_ReturnsTrue()
    {
        // arrange
        _stormStorage.StormAssetFileExists("Assets/Textures/btn-alarak.dds").Returns(true);

        // act
        bool result = _heroesData.StormAssetFileExists("Assets/Textures/btn-alarak.dds");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormAssetFileExists_FileNotFound_ReturnsFalse()
    {
        // arrange
        _stormStorage.StormAssetFileExists("Assets/Textures/unknown.dds").Returns(false);

        // act
        bool result = _heroesData.StormAssetFileExists("Assets/Textures/unknown.dds");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormAssetFile_FileFound_ReturnsStormFile()
    {
        // arrange
        StormFile stormFile = new(TestHelpers.GetStormPath("Assets/Textures/btn-alarak.dds"));
        _stormStorage.GetStormAssetFile("Assets/Textures/btn-alarak.dds").Returns(stormFile);

        // act
        StormFile? result = _heroesData.GetStormAssetFile("Assets/Textures/btn-alarak.dds");

        // assert
        result.Should().BeSameAs(stormFile);
    }

    [TestMethod]
    public void GetStormAssetFile_FileNotFound_ReturnsNull()
    {
        // arrange
        _stormStorage.GetStormAssetFile("unknown.dds").Returns((StormFile?)null);

        // act
        StormFile? result = _heroesData.GetStormAssetFile("unknown.dds");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void StormLayoutFileExists_FileExists_ReturnsTrue()
    {
        // arrange
        _stormStorage.StormLayoutFileExists("ui/layout/loadingscreen.stormlayout").Returns(true);

        // act
        bool result = _heroesData.StormLayoutFileExists("ui/layout/loadingscreen.stormlayout");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_FileNotFound_ReturnsFalse()
    {
        // arrange
        _stormStorage.StormLayoutFileExists("ui/layout/unknown.stormlayout").Returns(false);

        // act
        bool result = _heroesData.StormLayoutFileExists("ui/layout/unknown.stormlayout");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormLayoutFile_FileFound_ReturnsStormFile()
    {
        // arrange
        StormFile stormFile = new(TestHelpers.GetStormPath("ui/layout/loadingscreen.stormlayout"));
        _stormStorage.GetStormLayoutFile("ui/layout/loadingscreen.stormlayout").Returns(stormFile);

        // act
        StormFile? result = _heroesData.GetStormLayoutFile("ui/layout/loadingscreen.stormlayout");

        // assert
        result.Should().BeSameAs(stormFile);
    }

    [TestMethod]
    public void GetStormLayoutFile_FileNotFound_ReturnsNull()
    {
        // arrange
        _stormStorage.GetStormLayoutFile("ui/layout/unknown.stormlayout").Returns((StormFile?)null);

        // act
        StormFile? result = _heroesData.GetStormLayoutFile("ui/layout/unknown.stormlayout");

        // assert
        result.Should().BeNull();
    }

    private static StormElement CreateStormElement(string elementType, string? id = null)
    {
        XElement xElement = id is not null
            ? new XElement(elementType, new XAttribute("id", id))
            : new XElement(elementType);

        return new StormElement(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("test")));
    }
}