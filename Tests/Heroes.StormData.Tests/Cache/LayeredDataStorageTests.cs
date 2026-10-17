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

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstCatalogNameByElementName_ElementInSingleLayer_ReturnsCatalogName(StormModType stormModType)
    {
        // arrange
        const string elementName = "EffectDamage";
        const string catalogName = "Effect";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(stormModType).ElementCache.CatalogByClassElement.Add(elementName, catalogName);

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? foundCatalogName);

        // assert
        result.Should().BeTrue();
        foundCatalogName.Should().Be(catalogName);
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_ElementNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add("EffectDamage", "Effect");
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.CatalogByClassElement.Add("AbilTargetedHeal", "Abil");
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.CatalogByClassElement.Add("BehaviorBuff", "Behavior");

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName("DoesNotExist", out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_EmptyElementName_ReturnsFalse()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add("EffectDamage", "Effect");

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(string.Empty, out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_ElementNameWithDifferentCasing_ReturnsCatalogName()
    {
        // arrange - class element names are stored with StringComparer.OrdinalIgnoreCase, so lookups are case-insensitive
        const string elementName = "EffectDamage";
        const string catalogName = "Effect";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add(elementName, catalogName);

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName.ToUpperInvariant(), out string? foundCatalogName);

        // assert
        result.Should().BeTrue();
        foundCatalogName.Should().Be(catalogName);
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_ElementNameAsSpanSlice_ReturnsCatalogName()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.CatalogByClassElement.Add("EffectDamage", "Effect");

        ReadOnlySpan<char> elementName = "xxEffectDamageyy".AsSpan(2, 12);

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InBaseAndMap_ReturnsMapCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add(elementName, "BaseCatalog");
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.CatalogByClassElement.Add(elementName, "MapCatalog");

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("MapCatalog");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InBaseAndCustom_ReturnsCustomCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add(elementName, "BaseCatalog");
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.CatalogByClassElement.Add(elementName, "CustomCatalog");

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("CustomCatalog");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InAllLayers_ReturnsCustomCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        LayeredDataStorage layeredDataStorage = new();
        layeredDataStorage.GetDataStorage(StormModType.Base).ElementCache.CatalogByClassElement.Add(elementName, "BaseCatalog");
        layeredDataStorage.GetDataStorage(StormModType.Map).ElementCache.CatalogByClassElement.Add(elementName, "MapCatalog");
        layeredDataStorage.GetDataStorage(StormModType.Custom).ElementCache.CatalogByClassElement.Add(elementName, "CustomCatalog");

        // act
        bool result = layeredDataStorage.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("CustomCatalog");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_NoLayersHaveElement_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyBaseHasElement_ReturnsCachedInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(layeredDataStorage, StormModType.Base, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeSameAs(baseElement);
        result.Members["Name"].Value.RawValue.Should().Be("Base");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyMapHasElement_ReturnsCachedInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Map" />
            </CAbil>
            """;
        StormXmlElement mapElement = AddStormXmlElement(layeredDataStorage, StormModType.Map, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeSameAs(mapElement);
        result.Members["Name"].Value.RawValue.Should().Be("Map");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyCustomHasElement_ReturnsCachedInstance()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(layeredDataStorage, StormModType.Custom, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeSameAs(customElement);
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
    }

    [TestMethod]
    [DataRow("Abil")]
    [DataRow("abil")]
    [DataRow("ABIL")]
    public void GetStormXmlElementByClassElement_DifferentCase_FindsElement(string elementName)
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(layeredDataStorage, StormModType.Base, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement(elementName);

        // assert
        result.Should().BeSameAs(baseElement);
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_NameWithTypeLetterDeclaration_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("CAbil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_SliceOfLargerString_FindsElement()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(layeredDataStorage, StormModType.Base, xml);

        const string text = "xxAbilyy";

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement(text.AsSpan(2, 4));

        // assert
        result.Should().BeSameAs(baseElement);
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_DifferentClassElementInLayer_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string xml =
            """
            <CEffectDamage>
              <Amount value="10" />
            </CEffectDamage>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, xml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_ElementWithIdOnly_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        StormXmlElement elementWithId = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestAbil">
              <Name value="Base" />
            </CAbil>
            """);

        layeredDataStorage.GetDataStorage(StormModType.Base).AddElementWithId(elementWithId, "Abil");

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_BaseAndMap_MapOverridesBase()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
              <TechPlayer value="Upkeep" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Map, mapXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.ElementName.Should().Be("Abil");
        result.Members["Name"].Value.RawValue.Should().Be("Map");
        result.Members["TechPlayer"].Value.RawValue.Should().Be("Upkeep");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_BaseAndCustom_CustomOverridesBase()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_MapAndCustom_CustomOverridesMap()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="Upkeep" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
        result.Members["TechPlayer"].Value.RawValue.Should().Be("Upkeep");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_AllLayers_PrecedenceIsBaseThenMapThenCustom()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
              <TechPlayer value="BaseTech" />
              <TargetMessage value="BaseMessage" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="MapTech" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
        result.Members["TechPlayer"].Value.RawValue.Should().Be("MapTech");
        result.Members["TargetMessage"].Value.RawValue.Should().Be("BaseMessage");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_MultipleLayers_ReturnsCopyAndDoesNotModifyCachedElements()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="Map" />
            </CAbil>
            """;
        StormXmlElement mapElement = AddStormXmlElement(layeredDataStorage, StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseElement);
        result.Should().NotBeSameAs(mapElement);
        result.Should().NotBeSameAs(customElement);

        baseElement.Members["Name"].Value.RawValue.Should().Be("Base");
        baseElement.Members.ContainsMember("TechPlayer").Should().BeFalse();
        mapElement.Members["Name"].Value.RawValue.Should().Be("Map");
        mapElement.Members["TechPlayer"].Value.RawValue.Should().Be("Map");
        customElement.Members["Name"].Value.RawValue.Should().Be("Custom");
        customElement.Members.ContainsMember("TechPlayer").Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_MultipleLayers_ReturnsNewInstanceEachCall()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? first = layeredDataStorage.GetStormXmlElementByClassElement("Abil");
        StormXmlElement? second = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        first.Should().NotBeSameAs(second);
        first.Members["Name"].Value.RawValue.Should().Be("Custom");
        second.Members["Name"].Value.RawValue.Should().Be("Custom");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_MultipleLayers_StormFilesFromAllLayersAreIncluded()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.StormFiles.Should().HaveCount(2);
        baseElement.StormFiles.Should().ContainSingle();
        customElement.StormFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_ArrayMembersInMultipleLayers_ArraysAreMerged()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        string baseXml =
            """
            <CAbil>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbil>
            """;
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, customXml);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["SharedFlags"].ByIndex["DisableWhileDead"].Value.RawValue.Should().Be("0");
        result.Members["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void GetStormXmlElementById_ElementInSingleLayer_ReturnsElement(StormModType stormModType)
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement expected = CreateStormXmlElement(id, "1777");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, stormModType, catalogName, id, expected);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormXmlElementById_NoLayersPopulated_ReturnsNull()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById("TestEffect", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_IdNotInAnyLayer_ReturnsNull()
    {
        // arrange
        const string catalogName = "Effect";

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, "BaseEffect", CreateStormXmlElement("BaseEffect"));
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, "MapEffect", CreateStormXmlElement("MapEffect"));
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, "CustomEffect", CreateStormXmlElement("CustomEffect"));

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById("DoesNotExist", catalogName);

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_CatalogNotInAnyLayer_ReturnsNull()
    {
        // arrange
        const string id = "TestEffect";

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, "Effect", id, CreateStormXmlElement(id));

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, "Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_SameIdInDifferentCatalog_ReturnsNull()
    {
        // arrange - the id exists in the Abil catalog only, so an Effect lookup must not find it
        const string id = "TestEffect";

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, "Abil", id, CreateStormXmlElement(id));

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_CatalogNameWithDifferentCasing_ReturnsElement()
    {
        // arrange - catalog names are stored with StringComparer.OrdinalIgnoreCase, so lookups are case-insensitive
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement expected = CreateStormXmlElement(id);

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, expected);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName.ToUpperInvariant());

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormXmlElementById_IdAndCatalogNameAsSpanSlices_ReturnsElement()
    {
        // arrange
        StormXmlElement expected = CreateStormXmlElement("TestEffect");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Map, "Effect", "TestEffect", expected);

        ReadOnlySpan<char> id = "xxTestEffectyy".AsSpan(2, 10);
        ReadOnlySpan<char> catalogName = "xxEffectyy".AsSpan(2, 6);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormXmlElementById_InBaseAndMap_ReturnsMergedCopy()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = CreateStormXmlElement(id, "1");
        StormXmlElement mapElement = CreateStormXmlElement(id, "2");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, mapElement);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseElement);
        result.Should().NotBeSameAs(mapElement);
        result.Members["Amount"].Value.RawValue.Should().Be("2");
        result.StormFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetStormXmlElementById_InBaseAndCustom_ReturnsMergedCopy()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = CreateStormXmlElement(id, "1");
        StormXmlElement customElement = CreateStormXmlElement(id, "3");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseElement);
        result.Should().NotBeSameAs(customElement);
        result!.Members["Amount"].Value.RawValue.Should().Be("3");
        result.StormFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetStormXmlElementById_InMapAndCustom_ReturnsMergedCopy()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement mapElement = CreateStormXmlElement(id, "2");
        StormXmlElement customElement = CreateStormXmlElement(id, "3");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(mapElement);
        result.Should().NotBeSameAs(customElement);
        result!.Members["Amount"].Value.RawValue.Should().Be("3");
        result.StormFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetStormXmlElementById_InAllLayers_ReturnsMergedCopy()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = CreateStormXmlElement(id, "1");
        StormXmlElement mapElement = CreateStormXmlElement(id, "2");
        StormXmlElement customElement = CreateStormXmlElement(id, "3");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseElement);
        result.Should().NotBeSameAs(mapElement);
        result.Should().NotBeSameAs(customElement);
        result!.Members["Amount"].Value.RawValue.Should().Be("3");
        result.StormFiles.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetStormXmlElementById_InAllLayers_DoesNotModifyCachedElements()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = CreateStormXmlElement(id, "1");
        StormXmlElement mapElement = CreateStormXmlElement(id, "2");
        StormXmlElement customElement = CreateStormXmlElement(id, "3");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, customElement);

        // act
        layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        baseElement.Members["Amount"].Value.RawValue.Should().Be("1");
        baseElement.StormFiles.Should().ContainSingle();
        mapElement.Members["Amount"].Value.RawValue.Should().Be("2");
        mapElement.StormFiles.Should().ContainSingle();
        customElement.Members["Amount"].Value.RawValue.Should().Be("3");
        customElement.StormFiles.Should().ContainSingle();
    }

    [TestMethod]
    public void GetStormXmlElementById_CalledRepeatedly_DoesNotAccumulateMergedData()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, CreateStormXmlElement(id, "1"));
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, CreateStormXmlElement(id, "2"));
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, CreateStormXmlElement(id, "3"));

        // act
        StormXmlElement? first = layeredDataStorage.GetStormXmlElementById(id, catalogName);
        StormXmlElement? second = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        second.Should().NotBeSameAs(first);
        first!.StormFiles.Should().HaveCount(3);
        second!.StormFiles.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetStormXmlElementById_MemberOnlyInMapAndCustom_DoesNotModifyMapElement()
    {
        // arrange - the base element has no Cooldown, so the merge adopts the map's Cooldown and then merges the custom one into it
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = XmlReaderHelper.GetStormXmlElement($"""<CEffectDamage id="{id}"><Amount value="1" /></CEffectDamage>""");
        StormXmlElement mapElement = XmlReaderHelper.GetStormXmlElement($"""<CEffectDamage id="{id}"><Cooldown value="5" /></CEffectDamage>""");
        StormXmlElement customElement = XmlReaderHelper.GetStormXmlElement($"""<CEffectDamage id="{id}"><Cooldown value="6" /></CEffectDamage>""");

        LayeredDataStorage layeredDataStorage = new();
        AddStormXmlElement(layeredDataStorage, StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(layeredDataStorage, StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = layeredDataStorage.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Members["Amount"].Value.RawValue.Should().Be("1");
        result.Members["Cooldown"].Value.RawValue.Should().Be("6");
        mapElement.Members["Cooldown"].Value.RawValue.Should().Be("5");
    }

    // parses the xml into an element with no id and adds it to the data storage of the given layer
    private static StormXmlElement AddStormXmlElement(LayeredDataStorage layeredDataStorage, StormModType stormModType, string xml)
    {
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(xml);

        layeredDataStorage.GetDataStorage(stormModType).AddElementWithNoId(element);

        return element;
    }

    private static StormXmlConstElement CreateStormXmlConstElement(string id, string value = "1")
    {
        return XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />""");
    }

    private static StormXmlElement CreateStormXmlElement(string id, string amount = "1")
    {
        return XmlReaderHelper.GetStormXmlElement($"""
            <CEffectDamage id="{id}">
              <Amount value="{amount}" />
            </CEffectDamage>
            """);
    }

    private static void AddStormXmlElement(LayeredDataStorage layeredDataStorage, StormModType stormModType, string catalogName, string id, StormXmlElement element)
    {
        ElementCache elementCache = layeredDataStorage.GetDataStorage(stormModType).ElementCache;

        if (!elementCache.StormXmlElementsByCatalog.TryGetValue(catalogName, out Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>> elementsById))
        {
            // inner dictionary keys are case-sensitive
            elementsById = new Dictionary<string, StormXmlElement>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
            elementCache.StormXmlElementsByCatalog.Add(catalogName, elementsById);
        }

        elementsById.Dictionary.Add(id, element);
    }
}
