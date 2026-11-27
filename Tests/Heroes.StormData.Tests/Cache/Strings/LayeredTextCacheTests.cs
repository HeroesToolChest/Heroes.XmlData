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
        // arrange - mirrors the XML lookup, where the leading '@' is sliced off the attribute value
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

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstGameStringById_InSingleLayer_ReturnsValue(StormModType stormModType)
    {
        // arrange
        AddGameString(stormModType, "Button/Name/AlarakDeadlyChargeActivate", "Deadly Charge");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("Button/Name/AlarakDeadlyChargeActivate", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("Deadly Charge");
    }

    [TestMethod]
    public void TryGetFirstGameStringById_IdNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        AddGameString(StormModType.Base, "BaseId", "base");
        AddGameString(StormModType.Map, "MapId", "map");
        AddGameString(StormModType.Custom, "CustomId", "custom");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("DoesNotExist", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstGameStringById_IdOnlyAnAssetText_ReturnsFalse()
    {
        // arrange - gamestrings and asset texts are separate lookups
        AddAssetText(StormModType.Base, "AlarakPortrait", "alarak.dds");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("AlarakPortrait", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstGameStringById_IdWithDifferentCasing_ReturnsFalse()
    {
        // arrange
        AddGameString(StormModType.Base, "Button/Name/AlarakDeadlyChargeActivate", "Deadly Charge");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("button/name/alarakdeadlychargeactivate", out string? value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstGameStringById_InAllLayers_ReturnsCustomValue()
    {
        // arrange
        AddGameString(StormModType.Base, "Button/Name/AlarakDeadlyChargeActivate", "base");
        AddGameString(StormModType.Map, "Button/Name/AlarakDeadlyChargeActivate", "map");
        AddGameString(StormModType.Custom, "Button/Name/AlarakDeadlyChargeActivate", "custom");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("Button/Name/AlarakDeadlyChargeActivate", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("custom");
    }

    [TestMethod]
    public void TryGetFirstGameStringById_InBaseAndMap_ReturnsMapValue()
    {
        // arrange
        AddGameString(StormModType.Base, "Button/Name/AlarakDeadlyChargeActivate", "base");
        AddGameString(StormModType.Map, "Button/Name/AlarakDeadlyChargeActivate", "map");

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("Button/Name/AlarakDeadlyChargeActivate", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().Be("map");
    }

    [TestMethod]
    public void TryGetFirstGameStringById_EmptyValueInHigherLayer_ReturnsEmptyValue()
    {
        // arrange
        AddGameString(StormModType.Base, "Button/Name/AlarakDeadlyChargeActivate", "base");
        AddGameString(StormModType.Custom, "Button/Name/AlarakDeadlyChargeActivate", string.Empty);

        // act
        bool result = _layeredTextCache.TryGetFirstGameStringById("Button/Name/AlarakDeadlyChargeActivate", out string? value);

        // assert
        result.Should().BeTrue();
        value.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGameStrings_InAllLayers_ReturnsEveryGameStringWithHighestLayerValue()
    {
        // arrange
        AddGameString(StormModType.Base, "Unit/Name/HeroMuradin", "Muradin");
        AddGameString(StormModType.Base, "Unit/Name/HeroButcher", "The Butcher");
        AddGameString(StormModType.Map, "Unit/Name/HeroMuradin", "Map Muradin");
        AddGameString(StormModType.Custom, "Unit/Name/HeroButcher", "Custom Butcher");
        AddGameString(StormModType.Custom, "Unit/Name/CustomUnit", "Custom Unit");

        // act
        Dictionary<string, string> gameStrings = _layeredTextCache.GetGameStrings();

        // assert
        gameStrings.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["Unit/Name/HeroMuradin"] = "Map Muradin",
            ["Unit/Name/HeroButcher"] = "Custom Butcher",
            ["Unit/Name/CustomUnit"] = "Custom Unit",
        });
    }

    [TestMethod]
    public void GetGameStrings_EmptyValue_IsIncluded()
    {
        // arrange
        AddGameString(StormModType.Base, "Unit/Name/HeroMuradin", string.Empty);

        // act
        Dictionary<string, string> gameStrings = _layeredTextCache.GetGameStrings();

        // assert
        gameStrings.Should().ContainKey("Unit/Name/HeroMuradin").WhoseValue.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGameStrings_AssetTexts_AreNotIncluded()
    {
        // arrange
        AddAssetText(StormModType.Base, "AlarakPortrait", "alarak.dds");

        // act
        Dictionary<string, string> gameStrings = _layeredTextCache.GetGameStrings();

        // assert
        gameStrings.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGameStrings_IdIsCaseSensitive_KeepsIdsDifferingOnlyByCasing()
    {
        // arrange
        AddGameString(StormModType.Base, "Unit/Name/HeroMuradin", "Muradin");
        AddGameString(StormModType.Map, "unit/name/heromuradin", "lowercase");

        // act
        Dictionary<string, string> gameStrings = _layeredTextCache.GetGameStrings();

        // assert
        gameStrings.Should().HaveCount(2);
        gameStrings.ContainsKey("UNIT/NAME/HEROMURADIN").Should().BeFalse();
    }

    [TestMethod]
    public void GetGameStrings_CalledTwice_ReturnsNewDictionaries()
    {
        // arrange
        AddGameString(StormModType.Base, "Unit/Name/HeroMuradin", "Muradin");

        // act
        Dictionary<string, string> first = _layeredTextCache.GetGameStrings();
        first["Other"] = "Other";

        Dictionary<string, string> second = _layeredTextCache.GetGameStrings();

        // assert
        second.Keys.Should().Equal("Unit/Name/HeroMuradin");
    }

    // adds the asset text directly to the text cache of the given layer
    private void AddAssetText(StormModType stormModType, string id, string value)
    {
        GetTextCache(stormModType).AssetTextsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }

    // adds the gamestring directly to the text cache of the given layer
    private void AddGameString(StormModType stormModType, string id, string value)
    {
        GetTextCache(stormModType).GameStringsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }

    private TextCache GetTextCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseTextCache,
        StormModType.Map => _mapTextCache,
        StormModType.Custom => _customTextCache,
        _ => _customTextCache,
    };
}
