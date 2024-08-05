using CASCLib;
using System.IO.Abstractions;

namespace Heroes.XmlData.Tests.StormMods;

[TestClass]
public class FileStormModTests
{
    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public FileStormModTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void LoadStormData_AllHasData_AddedData()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "base.stormdata", "buildid.txt"), new MockFileData($"B1{Environment.NewLine}") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata", "file1data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CBehaviorBuff default=\"1\"></CBehaviorBuff></Catalog>") },
            {
                Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes>
  <Catalog path=""GameData/Heroes/Common/HitTestData.xml"" />
</Includes>
")
            },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata", "heroes", "common", "hittestdata.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog></Catalog>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "fontstyles.stormstyle"), new MockFileData("<Constant name=\"ColorEnergy\" val=\"339df5\" />") },
            {
                Path.Join("mods", "test.stormmod", "base.stormdata", "includes.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes id=""Mods/HeroesData.StormMod"">
  <Path value=""Mods/HeroMods/test1.StormMod"" />
</Includes>
")
            },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "descindex.stormlayout"), new MockFileData(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?><Desc></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "loadingscreens", "descindex.stormlayout"), new MockFileData(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?><Desc></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata", "assets.txt"), new MockFileData("id1=value1") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal));

        // act
        fileStormMod.LoadStormData();

        // assert
        stormStorage.StormCache.StormElementByElementType.Should().ContainSingle();
        fileStormMod.StormModStorage.BuildId.Should().Be(1);
        fileStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(2);
        fileStormMod.StormModStorage.AddedXmlFontStyleFilePaths.Should().ContainSingle();
        fileStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(2);
        fileStormMod.StormModStorage.AddedAssetsFilePaths.Should().ContainSingle();
        fileHeroesSource.StormStorage.StormModStorages.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormGameStrings_HasGameStringFile_AddsGameStringFile()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "test.stormmod", "base.stormdata", "includes.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes id=""Mods/HeroesData.StormMod"">
  <Path value=""Mods/HeroMods/test1.StormMod"" />
</Includes>
")
            },
            { Path.Join("mods", "test.stormmod", "enus.stormdata", "localizeddata", "gamestrings.txt"), new MockFileData($"﻿Abil/Activity/AbathurUltimateEvolution=Ultimate Evolution{Environment.NewLine}Abil/Activity/AerialBlitzkrieg=Aerial Blitzkrieg") },
            { Path.Join("mods", "heromods", "test1.stormmod", "enus.stormdata", "localizeddata", "gamestrings.txt"), new MockFileData($"﻿10 Seconds=10 Seconds{Environment.NewLine}40 seconds=40 seconds") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal));

        fileStormMod.LoadIncludesStormMods();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        fileStormMod.StormModStorage.AddedGameStringFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormGameStrings_GameStringFileNotFound_AddsToFileNotFound()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "test.stormmod", "base.stormdata", "includes.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes id=""Mods/HeroesData.StormMod"">
  <Path value=""Mods/HeroMods/test1.StormMod"" />
