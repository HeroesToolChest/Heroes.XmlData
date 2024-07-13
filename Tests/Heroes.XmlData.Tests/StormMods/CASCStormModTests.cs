using CASCLib;
using Heroes.XmlData.CASC;

namespace Heroes.XmlData.Tests.StormMods;

[TestClass]
public class CASCStormModTests
{
    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly ICASCHeroesStorage _cascHeroesStorage;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public CASCStormModTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    [DataRow("test.stormmod", 0, 2)]
    [DataRow("core.stormmod", 2, 2)]
    [DataRow("heroesdata.stormmod", 2, 2)]
    public void LoadGameDataDirectory_HasGameDataDirectories_AddsElementsToCaches(string stormmod, int dataObjectTypes, int stormElements)
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCStormMod cascStormMod = new(cascHeroesSource, stormmod, StormModType.Normal);

        CASCFolder rootFolder = new("name");
        rootFolder.Folders.Add("mods", new CASCFolder("mods")
        {
            Folders =
            {
                {
                    "test.stormmod", new CASCFolder("test.stormmod")
                    {
                        Folders =
                        {
                            {
                                "base.stormdata", new CASCFolder("base.stormdata")
                                {
                                    Folders =
                                    {
                                        {
                                            "gamedata", new CASCFolder("gamedata")
                                            {
                                                Files =
                                                {
                                                    {
                                                        "file1data.xml", new CASCFile(111, "file1data.xml")
                                                    },
                                                    {
                                                        "file2data.xml", new CASCFile(222, "file2data.xml")
                                                    },
                                                    {
                                                        "file3data.xml", new CASCFile(333, "file3data.xml")
                                                    },
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                {
                    "core.stormmod", new CASCFolder("core.stormmod")
                    {
                        Folders =
                        {
                            {
                                "base.stormdata", new CASCFolder("base.stormdata")
                                {
                                    Folders =
                                    {
                                        {
                                            "gamedata", new CASCFolder("gamedata")
                                            {
                                                Files =
                                                {
                                                    {
                                                        "file1data.xml", new CASCFile(111, "file1data.xml")
                                                    },
                                                    {
                                                        "file2data.xml", new CASCFile(222, "file2data.xml")
                                                    },
                                                    {
                                                        "file3data.xml", new CASCFile(333, "file3data.xml")
                                                    },
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
                {
                    "heroesdata.stormmod", new CASCFolder("heroesdata.stormmod")
                    {
                        Folders =
                        {
                            {
                                "base.stormdata", new CASCFolder("base.stormdata")
                                {
                                    Folders =
                                    {
                                        {
                                            "gamedata", new CASCFolder("gamedata")
                                            {
                                                Files =
                                                {
                                                    {
                                                        "file1data.xml", new CASCFile(111, "file1data.xml")
                                                    },
                                                    {
                                                        "file2data.xml", new CASCFile(222, "file2data.xml")
                                                    },
                                                    {
                                                        "file3data.xml", new CASCFile(333, "file3data.xml")
                                                    },
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        });

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        using MemoryStream stream1 = new();
        using StreamWriter writer1 = new(stream1);
        writer1.WriteLine("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CBehaviorBuff default=\"1\"></CBehaviorBuff></Catalog>");
        writer1.Flush();
        stream1.Position = 0;

        using MemoryStream stream2 = new();
        using StreamWriter writer2 = new(stream2);
        writer2.WriteLine("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CElementBuff default=\"1\"></CElementBuff></Catalog>");
        writer2.Flush();
        stream2.Position = 0;

        _cascHeroesStorage.CASCHandlerWrapper.FileExists("file1data.xml").Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists("file2data.xml").Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists("file3data.xml").Returns(false);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile("file1data.xml").Returns(stream1);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile("file2data.xml").Returns(stream2);

        // act
        cascStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(dataObjectTypes);
        stormStorage.StormCache.StormElementByElementType.Should().HaveCount(stormElements);
        cascStormMod.StormModStorage.NotFoundDirectories.Should().BeEmpty();
        cascStormMod.StormModStorage.NotFoundFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadGameDataDirectory_NoGameDirectoryFound_NotFoundDirectoriesAdded()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        CASCStormMod cascStormMod = new(cascHeroesSource, "test.stormmod", StormModType.Normal);

        _cascHeroesStorage.CASCFolderRoot.Returns(new CASCFolder("cascfolder"));

        // act
        cascStormMod.LoadGameDataDirectory();

        // assert
        cascStormMod.StormModStorage.NotFoundDirectories.Should().HaveCount(1).And
            .SatisfyRespectively(
                first =>
                {
                    first.Path.Should().Be(Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata"));
                    first.PathType.Should().Be(StormPathType.CASC);
                    first.StormModName.Should().Be("test");
                });

        stormStorage.StormCache.DataObjectTypeByElementType.Should().BeEmpty();
        stormStorage.StormCache.StormElementByElementType.Should().BeEmpty();
    }
}
