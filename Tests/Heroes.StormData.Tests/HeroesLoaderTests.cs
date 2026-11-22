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

        SetS2MAProperties(MapTitle, new S2MAProperties() { S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle } });
        SetS2MAProperties(OtherMapTitle, new S2MAProperties() { S2MVProperties = new S2MVProperties() { HeaderTitle = OtherMapTitle } });
    }

    [TestMethod]
    public void LoadWithFile_RootDirectoryDoesNotExist_ThrowsException()
    {
        // arrange
        string rootDirectory = Path.Combine(Path.GetTempPath(), nameof(HeroesLoaderTests), Guid.NewGuid().ToString("N"));

        // act
        Action act = () => HeroesLoader.LoadWithFile(rootDirectory);

        // assert
        act.Should().Throw<DirectoryNotFoundException>();
    }

    [TestMethod]
    public void LoadMapMod_MapTitleInDifferentCase_LoadedMapTitleIsTheMapsTitle()
    {
        // arrange
        string lowerCaseTitle = MapTitle.ToLowerInvariant();
        SetS2MAProperties(lowerCaseTitle, new S2MAProperties() { S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle } });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.LoadMapMod(lowerCaseTitle);

        // assert
        result.Should().BeTrue();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        _heroesSource.Received(1).LoadStormMapMod(MapTitle);
    }

    [TestMethod]
    public void LoadMapMod_SourceThrows_UnloadsMapAndClearsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        _heroesSource.LoadStormMapMod(OtherMapTitle).Returns(_ => throw new IOException("failed"));

        // act
        Action act = () => heroesLoader.LoadMapMod(OtherMapTitle);

        // assert
        act.Should().Throw<IOException>();
        heroesLoader.LoadedMapTitle.Should().BeNull();
        _heroesSource.Received(1).UnloadStormMapMod();
    }

    [TestMethod]
    public void LoadMapMod_SourceThrewForPreviousMap_PreviousMapCanBeLoadedAgain()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        _heroesSource.LoadStormMapMod(OtherMapTitle).Returns(_ => throw new IOException("failed"));

        try
        {
            heroesLoader.LoadMapMod(OtherMapTitle);
        }
        catch (IOException)
        {
        }

        // act
        bool result = heroesLoader.LoadMapMod(MapTitle);

        // assert
        result.Should().BeTrue();
        _heroesSource.Received(2).LoadStormMapMod(MapTitle);
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadStormMods_SourceThrows_LaterLoadsThrowInvalidOperationException()
    {
        // arrange
        _heroesSource.When(x => x.LoadStormMods()).Do(_ => throw new IOException("failed"));
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        heroesLoader.Invoking(x => x.LoadStormMods()).Should().Throw<IOException>();

        // act and assert
        heroesLoader.Invoking(x => x.LoadStormMods()).Should().Throw<InvalidOperationException>();
        heroesLoader.Invoking(x => x.LoadMapMod(MapTitle)).Should().Throw<InvalidOperationException>();
        heroesLoader.Invoking(x => x.LoadGameStrings(StormLocale.ENUS)).Should().Throw<InvalidOperationException>();
        heroesLoader.Invoking(x => x.Load(new HeroesLoaderOptions())).Should().Throw<InvalidOperationException>();

        _heroesSource.Received(1).LoadStormMods();
        heroesLoader.IsStormModsLoaded.Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormMods_SourceThrows_MapQueriesAndUnloadStillWork()
    {
        // arrange
        _heroesSource.When(x => x.LoadStormMods()).Do(_ => throw new IOException("failed"));
        _heroesSource.GetMapTitles().Returns([MapTitle]);
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        heroesLoader.Invoking(x => x.LoadStormMods()).Should().Throw<IOException>();

        // act and assert
        heroesLoader.GetMapTitles().Should().Equal(MapTitle);
        heroesLoader.GetStormMap(MapTitle).Should().NotBeNull();
        heroesLoader.UnloadMapMod().Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormMods_DepotCacheThrows_RetryLoadsOnlyDepotCache()
    {
        // arrange
        int depotCacheCalls = 0;
        _heroesSource.When(x => x.LoadDepotCache()).Do(_ =>
        {
            if (++depotCacheCalls == 1)
                throw new IOException("failed");
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.Invoking(x => x.LoadStormMods()).Should().Throw<IOException>();

        // act
        heroesLoader.LoadStormMods();

        // assert
        _heroesSource.Received(1).LoadStormMods();
        _heroesSource.Received(2).LoadDepotCache();
        heroesLoader.IsStormModsLoaded.Should().BeTrue();
    }

    [TestMethod]
    public void LoadGameStrings_SourceThrows_GameStringsLocaleIsNullAndRetryLoads()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        int calls = 0;
        _heroesSource.When(x => x.LoadGamestrings(StormLocale.DEDE)).Do(_ =>
        {
            if (++calls == 1)
                throw new IOException("failed");
        });

        heroesLoader.Invoking(x => x.LoadGameStrings(StormLocale.DEDE)).Should().Throw<IOException>();
        heroesLoader.GameStringsLocale.Should().BeNull();

        // act
        heroesLoader.LoadGameStrings(StormLocale.DEDE);

        // assert
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.DEDE);
        _heroesSource.Received(2).LoadGamestrings(StormLocale.DEDE);
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
    public void IsStormMapModLoaded_NoMapLoaded_ReturnsFalse()
    {
        // act
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // assert
        heroesLoader.IsStormMapModLoaded.Should().BeFalse();
    }

    [TestMethod]
    public void IsStormMapModLoaded_MapLoaded_ReturnsTrue()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        heroesLoader.IsStormMapModLoaded.Should().BeTrue();
    }

    [TestMethod]
    public void IsStormMapModLoaded_MapUnloaded_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        heroesLoader.UnloadMapMod();

        // assert
        heroesLoader.IsStormMapModLoaded.Should().BeFalse();
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
    public void GetStormMap_NothingLoaded_LoadsOnlyDepotCacheFirst()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties() { S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle } });
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        StormMap? result = heroesLoader.GetStormMap(MapTitle);

        // assert
        result.Should().NotBeNull();
        Received.InOrder(() =>
        {
            _heroesSource.LoadDepotCache();
            _heroesSource.TryGetS2MAProperties(MapTitle, out Arg.Any<S2MAProperties?>());
        });
        _heroesSource.DidNotReceive().LoadStormMods();
        heroesLoader.IsStormModsLoaded.Should().BeFalse();
    }

    [TestMethod]
    public void GetMapTitles_NothingLoaded_LoadsOnlyDepotCacheFirst()
    {
        // arrange
        _heroesSource.GetMapTitles().Returns([MapTitle]);
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        List<string> result = [.. heroesLoader.GetMapTitles()];

        // assert
        result.Should().Equal(MapTitle);
        Received.InOrder(() =>
        {
            _heroesSource.LoadDepotCache();
            _heroesSource.GetMapTitles();
        });
        _heroesSource.DidNotReceive().LoadStormMods();
    }

    [TestMethod]
    public void GetStormMap_CalledThenLoadStormMods_LoadsDepotCacheOnce()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.GetStormMap(MapTitle);
        heroesLoader.GetMapTitles();
        heroesLoader.LoadStormMods();

        // assert
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadStormMods();
    }

    [TestMethod]
    public void GetLoadedStormMapDependencies_MapLoaded_ReturnsNamesAndRelativePathsInLoadOrder()
    {
        // arrange
        IStormMod dependencyStormMod = Substitute.For<IStormMod>();
        dependencyStormMod.Name.Returns("volskayasound");
        dependencyStormMod.RelativeStormModPath.Returns(Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));

        IStormMod mapRootStormMod = Substitute.For<IStormMod>();
        mapRootStormMod.Name.Returns(MapTitle);
        mapRootStormMod.RelativeStormModPath.Returns(Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma"));

        _heroesSource.StormMapMods.Returns([dependencyStormMod, mapRootStormMod]);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        IReadOnlyList<StormMapDependency> result = heroesLoader.GetLoadedStormMapDependencies();

        // assert
        result.Should().BeEquivalentTo(
            [
                new StormMapDependency() { Name = "volskayasound", DirectoryPath = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod") },
                new StormMapDependency() { Name = MapTitle, DirectoryPath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma") },
            ],
            options => options.WithStrictOrdering());
    }

    [TestMethod]
    public void GetLoadedStormMapDependencies_NoMapLoaded_ReturnsEmpty()
    {
        // arrange
        _heroesSource.StormMapMods.Returns([]);
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        IReadOnlyList<StormMapDependency> result = heroesLoader.GetLoadedStormMapDependencies();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetStormMap_MapNotFound_ReturnsNull()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        StormMap? result = heroesLoader.GetStormMap("Unknown Map");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormMap_NoS2MVProperties_ReturnsNull()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties());
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        StormMap? result = heroesLoader.GetStormMap(MapTitle);

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormMap_MapFound_ReturnsMapFromS2MAAndS2MVProperties()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties()
        {
            MapId = "Volskaya",
            DirectoryPath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma"),
            S2MVProperties = new S2MVProperties()
            {
                HeaderTitle = MapTitle,
                NameByStormLocale = new Dictionary<StormLocale, string>()
                {
                    [StormLocale.ENUS] = MapTitle,
                    [StormLocale.FRFR] = "La fonderie Volskaya",
                },
                MapLink = "VolskayaLink",
                MapSize = new MapSize(184, 160),
                PreviewLargeImage = "preview.dds",
                LoadingImage = "loading.dds",
                CustomLayout = Path.Combine("ui", "layout", "loading.stormlayout"),
                CustomFrame = "LoadingScreen/Frame",
                DirectoryPath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "94", "a0", "94a00.s2mv"),
            },
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        StormMap? result = heroesLoader.GetStormMap(MapTitle);

        // assert
        result.Should().BeEquivalentTo(new StormMap()
        {
            Name = MapTitle,
            NameByLocale = new Dictionary<StormLocale, string>()
            {
                [StormLocale.ENUS] = MapTitle,
                [StormLocale.FRFR] = "La fonderie Volskaya",
            },
            MapId = "Volskaya",
            MapLink = "VolskayaLink",
            MapSize = (184, 160),
            ReplayPreviewImagePath = "preview.dds",
            LoadingScreenImagePath = "loading.dds",
            LayoutFilePath = Path.Combine("ui", "layout", "loading.stormlayout"),
            LayoutLoadingScreenFrame = "LoadingScreen/Frame",
            S2MAFilePath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma"),
            S2MVFilePath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "94", "a0", "94a00.s2mv"),
        });
    }

    [TestMethod]
    public void GetStormMap_MapTitleInDifferentCase_NameIsTheMapsTitle()
    {
        // arrange
        SetS2MAProperties(MapTitle.ToLowerInvariant(), new S2MAProperties()
        {
            S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle },
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        StormMap? result = heroesLoader.GetStormMap(MapTitle.ToLowerInvariant());

        // assert
        result.Should().NotBeNull();
        result.Name.Should().Be(MapTitle);
        result.MapId.Should().BeEmpty();
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

    private void SetS2MAProperties(string mapTitle, S2MAProperties s2maProperties)
    {
        _heroesSource.TryGetS2MAProperties(mapTitle, out Arg.Any<S2MAProperties?>()).Returns(x =>
        {
            x[1] = s2maProperties;
            return true;
        });
    }
}
