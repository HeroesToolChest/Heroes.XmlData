namespace Heroes.StormData.Cache.Elements.Tests;

[TestClass]
public sealed class LayeredAbilLinkCacheTests
{
    private readonly AbilLinkCache _baseAbilLinkCache = new();
    private readonly AbilLinkCache _mapAbilLinkCache = new();
    private readonly AbilLinkCache _customAbilLinkCache = new();
    private readonly LayeredAbilLinkCache _layeredAbilLinkCache;

    public LayeredAbilLinkCacheTests()
    {
        _layeredAbilLinkCache = new LayeredAbilLinkCache(_baseAbilLinkCache, _mapAbilLinkCache, _customAbilLinkCache);
    }

    [TestMethod]
    public void GetAbilLinkIds_NothingAdded_ReturnsEmpty()
    {
        // act
        Dictionary<int, string> ids = _layeredAbilLinkCache.GetAbilLinkIds();

        // assert
        ids.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAbilLinkIds_IdsAdded_FirstIndexIsOneAndIsInIndexOrder()
    {
        // arrange
        _baseAbilLinkCache.AddId("attack");
        _baseAbilLinkCache.AddId("move");
        _mapAbilLinkCache.AddId("MapAbility");

        // act
        Dictionary<int, string> ids = _layeredAbilLinkCache.GetAbilLinkIds();

        // assert
        ids.Keys.Should().Equal(1, 2, 3);
        ids.Values.Should().Equal("attack", "move", "MapAbility");
    }

    [TestMethod]
    public void GetAbilLinkIds_DefaultElementsAndIds_DefaultElementsTakeAnIndexWithNoId()
    {
        // arrange
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddDefaultElement("CAbilEffectTarget");
        _baseAbilLinkCache.AddId("ReviveSelf");
        _baseAbilLinkCache.AddDefaultElement("CAbilEffectTarget");
        _baseAbilLinkCache.AddId("stop");
        _baseAbilLinkCache.AddId("ReviveSelf");

        // act
        Dictionary<int, string> ids = _layeredAbilLinkCache.GetAbilLinkIds();

        // assert
        ids.Should().Equal(CreateIds(string.Empty, string.Empty, "ReviveSelf", "stop"));
    }

    [TestMethod]
    public void GetAbilLinkIds_IdsInEveryCache_BaseThenMapThenCustomWithEachIdOnce()
    {
        // arrange
        _customAbilLinkCache.AddId("CustomAbility");
        _customAbilLinkCache.AddId("MapAbility");
        _mapAbilLinkCache.AddId("attack");
        _mapAbilLinkCache.AddId("MapAbility");
        _mapAbilLinkCache.AddDefaultElement("CAbil");
        _mapAbilLinkCache.AddDefaultElement("CAbilMap");
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddId("attack");
        _baseAbilLinkCache.AddId("move");

        // act
        Dictionary<int, string> ids = _layeredAbilLinkCache.GetAbilLinkIds();

        // assert
        ids.Should().Equal(CreateIds(string.Empty, "attack", "move", "MapAbility", string.Empty, "CustomAbility"));
    }

    [TestMethod]
    public void GetAbilLinkIds_ReturnedDictionaryIsChanged_DoesNotChangeTheNextOne()
    {
        // arrange
        _baseAbilLinkCache.AddId("attack");

        Dictionary<int, string> ids = _layeredAbilLinkCache.GetAbilLinkIds();
        ids[1] = "changed";

        // act
        Dictionary<int, string> result = _layeredAbilLinkCache.GetAbilLinkIds();

        // assert
        result.Should().Equal(CreateIds("attack"));
    }

    [TestMethod]
    public void GetAbilLinkIds_IdAddedAfterGet_ReturnsUpdatedIds()
    {
        // arrange
        _baseAbilLinkCache.AddId("attack");
        _ = _layeredAbilLinkCache.GetAbilLinkIds();

        // act
        _mapAbilLinkCache.AddId("MapAbility");

        // assert
        _layeredAbilLinkCache.GetAbilLinkIds().Should().Equal(CreateIds("attack", "MapAbility"));
    }

    [TestMethod]
    public void GetAbilLinkIds_MapCacheClearedAfterGet_ReturnsIdsWithoutMapIds()
    {
        // arrange
        _baseAbilLinkCache.AddId("attack");
        _mapAbilLinkCache.AddId("MapAbility");
        _customAbilLinkCache.AddId("CustomAbility");
        _ = _layeredAbilLinkCache.GetAbilLinkIds();

        // act
        _mapAbilLinkCache.Clear();

        // assert
        _layeredAbilLinkCache.GetAbilLinkIds().Should().Equal(CreateIds("attack", "CustomAbility"));
        _layeredAbilLinkCache.TryGetAbilLink("MapAbility", out _).Should().BeFalse();
    }

    [TestMethod]
    public void TryGetAbilLinkId_IndexHasId_ReturnsTrueAndId()
    {
        // arrange
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddId("attack");

        // act
        bool result = _layeredAbilLinkCache.TryGetAbilLinkId(2, out string? id);

        // assert
        result.Should().BeTrue();
        id.Should().Be("attack");
    }

    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(3)]
    [DataRow(int.MaxValue)]
    public void TryGetAbilLinkId_IndexHasNoId_ReturnsFalse(int abilLink)
    {
        // arrange
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddId("attack");

        // act
        bool result = _layeredAbilLinkCache.TryGetAbilLinkId(abilLink, out string? id);

        // assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [TestMethod]
    public void TryGetAbilLink_IdHasIndex_ReturnsTrueAndIndex()
    {
        // arrange
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddId("attack");
        _mapAbilLinkCache.AddId("MapAbility");

        // act
        bool result = _layeredAbilLinkCache.TryGetAbilLink("xMapAbilityx".AsSpan(1, 10), out int abilLink);

        // assert
        result.Should().BeTrue();
        abilLink.Should().Be(3);
    }

    [TestMethod]
    [DataRow("Attack")]
    [DataRow("CAbil")]
    [DataRow("")]
    public void TryGetAbilLink_IdHasNoIndex_ReturnsFalse(string id)
    {
        // arrange
        _baseAbilLinkCache.AddDefaultElement("CAbil");
        _baseAbilLinkCache.AddId("attack");

        // act
        bool result = _layeredAbilLinkCache.TryGetAbilLink(id, out int abilLink);

        // assert
        result.Should().BeFalse();
        abilLink.Should().Be(0);
    }

    // the ids by their index, the first one is index 1
    private static Dictionary<int, string> CreateIds(params string[] ids)
    {
        Dictionary<int, string> idsByIndex = [];

        for (int i = 0; i < ids.Length; i++)
            idsByIndex.Add(i + 1, ids[i]);

        return idsByIndex;
    }
}
