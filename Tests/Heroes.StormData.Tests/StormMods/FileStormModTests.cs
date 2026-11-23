namespace Heroes.StormData.StormMods.Tests;

[TestClass]
internal sealed class FileStormModTests
{
    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
    private static readonly string _stormModDirectoryPath = Path.Combine(_modsDirectoryPath, "heroesdata.stormmod");
    private static readonly string _baseStormDataPath = Path.Combine(_stormModDirectoryPath, "base.stormdata");
    private static readonly string _gameDataDirectoryPath = Path.Combine(_baseStormDataPath, "GameData");

    private readonly MockFileSystem _fileSystem = new();
    private readonly SourceContainer _sourceContainer = new();
    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly IFileHeroesSource _heroesSource = Substitute.For<IFileHeroesSource>();

    public FileStormModTests()
    {
        _heroesSource.ModsDirectoryPath.Returns(_modsDirectoryPath);
        _heroesSource.SourceContainer.Returns(_sourceContainer);
        _heroesSource.StormModFactory.Returns(_stormModFactory);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void Constructor_StormModDirectoryPath_SetsProperties(StormModType stormModType)
    {
        // act
        FileStormMod fileStormMod = CreateFileStormMod(stormModType);

        // assert
        fileStormMod.Name.Should().Be("heroesdata");
        fileStormMod.DirectoryPath.Should().Be(_stormModDirectoryPath);
        fileStormMod.RelativeStormModPath.Should().Be("heroesdata.stormmod");
        fileStormMod.ModType.Should().Be(stormModType);
        fileStormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    public void Constructor_NestedStormModDirectoryPath_NameIsDirectoryNameAndRelativePathIncludesParents()
    {
        // arrange
        string stormModDirectoryPath = Path.Combine(_modsDirectoryPath, "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod");

        // act
        FileStormMod fileStormMod = new(_fileSystem, _heroesSource, stormModDirectoryPath, StormModType.Map);

        // assert
        fileStormMod.Name.Should().Be("volskayasound");
        fileStormMod.RelativeStormModPath.Should().Be(Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
    }

    [TestMethod]
    public void LoadStormFiles_StormModDirectoryDoesNotExist_LoadsNothing()
    {
        // arrange
        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        DataStorage dataStorage = GetDataStorage(StormModType.Base);
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
        dataStorage.TextCache.AssetTextsById.Dictionary.Should().BeEmpty();
        dataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().BeEmpty();
        dataStorage.FileCache.AssetFilesByRelativePath.Dictionary.Should().BeEmpty();
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
        _sourceContainer.StormModContainers.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_LoadsXmlFilesInDirectory()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "AbilData.xml"), "$AbilConst");
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "UNITDATA.XML"), "$UnitConst");

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$AbilConst", "$UnitConst");
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_StormFileIsNotInMpqArchive()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "AbilData.xml"), "$AbilConst");

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        StormFile stormFile = fileStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Subject;

        stormFile.IsInMpqArchive.Should().BeFalse();
        stormFile.MpqFilePath.Should().BeNull();
        stormFile.MpqEntryPath.Should().BeNull();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_StormFileHasModNameAndFilePath()
    {
        // arrange
        string filePath = Path.Combine(_gameDataDirectoryPath, "AbilData.xml");
        AddConstXmlFile(filePath, "$AbilConst");

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        StormXmlConstElement constElement = GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary["$AbilConst"];
        constElement.StormFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                StormModName = "heroesdata",
                RelativePath = Path.Combine("heroesdata.stormmod", "base.stormdata", "GameData", "AbilData.xml"),
                FullPath = filePath,
            });
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryFileNamesDifferInCase_LoadedInCaseInsensitiveOrder()
    {
        // arrange
        // an ordinal sort would put "B.xml" before "a.xml"
        string upperCaseFilePath = Path.Combine(_gameDataDirectoryPath, "B.xml");
        string lowerCaseFilePath = Path.Combine(_gameDataDirectoryPath, "a.xml");
        string lastFilePath = Path.Combine(_gameDataDirectoryPath, "c.xml");

        AddConstXmlFile(lastFilePath, "$CConst");
        AddConstXmlFile(upperCaseFilePath, "$BConst");
        AddConstXmlFile(lowerCaseFilePath, "$AConst");

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.ModContainer.LoadedXmlFiles.Select(x => x.FullPath).Should().Equal(lowerCaseFilePath, upperCaseFilePath, lastFilePath);
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryAndGameDataXmlFile_DirectoryFilesLoadedFirstThenCatalogsInListedOrder()
    {
        // arrange
        string directoryFilePath = Path.Combine(_gameDataDirectoryPath, "ZoneData.xml");
        string firstCatalogFilePath = Path.Combine(_gameDataDirectoryPath, "Heroes", "ZaryaData.xml");
        string secondCatalogFilePath = Path.Combine(_gameDataDirectoryPath, "Heroes", "AbathurData.xml");

        AddConstXmlFile(directoryFilePath, "$ZoneConst");
        AddConstXmlFile(firstCatalogFilePath, "$ZaryaConst");
        AddConstXmlFile(secondCatalogFilePath, "$AbathurConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameData/Heroes/ZaryaData.xml" />
              <Catalog path="GameData/Heroes/AbathurData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        // the catalog file paths are built from the normalized (lowercase) path in the gamedata.xml file
        fileStormMod.ModContainer.LoadedXmlFiles.Select(x => x.FullPath).Should().Equal(
            directoryFilePath,
            Path.Combine(_baseStormDataPath, "gamedata", "heroes", "zaryadata.xml"),
            Path.Combine(_baseStormDataPath, "gamedata", "heroes", "abathurdata.xml"));
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileListsFileInGameDataDirectory_LoadsItOnce()
    {
        // arrange
        // the file is found by the directory with the casing on disk and listed in the gamedata.xml file with a different casing
        string filePath = Path.Combine(_gameDataDirectoryPath, "AbilData.xml");
        AddConstXmlFile(filePath, "$AbilConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameData/AbilData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.FullPath.Should().Be(filePath);
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary["$AbilConst"].StormFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileListsCatalogTwice_LoadsItOnce()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "Heroes", "HeroData.xml"), "$HeroConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameData/Heroes/HeroData.xml" />
              <Catalog path="GameData\HEROES\HeroData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormFiles_CalledAgain_LoadsXmlFilesAgain()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "AbilData.xml"), "$AbilConst");

        FileStormMod fileStormMod = CreateFileStormMod();
        fileStormMod.LoadStormFiles();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        // the files already added are only tracked within one call
        fileStormMod.ModContainer.LoadedXmlFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadStormFiles_GameDataSubdirectoryAndNonXmlFiles_DoesNotLoadThem()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "Heroes", "HeroData.xml"), "$SubdirectoryConst");
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "AbilData.txt"), "$TextFileConst");

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileHasCatalogs_LoadsCatalogFilesFromGameDataDirectory()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "Heroes", "HeroData.xml"), "$HeroConst");
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "Heroes", "Abathur", "AbathurData.xml"), "$AbathurConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "Other", "OtherData.xml"), "$OtherConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <?xml version="1.0" encoding="us-ascii"?>
            <Includes>
              <Catalog path="GameData/Heroes/HeroData.xml" />
              <Catalog path="GameData\Heroes\Abathur\AbathurData.xml" />
              <Catalog path="Other/OtherData.xml" />
              <Catalog path="" />
              <Catalog />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$HeroConst", "$AbathurConst");
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileCatalogInDirectoryStartingWithGameData_DoesNotLoadFile()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameDataExtra", "ExtraData.xml"), "$ExtraConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameDataExtra/ExtraData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
        fileStormMod.ModContainer.LoadedXmlFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileCatalogFileDoesNotExist_SkipsMissingFile()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "Heroes", "HeroData.xml"), "$HeroConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameData/Heroes/MissingData.xml" />
              <Catalog path="GameData/Heroes/HeroData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().ContainSingle()
            .Which.Should().Be("$HeroConst");
    }

    [TestMethod]
    public void LoadStormFiles_AssetsTextFileExists_AddsAssetTexts()
    {
        // arrange
        // the file path is built from the lowercase constants, not the casing on disk
        string filePath = Path.Combine(_baseStormDataPath, "gamedata", "assets.txt");
        _fileSystem.AddFile(Path.Combine(_gameDataDirectoryPath, "Assets.txt"), new MockFileData("UI/HeroPortrait=Assets\\Textures\\portrait.dds\nUI/Empty="));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        var assetTexts = GetDataStorage(StormModType.Base).TextCache.AssetTextsById.Dictionary;
        assetTexts.Should().HaveCount(2);
        assetTexts["UI/HeroPortrait"].Value.Should().Be("Assets\\Textures\\portrait.dds");
        assetTexts["UI/HeroPortrait"].StormFile.Should().BeEquivalentTo(ExpectedStormFile(filePath));
        assetTexts["UI/Empty"].Value.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_FontStylesFileExists_AddsStyleConstantAndStyleElements()
    {
        // arrange
        // the file path is built from the lowercase constants, not the casing on disk
        string filePath = Path.Combine(_baseStormDataPath, "ui", "fontstyles.stormstyle");
        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "UI", "FontStyles.StormStyle"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8" standalone="yes"?>
            <StyleFile>
              <Constant name="ColorGray" val="a7a7a7" />
              <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="#ColorGray" />
            </StyleFile>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        StyleCache styleCache = GetDataStorage(StormModType.Base).StyleCache;

        StormXmlStyleConstantElement constantElement = styleCache.StormXmlStyleConstantElementByName.Dictionary.Should().ContainSingle().Which.Value;
        constantElement.Name.Should().Be("ColorGray");
        constantElement.Value.Should().Be("a7a7a7");
        constantElement.StormFiles.Should().ContainSingle().Which.Should().BeEquivalentTo(ExpectedStormFile(filePath));

        StormXmlStyleElement styleElement = styleCache.StormXmlStyleElementByName.Dictionary.Should().ContainSingle().Which.Value;
        styleElement.Name.Should().Be("ReticleEnemy");
        styleElement.Template.Should().Be("Storm_Tutorial_Reticle_Text");
        styleElement.Members["textcolor"].Value.RawValue.Should().Be("#ColorGray");
    }

    [TestMethod]
    public void LoadStormFiles_LayoutDirectoryHasStormLayoutFiles_AddsRelativeUiPaths()
    {
        // arrange
        string layoutDirectoryPath = Path.Combine(_baseStormDataPath, "UI", "Layout");
        string rootFilePath = Path.Combine(layoutDirectoryPath, "GameUI.StormLayout");
        string nestedFilePath = Path.Combine(layoutDirectoryPath, "Heroes", "HeroPanel.stormlayout");

        _fileSystem.AddFile(rootFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(nestedFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(Path.Combine(layoutDirectoryPath, "readme.txt"), new MockFileData(string.Empty));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        var layoutFiles = GetDataStorage(StormModType.Base).FileCache.StormLayoutFilesByRelativePath.Dictionary;
        layoutFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("ui", "layout", "gameui.stormlayout"),
            Path.Combine("ui", "layout", "heroes", "heropanel.stormlayout"));
        layoutFiles[Path.Combine("ui", "layout", "gameui.stormlayout")].Should().BeEquivalentTo(ExpectedStormFile(rootFilePath));
        layoutFiles[Path.Combine("ui", "layout", "heroes", "heropanel.stormlayout")].Should().BeEquivalentTo(ExpectedStormFile(nestedFilePath));
    }

    [TestMethod]
    public void LoadStormFiles_AssetsDirectoryHasDdsFiles_AddsRelativeAssetsPaths()
    {
        // arrange
        string assetsDirectoryPath = Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets");
        string rootFilePath = Path.Combine(assetsDirectoryPath, "Icon.DDS");
        string nestedFilePath = Path.Combine(assetsDirectoryPath, "Textures", "Portrait.dds");

        _fileSystem.AddFile(rootFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(nestedFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(Path.Combine(assetsDirectoryPath, "Textures", "Portrait.png"), new MockFileData(string.Empty));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        var assetFiles = GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary;
        assetFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("assets", "icon.dds"),
            Path.Combine("assets", "textures", "portrait.dds"));
        assetFiles[Path.Combine("assets", "icon.dds")].Should().BeEquivalentTo(ExpectedStormFile(rootFilePath));
        assetFiles[Path.Combine("assets", "textures", "portrait.dds")].Should().BeEquivalentTo(ExpectedStormFile(nestedFilePath));
    }

    [TestMethod]
    public void LoadStormFiles_LayoutAndAssetFileNamesDifferInCase_LoadedInCaseInsensitiveOrder()
    {
        // arrange
        // an ordinal sort would put the upper case names first
        string layoutDirectoryPath = Path.Combine(_baseStormDataPath, "UI", "Layout");
        string firstLayoutFilePath = Path.Combine(layoutDirectoryPath, "a.stormlayout");
        string secondLayoutFilePath = Path.Combine(layoutDirectoryPath, "B.stormlayout");

        string assetsDirectoryPath = Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets");
        string firstAssetFilePath = Path.Combine(assetsDirectoryPath, "a.dds");
        string secondAssetFilePath = Path.Combine(assetsDirectoryPath, "B.dds");

        _fileSystem.AddFile(secondLayoutFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(firstLayoutFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(secondAssetFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(firstAssetFilePath, new MockFileData(string.Empty));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.ModContainer.LoadedStormLayoutFiles.Select(x => x.FullPath).Should().Equal(firstLayoutFilePath, secondLayoutFilePath);
        fileStormMod.ModContainer.LoadedAssetFiles.Select(x => x.FullPath).Should().Equal(firstAssetFilePath, secondAssetFilePath);
    }

    [TestMethod]
    public void LoadStormFiles_ParentDirectoriesNamedBaseStormDataAndBaseStormAssets_AddsLayoutAndAssetFiles()
    {
        // arrange
        // the base.stormdata and base.stormassets names are only searched for inside the stormmod directory
        string modsDirectoryPath = MockUnixSupport.Path(@"C:\base.stormdata\base.stormassets\mods");
        string stormModDirectoryPath = Path.Combine(modsDirectoryPath, "heroesdata.stormmod");

        _heroesSource.ModsDirectoryPath.Returns(modsDirectoryPath);

        _fileSystem.AddFile(Path.Combine(stormModDirectoryPath, "base.stormdata", "UI", "Layout", "GameUI.stormlayout"), new MockFileData(string.Empty));
        _fileSystem.AddFile(Path.Combine(stormModDirectoryPath, "base.stormassets", "Assets", "Icon.dds"), new MockFileData(string.Empty));

        FileStormMod fileStormMod = new(_fileSystem, _heroesSource, stormModDirectoryPath, StormModType.Base);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        DataStorage dataStorage = GetDataStorage(StormModType.Base);
        dataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Keys.Should().Equal(Path.Combine("ui", "layout", "gameui.stormlayout"));
        dataStorage.FileCache.AssetFilesByRelativePath.Dictionary.Keys.Should().Equal(Path.Combine("assets", "icon.dds"));
    }

    [TestMethod]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadStormFiles_StormModType_LoadsIntoMatchingDataStorageOnly(StormModType stormModType)
    {
        // arrange
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "AbilData.xml"), "$AbilConst");
        _fileSystem.AddFile(Path.Combine(_gameDataDirectoryPath, "Assets.txt"), new MockFileData("Key=Value"));
        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "GameUI.stormlayout"), new MockFileData(string.Empty));

        FileStormMod fileStormMod = CreateFileStormMod(stormModType);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        DataStorage dataStorage = GetDataStorage(stormModType);
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary.Should().ContainKey("$AbilConst");
        dataStorage.TextCache.AssetTextsById.Dictionary.Should().ContainKey("Key");
        dataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().ContainKey(Path.Combine("ui", "layout", "gameui.stormlayout"));

        DataStorage baseDataStorage = GetDataStorage(StormModType.Base);
        baseDataStorage.ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
        baseDataStorage.TextCache.AssetTextsById.Dictionary.Should().BeEmpty();
        baseDataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_IncludesXmlFileHasPaths_LoadsIncludedStormModsAndAddsTheirContainers()
    {
        // arrange
        IStormMod coreStormMod = CreateIncludedStormMod();
        IStormMod heroesStormMod = CreateIncludedStormMod();

        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "core.stormmod"), StormModType.Base).Returns(coreStormMod);
        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "heroes.stormmod"), StormModType.Base).Returns(heroesStormMod);

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "Includes.xml"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Includes>
              <Path value="Mods/Core.StormMod" />
              <Path value="" />
              <Path />
              <Path value="Mods/Heroes.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Base);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        _stormModFactory.ReceivedWithAnyArgs(2).CreateFileStormMod(default!, default!, default);
        _stormModFactory.Received(1).CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "core.stormmod"), StormModType.Base);
        _stormModFactory.Received(1).CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "heroes.stormmod"), StormModType.Base);
        coreStormMod.Received(1).LoadStormFiles();
        heroesStormMod.Received(1).LoadStormFiles();
        _sourceContainer.StormModContainers.Should().Equal(coreStormMod.ModContainer, heroesStormMod.ModContainer);
    }

    [TestMethod]
    public void LoadStormFiles_MapStormModHasIncludesXmlFile_IncludedStormModsAreMapStormMods()
    {
        // arrange
        // so they load into the map data layer and are removed when the map is unloaded
        IStormMod includedStormMod = CreateIncludedStormMod();

        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"), StormModType.Map).Returns(includedStormMod);

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "Includes.xml"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Includes>
              <Path value="Mods/HeroesMapMods/BattlegroundMapMods/VolskayaSound.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Map);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        _stormModFactory.ReceivedWithAnyArgs(1).CreateFileStormMod(default!, default!, default);
        _stormModFactory.Received(1).CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"), StormModType.Map);
        includedStormMod.Received(1).LoadStormFiles();
        _sourceContainer.StormModContainers.Should().Equal(includedStormMod.ModContainer);
    }

    [TestMethod]
    public void LoadStormFiles_ModsDirectoryNotNamedMods_IncludedStormModsAreUnderIt()
    {
        // arrange
        string modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\hots_data");
        string stormModDirectoryPath = Path.Combine(modsDirectoryPath, "heroesdata.stormmod");

        _heroesSource.ModsDirectoryPath.Returns(modsDirectoryPath);

        IStormMod coreStormMod = CreateIncludedStormMod();
        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(modsDirectoryPath, "core.stormmod"), StormModType.Base).Returns(coreStormMod);

        _fileSystem.AddFile(Path.Combine(stormModDirectoryPath, "base.stormdata", "Includes.xml"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Includes>
              <Path value="Mods/Core.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = new(_fileSystem, _heroesSource, stormModDirectoryPath, StormModType.Base);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        _stormModFactory.Received(1).CreateFileStormMod(_heroesSource, Path.Combine(modsDirectoryPath, "core.stormmod"), StormModType.Base);
        coreStormMod.Received(1).LoadStormFiles();
    }

    [TestMethod]
    public void LoadStormFiles_ModsDirectoryNotNamedMods_StormFileRelativePathsAreRelativeToIt()
    {
        // arrange
        string modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\hots_data");
        string stormModDirectoryPath = Path.Combine(modsDirectoryPath, "heroesdata.stormmod");
        string xmlFilePath = Path.Combine(stormModDirectoryPath, "base.stormdata", "GameData", "test.xml");

        _heroesSource.ModsDirectoryPath.Returns(modsDirectoryPath);

        AddConstXmlFile(xmlFilePath, "$Const1");

        FileStormMod fileStormMod = new(_fileSystem, _heroesSource, stormModDirectoryPath, StormModType.Base);

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.RelativeStormModPath.Should().Be("heroesdata.stormmod");
        fileStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.RelativePath.Should().Be(Path.Combine("heroesdata.stormmod", "base.stormdata", "GameData", "test.xml"));
    }

    [TestMethod]
    public void LoadStormFiles_IncludedStormModCreatedByFactory_LoadsItsFilesFromSameFileSystem()
    {
        // arrange
        _heroesSource.StormModFactory.Returns(new StormModFactory(_fileSystem));

        string coreGameDataDirectoryPath = Path.Combine(_modsDirectoryPath, "core.stormmod", "base.stormdata", "GameData");
        AddConstXmlFile(Path.Combine(coreGameDataDirectoryPath, "CoreData.xml"), "$CoreConst");
        AddConstXmlFile(Path.Combine(_gameDataDirectoryPath, "HeroesData.xml"), "$HeroesDataConst");

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "Includes.xml"), new MockFileData(
            """
            <Includes>
              <Path value="Mods/Core.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$HeroesDataConst", "$CoreConst");
        _sourceContainer.StormModContainers.Should().ContainSingle()
            .Which.LoadedXmlFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                StormModName = "core",
                RelativePath = Path.Combine("core.stormmod", "base.stormdata", "GameData", "CoreData.xml"),
                FullPath = Path.Combine(coreGameDataDirectoryPath, "CoreData.xml"),
            });
    }

    [TestMethod]
    public void LoadStormFiles_StormModHasFiles_ModContainerListsLoadedFiles()
    {
        // arrange
        string abilDataFilePath = Path.Combine(_gameDataDirectoryPath, "AbilData.xml");
        string unitDataFilePath = Path.Combine(_gameDataDirectoryPath, "UnitData.xml");
        string layoutFilePath = Path.Combine(_baseStormDataPath, "UI", "Layout", "GameUI.stormlayout");
        string assetFilePath = Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets", "Icon.dds");

        AddConstXmlFile(unitDataFilePath, "$UnitConst");
        AddConstXmlFile(abilDataFilePath, "$AbilConst");
        _fileSystem.AddFile(Path.Combine(_gameDataDirectoryPath, "Assets.txt"), new MockFileData("Key=Value"));
        _fileSystem.AddFile(layoutFilePath, new MockFileData(string.Empty));
        _fileSystem.AddFile(assetFilePath, new MockFileData(string.Empty));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        StormModContainer modContainer = fileStormMod.ModContainer;
        modContainer.LoadedXmlFiles.Should().BeEquivalentTo([ExpectedStormFile(abilDataFilePath), ExpectedStormFile(unitDataFilePath)], options => options.WithStrictOrdering());
        modContainer.LoadedAssetsTextFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_baseStormDataPath, "gamedata", "assets.txt")));
        modContainer.LoadedStormLayoutFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(layoutFilePath));
        modContainer.LoadedAssetFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(assetFilePath));
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileCatalogFileDoesNotExist_DoesNotAddToLoadedXmlFiles()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "GameData.xml"), new MockFileData(
            """
            <Includes>
              <Catalog path="GameData/Heroes/MissingData.xml" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormFiles();

        // assert
        fileStormMod.ModContainer.LoadedXmlFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormGameStrings_GameStringFileExists_AddsGameStrings()
    {
        // arrange
        string filePath = Path.Combine(_stormModDirectoryPath, "enus.stormdata", "localizeddata", "gamestrings.txt");
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "enUS.StormData", "LocalizedData", "GameStrings.txt"), new MockFileData("Button/Name/Id1=Value1\nButton/Name/Empty="));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        var gameStrings = GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary;
        gameStrings.Should().HaveCount(2);
        gameStrings["Button/Name/Id1"].Value.Should().Be("Value1");
        gameStrings["Button/Name/Id1"].StormFile.Should().BeEquivalentTo(ExpectedStormFile(filePath));
        gameStrings["Button/Name/Empty"].Value.Should().BeEmpty();

        fileStormMod.ModContainer.LoadedGameStringFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(filePath));
    }

    [TestMethod]
    public void LoadStormGameStrings_OnlyOtherLocaleExists_LoadsNothing()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "dede.stormdata", "localizeddata", "gamestrings.txt"), new MockFileData("id1=Wert"));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
        fileStormMod.ModContainer.LoadedGameStringFiles.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadStormGameStrings_StormModType_LoadsIntoMatchingDataStorageOnly(StormModType stormModType)
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "enus.stormdata", "localizeddata", "gamestrings.txt"), new MockFileData("id1=value1"));

        FileStormMod fileStormMod = CreateFileStormMod(stormModType);

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>())
        {
            var gameStrings = GetDataStorage(otherStormModType).TextCache.GameStringsById.Dictionary;

            if (otherStormModType == stormModType)
                gameStrings.Keys.Should().Equal("id1");
            else
                gameStrings.Should().BeEmpty();
        }
    }

    [TestMethod]
    public void LoadStormGameStrings_HasIncludedStormMods_LoadsTheirGameStringsAfterOwn()
    {
        // arrange
        IStormMod coreStormMod = CreateIncludedStormMod();
        IStormMod heroesStormMod = CreateIncludedStormMod();

        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "core.stormmod"), StormModType.Base).Returns(coreStormMod);
        _stormModFactory.CreateFileStormMod(_heroesSource, Path.Combine(_modsDirectoryPath, "heroes.stormmod"), StormModType.Base).Returns(heroesStormMod);

        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "Includes.xml"), new MockFileData(
            """
            <Includes>
              <Path value="Mods/Core.StormMod" />
              <Path value="Mods/Heroes.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();
        fileStormMod.LoadStormFiles();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.KOKR);

        // assert
        coreStormMod.Received(1).LoadStormGameStrings(StormLocale.KOKR);
        heroesStormMod.Received(1).LoadStormGameStrings(StormLocale.KOKR);

        Received.InOrder(() =>
        {
            coreStormMod.LoadStormGameStrings(StormLocale.KOKR);
            heroesStormMod.LoadStormGameStrings(StormLocale.KOKR);
        });
    }

    [TestMethod]
    public void LoadStormGameStrings_StormFilesNotLoaded_DoesNotLoadIncludedStormMods()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_baseStormDataPath, "Includes.xml"), new MockFileData(
            """
            <Includes>
              <Path value="Mods/Core.StormMod" />
            </Includes>
            """));

        FileStormMod fileStormMod = CreateFileStormMod();

        // act
        fileStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
    }

    [TestMethod]
    public void GetMapDependencies_DocumentInfoHasDependencies_ReturnsDependencies()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "documentinfo"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <DocInfo>
                <Dependencies>
                    <Value>bnet:Volskaya Mechanics/0.0/1145,file:Mods\heroesmapmods/battlegroundmapmods/volskayamechanics.stormmod</Value>
                    <Value>bnet:Heroes Data/1.2/999,file:Mods/HeroesData.StormMod</Value>
                </Dependencies>
                <ModifiableDependencies>
                    <Value>Mods/HeroesData.StormMod</Value>
                </ModifiableDependencies>
            </DocInfo>
            """));

        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = fileStormMod.GetMapDependencies();

        // assert
        result.Should().Equal(
            new MapDependency()
            {
                BnetName = "Volskaya Mechanics",
                BnetVersionMajor = 0,
                BnetVersionMinor = 0,
                BnetNamespace = 1145,
                LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"),
            },
            new MapDependency()
            {
                BnetName = "Heroes Data",
                BnetVersionMajor = 1,
                BnetVersionMinor = 2,
                BnetNamespace = 999,
                LocalFile = "heroesdata.stormmod",
            });
    }

    [TestMethod]
    public void GetMapDependencies_DocumentInfoHasNoDependencies_ReturnsEmpty()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "documentinfo"), new MockFileData(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <DocInfo>
                <Flags>
                    <Value>PreloadNoTrigger</Value>
                </Flags>
            </DocInfo>
            """));

        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = fileStormMod.GetMapDependencies();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetMapDependencies_NoDocumentInfoFile_ReturnsEmpty()
    {
        // arrange
        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = fileStormMod.GetMapDependencies();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("not xml")]
    [DataRow("<DocInfo><Dependencies><Value>not a dependency</Value></Dependencies></DocInfo>")]
    public void GetMapDependencies_DocumentInfoCannotBeRead_ReturnsEmpty(string documentInfo)
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_stormModDirectoryPath, "documentinfo"), new MockFileData(documentInfo));

        FileStormMod fileStormMod = CreateFileStormMod(StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = fileStormMod.GetMapDependencies();

        // assert
        result.Should().BeEmpty();
    }

    private static string CreateConstXml(string constId) =>
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <Catalog>
          <const id="{constId}" value="1" />
        </Catalog>
        """;

    /// <summary>
    /// Gets the expected <see cref="StormFile"/> values for a file in the <c>heroesdata.stormmod</c>.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns>An object with the expected <see cref="StormFile"/> values.</returns>
    private static object ExpectedStormFile(string fullPath) => new
    {
        StormModName = "heroesdata",
        RelativePath = fullPath[(_modsDirectoryPath.Length + 1)..],
        FullPath = fullPath,
    };

    private FileStormMod CreateFileStormMod(StormModType stormModType = StormModType.Base) =>
        new(_fileSystem, _heroesSource, _stormModDirectoryPath, stormModType);

    private DataStorage GetDataStorage(StormModType stormModType) => _sourceContainer.LayeredDataStorage.GetDataStorage(stormModType);

    private void AddConstXmlFile(string filePath, string constId) => _fileSystem.AddFile(filePath, new MockFileData(CreateConstXml(constId)));

    private IStormMod CreateIncludedStormMod()
    {
        IStormMod stormMod = Substitute.For<IStormMod>();
        stormMod.ModContainer.Returns(new StormModContainer(_sourceContainer, StormModType.Base));

        return stormMod;
    }
}
