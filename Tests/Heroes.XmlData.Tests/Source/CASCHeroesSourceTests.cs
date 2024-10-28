using CASCLib;
using Heroes.XmlData.CASC;
using Heroes.XmlData.Extensions;

namespace Heroes.XmlData.Tests.Source;

[TestClass]
public class CASCHeroesSourceTests
{
    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly ICASCHeroesStorage _cascHeroesStorage;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public CASCHeroesSourceTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void FileExists_PathLookupStartsWithMods_FileFound()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_PathLookupStartsWithOutMods_FileFound()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_PathLookup_FileNotFound()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        // act
        bool result = cascHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_FileExists_GetsFile()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        using MemoryStream stream1 = GetMockStream("""text""");

        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(stream1);

        // act
        Stream? result = cascHeroesSource.GetFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void GetFile_FileDoesNotExists_GetsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(false);

        // act
        Stream? result = cascHeroesSource.GetFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().BeNull();
    }

    private static MemoryStream GetMockStream(string content)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.WriteLine(content);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}