</Includes>
")
            },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal));

        fileStormMod.LoadIncludesStormMods();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        fileStormMod.StormModStorage.NotFoundFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void GetStormMapMods_HasMapDependencies_ReturnsCorrectOrderOfMods()
    {
        // arrange
        FileStormMod fileStormMod = GetFileStormModForMapMods();

        S2MAProperties properties = new();
        properties.MapDependencies.Add(new MapDependency()
        {
            BnetName = string.Empty,
            BnetNamespace = 0,
            BnetVersionMinor = 0,
            BnetVersionMajor = 0,
            LocalFile = Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayafoundry.stormmod"),
        });

        // act
        List<IStormMod> stormMapMods = fileStormMod.GetStormMapMods(properties).ToList();

        // assert
        stormMapMods.Should().HaveCount(6).And
            .SatisfyRespectively(
                first =>
                {
                    first.Name.Should().Be("overwatchdata");
                },
                second =>
                {
                    second.Name.Should().Be("conveyorbelts");
                },
                third =>
                {
                    third.Name.Should().Be("volskayadata");
                },
                four =>
                {
                    four.Name.Should().Be("volskayamechanics");
                },
                five =>
                {
                    five.Name.Should().Be("volskayasound");
                },
                six =>
                {
                    six.Name.Should().Be("volskayafoundry");
                });
    }

    [TestMethod]
    public void LoadDocumentInfoFile_HasDocumentInfoFiles_ReturnsCorrectOrderOfMods()
    {
        // arrange
        FileStormMod fileStormMod = GetFileStormModForMapMods();

        // act
        List<IStormMod> stormMods = fileStormMod.LoadDocumentInfoFile();

        // assert
        stormMods.Should().HaveCount(8).And
            .SatisfyRespectively(
                first =>
                {
                    first.Name.Should().Be("heroesdata");
                },
                second =>
                {
                    second.Name.Should().Be("overwatchdata");
                },
                third =>
                {
                    third.Name.Should().Be("heroesdata");
                },
                four =>
                {
                    four.Name.Should().Be("heroesdata");
                },
                five =>
                {
                    five.Name.Should().Be("conveyorbelts");
                },
                six =>
                {
                    six.Name.Should().Be("volskayadata");
                },
                seven =>
                {
                    seven.Name.Should().Be("volskayamechanics");
                },
                eight =>
                {
                    eight.Name.Should().Be("volskayasound");
                });
    }

    [TestMethod]
    public void LoadFontStyleFile_HasFile_AddsFontStyle()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "fontstyles.stormstyle"), new MockFileData("<Constant name=\"ColorEnergy\" val=\"339df5\" />") },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadFontStyleFile();

        // assert
        fileStormMod.StormModStorage.AddedXmlFontStyleFilePaths.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadFontStyleFile_NoFileFound_AddsFontStyle()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadFontStyleFile();

        // assert
        fileStormMod.StormModStorage.AddedXmlFontStyleFilePaths.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadBuildIdFile_HasFileWithNumber_ShouldSetBuildId()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "base.stormdata", "buildid.txt"), new MockFileData($"B999{Environment.NewLine}") },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadBuildIdFile();

        // assert
        fileStormMod.StormModStorage.BuildId.Should().Be(999);
    }

    [TestMethod]
    public void LoadBuildIdFile_NoFileFound_ShouldNotSetBuildId()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadBuildIdFile();

        // assert
        fileStormMod.StormModStorage.BuildId.Should().BeNull();
    }

    [TestMethod]
    public void LoadGameDataXmlFile_HasXmlFile_AddsNoXmlFiles()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadGameDataXmlFile();

        // assert
        fileStormMod.StormModStorage.AddedXmlDataFilePaths.Should().BeEmpty();
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataXmlFile_HasNoCatalogElements_AddsNoXmlFiles()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes>
</Includes>
")
            },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadGameDataXmlFile();

        // assert
        fileStormMod.StormModStorage.AddedXmlDataFilePaths.Should().BeEmpty();
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataXmlFile_HasCatalogElements_AddsXmlFiles()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes>
  <Catalog path=""GameData/Heroes/Common/HitTestData.xml"" />
  <Catalog path=""GameData/Heroes/Common/GenericCursorData.xml"" />
  <Catalog path=""Heroes/Common/GenericEffectData.xml"" />
  <Catalog path="""" />
  <Catalog path="" "" />
  <Catalog />
</Includes>
")
            },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "heroes", "common", "hittestdata.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog></Catalog>") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "heroes", "common", "genericcursordata.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog></Catalog>") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "heroes", "common", "genericeffectdata.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog></Catalog>") },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadGameDataXmlFile();

        // assert
        fileStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(2);
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadIncludesStormMods_NoXmlFile_AddNoMods()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "top.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadIncludesStormMods();

        // assert
        fileHeroesSource.StormStorage.StormModStorages.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadIncludesStormMods_HasPathNoElements_AddNoMods()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "top.stormmod", "base.stormdata", "includes.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes id=""Mods/HeroesData.StormMod"">
</Includes>
")
            },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "top.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadIncludesStormMods();

        // assert
        fileHeroesSource.StormStorage.StormModStorages.Should().BeEmpty();
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadIncludesStormMods_HasPathElements_AddMods()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "top.stormmod", "base.stormdata", "includes.xml"), new MockFileData(
@"<?xml version=""1.0"" encoding=""us-ascii""?>
<Includes id=""Mods/HeroesData.StormMod"">
  <Path value=""Mods/HeroMods/test1.StormMod"" />
  <Path value=""Mods/HeroMods/test2.StormMod"" />
  <Path value="""" />
  <Path value="" "" />
  <Path />
