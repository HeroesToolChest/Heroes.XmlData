namespace Heroes.StormData.StormMods.Mpq.Tests;

[TestClass]
public sealed class MpqFolderTests
{
    [TestMethod]
    public void AddFile_FileInRoot_AddsFileToRoot()
    {
        // arrange
        MpqFolder root = new("root");

        // act
        root.AddFile("file.txt");

        // assert
        root.Folders.Should().BeEmpty();
        root.Files.Should().ContainSingle();
        root.Files["file.txt"].Name.Should().Be("file.txt");
        root.Files["file.txt"].FullName.Should().Be("file.txt");
    }

    [TestMethod]
    public void AddFile_NestedPath_CreatesFoldersAndFile()
    {
        // arrange
        MpqFolder root = new("root");

        // act
        root.AddFile(@"base.stormdata\GameData\UnitData.xml");

        // assert
        root.Files.Should().BeEmpty();
        root.Folders.Should().ContainSingle();

        MpqFolder baseFolder = root.Folders["base.stormdata"];
        baseFolder.Name.Should().Be("base.stormdata");
        baseFolder.Files.Should().BeEmpty();
        baseFolder.Folders.Should().ContainSingle();

        MpqFolder gameDataFolder = baseFolder.Folders["GameData"];
        gameDataFolder.Name.Should().Be("GameData");
        gameDataFolder.Folders.Should().BeEmpty();
        gameDataFolder.Files.Should().ContainSingle();

        MpqFile file = gameDataFolder.Files["UnitData.xml"];
        file.Name.Should().Be("UnitData.xml");
        file.FullName.Should().Be(@"base.stormdata\GameData\UnitData.xml");
    }

    [TestMethod]
    [DataRow("folder1/folder2/file.txt")]
    [DataRow(@"folder1\folder2\file.txt")]
    [DataRow(@"folder1/folder2\file.txt")]
    [DataRow(@"/folder1//folder2\\file.txt")]
    public void AddFile_AnySeparator_CreatesSameTree(string filePath)
    {
        // arrange
        MpqFolder root = new("root");

        // act
        root.AddFile(filePath);

        // assert
        root.Folders.Should().ContainSingle();
        root.Folders["folder1"].Folders.Should().ContainSingle();
        root.Folders["folder1"].Folders["folder2"].Files.Should().ContainSingle()
            .Which.Value.Name.Should().Be("file.txt");
    }

    [TestMethod]
    public void AddFile_HasExistingFolders_ReusesExistingFolders()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\file1.txt");

        // act
        root.AddFile(@"folder1\folder2\file2.txt");
        root.AddFile(@"folder1\folder3\file3.txt");

        // assert
        root.Folders.Should().ContainSingle();

