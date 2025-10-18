using CASCLib;
using Heroes.XmlData.CASC;
using Heroes.XmlData.Extensions;
using System.ComponentModel;

namespace Heroes.XmlData.Tests.Source;

[TestClass]
public class CASCHeroesSourceTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly ICASCHeroesStorage _cascHeroesStorage;
    private readonly IProgressReporter _progressReporter;

    public CASCHeroesSourceTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _progressReporter = Substitute.For<IProgressReporter>();
    }

    [TestMethod]
    [DataRow(true, "somefile.txt")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_PathLookupStartsWithMods_RetunsResult(bool found, string fileToLookup)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", fileToLookup));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow(true, "somefile.txt")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_PathLookupStartsWithOutMods_FileFound(bool found, string fileToLookup)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", fileToLookup));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow(true, "mpq.s2ma")]
    [DataRow(false, "does-not-exist.s2ma")]
    public void FileExists_CheckIfMpqS2maFileExists_ReturnsMpqResult(bool found, string mpqFile)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", mpqFile));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow(true, "(listfile)")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_LookupFileInMpq_ReturnsEntryResult(bool found, string fileToLookup)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(fileToLookup, Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [Category("StormFile")]
    [DataRow(true, "somefile.txt")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_WithStormFile_ReturnsResult(bool found, string fileToLookup)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(new StormFile(new StormPath()
        {
            Path = Path.Join("test.stormmod", "base.stormdata", fileToLookup),
            PathType = StormPathType.File,
            StormModName = "modName",
            StormModPath = "modPath",
        }));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [Category("StormFile")]
    [DataRow(true, "mpq.s2ma")]
    [DataRow(false, "does-not-exist.s2ma")]
    public void FileExists_CheckIfMpqS2maStormFileExists_ReturnsMpqResult(bool found, string mpqFile)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(new StormFile(new StormPath()
        {
            Path = "(listfile)",
            PathType = StormPathType.MPQ,
            StormModName = "modName",
            StormModPath = Path.Join("test.stormmod", "base.stormdata", "depotcache", mpqFile),
        }));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [Category("StormFile")]
    [DataRow(true, "(listfile)")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_LookupFileInMpqStorm_ReturnsEntryResult(bool found, string fileToLookup)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        bool result = cascHeroesSource.FileExists(new StormFile(new StormPath()
        {
            Path = fileToLookup,
            PathType = StormPathType.MPQ,
            StormModName = "modName",
            StormModPath = Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"),
        }));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void FileExists_PathIsEmtpy_ReturnsFalse(string emptyPath)
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        // act
        bool result = cascHeroesSource.FileExists(emptyPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_FileExists_GetsFile()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

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
    public void GetFile_LookupFileInMpq_GetsFile()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        Stream result = cascHeroesSource.GetFile("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void GetFile_FileDoesNotExists_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt")).Returns(false);

        // act
        Action act = () => cascHeroesSource.GetFile(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));
    }

    [TestMethod]
    public void GetFile_ActualMpqS2maDoesNotExist_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(false);

        // act
        Action act = () => cascHeroesSource.GetFile("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));
    }

    [TestMethod]
    public void GetFile_FileInMpqDoesNotExists_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        Action act = () => cascHeroesSource.GetFile("does-not-exist", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be("does-not-exist");
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileLookupFileInMpq_GetsFile()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        Stream result = cascHeroesSource.GetFile(new StormFile(new StormPath()
        {
            Path = "(listfile)",
            PathType = StormPathType.MPQ,
            StormModName = "modName",
            StormModPath = Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"),
        }));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileActualMpqS2maDoesNotExist_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        CASCFolder rootFolder = new("name");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        // act
        Action act = () => cascHeroesSource.GetFile(new StormFile(new StormPath()
        {
            Path = "(listfile)",
            PathType = StormPathType.MPQ,
            StormModName = "modName",
            StormModPath = Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"),
        }));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileInMpqDoesNotExists_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);

        const string rootDirectory = "mods";

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma")).Returns(true);

        // act
        Action act = () => cascHeroesSource.GetFile(new StormFile(new StormPath()
        {
            Path = "does-not-exist",
            PathType = StormPathType.MPQ,
            StormModName = "modName",
            StormModPath = Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"),
        }));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be("does-not-exist");
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
