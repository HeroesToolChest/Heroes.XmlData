namespace Heroes.StormData.Tests;

[TestClass]
public sealed class StormCatalogTests
{
    private const string GameStringId = "Button/Name/AlarakDeadlyChargeActivate";
    private const string AssetStringId = "UI/HeroIcons/Alarak";
    private const string StormLayoutPath = "ui/layout/hero/herotalents.stormlayout";
    private const string ChargeDamageGameString = "deals <c val=\"#TooltipNumbers\"><d const=\"$AlarakChargeDamage\" precision=\"1\"/></c> damage";

    private readonly LayeredDataStorage _layeredDataStorage = new();
    private readonly StormCatalog _stormCatalog;

    public StormCatalogTests()
    {
        _stormCatalog = new StormCatalog(_layeredDataStorage);
    }

    [TestMethod]
    public void Constructor_NullLayeredDataStorage_ThrowsArgumentNullException()
    {
        // act
        Action act = () => _ = new StormCatalog(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GameStringExists_GameStringLoaded_ReturnsTrue()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        bool result = _stormCatalog.GameStringExists(GameStringId);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void GameStringExists_GameStringNotLoaded_ReturnsFalse()
    {
        // act
        bool result = _stormCatalog.GameStringExists(GameStringId);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GameStringExists_EmptyId_ReturnsFalse()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        bool result = _stormCatalog.GameStringExists(ReadOnlySpan<char>.Empty);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void TryGetGameString_GameStringLoaded_ReturnsTrueAndValue()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        bool result = _stormCatalog.TryGetGameString(GameStringId, out string? gameString);

        // assert
        result.Should().BeTrue();
        gameString.Should().Be("Deadly Charge");
    }

    [TestMethod]
    public void TryGetGameString_GameStringNotLoaded_ReturnsFalseAndNull()
    {
        // act
        bool result = _stormCatalog.TryGetGameString(GameStringId, out string? gameString);

        // assert
        result.Should().BeFalse();
        gameString.Should().BeNull();
    }

    [TestMethod]
    public void TryGetGameString_InBaseMapAndCustom_ReturnsCustomValue()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "base");
        AddGameString(StormModType.Map, GameStringId, "map");
        AddGameString(StormModType.Custom, GameStringId, "custom");

        // act
        bool result = _stormCatalog.TryGetGameString(GameStringId, out string? gameString);

        // assert
        result.Should().BeTrue();
        gameString.Should().Be("custom");
    }

    [TestMethod]
    public void TryGetGameString_IdAsSpanSlice_ReturnsValue()
    {
        // arrange - mirrors a lookup from a gamestring tag, where the id is a slice of the text
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        bool result = _stormCatalog.TryGetGameString($"<d ref=\"{GameStringId}\"/>".AsSpan(8, GameStringId.Length), out string? gameString);

        // assert
        result.Should().BeTrue();
        gameString.Should().Be("Deadly Charge");
    }

    [TestMethod]
    public void TryGetGameString_MapGameStringsCleared_ReturnsBaseValue()
    {
        // arrange - the catalog is a live view, so it reflects data that is removed after it was created
        AddGameString(StormModType.Base, GameStringId, "base");
        AddGameString(StormModType.Map, GameStringId, "map");

        _layeredDataStorage.ClearDataStorage(StormModType.Map);

        // act
        bool result = _stormCatalog.TryGetGameString(GameStringId, out string? gameString);

        // assert
        result.Should().BeTrue();
        gameString.Should().Be("base");
    }

    [TestMethod]
    public void GetGameString_GameStringLoaded_ReturnsValue()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        string gameString = _stormCatalog.GetGameString(GameStringId);

