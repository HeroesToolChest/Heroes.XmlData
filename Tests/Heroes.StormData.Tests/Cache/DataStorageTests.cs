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
        Dictionary<string, StormXmlConstElement> cache = dataStorage.ElementCache.StormXmlConstElementById.Dictionary;

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
        bool result = dataStorage.ElementCache.StormXmlConstElementById.TryGetValue(constId.AsSpan(), out StormXmlConstElement? found);

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
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary[constId].Should().BeSameAs(firstElement);
        dataStorage.ElementCache.StormXmlConstElementById.Dictionary[constId].Should().NotBeSameAs(secondElement);
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
        Dictionary<string, StormXmlConstElement> cache = dataStorage.ElementCache.StormXmlConstElementById.Dictionary;

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
        Dictionary<string, string> cache = dataStorage.ElementCache.CatalogByClassElement.Dictionary;

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
        bool result = dataStorage.ElementCache.CatalogByClassElement.TryGetValue(element.ElementName.AsSpan(), out string? catalogName);

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
        Dictionary<string, string> catalogByClassElement = dataStorage.ElementCache.CatalogByClassElement.Dictionary;

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
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement.Dictionary[element.ElementName].Should().Be("Effect");
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
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement.Dictionary[element.ElementName].Should().Be("Effect");
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
        dataStorage.ElementCache.CatalogByClassElement.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.CatalogByClassElement.Dictionary[lowerElement.ElementName].Should().Be("Effect");
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
        dataStorage.ElementCache.CatalogByClassElement.Dictionary[damageElement.ElementName].Should().Be("Effect");
        dataStorage.ElementCache.CatalogByClassElement.Dictionary[healElement.ElementName].Should().Be("EFFECT");
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
        Dictionary<string, string> catalogByClassElement = dataStorage.ElementCache.CatalogByClassElement.Dictionary;

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
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary;

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
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary;

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
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary;

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
        Dictionary<string, StormXmlElement> cache = dataStorage.ElementCache.StormXmlElementByClassElement.Dictionary;

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainKey(catalogName);

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainSingle();

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().HaveCount(3);

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainSingle();
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Keys.Should().ContainSingle().Which.Should().Be("effect");

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
        dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainSingle();

        Dictionary<string, StormXmlElement> catalog = GetCatalogLookup(dataStorage, "Effect").Dictionary;

        catalog.Should().ContainSingle();
        catalog["MyDamage"].Should().BeSameAs(firstElement);
        catalog["MyDamage"].Should().NotBeSameAs(secondElement);
    }

    [TestMethod]
    public void AddLevelScalingElementValue_SingleModification_AddsFieldValueByCatalogAndEntry()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray("""
            <LevelScalingArray Ability="AlexstraszaGiftOfLife">
              <Modifications>
                <Catalog value="Behavior" />
                <Entry value="AlexstraszaDragonqueenHealthIncrease" />
                <Field value="Modification.VitalMaxArray[0]" />
                <Value value="0.040000" />
                <AffectedByOverdrive value="1" />
              </Modifications>
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert - only Catalog, Entry, Field and Value are used, AffectedByOverdrive is ignored
        dataStorage.ElementCache.ScalingFieldValueByIdByCatalog.Dictionary.Should().ContainSingle();
        GetScalingFieldValuesById(dataStorage, "Behavior").Should().ContainSingle();
        GetScalingFieldValues(dataStorage, "Behavior", "AlexstraszaDragonqueenHealthIncrease").Should().Equal(
            new LevelScalingFieldValue("Modification.VitalMaxArray[0]", "0.040000"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_MultipleModifications_AddsAllValues()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
              {CreateModificationsXml("Effect", "AbathurSymbioteSpikeBurstDamage", "Amount", "0.045")}
              {CreateModificationsXml("Behavior", "AbathurSymbioteCarapaceShield", "DamageResponse.ModifyLimit", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        dataStorage.ElementCache.ScalingFieldValueByIdByCatalog.Dictionary.Should().HaveCount(2);
        GetScalingFieldValuesById(dataStorage, "Effect").Should().HaveCount(2);
        GetScalingFieldValuesById(dataStorage, "Behavior").Should().ContainSingle();
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.04"));
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteSpikeBurstDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.045"));
        GetScalingFieldValues(dataStorage, "Behavior", "AbathurSymbioteCarapaceShield").Should().Equal(new LevelScalingFieldValue("DamageResponse.ModifyLimit", "0.05"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_MultipleLevelScalingEntries_AddsValuesFromEachEntry()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
            </LevelScalingArray>
            <LevelScalingArray Ability="AbathurToxicNest">
              {CreateModificationsXml("Effect", "AbathurToxicNestDamage", "Amount", "0.045")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValuesById(dataStorage, "Effect").Should().HaveCount(2);
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.04"));
        GetScalingFieldValues(dataStorage, "Effect", "AbathurToxicNestDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.045"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_IndexedEntries_AddsValuesFromEachEntry()
    {
        // arrange - a named index on LevelScalingArray and numerical indexes on Modifications
        StormXmlArray levelScalingArray = CreateLevelScalingArray("""
            <LevelScalingArray index="AbathurSymbiote">
              <Modifications index="0">
                <Catalog value="Effect" />
                <Entry value="AbathurSymbioteStabDamage" />
                <Field value="Amount" />
                <Value value="0.04" />
              </Modifications>
              <Modifications index="1">
                <Catalog value="Effect" />
                <Entry value="AbathurSymbioteSpikeBurstDamage" />
                <Field value="Amount" />
                <Value value="0.045" />
              </Modifications>
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValuesById(dataStorage, "Effect").Should().HaveCount(2);
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.04"));
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteSpikeBurstDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.045"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_SameFieldInSameArray_ReplacesValue()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.05"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_SameFieldAcrossCalls_ReplacesValue()
    {
        // arrange
        StormXmlArray firstLevelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
            </LevelScalingArray>
            """);

        StormXmlArray secondLevelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(firstLevelScalingArray);
        dataStorage.AddLevelScalingElementValue(secondLevelScalingArray);

        // assert
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", "0.05"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_SameFieldReplaced_KeepsOrderOfOtherFields()
    {
        // arrange - the replaced field keeps its position in the list
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="HeroAbathur">
              {CreateModificationsXml("Unit", "HeroAbathur", "LifeMax", "0.04")}
              {CreateModificationsXml("Unit", "HeroAbathur", "LifeRegenRate", "0.03")}
              {CreateModificationsXml("Unit", "HeroAbathur", "LifeMax", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValues(dataStorage, "Unit", "HeroAbathur").Should().Equal(
            new LevelScalingFieldValue("LifeMax", "0.05"),
            new LevelScalingFieldValue("LifeRegenRate", "0.03"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_SameEntryDifferentFields_AddsSeparateValues()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AlexstraszaGiftOfLife">
              {CreateModificationsXml("Behavior", "AlexstraszaDragonqueenHealthIncrease", "Modification.VitalMaxArray[0]", "0.04")}
              {CreateModificationsXml("Behavior", "AlexstraszaDragonqueenHealthIncrease", "Modification.VitalRegenArray[0]", "0.03")}
              {CreateModificationsXml("Effect", "AlexstraszaDragonqueenHealthIncrease", "Modification.VitalMaxArray[0]", "0.02")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert - each field of an entry is kept, and the same entry in another catalog is separate
        GetScalingFieldValues(dataStorage, "Behavior", "AlexstraszaDragonqueenHealthIncrease").Should().Equal(
            new LevelScalingFieldValue("Modification.VitalMaxArray[0]", "0.04"),
            new LevelScalingFieldValue("Modification.VitalRegenArray[0]", "0.03"));
        GetScalingFieldValues(dataStorage, "Effect", "AlexstraszaDragonqueenHealthIncrease").Should().Equal(
            new LevelScalingFieldValue("Modification.VitalMaxArray[0]", "0.02"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_CatalogsDifferingOnlyByCase_AddsToSameCatalog()
    {
        // arrange - catalog names are stored with StringComparer.OrdinalIgnoreCase
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
              {CreateModificationsXml("EFFECT", "AbathurSymbioteSpikeBurstDamage", "Amount", "0.045")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        dataStorage.ElementCache.ScalingFieldValueByIdByCatalog.Dictionary.Should().ContainSingle();
        GetScalingFieldValuesById(dataStorage, "effect").Should().HaveCount(2);
    }

    [TestMethod]
    public void AddLevelScalingElementValue_EntriesDifferingOnlyByCase_AddsSeparateEntries()
    {
        // arrange - entry ids are stored with StringComparer.Ordinal
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
              {CreateModificationsXml("Effect", "ABATHURSYMBIOTESTABDAMAGE", "Amount", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        Dictionary<string, List<LevelScalingFieldValue>> scalingFieldValuesById = GetScalingFieldValuesById(dataStorage, "Effect");

        scalingFieldValuesById.Should().HaveCount(2);
        scalingFieldValuesById["AbathurSymbioteStabDamage"].Should().Equal(new LevelScalingFieldValue("Amount", "0.04"));
        scalingFieldValuesById["ABATHURSYMBIOTESTABDAMAGE"].Should().Equal(new LevelScalingFieldValue("Amount", "0.05"));
        scalingFieldValuesById.Should().NotContainKey("abathursymbiotestabdamage");
    }

    [TestMethod]
    public void AddLevelScalingElementValue_FieldsDifferingOnlyByCase_AddsSeparateValues()
    {
        // arrange - the Field comparison is case-sensitive, so these are not replaced
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.04")}
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "AMOUNT", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(
            new LevelScalingFieldValue("Amount", "0.04"),
            new LevelScalingFieldValue("AMOUNT", "0.05"));
    }

    [TestMethod]
    [DataRow("Catalog")]
    [DataRow("Entry")]
    [DataRow("Field")]
    [DataRow("Value")]
    public void AddLevelScalingElementValue_ModificationMissingMember_SkipsOnlyThatModification(string missingMember)
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "IncompleteEntry", "Amount", "0.04", missingMember)}
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        Dictionary<string, List<LevelScalingFieldValue>> scalingFieldValuesById = GetScalingFieldValuesById(dataStorage, "Effect");

        scalingFieldValuesById.Should().ContainSingle();
        scalingFieldValuesById["AbathurSymbioteStabDamage"].Should().Equal(new LevelScalingFieldValue("Amount", "0.05"));
    }

    [TestMethod]
    [DataRow("Catalog")]
    [DataRow("Entry")]
    [DataRow("Field")]
    [DataRow("Value")]
    public void AddLevelScalingElementValue_ModificationMemberWithNoValueAttribute_SkipsOnlyThatModification(string memberWithNoValue)
    {
        // arrange - e.g. <Catalog /> instead of <Catalog value="Effect" />
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "IncompleteEntry", "Amount", "0.04", memberWithNoValue: memberWithNoValue)}
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", "0.05")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        Dictionary<string, List<LevelScalingFieldValue>> scalingFieldValuesById = GetScalingFieldValuesById(dataStorage, "Effect");

        scalingFieldValuesById.Should().ContainSingle();
        scalingFieldValuesById["AbathurSymbioteStabDamage"].Should().Equal(new LevelScalingFieldValue("Amount", "0.05"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_ModificationWithEmptyValueAttribute_AddsEmptyValue()
    {
        // arrange - value="" is an attribute that is present but empty, so it is not skipped
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote">
              {CreateModificationsXml("Effect", "AbathurSymbioteStabDamage", "Amount", string.Empty)}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        GetScalingFieldValues(dataStorage, "Effect", "AbathurSymbioteStabDamage").Should().Equal(new LevelScalingFieldValue("Amount", string.Empty));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_EntryWithNoModifications_SkipsOnlyThatEntry()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray($"""
            <LevelScalingArray Ability="AbathurSymbiote" />
            <LevelScalingArray Ability="AbathurToxicNest">
              {CreateModificationsXml("Effect", "AbathurToxicNestDamage", "Amount", "0.045")}
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        Dictionary<string, List<LevelScalingFieldValue>> scalingFieldValuesById = GetScalingFieldValuesById(dataStorage, "Effect");

        scalingFieldValuesById.Should().ContainSingle();
        scalingFieldValuesById["AbathurToxicNestDamage"].Should().Equal(new LevelScalingFieldValue("Amount", "0.045"));
    }

    [TestMethod]
    public void AddLevelScalingElementValue_NoModificationsInAnyEntry_AddsNothing()
    {
        // arrange
        StormXmlArray levelScalingArray = CreateLevelScalingArray("""
            <LevelScalingArray Ability="AbathurSymbiote" />
            <LevelScalingArray Ability="AbathurToxicNest">
              <SomethingElse value="1" />
            </LevelScalingArray>
            """);

        DataStorage dataStorage = new();

        // act
        dataStorage.AddLevelScalingElementValue(levelScalingArray);

        // assert
        dataStorage.ElementCache.ScalingFieldValueByIdByCatalog.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void ElementsVersion_NewDataStorage_IsZero()
    {
        // arrange
        DataStorage dataStorage = new();

        // act
        int version = dataStorage.ElementsVersion;

        // assert
        version.Should().Be(0);
    }

    [TestMethod]
    public void ElementsVersion_AddElementWithId_Changes()
    {
        // arrange
        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithId(CreateStormXmlElement("CEffectDamage", "First"), "Effect");
        int afterFirst = dataStorage.ElementsVersion;
        dataStorage.AddElementWithId(CreateStormXmlElement("CEffectDamage", "First"), "Effect"); // merged into the existing one
        int afterSecond = dataStorage.ElementsVersion;

        // assert
        afterFirst.Should().NotBe(0);
        afterSecond.Should().NotBe(afterFirst);
    }

    [TestMethod]
    public void ElementsVersion_AddElementWithNoId_Changes()
    {
        // arrange
        DataStorage dataStorage = new();

        // act
        dataStorage.AddElementWithNoId(CreateStormXmlElementWithNoId("CEffectDamage"));

        // assert
        dataStorage.ElementsVersion.Should().NotBe(0);
    }

    [TestMethod]
    public void ElementsVersion_AddBaseClassCatalogName_Changes()
    {
        // arrange
        DataStorage dataStorage = new();

        // act
        dataStorage.AddBaseClassCatalogName(CreateStormXmlElementWithNoId("CEffectDamage"), "Effect");

        // assert
        dataStorage.ElementsVersion.Should().NotBe(0);
    }

    [TestMethod]
    public void ElementsVersion_AddConstElementOrLevelScaling_DoesNotChange()
    {
        // arrange
        DataStorage dataStorage = new();

        // act
        dataStorage.AddConstElement(CreateStormXmlConstElement("$Const"));
        dataStorage.AddLevelScalingElementValue(CreateLevelScalingArray(
            $"""
            <LevelScalingArray Ability="Abil">
              {CreateModificationsXml("Effect", "Damage", "Amount", "0.04")}
            </LevelScalingArray>
            """));

        // assert
        dataStorage.ElementsVersion.Should().Be(0);
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
        return dataStorage.ElementCache.StormXmlElementsByIdByCatalog.Dictionary[catalogName];
    }

    private static Dictionary<string, List<LevelScalingFieldValue>> GetScalingFieldValuesById(DataStorage dataStorage, string catalogName)
    {
        return dataStorage.ElementCache.ScalingFieldValueByIdByCatalog.Dictionary[catalogName].Dictionary;
    }

    private static List<LevelScalingFieldValue> GetScalingFieldValues(DataStorage dataStorage, string catalogName, string id)
    {
        return GetScalingFieldValuesById(dataStorage, catalogName)[id];
    }

    private static StormXmlArray CreateLevelScalingArray(string levelScalingArrayXml)
    {
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement($"""<CHero id="Test">{levelScalingArrayXml}</CHero>""");

        return element.Members.GetMember("LevelScalingArray");
    }

    private static string CreateModificationsXml(string catalog, string entry, string field, string value, string? excludedMember = null, string? memberWithNoValue = null)
    {
        (string Name, string Value)[] members = [("Catalog", catalog), ("Entry", entry), ("Field", field), ("Value", value)];

        string membersXml = string.Concat(members
            .Where(x => x.Name != excludedMember)
            .Select(x => x.Name == memberWithNoValue ? $"<{x.Name} />" : $"""<{x.Name} value="{x.Value}" />"""));

        return $"<Modifications>{membersXml}</Modifications>";
    }
}
