namespace Heroes.StormData.Source.Tests;

[TestClass]
public sealed class CASCHeroesSourceTests
{
    private const string ModsDirectory = "mods";
    private const string MapTitle = "Volskaya Foundry";

    private static readonly string _testFilesDirectoryPath = Path.Combine("TestFiles", "DepotCache");
    private static readonly string _dependencyLocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod");

    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly ICASCHeroesStorage _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
    private readonly CASCFolder _cascFolderRoot = new("root");
    private readonly Dictionary<string, IStormMod> _stormModsByDirectoryPath = new(StringComparer.OrdinalIgnoreCase);

    public CASCHeroesSourceTests()
    {
        _cascHeroesStorage.CASCFolderRoot.Returns(_cascFolderRoot);

        _stormModFactory.CreateCASCStormMod(default!, default!, default)
            .ReturnsForAnyArgs(callInfo => CreateStormMod(callInfo.ArgAt<ICASCHeroesSource>(0), callInfo.ArgAt<string>(1), callInfo.ArgAt<StormModType>(2)));

        _stormModFactory.CreateCASCMpqStormMod(default!, default!, default!, default)
            .ReturnsForAnyArgs(callInfo => CreateStormMod(callInfo.ArgAt<ICASCHeroesSource>(0), callInfo.ArgAt<string>(2), callInfo.ArgAt<StormModType>(3)));
    }

    [TestMethod]
    public void Constructor_ModsDirectoryPath_CreatesBaseCASCStormModsInLoadOrder()
    {
        // act
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // assert
        Received.InOrder(() =>
        {
            _stormModFactory.CreateCASCStormMod(cascHeroesSource, Path.Combine(ModsDirectory, "core.stormmod"), StormModType.Base);
            _stormModFactory.CreateCASCStormMod(cascHeroesSource, Path.Combine(ModsDirectory, "heroes.stormmod"), StormModType.Base);
            _stormModFactory.CreateCASCStormMod(cascHeroesSource, Path.Combine(ModsDirectory, "heroesdata.stormmod"), StormModType.Base);
        });

        _stormModFactory.ReceivedWithAnyArgs(3).CreateCASCStormMod(default!, default!, default);
        _stormModFactory.DidNotReceiveWithAnyArgs().CreateFileStormMod(default!, default!, default);
    }

    [TestMethod]
    public void Constructor_Arguments_SetsProperties()
    {
        // act
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // assert
        cascHeroesSource.ModsDirectoryPath.Should().Be(ModsDirectory);
        cascHeroesSource.StormModFactory.Should().BeSameAs(_stormModFactory);
        cascHeroesSource.CASCHeroesStorage.Should().BeSameAs(_cascHeroesStorage);
        cascHeroesSource.SourceContainer.Should().NotBeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void GetCASCFolder_NoDirectory_ReturnsRootFolder(string? directory)
    {
        // arrange
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        CASCFolder result = cascHeroesSource.GetCASCFolder(directory);

        // assert
        result.Should().BeSameAs(_cascFolderRoot);
    }

    [TestMethod]
    public void GetCASCFolder_NoArgument_ReturnsRootFolder()
    {
        // arrange
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        CASCFolder result = cascHeroesSource.GetCASCFolder();

        // assert
        result.Should().BeSameAs(_cascFolderRoot);
    }

    [TestMethod]
    public void GetCASCFolder_DirectoryExists_ReturnsLastFolderOfDirectory()
    {
        // arrange
        _cascFolderRoot.AddDirectory("mods/core.stormmod/base.stormdata/gamedata");

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        CASCFolder result = cascHeroesSource.GetCASCFolder(Path.Join(ModsDirectory, "core.stormmod", "base.stormdata"));

        // assert
        result.Name.Should().Be("base.stormdata");
        result.Folders.Should().ContainSingle().Which.Key.Should().Be("gamedata");
    }

    [TestMethod]
    public void GetCASCFolder_SingleDirectoryExists_ReturnsFolder()
    {
        // arrange
        _cascFolderRoot.AddDirectory("mods/core.stormmod");

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        CASCFolder result = cascHeroesSource.GetCASCFolder(ModsDirectory);

        // assert
        result.Should().BeSameAs(_cascFolderRoot.GetFolder(ModsDirectory));
    }

    [TestMethod]
    public void GetCASCFolder_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // arrange
        _cascFolderRoot.AddDirectory("mods/core.stormmod");

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        string directory = Path.Join(ModsDirectory, "heroes.stormmod");

        // act
        Action act = () => cascHeroesSource.GetCASCFolder(directory);

        // assert
        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage($"Could not find folder: {directory}");
    }

