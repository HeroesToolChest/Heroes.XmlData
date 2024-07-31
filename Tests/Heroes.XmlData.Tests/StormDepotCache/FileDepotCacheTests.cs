using CASCLib;

namespace Heroes.XmlData.Tests.StormDepotCache;

[TestClass]
public class FileDepotCacheTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public FileDepotCacheTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void LoadDepotCache_HasFilesInDirectory_LoadsData()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "8d554.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "8d554.s2ma"))) },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "b90c6.s2ma"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "b90c6.s2ma"))) },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "94a00.s2mv"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "94a00.s2mv"))) },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "81f5f8.s2mv"), new MockFileData(File.ReadAllBytes(Path.Join(TestFilesFolder, "81f5f8.s2mv"))) },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "temp.text"), new MockFileData("temp") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileDepotCache fileDepotCache = new(mockFileSystem, fileHeroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        fileHeroesSource.S2MVPaths.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "94a00.s2mv"));
                },
                second =>
                {
                    second.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "81f5f8.s2mv"));
                });
        fileHeroesSource.S2MVPropertiesByHashCode.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Value.MapLink.Should().Be("Volskaya");
                    first.Value.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "94a00.s2mv"));
                    first.Value.DocInfoIconFile.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.Value.HeaderTitle.Should().Be("Volskaya Foundry");
                    first.Value.LoadingImage.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.Value.MapSize.Should().Be(new Point(248, 208));
                    first.Value.PreviewLargeImage.Should().Be(Path.Join("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
                    first.Value.CustomLayout.Should().Be(Path.Join("ui", "layout", "loadingscreens", "volskaya_loading.stormlayout"));
                    first.Value.CustomFrame.Should().Be(Path.Join("volskaya_loading", "screenmaploading_volskaya"));
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
                    second.Value.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "81f5f8.s2mv"));
                    second.Value.DocInfoIconFile.Should().Be("storm_ui_gamemode_mapselect_cursedhollow.png");
                    second.Value.HeaderTitle.Should().Be("Cursed Hollow");
                    second.Value.LoadingImage.Should().Be(Path.Join("assets", "textures", "ui_ingame_mapmechanic_loadscreen_cursedhollow.dds"));
                    second.Value.MapSize.Should().Be(new Point(256, 216));
                    second.Value.PreviewLargeImage.Should().Be("replayspreviewimage.tga");
                    second.Value.CustomLayout.Should().Be(Path.Join("ui", "layout", "loadingscreens", "cursedhollow_loading.stormlayout"));
                    second.Value.CustomFrame.Should().Be(Path.Join("cursedhollow_loading", "screenmaploading_cursedhollow"));
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

        fileHeroesSource.S2MAPaths.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "8d554.s2ma"));
                },
                second =>
                {
                    second.Should().EndWith(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "b90c6.s2ma"));
                });
        fileHeroesSource.S2MAProperties.Should().HaveCount(2)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.S2MVProperties.Should().NotBeNull();
                    first.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "8d554.s2ma"));
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
                    second.DirectoryPath.Should().Be(Path.Join(Path.DirectorySeparatorChar.ToString(), "core.stormmod", "base.stormdata", "depotcache", "b90c6.s2ma"));
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
        fileHeroesSource.S2MAPropertiesByTitle.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadDepotCache_HasNoFilesInDirectory_ShouldAllBeEmpty()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache", "temp.text"), new MockFileData("temp") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileDepotCache fileDepotCache = new(mockFileSystem, fileHeroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        fileHeroesSource.S2MVPaths.Should().BeEmpty();
        fileHeroesSource.S2MVPropertiesByHashCode.Should().BeEmpty();
        fileHeroesSource.S2MAPaths.Should().BeEmpty();
        fileHeroesSource.S2MAProperties.Should().BeEmpty();
        fileHeroesSource.S2MAPropertiesByTitle.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadDepotCache_MissingDirectory_AddsDirectoryToNotFound()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileDepotCache fileDepotCache = new(mockFileSystem, fileHeroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        fileHeroesSource.StormStorage.StormCache.NotFoundDirectoriesList.Should().ContainSingle()
            .And
            .SatisfyRespectively(
            first =>
            {
                first.Path.Should().Be(Path.Join("mods", "core.stormmod", "base.stormdata", "depotcache"));
                first.PathType.Should().Be(StormPathType.File);
                first.StormModName.Should().Be(string.Empty);
            });
    }
}
