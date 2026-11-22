namespace Heroes.StormData.Tests;

[TestClass]
public sealed class HeroesLoaderTests
{
    private const string MapTitle = "Volskaya Foundry";
    private const string OtherMapTitle = "Infernal Shrines";

    private readonly IHeroesSource _heroesSource = Substitute.For<IHeroesSource>();

    public HeroesLoaderTests()
    {
        _heroesSource.LoadStormMapMod(Arg.Is<string>(x => x == MapTitle || x == OtherMapTitle)).Returns(true);
    }

    [TestMethod]
    public void LoadWithInternal_NoOptions_LoadsNothing()
    {
        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // assert
        heroesLoader.IsStormModsLoaded.Should().BeFalse();
        _heroesSource.DidNotReceive().LoadStormMods();
    }

    [TestMethod]
    public void LoadWithInternal_WithOptions_LoadsStormModsThenMapThenGameStringsOnce()
    {
        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, new HeroesLoaderOptions()
        {
            MapTitle = MapTitle,
            GameStringsLocale = StormLocale.DEDE,
        });

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadStormMods();
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadStormMapMod(MapTitle);
            _heroesSource.LoadGamestrings(StormLocale.DEDE);
        });

        _heroesSource.Received(1).LoadGamestrings(Arg.Any<StormLocale>());
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.DEDE);
    }

    [TestMethod]
    public void Load_MapTitleNotFound_ThrowsException()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = "Unknown Map" });

        // assert
        act.Should().Throw<HeroesStormDataException>();
    }

    [TestMethod]
    public void Load_MapAndLocaleChanged_LoadsGameStringsOnce()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, new HeroesLoaderOptions() { MapTitle = MapTitle, GameStringsLocale = StormLocale.ENUS });
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = OtherMapTitle, GameStringsLocale = StormLocale.DEDE });

        // assert
        _heroesSource.Received(1).LoadGamestrings(Arg.Any<StormLocale>());
        _heroesSource.Received(1).LoadGamestrings(StormLocale.DEDE);
    }

    [TestMethod]
    public void Load_OnlyMapChangedWithGameStringsLoaded_ReloadsCurrentLocale()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, new HeroesLoaderOptions() { MapTitle = MapTitle, GameStringsLocale = StormLocale.FRFR });
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = OtherMapTitle });

        // assert
        _heroesSource.Received(1).LoadGamestrings(StormLocale.FRFR);
    }

    [TestMethod]
    public void Load_SameState_LoadsNothing()
    {
        // arrange
        HeroesLoaderOptions options = new() { MapTitle = MapTitle, GameStringsLocale = StormLocale.ENUS };
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, options);
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.Load(options);

        // assert
        _heroesSource.ReceivedCalls().Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormMods_CalledTwice_LoadsOnce()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadStormMods();
        heroesLoader.LoadStormMods();

        // assert
        _heroesSource.Received(1).LoadStormMods();
        _heroesSource.Received(1).LoadDepotCache();
        heroesLoader.IsStormModsLoaded.Should().BeTrue();
    }

    [TestMethod]
    public void LoadMapMod_StormModsNotLoaded_LoadsStormModsFirst()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadStormMods();
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadStormMapMod(MapTitle);
        });
    }

    [TestMethod]
    public void LoadMapMod_MapFound_ReturnsTrueAndSetsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.LoadMapMod(MapTitle);

        // assert
        result.Should().BeTrue();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadMapMod_MapNotFound_ReturnsFalseAndKeepsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        bool result = heroesLoader.LoadMapMod("Unknown Map");

        // assert
        result.Should().BeFalse();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadMapMod_SameMapDifferentCase_DoesNotReload()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        bool result = heroesLoader.LoadMapMod(MapTitle.ToUpperInvariant());

        // assert
        result.Should().BeTrue();
        _heroesSource.Received(1).LoadStormMapMod(Arg.Any<string>());
    }

    [TestMethod]
    public void LoadMapMod_GameStringsLoaded_ReloadsGameStrings()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadGameStrings(StormLocale.KOKR);
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadStormMapMod(MapTitle);
            _heroesSource.LoadGamestrings(StormLocale.KOKR);
        });
    }

    [TestMethod]
    public void LoadMapMod_GameStringsNotLoaded_DoesNotLoadGameStrings()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        _heroesSource.DidNotReceive().LoadGamestrings(Arg.Any<StormLocale>());
    }

    [TestMethod]
    public void UnloadMapMod_MapLoaded_ReturnsTrueAndClearsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        bool result = heroesLoader.UnloadMapMod();

        // assert
        result.Should().BeTrue();
        heroesLoader.LoadedMapTitle.Should().BeNull();
        _heroesSource.Received(1).UnloadStormMapMod();
    }

    [TestMethod]
    public void UnloadMapMod_NoMapLoaded_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.UnloadMapMod();

        // assert
        result.Should().BeFalse();
        _heroesSource.DidNotReceive().UnloadStormMapMod();
    }

    [TestMethod]
    public void LoadGameStrings_SameLocale_LoadsOnce()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadGameStrings(StormLocale.ENUS);
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        // assert
        _heroesSource.Received(1).LoadGamestrings(StormLocale.ENUS);
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.ENUS);
    }

    [TestMethod]
    public void LoadWithInternal_SourceHasModsDirectoryPath_SetsRootDirectory()
    {
        // arrange
        string modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
        _heroesSource.ModsDirectoryPath.Returns(modsDirectoryPath);

        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // assert
        heroesLoader.RootDirectory.Should().Be(modsDirectoryPath);
    }

    [TestMethod]
    public void LoadWithInternal_WithModsInfoFile_SetsVersionAndIsPtr()
    {
        // arrange
        ModsInfoFile modsInfoFile = new()
        {
            Version = "2.55.13.95301",
            IsPtr = true,
        };

        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, modsInfoFile: modsInfoFile);

        // assert
        heroesLoader.ModsInfoFile.Should().BeSameAs(modsInfoFile);
        heroesLoader.HeroesVersion.Should().Be("2.55.13.95301");
        heroesLoader.IsPtr.Should().BeTrue();
    }

    [TestMethod]
    public void LoadWithInternal_NoModsInfoFile_VersionIsNullAndIsNotPtr()
    {
        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // assert
        heroesLoader.ModsInfoFile.Should().BeNull();
        heroesLoader.HeroesVersion.Should().BeNull();
        heroesLoader.IsPtr.Should().BeFalse();
    }

    [TestMethod]
    public void GetModsInfoFile_FileDoesNotExist_ReturnsNull()
    {
        // arrange
        MockFileSystem fileSystem = new();

        // act
        ModsInfoFile? result = HeroesLoader.GetModsInfoFile(fileSystem, MockUnixSupport.Path(@"C:\Heroes\mods"));

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetModsInfoFile_FileExistsWithNoVersionProperty_PropertiesAreDefault()
    {
        // arrange
        string rootDirectory = MockUnixSupport.Path(@"C:\Heroes\mods");
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>()
        {
            [Path.Join(rootDirectory, HeroesLoader.ModsHdpInfoFileName)] = new MockFileData(
                """
                {
                  "property": "value"
                }
                """),
        });

        // act
        ModsInfoFile? result = HeroesLoader.GetModsInfoFile(fileSystem, rootDirectory);

        // assert
        result.Should().NotBeNull();
        result.Version.Should().BeNull();
        result.IsPtr.Should().BeFalse();
        result.HdpVersion.Should().BeEmpty();
    }

    [TestMethod]
    public void GetModsInfoFile_FileExistsWithProperties_ReturnsProperties()
    {
        // arrange
        string rootDirectory = MockUnixSupport.Path(@"C:\Heroes\mods");
        MockFileSystem fileSystem = new(new Dictionary<string, MockFileData>()
        {
            [Path.Join(rootDirectory, HeroesLoader.ModsHdpInfoFileName)] = new MockFileData(
                """
                {
                  // some comment
                  "version": "2.55.13.95301",
                  "isPtr": true,
                  "customproperty": "customvalue",
                  "hdpVersion": "5.12.64",
                  "extractedDate": "2024-06-01T12:34:56Z"
                }
                """),
        });

        // act
        ModsInfoFile? result = HeroesLoader.GetModsInfoFile(fileSystem, rootDirectory);

        // assert
        result.Should().NotBeNull();
        result.Version.Should().Be("2.55.13.95301");
        result.IsPtr.Should().BeTrue();
        result.HdpVersion.Should().Be("5.12.64");
        result.ExtractedDate.Should().Be(new DateTimeOffset(2024, 6, 1, 12, 34, 56, TimeSpan.Zero));
    }

    [TestMethod]
    public void LoadGameStrings_StormModsNotLoaded_LoadsStormModsFirst()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadStormMods();
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadGamestrings(StormLocale.ENUS);
        });
    }
}
