namespace Heroes.StormData.Tests.Cache;

[TestClass]
internal sealed class LayeredDataStorageTests
{
    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstStormXmlConstElementById_ElementInSingleLayer_ReturnsElement(StormModType stormModType)
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";
        StormXmlConstElement expected = CreateStormXmlConstElement(constId, "1777");

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(stormModType).ElementCache.StormXmlConstElementById.Add(constId, expected);

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

        // assert
        result.Should().BeTrue();
        constElement.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_NoLayersPopulated_ReturnsFalse()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById("$Id", out StormXmlConstElement? constElement);

        // assert
        result.Should().BeFalse();
        constElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_IdNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add("$BaseConst", CreateStormXmlConstElement("$BaseConst"));
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.StormXmlConstElementById.Add("$MapConst", CreateStormXmlConstElement("$MapConst"));
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.StormXmlConstElementById.Add("$CustomConst", CreateStormXmlConstElement("$CustomConst"));

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById("$DoesNotExist", out StormXmlConstElement? constElement);

        // assert
        result.Should().BeFalse();
        constElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_IdWithDifferentCasing_ReturnsFalse()
    {
        // arrange - const ids are stored with StringComparer.Ordinal, so lookups are case-sensitive
        const string constId = "$ZagaraHunterKillerDamage";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add(constId, CreateStormXmlConstElement(constId));

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById(constId.ToUpperInvariant(), out StormXmlConstElement? constElement);

        // assert
        result.Should().BeFalse();
        constElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_InBaseAndMap_ReturnsMapElement()
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";

        StormXmlConstElement baseElement = CreateStormXmlConstElement(constId, "1");
        StormXmlConstElement mapElement = CreateStormXmlConstElement(constId, "2");

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add(constId, baseElement);
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.StormXmlConstElementById.Add(constId, mapElement);

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

        // assert
        result.Should().BeTrue();
        constElement.Should().BeSameAs(mapElement);
        constElement.Should().NotBeSameAs(baseElement);
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_InBaseAndCustom_ReturnsCustomElement()
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";

        StormXmlConstElement baseElement = CreateStormXmlConstElement(constId, "1");
        StormXmlConstElement customElement = CreateStormXmlConstElement(constId, "3");

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add(constId, baseElement);
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.StormXmlConstElementById.Add(constId, customElement);

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

        // assert
        result.Should().BeTrue();
        constElement.Should().BeSameAs(customElement);
        constElement.Should().NotBeSameAs(baseElement);
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_InAllLayers_ReturnsCustomElement()
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";

        StormXmlConstElement baseElement = CreateStormXmlConstElement(constId, "1");
        StormXmlConstElement mapElement = CreateStormXmlConstElement(constId, "2");
        StormXmlConstElement customElement = CreateStormXmlConstElement(constId, "3");

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add(constId, baseElement);
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.StormXmlConstElementById.Add(constId, mapElement);
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.StormXmlConstElementById.Add(constId, customElement);

        // act
        bool result = layeredDataStorage.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

        // assert
        result.Should().BeTrue();
        constElement.Should().BeSameAs(customElement);
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_DifferentIdsInDifferentLayers_ReturnsEachFromItsLayer()
    {
        // arrange
        StormXmlConstElement baseElement = CreateStormXmlConstElement("$BaseConst");
        StormXmlConstElement mapElement = CreateStormXmlConstElement("$MapConst");
        StormXmlConstElement customElement = CreateStormXmlConstElement("$CustomConst");

        LayeredDataStorage layeredDataStorage = new();

        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Add("$BaseConst", baseElement);
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.StormXmlConstElementById.Add("$MapConst", mapElement);
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.StormXmlConstElementById.Add("$CustomConst", customElement);

        // act
        bool baseResult = layeredDataStorage.TryGetFirstStormXmlConstElementById("$BaseConst", out StormXmlConstElement? foundBase);
        bool mapResult = layeredDataStorage.TryGetFirstStormXmlConstElementById("$MapConst", out StormXmlConstElement? foundMap);
        bool customResult = layeredDataStorage.TryGetFirstStormXmlConstElementById("$CustomConst", out StormXmlConstElement? foundCustom);

        // assert
        baseResult.Should().BeTrue();
        mapResult.Should().BeTrue();
        customResult.Should().BeTrue();
        foundBase.Should().BeSameAs(baseElement);
        foundMap.Should().BeSameAs(mapElement);
        foundCustom.Should().BeSameAs(customElement);
    }

    private static StormXmlConstElement CreateStormXmlConstElement(string id, string value = "1")
    {
        return XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />""");
    }
}
