namespace Heroes.StormData.StormMods.Tests;

[TestClass]
public sealed class StormModFactoryTests
{
    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
    private static readonly string _stormModDirectoryPath = Path.Combine(_modsDirectoryPath, "heroesdata.stormmod");
    private static readonly string _mapFilePath = Path.Combine(_modsDirectoryPath, "core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma");

    private readonly MockFileSystem _fileSystem = new();
    private readonly SourceContainer _sourceContainer = new();
    private readonly IFileHeroesSource _fileHeroesSource = Substitute.For<IFileHeroesSource>();
    private readonly ICASCHeroesSource _cascHeroesSource = Substitute.For<ICASCHeroesSource>();
    private readonly StormModFactory _stormModFactory;

    public StormModFactoryTests()
    {
        _fileHeroesSource.ModsDirectoryPath.Returns(_modsDirectoryPath);
        _fileHeroesSource.SourceContainer.Returns(_sourceContainer);

        _cascHeroesSource.ModsDirectoryPath.Returns(_modsDirectoryPath);
        _cascHeroesSource.SourceContainer.Returns(_sourceContainer);

        _stormModFactory = new StormModFactory(_fileSystem);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void CreateFileStormMod_StormModDirectory_ReturnsFileStormMod(StormModType stormModType)
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateFileStormMod(_fileHeroesSource, _stormModDirectoryPath, stormModType);

        // assert
        stormMod.Should().BeOfType<FileStormMod>();
        stormMod.Name.Should().Be("heroesdata");
        stormMod.DirectoryPath.Should().Be(_stormModDirectoryPath);
        stormMod.RelativeStormModPath.Should().Be("heroesdata.stormmod");
        stormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    public void CreateFileStormMod_FileInFactoryFileSystem_LoadsFileFromFactoryFileSystem()
    {
        // arrange
        string xmlFilePath = Path.Combine(_stormModDirectoryPath, "base.stormdata", "gamedata", "unitdata.xml");
        _fileSystem.AddFile(xmlFilePath, new MockFileData("<Catalog />"));

        IStormMod stormMod = _stormModFactory.CreateFileStormMod(_fileHeroesSource, _stormModDirectoryPath, StormModType.Base);

        // act
        stormMod.LoadStormFiles();

        // assert
        stormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.FullPath.Should().Be(xmlFilePath);
    }

    [TestMethod]
    public void CreateFileStormMod_CalledTwice_ReturnsNewInstanceEachTime()
    {
        // act
        IStormMod first = _stormModFactory.CreateFileStormMod(_fileHeroesSource, _stormModDirectoryPath, StormModType.Base);
        IStormMod second = _stormModFactory.CreateFileStormMod(_fileHeroesSource, _stormModDirectoryPath, StormModType.Base);

        // assert
        second.Should().NotBeSameAs(first);
        second.ModContainer.Should().NotBeSameAs(first.ModContainer);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void CreateCASCStormMod_StormModDirectory_ReturnsCASCStormMod(StormModType stormModType)
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateCASCStormMod(_cascHeroesSource, _stormModDirectoryPath, stormModType);

        // assert
        stormMod.Should().BeOfType<CASCStormMod>();
        stormMod.Name.Should().Be("heroesdata");
        stormMod.DirectoryPath.Should().Be(_stormModDirectoryPath);
        stormMod.RelativeStormModPath.Should().Be("heroesdata.stormmod");
        stormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void CreateFileMpqStormMod_NoName_ReturnsFileMpqStormModNamedAfterFile(StormModType stormModType)
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateFileMpqStormMod(_fileHeroesSource, _mapFilePath, stormModType);

        // assert
        stormMod.Should().BeOfType<FileMpqStormMod>();
        stormMod.Name.Should().Be("8d554");
        stormMod.DirectoryPath.Should().Be(_mapFilePath);
        stormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    public void CreateFileMpqStormMod_HasName_ReturnsFileMpqStormModWithName()
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateFileMpqStormMod(_fileHeroesSource, "Volskaya Foundry", _mapFilePath, StormModType.Map);

        // assert
        stormMod.Should().BeOfType<FileMpqStormMod>();
        stormMod.Name.Should().Be("Volskaya Foundry");
        stormMod.DirectoryPath.Should().Be(_mapFilePath);
        stormMod.ModContainer.StormModType.Should().Be(StormModType.Map);
    }

    [TestMethod]
    public void CreateFileMpqStormMod_ArchiveInFactoryFileSystem_OpensArchiveFromFactoryFileSystem()
    {
        // arrange
        _fileSystem.AddFile(_mapFilePath, new MockFileData(File.ReadAllBytes(Path.Join("TestFiles", "DepotCache", "8d554.s2ma"))));

        IStormMod stormMod = _stormModFactory.CreateFileMpqStormMod(_fileHeroesSource, "Volskaya Foundry", _mapFilePath, StormModType.Map);

        // act
        IReadOnlyList<MapDependency> mapDependencies = stormMod.GetMapDependencies();

        // assert
        mapDependencies.Should().ContainSingle().Which.LocalFile.Should().Be(Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void CreateCASCMpqStormMod_NoName_ReturnsCASCMpqStormModNamedAfterFile(StormModType stormModType)
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateCASCMpqStormMod(_cascHeroesSource, _mapFilePath, stormModType);

        // assert
        stormMod.Should().BeOfType<CASCMpqStormMod>();
        stormMod.Name.Should().Be("8d554");
        stormMod.DirectoryPath.Should().Be(_mapFilePath);
        stormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    public void CreateCASCMpqStormMod_HasName_ReturnsCASCMpqStormModWithName()
    {
        // act
        IStormMod stormMod = _stormModFactory.CreateCASCMpqStormMod(_cascHeroesSource, "Volskaya Foundry", _mapFilePath, StormModType.Map);

        // assert
        stormMod.Should().BeOfType<CASCMpqStormMod>();
        stormMod.Name.Should().Be("Volskaya Foundry");
        stormMod.DirectoryPath.Should().Be(_mapFilePath);
        stormMod.ModContainer.StormModType.Should().Be(StormModType.Map);
    }
}
