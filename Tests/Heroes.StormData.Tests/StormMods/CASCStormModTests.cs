using System.Text;

namespace Heroes.StormData.StormMods.Tests;

[TestClass]
public sealed class CASCStormModTests
{
    private const string ModsDirectory = "mods";

    private static readonly string _stormModDirectoryPath = Path.Combine(ModsDirectory, "heroesdata.stormmod");
    private static readonly string _baseStormDataPath = Path.Combine(_stormModDirectoryPath, "base.stormdata");

    // the paths the stormmod builds use the lowercase constants
    private static readonly string _gameDataDirectoryPath = Path.Combine(_baseStormDataPath, "gamedata");

    private readonly SourceContainer _sourceContainer = new();
    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly ICASCHeroesSource _heroesSource = Substitute.For<ICASCHeroesSource>();
    private readonly ICASCHeroesStorage _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
    private readonly CASCFolder _cascFolderRoot = new("root");
    private readonly FakeCASCHandlerWrapper _cascHandlerWrapper = new();

    public CASCStormModTests()
    {
        _cascHeroesStorage.CASCFolderRoot.Returns(_cascFolderRoot);
        _cascHeroesStorage.CASCHandlerWrapper.Returns(_cascHandlerWrapper);

        _heroesSource.ModsDirectoryPath.Returns(ModsDirectory);
        _heroesSource.SourceContainer.Returns(_sourceContainer);
        _heroesSource.StormModFactory.Returns(_stormModFactory);
        _heroesSource.CASCHeroesStorage.Returns(_cascHeroesStorage);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void Constructor_StormModDirectoryPath_SetsProperties(StormModType stormModType)
    {
        // act
        CASCStormMod cascStormMod = CreateCASCStormMod(stormModType);

        // assert
        cascStormMod.Name.Should().Be("heroesdata");
        cascStormMod.DirectoryPath.Should().Be(_stormModDirectoryPath);
        cascStormMod.RelativeStormModPath.Should().Be("heroesdata.stormmod");
        cascStormMod.ModType.Should().Be(stormModType);
        cascStormMod.ModContainer.StormModType.Should().Be(stormModType);
    }

    [TestMethod]
    public void LoadStormFiles_StormModNotInCASC_LoadsNothingAndDoesNotThrow()
    {
        // arrange
        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        Action act = cascStormMod.LoadStormFiles;

        // assert
        act.Should().NotThrow();

        DataStorage dataStorage = GetDataStorage(StormModType.Base);
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
        dataStorage.TextCache.AssetTextsById.Dictionary.Should().BeEmpty();
        dataStorage.StyleCache.StormXmlStyleElementByName.Dictionary.Should().BeEmpty();
        dataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().BeEmpty();
        dataStorage.FileCache.AssetFilesByRelativePath.Dictionary.Should().BeEmpty();
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
        _sourceContainer.StormModContainers.Should().BeEmpty();
        _cascHandlerWrapper.OpenedFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_LoadsXmlFilesInDirectory()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.xml"), "$AbilConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "UNITDATA.XML"), "$UnitConst");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$AbilConst", "$UnitConst");
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_StormFileHasModNameAndFilePath()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.xml"), "$AbilConst");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        // the directory is from the lowercase constants and the file name is from the CASC folder
        StormXmlConstElement constElement = GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary["$AbilConst"];
        constElement.StormFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_gameDataDirectoryPath, "AbilData.xml")));
    }

    [TestMethod]
    public void LoadStormFiles_GameDataDirectoryHasXmlFiles_LoadsInPathOrder()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "UnitData.xml"), "$UnitConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "abilData.xml"), "$AbilConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "BehaviorData.xml"), "$BehaviorConst");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        cascStormMod.ModContainer.LoadedXmlFiles.Select(x => Path.GetFileName(x.FullPath)).Should().Equal("abilData.xml", "BehaviorData.xml", "UnitData.xml");
    }

    [TestMethod]
    public void LoadStormFiles_GameDataSubdirectoryAndNonXmlFiles_DoesNotLoadThem()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "Heroes", "HeroData.xml"), "$SubdirectoryConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.txt"), "$TextFileConst");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileListsFileInGameDataDirectory_LoadsItOnce()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.xml"), "$AbilConst");

        AddCASCFile(
            Path.Combine(_baseStormDataPath, "GameData.xml"),
            """
            <Includes>
              <Catalog path="GameData/AbilData.xml" />
            </Includes>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        cascStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.FullPath.Should().Be(Path.Combine(_gameDataDirectoryPath, "AbilData.xml"));
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary["$AbilConst"].StormFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void LoadStormFiles_GameDataXmlFileHasCatalogs_LoadsCatalogFilesFromGameDataDirectory()
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "Heroes", "HeroData.xml"), "$HeroConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "Heroes", "Abathur", "AbathurData.xml"), "$AbathurConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "Other", "OtherData.xml"), "$OtherConst");

        AddCASCFile(
            Path.Combine(_baseStormDataPath, "GameData.xml"),
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Includes>
              <Catalog path="GameData/Heroes/HeroData.xml" />
              <Catalog path="GameData\Heroes\Abathur\AbathurData.xml" />
              <Catalog path="Other/OtherData.xml" />
              <Catalog path="GameData/Heroes/MissingData.xml" />
            </Includes>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$HeroConst", "$AbathurConst");
        cascStormMod.ModContainer.LoadedXmlFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadStormFiles_AssetsTextFileExists_AddsAssetTexts()
    {
        // arrange
        AddCASCFile(Path.Combine(_baseStormDataPath, "GameData", "Assets.txt"), "UI/HeroPortrait=Assets\\Textures\\portrait.dds\nUI/Empty=");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        var assetTexts = GetDataStorage(StormModType.Base).TextCache.AssetTextsById.Dictionary;
        assetTexts.Should().HaveCount(2);
        assetTexts["UI/HeroPortrait"].Value.Should().Be("Assets\\Textures\\portrait.dds");
        assetTexts["UI/HeroPortrait"].StormFile.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_gameDataDirectoryPath, "assets.txt")));
        assetTexts["UI/Empty"].Value.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_FontStylesFileExists_AddsStyleConstantAndStyleElements()
    {
        // arrange
        AddCASCFile(
            Path.Combine(_baseStormDataPath, "UI", "FontStyles.StormStyle"),
            """
            <?xml version="1.0" encoding="utf-8" standalone="yes"?>
            <StyleFile>
              <Constant name="ColorGray" val="a7a7a7" />
              <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="#ColorGray" />
            </StyleFile>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        StyleCache styleCache = GetDataStorage(StormModType.Base).StyleCache;

        StormXmlStyleConstantElement constantElement = styleCache.StormXmlStyleConstantElementByName.Dictionary.Should().ContainSingle().Which.Value;
        constantElement.Name.Should().Be("ColorGray");
        constantElement.Value.Should().Be("a7a7a7");
        constantElement.StormFiles.Should().ContainSingle().Which.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_baseStormDataPath, "ui", "fontstyles.stormstyle")));

        styleCache.StormXmlStyleElementByName.Dictionary.Should().ContainSingle().Which.Value.Name.Should().Be("ReticleEnemy");
    }

    [TestMethod]
    public void LoadStormFiles_LayoutDirectoryHasStormLayoutFiles_AddsRelativeUiPaths()
    {
        // arrange
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "GameUI.StormLayout"), string.Empty);
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "Heroes", "Abathur", "HeroPanel.stormlayout"), string.Empty);
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "readme.txt"), string.Empty);
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Other.stormlayout"), string.Empty);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        // the layout directory is from the lowercase constants, the rest is from the CASC folders
        string layoutDirectoryPath = Path.Combine(_baseStormDataPath, "ui", "layout");

        var layoutFiles = GetDataStorage(StormModType.Base).FileCache.StormLayoutFilesByRelativePath.Dictionary;
        layoutFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("ui", "layout", "gameui.stormlayout"),
            Path.Combine("ui", "layout", "heroes", "abathur", "heropanel.stormlayout"));
        layoutFiles[Path.Combine("ui", "layout", "gameui.stormlayout")].Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(layoutDirectoryPath, "GameUI.StormLayout")));
        layoutFiles[Path.Combine("ui", "layout", "heroes", "abathur", "heropanel.stormlayout")].Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(layoutDirectoryPath, "Heroes", "Abathur", "HeroPanel.stormlayout")));
    }

    [TestMethod]
    public void LoadStormFiles_AssetsDirectoryHasDdsFiles_AddsRelativeAssetsPaths()
    {
        // arrange
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets", "Icon.DDS"), string.Empty);
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets", "Textures", "Portrait.dds"), string.Empty);
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets", "Textures", "Portrait.png"), string.Empty);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        string assetsDirectoryPath = Path.Combine(_stormModDirectoryPath, "base.stormassets", "assets");

        var assetFiles = GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary;
        assetFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("assets", "icon.dds"),
            Path.Combine("assets", "textures", "portrait.dds"));
        assetFiles[Path.Combine("assets", "icon.dds")].Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(assetsDirectoryPath, "Icon.DDS")));
        assetFiles[Path.Combine("assets", "textures", "portrait.dds")].Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(assetsDirectoryPath, "Textures", "Portrait.dds")));
    }

    [TestMethod]
    public void LoadStormFiles_LayoutAndAssetsDirectories_DoesNotOpenTheFiles()
    {
        // arrange
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "GameUI.stormlayout"), string.Empty);
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "base.stormassets", "Assets", "Icon.dds"), string.Empty);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        cascStormMod.ModContainer.LoadedStormLayoutFiles.Should().ContainSingle();
        cascStormMod.ModContainer.LoadedAssetFiles.Should().ContainSingle();
        _cascHandlerWrapper.OpenedFiles.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadStormFiles_StormModType_LoadsIntoMatchingDataStorageOnly(StormModType stormModType)
    {
        // arrange
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.xml"), "$AbilConst");
        AddCASCFile(Path.Combine(_baseStormDataPath, "GameData", "Assets.txt"), "Key=Value");
        AddCASCFile(Path.Combine(_baseStormDataPath, "UI", "Layout", "GameUI.stormlayout"), string.Empty);

        CASCStormMod cascStormMod = CreateCASCStormMod(stormModType);

        // act
        cascStormMod.LoadStormFiles();

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
    public void LoadStormFiles_IncludesXmlFileHasPaths_LoadsIncludedCASCStormModsAndAddsTheirContainers()
    {
        // arrange
        IStormMod coreStormMod = CreateIncludedStormMod();
        IStormMod heroesStormMod = CreateIncludedStormMod();

        _stormModFactory.CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "core.stormmod"), StormModType.Base).Returns(coreStormMod);
        _stormModFactory.CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "heroes.stormmod"), StormModType.Base).Returns(heroesStormMod);

        AddCASCFile(
            Path.Combine(_baseStormDataPath, "Includes.xml"),
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Includes>
              <Path value="Mods/Core.StormMod" />
              <Path value="" />
              <Path />
              <Path value="Mods/Heroes.StormMod" />
            </Includes>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod(StormModType.Map);

        // act
        cascStormMod.LoadStormFiles();

        // assert
        _stormModFactory.ReceivedWithAnyArgs(2).CreateCASCStormMod(default!, default!, default);
        _stormModFactory.Received(1).CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "core.stormmod"), StormModType.Base);
        _stormModFactory.Received(1).CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "heroes.stormmod"), StormModType.Base);
        _stormModFactory.DidNotReceiveWithAnyArgs().CreateFileStormMod(default!, default!, default);
        coreStormMod.Received(1).LoadStormFiles();
        heroesStormMod.Received(1).LoadStormFiles();
        _sourceContainer.StormModContainers.Should().Equal(coreStormMod.ModContainer, heroesStormMod.ModContainer);
    }

    [TestMethod]
    public void LoadStormFiles_IncludedStormModCreatedByFactory_LoadsItsFilesFromCASC()
    {
        // arrange
        _heroesSource.StormModFactory.Returns(new StormModFactory());

        string coreGameDataDirectoryPath = Path.Combine(ModsDirectory, "core.stormmod", "base.stormdata", "gamedata");
        AddConstXmlFile(Path.Combine(ModsDirectory, "core.stormmod", "base.stormdata", "GameData", "CoreData.xml"), "$CoreConst");
        AddConstXmlFile(Path.Combine(_baseStormDataPath, "GameData", "HeroesData.xml"), "$HeroesDataConst");

        AddCASCFile(
            Path.Combine(_baseStormDataPath, "Includes.xml"),
            """
            <Includes>
              <Path value="Mods/Core.StormMod" />
            </Includes>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$HeroesDataConst", "$CoreConst");
        _sourceContainer.StormModContainers.Should().ContainSingle()
            .Which.LoadedXmlFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                StormModName = "core",
                RelativePath = Path.Combine("core.stormmod", "base.stormdata", "gamedata", "CoreData.xml"),
                FullPath = Path.Combine(coreGameDataDirectoryPath, "CoreData.xml"),
            });
    }

    [TestMethod]
    public void LoadStormFiles_FileExistsButCannotBeOpened_SkipsFile()
    {
        // arrange
        ICASCHandlerWrapper cascHandlerWrapper = Substitute.For<ICASCHandlerWrapper>();
        cascHandlerWrapper.FileExists(Arg.Any<string>()).Returns(true);
        cascHandlerWrapper.OpenFile(Arg.Any<string>()).Returns((Stream?)null);

        _cascHeroesStorage.CASCHandlerWrapper.Returns(cascHandlerWrapper);

        _cascFolderRoot.AddFile(Path.Combine(_baseStormDataPath, "GameData", "AbilData.xml"));

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormFiles();

        // assert
        cascStormMod.ModContainer.LoadedXmlFiles.Should().BeEmpty();
        cascStormMod.ModContainer.LoadedAssetsTextFiles.Should().BeEmpty();
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormGameStrings_GameStringFileExists_AddsGameStrings()
    {
        // arrange
        string filePath = Path.Combine(_stormModDirectoryPath, "enus.stormdata", "localizeddata", "gamestrings.txt");
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "enUS.StormData", "LocalizedData", "GameStrings.txt"), "Button/Name/Id1=Value1\nButton/Name/Empty=");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        cascStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        var gameStrings = GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary;
        gameStrings.Should().HaveCount(2);
        gameStrings["Button/Name/Id1"].Value.Should().Be("Value1");
        gameStrings["Button/Name/Id1"].StormFile.Should().BeEquivalentTo(ExpectedStormFile(filePath));
        gameStrings["Button/Name/Empty"].Value.Should().BeEmpty();

        cascStormMod.ModContainer.LoadedGameStringFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(filePath));
    }

    [TestMethod]
    public void LoadStormGameStrings_OnlyOtherLocaleExists_LoadsNothingAndDoesNotThrow()
    {
        // arrange
        AddCASCFile(Path.Combine(_stormModDirectoryPath, "dede.stormdata", "localizeddata", "gamestrings.txt"), "id1=Wert");

        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        Action act = () => cascStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        act.Should().NotThrow();
        GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
        cascStormMod.ModContainer.LoadedGameStringFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormGameStrings_HasIncludedStormMods_LoadsTheirGameStringsAfterOwn()
    {
        // arrange
        IStormMod coreStormMod = CreateIncludedStormMod();
        IStormMod heroesStormMod = CreateIncludedStormMod();

        _stormModFactory.CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "core.stormmod"), StormModType.Base).Returns(coreStormMod);
        _stormModFactory.CreateCASCStormMod(_heroesSource, Path.Combine(ModsDirectory, "heroes.stormmod"), StormModType.Base).Returns(heroesStormMod);

        AddCASCFile(
            Path.Combine(_baseStormDataPath, "Includes.xml"),
            """
            <Includes>
              <Path value="Mods/Core.StormMod" />
              <Path value="Mods/Heroes.StormMod" />
            </Includes>
            """);

        CASCStormMod cascStormMod = CreateCASCStormMod();
        cascStormMod.LoadStormFiles();

        // act
        cascStormMod.LoadStormGameStrings(StormLocale.KOKR);

        // assert
        Received.InOrder(() =>
        {
            coreStormMod.LoadStormGameStrings(StormLocale.KOKR);
            heroesStormMod.LoadStormGameStrings(StormLocale.KOKR);
        });
    }

    [TestMethod]
    public void GetMapDependencies_NoDocumentInfoFile_ReturnsEmptyAndDoesNotThrow()
    {
        // arrange
        CASCStormMod cascStormMod = CreateCASCStormMod();

        // act
        IReadOnlyList<MapDependency> result = cascStormMod.GetMapDependencies();

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
        RelativePath = fullPath[(ModsDirectory.Length + 1)..],
        FullPath = fullPath,
    };

    private CASCStormMod CreateCASCStormMod(StormModType stormModType = StormModType.Base) =>
        new(_heroesSource, _stormModDirectoryPath, stormModType);

    private DataStorage GetDataStorage(StormModType stormModType) => _sourceContainer.LayeredDataStorage.GetDataStorage(stormModType);

    private void AddConstXmlFile(string filePath, string constId) => AddCASCFile(filePath, CreateConstXml(constId));

    /// <summary>
    /// Adds a file to the CASC folder tree and to the CASC storage.
    /// </summary>
    /// <param name="filePath">The path of the file, with the casing it has in CASC.</param>
    /// <param name="content">The content of the file.</param>
    private void AddCASCFile(string filePath, string content)
    {
        _cascFolderRoot.AddFile(filePath);
        _cascHandlerWrapper.AddFile(filePath, content);
    }

    private IStormMod CreateIncludedStormMod()
    {
        IStormMod stormMod = Substitute.For<IStormMod>();
        stormMod.ModContainer.Returns(new StormModContainer(_sourceContainer, StormModType.Base));

        return stormMod;
    }

    /// <summary>
    /// Opens files like the CASC storage: the lookup is case-insensitive, '/' and '\' are the same, and a missing file throws a
    /// <see cref="FileNotFoundException"/> (as <see cref="CASCConfig.ThrowOnFileNotFound"/> is set by the loader).
    /// </summary>
    private sealed class FakeCASCHandlerWrapper : ICASCHandlerWrapper
    {
        private readonly Dictionary<string, byte[]> _filesByPath = new(StringComparer.OrdinalIgnoreCase);

        public List<string> OpenedFiles { get; } = [];

        public void AddFile(string path, string content) => _filesByPath[Normalize(path)] = Encoding.UTF8.GetBytes(content);

        public bool FileExists(string file) => _filesByPath.ContainsKey(Normalize(file));

        public Stream? OpenFile(string name)
        {
            if (!_filesByPath.TryGetValue(Normalize(name), out byte[]? data))
                throw new FileNotFoundException("File not found in the CASC storage.", name);

            OpenedFiles.Add(name);

            return new MemoryStream(data);
        }

        private static string Normalize(string path) => path.Replace('/', '\\');
    }
}
