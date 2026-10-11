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

    private static StormXmlConstElement CreateStormXmlConstElement(string id, string value = "1")
    {
        return XmlReaderHelper.GetStormXmlConstElement($"""<const id="{id}" value="{value}" />""");
    }
}
