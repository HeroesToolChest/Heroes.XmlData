namespace Heroes.StormData.Cache.Files.Tests;

[TestClass]
internal sealed class LayeredFileCacheTests
{
    private readonly FileCache _baseFileCache = new();
    private readonly FileCache _mapFileCache = new();
    private readonly FileCache _customFileCache = new();
    private readonly LayeredFileCache _layeredFileCache;

    public LayeredFileCacheTests()
    {
        _layeredFileCache = new LayeredFileCache(_baseFileCache, _mapFileCache, _customFileCache);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstAssetFileByRelativePath_FileInSingleLayer_ReturnsFile(StormModType stormModType)
    {
        // arrange
        StormFile expected = AddAssetFile(stormModType, Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"));

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"), out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_NoLayersPopulated_ReturnsFalse()
    {
        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"), out StormFile? stormFile);

        // assert
        result.Should().BeFalse();
        stormFile.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_PathNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        AddAssetFile(StormModType.Base, Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"));

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(Path.Join("assets", "textures", "storm_ui_icon_alarak.dds"), out StormFile? stormFile);

        // assert
        result.Should().BeFalse();
        stormFile.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_EmptyPath_ReturnsFalse()
    {
        // arrange
        AddAssetFile(StormModType.Base, Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"));

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(string.Empty, out StormFile? stormFile);

        // assert
        result.Should().BeFalse();
        stormFile.Should().BeNull();
    }

    [TestMethod]
    [DataRow(@"Assets\Textures\Storm_UI_Icon_Abathur.dds")]
    [DataRow("Assets/Textures/Storm_UI_Icon_Abathur.dds")]
    [DataRow(@"assets/textures\storm_ui_icon_abathur.DDS")]
    public void TryGetFirstAssetFileByRelativePath_DifferentSeparatorsAndCasing_ReturnsFile(string relativePath)
    {
        // arrange
        StormFile expected = AddAssetFile(StormModType.Base, Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"));

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_PathLongerThanStackBuffer_ReturnsFile()
    {
        // arrange
        string relativePath = Path.Join("assets", new string('a', 300), "storm_ui_icon_abathur.dds");

        StormFile expected = AddAssetFile(StormModType.Base, relativePath);

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath.ToUpperInvariant(), out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_SliceOfLargerString_ReturnsFile()
    {
        // arrange
        StormFile expected = AddAssetFile(StormModType.Base, Path.Join("assets", "textures", "storm_ui_icon_abathur.dds"));

        string value = """<Icon value="Assets\Textures\storm_ui_icon_abathur.dds"/>""";
        ReadOnlySpan<char> relativePath = value.AsSpan(13, @"Assets\Textures\storm_ui_icon_abathur.dds".Length);

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_InBaseAndMap_ReturnsMapFile()
    {
        // arrange
        string relativePath = Path.Join("assets", "textures", "storm_ui_icon_abathur.dds");

        AddAssetFile(StormModType.Base, relativePath);
        StormFile mapFile = AddAssetFile(StormModType.Map, relativePath);

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(mapFile);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_InAllLayers_ReturnsCustomFile()
    {
        // arrange
        string relativePath = Path.Join("assets", "textures", "storm_ui_icon_abathur.dds");

        AddAssetFile(StormModType.Base, relativePath);
        AddAssetFile(StormModType.Map, relativePath);
        StormFile customFile = AddAssetFile(StormModType.Custom, relativePath);

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(customFile);
    }

    [TestMethod]
    public void TryGetFirstAssetFileByRelativePath_StormLayoutFileWithSamePath_ReturnsFalse()
    {
        // arrange
        string relativePath = Path.Join("ui", "layout", "hero", "herotalents.stormlayout");

        AddStormLayoutFile(StormModType.Base, relativePath);

        // act
        bool result = _layeredFileCache.TryGetFirstAssetFileByRelativePath(relativePath, out _);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstStormLayoutFileByRelativePath_FileInSingleLayer_ReturnsFile(StormModType stormModType)
    {
        // arrange
        StormFile expected = AddStormLayoutFile(stormModType, Path.Join("ui", "layout", "hero", "herotalents.stormlayout"));

        // act
        bool result = _layeredFileCache.TryGetFirstStormLayoutFileByRelativePath(Path.Join("ui", "layout", "hero", "herotalents.stormlayout"), out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstStormLayoutFileByRelativePath_PathNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        AddStormLayoutFile(StormModType.Base, Path.Join("ui", "layout", "hero", "herotalents.stormlayout"));

        // act
        bool result = _layeredFileCache.TryGetFirstStormLayoutFileByRelativePath(Path.Join("ui", "layout", "hero", "heroselect.stormlayout"), out StormFile? stormFile);

        // assert
        result.Should().BeFalse();
        stormFile.Should().BeNull();
    }

    [TestMethod]
    [DataRow(@"UI\Layout\Hero\HeroTalents.StormLayout")]
    [DataRow("UI/Layout/Hero/HeroTalents.StormLayout")]
    public void TryGetFirstStormLayoutFileByRelativePath_DifferentSeparatorsAndCasing_ReturnsFile(string relativePath)
    {
        // arrange
        StormFile expected = AddStormLayoutFile(StormModType.Base, Path.Join("ui", "layout", "hero", "herotalents.stormlayout"));

        // act
        bool result = _layeredFileCache.TryGetFirstStormLayoutFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstStormLayoutFileByRelativePath_InAllLayers_ReturnsCustomFile()
    {
        // arrange
        string relativePath = Path.Join("ui", "layout", "hero", "herotalents.stormlayout");

        AddStormLayoutFile(StormModType.Base, relativePath);
        AddStormLayoutFile(StormModType.Map, relativePath);
        StormFile customFile = AddStormLayoutFile(StormModType.Custom, relativePath);

        // act
        bool result = _layeredFileCache.TryGetFirstStormLayoutFileByRelativePath(relativePath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(customFile);
    }

    // adds a new storm file directly to the asset files cache of the given layer, keyed by the (already normalized) relative path
    private StormFile AddAssetFile(StormModType stormModType, string relativePath)
    {
        StormFile stormFile = StormFileHelper.CreateStormFile();
        GetFileCache(stormModType).AssetFilesByRelativePath.Dictionary[relativePath] = stormFile;

        return stormFile;
    }

    // adds a new storm file directly to the storm layout files cache of the given layer, keyed by the (already normalized) relative path
    private StormFile AddStormLayoutFile(StormModType stormModType, string relativePath)
    {
        StormFile stormFile = StormFileHelper.CreateStormFile();
        GetFileCache(stormModType).StormLayoutFilesByRelativePath.Dictionary[relativePath] = stormFile;

        return stormFile;
    }

    private FileCache GetFileCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseFileCache,
        StormModType.Map => _mapFileCache,
        StormModType.Custom => _customFileCache,
        _ => _customFileCache,
    };
}
