using CASCLib;
using Heroes.XmlData.CASC;

namespace Heroes.XmlData.Tests.StormDepotCache;

[TestClass]
public class CASCDepotCacheTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly ICASCHeroesStorage _cascHeroesStorage;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public CASCDepotCacheTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void LoadDepotCache_HasNoFilesInDirectory_ShouldAllBeEmpty()
    {
        // arrange
        CASCFolder rootFolder = new("name");
        rootFolder.Folders.Add("mods", new CASCFolder("mods")
        {
            Folders =
            {
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
                                            "depotcache", new CASCFolder("depotcache")
                                            {
                                                Folders =
                                                {
                                                    {
                                                        "aa", new CASCFolder("aa")
                                                        {
                                                            Folders =
                                                            {
                                                                {
                                                                    "bb", new CASCFolder("bb")
                                                                    {
                                                                        Files =
                                                                        {
                                                                            {
                                                                                "8d554.s2ma", new CASCFile(111, Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "8d554.s2ma"))
                                                                            },
                                                                            {
                                                                                "b90c6.s2ma", new CASCFile(222, Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "b90c6.s2ma"))
                                                                            },
                                                                            {
                                                                                "94a00.s2mv", new CASCFile(333, Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "94a00.s2mv"))
                                                                            },
                                                                            {
                                                                                "81f5f8.s2mv", new CASCFile(444, Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "81f5f8.s2mv"))
                                                                            },
                                                                            {
                                                                                "temp.text", new CASCFile(555, Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "temp.text"))
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
                                }
                            },
                        },
                    }
                },
            },
        });

        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "8d554.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "b90c6.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "b90c6.s2ma")));
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "94a00.s2mv")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "94a00.s2mv")));
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "81f5f8.s2mv")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "81f5f8.s2mv")));

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCDepotCache cascDepotCache = new(cascHeroesSource);

        // act
        cascDepotCache.LoadDepotCache();

        // assert
        cascHeroesSource.S2MVPaths.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "94a00.s2mv"));
                },
                second =>
                {
                    second.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "81f5f8.s2mv"));
                });
        cascHeroesSource.S2MVPropertiesByHashCode.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Value.MapLink.Should().Be("Volskaya");
                    first.Value.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "94a00.s2mv"));
                    first.Value.DocInfoIconFile.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.Value.HeaderTitle.Should().Be("Volskaya Foundry");
                    first.Value.LoadingImage.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.Value.MapDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            mapDepend1 =>
                            {
                                mapDepend1.LocalFile.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
                                mapDepend1.BnetName.Should().Be("Volskaya Sound");
                                mapDepend1.BnetNamespace.Should().Be(1146);
                                mapDepend1.BnetVersionMajor.Should().Be(0);
                                mapDepend1.BnetVersionMinor.Should().Be(0);
                            });
                    first.Value.MapSize.X.Should().Be(248);
                    first.Value.MapSize.Y.Should().Be(208);
                    first.Value.ModifiableDependencies.Should().HaveCount(3)
                        .And
                        .SatisfyRespectively(
                            modDepend1 =>
                            {
                                modDepend1.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesdata.stormmod"));
                            },
                            modDepend2 =>
                            {
                                modDepend2.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"));
                            },
                            modDepend3 =>
                            {
                                modDepend3.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"));
                            });
                },
                second =>
                {
                    second.Value.MapLink.Should().Be("CursedHollow");
                    second.Value.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "81f5f8.s2mv"));
                    second.Value.DocInfoIconFile.Should().Be("storm_ui_gamemode_mapselect_cursedhollow.png");
                    second.Value.HeaderTitle.Should().Be("Cursed Hollow");
                    second.Value.LoadingImage.Should().Be(Path.Join("assets", "textures", "ui_ingame_mapmechanic_loadscreen_cursedhollow.dds"));
                    second.Value.MapDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            mapDepend1 =>
                            {
                                mapDepend1.LocalFile.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "cursedhollow.stormmod"));
                                mapDepend1.BnetName.Should().Be("Cursed Hollow (Mod)");
                                mapDepend1.BnetNamespace.Should().Be(1497);
                                mapDepend1.BnetVersionMajor.Should().Be(0);
                                mapDepend1.BnetVersionMinor.Should().Be(0);
                            });
                    second.Value.MapSize.X.Should().Be(256);
                    second.Value.MapSize.Y.Should().Be(216);
                    second.Value.ModifiableDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            modDepend1 =>
                            {
                                modDepend1.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesdata.stormmod"));
                            });
                });

        cascHeroesSource.S2MAPaths.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "8d554.s2ma"));
                },
                second =>
                {
                    second.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "b90c6.s2ma"));
                });
        cascHeroesSource.S2MAProperties.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.S2MVProperties.Should().NotBeNull();
                    first.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "8d554.s2ma"));
                    first.DocInfoIconFile.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.MapDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            mapDepend1 =>
                            {
                                mapDepend1.LocalFile.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
                                mapDepend1.BnetName.Should().Be("Volskaya Sound");
                                mapDepend1.BnetNamespace.Should().Be(1146);
                                mapDepend1.BnetVersionMajor.Should().Be(0);
                                mapDepend1.BnetVersionMinor.Should().Be(0);
                            });
                    first.MapId.Should().Be("Volskaya");
                    first.ModifiableDependencies.Should().HaveCount(3)
                        .And
                        .SatisfyRespectively(
                            modDepend1 =>
                            {
                                modDepend1.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesdata.stormmod"));
                            },
                            modDepend2 =>
                            {
                                modDepend2.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"));
                            },
                            modDepend3 =>
                            {
                                modDepend3.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"));
                            });
                },
                second =>
                {
                    second.S2MVProperties.Should().NotBeNull();
                    second.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "aa", "bb", "b90c6.s2ma"));
                    second.DocInfoIconFile.Should().Be("storm_ui_gamemode_mapselect_cursedhollow.png");
                    second.MapDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            mapDepend1 =>
                            {
                                mapDepend1.LocalFile.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesmapmods", "battlegroundmapmods", "cursedhollow.stormmod"));
                                mapDepend1.BnetName.Should().Be("Cursed Hollow (Mod)");
                                mapDepend1.BnetNamespace.Should().Be(1497);
                                mapDepend1.BnetVersionMajor.Should().Be(0);
                                mapDepend1.BnetVersionMinor.Should().Be(0);
                            });
                    second.MapId.Should().Be("CursedHollow");
                    second.ModifiableDependencies.Should().ContainSingle()
                        .And
                        .SatisfyRespectively(
                            modDepend1 =>
                            {
                                modDepend1.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "heroesdata.stormmod"));
                            });
                });
        cascHeroesSource.S2MAPropertiesByTitle.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadDepotCache_MissingDirectory_AddsDirectoryToNotFound()
    {
        // arrange
        CASCFolder rootFolder = new("name");
        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCDepotCache cascDepotCache = new(cascHeroesSource);

        // act
        cascDepotCache.LoadDepotCache();

        // assert
        cascHeroesSource.StormStorage.StormCache.NotFoundDirectoriesList.Should().ContainSingle()
            .And
            .SatisfyRespectively(
            first =>
            {
                first.Path.Should().Be(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache"));
                first.PathType.Should().Be(StormPathType.CASC);
                first.StormModDirectoryPath.Should().Be(string.Empty);
                first.StormModName.Should().Be(string.Empty);
            });
    }
}
