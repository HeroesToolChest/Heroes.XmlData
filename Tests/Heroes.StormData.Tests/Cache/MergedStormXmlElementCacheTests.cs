namespace Heroes.StormData.Tests.Cache;

[TestClass]
public class MergedStormXmlElementCacheTests
{
    [TestMethod]
    public void GetOrMerge_ElementHasParent_ReturnsMergedElement()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));

        // act
        StormXmlElement? result = cache.GetOrMerge("ChildDamage", "Effect", 0);

        // assert
        result.Should().NotBeNull();
        result.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
        result.Members.GetMember("Kind").Value.RawValue.Should().Be("Spell");
    }

    [TestMethod]
    public void GetOrMerge_SameVersion_ReturnsSameInstance()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));

        // act
        StormXmlElement? first = cache.GetOrMerge("ChildDamage", "Effect", 3);
        StormXmlElement? second = cache.GetOrMerge("ChildDamage", "Effect", 3);

        // assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [TestMethod]
    public void GetOrMerge_VersionChanged_ReturnsNewlyMergedInstance()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));

        // act
        StormXmlElement? first = cache.GetOrMerge("ChildDamage", "Effect", 3);
        StormXmlElement? second = cache.GetOrMerge("ChildDamage", "Effect", 4);

        // assert
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        second.Should().NotBeSameAs(first);
    }

    [TestMethod]
    public void GetOrMerge_VersionChanged_ClearsOtherCachedElements()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));
        StormXmlElement? before = cache.GetOrMerge("ChildDamage", "Effect", 3);

        // act
        cache.GetOrMerge("ParentDamage", "Effect", 4);
        StormXmlElement? after = cache.GetOrMerge("ChildDamage", "Effect", 4);

        // assert
        after.Should().NotBeSameAs(before);
    }

    [TestMethod]
    public void GetOrMerge_DataAddedButSameVersion_ReturnsCachedElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateStorage();
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(layeredDataStorage));
        StormXmlElement? before = cache.GetOrMerge("ChildDamage", "Effect", 0);

        layeredDataStorage.GetDataStorage(StormModType.Custom).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ChildDamage">
                  <Amount value="150" />
                </CEffectDamage>
                """),
            "Effect");

        // act
        StormXmlElement? after = cache.GetOrMerge("ChildDamage", "Effect", 0);

        // assert
        after.Should().BeSameAs(before);
        after.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
    }

    [TestMethod]
    public void GetOrMerge_NotFoundThenAddedWithNewVersion_ReturnsElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateStorage();
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(layeredDataStorage));
        StormXmlElement? before = cache.GetOrMerge("NewDamage", "Effect", 0);

        layeredDataStorage.GetDataStorage(StormModType.Custom).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="NewDamage">
                  <Amount value="5" />
                </CEffectDamage>
                """),
            "Effect");

        // act
        StormXmlElement? sameVersion = cache.GetOrMerge("NewDamage", "Effect", 0);
        StormXmlElement? newVersion = cache.GetOrMerge("NewDamage", "Effect", 1);

        // assert
        before.Should().BeNull();
        sameVersion.Should().BeNull();
        newVersion.Should().NotBeNull();
    }

    [TestMethod]
    public void GetOrMerge_CatalogNameWithDifferentCasing_ReturnsSameInstance()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));

        // act
        StormXmlElement? first = cache.GetOrMerge("ChildDamage", "Effect", 0);
        StormXmlElement? second = cache.GetOrMerge("ChildDamage", "EFFECT", 0);

        // assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [TestMethod]
    public void GetOrMerge_IdWithDifferentCasing_ReturnsNull()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));
        cache.GetOrMerge("ChildDamage", "Effect", 0);

        // act
        StormXmlElement? result = cache.GetOrMerge("CHILDDAMAGE", "Effect", 0);

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void Clear_AfterCaching_ReturnsNewlyMergedInstance()
    {
        // arrange
        MergedStormXmlElementCache cache = new(new StormXmlElementMerger(CreateStorage()));
        StormXmlElement? before = cache.GetOrMerge("ChildDamage", "Effect", 0);

        // act
        cache.Clear();
        StormXmlElement? after = cache.GetOrMerge("ChildDamage", "Effect", 0);

        // assert
        after.Should().NotBeSameAs(before);
        after.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
    }

    // Effect,ChildDamage (Amount 120) with parent Effect,ParentDamage (Kind Spell)
    private static LayeredDataStorage CreateStorage()
    {
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Base);

        dataStorage.AddBaseClassCatalogName(XmlReaderHelper.GetStormXmlElement("<CEffectDamage />"), "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage default="1" id="ParentDamage">
                  <Kind value="Spell" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ChildDamage" parent="ParentDamage">
                  <Amount value="120" />
                </CEffectDamage>
                """),
            "Effect");

        return layeredDataStorage;
    }
}