</Includes>
")
            },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "top.stormmod", StormModType.Normal);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test2.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test2.stormmod"), StormModType.Normal));

        // act
        fileStormMod.LoadIncludesStormMods();

        // assert
        fileHeroesSource.StormStorage.StormModStorages.Should().HaveCount(2);
        fileHeroesSource.StormStorage.StormModStorages.Should().SatisfyRespectively(
            first =>
            {
                first.Name.Should().Be("test1");
            },
            second =>
            {
                second.Name.Should().Be("test2");
            });
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataDirectory_AreAllDataObjectTypes_AddsElementsToCache()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "accumulatordata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAccumulator default="1" id="BaseAccumulator"/></Catalog>""") },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "abildata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAbil default="1"/></Catalog>""") },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "innerfolder", "armordata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CArmor default="1"/></Catalog>""") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "core.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(2);
        stormStorage.StormCache.ElementTypesByDataObjectType.Should().HaveCount(2);
        stormStorage.StormCache.StormElementByElementType.Should().ContainSingle();
        stormStorage.StormCache.StormElementsByDataObjectType.Should().ContainSingle();
        fileStormMod.StormModStorage.NotFoundDirectories.Should().BeEmpty();
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataDirectory_AreAllDataObjectTypesInTwoStormModTypes_AddsElementsToCaches()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "abildata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAbil default="1"/><CAbilEffectInstant default="1"/></Catalog>""") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "abildata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAbilEffectInstant default="1"/></Catalog>""") },
            { Path.Join("mods", "map.stormmod", "base.stormdata", "gamedata", "abildata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAbilEffectInstant default="1"/></Catalog>""") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileCoreStormMod = new(mockFileSystem, fileHeroesSource, "core.stormmod", StormModType.Normal);
        FileStormMod fileHeroesDataStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);
        FileStormMod fileMapDataStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Map);

        // act
        fileCoreStormMod.LoadGameDataDirectory();
        fileHeroesDataStormMod.LoadGameDataDirectory();
        fileMapDataStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(2);
        stormStorage.StormCache.ElementTypesByDataObjectType.Should().ContainSingle();
        stormStorage.StormCache.StormElementByElementType.Should().HaveCount(2);
        stormStorage.StormCache.StormElementsByDataObjectType.Should().BeEmpty();

        stormStorage.StormMapCache.DataObjectTypeByElementType.Should().ContainSingle();
        stormStorage.StormMapCache.ElementTypesByDataObjectType.Should().ContainSingle();
        stormStorage.StormMapCache.StormElementByElementType.Should().ContainSingle();
        stormStorage.StormMapCache.StormElementsByDataObjectType.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameDataDirectory_MapElements_AddsElementsToCaches()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "announcerpackdata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAnnouncerPack default="1"/></Catalog>""") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "announcers", "adjutantvodata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAnnouncerPack id="Adjutant" parent="StormAnnouncerPackQuick" heroid="AI"/></Catalog>""") },
            { Path.Join("mods", "map.stormmod", "base.stormdata", "gamedata", "mymapannouncer.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CAnnouncerPack id="Special"/></Catalog>""") },
            { Path.Join("mods", "map.stormmod", "base.stormdata", "gamedata", "lightdata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CLight id="EditorTestLight" parent="default"/></Catalog>""") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileCoreStormMod = new(mockFileSystem, fileHeroesSource, "core.stormmod", StormModType.Normal);
        FileStormMod fileHeroesDataStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);
        FileStormMod fileMapDataStormMod = new(mockFileSystem, fileHeroesSource, "map.stormmod", StormModType.Map);

        // act
        fileCoreStormMod.LoadGameDataDirectory();
        fileHeroesDataStormMod.LoadGameDataDirectory();
        fileMapDataStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().ContainSingle();
        stormStorage.StormCache.ElementTypesByDataObjectType.Should().ContainSingle();
        stormStorage.StormCache.StormElementByElementType.Should().ContainSingle();
        stormStorage.StormCache.StormElementsByDataObjectType.Should().BeEmpty();

        stormStorage.StormMapCache.DataObjectTypeByElementType.Should().ContainSingle();
        stormStorage.StormMapCache.ElementTypesByDataObjectType.Should().ContainSingle();
        stormStorage.StormMapCache.StormElementByElementType.Should().BeEmpty();
        stormStorage.StormMapCache.StormElementsByDataObjectType.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadGameDataDirectory_FindsExistingDataObjectType_AddsElementsToCaches()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "requirementdata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CRequirement default="1"/></Catalog>""") },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "requirementnodedata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CRequirementNode default="1"/><CRequirementConst default="1"/></Catalog>""") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "requirementnodedata.xml"), new MockFileData("""<?xml version="1.0" encoding="us-ascii"?><Catalog><CRequirementConst id="01081602903" /><CRequirementAnd id="AndEqCountBehaviorMountedCompleteOnlyAtUnit0"/></Catalog>""") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileCoreStormMod = new(mockFileSystem, fileHeroesSource, "core.stormmod", StormModType.Normal);
        FileStormMod fileHeroesDataStormMod = new(mockFileSystem, fileHeroesSource, "heroesdata.stormmod", StormModType.Normal);

        // act
        fileCoreStormMod.LoadGameDataDirectory();
        fileHeroesDataStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(4);
        stormStorage.StormCache.ElementTypesByDataObjectType.Should().HaveCount(2);
        stormStorage.StormCache.StormElementByElementType.Should().HaveCount(3);
        stormStorage.StormCache.StormElementsByDataObjectType.Should().ContainSingle();
        stormStorage.StormCache.StormElementsByDataObjectType["requirement"].Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadGameDataDirectory_NoGameDirectoryFound_NotFoundDirectoriesAdded()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadGameDataDirectory();

        // assert
        fileStormMod.StormModStorage.NotFoundDirectories.Should().ContainSingle().And
            .SatisfyRespectively(
                first =>
                {
                    first.Path.Should().Be(Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata"));
                    first.PathType.Should().Be(StormPathType.File);
                    first.StormModName.Should().Be("test");
                });

        stormStorage.StormCache.DataObjectTypeByElementType.Should().BeEmpty();
        stormStorage.StormCache.StormElementByElementType.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormLayoutDirectory_HasStormLayoutDirectories_LoadsLayoutPaths()
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "loadingscreens", "layout1.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "loadingscreens", "layout2.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "homescreens", "layout1.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "layout1.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "ui", "layout", "layout2.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("mods", "test.stormmod", "ui", "layout", "layout1.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
            { Path.Join("ui", "layout", "layout1.stormlayout"), new MockFileData("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Desc><DescFlags val=\"Locked\" /></Desc>") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadStormLayoutDirectory();

        // assert
        fileStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(5);
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
    public void LoadStormLayoutDirectory_NoStormLayoutDirectoryFound_NothingIsAdded()
    {
        // arrange
        IFileSystem fileSystem = Substitute.For<IFileSystem>();

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(fileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        // act
        fileStormMod.LoadStormLayoutDirectory();

        // assert
        fileSystem.Received().Directory.Exists(Arg.Any<string>());
        fileStormMod.StormModStorage.FoundLayoutFilePaths.Should().BeEmpty();
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().BeEmpty();
    }

    private FileStormMod GetFileStormModForMapMods()
    {
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "volskayafoundry.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Icon>
        <Value>Assets\Textures\storm_ui_homescreenbackground_volskaya.dds</Value>
    </Icon>
    <MatchMakerTag>
        <Value>Hero</Value>
    </MatchMakerTag>
    <Dependencies>
        <Value>bnet:Volskaya Sound/0.0/1146,file:Mods\heroesmapmods/battlegroundmapmods/volskayasound.stormmod</Value>
    </Dependencies>
    <ModifiableDependencies>
        <Value>Mods/HeroesData.StormMod</Value>
        <Value>Mods\heroesmapmods/battlegroundmapmods/overwatchdata.stormmod</Value>
        <Value>Mods\heroesmapmods/battlegroundmapmods/volskayamechanics.stormmod</Value>
    </ModifiableDependencies>
</DocInfo>
")
            },
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Flags>
        <Value>PreloadNoTrigger</Value>
    </Flags>
    <Dependencies>
        <Value>bnet:Volskaya Mechanics/0.0/1145,file:Mods\heroesmapmods/battlegroundmapmods/volskayamechanics.stormmod</Value>
    </Dependencies>
    <ModifiableDependencies>
        <Value>Mods/HeroesData.StormMod</Value>
    </ModifiableDependencies>
    <Preload>
        <Value>GameLink:Unit;JungleCampIconUnit</Value>
        <Value>GameLink:Unit;RangedMinion</Value>
    </Preload>
</DocInfo>
")
            },
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Flags>
        <Value>PreloadNoTrigger</Value>
    </Flags>
    <Dependencies>
        <Value>bnet:Volskaya Data/0.0/1144,file:Mods\heroesmapmods/battlegroundmapmods/volskayadata.stormmod</Value>
        <Value>bnet:ConveyorBelts Mod/0.0/1860,file:Mods\heroesmapmods/battlegroundmapmods/conveyorbelts.stormmod</Value>
    </Dependencies>
    <ModifiableDependencies>
    </ModifiableDependencies>
    <Preload>
        <Value>GameLink:Unit;JungleCampIconUnit</Value>
        <Value>GameLink:Unit;RangedMinion</Value>
    </Preload>
</DocInfo>
")
            },
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "volskayadata.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Flags>
        <Value>PreloadNoTrigger</Value>
    </Flags>
    <Dependencies>
        <Value>bnet:Heroes of the Storm (Data Mod)/0.0/999,file:Mods/HeroesData.StormMod</Value>
        <Value>bnet:Overwatch Data (Mod)/0.0/1143,file:Mods\heroesmapmods/battlegroundmapmods/overwatchdata.stormmod</Value>
    </Dependencies>
    <ModifiableDependencies>
        <Value>Mods/HeroesData.StormMod</Value>
    </ModifiableDependencies>
    <Preload>
        <Value>GameLink:Unit;JungleCampIconUnit</Value>
        <Value>GameLink:Unit;RangedMinion</Value>
    </Preload>
</DocInfo>
")
            },
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "conveyorbelts.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Dependencies>
        <Value>bnet:Heroes of the Storm (Data Mod)/0.0/999,file:Mods/HeroesData.StormMod</Value>
    </Dependencies>
</DocInfo>
")
            },
            {
                Path.Join("mods", "heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod", "documentinfo"), new MockFileData(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocInfo>
    <Dependencies>
        <Value>bnet:Heroes of the Storm (Data Mod)/0.0/999,file:Mods/HeroesData.StormMod</Value>
    </Dependencies>
    <ModifiableDependencies>
        <Value>Mods/HeroesData.StormMod</Value>
    </ModifiableDependencies>
</DocInfo>

")
            },
        });

        FileHeroesSource fileHeroesSource = new(new StormStorage(false), _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayafoundry.stormmod"), StormModType.Map);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayafoundry.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayafoundry.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayadata.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayadata.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "conveyorbelts.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "conveyorbelts.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"), StormModType.Map));

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesdata.stormmod"), StormModType.Map)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heroesdata.stormmod"), StormModType.Map));

        return fileStormMod;
    }
}
