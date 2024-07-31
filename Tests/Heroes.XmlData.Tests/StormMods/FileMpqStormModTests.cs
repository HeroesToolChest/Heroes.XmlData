using CASCLib;

namespace Heroes.XmlData.Tests.StormMods;

[TestClass]
public class FileMpqStormModTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public FileMpqStormModTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void LoadStormData_HasS2maFile_LoadsData()
    {
        // arrange
        FileMpqStormMod fileMpqStormMod = ArrangeFileMpqStormMod();

        // act
        fileMpqStormMod.LoadStormData();

        // assert
        fileMpqStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(3);
        fileMpqStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(2);
        fileMpqStormMod.StormModStorage.AddedAssetsFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormData_DoubleCall_ShouldNotDuplicate()
    {
        // arrange
        FileMpqStormMod fileMpqStormMod = ArrangeFileMpqStormMod();
        fileMpqStormMod.LoadStormData();

        // act
        fileMpqStormMod.LoadStormData();

        // assert
        fileMpqStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(3);
        fileMpqStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(2);
        fileMpqStormMod.StormModStorage.AddedAssetsFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormData_HasNoFile_ThrowsException()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, Path.Join("test.stormmod", "depotcache", "8d554.s2ma"), StormModType.Normal);

        // act
        Action action = () => fileMpqStormMod.LoadStormData();

        // assert
        action.Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void LoadStormGameStrings_HasGameStrings_AddsGameStrings()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "depotcache", "8d554.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, Path.Join("test.stormmod", "depotcache", "8d554.s2ma"), StormModType.Normal);

        // act
        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        fileMpqStormMod.StormModStorage.AddedGameStringFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormGameStrings_DoubleCall_AddsGameStrings()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "depotcache", "8d554.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, Path.Join("test.stormmod", "depotcache", "8d554.s2ma"), StormModType.Normal);

        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // act
        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        fileMpqStormMod.StormModStorage.AddedGameStringFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadGameDataDirectory_MpqFolderRootNotInitialized_ShouldAddToNotFoundDirectory()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileMpqStormMod.LoadGameDataDirectory();

        // assert
        fileMpqStormMod.StormModStorage.NotFoundDirectories.Should().ContainSingle().And
            .SatisfyRespectively(
                first =>
                {
                    first.Path.Should().Be(Path.Join("base.stormdata", "gamedata"));
                    first.PathType.Should().Be(StormPathType.MPQ);
                    first.StormModName.Should().Be("test");
                });

        stormStorage.StormCache.DataObjectTypeByElementType.Should().BeEmpty();
        stormStorage.StormCache.StormElementByElementType.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormLayoutDirectory_MpqFolderRootNotInitialized_NothingIsAdded()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileMpqStormMod.LoadStormLayoutDirectory();

        // assert
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().BeEmpty();
    }

    private FileMpqStormMod ArrangeFileMpqStormMod()
    {
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "depotcache", "test.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "test.s2ma"))) },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileMpqStormMod fileMpqStormMod = new(mockFileSystem, fileHeroesSource, Path.Join("test.stormmod", "depotcache", "test.s2ma"), StormModType.Normal);

        // base types
        fileMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CActor default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("ActorData.xml"),
    true);

        return fileMpqStormMod;
    }
}
