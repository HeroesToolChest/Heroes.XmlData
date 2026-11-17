namespace Heroes.StormData.Cache.Tests;

[TestClass]
internal sealed class LayeredDataStorageTests
{
    [TestMethod]
    public void GetMergedStormXmlElementById_ElementHasParent_ReturnsMergedElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();

        // act
        StormXmlElement? result = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // assert
        result.Should().NotBeNull();
        result.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
        result.Members.GetMember("Kind").Value.RawValue.Should().Be("Spell");
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_CalledTwice_ReturnsSameInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();

        // act
        StormXmlElement? first = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");
        StormXmlElement? second = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_CatalogNameWithDifferentCasing_ReturnsSameInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();

        // act
        StormXmlElement? first = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");
        StormXmlElement? second = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "effect");

        // assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_IdWithDifferentCasing_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();

        // act
        StormXmlElement? result = layeredDataStorage.GetMergedStormXmlElementById("childdamage", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_IdNotFound_ReturnsNullEachCall()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();

        // act
        StormXmlElement? first = layeredDataStorage.GetMergedStormXmlElementById("UnknownDamage", "Effect");
        StormXmlElement? second = layeredDataStorage.GetMergedStormXmlElementById("UnknownDamage", "Effect");

        // assert
        first.Should().BeNull();
        second.Should().BeNull();
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_IdAndCatalogNameAsSpanSlices_ReturnsSameInstanceAsStrings()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        ReadOnlySpan<char> dRef = "Effect,ChildDamage,Amount";

        // act
        StormXmlElement? fromStrings = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");
        StormXmlElement? fromSlices = layeredDataStorage.GetMergedStormXmlElementById(dRef[7..18], dRef[..6]);

        // assert
        fromStrings.Should().NotBeNull();
        fromSlices.Should().BeSameAs(fromStrings);
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_ElementAddedAfterCaching_ReturnsUpdatedElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        StormXmlElement? before = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        layeredDataStorage.GetDataStorage(StormModType.Custom).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="ChildDamage">
                  <Amount value="150" />
                </CEffectDamage>
                """),
            "Effect");

        // act
        StormXmlElement? after = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // assert
        before!.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
        after.Should().NotBeSameAs(before);
        after!.Members.GetMember("Amount").Value.RawValue.Should().Be("150");
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_ParentAddedAfterCaching_ReturnsElementWithParentValues()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        StormXmlElement? before = layeredDataStorage.GetMergedStormXmlElementById("OrphanDamage", "Effect");

        layeredDataStorage.GetDataStorage(StormModType.Base).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage default="1" id="LateParentDamage">
                  <Death value="Fire" />
                </CEffectDamage>
                """),
            "Effect");

        // act
        StormXmlElement? after = layeredDataStorage.GetMergedStormXmlElementById("OrphanDamage", "Effect");

        // assert
        before!.Members.TryGetMember("Death", out _).Should().BeFalse();
        after!.Members.GetMember("Death").Value.RawValue.Should().Be("Fire");
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_NotFoundThenAdded_ReturnsElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        StormXmlElement? before = layeredDataStorage.GetMergedStormXmlElementById("NewDamage", "Effect");

        layeredDataStorage.GetDataStorage(StormModType.Map).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="NewDamage">
                  <Amount value="5" />
                </CEffectDamage>
                """),
            "Effect");

        // act
        StormXmlElement? after = layeredDataStorage.GetMergedStormXmlElementById("NewDamage", "Effect");

        // assert
        before.Should().BeNull();
        after.Should().NotBeNull();
        after.Members.GetMember("Amount").Value.RawValue.Should().Be("5");
    }

    [TestMethod]
    public void GetMergedStormXmlElementById_ConstAddedAfterCaching_ReturnsSameInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        StormXmlElement? before = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        layeredDataStorage.GetDataStorage(StormModType.Custom).AddConstElement(CreateStormXmlConstElement("$Const"));

        // act
        StormXmlElement? after = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // assert
        after.Should().BeSameAs(before);
    }

    [TestMethod]
    public void ClearMergedStormXmlElementCache_AfterCaching_ReturnsNewInstanceWithSameValues()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateMergeableStorage();
        StormXmlElement? before = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // act
        layeredDataStorage.ClearMergedStormXmlElementCache();
        StormXmlElement? after = layeredDataStorage.GetMergedStormXmlElementById("ChildDamage", "Effect");

        // assert
        after.Should().NotBeSameAs(before);
        after!.Members.GetMember("Amount").Value.RawValue.Should().Be("120");
        after.Members.GetMember("Kind").Value.RawValue.Should().Be("Spell");
    }

    // Effect,ChildDamage (Amount 120) with parent Effect,ParentDamage (Kind Spell), and Effect,OrphanDamage whose parent doesn't exist yet
    private static LayeredDataStorage CreateMergeableStorage()
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
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="OrphanDamage" parent="LateParentDamage">
                  <Amount value="10" />
                </CEffectDamage>
                """),
            "Effect");

        return layeredDataStorage;
    }

    private static StormXmlConstElement CreateStormXmlConstElement(string id, string value = "1")
    {
        return XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />""");
    }
}