    [TestMethod]
    public void LoadStormMods_BaseStormMods_AddsContainersAndLoadsStormFilesInOrder()
    {
        // arrange
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        IStormMod coreStormMod = _stormModsByDirectoryPath[Path.Combine(ModsDirectory, "core.stormmod")];
        IStormMod heroesStormMod = _stormModsByDirectoryPath[Path.Combine(ModsDirectory, "heroes.stormmod")];
        IStormMod heroesDataStormMod = _stormModsByDirectoryPath[Path.Combine(ModsDirectory, "heroesdata.stormmod")];

        // act
        cascHeroesSource.LoadStormMods();

        // assert
        Received.InOrder(() =>
        {
            coreStormMod.LoadStormFiles();
            heroesStormMod.LoadStormFiles();
            heroesDataStormMod.LoadStormFiles();
        });

        cascHeroesSource.SourceContainer.StormModContainers.Should().Equal(coreStormMod.ModContainer, heroesStormMod.ModContainer, heroesDataStormMod.ModContainer);
    }

    [TestMethod]
    public void LoadDepotCache_DepotCacheFilesInCASC_LoadsMapTitles()
    {
        // arrange
        AddVolskayaDepotCacheFiles();

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        cascHeroesSource.LoadDepotCache();

        // assert
        cascHeroesSource.GetMapTitles().Should().ContainSingle().Which.Should().Be(MapTitle);
    }

