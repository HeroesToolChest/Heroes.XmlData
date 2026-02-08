namespace Heroes.XmlData.Tests;

[TestClass]
public class HeroesXmlLoaderTests
{
    private readonly IHeroesSource _heroesSource;

    public HeroesXmlLoaderTests()
    {
        _heroesSource = Substitute.For<IHeroesSource>();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("mods")]
    [DataRow("mods_1234")]
    [DataRow("other")]
    public void LoadWithEmptyStatic_InputRootDirectory_SetsRootDirectory(string? rootDirectory)
    {
        // arrange
        string expectedRootDirectory = rootDirectory ?? string.Empty;

        // act
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithEmpty(rootDirectory);

        // assert
        heroesXmlLoader.RootDirectory.Should().Be(expectedRootDirectory);
        heroesXmlLoader.LoadedType.Should().Be(HeroesXmlLoaderType.File);
    }

    [TestMethod]
    public void LoadStormMods_InitialLoad_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource);

        // act
        heroesXmlLoader.LoadStormMods(); // initial load

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
    }

    [TestMethod]
    public void LoadStormMods_RepeatedLoad_DoesNotReload()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods(); // initial load

        // act
        heroesXmlLoader.LoadStormMods(); // load it again

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
    }

    [TestMethod]
    public void LoadMapMod_InitialLoad_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource);

        // act
        heroesXmlLoader.LoadMapMod("map"); // initial load

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadStormMapData(Arg.Any<string>());
        _heroesSource.DidNotReceive().LoadGamestrings(Arg.Any<StormLocale>());
        heroesXmlLoader.LoadedMapTitle.Should().Be("map");
    }

    [TestMethod]
    public void LoadMapMod_InitialLoadWithLoadStormMods_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods(); // load

        // act
        heroesXmlLoader.LoadMapMod("map"); // initial load

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadStormMapData(Arg.Any<string>());
        _heroesSource.DidNotReceive().LoadGamestrings(Arg.Any<StormLocale>());
        heroesXmlLoader.LoadedMapTitle.Should().Be("map");
    }

    [TestMethod]
    public void LoadMapMod_ReloadSameMap_DoesNotReload()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadMapMod("map"); // initial load

        // act
        heroesXmlLoader.LoadMapMod("Map"); // reload same map load

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadStormMapData(Arg.Any<string>());
        _heroesSource.DidNotReceive().LoadGamestrings(Arg.Any<StormLocale>());
        heroesXmlLoader.LoadedMapTitle.Should().Be("map");
    }

    [TestMethod]
    public void LoadMapMod_LoadNewMap_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadMapMod("map"); // initial load

        // act
        heroesXmlLoader.LoadMapMod("OtherMap");

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(2).LoadStormMapData(Arg.Any<string>());
        _heroesSource.DidNotReceive().LoadGamestrings(Arg.Any<StormLocale>());
        heroesXmlLoader.LoadedMapTitle.Should().Be("OtherMap");
    }

    [TestMethod]
    public void LoadMapMod_HasAlreadyLoadedGameStrings_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadGameStrings(StormLocale.ENUS); // initial load

        // act
        heroesXmlLoader.LoadMapMod("OtherMap");

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadStormMapData(Arg.Any<string>());
        _heroesSource.Received().LoadGamestrings(Arg.Any<StormLocale>());
    }

    [TestMethod]
    public void UnloadMapMod_UnloadsMap_Unloads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource);

        // act
        heroesXmlLoader.UnloadMapMod();

        // assert
        _heroesSource.Received(1).LoadStormMapData(string.Empty);
        heroesXmlLoader.LoadedMapTitle.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameStrings_InitialLoad_Loads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource);

        // act
        heroesXmlLoader.LoadGameStrings(StormLocale.ENUS);

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadGamestrings(StormLocale.ENUS);
        heroesXmlLoader.CurrentStormLocale.Should().Be(StormLocale.ENUS);
    }

    [TestMethod]
    public void LoadGameStrings_ReloadSameLocale_Reloads()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadGameStrings(StormLocale.ENUS);

        // act
        heroesXmlLoader.LoadGameStrings(StormLocale.ENUS);

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(2).LoadGamestrings(StormLocale.ENUS);
        heroesXmlLoader.CurrentStormLocale.Should().Be(StormLocale.ENUS);
    }

    [TestMethod]
    public void LoadGameStrings_ReloadDifferentLocale_LoadNewLocale()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadGameStrings(StormLocale.ENUS);

        // act
        heroesXmlLoader.LoadGameStrings(StormLocale.ITIT);

        // assert
        _heroesSource.Received(1).LoadStormData();
        _heroesSource.Received(1).LoadDepotCache();
        _heroesSource.Received(1).LoadGamestrings(StormLocale.ENUS);
        _heroesSource.Received(1).LoadGamestrings(StormLocale.ITIT);
        heroesXmlLoader.CurrentStormLocale.Should().Be(StormLocale.ITIT);
    }

    [TestMethod]
    public void LoadCustomMod_ManualModLoader_LoadsCustomMod()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        heroesXmlLoader.LoadCustomMod(new ManualModLoader("test"));

        // assert
        _heroesSource.Received(1).LoadCustomMod(Arg.Any<IStormMod>());
    }

    [TestMethod]
    public void LoadCustomMod_StormModDirectoryPath_LoadsCustomMod()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        heroesXmlLoader.LoadCustomMod("pathToStormMod");

        // assert
        _heroesSource.Received(1).LoadCustomMod(Arg.Any<string>());
    }

    [TestMethod]
    public void UnloadCustomMods_Unload_UnloadCustomMods()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        heroesXmlLoader.UnloadCustomMods();

        // assert
        _heroesSource.Received(1).UnloadCustomMods();
    }

    [TestMethod]
    public void GetMapTitles_HasMapTitles_ReturnsCount()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.S2MAPropertiesByTitle.Returns(new Dictionary<string, S2MAProperties>()
        {
            { "map1", new S2MAProperties() },
            { "map2", new S2MAProperties() },
        });

        // act
        List<string> mapTitles = heroesXmlLoader.GetMapTitles().ToList();

        // assert
        mapTitles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetStormMap_MapNotFound_ReturnsNull()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.S2MAPropertiesByTitle.Returns(new Dictionary<string, S2MAProperties>()
        {
            { "map1", new S2MAProperties() },
            { "map2", new S2MAProperties() },
        });

        // act
        StormMap? map = heroesXmlLoader.GetStormMap("MapTitle");

        // assert
        map.Should().BeNull();
    }

    [TestMethod]
    public void GetStormMap_NotS2mvProperties_ReturnsNull()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.S2MAPropertiesByTitle.Returns(new Dictionary<string, S2MAProperties>()
        {
            { "map1", new S2MAProperties() },
            { "map2", new S2MAProperties() },
        });

        // act
        StormMap? map = heroesXmlLoader.GetStormMap("map1");

        // assert
        map.Should().BeNull();
    }

    [TestMethod]
    public void GetStormMap_MapFound_ReturnsMap()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.S2MAPropertiesByTitle.Returns(new Dictionary<string, S2MAProperties>()
        {
            { "map1", new S2MAProperties() { S2MVProperties = new S2MVProperties() } },
            { "map2", new S2MAProperties() },
        });

        // act
        StormMap? map = heroesXmlLoader.GetStormMap("map1");

        // assert
        map.Should().NotBeNull();
    }

    [TestMethod]
    public void GetLoadedStormMapDependencies_HasMapDependencies_ReturnsDependencies()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.GetMapDependencies().Returns(new List<StormMapDependency>()
        {
            new()
            {
                Name = "map1",
                DirectoryPath = "path1",
            },
            new()
            {
                Name = "map2",
                DirectoryPath = "path2",
            },
        });

        // act
        List<StormMapDependency> mapDependencies = heroesXmlLoader.GetLoadedStormMapDependencies().ToList();

        // assert
        mapDependencies.Should().HaveCount(2);
    }

    [TestMethod]
    public void IsMapModLoaded_MapModIsLoaded_ReturnsTrue()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.IsMapModLoaded().Returns(true);

        // act
        bool isLoaded = heroesXmlLoader.IsMapModLoaded();

        // assert
        isLoaded.Should().BeTrue();
    }

    [TestMethod]
    public void GetLoadedMapModTitle_HasMapLoaded_ReturnsName()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.LoadedStormMapTitle().Returns("map1");

        // act
        string? mapTitle = heroesXmlLoader.GetLoadedMapModTitle();

        // assert
        mapTitle.Should().Be("map1");
    }

    [TestMethod]
    public void GetLoadedMapModTitle_HasNoMap_ReturnsNull()
    {
        // arrange
        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        _heroesSource.LoadedStormMapTitle().Returns((string?)null);

        // act
        string? mapTitle = heroesXmlLoader.GetLoadedMapModTitle();

        // assert
        mapTitle.Should().BeNull();
    }

    [TestMethod]
    public void GetCountOfNotFoundDirectories_StormModStoragesHaveNotFoundDirectories_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfNotFoundDirectories.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfNotFoundDirectories.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfNotFoundDirectories.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int numOfNotFoundDirectories = heroesXmlLoader.GetCountOfNotFoundDirectories();

        // assert
        numOfNotFoundDirectories.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfNotFoundFiles_StormModStoragesHaveNotFoundFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfNotFoundFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfNotFoundFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfNotFoundFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfNotFoundFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfXmlDataFiles_StormModStoragesHaveXmlDataFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfXmlDataFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfXmlDataFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfXmlDataFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfXmlDataFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfFontStyleFiles_StormModStoragesHaveFontStyleFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfXmlFontStyleFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfXmlFontStyleFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfXmlFontStyleFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfFontStyleFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfGameStringsFiles_StormModStoragesHaveGameStringFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfGameStringFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfGameStringFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfGameStringFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfGameStringsFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfAssetsTextFiles_StormModStoragesHaveAssetsTextFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfAssetsTextFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfAssetsTextFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfAssetsTextFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfAssetsTextFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfLayoutFiles_StormModStoragesHaveLayoutFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfLayoutFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfLayoutFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfLayoutFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfLayoutFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetCountOfAssetFiles_StormModStoragesHaveAssetFiles_ReturnsCount()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NumberOfAssetFiles.Returns(1);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NumberOfAssetFiles.Returns(2);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NumberOfAssetFiles.Returns(3);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        int count = heroesXmlLoader.GetCountOfAssetFiles();

        // assert
        count.Should().Be(6);
    }

    [TestMethod]
    public void GetNotFoundDirectories_StormModStoragesHaveNotFoundDirectories_ReturnsPaths()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NotFoundDirectories.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NotFoundDirectories.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NotFoundDirectories.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        List<StormPath> count = heroesXmlLoader.GetNotFoundDirectories().ToList();

        // assert
        count.Should().HaveCount(6);
    }

    [TestMethod]
    public void GetNotFoundFiles_StormModStoragesHaveNotFoundFiles_ReturnsPaths()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.NotFoundFiles.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.NotFoundFiles.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.NotFoundFiles.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        List<StormPath> notFoundFiles = heroesXmlLoader.GetNotFoundFiles().ToList();

        // assert
        notFoundFiles.Should().HaveCount(6);
    }

    [TestMethod]
    public void GetDataFilePaths_HasFiles_ReturnsPaths()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.AddedXmlDataFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.AddedXmlFontStyleFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.AddedGameStringFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.AddedAssetsTextFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.AddedXmlDataFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.AddedXmlFontStyleFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.AddedGameStringFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.AddedAssetsTextFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.AddedXmlDataFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage3.AddedXmlFontStyleFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage3.AddedGameStringFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage3.AddedAssetsTextFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage3.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage2.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        List<StormPath> dataFilePaths = heroesXmlLoader.GetDataFilePaths().ToList();

        // assert
        dataFilePaths.Should().HaveCount(30);
    }

    [TestMethod]
    public void GetAssetFilePaths_HasFiles_ReturnsPaths()
    {
        // arrange
        IStormModStorage stormModStorage1 = Substitute.For<IStormModStorage>();
        stormModStorage1.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);
        stormModStorage1.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage2 = Substitute.For<IStormModStorage>();
        stormModStorage2.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);
        stormModStorage2.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
        ]);

        IStormModStorage stormModStorage3 = Substitute.For<IStormModStorage>();
        stormModStorage3.FoundLayoutFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
            TestHelpers.GetStormPath("path3"),
        ]);
        stormModStorage3.FoundAssetFilePaths.Returns(
        [
            TestHelpers.GetStormPath("path1"),
            TestHelpers.GetStormPath("path2"),
        ]);

        _heroesSource.StormStorage.StormModStorages.Returns(
        [
             stormModStorage1,
             stormModStorage2,
             stormModStorage3,
        ]);

        HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader
            .LoadWithInternal(_heroesSource)
            .LoadStormMods();

        // act
        List<StormPath> dataFilePaths = heroesXmlLoader.GetAssetFilePaths().ToList();

        // assert
        dataFilePaths.Should().HaveCount(4);
    }

    [TestMethod]
    public void GetInfoFile_FileDoesNotExists_ReturnsNull()
    {
        // arrange
        string rootDirectory = Path.Combine("test", nameof(HeroesXmlLoaderTests), "mods");

        // act
        ModsInfoFile? result = HeroesXmlLoader.GetModsInfoFile(rootDirectory);

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetInfoFile_FileExistsWithNoVersionProperty_PropertyReturnsNull()
    {
        // arrange
        string rootDirectory = Path.Combine("TestFiles", "modsNoVersion");

        // act
        ModsInfoFile? result = HeroesXmlLoader.GetModsInfoFile(rootDirectory);

        // assert
        result.Should().NotBeNull();
        result.Version.Should().BeNull();
        result.IsPtr.Should().BeFalse();
    }

    [TestMethod]
    public void GetInfoFile_FileExistsWithProperties_ReturnsFileInfo()
    {
        // arrange
        string rootDirectory = Path.Combine("TestFiles", "modsProperties");

        // act
        ModsInfoFile? result = HeroesXmlLoader.GetModsInfoFile(rootDirectory);

        // assert
        result.Should().NotBeNull();
        result.Version.Should().Be("2.55.13.95301");
        result.IsPtr.Should().BeTrue();
    }
}