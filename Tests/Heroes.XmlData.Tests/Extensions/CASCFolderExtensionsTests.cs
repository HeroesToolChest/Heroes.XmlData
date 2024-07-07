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
        cascFolder.Files.Add("file1", new CASCFile(111, "fileName"));
        cascFolder.Folders.Add("folder1", new CASCFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new CASCFolder("folderName2")
                    {
                        Folders =
                        {
                            {
                                "folder3", new CASCFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new CASCFolder("folderName4")
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
        bool success = cascFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder3"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeTrue();
        resultCASCFolder.Should().NotBeNull();
        resultCASCFolder!.Name.Should().Be("folderName3");
    }

    [TestMethod]
    public void TryGetLastDirectory_NonExistDirectoryPath_ReturnsNull()
    {
        // arrange
        CASCFolder cascFolder = new("name");
        cascFolder.Files.Add("file1", new CASCFile(111, "fileName"));
        cascFolder.Folders.Add("folder1", new CASCFolder("folderName")
        {
            Folders =
            {
                {
                    "folder2", new CASCFolder("folderName2")
                    {
                        Folders =
                        {
                            {
                                "folder3", new CASCFolder("folderName3")
                                {
                                    Folders =
                                    {
                                        {
                                            "folder4", new CASCFolder("folderName4")
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
        bool success = cascFolder.TryGetLastDirectory(Path.Join("folder1", "folder2", "folder5"), out CASCFolder? resultCASCFolder);

        // assert
        success.Should().BeFalse();
        resultCASCFolder.Should().BeNull();
    }
}
