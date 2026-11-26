namespace Heroes.StormData.Tests;

[TestClass]
public sealed class HeroesLoaderTests
{
    private const string MapTitle = "Volskaya Foundry";
    private const string OtherMapTitle = "Infernal Shrines";

    private readonly IHeroesSource _heroesSource = Substitute.For<IHeroesSource>();

    public HeroesLoaderTests()
    {
        _heroesSource.SourceContainer.Returns(new SourceContainer());
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
    public void LoadWithFile_RootDirectoryHasTrailingSeparator_RootDirectoryIsFullPathWithoutIt()
    {
        // arrange
        string rootDirectory = CreateTempDirectory();

        try
        {
            // act
            HeroesLoader heroesLoader = HeroesLoader.LoadWithFile(rootDirectory + Path.DirectorySeparatorChar);

            // assert
            heroesLoader.RootDirectory.Should().Be(rootDirectory);
        }
        finally
        {
            Directory.Delete(rootDirectory);
        }
    }

    [TestMethod]
    public void LoadWithFile_RelativeRootDirectory_RootDirectoryIsFullPath()
    {
        // arrange
        string rootDirectory = CreateTempDirectory();
        string relativeRootDirectory = Path.GetRelativePath(Environment.CurrentDirectory, rootDirectory);

        try
        {
            // act
            HeroesLoader heroesLoader = HeroesLoader.LoadWithFile(relativeRootDirectory);

            // assert
            Path.IsPathFullyQualified(heroesLoader.RootDirectory).Should().BeTrue();
            heroesLoader.RootDirectory.Should().Be(rootDirectory);
        }
        finally
        {
            Directory.Delete(rootDirectory);
        }
    }

    [TestMethod]
    [DataRow((StormLocale)999, null)]
    [DataRow(null, " ")]
    public void LoadWithFile_OptionsNotValid_ThrowsBeforeCheckingRootDirectory(StormLocale? locale, string? mapTitle)
    {
        // arrange
        // the directory does not exist, so the options must be checked first for this to be an argument exception
        string rootDirectory = Path.Combine(Path.GetTempPath(), nameof(HeroesLoaderTests), Guid.NewGuid().ToString("N"));

        // act
        Action act = () => HeroesLoader.LoadWithFile(rootDirectory, options: new HeroesLoaderOptions() { GameStringsLocale = locale, MapTitle = mapTitle });

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("options");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void GetModsInfoFile_NullOrWhiteSpaceRootDirectory_ThrowsArgumentException(string? rootDirectory)
    {
        // arrange
        MockFileSystem fileSystem = new();

        // act
        Action act = () => HeroesLoader.GetModsInfoFile(fileSystem, rootDirectory!);

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("rootDirectory");
    }

    [TestMethod]
    public void LoadWithCASC_CASCHandlerIsNull_ThrowsArgumentNullException()
    {
        // act
        Action act = () => HeroesLoader.LoadWithCASC(null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cascHandler");
    }

    [TestMethod]
    public void OpenCASCStorage_CASCConfigIsNull_ThrowsArgumentNullException()
    {
        // arrange
        using HttpClient httpClient = new();

        // act
        Action act = () => HeroesLoader.OpenCASCStorage(null!, httpClient);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cascConfig");
    }

    [TestMethod]
    public void LoadMapMod_MapTitleInDifferentCase_LoadedMapTitleIsTheMapsTitle()
    {
        // arrange
        string lowerCaseTitle = MapTitle.ToLowerInvariant();
        SetS2MAProperties(lowerCaseTitle, new S2MAProperties() { S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle } });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(lowerCaseTitle);

        // assert
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
        heroesLoader.LoadMapMod(MapTitle);

        // assert
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
        heroesLoader.Invoking(x => x.TryLoadMapMod(MapTitle)).Should().Throw<InvalidOperationException>();
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
    public void LoadMapMod_DepotCacheThrows_RetryLoadsOnlyDepotCache()
    {
        // arrange
        int depotCacheCalls = 0;
        _heroesSource.When(x => x.LoadDepotCache()).Do(_ =>
        {
            if (++depotCacheCalls == 1)
                throw new IOException("failed");
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.Invoking(x => x.LoadMapMod(MapTitle)).Should().Throw<IOException>();

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        _heroesSource.Received(1).LoadStormMods();
        _heroesSource.Received(2).LoadDepotCache();
        heroesLoader.IsStormModsLoaded.Should().BeTrue();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadGameStrings_DepotCacheWouldThrow_LoadsWithoutIt()
    {
        // arrange
        // the map files are not needed for the base stormmods and their gamestrings
        _heroesSource.When(x => x.LoadDepotCache()).Do(_ => throw new IOException("failed"));
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        // assert
        heroesLoader.IsStormModsLoaded.Should().BeTrue();
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.ENUS);
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void LoadGameStrings_SourceThrows_GameStringsLocaleIsNullAndRetryLoads()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        int calls = 0;
        _heroesSource.When(x => x.LoadGameStrings(StormLocale.DEDE)).Do(_ =>
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
        _heroesSource.Received(2).LoadGameStrings(StormLocale.DEDE);
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
    public void LoadWithInternal_WithOptions_LooksUpMapThenLoadsStormModsThenMapThenGameStringsOnce()
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
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadStormMods();
            _heroesSource.LoadStormMapMod(MapTitle);
            _heroesSource.LoadGameStrings(StormLocale.DEDE);
        });

        _heroesSource.Received(1).LoadGameStrings(Arg.Any<StormLocale>());
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.DEDE);
    }

    [TestMethod]
    public void Load_MapTitleNotFound_ThrowsKeyNotFoundExceptionWithoutLoadingStormMods()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = "Unknown Map" });

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*Unknown Map*");
        _heroesSource.DidNotReceive().LoadStormMods();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void Load_EmptyOrWhiteSpaceMapTitle_ThrowsArgumentExceptionWithoutLoading(string mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = mapTitle });

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("options");
        _heroesSource.DidNotReceive().LoadStormMods();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void Load_MapLoadedButGameStringsThrow_MapStaysLoadedWithoutGameStrings()
    {
        // arrange
        _heroesSource.When(x => x.LoadGameStrings(StormLocale.DEDE)).Do(_ => throw new IOException("failed"));
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = MapTitle, GameStringsLocale = StormLocale.DEDE });

        // assert
        act.Should().Throw<IOException>();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        heroesLoader.GameStringsLocale.Should().BeNull();
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
        _heroesSource.Received(1).LoadGameStrings(Arg.Any<StormLocale>());
        _heroesSource.Received(1).LoadGameStrings(StormLocale.DEDE);
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
        _heroesSource.Received(1).LoadGameStrings(StormLocale.FRFR);
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
    public void LoadStormMods_CalledTwice_LoadsOnceWithoutDepotCache()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadStormMods();
        heroesLoader.LoadStormMods();

        // assert
        _heroesSource.Received(1).LoadStormMods();
        _heroesSource.DidNotReceive().LoadDepotCache();
        heroesLoader.IsStormModsLoaded.Should().BeTrue();
    }

    [TestMethod]
    public void Load_NoMapTitle_DoesNotLoadDepotCache()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.Load(new HeroesLoaderOptions() { GameStringsLocale = StormLocale.ENUS });

        // assert
        _heroesSource.Received(1).LoadStormMods();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void LoadMapMod_StormModsNotLoaded_LooksUpMapThenLoadsStormModsThenMap()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadStormMods();
            _heroesSource.LoadStormMapMod(MapTitle);
        });
    }

    [TestMethod]
    public void LoadMapMod_MapNotFound_DoesNotLoadStormMods()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.LoadMapMod("Unknown Map");

        // assert
        act.Should().Throw<KeyNotFoundException>();
        _heroesSource.DidNotReceive().LoadStormMods();
        heroesLoader.IsStormModsLoaded.Should().BeFalse();
    }

    [TestMethod]
    public void LoadMapMod_GameStringsReloadThrows_MapStaysLoadedWithoutGameStrings()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadGameStrings(StormLocale.ENUS);

        _heroesSource.When(x => x.LoadGameStrings(StormLocale.ENUS)).Do(_ => throw new IOException("failed"));

        // act
        Action act = () => heroesLoader.LoadMapMod(MapTitle);

        // assert
        act.Should().Throw<IOException>();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        heroesLoader.GameStringsLocale.Should().BeNull();
        _heroesSource.DidNotReceive().UnloadStormMapMod();
    }

    [TestMethod]
    public void LoadMapMod_MapFound_SetsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadMapMod_MapNotFound_ThrowsKeyNotFoundExceptionAndKeepsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        Action act = () => heroesLoader.LoadMapMod("Unknown Map");

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*Unknown Map*");
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        _heroesSource.DidNotReceive().UnloadStormMapMod();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void LoadMapMod_NullOrWhiteSpaceMapTitle_ThrowsArgumentExceptionWithoutLoading(string? mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.LoadMapMod(mapTitle!);

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("mapTitle");
        _heroesSource.DidNotReceive().LoadStormMods();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void LoadMapMod_SameMapDifferentCase_DoesNotReload()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        heroesLoader.LoadMapMod(MapTitle.ToUpperInvariant());

        // assert
        _heroesSource.Received(1).LoadStormMapMod(Arg.Any<string>());
    }

    [TestMethod]
    public void TryLoadMapMod_MapFound_ReturnsTrueAndSetsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryLoadMapMod(MapTitle);

        // assert
        result.Should().BeTrue();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
    }

    [TestMethod]
    public void TryLoadMapMod_MapNotFound_ReturnsFalseAndKeepsLoadedMapTitle()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        // act
        bool result = heroesLoader.TryLoadMapMod("Unknown Map");

        // assert
        result.Should().BeFalse();
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
        _heroesSource.DidNotReceive().UnloadStormMapMod();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void TryLoadMapMod_NullOrWhiteSpaceMapTitle_ReturnsFalseWithoutLoading(string? mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryLoadMapMod(mapTitle);

        // assert
        result.Should().BeFalse();
        _heroesSource.DidNotReceive().LoadStormMods();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void TryLoadMapMod_StormModsNotLoaded_LooksUpMapThenLoadsStormModsThenMap()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.TryLoadMapMod(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadDepotCache();
            _heroesSource.LoadStormMods();
            _heroesSource.LoadStormMapMod(MapTitle);
        });
    }

    [TestMethod]
    public void TryLoadMapMod_MapNotFound_DoesNotLoadStormMods()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryLoadMapMod("Unknown Map");

        // assert
        result.Should().BeFalse();
        _heroesSource.DidNotReceive().LoadStormMods();
        heroesLoader.IsStormModsLoaded.Should().BeFalse();
    }

    [TestMethod]
    public void TryLoadMapMod_GameStringsLoaded_ReloadsGameStrings()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadGameStrings(StormLocale.KOKR);
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.TryLoadMapMod(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadStormMapMod(MapTitle);
            _heroesSource.LoadGameStrings(StormLocale.KOKR);
        });
    }

    [TestMethod]
    public void TryLoadMapMod_SourceThrows_ThrowsAndUnloadsMap()
    {
        // arrange
        // only a map that is not found returns false, other failures still throw
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);

        _heroesSource.LoadStormMapMod(OtherMapTitle).Returns(_ => throw new IOException("failed"));

        // act
        Action act = () => heroesLoader.TryLoadMapMod(OtherMapTitle);

        // assert
        act.Should().Throw<IOException>();
        heroesLoader.LoadedMapTitle.Should().BeNull();
        _heroesSource.Received(1).UnloadStormMapMod();
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
            _heroesSource.LoadGameStrings(StormLocale.KOKR);
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
        _heroesSource.DidNotReceive().LoadGameStrings(Arg.Any<StormLocale>());
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
    public void FileExists_Path_ReturnsSourceResult()
    {
        // arrange
        _heroesSource.FileExists("core.stormmod/gamedata.xml", null).Returns(true);
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.FileExists("core.stormmod/gamedata.xml");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void FileExists_NullOrWhiteSpacePath_PassedToSource(string? path)
    {
        // arrange
        // the source handles a null or whitespace path
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.FileExists(path);

        // assert
        result.Should().BeFalse();
        _heroesSource.Received(1).FileExists(path, null);
    }

    [TestMethod]
    public void FileExists_NullLiteral_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.FileExists(null);

        // assert
        result.Should().BeFalse();
        _heroesSource.DidNotReceiveWithAnyArgs().FileExists(default!, default);
    }

    [TestMethod]
    public void FileExists_StormFileInMpqArchive_ChecksEntryInArchive()
    {
        // arrange
        StormFile stormFile = CreateMpqStormFile();
        _heroesSource.FileExists(stormFile.MpqFilePath!, stormFile.MpqEntryPath).Returns(true);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.FileExists(stormFile);

        // assert
        result.Should().BeTrue();
        _heroesSource.DidNotReceive().FileExists(stormFile.FullPath, Arg.Any<string?>());
    }

    [TestMethod]
    public void FileExists_StormFileNotInMpqArchive_ChecksFullPath()
    {
        // arrange
        StormFile stormFile = StormFileHelper.CreateStormFile();
        _heroesSource.FileExists(stormFile.FullPath, null).Returns(true);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.FileExists(stormFile);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void GetFile_Path_ReturnsSourceStream()
    {
        // arrange
        using MemoryStream stream = new();
        _heroesSource.GetFile("core.stormmod/gamedata.xml", null).Returns(stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Stream result = heroesLoader.GetFile("core.stormmod/gamedata.xml");

        // assert
        result.Should().BeSameAs(stream);
    }

    [TestMethod]
    public void GetFile_StormFileInMpqArchive_OpensEntryInArchive()
    {
        // arrange
        using MemoryStream stream = new();
        StormFile stormFile = CreateMpqStormFile();
        _heroesSource.GetFile(stormFile.MpqFilePath!, stormFile.MpqEntryPath).Returns(stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Stream result = heroesLoader.GetFile(stormFile);

        // assert
        result.Should().BeSameAs(stream);
    }

    [TestMethod]
    public void GetFile_StormFileNotInMpqArchive_OpensFullPath()
    {
        // arrange
        using MemoryStream stream = new();
        StormFile stormFile = StormFileHelper.CreateStormFile();
        _heroesSource.GetFile(stormFile.FullPath, null).Returns(stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Stream result = heroesLoader.GetFile(stormFile);

        // assert
        result.Should().BeSameAs(stream);
    }

    [TestMethod]
    public void TryGetFile_Path_ReturnsSourceResultAndStream()
    {
        // arrange
        using MemoryStream stream = new();
        SetTryGetFile("core.stormmod/gamedata.xml", null, stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile("core.stormmod/gamedata.xml", out Stream? resultStream);

        // assert
        result.Should().BeTrue();
        resultStream.Should().BeSameAs(stream);
    }

    [TestMethod]
    public void TryGetFile_PathAndMpqEntryPath_PassesEntryPathToSource()
    {
        // arrange
        using MemoryStream stream = new();
        SetTryGetFile("heroesmapmods/test.stormmap", "base.stormdata/gamedata.xml", stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile("heroesmapmods/test.stormmap", "base.stormdata/gamedata.xml", out Stream? resultStream);

        // assert
        result.Should().BeTrue();
        resultStream.Should().BeSameAs(stream);
    }

    [TestMethod]
    public void TryGetFile_SourceDoesNotFindFile_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile("core.stormmod/missing.xml", out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void TryGetFile_NullOrWhiteSpacePath_PassedToSource(string? path)
    {
        // arrange
        // the source handles a null or whitespace path
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile(path, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
        _heroesSource.Received(1).TryGetFile(path, null, out Arg.Any<Stream?>());
    }

    [TestMethod]
    public void TryGetFile_NullStormFile_ReturnsFalseWithoutCallingSource()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile((StormFile?)null, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
        _heroesSource.DidNotReceiveWithAnyArgs().TryGetFile(default!, default, out _);
    }

    [TestMethod]
    public void TryGetFile_StormFileInMpqArchive_OpensEntryInArchive()
    {
        // arrange
        using MemoryStream stream = new();
        StormFile stormFile = CreateMpqStormFile();
        SetTryGetFile(stormFile.MpqFilePath!, stormFile.MpqEntryPath, stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile(stormFile, out Stream? resultStream);

        // assert
        result.Should().BeTrue();
        resultStream.Should().BeSameAs(stream);
        _heroesSource.DidNotReceive().TryGetFile(stormFile.FullPath, Arg.Any<string?>(), out Arg.Any<Stream?>());
    }

    [TestMethod]
    public void TryGetFile_StormFileNotInMpqArchive_OpensFullPath()
    {
        // arrange
        using MemoryStream stream = new();
        StormFile stormFile = StormFileHelper.CreateStormFile();
        SetTryGetFile(stormFile.FullPath, null, stream);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetFile(stormFile, out Stream? resultStream);

        // assert
        result.Should().BeTrue();
        resultStream.Should().BeSameAs(stream);
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
    public void UnloadMapMod_GameStringsLoaded_DoesNotReloadGameStringsAndKeepsLocale()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource, new HeroesLoaderOptions() { MapTitle = MapTitle, GameStringsLocale = StormLocale.KOKR });
        _heroesSource.ClearReceivedCalls();

        // act
        heroesLoader.UnloadMapMod();

        // assert
        _heroesSource.DidNotReceive().LoadGameStrings(Arg.Any<StormLocale>());
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.KOKR);
    }

    [TestMethod]
    public void UnloadMapMod_ThenSameMapLoaded_LoadsMapAgain()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        heroesLoader.LoadMapMod(MapTitle);
        heroesLoader.UnloadMapMod();

        // act
        heroesLoader.LoadMapMod(MapTitle);

        // assert
        _heroesSource.Received(2).LoadStormMapMod(MapTitle);
        heroesLoader.LoadedMapTitle.Should().Be(MapTitle);
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
        _heroesSource.Received(1).LoadGameStrings(StormLocale.ENUS);
        heroesLoader.GameStringsLocale.Should().Be(StormLocale.ENUS);
    }

    [TestMethod]
    public void LoadGameStrings_UndefinedLocale_ThrowsArgumentOutOfRangeExceptionWithoutLoading()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.LoadGameStrings((StormLocale)999);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("locale");
        _heroesSource.DidNotReceive().LoadStormMods();
        _heroesSource.DidNotReceive().LoadGameStrings(Arg.Any<StormLocale>());
    }

    [TestMethod]
    public void LoadGameStrings_EveryDefinedLocale_Loads()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        foreach (StormLocale locale in Enum.GetValues<StormLocale>())
        {
            // act
            heroesLoader.LoadGameStrings(locale);

            // assert
            heroesLoader.GameStringsLocale.Should().Be(locale);
        }
    }

    [TestMethod]
    public void Load_UndefinedGameStringsLocale_ThrowsArgumentOutOfRangeExceptionWithoutLoading()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.Load(new HeroesLoaderOptions() { MapTitle = MapTitle, GameStringsLocale = (StormLocale)999 });

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("options");
        _heroesSource.DidNotReceive().LoadStormMods();
        _heroesSource.DidNotReceive().LoadStormMapMod(Arg.Any<string>());
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
        StormMap result = heroesLoader.GetStormMap(MapTitle);

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
        IReadOnlyList<string> result = heroesLoader.GetMapTitles();

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
    public void GetLoadedStormMapDependencies_MapLoaded_ReturnsNamesAndRelativePathsInLoadOrderWithMapLast()
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
        result.Should().Equal(
            new StormMapDependency() { Name = "volskayasound", RelativePath = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"), IsMap = false },
            new StormMapDependency() { Name = MapTitle, RelativePath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma"), IsMap = true });
    }

    [TestMethod]
    public void GetLoadedStormMapDependencies_CalledTwice_ReturnsNewListsWithEqualItems()
    {
        // arrange
        IStormMod mapRootStormMod = Substitute.For<IStormMod>();
        mapRootStormMod.Name.Returns(MapTitle);
        mapRootStormMod.RelativeStormModPath.Returns(Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma"));

        _heroesSource.StormMapMods.Returns([mapRootStormMod]);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        IReadOnlyList<StormMapDependency> first = heroesLoader.GetLoadedStormMapDependencies();
        IReadOnlyList<StormMapDependency> second = heroesLoader.GetLoadedStormMapDependencies();

        // assert
        second.Should().NotBeSameAs(first);
        second.Should().Equal(first);
        second.Should().ContainSingle().Which.IsMap.Should().BeTrue();
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
    public void GetLoadedStormModContainers_ContainersLoaded_ReturnsContainersInLayerOrder()
    {
        // arrange
        SourceContainer sourceContainer = new();
        _heroesSource.SourceContainer.Returns(sourceContainer);

        StormModContainer baseContainer = AddModContainer(sourceContainer, StormModType.Base);
        StormModContainer customContainer = AddModContainer(sourceContainer, StormModType.Custom);
        StormModContainer mapContainer = AddModContainer(sourceContainer, StormModType.Map);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        IReadOnlyList<StormModContainer> result = heroesLoader.GetLoadedStormModContainers();

        // assert
        result.Should().Equal(baseContainer, mapContainer, customContainer);
    }

    [TestMethod]
    public void GetLoadedStormModContainers_ContainerAddedAfterCall_ReturnedListDoesNotChange()
    {
        // arrange
        SourceContainer sourceContainer = new();
        _heroesSource.SourceContainer.Returns(sourceContainer);

        StormModContainer baseContainer = AddModContainer(sourceContainer, StormModType.Base);

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);
        IReadOnlyList<StormModContainer> result = heroesLoader.GetLoadedStormModContainers();

        // act
        AddModContainer(sourceContainer, StormModType.Map);

        // assert
        result.Should().Equal(baseContainer);
        heroesLoader.GetLoadedStormModContainers().Should().NotBeSameAs(result).And.HaveCount(2);
    }

    [TestMethod]
    public void GetLoadedStormModContainers_NothingLoaded_ReturnsEmpty()
    {
        // arrange
        _heroesSource.SourceContainer.Returns(new SourceContainer());
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        IReadOnlyList<StormModContainer> result = heroesLoader.GetLoadedStormModContainers();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetStormMap_MapNotFound_ThrowsKeyNotFoundException()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.GetStormMap("Unknown Map");

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*Unknown Map*");
    }

    [TestMethod]
    public void GetStormMap_NoS2MVProperties_ThrowsKeyNotFoundException()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties());
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.GetStormMap(MapTitle);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void GetStormMap_NullOrWhiteSpaceMapTitle_ThrowsArgumentException(string? mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        Action act = () => heroesLoader.GetStormMap(mapTitle!);

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("mapTitle");
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void StormMapExists_MapFound_ReturnsTrue()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.StormMapExists(MapTitle);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormMapExists_MapNotFound_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.StormMapExists("Unknown Map");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void StormMapExists_NoS2MVProperties_ReturnsFalse()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties());
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.StormMapExists(MapTitle);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void StormMapExists_NullOrWhiteSpaceMapTitle_ReturnsFalseWithoutLoadingDepotCache(string? mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.StormMapExists(mapTitle);

        // assert
        result.Should().BeFalse();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void StormMapExists_NothingLoaded_LoadsOnlyDepotCacheFirst()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        heroesLoader.StormMapExists(MapTitle);

        // assert
        Received.InOrder(() =>
        {
            _heroesSource.LoadDepotCache();
            _heroesSource.TryGetS2MAProperties(MapTitle, out Arg.Any<S2MAProperties?>());
        });
        _heroesSource.DidNotReceive().LoadStormMods();
    }

    [TestMethod]
    public void TryGetStormMap_MapFound_ReturnsTrueAndMap()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties()
        {
            MapId = "Volskaya",
            S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle },
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetStormMap(MapTitle, out StormMap? stormMap);

        // assert
        result.Should().BeTrue();
        stormMap.Should().NotBeNull();
        stormMap.Name.Should().Be(MapTitle);
        stormMap.MapId.Should().Be("Volskaya");
    }

    [TestMethod]
    public void TryGetStormMap_MapNotFound_ReturnsFalse()
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetStormMap("Unknown Map", out StormMap? stormMap);

        // assert
        result.Should().BeFalse();
        stormMap.Should().BeNull();
    }

    [TestMethod]
    public void TryGetStormMap_NoS2MVProperties_ReturnsFalse()
    {
        // arrange
        SetS2MAProperties(MapTitle, new S2MAProperties());
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetStormMap(MapTitle, out StormMap? stormMap);

        // assert
        result.Should().BeFalse();
        stormMap.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void TryGetStormMap_NullOrWhiteSpaceMapTitle_ReturnsFalseWithoutLoadingDepotCache(string? mapTitle)
    {
        // arrange
        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetStormMap(mapTitle, out StormMap? stormMap);

        // assert
        result.Should().BeFalse();
        stormMap.Should().BeNull();
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    [TestMethod]
    public void TryGetStormMap_MapTitleInDifferentCase_NameIsTheMapsTitle()
    {
        // arrange
        SetS2MAProperties(MapTitle.ToLowerInvariant(), new S2MAProperties()
        {
            S2MVProperties = new S2MVProperties() { HeaderTitle = MapTitle },
        });

        HeroesLoader heroesLoader = HeroesLoader.LoadWithInternal(_heroesSource);

        // act
        bool result = heroesLoader.TryGetStormMap(MapTitle.ToLowerInvariant(), out StormMap? stormMap);

        // assert
        result.Should().BeTrue();
        stormMap!.Name.Should().Be(MapTitle);
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
        StormMap result = heroesLoader.GetStormMap(MapTitle);

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
        StormMap result = heroesLoader.GetStormMap(MapTitle.ToLowerInvariant());

        // assert
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
            _heroesSource.LoadGameStrings(StormLocale.ENUS);
        });
        _heroesSource.DidNotReceive().LoadDepotCache();
    }

    // a real directory, because LoadWithFile checks the real file system
    private static string CreateTempDirectory() =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), nameof(HeroesLoaderTests), Guid.NewGuid().ToString("N"))).FullName;

    private static StormFile CreateMpqStormFile() => new()
    {
        StormModName = "test",
        RelativePath = Path.Combine("heroesmapmods", "test.stormmap", "base.stormdata", "gamedata.xml"),
        FullPath = Path.Combine("mods", "heroesmapmods", "test.stormmap", "base.stormdata", "gamedata.xml"),
        MpqFilePath = Path.Combine("mods", "heroesmapmods", "test.stormmap"),
        MpqEntryPath = Path.Combine("base.stormdata", "gamedata.xml"),
    };

    private static StormModContainer AddModContainer(SourceContainer sourceContainer, StormModType stormModType)
    {
        StormModContainer stormModContainer = new(sourceContainer, stormModType);
        sourceContainer.AddModContainer(stormModContainer);

        return stormModContainer;
    }

    private void SetTryGetFile(string path, string? mpqEntryPath, Stream stream)
    {
        _heroesSource.TryGetFile(path, mpqEntryPath, out Arg.Any<Stream?>()).Returns(x =>
        {
            x[2] = stream;
            return true;
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
