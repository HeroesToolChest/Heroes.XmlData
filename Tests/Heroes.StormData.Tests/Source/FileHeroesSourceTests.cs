using System.IO.Abstractions;

namespace Heroes.StormData.Source.Tests;

[TestClass]
public sealed class FileHeroesSourceTests
{
    private const string MpqEntryPath = "Base.StormData/GameData/ActorData.xml";

    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\hots_data");
    private static readonly string _filePath = Path.Combine(_modsDirectoryPath, "core.stormmod", "base.stormdata", "gamedata.xml");
    private static readonly string _archivePath = Path.Combine(_modsDirectoryPath, "heroesmapmods", "battlegroundmapmods", "test.stormmap");

    private readonly MockFileSystem _fileSystem = new();
    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();

    public FileHeroesSourceTests()
    {
        _fileSystem.AddFile(_filePath, new MockFileData("file content"));
        _fileSystem.AddFile(_archivePath, new MockFileData(File.ReadAllBytes(Path.Join("TestFiles", "Mpq", "test.s2ma"))));
    }

    [TestMethod]
    public void FileExists_PathRelativeToModsDirectory_ReturnsTrue()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(Path.Combine("core.stormmod", "base.stormdata", "gamedata.xml"), null);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("mods/Core.StormMod/Base.StormData/GameData.xml")]
    [DataRow(@"Mods\core.stormmod\base.stormdata\gamedata.xml")]
    public void FileExists_PathStartsWithGameModsDirectory_ReturnsTrue(string path)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(path, null);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_AbsolutePath_ReturnsTrue()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(_filePath, null);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_AbsolutePathInModsDirectory_ChecksNormalizedPath()
    {
        // arrange
        IFileSystem fileSystem = Substitute.For<IFileSystem>();
        fileSystem.Path.Returns(_fileSystem.Path);

        FileHeroesSource fileHeroesSource = new(fileSystem, _modsDirectoryPath, _stormModFactory);

        // act
        fileHeroesSource.FileExists(Path.Combine(_modsDirectoryPath, "Core.StormMod", "Base.StormData", "GameData.xml"), null);

        // assert
        fileSystem.File.Received(1).Exists(_filePath);
    }

    [TestMethod]
    public void FileExists_AbsolutePathOutsideModsDirectory_ChecksPathAsIs()
    {
        // arrange
        string path = MockUnixSupport.Path(@"C:\Other\Core.StormMod\GameData.xml");

        IFileSystem fileSystem = Substitute.For<IFileSystem>();
        fileSystem.Path.Returns(_fileSystem.Path);

        FileHeroesSource fileHeroesSource = new(fileSystem, _modsDirectoryPath, _stormModFactory);

        // act
        fileHeroesSource.FileExists(path, null);

        // assert
        fileSystem.File.Received(1).Exists(path);
    }

    [TestMethod]
    public void FileExists_RelativeModsDirectoryAndPathStartsWithIt_ReturnsTrue()
    {
        // arrange
        string modsDirectoryPath = "hots_data";
        _fileSystem.AddFile(Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), modsDirectoryPath, "core.stormmod", "documentinfo"), new MockFileData("info"));

        FileHeroesSource fileHeroesSource = new(_fileSystem, modsDirectoryPath, _stormModFactory);

        // act
        bool result = fileHeroesSource.FileExists(Path.Combine(modsDirectoryPath, "core.stormmod", "documentinfo"), null);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("core.stormmod/missing.xml")]
    public void FileExists_FileNotFound_ReturnsFalse(string path)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(path, null);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(MpqEntryPath)]
    [DataRow(@"base.stormdata\gamedata\actordata.xml")]
    public void FileExists_MpqEntryExists_ReturnsTrue(string mpqEntryPath)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(_archivePath, mpqEntryPath);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void FileExists_MpqEntryNotFound_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(_archivePath, "base.stormdata/missing.xml");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void FileExists_MpqFileNotFound_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(Path.Combine("heroesmapmods", "missing.stormmap"), MpqEntryPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void FileExists_MpqEntryInFileThatIsNotMpqArchive_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists(_filePath, MpqEntryPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(MpqEntryPath)]
    public void FileExists_PathIsDirectory_ReturnsFalse(string? mpqEntryPath)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.FileExists("core.stormmod", mpqEntryPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_FileExists_ReturnsContent()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        using Stream stream = fileHeroesSource.GetFile("mods/core.stormmod/base.stormdata/gamedata.xml", null);

        // assert
        new StreamReader(stream).ReadToEnd().Should().Be("file content");
    }

    [TestMethod]
    public void GetFile_FileNotFound_ThrowsException()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        Action act = () => fileHeroesSource.GetFile("core.stormmod/missing.xml", null);

        // assert
        act.Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void GetFile_MpqEntryExists_ReturnsEntryContentAfterArchiveIsClosed()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        using Stream stream = fileHeroesSource.GetFile(_archivePath, MpqEntryPath);

        // assert
        string content = new StreamReader(stream).ReadToEnd();
        content.Should().Contain("<Catalog");
    }

    [TestMethod]
    public void GetFile_MpqEntryNotFound_ThrowsException()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        Action act = () => fileHeroesSource.GetFile(_archivePath, "base.stormdata/missing.xml");

        // assert
        act.Should().Throw<FileNotFoundException>().Which.FileName.Should().Be("base.stormdata/missing.xml");
    }

    [TestMethod]
    public void GetFile_MpqFileNotFound_ThrowsException()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        Action act = () => fileHeroesSource.GetFile(Path.Combine("heroesmapmods", "missing.stormmap"), MpqEntryPath);

        // assert
        act.Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void GetFile_PathIsDirectory_ThrowsFileNotFoundException()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        Action act = () => fileHeroesSource.GetFile("core.stormmod", null);

        // assert
        act.Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void GetFile_MpqEntryInFileThatIsNotMpqArchive_ThrowsFileNotFoundException()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        Action act = () => fileHeroesSource.GetFile(_filePath, MpqEntryPath);

        // assert
        act.Should().Throw<FileNotFoundException>().Which.FileName.Should().Be(MpqEntryPath);
    }

    [TestMethod]
    [DataRow("mods/Core.StormMod/Base.StormData/GameData.xml")]
    [DataRow(@"core.stormmod\base.stormdata\gamedata.xml")]
    public void TryGetFile_FileExists_ReturnsTrueAndContent(string path)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(path, null, out Stream? stream);

        // assert
        result.Should().BeTrue();

        using (stream)
            new StreamReader(stream!).ReadToEnd().Should().Be("file content");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("core.stormmod/missing.xml")]
    [DataRow("missingdirectory/missing.xml")]
    public void TryGetFile_FileNotFound_ReturnsFalse(string path)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(path, null, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_MpqEntryExists_ReturnsTrueAndEntryContent()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(_archivePath, @"base.stormdata\gamedata\actordata.xml", out Stream? stream);

        // assert
        result.Should().BeTrue();

        using (stream)
            new StreamReader(stream!).ReadToEnd().Should().Contain("<Catalog");
    }

    [TestMethod]
    public void TryGetFile_MpqEntryNotFound_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(_archivePath, "base.stormdata/missing.xml", out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_MpqFileNotFound_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(Path.Combine("heroesmapmods", "missing.stormmap"), MpqEntryPath, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_MpqEntryInFileThatIsNotMpqArchive_ReturnsFalse()
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile(_filePath, MpqEntryPath, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(MpqEntryPath)]
    public void TryGetFile_PathIsDirectory_ReturnsFalse(string? mpqEntryPath)
    {
        // arrange
        FileHeroesSource fileHeroesSource = CreateFileHeroesSource();

        // act
        bool result = fileHeroesSource.TryGetFile("core.stormmod", mpqEntryPath, out Stream? stream);

        // assert
        result.Should().BeFalse();
        stream.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(MpqEntryPath)]
    public void TryGetFile_PathIsRealDirectory_ReturnsFalse(string? mpqEntryPath)
    {
        // arrange
        // also checked with the real file system, where opening a directory throws an UnauthorizedAccessException
        DirectoryInfo modsDirectory = Directory.CreateTempSubdirectory(nameof(FileHeroesSourceTests));

        try
        {
            modsDirectory.CreateSubdirectory("core.stormmod");

            FileHeroesSource fileHeroesSource = new(new FileSystem(), modsDirectory.FullName, _stormModFactory);

            // act
            bool result = fileHeroesSource.TryGetFile("core.stormmod", mpqEntryPath, out Stream? stream);

            // assert
            result.Should().BeFalse();
            stream.Should().BeNull();
        }
        finally
        {
            modsDirectory.Delete(true);
        }
    }

    private FileHeroesSource CreateFileHeroesSource() => new(_fileSystem, _modsDirectoryPath, _stormModFactory);
}
