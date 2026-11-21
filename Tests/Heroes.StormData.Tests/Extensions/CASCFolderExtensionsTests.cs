using CASCLib;

namespace Heroes.StormData.Extensions.Tests;

[TestClass]
public sealed class CASCFolderExtensionsTests
{
    [TestMethod]
    [DataRow('/')]
    [DataRow('\\')]
    public void TryGetLastDirectory_GetLastDirectoryFromGivenPath_ReturnsCASCFolder(char separator)
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2", "folder3", "folder4"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join(separator, "folder1", "folder2", "folder3"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder.Should().BeSameAs(cascFolder.GetFolder("folder1").GetFolder("folder2").GetFolder("folder3"));
    }

    [TestMethod]
    public void TryGetLastDirectory_PlatformSeparatorPath_ReturnsCASCFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2", "folder3"));

        // act
        bool success = cascFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder3"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder!.Name.Should().Be("folder3");
    }

    [TestMethod]
    public void TryGetLastDirectory_DeepestDirectory_ReturnsCASCFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2", "folder3", "folder4"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join('/', "folder1", "folder2", "folder3", "folder4"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder!.Name.Should().Be("folder4");
        resultCASCFolder.Folders.Should().BeEmpty();
    }

    [TestMethod]
    public void TryGetLastDirectory_FirstLevelDirectory_ReturnsCASCFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2"));

        // act
        bool success = cascFolder.TryGetLastDirectory("folder1", out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder.Should().BeSameAs(cascFolder.GetFolder("folder1"));
    }

    [TestMethod]
    [DataRow('/')]
    [DataRow('\\')]
    public void TryGetLastDirectory_NonExistDirectoryPath_ReturnsFalse(char separator)
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2", "folder3", "folder4"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join(separator, "folder1", "folder2", "folder5"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeFalse();
        resultCASCFolder.Should().BeNull();
    }

    [TestMethod]
    public void TryGetLastDirectory_FirstDirectoryDoesNotExist_ReturnsFalse()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join('/', "folder5", "folder2"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeFalse();
        resultCASCFolder.Should().BeNull();
    }

    [TestMethod]
    public void TryGetLastDirectory_PathIsAFile_ReturnsFalse()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddFile(string.Join('/', "folder1", "file.txt"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join('/', "folder1", "file.txt"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeFalse();
        resultCASCFolder.Should().BeNull();
    }

    [TestMethod]
    public void TryGetLastDirectory_DifferentCasing_ReturnsCASCFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "Mods", "Core.StormMod", "Base.StormData"));

        // act
        bool success = cascFolder.TryGetLastDirectory(string.Join('/', "mods", "core.stormmod", "base.stormdata"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder!.Name.Should().Be("Base.StormData");
    }

    [TestMethod]
    [DataRow("/folder1/folder2/")]
    [DataRow("\\folder1\\folder2\\")]
    [DataRow("folder1//folder2")]
    public void TryGetLastDirectory_ExtraSeparators_ReturnsCASCFolder(string directoryPath)
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "folder1", "folder2"));

        // act
        bool success = cascFolder.TryGetLastDirectory(directoryPath, out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder!.Name.Should().Be("folder2");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    [DataRow("\\")]
    public void TryGetLastDirectory_NoDirectories_ReturnsSameFolder(string directoryPath)
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory("folder1");

        // act
        bool success = cascFolder.TryGetLastDirectory(directoryPath, out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder.Should().BeSameAs(cascFolder);
    }

    [TestMethod]
    [DataRow('/')]
    [DataRow('\\')]
    public void AddDirectory_NewDirectoryPath_AddsDirectories(char separator)
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddDirectory(string.Join(separator, "this", "is", "path"));

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();

        CASCFolder currentFolder1 = cascFolder.GetFolder("this");
        currentFolder1.Files.Should().BeEmpty();
        currentFolder1.Folders.Should().ContainSingle();
        currentFolder1.Name.Should().Be("this");

        CASCFolder currentFolder2 = currentFolder1.GetFolder("is");
        currentFolder2.Files.Should().BeEmpty();
        currentFolder2.Folders.Should().ContainSingle();
        currentFolder2.Name.Should().Be("is");

        CASCFolder currentFolder3 = currentFolder2.GetFolder("path");
        currentFolder3.Files.Should().BeEmpty();
        currentFolder3.Folders.Should().BeEmpty();
        currentFolder3.Name.Should().Be("path");
    }

    [TestMethod]
    public void AddDirectory_HasExistingDirectories_AddsOnlyNewDirectory()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "this", "is", "path"));

        CASCFolder existingFolder = cascFolder.GetFolder("this").GetFolder("is").GetFolder("path");

        // act
        cascFolder.AddDirectory(string.Join('/', "this", "is", "path2"));

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();

        CASCFolder currentFolder1 = cascFolder.GetFolder("this");
        currentFolder1.Files.Should().BeEmpty();
        currentFolder1.Folders.Should().ContainSingle();
        currentFolder1.Name.Should().Be("this");

        CASCFolder currentFolder2 = currentFolder1.GetFolder("is");
        currentFolder2.Files.Should().BeEmpty();
        currentFolder2.Folders.Should().HaveCount(2);
        currentFolder2.Name.Should().Be("is");

        CASCFolder currentFolder3a = currentFolder2.GetFolder("path");
        currentFolder3a.Should().BeSameAs(existingFolder);
        currentFolder3a.Files.Should().BeEmpty();
        currentFolder3a.Folders.Should().BeEmpty();
        currentFolder3a.Name.Should().Be("path");

        CASCFolder currentFolder3b = currentFolder2.GetFolder("path2");
        currentFolder3b.Files.Should().BeEmpty();
        currentFolder3b.Folders.Should().BeEmpty();
        currentFolder3b.Name.Should().Be("path2");
    }

    [TestMethod]
    public void AddDirectory_ExistingDirectoryWithDifferentCasing_DoesNotAddDuplicate()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "Mods", "Core.StormMod"));

        // act
        cascFolder.AddDirectory(string.Join('/', "mods", "core.stormmod", "base.stormdata"));

        // assert
        cascFolder.Folders.Should().ContainSingle();

        CASCFolder modsFolder = cascFolder.GetFolder("mods");
        modsFolder.Name.Should().Be("Mods");
        modsFolder.Folders.Should().ContainSingle();

        CASCFolder coreFolder = modsFolder.GetFolder("core.stormmod");
        coreFolder.Name.Should().Be("Core.StormMod");
        coreFolder.Folders.Should().ContainSingle().Which.Key.Should().Be("base.stormdata");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public void AddDirectory_NoDirectories_AddsNothing(string directoryPath)
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddDirectory(directoryPath);

        // assert
        cascFolder.Folders.Should().BeEmpty();
        cascFolder.Files.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow('/')]
    [DataRow('\\')]
    public void AddFile_NewFilePath_AddsDirectoriesAndFile(char separator)
    {
        // arrange
        CASCFolder cascFolder = new("name");
        string filePath = string.Join(separator, "this", "is", "file.txt");

        // act
        cascFolder.AddFile(filePath);

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();

        CASCFolder currentFolder1 = cascFolder.GetFolder("this");
        currentFolder1.Files.Should().BeEmpty();
        currentFolder1.Folders.Should().ContainSingle();
        currentFolder1.Name.Should().Be("this");

        CASCFolder currentFolder2 = currentFolder1.GetFolder("is");
        currentFolder2.Files.Should().ContainSingle();
        currentFolder2.Folders.Should().BeEmpty();
        currentFolder2.Name.Should().Be("is");

        CASCFile file = currentFolder2.GetFile("file.txt");
        file.FullName.Should().Be(filePath);
    }

    [TestMethod]
    public void AddFile_FileAtRoot_AddsFileOnly()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddFile("file.txt");

        // assert
        cascFolder.Folders.Should().BeEmpty();
        cascFolder.Files.Should().ContainSingle().Which.Key.Should().Be("file.txt");
        cascFolder.GetFile("file.txt").FullName.Should().Be("file.txt");
    }

    [TestMethod]
    public void AddFile_HasExistingDirectories_AddsNewDirectories()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddFile(string.Join('/', "this", "is", "file.txt"));

        // act
        cascFolder.AddFile(string.Join('/', "this", "is", "another", "file.txt"));

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();

        CASCFolder currentFolder1 = cascFolder.GetFolder("this");
        currentFolder1.Files.Should().BeEmpty();
        currentFolder1.Folders.Should().ContainSingle();
        currentFolder1.Name.Should().Be("this");

        CASCFolder currentFolder2 = currentFolder1.GetFolder("is");
        currentFolder2.Files.Should().ContainSingle();
        currentFolder2.Folders.Should().ContainSingle();
        currentFolder2.Name.Should().Be("is");

        CASCFile file = currentFolder2.GetFile("file.txt");
        file.Name.Should().Be("file.txt");
        file.FullName.Should().Be(string.Join('/', "this", "is", "file.txt"));

        CASCFolder currentFolder3 = currentFolder2.GetFolder("another");
        currentFolder3.Files.Should().ContainSingle();
        currentFolder3.Folders.Should().BeEmpty();
        currentFolder3.Name.Should().Be("another");

        CASCFile file3 = currentFolder3.GetFile("file.txt");
        file3.Name.Should().Be("file.txt");
        file3.FullName.Should().Be(string.Join('/', "this", "is", "another", "file.txt"));
    }

    [TestMethod]
    public void AddFile_HasExistingFile_ReplacesExistingFile()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        cascFolder.AddFile(string.Join('/', "this", "is", "file.txt"));
        CASCFolder currentFolder2 = cascFolder.GetFolder("this").GetFolder("is");
        CASCFile originalFile = currentFolder2.GetFile("file.txt");

        // act
        cascFolder.AddFile(string.Join('/', "this", "is", "file.txt"));

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();
        currentFolder2.Files.Should().ContainSingle();

        CASCFile file = currentFolder2.GetFile("file.txt");
        file.Should().NotBeSameAs(originalFile);
        file.Name.Should().Be("file.txt");
        file.FullName.Should().Be(string.Join('/', "this", "is", "file.txt"));
    }

    [TestMethod]
    public void AddFile_FileNameMatchesExistingFolder_AddsFileAndKeepsFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(string.Join('/', "this", "is", "name"));

        // act
        cascFolder.AddFile(string.Join('/', "this", "is", "name"));

        // assert
        CASCFolder currentFolder2 = cascFolder.GetFolder("this").GetFolder("is");
        currentFolder2.Folders.Should().ContainSingle().Which.Key.Should().Be("name");
        currentFolder2.Files.Should().ContainSingle().Which.Key.Should().Be("name");
    }

    [TestMethod]
    public void AddFile_SamePath_HasSameHash()
    {
        // arrange
        CASCFolder cascFolder1 = new("name");
        CASCFolder cascFolder2 = new("name");

        // act
        cascFolder1.AddFile(string.Join('/', "this", "is", "file.txt"));
        cascFolder2.AddFile(string.Join('/', "this", "is", "file.txt"));

        // assert
        ulong hash1 = cascFolder1.GetFolder("this").GetFolder("is").GetFile("file.txt").Hash;
        ulong hash2 = cascFolder2.GetFolder("this").GetFolder("is").GetFile("file.txt").Hash;

        hash1.Should().Be(hash2);
    }

    [TestMethod]
    public void AddFile_DifferentPaths_HaveDifferentHashes()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddFile(string.Join('/', "this", "is", "file1.txt"));
        cascFolder.AddFile(string.Join('/', "this", "is", "file2.txt"));

        // assert
        CASCFolder currentFolder2 = cascFolder.GetFolder("this").GetFolder("is");

        currentFolder2.GetFile("file1.txt").Hash.Should().NotBe(currentFolder2.GetFile("file2.txt").Hash);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public void AddFile_NoFileName_AddsNothing(string filePath)
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddFile(filePath);

        // assert
        cascFolder.Folders.Should().BeEmpty();
        cascFolder.Files.Should().BeEmpty();
    }
}
