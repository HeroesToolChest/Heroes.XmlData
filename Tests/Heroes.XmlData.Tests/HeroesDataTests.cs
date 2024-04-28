using Heroes.XmlData.StormData;

namespace Heroes.XmlData.Tests;

[TestClass]
public class HeroesDataTests
{
    private readonly IStormStorage _stormStorage;
    private readonly HeroesData _heroesData;

    public HeroesDataTests()
    {
        _stormStorage = Substitute.For<IStormStorage>();

        _heroesData = new HeroesData(_stormStorage);
    }

    [TestMethod]
    [DataRow("cache-id1", true)]
    [DataRow("cache-id2", true)]
    [DataRow("mapcache-id2", true)]
    [DataRow("customcache-id2", true)]
    [DataRow("other", false)]
    [DataRow("", false)]
    public void IsGameStringExists_Id_ReturnsIsFound(string id, bool isFound)
    {
        // arrange
        const string sameId = "cache-id2";

        StormCache stormCache = new();
        stormCache.GameStringsById.Add("cache-id1", new GameStringText("value1", "path1"));
        stormCache.GameStringsById.Add(sameId, new GameStringText("value2", "path2"));
        stormCache.GameStringsById.Add("cache-id3", new GameStringText("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.GameStringsById.Add("mapcache-id1", new GameStringText("value1", "path1"));
        stormMapCache.GameStringsById.Add("mapcache-id2", new GameStringText("value2", "path2"));
        stormMapCache.GameStringsById.Add(sameId, new GameStringText("value5", "path5"));

        StormCache stormCustomCache = new();
        stormCustomCache.GameStringsById.Add("customcache-id1", new GameStringText("value1", "path1"));
        stormCustomCache.GameStringsById.Add("customcache-id2", new GameStringText("value2", "path2"));
        stormCustomCache.GameStringsById.Add(sameId, new GameStringText("value5", "path5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        // act
        bool result = _heroesData.IsGameStringExists(id);
        bool resultSpan = _heroesData.IsGameStringExists(id.AsSpan());

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);
    }

    [TestMethod]
    public void IsGameStringExists_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.IsGameStringExists(id);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("cache-id1", "value1", "path1")]
    [DataRow("mapcache-id1", "mapvalue1", "mappath1")]
    [DataRow("cache-id2", "customvalue5", "custompath5")]
    [DataRow("customcache-id2", "customvalue2", "custompath2")]
    [DataRow("cache-id4", "mapvalue6", "mapvalue6")]
    public void GetGameString_Id_ReturnsGameStringText(string id, string value, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id4";

        StormCache stormCache = new();
        stormCache.GameStringsById.Add("cache-id1", new GameStringText("value1", "path1"));
        stormCache.GameStringsById.Add(sameId, new GameStringText("value2", "path2"));
        stormCache.GameStringsById.Add("cache-id3", new GameStringText("value3", "path3"));
        stormCache.GameStringsById.Add(sameId2, new GameStringText("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.GameStringsById.Add("mapcache-id1", new GameStringText("mapvalue1", "mappath1"));
        stormMapCache.GameStringsById.Add("mapcache-id2", new GameStringText("mapvalue2", "mappath2"));
        stormMapCache.GameStringsById.Add(sameId, new GameStringText("mapvalue5", "mappath5"));
        stormMapCache.GameStringsById.Add(sameId2, new GameStringText("mapvalue6", "mapvalue6"));

        StormCache stormCustomCache = new();
        stormCustomCache.GameStringsById.Add("customcache-id1", new GameStringText("customvalue1", "custompath1"));
        stormCustomCache.GameStringsById.Add("customcache-id2", new GameStringText("customvalue2", "custompath2"));
        stormCustomCache.GameStringsById.Add(sameId, new GameStringText("customvalue5", "custompath5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        GameStringText outcome = new(value, path);

        // act
        GameStringText result = _heroesData.GetGameString(id);
        GameStringText resultSpan = _heroesData.GetGameString(id.AsSpan());

        // assert
        result.Should().Be(outcome);
        resultSpan.Should().Be(outcome);
    }

    [TestMethod]
    public void GetGameString_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.GetGameString(id);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetGameString_NonExistsId_ThrowsException()
    {
        // arrange
        string id = "other";

        _stormStorage.StormCache.Returns(new StormCache());
        _stormStorage.StormMapCache.Returns(new StormCache());
        _stormStorage.StormCustomCache.Returns(new StormCache());

        // act
        Action act = () => _heroesData.GetGameString(id);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    [DataRow("cache-id1", true, "value1", "path1")]
    [DataRow("mapcache-id1", true, "mapvalue1", "mappath1")]
    [DataRow("cache-id2", true, "customvalue5", "custompath5")]
    [DataRow("cache-id4", true, "mapvalue6", "mapvalue6")]
    [DataRow("other", false, null, null)]
    public void TryGetGameString_Id_ReturnsResult(string id, bool isFound, string value, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id4";

        StormCache stormCache = new();
        stormCache.GameStringsById.Add("cache-id1", new GameStringText("value1", "path1"));
        stormCache.GameStringsById.Add(sameId, new GameStringText("value2", "path2"));
        stormCache.GameStringsById.Add("cache-id3", new GameStringText("value3", "path3"));
        stormCache.GameStringsById.Add(sameId2, new GameStringText("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.GameStringsById.Add("mapcache-id1", new GameStringText("mapvalue1", "mappath1"));
        stormMapCache.GameStringsById.Add("mapcache-id2", new GameStringText("mapvalue2", "mappath2"));
        stormMapCache.GameStringsById.Add(sameId, new GameStringText("mapvalue5", "mappath5"));
        stormMapCache.GameStringsById.Add(sameId2, new GameStringText("mapvalue6", "mapvalue6"));

        StormCache stormCustomCache = new();
        stormCustomCache.GameStringsById.Add("customcache-id1", new GameStringText("customvalue1", "custompath1"));
        stormCustomCache.GameStringsById.Add("customcache-id2", new GameStringText("customvalue2", "custompath2"));
        stormCustomCache.GameStringsById.Add(sameId, new GameStringText("customvalue5", "custompath5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        GameStringText? outcome = null;
        if (value is not null && path is not null)
            outcome = new(value, path);

        // act
        bool result = _heroesData.TryGetGameString(id, out GameStringText? gameStringText);
        bool resultSpan = _heroesData.TryGetGameString(id.AsSpan(), out GameStringText? gameStringTextSpan);

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);
        gameStringText.Should().Be(outcome);
        gameStringTextSpan.Should().Be(outcome);
    }

    [TestMethod]
    public void TryGetGameString_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.TryGetGameString(id, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("catalog1", "entry1", "field1", true)]
    [DataRow("map-catalog1", "map-entry1", "map-field1", true)]
    [DataRow("catalog3", "entry3", "field3", true)]
    [DataRow("custom-catalog1", "custom-entry1", "custom-field1", true)]
    [DataRow("other", "other", "other", false)]
    [DataRow("", "", "", false)]
    public void IsLevelScalingEntryExists_Id_ReturnsIsFound(string catalog, string entry, string field, bool isFound)
    {
        // arrange
        LevelScalingEntry sameEntry = new("catalog3", "entry3", "field3");

        StormCache stormCache = new();
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog1", "entry1", "field1"), new StormStringValue("value1", "path1"));
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog2", "entry2", "field2"), new StormStringValue("value2", "path2"));
        stormCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog1", "map-entry1", "map-field1"), new StormStringValue("map-value1", "map-path1"));
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog2", "map-entry2", "map-field2"), new StormStringValue("map-value2", "map-path2"));
        stormMapCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("map-value5", "map-path5"));

        StormCache stormCustomCache = new();
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog1", "custom-entry1", "custom-field1"), new StormStringValue("custom-value1", "custom-path1"));
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog2", "custom-entry2", "custom-field2"), new StormStringValue("custom-value2", "custom-path2"));
        stormCustomCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("custom-value5", "custom-path5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        // act
        bool result = _heroesData.IsLevelScalingEntryExists(catalog, entry, field);

        // assert
        result.Should().Be(isFound);
    }

    [TestMethod]
    [DataRow(null, "", "")]
    [DataRow("", null, "")]
    [DataRow("", "", null)]
    public void IsLevelScalingEntryExists_ParameterNull_ThrowsException(string catalog, string entry, string field)
    {
        // arrange
        // act
        Action act = () => _heroesData.IsLevelScalingEntryExists(catalog, entry, field);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("catalog1", "entry1", "field1", "value1", "path1")]
    [DataRow("map-catalog1", "map-entry1", "map-field1", "map-value1", "map-path1")]
    [DataRow("catalog3", "entry3", "field3", "custom-value5", "custom-path5")]
    [DataRow("custom-catalog1", "custom-entry1", "custom-field1", "custom-value1", "custom-path1")]
    [DataRow("catalog4", "entry4", "field4", "map-value6", "map-path6")]
    public void GetLevelScalingEntryExists_Id_ReturnsIsFound(string catalog, string entry, string field, string value, string path)
    {
        // arrange
        LevelScalingEntry sameEntry = new("catalog3", "entry3", "field3");
        LevelScalingEntry sameEntry2 = new("catalog4", "entry4", "field4");

        StormCache stormCache = new();
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog1", "entry1", "field1"), new StormStringValue("value1", "path1"));
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog2", "entry2", "field2"), new StormStringValue("value2", "path2"));
        stormCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("value3", "path3"));
        stormCache.ScaleValueByEntry.Add(sameEntry2, new StormStringValue("value4", "path4"));

        StormCache stormMapCache = new();
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog1", "map-entry1", "map-field1"), new StormStringValue("map-value1", "map-path1"));
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog2", "map-entry2", "map-field2"), new StormStringValue("map-value2", "map-path2"));
        stormMapCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("map-value5", "map-path5"));
        stormMapCache.ScaleValueByEntry.Add(sameEntry2, new StormStringValue("map-value6", "map-path6"));

        StormCache stormCustomCache = new();
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog1", "custom-entry1", "custom-field1"), new StormStringValue("custom-value1", "custom-path1"));
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog2", "custom-entry2", "custom-field2"), new StormStringValue("custom-value2", "custom-path2"));
        stormCustomCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("custom-value5", "custom-path5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormStringValue outcome = new(value, path);

        // act
        StormStringValue result = _heroesData.GetLevelScalingEntryExists(catalog, entry, field);

        // assert
        result.Should().Be(outcome);
    }

    [TestMethod]
    [DataRow(null, "", "")]
    [DataRow("", null, "")]
    [DataRow("", "", null)]
    public void GetLevelScalingEntryExists_ParameterNull_ThrowsException(string catalog, string entry, string field)
    {
        // arrange
        // act
        Action act = () => _heroesData.GetLevelScalingEntryExists(catalog, entry, field);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetLevelScalingEntryExists_NonExistsId_ThrowsException()
    {
        // arrange
        string id = "other";

        _stormStorage.StormCache.Returns(new StormCache());
        _stormStorage.StormMapCache.Returns(new StormCache());
        _stormStorage.StormCustomCache.Returns(new StormCache());

        // act
        Action act = () => _heroesData.GetLevelScalingEntryExists(id, id, id);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    [DataRow("catalog1", "entry1", "field1", true, "value1", "path1")]
    [DataRow("map-catalog1", "map-entry1", "map-field1", true, "map-value1", "map-path1")]
    [DataRow("catalog3", "entry3", "field3", true, "custom-value5", "custom-path5")]
    [DataRow("custom-catalog1", "custom-entry1", "custom-field1", true, "custom-value1", "custom-path1")]
    [DataRow("catalog4", "entry4", "field4", true, "map-value6", "map-path6")]
    [DataRow("other", "other", "other", false, null, null)]
    public void TryGetLevelScalingEntryExists_Id_ReturnsResult(string catalog, string entry, string field, bool isFound, string value, string path)
    {
        // arrange
        LevelScalingEntry sameEntry = new("catalog3", "entry3", "field3");
        LevelScalingEntry sameEntry2 = new("catalog4", "entry4", "field4");

        StormCache stormCache = new();
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog1", "entry1", "field1"), new StormStringValue("value1", "path1"));
        stormCache.ScaleValueByEntry.Add(new LevelScalingEntry("catalog2", "entry2", "field2"), new StormStringValue("value2", "path2"));
        stormCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("value3", "path3"));
        stormCache.ScaleValueByEntry.Add(sameEntry2, new StormStringValue("value4", "path4"));

        StormCache stormMapCache = new();
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog1", "map-entry1", "map-field1"), new StormStringValue("map-value1", "map-path1"));
        stormMapCache.ScaleValueByEntry.Add(new LevelScalingEntry("map-catalog2", "map-entry2", "map-field2"), new StormStringValue("map-value2", "map-path2"));
        stormMapCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("map-value5", "map-path5"));
        stormMapCache.ScaleValueByEntry.Add(sameEntry2, new StormStringValue("map-value6", "map-path6"));

        StormCache stormCustomCache = new();
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog1", "custom-entry1", "custom-field1"), new StormStringValue("custom-value1", "custom-path1"));
        stormCustomCache.ScaleValueByEntry.Add(new LevelScalingEntry("custom-catalog2", "custom-entry2", "custom-field2"), new StormStringValue("custom-value2", "custom-path2"));
        stormCustomCache.ScaleValueByEntry.Add(sameEntry, new StormStringValue("custom-value5", "custom-path5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormStringValue? outcome = null;
        if (value is not null && path is not null)
            outcome = new(value, path);

        // act
        bool result = _heroesData.TryGetLevelScalingEntryExists(catalog, entry, field, out StormStringValue? stormStringValue);

        // assert
        result.Should().Be(isFound);
        stormStringValue.Should().Be(outcome);
    }

    [TestMethod]
    [DataRow(null, "", "")]
    [DataRow("", null, "")]
    [DataRow("", "", null)]
    public void TryGetGameString_ParameterNull_ThrowsException(string catalog, string entry, string field)
    {
        // arrange
        // act
        Action act = () => _heroesData.TryGetLevelScalingEntryExists(catalog, entry, field, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("cache-id1", true)]
    [DataRow("cache-id2", true)]
    [DataRow("mapcache-id2", true)]
    [DataRow("customcache-id2", true)]
    [DataRow("other", false)]
    [DataRow("", false)]
    public void IsStormStyleHexColorValueExists_Name_ReturnsIsFound(string name, bool isFound)
    {
        // arrange
        const string sameId = "cache-id2";

        StormCache stormCache = new();
        stormCache.StormStyleHexColorValueByName.Add("cache-id1", new StormStringValue("value1", "path1"));
        stormCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("value2", "path2"));
        stormCache.StormStyleHexColorValueByName.Add("cache-id3", new StormStringValue("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id1", new StormStringValue("value1", "path1"));
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id2", new StormStringValue("value2", "path2"));
        stormMapCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("value5", "path5"));

        StormCache stormCustomCache = new();
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id1", new StormStringValue("value1", "path1"));
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id2", new StormStringValue("value2", "path2"));
        stormCustomCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("value5", "path5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        // act
        bool result = _heroesData.IsStormStyleHexColorValueExists(name);
        bool resultSpan = _heroesData.IsStormStyleHexColorValueExists(name.AsSpan());

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);
    }

    [TestMethod]
    public void IsStormStyleHexColorValueExists_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.IsStormStyleHexColorValueExists(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("cache-id1", "value1", "path1")]
    [DataRow("mapcache-id1", "mapvalue1", "mappath1")]
    [DataRow("cache-id2", "customvalue5", "custompath5")]
    [DataRow("cache-id5", "mapvalue6", "mappath6")]
    [DataRow("customcache-id2", "customvalue2", "custompath2")]
    public void GetStormStyleHexColorValue_Name_ReturnsStormStringValue(string name, string value, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id5";

        StormCache stormCache = new();
        stormCache.StormStyleHexColorValueByName.Add("cache-id1", new StormStringValue("value1", "path1"));
        stormCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("value2", "path2"));
        stormCache.StormStyleHexColorValueByName.Add("cache-id3", new StormStringValue("value3", "path3"));
        stormCache.StormStyleHexColorValueByName.Add(sameId2, new StormStringValue("value4", "path4"));

        StormCache stormMapCache = new();
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id1", new StormStringValue("mapvalue1", "mappath1"));
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id2", new StormStringValue("mapvalue2", "mappath2"));
        stormMapCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("mapvalue5", "mappath5"));
        stormMapCache.StormStyleHexColorValueByName.Add(sameId2, new StormStringValue("mapvalue6", "mappath6"));

        StormCache stormCustomCache = new();
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id1", new StormStringValue("customvalue1", "custompath1"));
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id2", new StormStringValue("customvalue2", "custompath2"));
        stormCustomCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("customvalue5", "custompath5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormStringValue outcome = new(value, path);

        // act
        StormStringValue result = _heroesData.GetStormStyleHexColorValue(name);
        StormStringValue resultSpan = _heroesData.GetStormStyleHexColorValue(name.AsSpan());

        // assert
        result.Should().Be(outcome);
        resultSpan.Should().Be(outcome);
    }

    [TestMethod]
    public void GetStormStyleHexColorValue_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.GetStormStyleHexColorValue(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormStyleHexColorValue_NonExistsName_ThrowsException()
    {
        // arrange
        string name = "other";

        _stormStorage.StormCache.Returns(new StormCache());
        _stormStorage.StormMapCache.Returns(new StormCache());
        _stormStorage.StormCustomCache.Returns(new StormCache());

        // act
        Action act = () => _heroesData.GetStormStyleHexColorValue(name);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    [DataRow("cache-id1", true, "value1", "path1")]
    [DataRow("mapcache-id1", true, "mapvalue1", "mappath1")]
    [DataRow("cache-id2", true, "customvalue5", "custompath5")]
    [DataRow("customcache-id2", true, "customvalue2", "custompath2")]
    [DataRow("cache-id4", true, "mapvalue6", "mappath6")]
    [DataRow("other", false, null, null)]
    public void TryGetStormStyleHexColorValue_Name_ReturnsStormStringValue(string name, bool isFound, string value, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id4";

        StormCache stormCache = new();
        stormCache.StormStyleHexColorValueByName.Add("cache-id1", new StormStringValue("value1", "path1"));
        stormCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("value2", "path2"));
        stormCache.StormStyleHexColorValueByName.Add(sameId2, new StormStringValue("value4", "path4"));
        stormCache.StormStyleHexColorValueByName.Add("cache-id3", new StormStringValue("value3", "path3"));

        StormCache stormMapCache = new();
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id1", new StormStringValue("mapvalue1", "mappath1"));
        stormMapCache.StormStyleHexColorValueByName.Add("mapcache-id2", new StormStringValue("mapvalue2", "mappath2"));
        stormMapCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("mapvalue5", "mappath5"));
        stormMapCache.StormStyleHexColorValueByName.Add(sameId2, new StormStringValue("mapvalue6", "mappath6"));

        StormCache stormCustomCache = new();
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id1", new StormStringValue("customvalue1", "custompath1"));
        stormCustomCache.StormStyleHexColorValueByName.Add("customcache-id2", new StormStringValue("customvalue2", "custompath2"));
        stormCustomCache.StormStyleHexColorValueByName.Add(sameId, new StormStringValue("customvalue5", "custompath5"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormStringValue? outcome = null;
        if (value is not null && path is not null)
            outcome = new(value, path);

        // act
        bool result = _heroesData.TryGetStormStyleHexColorValue(name, out StormStringValue? stormStringValue);
        bool resultSpan = _heroesData.TryGetStormStyleHexColorValue(name.AsSpan(), out StormStringValue? stormStringValueSpan);

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);
        stormStringValue.Should().Be(outcome);
        stormStringValueSpan.Should().Be(outcome);
    }

    [TestMethod]
    public void TryGetStormStyleHexColorValue_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.TryGetStormStyleHexColorValue(name, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("cache-id1", true)]
    [DataRow("cache-id2", true)]
    [DataRow("mapcache-id2", true)]
    [DataRow("customcache-id2", true)]
    [DataRow("other", false)]
    [DataRow("", false)]
    public void IsConstantElementExists_Id_ReturnsIsFound(string id, bool isFound)
    {
        // arrange
        const string sameId = "cache-id2";

        StormCache stormCache = new();
        stormCache.ConstantXElementById.Add("cache-id1", new StormXElementValuePath(new XElement("const"), "path1"));
        stormCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("const"), "path2"));
        stormCache.ConstantXElementById.Add("cache-id3", new StormXElementValuePath(new XElement("const"), "path3"));

        StormCache stormMapCache = new();
        stormMapCache.ConstantXElementById.Add("mapcache-id1", new StormXElementValuePath(new XElement("const"), "mapcache-path1"));
        stormMapCache.ConstantXElementById.Add("mapcache-id2", new StormXElementValuePath(new XElement("const"), "mapcache-path2"));
        stormMapCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("const"), "mapcache-path3"));

        StormCache stormCustomCache = new();
        stormCustomCache.ConstantXElementById.Add("customcache-id1", new StormXElementValuePath(new XElement("const"), "customcache-path1"));
        stormCustomCache.ConstantXElementById.Add("customcache-id2", new StormXElementValuePath(new XElement("const"), "customcache-path2"));
        stormCustomCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("const"), "customcache-path3"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        // act
        bool result = _heroesData.IsConstantElementExists(id);
        bool resultSpan = _heroesData.IsConstantElementExists(id.AsSpan());

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);
    }

    [TestMethod]
    public void IsConstantElement_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.IsConstantElementExists(id);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("cache-id1", "const1", "path1")]
    [DataRow("mapcache-id1", "mapcache-const1", "mapcache-path1")]
    [DataRow("cache-id2", "customcache-const3", "customcache-path3")]
    [DataRow("customcache-id1", "customcache-const1", "customcache-path1")]
    [DataRow("cache-id5", "mapcache-const5", "mapcache-path5")]
    public void GetConstantElement_Id_ReturnsStormXElementValuePath(string id, string xElementName, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id5";

        StormCache stormCache = new();
        stormCache.ConstantXElementById.Add("cache-id1", new StormXElementValuePath(new XElement("const1"), "path1"));
        stormCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("const2"), "path2"));
        stormCache.ConstantXElementById.Add(sameId2, new StormXElementValuePath(new XElement("const4"), "path4"));
        stormCache.ConstantXElementById.Add("cache-id3", new StormXElementValuePath(new XElement("const3"), "path3"));

        StormCache stormMapCache = new();
        stormMapCache.ConstantXElementById.Add("mapcache-id1", new StormXElementValuePath(new XElement("mapcache-const1"), "mapcache-path1"));
        stormMapCache.ConstantXElementById.Add("mapcache-id2", new StormXElementValuePath(new XElement("mapcache-const2"), "mapcache-path2"));
        stormMapCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("mapcache-const3"), "mapcache-path3"));
        stormMapCache.ConstantXElementById.Add(sameId2, new StormXElementValuePath(new XElement("mapcache-const5"), "mapcache-path5"));

        StormCache stormCustomCache = new();
        stormCustomCache.ConstantXElementById.Add("customcache-id1", new StormXElementValuePath(new XElement("customcache-const1"), "customcache-path1"));
        stormCustomCache.ConstantXElementById.Add("customcache-id2", new StormXElementValuePath(new XElement("customcache-const2"), "customcache-path2"));
        stormCustomCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("customcache-const3"), "customcache-path3"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormXElementValuePath outcome = new(new XElement(xElementName), path);

        // act
        StormXElementValuePath result = _heroesData.GetConstantElement(id);
        StormXElementValuePath resultSpan = _heroesData.GetConstantElement(id.AsSpan());

        // assert
        result.Path.Should().Be(outcome.Path);
        result.Value.Name.Should().Be(outcome.Value.Name);
        resultSpan.Path.Should().Be(outcome.Path);
        resultSpan.Value.Name.Should().Be(outcome.Value.Name);
    }

    [TestMethod]
    public void GetConstantElement_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.GetConstantElement(id);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetConstantElement_NonExistsId_ThrowsException()
    {
        // arrange
        string id = "other";

        _stormStorage.StormCache.Returns(new StormCache());
        _stormStorage.StormMapCache.Returns(new StormCache());
        _stormStorage.StormCustomCache.Returns(new StormCache());

        // act
        Action act = () => _heroesData.GetConstantElement(id);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    [DataRow("cache-id1", true, "const1", "path1")]
    [DataRow("mapcache-id1", true, "mapcache-const1", "mapcache-path1")]
    [DataRow("cache-id2", true, "customcache-const3", "customcache-path3")]
    [DataRow("cache-id5", true, "mapcache-const5", "mapcache-path5")]
    [DataRow("customcache-id2", true, "customcache-const2", "customcache-path2")]
    [DataRow("other", false, null, null)]
    public void TryGetConstantElement_Id_ReturnsStormXElementValuePath(string id, bool isFound, string xElementName, string path)
    {
        // arrange
        const string sameId = "cache-id2";
        const string sameId2 = "cache-id5";

        StormCache stormCache = new();
        stormCache.ConstantXElementById.Add("cache-id1", new StormXElementValuePath(new XElement("const1"), "path1"));
        stormCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("const2"), "path2"));
        stormCache.ConstantXElementById.Add(sameId2, new StormXElementValuePath(new XElement("const4"), "path4"));
        stormCache.ConstantXElementById.Add("cache-id3", new StormXElementValuePath(new XElement("const3"), "path3"));

        StormCache stormMapCache = new();
        stormMapCache.ConstantXElementById.Add("mapcache-id1", new StormXElementValuePath(new XElement("mapcache-const1"), "mapcache-path1"));
        stormMapCache.ConstantXElementById.Add("mapcache-id2", new StormXElementValuePath(new XElement("mapcache-const2"), "mapcache-path2"));
        stormMapCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("mapcache-const3"), "mapcache-path3"));
        stormMapCache.ConstantXElementById.Add(sameId2, new StormXElementValuePath(new XElement("mapcache-const5"), "mapcache-path5"));

        StormCache stormCustomCache = new();
        stormCustomCache.ConstantXElementById.Add("customcache-id1", new StormXElementValuePath(new XElement("customcache-const1"), "customcache-path1"));
        stormCustomCache.ConstantXElementById.Add("customcache-id2", new StormXElementValuePath(new XElement("customcache-const2"), "customcache-path2"));
        stormCustomCache.ConstantXElementById.Add(sameId, new StormXElementValuePath(new XElement("customcache-const3"), "customcache-path3"));

        _stormStorage.StormCache.Returns(stormCache);
        _stormStorage.StormMapCache.Returns(stormMapCache);
        _stormStorage.StormCustomCache.Returns(stormCustomCache);

        StormXElementValuePath? outcome = null;
        if (xElementName is not null && path is not null)
            outcome = new(new XElement(xElementName), path);

        // act
        bool result = _heroesData.TryGetConstantElement(id, out StormXElementValuePath? stormXElementValue);
        bool resultSpan = _heroesData.TryGetConstantElement(id.AsSpan(), out StormXElementValuePath? stormXElementValueSpan);

        // assert
        result.Should().Be(isFound);
        resultSpan.Should().Be(isFound);

        stormXElementValue?.Path.Should().Be(outcome?.Path);
        stormXElementValue?.Value.Name.Should().Be(outcome?.Value.Name);
        stormXElementValueSpan?.Path.Should().Be(outcome?.Path);
        stormXElementValueSpan?.Value.Name.Should().Be(outcome?.Value.Name);
    }

    [TestMethod]
    public void TryGetConstantElement_NullId_ThrowsException()
    {
        // arrange
        string id = null!;

        // act
        Action act = () => _heroesData.TryGetConstantElement(id, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void IsElementExists_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.IsElementExists(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetElements_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.GetElements(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetElements_NonExistsName_ThrowsException()
    {
        // arrange
        string name = "other";

        _stormStorage.StormCache.Returns(new StormCache());
        _stormStorage.StormMapCache.Returns(new StormCache());
        _stormStorage.StormCustomCache.Returns(new StormCache());

        // act
        Action act = () => _heroesData.GetElements(name);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void TryGetElements_NullName_ThrowsException()
    {
        // arrange
        string name = null!;

        // act
        Action act = () => _heroesData.TryGetElements(name, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}