        MpqFolder folder1 = root.Folders["folder1"];
        folder1.Folders.Should().HaveCount(2);
        folder1.Folders["folder2"].Files.Keys.Should().BeEquivalentTo("file1.txt", "file2.txt");
        folder1.Folders["folder3"].Files.Keys.Should().BeEquivalentTo("file3.txt");
    }

    [TestMethod]
    public void AddFile_DuplicateFilePath_KeepsExistingFile()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\file.txt");
        MpqFile existingFile = root.Folders["folder1"].Files["file.txt"];

        // act
        root.AddFile(@"folder1\FILE.TXT");

        // assert
        root.Folders.Should().ContainSingle();
        root.Folders["folder1"].Folders.Should().BeEmpty();
        root.Folders["folder1"].Files.Should().ContainSingle()
            .Which.Value.Should().BeSameAs(existingFile);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    [DataRow(@"\\")]
    public void AddFile_EmptyPath_AddsNothing(string filePath)
    {
        // arrange
        MpqFolder root = new("root");

        // act
        root.AddFile(filePath);

        // assert
        root.Files.Should().BeEmpty();
        root.Folders.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("folder1/folder2")]
    [DataRow(@"folder1\folder2")]
    [DataRow(@"folder1\folder2\")]
    [DataRow(@"/folder1/folder2/")]
    [DataRow(@"FOLDER1\Folder2")]
    public void TryGetFolder_ExistingFolderPath_ReturnsFolder(string folderPath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\folder3\file.txt");

        // act
        bool success = root.TryGetFolder(folderPath, out MpqFolder? result);

        // assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Name.Should().Be("folder2");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public void TryGetFolder_EmptyPath_ReturnsSelf(string folderPath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\file.txt");

        // act
        bool success = root.TryGetFolder(folderPath, out MpqFolder? result);

        // assert
        success.Should().BeTrue();
        result.Should().BeSameAs(root);
    }

    [TestMethod]
    [DataRow(@"folder1\folder5")]
    [DataRow(@"folder5\folder2")]
    [DataRow(@"folder1\folder2\folder3\folder4")]
    public void TryGetFolder_NonExistingFolderPath_ReturnsFalse(string folderPath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\folder3\file.txt");

        // act
        bool success = root.TryGetFolder(folderPath, out MpqFolder? result);

        // assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFolder_PathIsFile_ReturnsFalse()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\file.txt");

        // act
        bool success = root.TryGetFolder(@"folder1\file.txt", out MpqFolder? result);

        // assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    [DataRow("folder1/folder2/file.txt")]
    [DataRow(@"folder1\folder2\file.txt")]
    [DataRow(@"/folder1\folder2/file.txt")]
    [DataRow(@"FOLDER1\folder2\FILE.txt")]
    public void TryGetFile_ExistingFilePath_ReturnsFile(string filePath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\file.txt");

        // act
        bool success = root.TryGetFile(filePath, out MpqFile? result);

        // assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Name.Should().Be("file.txt");
        result.FullName.Should().Be(@"folder1\folder2\file.txt");
    }

    [TestMethod]
    public void TryGetFile_FileInRoot_ReturnsFile()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile("file.txt");

        // act
        bool success = root.TryGetFile("file.txt", out MpqFile? result);

        // assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Name.Should().Be("file.txt");
    }

    [TestMethod]
    [DataRow(@"folder1\folder2\other.txt")]
    [DataRow(@"folder1\folder5\file.txt")]
    [DataRow(@"folder5\folder2\file.txt")]
    [DataRow("file.txt")]
    public void TryGetFile_NonExistingFilePath_ReturnsFalse(string filePath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\file.txt");

        // act
        bool success = root.TryGetFile(filePath, out MpqFile? result);

        // assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_PathIsFolder_ReturnsFalse()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile(@"folder1\folder2\file.txt");

        // act
        bool success = root.TryGetFile(@"folder1\folder2", out MpqFile? result);

        // assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public void TryGetFile_EmptyPath_ReturnsFalse(string filePath)
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile("file.txt");

        // act
        bool success = root.TryGetFile(filePath, out MpqFile? result);

        // assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void EnumerateFiles_NestedFolders_ReturnsAllFiles()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile("file1.txt");
        root.AddFile(@"folder1\file2.txt");
        root.AddFile(@"folder1\folder2\file3.txt");
        root.AddFile(@"folder3\file4.txt");

        // act
        List<MpqFile> result = [.. root.EnumerateFiles()];

        // assert
        result.Select(x => x.FullName).Should().BeEquivalentTo(
            "file1.txt",
            @"folder1\file2.txt",
            @"folder1\folder2\file3.txt",
            @"folder3\file4.txt");
    }

    [TestMethod]
    public void EnumerateFiles_FromSubfolder_ReturnsOnlyFilesUnderSubfolder()
    {
        // arrange
        MpqFolder root = new("root");
        root.AddFile("file1.txt");
        root.AddFile(@"folder1\file2.txt");
        root.AddFile(@"folder1\folder2\file3.txt");
        root.AddFile(@"folder3\file4.txt");
        root.TryGetFolder("folder1", out MpqFolder? folder1).Should().BeTrue();

        // act
        List<MpqFile> result = [.. folder1!.EnumerateFiles()];

        // assert
        result.Select(x => x.FullName).Should().BeEquivalentTo(
            @"folder1\file2.txt",
            @"folder1\folder2\file3.txt");
    }

    [TestMethod]
    public void EnumerateFiles_NoFiles_ReturnsEmpty()
    {
        // arrange
        MpqFolder root = new("root");

        // act
        List<MpqFile> result = [.. root.EnumerateFiles()];

        // assert
        result.Should().BeEmpty();
    }
}
