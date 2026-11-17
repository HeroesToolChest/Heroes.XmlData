namespace Heroes.StormData.Cache.Strings.Tests;

[TestClass]
internal sealed class LayeredTextCacheTests
{
    private readonly TextCache _baseTextCache = new();
    private readonly TextCache _mapTextCache = new();
    private readonly TextCache _customTextCache = new();
    private readonly LayeredTextCache _layeredTextCache;

    public LayeredTextCacheTests()
    {
        _layeredTextCache = new LayeredTextCache(_baseTextCache, _mapTextCache, _customTextCache);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstAssetTextById_InSingleLayer_ReturnsValue(StormModType stormModType)
    {
        // arrange
        AddAssetText(stormModType, "AlarakPortrait", "Assets/Textures/ui_alarak_portrait.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("Assets/Textures/ui_alarak_portrait.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_NoLayersPopulated_ReturnsFalse()
    {
        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_IdNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        AddAssetText(StormModType.Base, "BaseId", "base.dds");
        AddAssetText(StormModType.Map, "MapId", "map.dds");
        AddAssetText(StormModType.Custom, "CustomId", "custom.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("DoesNotExist", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_IdWithDifferentCasing_ReturnsFalse()
    {
        // arrange - asset ids are stored with StringComparer.Ordinal, so lookups are case-sensitive
        AddAssetText(StormModType.Base, "AlarakPortrait", "alarak.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("alarakportrait", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_EmptyId_ReturnsFalse()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "alarak.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById(ReadOnlySpan<char>.Empty, out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_IdAsSpanSlice_ReturnsValue()
    {
        // arrange - mirrors the xml lookup, where the leading '@' is sliced off the attribute value
        AddAssetText(StormModType.Base, "AlarakPortrait", "alarak.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("@AlarakPortrait".AsSpan(1), out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("alarak.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_ValueIsEmpty_ReturnsTrueWithEmptyValue()
    {
        // arrange - an assets.txt entry with no value is stored as an empty string
        AddAssetText(StormModType.Base, "AlarakPortrait", string.Empty);

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().BeEmpty();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_InBaseAndMap_ReturnsMapValue()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "base.dds");
        AddAssetText(StormModType.Map, "AlarakPortrait", "map.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("map.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_InBaseAndCustom_ReturnsCustomValue()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "base.dds");
        AddAssetText(StormModType.Custom, "AlarakPortrait", "custom.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("custom.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_InMapAndCustom_ReturnsCustomValue()
    {
        // arrange
        AddAssetText(StormModType.Map, "AlarakPortrait", "map.dds");
        AddAssetText(StormModType.Custom, "AlarakPortrait", "custom.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("custom.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_InAllLayers_ReturnsCustomValue()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "base.dds");
        AddAssetText(StormModType.Map, "AlarakPortrait", "map.dds");
        AddAssetText(StormModType.Custom, "AlarakPortrait", "custom.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("custom.dds");
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_EmptyValueInHigherLayer_ReturnsEmptyValue()
    {
        // arrange - a higher layer with an empty value still takes precedence, it does not fall through to a lower layer
        AddAssetText(StormModType.Base, "AlarakPortrait", "base.dds");
        AddAssetText(StormModType.Custom, "AlarakPortrait", string.Empty);

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().BeEmpty();
    }

    [TestMethod]
    public void TryGetFirstAssetTextById_HigherLayerHasOtherIds_ReturnsLowerLayerValue()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "base.dds");
        AddAssetText(StormModType.Map, "OtherId", "map.dds");
        AddAssetText(StormModType.Custom, "AnotherId", "custom.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstAssetTextById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("base.dds");
    }

    // adds the asset text directly to the text cache of the given layer
    private void AddAssetText(StormModType stormModType, string id, string value)
    {
        GetTextCache(stormModType).AssetTextsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }

    private TextCache GetTextCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseTextCache,
        StormModType.Map => _mapTextCache,
        StormModType.Custom => _customTextCache,
        _ => _customTextCache,
    };
}
