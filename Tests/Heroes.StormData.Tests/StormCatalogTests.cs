namespace Heroes.StormData.Tests;

[TestClass]
public sealed class StormCatalogTests
{
    private const string GameStringId = "Button/Name/AlarakDeadlyChargeActivate";

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

    // adds the gamestring directly to the text cache of the given layer
    private void AddGameString(StormModType stormModType, string id, string value)
    {
        _layeredDataStorage.GetDataStorage(stormModType).TextCache.GameStringsById.Dictionary.Add(id, new StringValue(value, StormFileHelper.CreateStormFile()));
    }
}
