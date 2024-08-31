using CASCLib;

namespace Heroes.XmlData.Tests.Source;

[TestClass]
public class FileHeroesSourceTests
{
    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public FileHeroesSourceTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void FileExists_PathLookupStartsWithMods_FileFound()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _backgroundWorkerEx);

        // act
        bool result = fileHeroesSource.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_PathLookupStartsWithOutMods_FileFound()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _backgroundWorkerEx);

        // act
        bool result = fileHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_PathLookup_FileNotFound()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _backgroundWorkerEx);

        // act
        bool result = fileHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_FileExists_GetsFile()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _backgroundWorkerEx);

        // act
        Stream? result = fileHeroesSource.GetFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void GetFile_FileDoesNotExists_GetsNull()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _backgroundWorkerEx);

        // act
        Stream? result = fileHeroesSource.GetFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeNull();
    }
}
