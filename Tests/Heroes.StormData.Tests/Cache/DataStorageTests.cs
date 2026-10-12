namespace Heroes.StormData.Tests.Cache;

[TestClass]
public class DataStorageTests
{
    [TestMethod]
    public void AddConstElement_MultipleDifferentIds_AddsAllElements()
    {
        // arrange
        StormXmlConstElement first = CreateStormXmlConstElement("$First", "1");
        StormXmlConstElement second = CreateStormXmlConstElement("$Second", "2");
        StormXmlConstElement third = CreateStormXmlConstElement("$Third", "3");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddConstElement(first);
        dataStorage.AddConstElement(second);
        dataStorage.AddConstElement(third);

        // assert
        Dictionary<string, StormXmlConstElement> cache = dataStorage.ElementCache.StormXmlConstElementById;

        cache.Should().HaveCount(3);
        cache["$First"].Should().BeSameAs(first);
        cache["$Second"].Should().BeSameAs(second);
        cache["$Third"].Should().BeSameAs(third);
    }

    [TestMethod]
    public void AddConstElement_ConstElement_IsFoundByAlternateLookup()
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";
        StormXmlConstElement element = CreateStormXmlConstElement(constId);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddConstElement(element);

        // assert
        bool result = dataStorage.ElementCache.StormXmlConstElementByIdAltLookup.TryGetValue(constId.AsSpan(), out StormXmlConstElement? found);

