using CASCLib;
using Heroes.XmlData.CASC;
using Heroes.XmlData.Extensions;

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
    public void LoadGameDataDirectory_AreAllDataObjectTypes_AddsElementsToCache()
    {
        // arrange
        StormStorage stormStorage = new(false);
        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCStormMod cascStormMod = new(cascHeroesSource, "core.stormmod", StormModType.Normal);

        CASCFolder rootFolder = new("name");
        rootFolder.AddFile("mods/core.stormmod/base.stormdata/gamedata/accumulatordata.xml");
        rootFolder.AddFile("mods/core.stormmod/base.stormdata/gamedata/abildata.xml");
        rootFolder.AddFile("mods/core.stormmod/base.stormdata/gamedata/innerfolder/armordata.xml");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        using MemoryStream stream1 = GetMockStream("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAccumulator default="1" id="BaseAccumulator"/></Catalog>""");
        using MemoryStream stream2 = GetMockStream("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAbil default="1"/></Catalog>""");
        using MemoryStream stream3 = GetMockStream("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CArmor default="1"/></Catalog>""");

        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "accumulatordata.xml")).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "abildata.xml")).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.FileExists(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "innerfolder", "armordata.xml")).Returns(true);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "accumulatordata.xml")).Returns(stream1);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "abildata.xml")).Returns(stream2);
        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "innerfolder", "armordata.xml")).Returns(stream3);

        // act
        cascStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(2);
        stormStorage.StormCache.ElementTypesByDataObjectType.Should().HaveCount(2);
        stormStorage.StormCache.StormElementByElementType.Should().ContainSingle();
        stormStorage.StormCache.StormElementsByDataObjectType.Should().ContainSingle();
        cascStormMod.StormModStorage.NotFoundDirectories.Should().BeEmpty();
        cascStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataDirectory_NoGameDirectoryFound_NotFoundDirectoriesAdded()
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        CASCStormMod cascStormMod = new(cascHeroesSource, "test.stormmod", StormModType.Normal);

        _cascHeroesStorage.CASCFolderRoot.Returns(new CASCFolder("cascfolder"));

        // act
        cascStormMod.LoadGameDataDirectory();

        // assert
        cascStormMod.StormModStorage.NotFoundDirectories.Should().ContainSingle().And
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

    [TestMethod]
    public void LoadStormLayoutDirectory_HasStormLayoutDirectories_LoadsLayoutPaths()
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCStormMod cascStormMod = new(cascHeroesSource, "test.stormmod", StormModType.Normal);

        CASCFolder rootFolder = new("name");

        rootFolder.AddFile("mods/test.stormmod/base.stormdata/ui/layout/loadingscreens/layout1.stormlayout");
        rootFolder.AddFile("mods/test.stormmod/base.stormdata/ui/layout/loadingscreens/layout2.stormlayout");

        rootFolder.AddFile("mods/test.stormmod/base.stormdata/ui/layout/homescreens/layout1.stormlayout");

        rootFolder.AddFile("mods/test.stormmod/base.stormdata/ui/layout/layout1.stormlayout");
        rootFolder.AddFile("mods/test.stormmod/base.stormdata/ui/layout/layout2.stormlayout");

        rootFolder.AddFile("mods/test.stormmod/ui/layout/layout1.stormlayout");

        rootFolder.AddFile("ui/layout/layout1.stormlayout");

        _cascHeroesStorage.CASCFolderRoot.Returns(rootFolder);

        // act
        cascStormMod.LoadStormLayoutDirectory();

        // assert
        cascStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(5);
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().HaveCount(5).And
            .SatisfyRespectively(
                first =>
                {
                    first.Key.Should().Be(Path.Join("ui", "layout", "homescreens", "layout1.stormlayout"));
                },
                second =>
                {
                    second.Key.Should().Be(Path.Join("ui", "layout", "layout1.stormlayout"));
                },
                third =>
                {
                    third.Key.Should().Be(Path.Join("ui", "layout", "layout2.stormlayout"));
                },
                fourth =>
                {
                    fourth.Key.Should().Be(Path.Join("ui", "layout", "loadingscreens", "layout1.stormlayout"));
                },
                five =>
                {
                    five.Key.Should().Be(Path.Join("ui", "layout", "loadingscreens", "layout2.stormlayout"));
                });
    }

    [TestMethod]
    public void LoadStormLayoutDirectory_NoLayoutDirectoryFound_NothingIsAdded()
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);

        CASCStormMod cascStormMod = new(cascHeroesSource, "test.stormmod", StormModType.Normal);

        _cascHeroesStorage.CASCFolderRoot.Returns(new CASCFolder("cascfolder"));

        // act
        cascStormMod.LoadStormLayoutDirectory();

        // assert
        cascStormMod.StormModStorage.FoundLayoutFilePaths.Should().BeEmpty();
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().BeEmpty();
    }

    private static MemoryStream GetMockStream(string content)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.WriteLine(content);
        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}