        // assert
        gameString.Should().Be("Deadly Charge");
    }

    [TestMethod]
    public void GetGameString_ValueIsEmpty_ReturnsEmpty()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, string.Empty);

        // act
        string gameString = _stormCatalog.GetGameString(GameStringId);

        // assert
        gameString.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGameString_GameStringNotLoaded_ThrowsKeyNotFoundExceptionWithId()
    {
        // act
        Action act = () => _stormCatalog.GetGameString(GameStringId);

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{GameStringId}*");
    }

    [TestMethod]
    public void GetGameString_GameStringsCleared_ThrowsKeyNotFoundException()
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        _layeredDataStorage.ClearGameStrings();

        // act
        Action act = () => _stormCatalog.GetGameString(GameStringId);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void AssetStringExists_AssetStringLoaded_ReturnsTrue()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        bool result = _stormCatalog.AssetStringExists(AssetStringId);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void AssetStringExists_AssetStringNotLoaded_ReturnsFalse()
    {
        // act
        bool result = _stormCatalog.AssetStringExists(AssetStringId);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void AssetStringExists_EmptyId_ReturnsFalse()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        bool result = _stormCatalog.AssetStringExists(ReadOnlySpan<char>.Empty);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void AssetStringExists_IdWithLeadingAt_ReturnsFalse()
    {
        // arrange - the id does not include the '@' used to reference it in XML
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        bool result = _stormCatalog.AssetStringExists($"@{AssetStringId}");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void AssetStringExists_OnlyAGameString_ReturnsFalse()
    {
        // arrange - asset strings and gamestrings are separate lookups
        AddGameString(StormModType.Base, AssetStringId, "Alarak");

        // act
        bool result = _stormCatalog.AssetStringExists(AssetStringId);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void TryGetAssetString_AssetStringLoaded_ReturnsTrueAndValue()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        bool result = _stormCatalog.TryGetAssetString(AssetStringId, out string? assetString);

        // assert
        result.Should().BeTrue();
        assetString.Should().Be("Assets/Textures/storm_ui_icon_alarak.dds");
    }

    [TestMethod]
    public void TryGetAssetString_AssetStringNotLoaded_ReturnsFalseAndNull()
    {
        // act
        bool result = _stormCatalog.TryGetAssetString(AssetStringId, out string? assetString);

        // assert
        result.Should().BeFalse();
        assetString.Should().BeNull();
    }

    [TestMethod]
    public void TryGetAssetString_InBaseMapAndCustom_ReturnsCustomValue()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, "base.dds");
        AddAssetString(StormModType.Map, AssetStringId, "map.dds");
        AddAssetString(StormModType.Custom, AssetStringId, "custom.dds");

        // act
        bool result = _stormCatalog.TryGetAssetString(AssetStringId, out string? assetString);

        // assert
        result.Should().BeTrue();
        assetString.Should().Be("custom.dds");
    }

    [TestMethod]
    public void TryGetAssetString_IdAsSpanSliceWithoutLeadingAt_ReturnsValue()
    {
        // arrange - mirrors the XML lookup, where the leading '@' is sliced off the attribute value
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        bool result = _stormCatalog.TryGetAssetString($"@{AssetStringId}".AsSpan(1), out string? assetString);

        // assert
        result.Should().BeTrue();
        assetString.Should().Be("Assets/Textures/storm_ui_icon_alarak.dds");
    }

    [TestMethod]
    public void TryGetAssetString_MapDataCleared_ReturnsBaseValue()
    {
        // arrange - the catalog is a live view, so it reflects data that is removed after it was created
        AddAssetString(StormModType.Base, AssetStringId, "base.dds");
        AddAssetString(StormModType.Map, AssetStringId, "map.dds");

        _layeredDataStorage.ClearDataStorage(StormModType.Map);

        // act
        bool result = _stormCatalog.TryGetAssetString(AssetStringId, out string? assetString);

        // assert
        result.Should().BeTrue();
        assetString.Should().Be("base.dds");
    }

    [TestMethod]
    public void TryGetAssetString_GameStringsCleared_StillReturnsValue()
    {
        // arrange - clearing the gamestrings (such as when changing the locale) does not remove the asset strings
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        _layeredDataStorage.ClearGameStrings();

        // act
        bool result = _stormCatalog.TryGetAssetString(AssetStringId, out string? assetString);

        // assert
        result.Should().BeTrue();
        assetString.Should().Be("Assets/Textures/storm_ui_icon_alarak.dds");
    }

    [TestMethod]
    public void GetAssetString_AssetStringLoaded_ReturnsValue()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, "Assets/Textures/storm_ui_icon_alarak.dds");

        // act
        string assetString = _stormCatalog.GetAssetString(AssetStringId);

        // assert
        assetString.Should().Be("Assets/Textures/storm_ui_icon_alarak.dds");
    }

    [TestMethod]
    public void GetAssetString_ValueIsEmpty_ReturnsEmpty()
    {
        // arrange
        AddAssetString(StormModType.Base, AssetStringId, string.Empty);

        // act
        string assetString = _stormCatalog.GetAssetString(AssetStringId);

        // assert
        assetString.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAssetString_AssetStringNotLoaded_ThrowsKeyNotFoundExceptionWithId()
    {
        // act
        Action act = () => _stormCatalog.GetAssetString(AssetStringId);

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{AssetStringId}*");
    }

    [TestMethod]
    public void StormLayoutFileExists_FileLoaded_ReturnsTrue()
    {
        // arrange
        AddStormLayoutFile(StormModType.Base, StormLayoutPath);

        // act
        bool result = _stormCatalog.StormLayoutFileExists(StormLayoutPath);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_FileNotLoaded_ReturnsFalse()
    {
        // act
        bool result = _stormCatalog.StormLayoutFileExists(StormLayoutPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void StormLayoutFileExists_EmptyPath_ReturnsFalse()
    {
        // arrange
        AddStormLayoutFile(StormModType.Base, StormLayoutPath);

        // act
        bool result = _stormCatalog.StormLayoutFileExists(ReadOnlySpan<char>.Empty);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("UI/Layout/Hero/HeroTalents.StormLayout")]
    [DataRow(@"ui\layout\hero\herotalents.stormlayout")]
    [DataRow("ui/layout/hero/herotalents.stormlayout")]
    public void StormLayoutFileExists_PathWithDifferentCasingOrSeparators_ReturnsTrue(string relativePath)
    {
        // arrange
        AddStormLayoutFile(StormModType.Base, StormLayoutPath);

        // act
        bool result = _stormCatalog.StormLayoutFileExists(relativePath);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_OnlyAnAssetFileWithSamePath_ReturnsFalse()
    {
        // arrange - layout files and asset files are separate lookups
        _layeredDataStorage.GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary[PathNormalizer.NormalizePath(StormLayoutPath)] = StormFileHelper.CreateStormFile();

        // act
        bool result = _stormCatalog.StormLayoutFileExists(StormLayoutPath);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void TryGetStormLayoutFile_FileLoaded_ReturnsTrueAndFile()
    {
        // arrange
        StormFile expected = AddStormLayoutFile(StormModType.Base, StormLayoutPath);

        // act
        bool result = _stormCatalog.TryGetStormLayoutFile(StormLayoutPath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetStormLayoutFile_FileNotLoaded_ReturnsFalseAndNull()
    {
        // act
        bool result = _stormCatalog.TryGetStormLayoutFile(StormLayoutPath, out StormFile? stormFile);

        // assert
        result.Should().BeFalse();
        stormFile.Should().BeNull();
    }

    [TestMethod]
    public void TryGetStormLayoutFile_InBaseMapAndCustom_ReturnsCustomFile()
    {
        // arrange
        AddStormLayoutFile(StormModType.Base, StormLayoutPath);
        AddStormLayoutFile(StormModType.Map, StormLayoutPath);
        StormFile expected = AddStormLayoutFile(StormModType.Custom, StormLayoutPath);

        // act
        bool result = _stormCatalog.TryGetStormLayoutFile(StormLayoutPath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetStormLayoutFile_MapDataCleared_ReturnsBaseFile()
    {
        // arrange - the catalog is a live view, so it reflects data that is removed after it was created
        StormFile expected = AddStormLayoutFile(StormModType.Base, StormLayoutPath);
        AddStormLayoutFile(StormModType.Map, StormLayoutPath);

        _layeredDataStorage.ClearDataStorage(StormModType.Map);

        // act
        bool result = _stormCatalog.TryGetStormLayoutFile(StormLayoutPath, out StormFile? stormFile);

        // assert
        result.Should().BeTrue();
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormLayoutFile_FileLoaded_ReturnsFile()
    {
        // arrange
        StormFile expected = AddStormLayoutFile(StormModType.Base, StormLayoutPath);

        // act
        StormFile stormFile = _stormCatalog.GetStormLayoutFile(StormLayoutPath);

        // assert
        stormFile.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormLayoutFile_FileNotLoaded_ThrowsKeyNotFoundExceptionWithPath()
    {
        // act
        Action act = () => _stormCatalog.GetStormLayoutFile(StormLayoutPath);

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{StormLayoutPath}*");
    }

    [TestMethod]
    public void GameStringsLocale_NewCatalog_IsNull()
    {
        // assert
        _stormCatalog.GameStringsLocale.Should().BeNull();
    }

    [TestMethod]
    public void ParseGameString_NoDataTags_ReturnsTextUnchanged()
    {
        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString("Deadly Charge");

        // assert
        gameStringText.RawText.Should().Be("Deadly Charge");
    }

    [TestMethod]
    public void ParseGameString_HasDataTag_ReplacesTagWithValue()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");

        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString(ChargeDamageGameString);

        // assert
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
    }

    [TestMethod]
    public void ParseGameString_NoLocaleAndNoGameStringsLoaded_UsesENUS()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");

        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString(ChargeDamageGameString);

        // assert
        gameStringText.GameStringLocale.Should().Be(StormLocale.ENUS);
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
    }

    [TestMethod]
    public void ParseGameString_NoLocale_UsesGameStringsLocale()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");
        _stormCatalog.GameStringsLocale = StormLocale.DEDE;

        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString(ChargeDamageGameString);

        // assert
        gameStringText.GameStringLocale.Should().Be(StormLocale.DEDE);
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1,5</c> damage");
    }

    [TestMethod]
    public void ParseGameString_LocaleGiven_UsesItInsteadOfGameStringsLocale()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");
        _stormCatalog.GameStringsLocale = StormLocale.DEDE;

        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString(ChargeDamageGameString, StormLocale.ENUS);

        // assert
        gameStringText.GameStringLocale.Should().Be(StormLocale.ENUS);
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
    }

    [TestMethod]
    public void ParseGameString_UndefinedLocale_ThrowsArgumentOutOfRangeException()
    {
        // act
        Action act = () => _stormCatalog.ParseGameString("Deadly Charge", (StormLocale)999);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("locale");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ParseGameString_ExtractFontValues_PassedToGameStringText(bool extractFontValues)
    {
        // act
        GameStringText gameStringText = _stormCatalog.ParseGameString("Deadly Charge", extractFontValues: extractFontValues);

        // assert
        gameStringText.IsFontValuesExtracted.Should().Be(extractFontValues);
    }

    [TestMethod]
    public void ParseGameString_DataChangedAfterParse_ParsesWithCurrentData()
    {
        // arrange - the catalog is a live view, so a higher layer's value is used once it is loaded
        AddConst("$AlarakChargeDamage", "1.5");
        GameStringText before = _stormCatalog.ParseGameString(ChargeDamageGameString);

        AddConst("$AlarakChargeDamage", "2.5", StormModType.Map);

        // act
        GameStringText after = _stormCatalog.ParseGameString(ChargeDamageGameString);

        // assert
        before.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
        after.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">2.5</c> damage");
    }

    [TestMethod]
    public void TryGetParsedGameString_GameStringLoaded_ReturnsTrueAndParsedText()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");
        AddGameString(StormModType.Base, GameStringId, ChargeDamageGameString);

        // act
        bool result = _stormCatalog.TryGetParsedGameString(GameStringId, out GameStringText? gameStringText);

        // assert
        result.Should().BeTrue();
        gameStringText!.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
    }

    [TestMethod]
    public void TryGetParsedGameString_GameStringLoaded_UsesGameStringsLocale()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");
        AddGameString(StormModType.Base, GameStringId, ChargeDamageGameString);
        _stormCatalog.GameStringsLocale = StormLocale.FRFR;

        // act
        bool result = _stormCatalog.TryGetParsedGameString(GameStringId, out GameStringText? gameStringText);

        // assert
        result.Should().BeTrue();
        gameStringText!.GameStringLocale.Should().Be(StormLocale.FRFR);
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1,5</c> damage");
    }

    [TestMethod]
    public void TryGetParsedGameString_GameStringNotLoaded_ReturnsFalseAndNull()
    {
        // act
        bool result = _stormCatalog.TryGetParsedGameString(GameStringId, out GameStringText? gameStringText);

        // assert
        result.Should().BeFalse();
        gameStringText.Should().BeNull();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void TryGetParsedGameString_ExtractFontValues_PassedToGameStringText(bool extractFontValues)
    {
        // arrange
        AddGameString(StormModType.Base, GameStringId, "Deadly Charge");

        // act
        bool result = _stormCatalog.TryGetParsedGameString(GameStringId, out GameStringText? gameStringText, extractFontValues);

        // assert
        result.Should().BeTrue();
        gameStringText!.IsFontValuesExtracted.Should().Be(extractFontValues);
    }

    [TestMethod]
    public void GetParsedGameString_GameStringLoaded_ReturnsParsedText()
    {
        // arrange
        AddConst("$AlarakChargeDamage", "1.5");
        AddGameString(StormModType.Base, GameStringId, ChargeDamageGameString);

        // act
        GameStringText gameStringText = _stormCatalog.GetParsedGameString(GameStringId);

        // assert
        gameStringText.RawText.Should().Be("deals <c val=\"#TooltipNumbers\">1.5</c> damage");
    }

    [TestMethod]
    public void GetParsedGameString_GameStringNotLoaded_ThrowsKeyNotFoundExceptionWithId()
    {
        // act
        Action act = () => _stormCatalog.GetParsedGameString(GameStringId);

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{GameStringId}*");
    }

    [TestMethod]
    public void TryGetCatalogName_ElementNameWithDeclaration_ReturnsTrueAndCatalogName()
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");

        // act
        bool result = _stormCatalog.TryGetCatalogName("CEffectDamage", out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void TryGetCatalogName_ElementNameWithoutDeclaration_ReturnsFalse()
    {
        // arrange - the name must be as written in the XML, with the leading C
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");

        // act
        bool result = _stormCatalog.TryGetCatalogName("EffectDamage", out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetCatalogName_ElementNameStartingWithCWithoutDeclaration_ReturnsFalse()
    {
        // arrange - CCamera is stored as Camera, passing Camera has its first letter removed (amera), so it is not found
        AddBaseClassCatalogName(StormModType.Base, "CCamera", "Camera");

        // act
        bool result = _stormCatalog.TryGetCatalogName("Camera", out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    [DataRow("CEFFECTDAMAGE")]
    [DataRow("ceffectdamage")]
    public void TryGetCatalogName_ElementNameWithDifferentCasing_ReturnsCatalogName(string elementName)
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");

        // act
        bool result = _stormCatalog.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("C")]
    public void TryGetCatalogName_EmptyOrOnlyDeclaration_ReturnsFalse(string elementName)
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");

        // act
        bool result = _stormCatalog.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetCatalogName_ElementNameNotLoaded_ReturnsFalse()
    {
        // act
        bool result = _stormCatalog.TryGetCatalogName("CEffectDamage", out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetCatalogName_InBaseAndCustom_ReturnsCustomCatalogName()
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");
        AddBaseClassCatalogName(StormModType.Custom, "CEffectDamage", "EffectCustom");

        // act
        bool result = _stormCatalog.TryGetCatalogName("CEffectDamage", out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("EffectCustom");
    }

    [TestMethod]
    public void GetCatalogName_ElementNameLoaded_ReturnsCatalogName()
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");

        // act
        string catalogName = _stormCatalog.GetCatalogName("CEffectDamage");

        // assert
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void GetCatalogName_ElementNameNotLoaded_ThrowsKeyNotFoundExceptionWithElementName()
    {
        // act
        Action act = () => _stormCatalog.GetCatalogName("CEffectDamage");

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("*CEffectDamage*");
    }

    [TestMethod]
    public void GetElementNames_CatalogLoaded_ReturnsElementNamesWithDeclarationSorted()
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectSet", "Effect");
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");
        AddBaseClassCatalogName(StormModType.Base, "CAbilEffectTarget", "Abil");

        // act
        IReadOnlyList<string> elementNames = _stormCatalog.GetElementNames("Effect");

        // assert
        elementNames.Should().Equal("CEffectDamage", "CEffectSet");
    }

    [TestMethod]
    public void GetElementNames_InMoreThanOneLayer_ReturnsUnionOnce()
    {
        // arrange
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");
        AddBaseClassCatalogName(StormModType.Map, "CEffectDamage", "Effect");
        AddBaseClassCatalogName(StormModType.Map, "CEffectSet", "Effect");
        AddBaseClassCatalogName(StormModType.Custom, "CEffectCreatePersistent", "Effect");

        // act
        IReadOnlyList<string> elementNames = _stormCatalog.GetElementNames("Effect");

        // assert
        elementNames.Should().Equal("CEffectCreatePersistent", "CEffectDamage", "CEffectSet");
    }

    [TestMethod]
    public void GetElementNames_CatalogNotLoaded_ReturnsEmpty()
    {
        // act
        IReadOnlyList<string> elementNames = _stormCatalog.GetElementNames("Effect");

        // assert
        elementNames.Should().BeEmpty();
    }

    [TestMethod]
    public void GetElementNames_ReturnedNames_EachFoundByTryGetCatalogName()
    {
        // arrange - the names are returned in the form that TryGetCatalogName takes
        AddBaseClassCatalogName(StormModType.Base, "CEffectDamage", "Effect");
        AddBaseClassCatalogName(StormModType.Base, "CEffectSet", "Effect");

        // act
        IReadOnlyList<string> elementNames = _stormCatalog.GetElementNames("Effect");

        // assert
        elementNames.Should().AllSatisfy(x => _stormCatalog.GetCatalogName(x).Should().Be("Effect"));
    }

    // adds the class element's catalog name the same way loading an XML file does
    private void AddBaseClassCatalogName(StormModType stormModType, string elementNameWithDeclaration, string catalogName)
    {
        _layeredDataStorage.GetDataStorage(stormModType).AddBaseClassCatalogName(XmlReaderHelper.GetStormXmlElement($"<{elementNameWithDeclaration} />"), catalogName);
    }

    // adds the const element directly to the data storage of the given layer
    private void AddConst(string id, string value, StormModType stormModType = StormModType.Base)
    {
        _layeredDataStorage.GetDataStorage(stormModType).AddConstElement(XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />"""));
    }

    // adds the gamestring directly to the text cache of the given layer
    private void AddGameString(StormModType stormModType, string id, string value)
    {
        _layeredDataStorage.GetDataStorage(stormModType).TextCache.GameStringsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }

    // adds the asset string directly to the text cache of the given layer
    private void AddAssetString(StormModType stormModType, string id, string value)
    {
        _layeredDataStorage.GetDataStorage(stormModType).TextCache.AssetTextsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }

    // adds the layout file directly to the file cache of the given layer, keyed by the normalized path like the loaded files are
    private StormFile AddStormLayoutFile(StormModType stormModType, string relativePath)
    {
        StormFile stormFile = StormFileHelper.CreateStormFile();
        _layeredDataStorage.GetDataStorage(stormModType).FileCache.StormLayoutFilesByRelativePath.Dictionary[PathNormalizer.NormalizePath(relativePath)] = stormFile;

        return stormFile;
    }
}
