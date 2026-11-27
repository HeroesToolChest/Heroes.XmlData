namespace Heroes.StormData.Cache.Elements.Tests;

[TestClass]
internal sealed class LayeredElementCacheTests
{
    private readonly ElementCache _baseElementCache = new();
    private readonly ElementCache _mapElementCache = new();
    private readonly ElementCache _customElementCache = new();
    private readonly LayeredElementCache _layeredElementCache;

    public LayeredElementCacheTests()
    {
        _layeredElementCache = new LayeredElementCache(_baseElementCache, _mapElementCache, _customElementCache);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void TryGetFirstStormXmlConstElementById_ElementInSingleLayer_ReturnsElement(StormModType stormModType)
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";
        StormXmlConstElement expected = CreateStormXmlConstElement(constId, "1777");

        GetElementCache(stormModType).StormXmlConstElementById.Dictionary.Add(constId, expected);

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

        // assert
        result.Should().BeTrue();
        constElement.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_NoLayersPopulated_ReturnsFalse()
    {
        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById("$Id", out StormXmlConstElement? constElement);

        // assert
        result.Should().BeFalse();
        constElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_IdNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add("$BaseConst", CreateStormXmlConstElement("$BaseConst"));
        GetElementCache(StormModType.Map).StormXmlConstElementById.Dictionary.Add("$MapConst", CreateStormXmlConstElement("$MapConst"));
        GetElementCache(StormModType.Custom).StormXmlConstElementById.Dictionary.Add("$CustomConst", CreateStormXmlConstElement("$CustomConst"));

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById("$DoesNotExist", out StormXmlConstElement? constElement);

        // assert
        result.Should().BeFalse();
        constElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormXmlConstElementById_IdWithDifferentCasing_ReturnsFalse()
    {
        // arrange - const ids are stored with StringComparer.Ordinal, so lookups are case-sensitive
        const string constId = "$ZagaraHunterKillerDamage";

        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add(constId, CreateStormXmlConstElement(constId));

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById(constId.ToUpperInvariant(), out StormXmlConstElement? constElement);

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

        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add(constId, baseElement);
        GetElementCache(StormModType.Map).StormXmlConstElementById.Dictionary.Add(constId, mapElement);

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

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

        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add(constId, baseElement);
        GetElementCache(StormModType.Custom).StormXmlConstElementById.Dictionary.Add(constId, customElement);

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

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

        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add(constId, baseElement);
        GetElementCache(StormModType.Map).StormXmlConstElementById.Dictionary.Add(constId, mapElement);
        GetElementCache(StormModType.Custom).StormXmlConstElementById.Dictionary.Add(constId, customElement);

        // act
        bool result = _layeredElementCache.TryGetFirstStormXmlConstElementById(constId, out StormXmlConstElement? constElement);

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

        GetElementCache(StormModType.Base).StormXmlConstElementById.Dictionary.Add("$BaseConst", baseElement);
        GetElementCache(StormModType.Map).StormXmlConstElementById.Dictionary.Add("$MapConst", mapElement);
        GetElementCache(StormModType.Custom).StormXmlConstElementById.Dictionary.Add("$CustomConst", customElement);

        // act
        bool baseResult = _layeredElementCache.TryGetFirstStormXmlConstElementById("$BaseConst", out StormXmlConstElement? foundBase);
        bool mapResult = _layeredElementCache.TryGetFirstStormXmlConstElementById("$MapConst", out StormXmlConstElement? foundMap);
        bool customResult = _layeredElementCache.TryGetFirstStormXmlConstElementById("$CustomConst", out StormXmlConstElement? foundCustom);

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
    public void TryGetIdByUnitNameInLayer_InThatLayer_ReturnsId(StormModType stormModType)
    {
        // arrange
        AddIdByUnitName(stormModType, "Actor", "HeroMuradin", "HeroMuradinActor");

        // act
        bool result = _layeredElementCache.TryGetIdByUnitNameInLayer(stormModType, "HeroMuradin", "Actor", out string? id);

        // assert
        result.Should().BeTrue();
        id.Should().Be("HeroMuradinActor");
    }

    [TestMethod]
    [DataRow("HeroButcher", "Actor")]
    [DataRow("heromuradin", "Actor")]
    [DataRow("HeroMuradin", "Unit")]
    [DataRow("", "Actor")]
    public void TryGetIdByUnitNameInLayer_NotFound_ReturnsFalse(string unitName, string catalogName)
    {
        // arrange - the unit name is case-sensitive and is only in the Actor catalog
        AddIdByUnitName(StormModType.Base, "Actor", "HeroMuradin", "HeroMuradinActor");

        // act
        bool result = _layeredElementCache.TryGetIdByUnitNameInLayer(StormModType.Base, unitName, catalogName, out string? id);

        // assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [TestMethod]
    public void TryGetIdByUnitNameInLayer_EachLayer_ReturnsOnlyThatLayersId()
    {
        // arrange
        AddIdByUnitName(StormModType.Base, "Actor", "HeroMuradin", "BaseActor");
        AddIdByUnitName(StormModType.Map, "Actor", "HeroMuradin", "MapActor");

        // act
        bool baseResult = _layeredElementCache.TryGetIdByUnitNameInLayer(StormModType.Base, "HeroMuradin", "Actor", out string? baseId);
        bool mapResult = _layeredElementCache.TryGetIdByUnitNameInLayer(StormModType.Map, "HeroMuradin", "Actor", out string? mapId);
        bool customResult = _layeredElementCache.TryGetIdByUnitNameInLayer(StormModType.Custom, "HeroMuradin", "Actor", out string? customId);

        // assert
        baseResult.Should().BeTrue();
        baseId.Should().Be("BaseActor");
        mapResult.Should().BeTrue();
        mapId.Should().Be("MapActor");
        customResult.Should().BeFalse();
        customId.Should().BeNull();
    }

    [TestMethod]
    public void TryGetIdByUnitNameInLayer_UndefinedLayer_ReturnsFalse()
    {
        // arrange
        AddIdByUnitName(StormModType.Base, "Actor", "HeroMuradin", "BaseActor");

        // act
        bool result = _layeredElementCache.TryGetIdByUnitNameInLayer((StormModType)99, "HeroMuradin", "Actor", out string? id);

        // assert
        result.Should().BeFalse();
        id.Should().BeNull();
    }

    [TestMethod]
    public void TryGetIdByUnitNameInLayer_CatalogNameWithDifferentCasing_ReturnsId()
    {
        // arrange
        AddIdByUnitName(StormModType.Base, "Actor", "HeroMuradin", "HeroMuradinActor");

        // act
        bool result = _layeredElementCache.TryGetIdByUnitNameInLayer(StormModType.Base, "HeroMuradin", "actor", out string? id);

        // assert
        result.Should().BeTrue();
        id.Should().Be("HeroMuradinActor");
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

        GetElementCache(stormModType).CatalogByClassElement.Dictionary.Add(elementName, catalogName);

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? foundCatalogName);

        // assert
        result.Should().BeTrue();
        foundCatalogName.Should().Be(catalogName);
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_ElementNotInAnyLayer_ReturnsFalse()
    {
        // arrange
        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add("EffectDamage", "Effect");
        GetElementCache(StormModType.Map).CatalogByClassElement.Dictionary.Add("AbilTargetedHeal", "Abil");
        GetElementCache(StormModType.Custom).CatalogByClassElement.Dictionary.Add("BehaviorBuff", "Behavior");

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName("DoesNotExist", out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_EmptyElementName_ReturnsFalse()
    {
        // arrange
        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add("EffectDamage", "Effect");

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(string.Empty, out string? catalogName);

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

        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add(elementName, catalogName);

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName.ToUpperInvariant(), out string? foundCatalogName);

        // assert
        result.Should().BeTrue();
        foundCatalogName.Should().Be(catalogName);
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_ElementNameAsSpanSlice_ReturnsCatalogName()
    {
        // arrange
        GetElementCache(StormModType.Map).CatalogByClassElement.Dictionary.Add("EffectDamage", "Effect");

        ReadOnlySpan<char> elementName = "xxEffectDamageyy".AsSpan(2, 12);

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InBaseAndMap_ReturnsMapCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add(elementName, "BaseCatalog");
        GetElementCache(StormModType.Map).CatalogByClassElement.Dictionary.Add(elementName, "MapCatalog");

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("MapCatalog");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InBaseAndCustom_ReturnsCustomCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add(elementName, "BaseCatalog");
        GetElementCache(StormModType.Custom).CatalogByClassElement.Dictionary.Add(elementName, "CustomCatalog");

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("CustomCatalog");
    }

    [TestMethod]
    public void TryGetFirstCatalogNameByElementName_InAllLayers_ReturnsCustomCatalogName()
    {
        // arrange
        const string elementName = "EffectDamage";

        GetElementCache(StormModType.Base).CatalogByClassElement.Dictionary.Add(elementName, "BaseCatalog");
        GetElementCache(StormModType.Map).CatalogByClassElement.Dictionary.Add(elementName, "MapCatalog");
        GetElementCache(StormModType.Custom).CatalogByClassElement.Dictionary.Add(elementName, "CustomCatalog");

        // act
        bool result = _layeredElementCache.TryGetFirstCatalogNameByElementName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("CustomCatalog");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void GetClassElementNamesByCatalogName_InSingleLayer_ReturnsElementNames(StormModType stormModType)
    {
        // arrange
        AddClassElementName(stormModType, "Effect", "EffectDamage");
        AddClassElementName(stormModType, "Effect", "EffectSet");

        // act
        HashSet<string> elementNames = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");

        // assert
        elementNames.Should().BeEquivalentTo("EffectDamage", "EffectSet");
    }

    [TestMethod]
    public void GetClassElementNamesByCatalogName_NoLayersHaveCatalog_ReturnsEmpty()
    {
        // arrange
        AddClassElementName(StormModType.Base, "Abil", "AbilEffectTarget");

        // act
        HashSet<string> elementNames = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");

        // assert
        elementNames.Should().BeEmpty();
    }

    [TestMethod]
    public void GetClassElementNamesByCatalogName_InAllLayers_ReturnsUnion()
    {
        // arrange
        AddClassElementName(StormModType.Base, "Effect", "EffectDamage");
        AddClassElementName(StormModType.Map, "Effect", "EffectSet");
        AddClassElementName(StormModType.Custom, "Effect", "EffectCreatePersistent");

        // act
        HashSet<string> elementNames = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");

        // assert
        elementNames.Should().BeEquivalentTo("EffectDamage", "EffectSet", "EffectCreatePersistent");
    }

    [TestMethod]
    public void GetClassElementNamesByCatalogName_SameElementNameInMoreThanOneLayer_ReturnsItOnce()
    {
        // arrange - the set is case-insensitive, like the stored element names
        AddClassElementName(StormModType.Base, "Effect", "EffectDamage");
        AddClassElementName(StormModType.Map, "Effect", "effectdamage");

        // act
        HashSet<string> elementNames = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");

        // assert
        elementNames.Should().ContainSingle();
    }

    [TestMethod]
    public void GetClassElementNamesByCatalogName_CatalogNameWithDifferentCasing_ReturnsElementNames()
    {
        // arrange
        AddClassElementName(StormModType.Base, "Effect", "EffectDamage");

        // act
        HashSet<string> elementNames = _layeredElementCache.GetClassElementNamesByCatalogName("EFFECT");

        // assert
        elementNames.Should().BeEquivalentTo("EffectDamage");
    }

    [TestMethod]
    public void GetClassElementNamesByCatalogName_CalledTwice_ReturnsNewSets()
    {
        // arrange
        AddClassElementName(StormModType.Base, "Effect", "EffectDamage");

        // act
        HashSet<string> first = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");
        first.Add("EffectSet");

        HashSet<string> second = _layeredElementCache.GetClassElementNamesByCatalogName("Effect");

        // assert
        second.Should().BeEquivalentTo("EffectDamage");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_NoLayersHaveElement_ReturnsNull()
    {
        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyBaseHasElement_ReturnsCachedInstance()
    {
        // arrange
        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(StormModType.Base, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeSameAs(baseElement);
        result.Members["Name"].Value.RawValue.Should().Be("Base");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyMapHasElement_ReturnsCachedInstance()
    {
        // arrange
        string xml =
            """
            <CAbil>
              <Name value="Map" />
            </CAbil>
            """;
        StormXmlElement mapElement = AddStormXmlElement(StormModType.Map, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeSameAs(mapElement);
        result.Members["Name"].Value.RawValue.Should().Be("Map");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_OnlyCustomHasElement_ReturnsCachedInstance()
    {
        // arrange
        string xml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(StormModType.Custom, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(StormModType.Base, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement(elementName);

        // assert
        result.Should().BeSameAs(baseElement);
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_NameWithTypeLetterDeclaration_ReturnsNull()
    {
        // arrange
        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("CAbil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_SliceOfLargerString_FindsElement()
    {
        // arrange
        string xml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(StormModType.Base, xml);

        const string text = "xxAbilyy";

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement(text.AsSpan(2, 4));

        // assert
        result.Should().BeSameAs(baseElement);
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_DifferentClassElementInLayer_ReturnsNull()
    {
        // arrange
        string xml =
            """
            <CEffectDamage>
              <Amount value="10" />
            </CEffectDamage>
            """;
        AddStormXmlElement(StormModType.Base, xml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_ElementWithIdOnly_ReturnsNull()
    {
        // arrange
        StormXmlElement elementWithId = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil id="TestAbil">
              <Name value="Base" />
            </CAbil>
            """);

        AddStormXmlElement(StormModType.Base, "Abil", elementWithId.Id!, elementWithId);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_BaseAndMap_MapOverridesBase()
    {
        // arrange
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
              <TechPlayer value="Upkeep" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Map, mapXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_MapAndCustom_CustomOverridesMap()
    {
        // arrange
        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="Upkeep" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["Name"].Value.RawValue.Should().Be("Custom");
        result.Members["TechPlayer"].Value.RawValue.Should().Be("Upkeep");
    }

    [TestMethod]
    public void GetStormXmlElementByClassElement_AllLayers_PrecedenceIsBaseThenMapThenCustom()
    {
        // arrange
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
              <TechPlayer value="BaseTech" />
              <TargetMessage value="BaseMessage" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="MapTech" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(StormModType.Base, baseXml);

        string mapXml =
            """
            <CAbil>
              <Name value="Map" />
              <TechPlayer value="Map" />
            </CAbil>
            """;
        StormXmlElement mapElement = AddStormXmlElement(StormModType.Map, mapXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? first = _layeredElementCache.GetStormXmlElementByClassElement("Abil");
        StormXmlElement? second = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string baseXml =
            """
            <CAbil>
              <Name value="Base" />
            </CAbil>
            """;
        StormXmlElement baseElement = AddStormXmlElement(StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <Name value="Custom" />
            </CAbil>
            """;
        StormXmlElement customElement = AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

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
        string baseXml =
            """
            <CAbil>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Base, baseXml);

        string customXml =
            """
            <CAbil>
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbil>
            """;
        AddStormXmlElement(StormModType.Custom, customXml);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementByClassElement("Abil");

        // assert
        result.Should().NotBeNull();
        result.Members["SharedFlags"].ByIndex["DisableWhileDead"].Value.RawValue.Should().Be("0");
        result.Members["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void GetStormXmlElementIdsByCatalogName_InAllLayers_ReturnsUnionOnce()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));
        AddStormXmlElement(StormModType.Base, "Unit", "HeroButcher", CreateStormXmlElement("HeroButcher"));
        AddStormXmlElement(StormModType.Map, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));
        AddStormXmlElement(StormModType.Custom, "Unit", "CustomUnit", CreateStormXmlElement("CustomUnit"));

        // act
        HashSet<string> ids = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Unit");

        // assert
        ids.Should().BeEquivalentTo("HeroMuradin", "HeroButcher", "CustomUnit");
    }

    [TestMethod]
    [DataRow(StormModType.Base, "HeroMuradin", "HeroButcher")]
    [DataRow(StormModType.Map, "HeroMuradin")]
    [DataRow(StormModType.Custom, "CustomUnit")]
    public void GetStormXmlElementIdsByCatalogName_OneLayer_ReturnsOnlyThatLayersIds(StormModType stormModType, params string[] expected)
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));
        AddStormXmlElement(StormModType.Base, "Unit", "HeroButcher", CreateStormXmlElement("HeroButcher"));
        AddStormXmlElement(StormModType.Map, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));
        AddStormXmlElement(StormModType.Custom, "Unit", "CustomUnit", CreateStormXmlElement("CustomUnit"));

        // act
        HashSet<string> ids = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Unit", stormModType);

        // assert
        ids.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public void GetStormXmlElementIdsByCatalogName_CatalogNameWithDifferentCasing_ReturnsIds()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));

        // act
        HashSet<string> ids = _layeredElementCache.GetStormXmlElementIdsByCatalogName("UNIT");

        // assert
        ids.Should().BeEquivalentTo("HeroMuradin");
    }

    [TestMethod]
    public void GetStormXmlElementIdsByCatalogName_IdsDifferingOnlyByCasing_ReturnsBoth()
    {
        // arrange - ids are case-sensitive
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));
        AddStormXmlElement(StormModType.Map, "Unit", "heromuradin", CreateStormXmlElement("heromuradin"));

        // act
        HashSet<string> ids = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Unit");

        // assert
        ids.Should().BeEquivalentTo("HeroMuradin", "heromuradin");
    }

    [TestMethod]
    public void GetStormXmlElementIdsByCatalogName_CatalogNotInAnyLayer_ReturnsEmpty()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));

        // act
        HashSet<string> ids = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Effect");

        // assert
        ids.Should().BeEmpty();
    }

    [TestMethod]
    public void GetStormXmlElementIdsByCatalogName_CalledTwice_ReturnsNewSets()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Unit", "HeroMuradin", CreateStormXmlElement("HeroMuradin"));

        // act
        HashSet<string> first = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Unit");
        first.Add("Other");

        HashSet<string> second = _layeredElementCache.GetStormXmlElementIdsByCatalogName("Unit");

        // assert
        second.Should().BeEquivalentTo("HeroMuradin");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void ContainsStormXmlElementByClassElement_ElementInSingleLayer_ReturnsTrue(StormModType stormModType)
    {
        // arrange
        AddStormXmlElement(stormModType, """<CEffectDamage default="1"><Kind value="Basic" /></CEffectDamage>""");

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementByClassElement("EffectDamage");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsStormXmlElementByClassElement_ElementNameWithDifferentCasing_ReturnsTrue()
    {
        // arrange - class element names are case-insensitive
        AddStormXmlElement(StormModType.Base, """<CEffectDamage default="1" />""");

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementByClassElement("effectdamage");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("EffectSet")]
    [DataRow("CEffectDamage")]
    [DataRow("")]
    public void ContainsStormXmlElementByClassElement_ElementNotInAnyLayer_ReturnsFalse(string elementName)
    {
        // arrange - the name is without its leading C
        AddStormXmlElement(StormModType.Base, """<CEffectDamage default="1" />""");

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementByClassElement(elementName);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsStormXmlElementByClassElement_OnlyElementWithId_ReturnsFalse()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Effect", "AlarakDamage", CreateStormXmlElement("AlarakDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementByClassElement("EffectDamage");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void ContainsStormXmlElementById_ElementInSingleLayer_ReturnsTrue(StormModType stormModType)
    {
        // arrange
        AddStormXmlElement(stormModType, "Effect", "AlarakDamage", CreateStormXmlElement("AlarakDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementById("AlarakDamage", "Effect");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsStormXmlElementById_NoLayersHaveElement_ReturnsFalse()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Effect", "OtherDamage", CreateStormXmlElement("OtherDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementById("AlarakDamage", "Effect");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsStormXmlElementById_IdInOtherCatalog_ReturnsFalse()
    {
        // arrange
        AddStormXmlElement(StormModType.Base, "Abil", "AlarakDamage", CreateStormXmlElement("AlarakDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementById("AlarakDamage", "Effect");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsStormXmlElementById_IdWithDifferentCasing_ReturnsFalse()
    {
        // arrange - ids are case-sensitive
        AddStormXmlElement(StormModType.Base, "Effect", "AlarakDamage", CreateStormXmlElement("AlarakDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementById("alarakdamage", "Effect");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsStormXmlElementById_CatalogNameWithDifferentCasing_ReturnsTrue()
    {
        // arrange - catalog names are case-insensitive
        AddStormXmlElement(StormModType.Base, "Effect", "AlarakDamage", CreateStormXmlElement("AlarakDamage"));

        // act
        bool result = _layeredElementCache.ContainsStormXmlElementById("AlarakDamage", "effect");

        // assert
        result.Should().BeTrue();
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

        AddStormXmlElement(stormModType, catalogName, id, expected);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormXmlElementById_NoLayersPopulated_ReturnsNull()
    {
        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById("TestEffect", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_IdNotInAnyLayer_ReturnsNull()
    {
        // arrange
        const string catalogName = "Effect";

        AddStormXmlElement(StormModType.Base, catalogName, "BaseEffect", CreateStormXmlElement("BaseEffect"));
        AddStormXmlElement(StormModType.Map, catalogName, "MapEffect", CreateStormXmlElement("MapEffect"));
        AddStormXmlElement(StormModType.Custom, catalogName, "CustomEffect", CreateStormXmlElement("CustomEffect"));

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById("DoesNotExist", catalogName);

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_CatalogNotInAnyLayer_ReturnsNull()
    {
        // arrange
        const string id = "TestEffect";

        AddStormXmlElement(StormModType.Base, "Effect", id, CreateStormXmlElement(id));

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, "Abil");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlElementById_SameIdInDifferentCatalog_ReturnsNull()
    {
        // arrange - the id exists in the Abil catalog only, so an Effect lookup must not find it
        const string id = "TestEffect";

        AddStormXmlElement(StormModType.Base, "Abil", id, CreateStormXmlElement(id));

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, "Effect");

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

        AddStormXmlElement(StormModType.Base, catalogName, id, expected);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName.ToUpperInvariant());

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetStormXmlElementById_IdAndCatalogNameAsSpanSlices_ReturnsElement()
    {
        // arrange
        StormXmlElement expected = CreateStormXmlElement("TestEffect");

        AddStormXmlElement(StormModType.Map, "Effect", "TestEffect", expected);

        ReadOnlySpan<char> id = "xxTestEffectyy".AsSpan(2, 10);
        ReadOnlySpan<char> catalogName = "xxEffectyy".AsSpan(2, 6);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(StormModType.Map, catalogName, id, mapElement);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseElement);
        result.Should().NotBeSameAs(mapElement);
        result.Members["Amount"].Value.RawValue.Should().Be("2");
        result.StormFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetStormXmlElementById_InBaseWithParentAndMapWithout_KeepsParent()
    {
        // arrange - the map's override leaves out the parent attribute
        AddStormXmlElement(StormModType.Base, "Effect", "ChildDamage", XmlReaderHelper.GetStormXmlElement("""<CEffectDamage id="ChildDamage" parent="ParentDamage" />"""));
        AddStormXmlElement(StormModType.Map, "Effect", "ChildDamage", CreateStormXmlElement("ChildDamage", "150"));

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById("ChildDamage", "Effect");

        // assert
        result.Should().NotBeNull();
        result.Parent.Should().Be("ParentDamage");
        result.HasParent.Should().BeTrue();
        result.Members["Amount"].Value.RawValue.Should().Be("150");
    }

    [TestMethod]
    public void GetStormXmlElementById_InBaseAndCustom_ReturnsMergedCopy()
    {
        // arrange
        const string catalogName = "Effect";
        const string id = "TestEffect";

        StormXmlElement baseElement = CreateStormXmlElement(id, "1");
        StormXmlElement customElement = CreateStormXmlElement(id, "3");

        AddStormXmlElement(StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(StormModType.Custom, catalogName, id, customElement);

        // act
        _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Base, catalogName, id, CreateStormXmlElement(id, "1"));
        AddStormXmlElement(StormModType.Map, catalogName, id, CreateStormXmlElement(id, "2"));
        AddStormXmlElement(StormModType.Custom, catalogName, id, CreateStormXmlElement(id, "3"));

        // act
        StormXmlElement? first = _layeredElementCache.GetStormXmlElementById(id, catalogName);
        StormXmlElement? second = _layeredElementCache.GetStormXmlElementById(id, catalogName);

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

        AddStormXmlElement(StormModType.Base, catalogName, id, baseElement);
        AddStormXmlElement(StormModType.Map, catalogName, id, mapElement);
        AddStormXmlElement(StormModType.Custom, catalogName, id, customElement);

        // act
        StormXmlElement? result = _layeredElementCache.GetStormXmlElementById(id, catalogName);

        // assert
        result.Should().NotBeNull();
        result.Members["Amount"].Value.RawValue.Should().Be("1");
        result.Members["Cooldown"].Value.RawValue.Should().Be("6");
        mapElement.Members["Cooldown"].Value.RawValue.Should().Be("5");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void GetScalingFieldValuesById_InSingleLayer_ReturnsCachedList(StormModType stormModType)
    {
        // arrange
        List<LevelScalingFieldValue> expected = AddScalingFieldValues(stormModType, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"), new LevelScalingFieldValue("LifeRegenRate", "0.03"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert - nothing to merge, so the cached list is returned without a copy
        result.Should().BeSameAs(expected);
        result.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.04"), new LevelScalingFieldValue("LifeRegenRate", "0.03"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_NoLayersPopulated_ReturnsNull()
    {
        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScalingFieldValuesById_IdNotInAnyLayer_ReturnsNull()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Map, "Unit", "HeroAlarak", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAlexstrasza", new LevelScalingFieldValue("LifeMax", "0.04"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("DoesNotExist", "Unit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScalingFieldValuesById_CatalogNotInAnyLayer_ReturnsNull()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScalingFieldValuesById_SameIdInDifferentCatalog_ReturnsNull()
    {
        // arrange - the id exists in the Behavior catalog only, so an Effect lookup must not find it
        AddScalingFieldValues(StormModType.Base, "Behavior", "AlexstraszaDragonqueenHealthIncrease", new LevelScalingFieldValue("Modification.VitalMaxArray[0]", "0.04"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("AlexstraszaDragonqueenHealthIncrease", "Effect");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScalingFieldValuesById_IdWithDifferentCasing_ReturnsNull()
    {
        // arrange - ids are stored with StringComparer.Ordinal, so lookups are case-sensitive
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HEROABATHUR", "Unit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScalingFieldValuesById_CatalogNameWithDifferentCasing_ReturnsList()
    {
        // arrange - catalog names are stored with StringComparer.OrdinalIgnoreCase, so lookups are case-insensitive
        List<LevelScalingFieldValue> expected = AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "UNIT");

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetScalingFieldValuesById_IdAndCatalogNameAsSpanSlices_ReturnsList()
    {
        // arrange
        List<LevelScalingFieldValue> expected = AddScalingFieldValues(StormModType.Map, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));

        ReadOnlySpan<char> id = "xxHeroAbathuryy".AsSpan(2, 11);
        ReadOnlySpan<char> catalogName = "xxUnityy".AsSpan(2, 4);

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById(id, catalogName);

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetScalingFieldValuesById_OtherLayersHaveCatalogButNotId_ReturnsCachedList()
    {
        // arrange - the map and custom layers have the catalog, but not the id, so there is nothing to merge
        List<LevelScalingFieldValue> expected = AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Map, "Unit", "HeroAlarak", new LevelScalingFieldValue("LifeMax", "0.05"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAlexstrasza", new LevelScalingFieldValue("LifeMax", "0.06"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().BeSameAs(expected);
    }

    [TestMethod]
    public void GetScalingFieldValuesById_SameFieldInBaseAndMap_MapReplacesValue()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Map, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.05"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.05"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_SameFieldInBaseAndCustom_CustomReplacesValue()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.06"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.06"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_SameFieldInMapAndCustom_CustomReplacesValue()
    {
        // arrange
        AddScalingFieldValues(StormModType.Map, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.05"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.06"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.06"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_DifferentFieldsInBaseAndCustom_KeepsBaseFieldsAndAddsCustomFields()
    {
        // arrange - a higher layer must not hide the base fields of the same entry
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"), new LevelScalingFieldValue("LifeRegenRate", "0.03"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("ShieldsMax", "0.02"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert - base fields first, then the new fields
        result.Should().Equal(
            new LevelScalingFieldValue("LifeMax", "0.04"),
            new LevelScalingFieldValue("LifeRegenRate", "0.03"),
            new LevelScalingFieldValue("ShieldsMax", "0.02"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_AllLayers_PrecedenceIsBaseThenMapThenCustom()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "base"), new LevelScalingFieldValue("LifeRegenRate", "base"), new LevelScalingFieldValue("ShieldsMax", "base"));
        AddScalingFieldValues(StormModType.Map, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "map"), new LevelScalingFieldValue("LifeRegenRate", "map"), new LevelScalingFieldValue("EnergyMax", "map"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "custom"), new LevelScalingFieldValue("EnergyRegenRate", "custom"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert - replaced fields keep their position, new fields are added in layer order
        result.Should().Equal(
            new LevelScalingFieldValue("LifeMax", "custom"),
            new LevelScalingFieldValue("LifeRegenRate", "map"),
            new LevelScalingFieldValue("ShieldsMax", "base"),
            new LevelScalingFieldValue("EnergyMax", "map"),
            new LevelScalingFieldValue("EnergyRegenRate", "custom"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_FieldsDifferingOnlyByCase_AreNotMerged()
    {
        // arrange - the Field comparison is case-sensitive
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LIFEMAX", "0.06"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().Equal(
            new LevelScalingFieldValue("LifeMax", "0.04"),
            new LevelScalingFieldValue("LIFEMAX", "0.06"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_MultipleLayers_ReturnsCopyAndDoesNotModifyCachedLists()
    {
        // arrange
        List<LevelScalingFieldValue> baseValues = AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        List<LevelScalingFieldValue> mapValues = AddScalingFieldValues(StormModType.Map, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeRegenRate", "0.03"));
        List<LevelScalingFieldValue> customValues = AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.06"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? result = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(baseValues);
        result.Should().NotBeSameAs(mapValues);
        result.Should().NotBeSameAs(customValues);

        baseValues.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.04"));
        mapValues.Should().Equal(new LevelScalingFieldValue("LifeRegenRate", "0.03"));
        customValues.Should().Equal(new LevelScalingFieldValue("LifeMax", "0.06"));
    }

    [TestMethod]
    public void GetScalingFieldValuesById_CalledRepeatedly_ReturnsNewListAndDoesNotAccumulate()
    {
        // arrange
        AddScalingFieldValues(StormModType.Base, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeMax", "0.04"));
        AddScalingFieldValues(StormModType.Custom, "Unit", "HeroAbathur", new LevelScalingFieldValue("LifeRegenRate", "0.03"));

        // act
        IReadOnlyList<LevelScalingFieldValue>? first = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");
        IReadOnlyList<LevelScalingFieldValue>? second = _layeredElementCache.GetScalingFieldValuesById("HeroAbathur", "Unit");

        // assert
        second.Should().NotBeSameAs(first);
        first.Should().HaveCount(2);
        second.Should().Equal(first);
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

    // adds the class element name to the catalog's set, the same way DataStorage.AddBaseClassCatalogName does
    private void AddClassElementName(StormModType stormModType, string catalogName, string elementName)
    {
        Dictionary<string, HashSet<string>> classElementsByCatalog = GetElementCache(stormModType).ClassElementsByCatalog;

        if (!classElementsByCatalog.TryGetValue(catalogName, out HashSet<string>? elementNames))
        {
            elementNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            classElementsByCatalog.Add(catalogName, elementNames);
        }

        elementNames.Add(elementName);
    }

    // parses the XML into an element with no id and adds it to the element cache of the given layer
    private StormXmlElement AddStormXmlElement(StormModType stormModType, string xml)
    {
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(xml);

        GetElementCache(stormModType).StormXmlElementByClassElement.Dictionary.Add(element.ElementName, element);

        return element;
    }

    private void AddStormXmlElement(StormModType stormModType, string catalogName, string id, StormXmlElement element)
    {
        ElementCache elementCache = GetElementCache(stormModType);

        if (!elementCache.StormXmlElementsByIdByCatalog.Dictionary.TryGetValue(catalogName, out Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>> elementsById))
        {
            // inner dictionary keys are case-sensitive
            elementsById = new Dictionary<string, StormXmlElement>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
            elementCache.StormXmlElementsByIdByCatalog.Dictionary.Add(catalogName, elementsById);
        }

        elementsById.Dictionary.Add(id, element);
    }

    // adds the scaling field values directly to the cache of the given layer, and returns the cached list
    private List<LevelScalingFieldValue> AddScalingFieldValues(StormModType stormModType, string catalogName, string id, params LevelScalingFieldValue[] scalingFieldValues)
    {
        ElementCache elementCache = GetElementCache(stormModType);

        if (!elementCache.ScalingFieldValueByIdByCatalog.Dictionary.TryGetValue(catalogName, out Dictionary<string, List<LevelScalingFieldValue>>.AlternateLookup<ReadOnlySpan<char>> scalingFieldValuesById))
        {
            // inner dictionary keys are case-sensitive
            scalingFieldValuesById = new Dictionary<string, List<LevelScalingFieldValue>>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
            elementCache.ScalingFieldValueByIdByCatalog.Dictionary.Add(catalogName, scalingFieldValuesById);
        }

        List<LevelScalingFieldValue> cachedScalingFieldValues = [.. scalingFieldValues];
        scalingFieldValuesById.Dictionary.Add(id, cachedScalingFieldValues);

        return cachedScalingFieldValues;
    }

    // adds the id by its unit name directly to the element cache of the given layer
    private void AddIdByUnitName(StormModType stormModType, string catalogName, string unitName, string id)
    {
        ElementCache elementCache = GetElementCache(stormModType);

        if (!elementCache.IdByUnitNameByCatalog.Dictionary.TryGetValue(catalogName, out Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> idByUnitName))
        {
            // inner dictionary keys are case-sensitive
            idByUnitName = new Dictionary<string, string>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
            elementCache.IdByUnitNameByCatalog.Dictionary.Add(catalogName, idByUnitName);
        }

        idByUnitName.Dictionary[unitName] = id;
    }

    private ElementCache GetElementCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseElementCache,
        StormModType.Map => _mapElementCache,
        StormModType.Custom => _customElementCache,
        _ => _customElementCache,
    };
}
