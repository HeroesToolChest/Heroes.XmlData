using CASCLib;
using Heroes.XmlData.Extensions;

namespace Heroes.XmlData.Tests.Extensions;

[TestClass]
public class CASCFolderExtensionsTests
{
    [TestMethod]
    public void TryGetLastDirectory_GetLastDirectoryFromGivenPath_ReturnsCASCFolder()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(Path.Join("folder1", "folder2", "folder3", "folder4"));

        // act
        bool success = cascFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder3"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder.Should().NotBeNull();
        resultCASCFolder!.Name.Should().Be("folder3");
    }

    [TestMethod]
    public void TryGetLastDirectory_NonExistDirectoryPath_ReturnsNull()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddDirectory(Path.Join("folder1", "folder2", "folder3", "folder4"));

        // act
        bool success = cascFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder5"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeFalse();
        resultCASCFolder.Should().BeNull();
    }

    [TestMethod]
    public void AddDirectory_NewDirectoryPath_AddsDirectories()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddDirectory(Path.Join("this", "is", "path"));

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
        cascFolder.AddDirectory(Path.Join("this", "is", "path"));

        // act
        cascFolder.AddDirectory(Path.Join("this", "is", "path2"));

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
        currentFolder3a.Files.Should().BeEmpty();
        currentFolder3a.Folders.Should().BeEmpty();
        currentFolder3a.Name.Should().Be("path");

        CASCFolder currentFolder3b = currentFolder2.GetFolder("path2");
        currentFolder3b.Files.Should().BeEmpty();
        currentFolder3b.Folders.Should().BeEmpty();
        currentFolder3b.Name.Should().Be("path2");
    }

    [TestMethod]
    public void AddFile_NewFilePath_AddsDiretoriesAndFile()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        // act
        cascFolder.AddFile(Path.Join("this", "is", "file.txt"));

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
        file.Name.Should().Be("file.txt");
        file.FullName.Should().Be(Path.Join("this", "is", "file.txt"));
    }

    [TestMethod]
    public void AddFile_HasExistingDirectories_AddsNewDirectories()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.AddFile(Path.Join("this", "is", "file.txt"));

        // act
        cascFolder.AddFile(Path.Join("this", "is", "another", "file.txt"));

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
        file.FullName.Should().Be(Path.Join("this", "is", "file.txt"));

        CASCFolder currentFolder3 = currentFolder2.GetFolder("another");
        currentFolder3.Files.Should().ContainSingle();
        currentFolder3.Folders.Should().BeEmpty();
        currentFolder3.Name.Should().Be("another");

        CASCFile file3 = currentFolder3.GetFile("file.txt");
        file3.Name.Should().Be("file.txt");
        file3.FullName.Should().Be(Path.Join("this", "is", "another", "file.txt"));
    }

    [TestMethod]
    public void AddFile_HasExistingFile_OverrideExistingFile()
    {
        // arrange
        CASCFolder cascFolder = new("name");

        cascFolder.AddFile(Path.Join("this", "is", "file.txt"));
        CASCFolder currentFolder1 = cascFolder.GetFolder("this");
        CASCFolder currentFolder2 = currentFolder1.GetFolder("is");
        ulong originalHash = currentFolder2.GetFile("file.txt").Hash;

        // act
        cascFolder.AddFile(Path.Join("this", "is", "file.txt"));

        // assert
        cascFolder.Files.Should().BeEmpty();
        cascFolder.Folders.Should().ContainSingle();

        CASCFile file = currentFolder2.GetFile("file.txt");
        file.Name.Should().Be("file.txt");
        file.FullName.Should().Be(Path.Join("this", "is", "file.txt"));
        file.Hash.Should().NotBe(originalHash);
    }
}
