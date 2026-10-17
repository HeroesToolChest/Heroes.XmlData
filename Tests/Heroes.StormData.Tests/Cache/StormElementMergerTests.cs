namespace Heroes.StormData.Cache.Tests;

[TestClass]
public class StormElementMergerTests
{
    [TestMethod]
    public void GetMerged_NoElements_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        StormElementMerger stormElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormElementMerger.GetMerged("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetMerged_HasNoParentId_ReturnsMergedElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Add("Effect", "Effect");
        dataStorage.ElementCache.CatalogByClassElement.Add("EffectApplyBehavior", "Effect");
        dataStorage.ElementCache.StormXmlElementByClassElement.Add("Effect", XmlReaderHelper.GetStormXmlElement(
            """
            <CEffect default="1">
              <Chance value="1" />
            </CEffect>
            """,
            layeredDataStorage));
        dataStorage.ElementCache.StormXmlElementByClassElement.Add("EffectApplyBehavior", XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectApplyBehavior default="1">
              <Behavior value="##id##" />
              <ValidatorArray value="##id##TargetFilters" />
            </CEffectApplyBehavior>
            """,
            layeredDataStorage));

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "ZagaraInfestApplyBuffBehavior",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CEffectApplyBehavior id="ZagaraInfestApplyBuffBehavior">
                      <ValidatorArray index="0" value="IsRangedMinion" />
                    </CEffectApplyBehavior>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByCatalog.Add("Effect", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormElementMerger stormElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormElementMerger.GetMerged("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("EffectApplyBehavior");
        result.StormFiles.Should().HaveCount(3);
        result.Members["Behavior"].Value.GetString().Should().Be("ZagaraInfestApplyBuffBehavior");
    }

    [TestMethod]
    public void GetMerged_BaseElementSameAsElementNoId_ShouldNotHaveDuplicate()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Add("Unit", "Unit");

        StormXmlElement baseElement = XmlReaderHelper.GetStormXmlElement(
            """
            <CUnit default="1">
              <Name value="Unit/Name/##id##" />
            </CUnit>
            """,
            layeredDataStorage);

        baseElement.Merge(XmlReaderHelper.GetStormXmlElement(
            """
            <CUnit default="1">
              <TauntDoesntStopUnit index="Cheer" value="1" />
            </CUnit>
            """,
            layeredDataStorage));

        dataStorage.ElementCache.StormXmlElementByClassElement.Add("Unit", baseElement);

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "StormBasicHeroicUnit",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CUnit default="1" id="StormBasicHeroicUnit">
                      <FlagArray index="Unclickable" value="0" />
                    </CUnit>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByCatalog.Add("Unit", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormElementMerger stormElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormElementMerger.GetMerged("StormBasicHeroicUnit", "Unit");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("Unit");
        result.StormFiles.Should().HaveCount(3);
        result.Members["Name"].Value.GetString().Should().Be("Unit/Name/StormBasicHeroicUnit");
    }

    [TestMethod]
    public void GetMerged_BaseElementHasAParentIdThatIsSameAsId_ShouldReturnAllProperties()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Add("LootChest", "LootChest");

        StormXmlElement baseElement = XmlReaderHelper.GetStormXmlElement(
            """
            <CLootChest default="1" id="LootChestChristmas2018Common" parent="LootChestChristmas2018Parent">
              <Other value="10" />
              <MaxRerolls value="2" />
            </CLootChest>
            """,
            layeredDataStorage);

        baseElement.Merge(XmlReaderHelper.GetStormXmlElement(
            """
            <CLootChest id="LootChestChristmas2018Common" parent="LootChestChristmas2018Common">
              <MaxRerolls value="3" />
            </CLootChest>
            """,
            layeredDataStorage));

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "BaseLootChest",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CLootChest default="1" id="BaseLootChest">
                      <Name value="LootChest/Name/##id##" />
                      <HyperlinkId value="##id##" />
                      <MaxRerolls value="5" />
                      <TypeDescription value="LootChestCommon" />
                    </CLootChest>
                    """,
                    layeredDataStorage)
            },
            {
                "LootChestChristmas2018Parent",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CLootChest default="1" id="LootChestChristmas2018Parent" parent="BaseLootChest">
                      <TypeDescription value="LootChestChristmas2018Common" />
                      <EventName value="Toys18" />
                    </CLootChest>
                    """,
                    layeredDataStorage)
            },
            {
                "LootChestChristmas2018Common", baseElement
            },
        };
        dataStorage.ElementCache.StormXmlElementsByCatalog.Add("LootChest", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormElementMerger stormElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormElementMerger.GetMerged("LootChestChristmas2018Common", "LootChest");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("LootChest");
        result.StormFiles.Should().HaveCount(4);
        result.Members["Name"].Value.GetString().Should().Be("LootChest/Name/LootChestChristmas2018Common");
        result.Members["id"].Value.GetString().Should().Be("LootChestChristmas2018Common");
        result.Id.Should().Be("LootChestChristmas2018Common");
        result.Parent.Should().Be("LootChestChristmas2018Parent");
        result.Members["parent"].Value.GetString().Should().Be("LootChestChristmas2018Parent");
        result.Members["MaxRerolls"].Value.GetInt32().Should().Be(3);
        result.Members["EventName"].Value.GetString().Should().Be("Toys18");
    }
}
