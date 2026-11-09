namespace Heroes.StormData.Cache.Elements.Tests;

[TestClass]
public class StormXmlElementMergerTests
{
    [TestMethod]
    public void GetMerged_NoElements_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetMerged_HasNoParentId_ReturnsMergedElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("Effect", "Effect");
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("EffectApplyBehavior", "Effect");
        dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary.Add("Effect", XmlReaderHelper.GetStormXmlElement(
            """
            <CEffect default="1">
              <Chance value="1" />
            </CEffect>
            """,
            layeredDataStorage));
        dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary.Add("EffectApplyBehavior", XmlReaderHelper.GetStormXmlElement(
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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("Effect", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("EffectApplyBehavior");
        result.StormFiles.Should().HaveCount(3);
        result.Members["Behavior"].Value.GetString().Should().Be("ZagaraInfestApplyBuffBehavior");
    }

    [TestMethod]
    public void GetMerged_OnlyParentElement_ReturnsStormXmlElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("EffectModifyTokenCount", "Effect");

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "BaseEffectModifyTokenCount",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CEffectModifyTokenCount default="1" id="BaseEffectModifyTokenCount">
                      <Value value="1" />
                    </CEffectModifyTokenCount>
                    """,
                    layeredDataStorage)
            },
            {
                "KelThuzadMasterOfTheColdDarkModifyToken",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CEffectModifyTokenCount id="KelThuzadMasterOfTheColdDarkModifyToken" parent="BaseEffectModifyTokenCount">
                      <ValidatorArray value="TargetIsHero" />
                    </CEffectModifyTokenCount>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("Effect", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("EffectModifyTokenCount");
        result.StormFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetMerged_HasParentIdButNotFound_ReturnsStormXmlElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("EffectModifyTokenCount", "Effect");

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "KelThuzadMasterOfTheColdDarkModifyToken",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CEffectModifyTokenCount id="KelThuzadMasterOfTheColdDarkModifyToken" parent="BaseEffectModifyTokenCount">
                      <ValidatorArray value="TargetIsHero" />
                    </CEffectModifyTokenCount>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("Effect", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("EffectModifyTokenCount");
        result.StormFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void GetMerged_BaseElementSameAsElementNoId_ShouldNotHaveDuplicate()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("Unit", "Unit");

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

        dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary.Add("Unit", baseElement);

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("Unit", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("StormBasicHeroicUnit", "Unit");

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
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("LootChest", "LootChest");

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("LootChest", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("LootChestChristmas2018Common", "LootChest");

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

    [TestMethod]
    public void GetMerged_HasProcessingInstructions_ShouldReturnCorrectValues()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("ActorRange", "ActorRange");

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "RangeAbil",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CActorRange default="1" id="RangeAbil">
                      <?token id="abil" type="CAbilLink"?>
                      <Abil Link="##abil##" />
                      <On Terms="Abil.##abil##.TargetOn" Send="Create" />
                      <On Terms="Abil.##abil##.TargetOff" Send="Destroy" />
                      <On Terms="Abil.##abil##.ButtonHoverOn" Send="Create" />
                      <On Terms="Abil.##abil##.ButtonHoverOff" Send="Destroy" />
                      <Flags index="SuppressSaveLoad" value="1" />
                    </CActorRange>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("ActorRange", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("RangeAbil", "ActorRange");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("ActorRange");
        result.Members["Abil"].ByMember["Link"].Value.GetString().Should().BeEmpty();
        result.Members["On"].ByIndex["0"]["Terms"].Value.GetString().Should().Be("Abil..TargetOn");
    }

    [TestMethod]
    public void GetMerged_HasProcessingInstructionsWithReplacement_ShouldReturnCorrectValues()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Add("VoiceLine", "VoiceLine");

        dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary.Add("VoiceLine", XmlReaderHelper.GetStormXmlElement(
            """
            <CVoiceLine default="1">
              <Name value="VoiceLine/Name/##id##" />
              <SortName value="VoiceLine/SortName/##id##" />
              <Description value="VoiceLine/Description/##id##" />
              <ReleaseDate>
                <Month value="1" />
                <Day value="1" />
                <Year value="2014" />
              </ReleaseDate>
              <AttributeId value="TODO" />
              <ProductId value="11089" />
              <LootChestRewardCutsceneFile value="Cutscenes/UI_LootChest_Reward_BG.StormCutscene" />
              <TileCutsceneFile value="Cutscenes/UI_LootChest_Reward_BG.StormCutscene" />
            </CVoiceLine>
            """,
            layeredDataStorage));

        Dictionary<string, StormXmlElement> newStormXmlElementById = new(StringComparer.Ordinal)
        {
            {
                "StormVoiceLineCommonBase",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CVoiceLine default="1" id="StormVoiceLineCommonBase">
                      <?token id="heroid" type="CHeroLink" value="Bogus"?>
                      <Hero value="##heroid##" />
                      <TileTexture value="Assets\Textures\Storm_UI_Voice_##heroid##.dds" />
                    </CVoiceLine>
                    """,
                    layeredDataStorage)
            },
            {
                "StormVoiceLine01Common",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CVoiceLine default="1" id="StormVoiceLine01Common" parent="StormVoiceLineCommonBase">
                      <Flags index="FreePlay" value="1" />
                      <HyperlinkId value="##heroid##VoiceLine01" />
                      <ProductId value="0" />
                    </CVoiceLine>
                    """,
                    layeredDataStorage)
            },
            {
                "Abathur_VoiceLine01Common",
                XmlReaderHelper.GetStormXmlElement(
                    """
                    <CVoiceLine default="1" id="Abathur_VoiceLine01Common" parent="StormVoiceLine01Common">
                      <?token id="heroid" type="CHeroLink" value="Abathur"?>
                      <AttributeId value="AB01" />
                      <Sound value="AbathurHero_VoiceLineOne" />
                    </CVoiceLine>
                    """,
                    layeredDataStorage)
            },
        };
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Add("VoiceLine", newStormXmlElementById.GetAlternateLookup<ReadOnlySpan<char>>());

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? result = stormXmlElementMerger.GetMerged("Abathur_VoiceLine01Common", "VoiceLine");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("VoiceLine");
        result.Members["HyperlinkId"].Value.GetString().Should().Be("AbathurVoiceLine01");
        result.Members["Hero"].Value.GetString().Should().Be("Abathur");
        result.Members["TileTexture"].Value.GetString().Should().Be("Assets\\Textures\\Storm_UI_Voice_Abathur.dds");
        result.Members["ReleaseDate"].ByMember["Month"].Value.GetInt32().Should().Be(1);
        result.Members["Description"].Value.GetString().Should().Be("VoiceLine/Description/Abathur_VoiceLine01Common");
    }

    [TestMethod]
    public void GetMerged_ChildMergedIntoParent_DoesNotModifyStoredParent()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectEnumArea id="ParentSearch">
                  <AreaArray>
                    <RectangleWidth value="1" />
                  </AreaArray>
                </CEffectEnumArea>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectEnumArea id="ChildSearch" parent="ParentSearch">
                  <AreaArray index="0">
                    <RectangleWidth value="1.35" />
                  </AreaArray>
                </CEffectEnumArea>
                """),
            "Effect");

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? child = stormXmlElementMerger.GetMerged("ChildSearch", "Effect");
        StormXmlElement? parent = stormXmlElementMerger.GetMerged("ParentSearch", "Effect");

        // assert
        child.Should().NotBeNull();
        parent.Should().NotBeNull();
        child.Members["AreaArray"].ByIndex[0]["RectangleWidth"].Value.GetString().Should().Be("1.35");
        parent.Members["AreaArray"].ByIndex[0]["RectangleWidth"].Value.GetString().Should().Be("1");
    }

    [TestMethod]
    public void GetMerged_ParentInMultipleLayers_DoesNotModifyStoredLayerElements()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        StormXmlElement baseParent = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectEnumArea id="ParentSearch">
              <AreaArray>
                <RectangleWidth value="1" />
                <RectangleHeight value="6" />
              </AreaArray>
            </CEffectEnumArea>
            """);
        StormXmlElement customParent = XmlReaderHelper.GetStormXmlElement(
            """
            <CEffectEnumArea id="ParentSearch">
              <AreaArray index="0">
                <RectangleHeight value="7" />
              </AreaArray>
            </CEffectEnumArea>
            """);

        layeredDataStorage.GetDataStorage(StormModType.Base).AddElementWithId(baseParent, "Effect");
        layeredDataStorage.GetDataStorage(StormModType.Custom).AddElementWithId(customParent, "Effect");
        layeredDataStorage.GetDataStorage(StormModType.Custom).AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectEnumArea id="ChildSearch" parent="ParentSearch">
                  <AreaArray index="0">
                    <RectangleWidth value="1.35" />
                  </AreaArray>
                </CEffectEnumArea>
                """),
            "Effect");

        StormXmlElementMerger stormXmlElementMerger = new(layeredDataStorage);

        // act
        StormXmlElement? child = stormXmlElementMerger.GetMerged("ChildSearch", "Effect");

        // assert
        child.Should().NotBeNull();
        child.Members["AreaArray"].ByIndex[0]["RectangleWidth"].Value.GetString().Should().Be("1.35");
        child.Members["AreaArray"].ByIndex[0]["RectangleHeight"].Value.GetString().Should().Be("7");

        baseParent.Members["AreaArray"].ByIndex[0]["RectangleWidth"].Value.GetString().Should().Be("1");
        baseParent.Members["AreaArray"].ByIndex[0]["RectangleHeight"].Value.GetString().Should().Be("6");
        customParent.Members["AreaArray"].ByIndex[0].ContainsMember("RectangleWidth").Should().BeFalse();
        customParent.Members["AreaArray"].ByIndex[0]["RectangleHeight"].Value.GetString().Should().Be("7");
    }
}