        result.Should().BeTrue();
        found.Should().BeSameAs(element);
    }

    [TestMethod]
    public void AddConstElement_DuplicateId_KeepsFirstElement()
    {
        // arrange
        const string constId = "$ZagaraHunterKillerDamage";

        StormXmlConstElement firstElement = CreateStormXmlConstElement(constId, "1");
        StormXmlConstElement secondElement = CreateStormXmlConstElement(constId, "2");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddConstElement(firstElement);
        dataStorage.AddConstElement(secondElement);

        // assert
        dataStorage.ElementCache.StormXmlConstElementById.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlConstElementById[constId].Should().BeSameAs(firstElement);
        dataStorage.ElementCache.StormXmlConstElementById[constId].Should().NotBeSameAs(secondElement);
    }

    [TestMethod]
    public void AddConstElement_IdsDifferingOnlyByCase_AddsBothElements()
    {
        // arrange - const ids are stored with StringComparer.Ordinal, so they are case-sensitive
        StormXmlConstElement lowerElement = CreateStormXmlConstElement("$zagarahunterkillerdamage", "1");
        StormXmlConstElement upperElement = CreateStormXmlConstElement("$ZAGARAHUNTERKILLERDAMAGE", "2");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddConstElement(lowerElement);
        dataStorage.AddConstElement(upperElement);

        // assert
        Dictionary<string, StormXmlConstElement> cache = dataStorage.ElementCache.StormXmlConstElementById;

        cache.Should().HaveCount(2);
        cache["$zagarahunterkillerdamage"].Should().BeSameAs(lowerElement);
        cache["$ZAGARAHUNTERKILLERDAMAGE"].Should().BeSameAs(upperElement);
    }

    [TestMethod]
    public void AddBaseClassCatalogName_NewElement_AddsElementNameToClassElementsByCatalog()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");

        // assert
        Dictionary<string, HashSet<string>> cache = dataStorage.ElementCache.ClassElementsByCatalog;

        cache.Should().ContainSingle();
        cache.Should().ContainKey("Effect");
        cache["Effect"].Should().ContainSingle();
        cache["Effect"].Should().Contain(element.ElementName);
    }

    [TestMethod]
    public void AddBaseClassCatalogName_NewElement_SetsCatalogByClassElement()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");

        // assert
        Dictionary<string, string> cache = dataStorage.ElementCache.CatalogByClassElement;

        cache.Should().ContainSingle();
        cache[element.ElementName].Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_NewElement_IsFoundByAlternateLookup()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");

        // assert
        bool result = dataStorage.ElementCache.CatalogByClassElementAltLookup.TryGetValue(element.ElementName.AsSpan(), out string? catalogName);

        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_MultipleElementsSameCatalog_AddsAllToSameSet()
    {
        // arrange
        StormXmlElement damageElement = CreateStormXmlElement("CEffectDamage");
        StormXmlElement healElement = CreateStormXmlElement("CEffectHeal");
        StormXmlElement setElement = CreateStormXmlElement("CEffectSet");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(damageElement, "Effect");
        dataStorage.AddBaseClassCatalogName(healElement, "Effect");
        dataStorage.AddBaseClassCatalogName(setElement, "Effect");

        // assert
        Dictionary<string, HashSet<string>> classElementsByCatalog = dataStorage.ElementCache.ClassElementsByCatalog;
        Dictionary<string, string> catalogByClassElement = dataStorage.ElementCache.CatalogByClassElement;

        classElementsByCatalog.Should().ContainSingle();
        classElementsByCatalog["Effect"].Should().HaveCount(3);
        classElementsByCatalog["Effect"].Should().Contain(damageElement.ElementName);
        classElementsByCatalog["Effect"].Should().Contain(healElement.ElementName);
        classElementsByCatalog["Effect"].Should().Contain(setElement.ElementName);

        catalogByClassElement.Should().HaveCount(3);
        catalogByClassElement[damageElement.ElementName].Should().Be("Effect");
        catalogByClassElement[healElement.ElementName].Should().Be("Effect");
        catalogByClassElement[setElement.ElementName].Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_SameElementDifferentCatalogs_AddsElementToEachCatalogSet()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");
        dataStorage.AddBaseClassCatalogName(element, "Behavior");
        dataStorage.AddBaseClassCatalogName(element, "Abil");

        // assert
        Dictionary<string, HashSet<string>> cache = dataStorage.ElementCache.ClassElementsByCatalog;

        cache.Should().HaveCount(3);
        cache["Effect"].Should().ContainSingle().Which.Should().Be(element.ElementName);
        cache["Behavior"].Should().ContainSingle().Which.Should().Be(element.ElementName);
        cache["Abil"].Should().ContainSingle().Which.Should().Be(element.ElementName);
    }

    [TestMethod]
    public void AddBaseClassCatalogName_SameElementDifferentCatalogs_KeepsFirstInCatalogByClassElement()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");
        dataStorage.AddBaseClassCatalogName(element, "Behavior");

        // assert
        dataStorage.ElementCache.CatalogByClassElement.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement[element.ElementName].Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_DuplicateElementSameCatalog_DoesNotAddDuplicate()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");
        dataStorage.AddBaseClassCatalogName(element, "Effect");

        // assert
        dataStorage.ElementCache.ClassElementsByCatalog.Should().ContainSingle();
        dataStorage.ElementCache.ClassElementsByCatalog["Effect"].Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement[element.ElementName].Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_ElementNamesDifferingOnlyByCase_DoesNotAddDuplicate()
    {
        // arrange - the element name set and CatalogByClassElement both use StringComparer.OrdinalIgnoreCase
        StormXmlElement element = CreateStormXmlElement("CEffectDamage");
        StormXmlElement lowerElement = CreateStormXmlElement("ceffectdamage");
        StormXmlElement upperElement = CreateStormXmlElement("CEFFECTDAMAGE");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(element, "Effect");
        dataStorage.AddBaseClassCatalogName(lowerElement, "Effect");
        dataStorage.AddBaseClassCatalogName(upperElement, "Effect");

        // assert - the first casing is kept
        HashSet<string> elementNames = dataStorage.ElementCache.ClassElementsByCatalog["Effect"];

        elementNames.Should().ContainSingle();
        elementNames.Should().Contain(element.ElementName);
        dataStorage.ElementCache.CatalogByClassElement.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement[lowerElement.ElementName].Should().Be("Effect");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_CatalogNamesDifferingOnlyByCase_ShareSameEntry()
    {
        // arrange - ClassElementsByCatalog uses StringComparer.OrdinalIgnoreCase for the catalog name key
        StormXmlElement damageElement = CreateStormXmlElement("CEffectDamage");
        StormXmlElement healElement = CreateStormXmlElement("CEffectHeal");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(damageElement, "Effect");
        dataStorage.AddBaseClassCatalogName(healElement, "EFFECT");

        // assert
        Dictionary<string, HashSet<string>> classElementsByCatalog = dataStorage.ElementCache.ClassElementsByCatalog;

        classElementsByCatalog.Should().ContainSingle();
        classElementsByCatalog["effect"].Should().HaveCount(2);
        classElementsByCatalog["effect"].Should().Contain(damageElement.ElementName);
        classElementsByCatalog["effect"].Should().Contain(healElement.ElementName);

        // the catalog name is stored as passed for each element in CatalogByClassElement
        dataStorage.ElementCache.CatalogByClassElement[damageElement.ElementName].Should().Be("Effect");
        dataStorage.ElementCache.CatalogByClassElement[healElement.ElementName].Should().Be("EFFECT");
    }

    [TestMethod]
    public void AddBaseClassCatalogName_DifferentElementsAndCatalogs_AddsSeparateEntries()
    {
        // arrange
        StormXmlElement effectElement = CreateStormXmlElement("CEffectDamage");
        StormXmlElement behaviorElement = CreateStormXmlElement("CBehaviorBuff");
        StormXmlElement abilElement = CreateStormXmlElement("CAbilEffectTarget");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(effectElement, "Effect");
        dataStorage.AddBaseClassCatalogName(behaviorElement, "Behavior");
        dataStorage.AddBaseClassCatalogName(abilElement, "Abil");

        // assert
        Dictionary<string, HashSet<string>> classElementsByCatalog = dataStorage.ElementCache.ClassElementsByCatalog;
        Dictionary<string, string> catalogByClassElement = dataStorage.ElementCache.CatalogByClassElement;

        classElementsByCatalog.Should().HaveCount(3);
        classElementsByCatalog["Effect"].Should().ContainSingle().Which.Should().Be(effectElement.ElementName);
        classElementsByCatalog["Behavior"].Should().ContainSingle().Which.Should().Be(behaviorElement.ElementName);
        classElementsByCatalog["Abil"].Should().ContainSingle().Which.Should().Be(abilElement.ElementName);

        catalogByClassElement.Should().HaveCount(3);
        catalogByClassElement[effectElement.ElementName].Should().Be("Effect");
        catalogByClassElement[behaviorElement.ElementName].Should().Be("Behavior");
        catalogByClassElement[abilElement.ElementName].Should().Be("Abil");
    }

    [TestMethod]
    public void AddElementWithNoId_NewElement_AddsElementByClassElementName()
    {
        // arrange
        StormXmlElement element = CreateStormXmlElementWithNoId("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithNoId(element);

        // assert
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement;

        cache.Should().ContainSingle();
        cache.Should().ContainKey(element.ElementName);
        cache[element.ElementName].Should().BeSameAs(element);
    }

    [TestMethod]
    public void AddElementWithNoId_MultipleDifferentElements_AddsAllElements()
    {
        // arrange
        StormXmlElement effectElement = CreateStormXmlElementWithNoId("CEffectDamage");
        StormXmlElement behaviorElement = CreateStormXmlElementWithNoId("CBehaviorBuff");
        StormXmlElement abilElement = CreateStormXmlElementWithNoId("CAbilEffectTarget");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithNoId(effectElement);
        dataStorage.AddElementWithNoId(behaviorElement);
        dataStorage.AddElementWithNoId(abilElement);

        // assert
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement;

        cache.Should().HaveCount(3);
        cache[effectElement.ElementName].Should().BeSameAs(effectElement);
        cache[behaviorElement.ElementName].Should().BeSameAs(behaviorElement);
        cache[abilElement.ElementName].Should().BeSameAs(abilElement);
    }

    [TestMethod]
    public void AddElementWithNoId_DuplicateElementName_KeepsFirstElement()
    {
        // arrange
        StormXmlElement firstElement = CreateStormXmlElementWithNoId("CEffectDamage");
        StormXmlElement secondElement = CreateStormXmlElementWithNoId("CEffectDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithNoId(firstElement);
        dataStorage.AddElementWithNoId(secondElement);

        // assert - the second element is merged into the first, not stored
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement;

        cache.Should().ContainSingle();
        cache[firstElement.ElementName].Should().BeSameAs(firstElement);
        cache[firstElement.ElementName].Should().NotBeSameAs(secondElement);
    }

    [TestMethod]
    public void AddElementWithNoId_ElementNamesDifferingOnlyByCase_ShareSameEntry()
    {
        // arrange - StormXmlElementByClassElement uses StringComparer.OrdinalIgnoreCase
        StormXmlElement element = CreateStormXmlElementWithNoId("CEffectDamage");
        StormXmlElement lowerElement = CreateStormXmlElementWithNoId("ceffectdamage");
        StormXmlElement upperElement = CreateStormXmlElementWithNoId("CEFFECTDAMAGE");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithNoId(element);
        dataStorage.AddElementWithNoId(lowerElement);
        dataStorage.AddElementWithNoId(upperElement);

        // assert - the first element and its key casing are kept
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement;

        cache.Should().ContainSingle();
        cache.Keys.Should().ContainSingle().Which.Should().BeEquivalentTo(element.ElementName);
        cache[lowerElement.ElementName].Should().BeSameAs(element);
        cache[upperElement.ElementName].Should().BeSameAs(element);
    }

    [TestMethod]
    public void AddElementWithId_NewCatalog_AddsElementToCatalog()
    {
        // arrange
        const string catalogName = "Effect";

        StormXmlElement element = CreateStormXmlElement("CEffectDamage", "MyDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(element, catalogName);

        // assert
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().ContainKey(catalogName);

        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, catalogName).Dictionary;

        catalog.Should().ContainSingle();
        catalog[element.Id!].Should().BeSameAs(element);
    }

    [TestMethod]
    public void AddElementWithId_NewCatalog_IsFoundByAlternateLookup()
    {
        // arrange
        const string catalogName = "Effect";
        StormXmlElement element = CreateStormXmlElement("CEffectDamage", "MyDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(element, catalogName);

        // assert
        bool result = GetCatalogLookup(dataStorage, catalogName).TryGetValue(element.Id.AsSpan(), out StormXmlElement? found);

        result.Should().BeTrue();
        found.Should().BeSameAs(element);
    }

    [TestMethod]
    public void AddElementWithId_SameCatalogDifferentIds_AddsAllElements()
    {
        // arrange
        const string catalogName = "Effect";
        StormXmlElement first = CreateStormXmlElement("CEffectDamage", "First");
        StormXmlElement second = CreateStormXmlElement("CEffectDamage", "Second");
        StormXmlElement third = CreateStormXmlElement("CEffectHeal", "Third");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(first, catalogName);
        dataStorage.AddElementWithId(second, catalogName);
        dataStorage.AddElementWithId(third, catalogName);

        // assert
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().ContainSingle();

        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, catalogName).Dictionary;

        catalog.Should().HaveCount(3);
        catalog["First"].Should().BeSameAs(first);
        catalog["Second"].Should().BeSameAs(second);
        catalog["Third"].Should().BeSameAs(third);
    }

    [TestMethod]
    public void AddElementWithId_DuplicateIdInSameCatalog_KeepsFirstElement()
    {
        // arrange
        const string catalogName = "Effect";
        StormXmlElement firstElement = CreateStormXmlElement("CEffectDamage", "MyDamage");
        StormXmlElement secondElement = CreateStormXmlElement("CEffectDamage", "MyDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(firstElement, catalogName);
        dataStorage.AddElementWithId(secondElement, catalogName);

        // assert - the second element is merged into the first, not stored
        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, catalogName).Dictionary;

        catalog.Should().ContainSingle();
        catalog["MyDamage"].Should().BeSameAs(firstElement);
        catalog["MyDamage"].Should().NotBeSameAs(secondElement);
    }

    [TestMethod]
    public void AddElementWithId_IdsDifferingOnlyByCase_AddsBothElements()
    {
        // arrange - the inner dictionary uses StringComparer.Ordinal, so ids are case-sensitive
        const string catalogName = "Effect";
        StormXmlElement lowerElement = CreateStormXmlElement("CEffectDamage", "mydamage");
        StormXmlElement upperElement = CreateStormXmlElement("CEffectDamage", "MYDAMAGE");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(lowerElement, catalogName);
        dataStorage.AddElementWithId(upperElement, catalogName);

        // assert
        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, catalogName).Dictionary;

        catalog.Should().HaveCount(2);
        catalog["mydamage"].Should().BeSameAs(lowerElement);
        catalog["MYDAMAGE"].Should().BeSameAs(upperElement);
    }

    [TestMethod]
    public void AddElementWithId_IdWithDifferentCasing_IsNotFoundByAlternateLookup()
    {
        // arrange
        const string catalogName = "Effect";
        StormXmlElement element = CreateStormXmlElement("CEffectDamage", "MyDamage");

        DataStorage dataStorage = new();

        dataStorage.AddElementWithId(element, catalogName);

        // act
        var lookup = GetCatalogLookup(dataStorage, catalogName);
        bool exactResult = lookup.TryGetValue("MyDamage".AsSpan(), out StormXmlElement? exactFound);
        bool differentCaseResult = lookup.TryGetValue("mydamage".AsSpan(), out StormXmlElement? differentCaseFound);

        // assert
        exactResult.Should().BeTrue();
        exactFound.Should().BeSameAs(element);
        differentCaseResult.Should().BeFalse();
        differentCaseFound.Should().BeNull();
    }

    [TestMethod]
    public void AddElementWithId_SameIdDifferentCatalogs_AddsElementToEachCatalog()
    {
        // arrange
        StormXmlElement effectElement = CreateStormXmlElement("CEffectDamage", "Shared");
        StormXmlElement behaviorElement = CreateStormXmlElement("CBehaviorBuff", "Shared");
        StormXmlElement abilElement = CreateStormXmlElement("CAbilEffectTarget", "Shared");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(effectElement, "Effect");
        dataStorage.AddElementWithId(behaviorElement, "Behavior");
        dataStorage.AddElementWithId(abilElement, "Abil");

        // assert
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().HaveCount(3);

        GetCatalogLookup(dataStorage, "Effect").Dictionary["Shared"].Should().BeSameAs(effectElement);
        GetCatalogLookup(dataStorage, "Behavior").Dictionary["Shared"].Should().BeSameAs(behaviorElement);
        GetCatalogLookup(dataStorage, "Abil").Dictionary["Shared"].Should().BeSameAs(abilElement);
    }

    [TestMethod]
    public void AddElementWithId_CatalogNamesDifferingOnlyByCase_ShareSameCatalog()
    {
        // arrange - StormXmlElementsByCatalog uses StringComparer.OrdinalIgnoreCase for the catalog name key
        StormXmlElement lowerElement = CreateStormXmlElement("CEffectDamage", "DamageOne");
        StormXmlElement upperElement = CreateStormXmlElement("CEffectHeal", "HealOne");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(lowerElement, "effect");
        dataStorage.AddElementWithId(upperElement, "EFFECT");

        // assert - the first casing of the catalog name is kept as the key
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlElementsByCatalog.Keys.Should().ContainSingle().Which.Should().Be("effect");

        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, "Effect").Dictionary;

        catalog.Should().HaveCount(2);
        catalog["DamageOne"].Should().BeSameAs(lowerElement);
        catalog["HealOne"].Should().BeSameAs(upperElement);
    }

    [TestMethod]
    public void AddElementWithId_SameIdCatalogNamesDifferingOnlyByCase_KeepsFirstElement()
    {
        // arrange
        StormXmlElement firstElement = CreateStormXmlElement("CEffectDamage", "MyDamage");
        StormXmlElement secondElement = CreateStormXmlElement("CEffectDamage", "MyDamage");

        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(firstElement, "effect");
        dataStorage.AddElementWithId(secondElement, "EFFECT");

        // assert - same catalog and same id, so the second element is merged into the first
        dataStorage.ElementCache.StormXmlElementsByCatalog.Should().ContainSingle();

        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, "Effect").Dictionary;

        catalog.Should().ContainSingle();
        catalog["MyDamage"].Should().BeSameAs(firstElement);
        catalog["MyDamage"].Should().NotBeSameAs(secondElement);
    }

    private static StormXmlConstElement CreateStormXmlConstElement(string id, string value = "1")
    {
        return XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />""");
    }

    private static StormXmlElement CreateStormXmlElement(string elementName, string id = "Test")
    {
        return XmlReaderHelper.GetStormXmlElement($"""<{elementName} id="{id}" />""");
    }

    private static StormXmlElement CreateStormXmlElementWithNoId(string elementName)
    {
        return XmlReaderHelper.GetStormXmlElement($"""<{elementName} />""");
    }

    private static Dictionary<string, StormXmlElement>.AlternateLookup<ReadOnlySpan<char>> GetCatalogLookup(DataStorage dataStorage, string catalogName)
    {
        return dataStorage.ElementCache.StormXmlElementsByCatalog[catalogName];
    }
}
