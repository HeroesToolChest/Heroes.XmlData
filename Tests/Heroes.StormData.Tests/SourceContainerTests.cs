namespace Heroes.StormData.Tests;

[TestClass]
public sealed class SourceContainerTests
{
    private readonly SourceContainer _sourceContainer = new();

    [TestMethod]
    public void ClearStormMapMods_HasBaseMapAndCustomContainers_RemovesOnlyMapContainers()
    {
        // arrange
        StormModContainer baseContainer = AddContainer(StormModType.Base);
        AddContainer(StormModType.Map);
        AddContainer(StormModType.Map);
        StormModContainer customContainer = AddContainer(StormModType.Custom);

        // act
        _sourceContainer.ClearStormMapMods();

        // assert
        _sourceContainer.StormModContainers.Should().Equal(baseContainer, customContainer);
    }

    [TestMethod]
    public void ClearStormMapMods_ThenBaseContainerAdded_AddsBaseContainerAtEnd()
    {
        // arrange
        StormModContainer baseContainer = AddContainer(StormModType.Base);
        AddContainer(StormModType.Map);
        _sourceContainer.ClearStormMapMods();

        // act
        StormModContainer newBaseContainer = AddContainer(StormModType.Base);

        // assert
        _sourceContainer.StormModContainers.Should().Equal(baseContainer, newBaseContainer);
    }

    [TestMethod]
    public void ClearStormMapMods_ThenMapContainerAdded_AddsMapContainerBeforeCustomContainers()
    {
        // arrange
        StormModContainer baseContainer = AddContainer(StormModType.Base);
        AddContainer(StormModType.Map);
        StormModContainer customContainer = AddContainer(StormModType.Custom);
        _sourceContainer.ClearStormMapMods();

        // act
        StormModContainer newMapContainer = AddContainer(StormModType.Map);

        // assert
        _sourceContainer.StormModContainers.Should().Equal(baseContainer, newMapContainer, customContainer);
    }

    [TestMethod]
    public void ClearStormMapMods_HasDataInEveryLayer_RemovesMapDataOnly()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = _sourceContainer.LayeredDataStorage;
        int baseElementCount = layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlElementByClassElement.Dictionary.Count;

        layeredDataStorage.GetDataStorage(StormModType.Base).AddConstElement(CreateStormXmlConstElement("$Base"));
        layeredDataStorage.GetDataStorage(StormModType.Map).AddConstElement(CreateStormXmlConstElement("$Map"));
        layeredDataStorage.GetDataStorage(StormModType.Custom).AddConstElement(CreateStormXmlConstElement("$Custom"));

        // act
        _sourceContainer.ClearStormMapMods();

        // assert
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.StormXmlConstElementById.Dictionary.Should().BeEmpty();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().Equal("$Base");
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.StormXmlConstElementById.Dictionary.Keys.Should().Equal("$Custom");

        // the default elements are in the base layer and stay
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.StormXmlElementByClassElement.Dictionary.Should().HaveCount(baseElementCount);
    }

    [TestMethod]
    public void ClearStormMapMods_NoMapContainers_KeepsOtherContainers()
    {
        // arrange
        StormModContainer baseContainer = AddContainer(StormModType.Base);
        StormModContainer customContainer = AddContainer(StormModType.Custom);

        // act
        _sourceContainer.ClearStormMapMods();

        // assert
        _sourceContainer.StormModContainers.Should().Equal(baseContainer, customContainer);
    }

    [TestMethod]
    public void ClearGamestrings_HasTextsInEveryLayer_RemovesOnlyGameStrings()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = _sourceContainer.LayeredDataStorage;
        StormFile stormFile = StormFileHelper.CreateStormFile();

        foreach (StormModType stormModType in new[] { StormModType.Base, StormModType.Map, StormModType.Custom })
        {
            TextCache textCache = layeredDataStorage.GetDataStorage(stormModType).TextCache;
            textCache.GameStringsById.Dictionary[$"{stormModType}GameString"] = new StringValue("gamestring", stormFile);
            textCache.AssetTextsById.Dictionary[$"{stormModType}AssetText"] = new StringValue("asset", stormFile);
        }

        // act
        _sourceContainer.ClearGamestrings();

        // assert
        foreach (StormModType stormModType in new[] { StormModType.Base, StormModType.Map, StormModType.Custom })
        {
            TextCache textCache = layeredDataStorage.GetDataStorage(stormModType).TextCache;
            textCache.GameStringsById.Dictionary.Should().BeEmpty();
            textCache.AssetTextsById.Dictionary.Keys.Should().Equal($"{stormModType}AssetText");
        }
    }

    [TestMethod]
    public void ClearGamestrings_HasContainers_KeepsContainers()
    {
        // arrange
        StormModContainer baseContainer = AddContainer(StormModType.Base);
        StormModContainer mapContainer = AddContainer(StormModType.Map);
        StormModContainer customContainer = AddContainer(StormModType.Custom);

        // act
        _sourceContainer.ClearGamestrings();

        // assert
        _sourceContainer.StormModContainers.Should().Equal(baseContainer, mapContainer, customContainer);
        _sourceContainer.StormModContainers.Should().AllSatisfy(x => x.LoadedGameStringFiles.Should().BeEmpty());
    }

    private static StormXmlConstElement CreateStormXmlConstElement(string id) =>
        XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="1" />""");

    private StormModContainer AddContainer(StormModType stormModType)
    {
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        _sourceContainer.AddModContainer(stormModContainer);

        return stormModContainer;
    }
}