    [TestMethod]
    public void LoadDepotCache_NoDepotCacheFolderInCASC_HasNoMapTitles()
    {
        // arrange
        _cascFolderRoot.AddDirectory("mods/core.stormmod/base.stormdata");

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        cascHeroesSource.LoadDepotCache();

        // assert
        cascHeroesSource.GetMapTitles().Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_CreatesCASCMpqStormModForMapRoot()
    {
        // arrange
        string mapFilePath = AddVolskayaDepotCacheFiles();

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();
        cascHeroesSource.LoadDepotCache();

        // act
        bool result = cascHeroesSource.LoadStormMapMod(MapTitle);

        // assert
        result.Should().BeTrue();

        _stormModFactory.Received(1).CreateCASCMpqStormMod(cascHeroesSource, MapTitle, Path.Join(ModsDirectory, mapFilePath), StormModType.Map);
        _stormModFactory.DidNotReceiveWithAnyArgs().CreateFileMpqStormMod(default!, default!, default!, default);
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_CreatesCASCStormModsForDependencies()
    {
        // arrange
        AddVolskayaDepotCacheFiles();

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();
        cascHeroesSource.LoadDepotCache();

        // act
        cascHeroesSource.LoadStormMapMod(MapTitle);

        // assert
        _stormModFactory.Received(1).CreateCASCStormMod(cascHeroesSource, Path.Join(ModsDirectory, _dependencyLocalFile), StormModType.Map);
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_LoadsDependenciesBeforeMapRoot()
    {
        // arrange
        string mapFilePath = AddVolskayaDepotCacheFiles();

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();
        cascHeroesSource.LoadDepotCache();

        // act
        cascHeroesSource.LoadStormMapMod(MapTitle);

        // assert
        IStormMod dependencyStormMod = _stormModsByDirectoryPath[Path.Join(ModsDirectory, _dependencyLocalFile)];
        IStormMod mapRootStormMod = _stormModsByDirectoryPath[Path.Join(ModsDirectory, mapFilePath)];

        Received.InOrder(() =>
        {
            dependencyStormMod.LoadStormFiles();
            mapRootStormMod.LoadStormFiles();
        });

        cascHeroesSource.SourceContainer.StormModContainers.Should().Equal(dependencyStormMod.ModContainer, mapRootStormMod.ModContainer);
    }

    [TestMethod]
    public void LoadStormMapMod_DepotCacheNotLoaded_ReturnsFalse()
    {
        // arrange
        AddVolskayaDepotCacheFiles();

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        bool result = cascHeroesSource.LoadStormMapMod(MapTitle);

        // assert
        result.Should().BeFalse();
        _stormModFactory.DidNotReceiveWithAnyArgs().CreateCASCMpqStormMod(default!, default!, default!, default);
    }

    [TestMethod]
    [DataRow("core.stormmod/base.stormdata/gamedata.xml")]
    [DataRow("Mods/Core.StormMod/Base.StormData/GameData.xml")]
    public void FileExists_PathRelativeOrStartingWithMods_ChecksPathUnderModsDirectory(string path)
    {
        // arrange
        string expectedPath = Path.Combine(ModsDirectory, "core.stormmod", "base.stormdata", "gamedata.xml");
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(expectedPath).Returns(true);

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        bool result = cascHeroesSource.FileExists(path, null);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_FileNotFound_ReturnsFalse()
    {
        // arrange
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        bool result = cascHeroesSource.FileExists("core.stormmod/missing.xml", null);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_FileExists_ReturnsStream()
    {
        // arrange
        string expectedPath = Path.Combine(ModsDirectory, "core.stormmod", "base.stormdata", "gamedata.xml");
        using MemoryStream fileStream = new([1, 2, 3]);

        _cascHeroesStorage.CASCHandlerWrapper.FileExists(expectedPath).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(expectedPath).Returns(fileStream);

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        Stream result = cascHeroesSource.GetFile("mods/core.stormmod/base.stormdata/gamedata.xml", null);

        // assert
        result.Should().BeSameAs(fileStream);
    }

    [TestMethod]
    public void GetFile_FileNotFound_ThrowsExceptionWithoutOpeningFile()
    {
        // arrange
        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        Action act = () => cascHeroesSource.GetFile("core.stormmod/missing.xml", null);

        // assert
        act.Should().Throw<FileNotFoundException>();
        _cascHeroesStorage.CASCHandlerWrapper.DidNotReceiveWithAnyArgs().OpenFile(default!);
    }

    [TestMethod]
    public void GetFile_MpqEntryInCASCFile_ReturnsEntryContent()
    {
        // arrange
        string archivePath = Path.Combine(ModsDirectory, "heroesmapmods", "test.stormmap");

        _cascHeroesStorage.CASCHandlerWrapper.FileExists(archivePath).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(archivePath).Returns(_ => new MemoryStream(File.ReadAllBytes(Path.Join("TestFiles", "Mpq", "test.s2ma"))));

        CASCHeroesSource cascHeroesSource = CreateCASCHeroesSource();

        // act
        using Stream result = cascHeroesSource.GetFile(archivePath, "Base.StormData/GameData/ActorData.xml");

        // assert
        new StreamReader(result).ReadToEnd().Should().Contain("<Catalog");
    }

    private CASCHeroesSource CreateCASCHeroesSource() => new(ModsDirectory, _stormModFactory, _cascHeroesStorage);

    /// <summary>
    /// Adds the Volskaya Foundry <c>.s2mv</c> and <c>.s2ma</c> files to the CASC depot cache folder.
    /// </summary>
    /// <returns>The path of the <c>.s2ma</c> file, relative to the mods directory.</returns>
    private string AddVolskayaDepotCacheFiles()
    {
        AddDepotCacheFile("94", "a0", "94a00.s2mv");
        AddDepotCacheFile("8d", "55", "8d554.s2ma");

        return Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma");
    }

    private void AddDepotCacheFile(string folder1, string folder2, string fileName)
    {
        string cascFilePath = string.Join('/', ModsDirectory, "core.stormmod", "base.stormdata", "depotcache", folder1, folder2, fileName);

        _cascFolderRoot.AddFile(cascFilePath);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(cascFilePath).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(cascFilePath).Returns(_ => File.OpenRead(Path.Join(_testFilesDirectoryPath, fileName)));
    }

    private IStormMod CreateStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
    {
        IStormMod stormMod = Substitute.For<IStormMod>();
        stormMod.DirectoryPath.Returns(directoryPath);
        stormMod.ModContainer.Returns(new StormModContainer(cascHeroesSource.SourceContainer, stormModType));
        stormMod.GetMapDependencies().Returns([]);

        _stormModsByDirectoryPath[directoryPath] = stormMod;

        return stormMod;
    }
}
