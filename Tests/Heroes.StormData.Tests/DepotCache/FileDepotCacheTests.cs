namespace Heroes.StormData.DepotCache.Tests;

[TestClass]
internal sealed class FileDepotCacheTests
{
    private const string VolskayaS2MVFile = "94a00.s2mv";
    private const string CursedHollowS2MVFile = "81f5f8.s2mv";
    private const string VolskayaS2MAFile = "8d554.s2ma";
    private const string CursedHollowS2MAFile = "b90c6.s2ma";
    private const string ModArchiveS2MAFile = "c05eba.s2ma";

    private static readonly string _testFilesDirectoryPath = Path.Combine("TestFiles", "DepotCache");
    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
    private static readonly string _depotCacheDirectoryPath = Path.Combine(_modsDirectoryPath, "core.stormmod", "base.stormdata", "depotcache");

    private readonly MockFileSystem _fileSystem = new();
    private readonly IFileHeroesSource _heroesSource = Substitute.For<IFileHeroesSource>();

    public FileDepotCacheTests()
    {
        _heroesSource.ModsDirectoryPath.Returns(_modsDirectoryPath);
    }

    [TestMethod]
    public void LoadDepotCache_DepotCacheDirectoryDoesNotExist_DataIsEmpty()
    {
        // arrange
        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        AssertDataIsEmpty(fileDepotCache.Data);
    }

