namespace Heroes.XmlData.MpqEntry.Tests;

[TestClass]
public class MpqFolderTests
{
    [TestMethod]
    public void TryGetLastDirectory_GetLastDirectoryFromGivenPath_ReturnsMpqFolder()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new MpqFolder("folderName2")
                    {
                        Folders =
                        {
                            {
                                "folder3", new MpqFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new MpqFolder("folderName4")
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        // act
        bool success = mpqFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder3"), out MpqFolder? resultMpqFolder);

        // assert
        success.Should().BeTrue();
        resultMpqFolder.Should().NotBeNull();
        resultMpqFolder!.Name.Should().Be("folderName3");
    }

    [TestMethod]
    public void TryGetLastDirectory_NonExistDirectoryPath_ReturnsNull()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new MpqFolder("folderName2")
                    {
                        Folders =
                        {
                            {
                                "folder3", new MpqFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new MpqFolder("folderName4")
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        // act
        bool success = mpqFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder5"), out MpqFolder? resultMpqFolder);

        // assert
        success.Should().BeFalse();
        resultMpqFolder.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_GetFile_ReturnFile()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new MpqFolder("folderName2")
                    {
                        Files =
                        {
                            {
                                "mysqlfile.sql", new MpqFile("name1")
                            },
                            {
                                "mytextfile.txt", new MpqFile("name2")
                            },
                        },
                        Folders =
                        {
                            {
                                "folder3", new MpqFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new MpqFolder("folderName4")
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        // act
        bool success = mpqFolder.TryGetFile(Path.Join("folder1", "folder2", "mytextfile.txt"), out MpqFile? mpqFile);

        // assert
        success.Should().BeTrue();
        mpqFile.Should().NotBeNull();
        mpqFile!.Name.Should().Be("name2");
    }

    [TestMethod]
    public void TryGetFile_NonExistsFile_ReturnNull()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new MpqFolder("folderName2")
                    {
                        Files =
                        {
                            {
                                "mysqlfile.sql", new MpqFile("name1")
                            },
                            {
                                "mytextfile.txt", new MpqFile("name2")
                            },
                        },
                        Folders =
                        {
                            {
                                "folder3", new MpqFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new MpqFolder("folderName4")
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        // act
        bool success = mpqFolder.TryGetFile(Path.Join("folder1", "folder2", "mycsfile.cs"), out MpqFile? mpqFile);

        // assert
        success.Should().BeFalse();
        mpqFile.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFile_NonExistsDirectoryForFile_ReturnNull()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new MpqFolder("folderName2")
                    {
                        Files =
                        {
                            {
                                "mysqlfile.sql", new MpqFile("name1")
                            },
                            {
                                "mytextfile.txt", new MpqFile("name2")
                            },
                        },
                        Folders =
                        {
                            {
                                "folder3", new MpqFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new MpqFolder("folderName4")
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        // act
        bool success = mpqFolder.TryGetFile(Path.Join("folder1", "folder2", "folder9", "mycsfile.cs"), out MpqFile? mpqFile);

        // assert
        success.Should().BeFalse();
        mpqFile.Should().BeNull();
    }

    [TestMethod]
    public void GetFolder_FolderFound_ReturnsFolder()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName")
        {
            Files =
            {
                {
                    "innerFile1", new MpqFile("otherFile")
                },
            },
            Folders =
            {
                {
                    "innerFolder1", new MpqFolder("otherFolder")
                },
            },
        });

        // act
        MpqFolder? retrievedFolder = mpqFolder.GetFolder("folder1");

        // assert
        retrievedFolder.Should().NotBeNull();
        retrievedFolder!.Files.Count.Should().Be(1);
        retrievedFolder.Folders.Count.Should().Be(1);
        retrievedFolder.Name.Should().Be("folderName");
    }

    [TestMethod]
    public void GetFolder_FolderNotFound_ReturnsNull()
    {
        // arrange
        MpqFolder mpqFolder = new("name");
        mpqFolder.Files.Add("file1", new MpqFile("fileName"));
        mpqFolder.Folders.Add("folder1", new MpqFolder("folderName"));

        // act
        MpqFolder? retrievedFolder = mpqFolder.GetFolder("someFolderThatDoesntExist");

        // assert
        retrievedFolder.Should().BeNull();
    }
}
