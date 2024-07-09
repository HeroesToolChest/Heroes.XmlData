using CASCLib;

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
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, "test.stormmod", StormModType.Normal);

        _stormModFactory.CreateFileStormModInstance(fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal)
            .Returns(new FileStormMod(mockFileSystem, fileHeroesSource, Path.Join($"{Path.DirectorySeparatorChar}", "heromods", "test1.stormmod"), StormModType.Normal));

        // act
        fileStormMod.LoadStormData();

        // assert
        fileStormMod.StormModStorage.BuildId.Should().Be(1);
        stormStorage.StormCache.StormElementByElementType.Should().ContainSingle();
        fileStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(2);
        fileStormMod.StormModStorage.AddedXmlFontStyleFilePaths.Should().ContainSingle();
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
    [DataRow("test.stormmod", 0, 2)]
    [DataRow("core.stormmod", 2, 2)]
    [DataRow("heroesdata.stormmod", 2, 2)]
    public void LoadGameDataDirectory_HasGameDataDirectories_AddsElementsToCaches(string stormmod, int dataObjectTypes, int stormElements)
    {
        // arrange
        MockFileSystem mockFileSystem = new(new Dictionary<string, MockFileData>
        {
            { Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata", "file1data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CBehaviorBuff default=\"1\"></CBehaviorBuff></Catalog>") },
            { Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata", "file2data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CElementBuff default=\"1\"></CElementBuff></Catalog>") },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "file1data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CFoodBuff default=\"1\"></CFoodBuff></Catalog>") },
            { Path.Join("mods", "core.stormmod", "base.stormdata", "gamedata", "file2data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CPetBuff default=\"1\"></CPetBuff></Catalog>") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "file1data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CWoodBuff default=\"1\"></CWoodBuff></Catalog>") },
            { Path.Join("mods", "heroesdata.stormmod", "base.stormdata", "gamedata", "file2data.xml"), new MockFileData("<?xml version=\"1.0\" encoding=\"us-ascii\"?><Catalog><CRockBuff default=\"1\"></CRockBuff></Catalog>") },
        });

        StormStorage stormStorage = new(false);
        FileHeroesSource fileHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, "mods", _backgroundWorkerEx);
        FileStormMod fileStormMod = new(mockFileSystem, fileHeroesSource, stormmod, StormModType.Normal);

        // act
        fileStormMod.LoadGameDataDirectory();

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType.Should().HaveCount(dataObjectTypes);
        stormStorage.StormCache.StormElementByElementType.Should().HaveCount(stormElements);
        fileStormMod.StormModStorage.NotFoundDirectories.Should().BeEmpty();
        fileStormMod.StormModStorage.NotFoundFiles.Should().BeEmpty();
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
        fileStormMod.StormModStorage.NotFoundDirectories.Should().HaveCount(1).And
            .SatisfyRespectively(
                first =>
                {
                    first.Path.Should().Be(Path.Join("mods", "test.stormmod", "base.stormdata", "gamedata"));
                    first.PathType.Should().Be(StormPathType.File);
                    first.StormModDirectoryPath.Should().Be("test.stormmod");
                    first.StormModName.Should().Be("test");
                });

        stormStorage.StormCache.DataObjectTypeByElementType.Should().BeEmpty();
        stormStorage.StormCache.StormElementByElementType.Should().BeEmpty();
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
