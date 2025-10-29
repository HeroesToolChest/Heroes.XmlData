using CASCLib;
using System.ComponentModel;

namespace Heroes.XmlData.Tests.Source;

[TestClass]
public class FileHeroesSourceTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly IProgressReporter _progressReporter;

    public FileHeroesSourceTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _progressReporter = Substitute.For<IProgressReporter>();
    }

    [TestMethod]
    [DataRow("mods", true, "somefile.txt")]
    [DataRow("mods", false, "file-no-exist.txt")]
    [DataRow("other", false, "somefile.txt")]
    public void FileExists_PathLookupStartsWithInput_ReturnsResult(string inputRootDirectory, bool found, string fileToLookup)
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(inputRootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, inputRootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(Path.Join(inputRootDirectory, "test.stormmod", "base.stormdata", fileToLookup));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow("mods", true, "somefile.txt")]
    [DataRow("mods_1234", true, "somefile.txt")]
    [DataRow("mods", false, "file-no-exist.txt")]
    [DataRow("other", true, "somefile.txt")]
    public void FileExists_PathLookupStartsWithOutMods_ReturnsResult(string rootDirectory, bool expectedIsFound, string fileToLookup)
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(Path.Join("test.stormmod", "base.stormdata", fileToLookup));

        // assert
        result.Should().Be(expectedIsFound);
    }

    [TestMethod]
    [DataRow(true, "mpq.s2ma")]
    [DataRow(false, "does-not-exist.s2ma")]
    public void FileExists_CheckIfMpqS2maFileExists_ReturnsMpqResult(bool found, string mpqFile)
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", mpqFile));

        // assert
        result.Should().Be(found);
    }

    [TestMethod]
    [DataRow(true, "(listfile)")]
    [DataRow(false, "file-no-exist.txt")]
    public void FileExists_LookupFileInMpq_ReturnsEntryResult(bool found, string fileToLookup)
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(fileToLookup, Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(new StormFile(new StormPath()
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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(new StormFile(new StormPath()
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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(new StormFile(new StormPath()
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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        bool result = fileHeroesSource.FileExists(emptyPath);

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
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Stream result = fileHeroesSource.GetFile(Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void GetFile_LookupFileInMpq_GetsFile()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Stream result = fileHeroesSource.GetFile("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void GetFile_FileDoesNotExists_ThrowsException()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));
    }

    [TestMethod]
    public void GetFile_ActualMpqS2maDoesNotExist_ThrowsException()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile("(listfile)", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));
    }

    [TestMethod]
    public void GetFile_FileInMpqDoesNotExists_ThrowsException()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile("does-not-exist", Path.Join("test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be("does-not-exist");
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileLookupFileShoudExist_GetFile()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "somefile.txt"), new MockFileData("text") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Stream result = fileHeroesSource.GetFile(new StormFile(new StormPath()
        {
            Path = Path.Join("test.stormmod", "base.stormdata", "somefile.txt"),
            PathType = StormPathType.File,
            StormModName = "modName",
            StormModPath = "modPath",
        }));

        // assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileLookupFileShoudNotExist_ThrowsException()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile(new StormFile(new StormPath()
        {
            Path = Path.Join("test.stormmod", "base.stormdata", "somefile.txt"),
            PathType = StormPathType.File,
            StormModName = "modName",
            StormModPath = "modPath",
        }));

        // assert
        act.Should().Throw<FileNotFoundException>()
            .Which.FileName.Should().Be(Path.Join("test.stormmod", "base.stormdata", "somefile.txt"));
    }

    [TestMethod]
    [Category("StormFile")]
    public void GetFile_WithStormFileLookupFileInMpq_GetsFile()
    {
        // arrange
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Stream result = fileHeroesSource.GetFile(new StormFile(new StormPath()
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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile(new StormFile(new StormPath()
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
        const string rootDirectory = "mods";

        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join(rootDirectory, "test.stormmod", "base.stormdata", "depotcache", "mpq.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
        });
        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(mockFileSystem, stormStorage, _stormModFactory, _depotCacheFactory, rootDirectory, _progressReporter);

        // act
        Action act = () => fileHeroesSource.GetFile(new StormFile(new StormPath()
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
}