    [TestMethod]
    public void LoadDepotCache_NoS2MVOrS2MAFiles_DataIsEmpty()
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_depotCacheDirectoryPath, "readme.txt"), new MockFileData("temp"));
        _fileSystem.AddFile(Path.Combine(_depotCacheDirectoryPath, "00", "01", "0001.s2ml"), new MockFileData("<Locale/>"));

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        AssertDataIsEmpty(fileDepotCache.Data);
    }

    [TestMethod]
    public void LoadDepotCache_S2MVFiles_LoadsS2MVProperties()
    {
        // arrange
        string volskayaFilePath = AddTestFile(VolskayaS2MVFile);
        string cursedHollowFilePath = AddTestFile(CursedHollowS2MVFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        data.S2MVFilePaths.Should().BeEquivalentTo([volskayaFilePath, cursedHollowFilePath]);
        data.S2MVPropertiesByDependencyKey.Should().HaveCount(2);

        S2MVProperties volskaya = GetS2MVProperties(data, "Volskaya Foundry");
        volskaya.MapLink.Should().Be("Volskaya");
        volskaya.DirectoryPath.Should().Be(GetExpectedDirectoryPath(VolskayaS2MVFile));
        volskaya.DocInfoIconFile.Should().Be(Path.Combine("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
        volskaya.NameByStormLocale.Should().BeEquivalentTo(new Dictionary<StormLocale, string>()
        {
            [StormLocale.PTBR] = "Fundição Volskaya",
            [StormLocale.FRFR] = "La fonderie Volskaya",
            [StormLocale.KOKR] = "볼스카야 공장",
            [StormLocale.ENUS] = "Volskaya Foundry",
        });
        volskaya.LoadingImage.Should().Be(Path.Combine("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
        volskaya.MapSize.Should().Be(new MapSize(248, 208));
        volskaya.PreviewLargeImage.Should().Be(Path.Combine("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
        volskaya.CustomLayout.Should().Be(Path.Combine("ui", "layout", "loadingscreens", "volskaya_loading.stormlayout"));
        volskaya.CustomFrame.Should().Be("Volskaya_Loading/ScreenMapLoading_Volskaya");
        volskaya.MapDependencies.Should().ContainSingle().Which.Should().Be(new MapDependency()
        {
            BnetName = "Volskaya Sound",
            BnetVersionMajor = 0,
            BnetVersionMinor = 0,
            BnetNamespace = 1146,
            LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"),
        });
        volskaya.ModifiableDependencies.Should().Equal(
            "heroesdata.stormmod",
            Path.Combine("heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"),
            Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"));

        S2MVProperties cursedHollow = GetS2MVProperties(data, "Cursed Hollow");
        cursedHollow.MapLink.Should().Be("CursedHollow");
        cursedHollow.DirectoryPath.Should().Be(GetExpectedDirectoryPath(CursedHollowS2MVFile));
        cursedHollow.DocInfoIconFile.Should().Be("storm_ui_gamemode_mapselect_cursedhollow.png");
        cursedHollow.NameByStormLocale.Should().BeEquivalentTo(new Dictionary<StormLocale, string>()
        {
            [StormLocale.PTBR] = "Clareira Maldita",
            [StormLocale.FRFR] = "Le val maudit",
            [StormLocale.KOKR] = "저주받은 골짜기",
            [StormLocale.ENUS] = "Cursed Hollow",
        });
        cursedHollow.LoadingImage.Should().Be(Path.Combine("assets", "textures", "ui_ingame_mapmechanic_loadscreen_cursedhollow.dds"));
        cursedHollow.MapSize.Should().Be(new MapSize(256, 216));
        cursedHollow.PreviewLargeImage.Should().Be("replayspreviewimage.tga");
        cursedHollow.CustomLayout.Should().Be(Path.Combine("ui", "layout", "loadingscreens", "cursedhollow_loading.stormlayout"));
        cursedHollow.CustomFrame.Should().Be("CursedHollow_Loading/ScreenMapLoading_CursedHollow");
        cursedHollow.MapDependencies.Should().ContainSingle().Which.Should().Be(new MapDependency()
        {
            BnetName = "Cursed Hollow (Mod)",
            BnetVersionMajor = 0,
            BnetVersionMinor = 0,
            BnetNamespace = 1497,
            LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "cursedhollow.stormmod"),
        });
        cursedHollow.ModifiableDependencies.Should().Equal("heroesdata.stormmod");

        data.S2MAFilePaths.Should().BeEmpty();
        data.S2MAProperties.Should().BeEmpty();
        data.S2MAPropertiesByTitle.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadDepotCache_S2MAFiles_LoadsS2MAProperties()
    {
        // arrange
        string volskayaFilePath = AddTestFile(VolskayaS2MAFile);
        string cursedHollowFilePath = AddTestFile(CursedHollowS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        data.S2MAFilePaths.Should().BeEquivalentTo([volskayaFilePath, cursedHollowFilePath]);
        data.S2MAProperties.Should().HaveCount(2);

        S2MAProperties volskaya = data.S2MAProperties.Should().ContainSingle(x => x.MapId == "Volskaya").Subject;
        volskaya.DirectoryPath.Should().Be(GetExpectedDirectoryPath(VolskayaS2MAFile));
        volskaya.DocInfoIconFile.Should().Be(Path.Combine("assets", "textures", "storm_ui_homescreenbackground_volskaya.dds"));
        volskaya.MapDependencies.Should().ContainSingle().Which.Should().Be(new MapDependency()
        {
            BnetName = "Volskaya Sound",
            BnetVersionMajor = 0,
            BnetVersionMinor = 0,
            BnetNamespace = 1146,
            LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"),
        });
        volskaya.ModifiableDependencies.Should().Equal(
            "heroesdata.stormmod",
            Path.Combine("heroesmapmods", "battlegroundmapmods", "overwatchdata.stormmod"),
            Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayamechanics.stormmod"));

        S2MAProperties cursedHollow = data.S2MAProperties.Should().ContainSingle(x => x.MapId == "CursedHollow").Subject;
        cursedHollow.DirectoryPath.Should().Be(GetExpectedDirectoryPath(CursedHollowS2MAFile));
        cursedHollow.DocInfoIconFile.Should().Be("storm_ui_gamemode_mapselect_cursedhollow.png");
        cursedHollow.MapDependencies.Should().ContainSingle().Which.Should().Be(new MapDependency()
        {
            BnetName = "Cursed Hollow (Mod)",
            BnetVersionMajor = 0,
            BnetVersionMinor = 0,
            BnetNamespace = 1497,
            LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "cursedhollow.stormmod"),
        });
        cursedHollow.ModifiableDependencies.Should().Equal("heroesdata.stormmod");
    }

    [TestMethod]
    public void LoadDepotCache_ModsDirectoryNotNamedMods_DirectoryPathsAreRelativeToIt()
    {
        // arrange
        string modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\hots_data");
        string depotCacheDirectoryPath = Path.Combine(modsDirectoryPath, "core.stormmod", "base.stormdata", "depotcache");

        _heroesSource.ModsDirectoryPath.Returns(modsDirectoryPath);

        foreach (string fileName in new[] { VolskayaS2MVFile, VolskayaS2MAFile })
        {
            _fileSystem.AddFile(
                Path.Combine([depotCacheDirectoryPath, .. GetSubDirectories(fileName), fileName]),
                new MockFileData(File.ReadAllBytes(Path.Combine(_testFilesDirectoryPath, fileName))));
        }

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        S2MAProperties volskaya = data.S2MAPropertiesByTitle.Should().ContainKey("Volskaya Foundry").WhoseValue;
        volskaya.DirectoryPath.Should().Be(GetExpectedDirectoryPath(VolskayaS2MAFile));
        volskaya.S2MVProperties!.DirectoryPath.Should().Be(GetExpectedDirectoryPath(VolskayaS2MVFile));
        volskaya.MapDependencies.Should().ContainSingle().Which.LocalFile.Should().Be(Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
    }

    [TestMethod]
    public void LoadDepotCache_S2MAWithMatchingS2MV_LinksS2MVAndAddsByTitle()
    {
        // arrange
        AddTestFile(VolskayaS2MVFile);
        AddTestFile(CursedHollowS2MVFile);
        AddTestFile(VolskayaS2MAFile);
        AddTestFile(CursedHollowS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        data.S2MAPropertiesByTitle.Keys.Should().BeEquivalentTo(["Volskaya Foundry", "Cursed Hollow"]);

        data.S2MAPropertiesByTitle["Volskaya Foundry"].MapId.Should().Be("Volskaya");
        data.S2MAPropertiesByTitle["Volskaya Foundry"].S2MVProperties.Should().BeSameAs(GetS2MVProperties(data, "Volskaya Foundry"));

        data.S2MAPropertiesByTitle["Cursed Hollow"].MapId.Should().Be("CursedHollow");
        data.S2MAPropertiesByTitle["Cursed Hollow"].S2MVProperties.Should().BeSameAs(GetS2MVProperties(data, "Cursed Hollow"));
    }

    [TestMethod]
    public void LoadDepotCache_S2MAWithMatchingS2MV_TitleLookupIsCaseInsensitive()
    {
        // arrange
        AddTestFile(VolskayaS2MVFile);
        AddTestFile(VolskayaS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        fileDepotCache.Data.S2MAPropertiesByTitle.Should().ContainKey("VOLSKAYA FOUNDRY");
    }

    [TestMethod]
    public void LoadDepotCache_S2MAWithoutMatchingS2MV_NotLinkedOrAddedByTitle()
    {
        // arrange
        AddTestFile(VolskayaS2MVFile);
        AddTestFile(CursedHollowS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        data.S2MAProperties.Should().ContainSingle().Which.S2MVProperties.Should().BeNull();
        data.S2MAPropertiesByTitle.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("", DisplayName = "Empty file")]
    [DataRow("DDS |\0\0\0", DisplayName = "DDS image")]
    [DataRow("  {}", DisplayName = "Does not start with {")]
    [DataRow("""{ "AddMultiMod": {}, "UsedStrings": {} }""", DisplayName = "JSON without MapInfo")]
    [DataRow("""{ "UsedStrings": { "Name": "MapInfo" } }""", DisplayName = "JSON with MapInfo only as a value")]
    [DataRow("""{ "MapInfo": { "Properties": """, DisplayName = "Invalid JSON")]
    public void LoadDepotCache_S2MVFileIsNotAMap_FileIsSkipped(string fileContent)
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_depotCacheDirectoryPath, "ab", "cd", "abcd.s2mv"), new MockFileData(fileContent));

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        AssertDataIsEmpty(fileDepotCache.Data);
    }

    [TestMethod]
    [DataRow("", DisplayName = "Empty file")]
    [DataRow("MP", DisplayName = "Shorter than the MPQ header")]
    [DataRow("Standard Data: Teams03.StormMod", DisplayName = "Text file")]
    public void LoadDepotCache_S2MAFileIsNotAnMpqArchive_FileIsSkipped(string fileContent)
    {
        // arrange
        _fileSystem.AddFile(Path.Combine(_depotCacheDirectoryPath, "ab", "cd", "abcd.s2ma"), new MockFileData(fileContent));

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        AssertDataIsEmpty(fileDepotCache.Data);
    }

    [TestMethod]
    public void LoadDepotCache_S2MAFileIsAModArchive_FileIsSkipped()
    {
        // arrange
        AddTestFile(ModArchiveS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();

        // assert
        AssertDataIsEmpty(fileDepotCache.Data);
    }

    [TestMethod]
    public void LoadDepotCache_CalledTwice_DataIsNotDuplicated()
    {
        // arrange
        AddTestFile(VolskayaS2MVFile);
        AddTestFile(VolskayaS2MAFile);

        FileDepotCache fileDepotCache = new(_fileSystem, _heroesSource);

        // act
        fileDepotCache.LoadDepotCache();
        fileDepotCache.LoadDepotCache();

        // assert
        DepotCacheData data = fileDepotCache.Data;

        data.S2MVFilePaths.Should().ContainSingle();
        data.S2MVPropertiesByDependencyKey.Should().ContainSingle();
        data.S2MAFilePaths.Should().ContainSingle();
        data.S2MAProperties.Should().ContainSingle();
        data.S2MAPropertiesByTitle.Should().ContainSingle();
    }

    private static void AssertDataIsEmpty(DepotCacheData data)
    {
        data.S2MVFilePaths.Should().BeEmpty();
        data.S2MVPropertiesByDependencyKey.Should().BeEmpty();
        data.S2MAFilePaths.Should().BeEmpty();
        data.S2MAProperties.Should().BeEmpty();
        data.S2MAPropertiesByTitle.Should().BeEmpty();
    }

    private static S2MVProperties GetS2MVProperties(DepotCacheData data, string headerTitle) =>
        data.S2MVPropertiesByDependencyKey.Values.Should().ContainSingle(x => x.HeaderTitle == headerTitle).Subject;

    // the depotcache stores files two directories deep, e.g. depotcache/94/a0/94a00.s2mv
    private static string[] GetSubDirectories(string fileName) => [fileName[..2], fileName[2..4]];

    private static string GetExpectedDirectoryPath(string fileName) =>
        Path.Combine(["core.stormmod", "base.stormdata", "depotcache", .. GetSubDirectories(fileName), fileName]);

    private string AddTestFile(string fileName)
    {
        string filePath = Path.Combine([_depotCacheDirectoryPath, .. GetSubDirectories(fileName), fileName]);

        _fileSystem.AddFile(filePath, new MockFileData(File.ReadAllBytes(Path.Combine(_testFilesDirectoryPath, fileName))));

        return filePath;
    }
}
