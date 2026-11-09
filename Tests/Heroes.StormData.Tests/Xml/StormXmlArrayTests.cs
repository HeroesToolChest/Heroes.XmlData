namespace Heroes.StormData.Xml.Tests;

[TestClass]
public class StormXmlArrayTests
{
    [TestMethod]
    public void GetIndexes_NoIndex_ReturnsNumericalIndexesInOrder()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));

        // act
        IReadOnlyList<string> indexes = parent["Item"].GetIndexes();

        // assert
        indexes.Should().Equal("0", "1", "2");
    }

    [TestMethod]
    public void GetIndexes_NumericalIndexWithGap_IncludesPlaceholderIndexes()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"), "2");

        // act
        IReadOnlyList<string> indexes = parent["Item"].GetIndexes();

        // assert
        indexes.Should().Equal("0", "1", "2");
    }

    [TestMethod]
    public void GetIndexes_NamedIndexes_ReturnsNamesInInsertionOrder()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags"), "DisableWhileDead");
        parent.Add(CreateArrayMember("Flags"), "Link");

        // act
        IReadOnlyList<string> indexes = parent["Flags"].GetIndexes();

        // assert
        indexes.Should().Equal("DisableWhileDead", "Link");
    }

    [TestMethod]
    public void GetIndexes_ExistingNamedIndexReplaced_ReturnsNameOnce()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags"), "Link");
        parent.Add(CreateArrayMember("Flags"), "Link");

        // act
        IReadOnlyList<string> indexes = parent["Flags"].GetIndexes();

        // assert
        indexes.Should().Equal("Link");
    }

    [TestMethod]
    public void GetIndexes_MixedNamedAndNumericalIndexes_ReturnsNameOrNumberPerPosition()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"), "A");
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"), "B");

        // act
        IReadOnlyList<string> indexes = parent["Item"].GetIndexes();

        // assert
        indexes.Should().Equal("A", "1", "B");
    }

    [TestMethod]
    public void GetIndexes_EachIndex_ResolvesThroughByIndex()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item", ("X", "1"));
        StormXmlMember second = CreateArrayMember("Item", ("X", "2"));
        StormXmlMember third = CreateArrayMember("Item", ("X", "3"));

        parent.Add(first, "A");
        parent.Add(second);
        parent.Add(third, "B");

        StormXmlArray array = parent["Item"];

        // act
        List<StormXmlMember> members = [];

        foreach (string index in array.GetIndexes())
            members.Add(array.ByIndex[index]);

        // assert
        members.Should().Equal(first, second, third);
    }

    [TestMethod]
    public void GetIndexes_AfterMerge_ReturnsIndexesFromBoth()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Flags", ("X", "1")), "A");

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Flags", ("X", "2")), "A");
        other.Add(CreateArrayMember("Flags", ("X", "3")), "B");

        // act
        member.Merge(other);

        // assert
        member["Flags"].GetIndexes().Should().Equal("A", "B");
    }

    [TestMethod]
    public void GetIndexes_NotAnArray_ReturnsEmpty()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Child", ("Attr", "x")));

        // act
        IReadOnlyList<string> indexes = parent["Child"].GetIndexes();

        // assert
        indexes.Should().BeEmpty();
    }

    [TestMethod]
    public void GetIndexes_EmptyArray_ReturnsEmpty()
    {
        // arrange
        StormXmlArray array = new(true);

        // act
        IReadOnlyList<string> indexes = array.GetIndexes();

        // assert
        indexes.Should().BeEmpty();
    }

    [TestMethod]
    public void Value_NoValueSet_HasNoValue()
    {
        // arrange
        StormXmlArray array = new(true);

        // act
        StormXmlValue value = array.Value;

        // assert
        value.HasValue.Should().BeFalse();
        value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void Value_Attribute_ReturnsAttributeValue()
    {
        // arrange
        StormXmlMember parent = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        StormXmlValue value = parent["Link"].Value;

        // assert
        value.HasValue.Should().BeTrue();
        value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void Value_SingleElementWithValueAttribute_ReturnsElementValue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Amount", ("value", "5")));

        // act
        StormXmlValue value = parent["Amount"].Value;

        // assert
        value.HasValue.Should().BeTrue();
        value.RawValue.Should().Be("5");
    }

    [TestMethod]
    public void Value_ArrayElementWithValueAttribute_HasNoValue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        StormXmlValue value = parent["Flags"].Value;

        // assert
        value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void IsArray_ArrayElement_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags"), "Link");

        // act
        bool isArray = parent["Flags"].IsArray;

        // assert
        isArray.Should().BeTrue();
    }

    [TestMethod]
    public void IsArray_SingleElement_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool isArray = parent["Cost"].IsArray;

        // assert
        isArray.Should().BeFalse();
    }

    [TestMethod]
    public void IsArray_Attribute_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool isArray = parent["Link"].IsArray;

        // assert
        isArray.Should().BeFalse();
    }

    [TestMethod]
    public void GetMember_String_SingleElement_ReturnsChildMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        StormXmlArray result = parent["Cost"].GetMember("Charge");

        // assert
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void GetMember_String_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlArray> act = () => parent["Cost"].GetMember("Missing");

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void GetMember_String_Array_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlArray> act = () => parent["Flags"].GetMember("value");

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void GetMember_String_Attribute_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        Func<StormXmlArray> act = () => parent["Link"].GetMember("Missing");

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void GetMember_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));
        string name = null!;

        // act
        Func<StormXmlArray> act = () => parent["Cost"].GetMember(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetMember_Span_SingleElement_ReturnsChildMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        StormXmlArray result = parent["Cost"].GetMember("Charge".AsSpan());

        // assert
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void GetMember_Span_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlArray> act = () => parent["Cost"].GetMember("Missing".AsSpan());

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void GetMember_Span_Array_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlArray> act = () => parent["Flags"].GetMember("value".AsSpan());

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void TryGetMember_String_SingleElement_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetMember("Charge", out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void TryGetMember_String_MissingMember_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetMember("Missing", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_DifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetMember("charge", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_Array_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool found = parent["Flags"].TryGetMember("value", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_Attribute_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = parent["Link"].TryGetMember("Missing", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));
        string name = null!;

        // act
        Func<bool> act = () => parent["Cost"].TryGetMember(name, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void TryGetMember_Span_SingleElement_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetMember("Charge".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void TryGetMember_Span_MissingMember_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetMember("Missing".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_Span_Array_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool found = parent["Flags"].TryGetMember("value".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetByIndex_Int_ExistingIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        StormXmlMember result = parent["Item"].GetByIndex(1);

        // assert
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    public void GetByIndex_Int_PositionOfNamedIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember a = CreateArrayMember("Flags");
        StormXmlMember b = CreateArrayMember("Flags");
        parent.Add(a, "A");
        parent.Add(b, "B");

        // act
        StormXmlMember result = parent["Flags"].GetByIndex(1);

        // assert
        result.Should().BeSameAs(b);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void GetByIndex_Int_OutOfRange_ThrowsKeyNotFoundException(int index)
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        Func<StormXmlMember> act = () => parent["Item"].GetByIndex(index);

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains($"'{index}'"));
    }

    [TestMethod]
    public void GetByIndex_Int_NotAnArray_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlMember> act = () => parent["Cost"].GetByIndex(0);

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void GetByIndex_String_NamedIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        StormXmlMember result = parent["Flags"].GetByIndex("Link");

        // assert
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void GetByIndex_String_NumericalIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        StormXmlMember result = parent["Item"].GetByIndex("1");

        // assert
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    public void GetByIndex_String_MissingNamedIndex_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlMember> act = () => parent["Flags"].GetByIndex("Missing");

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void GetByIndex_String_NotAnArray_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlMember> act = () => parent["Cost"].GetByIndex("0");

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void GetByIndex_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        string name = null!;

        // act
        Func<StormXmlMember> act = () => parent["Item"].GetByIndex(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetByIndex_Span_NamedIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        StormXmlMember result = parent["Flags"].GetByIndex("Link".AsSpan());

        // assert
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void GetByIndex_Span_NumericalIndex_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        StormXmlMember result = parent["Item"].GetByIndex("1".AsSpan());

        // assert
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    public void GetByIndex_Span_MissingNamedIndex_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlMember> act = () => parent["Flags"].GetByIndex("Missing".AsSpan());

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void TryGetByIndex_Int_ExistingIndex_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        bool found = parent["Item"].TryGetByIndex(1, out StormXmlMember? result);

        // assert
        found.Should().BeTrue();
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void TryGetByIndex_Int_OutOfRange_ReturnsFalseAndNull(int index)
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        bool found = parent["Item"].TryGetByIndex(index, out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_Int_NotAnArray_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetByIndex(0, out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_String_NamedIndex_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        bool found = parent["Flags"].TryGetByIndex("Link", out StormXmlMember? result);

        // assert
        found.Should().BeTrue();
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void TryGetByIndex_String_NumericalIndex_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        bool found = parent["Item"].TryGetByIndex("1", out StormXmlMember? result);

        // assert
        found.Should().BeTrue();
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    [DataRow("-1")]
    [DataRow("1")]
    public void TryGetByIndex_String_NumericalIndexOutOfRange_ReturnsFalseAndNull(string index)
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        bool found = parent["Item"].TryGetByIndex(index, out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_String_MissingNamedIndex_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool found = parent["Flags"].TryGetByIndex("Missing", out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_String_NamedIndexDifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool found = parent["Flags"].TryGetByIndex("link", out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_String_NotAnArray_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetByIndex("0", out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        string name = null!;

        // act
        Func<bool> act = () => parent["Item"].TryGetByIndex(name, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void TryGetByIndex_Span_NamedIndex_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        bool found = parent["Flags"].TryGetByIndex("Link".AsSpan(), out StormXmlMember? result);

        // assert
        found.Should().BeTrue();
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void TryGetByIndex_Span_NumericalIndex_ReturnsTrueAndMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        bool found = parent["Item"].TryGetByIndex("1".AsSpan(), out StormXmlMember? result);

        // assert
        found.Should().BeTrue();
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    public void TryGetByIndex_Span_MissingNamedIndex_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool found = parent["Flags"].TryGetByIndex("Missing".AsSpan(), out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetByIndex_Span_NotAnArray_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool found = parent["Cost"].TryGetByIndex("0".AsSpan(), out StormXmlMember? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void ContainsIndex_Int_ExistingIndex_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));

        // act
        bool contains = parent["Item"].ContainsIndex(1);

        // assert
        contains.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(1)]
    public void ContainsIndex_Int_OutOfRange_ReturnsFalse(int index)
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        bool contains = parent["Item"].ContainsIndex(index);

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_Int_NotAnArray_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool contains = parent["Cost"].ContainsIndex(0);

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_String_NamedIndex_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool contains = parent["Flags"].ContainsIndex("Link");

        // assert
        contains.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsIndex_String_NumericalIndex_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));

        // act
        bool contains = parent["Item"].ContainsIndex("1");

        // assert
        contains.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("-1")]
    [DataRow("1")]
    public void ContainsIndex_String_NumericalIndexOutOfRange_ReturnsFalse(string index)
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        bool contains = parent["Item"].ContainsIndex(index);

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_String_MissingNamedIndex_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool contains = parent["Flags"].ContainsIndex("Missing");

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_String_NotAnArray_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool contains = parent["Cost"].ContainsIndex("0");

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        string name = null!;

        // act
        Func<bool> act = () => parent["Item"].ContainsIndex(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ContainsIndex_Span_NamedIndex_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool contains = parent["Flags"].ContainsIndex("Link".AsSpan());

        // assert
        contains.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsIndex_Span_NumericalIndex_ReturnsTrue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));

        // act
        bool contains = parent["Item"].ContainsIndex("1".AsSpan());

        // assert
        contains.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsIndex_Span_MissingNamedIndex_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        bool contains = parent["Flags"].ContainsIndex("Missing".AsSpan());

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIndex_Span_NotAnArray_ReturnsFalse()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        bool contains = parent["Cost"].ContainsIndex("0".AsSpan());

        // assert
        contains.Should().BeFalse();
    }

    [TestMethod]
    public void ByIndex_Int_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");
        parent.Add(first);
        parent.Add(second);

        // act
        StormXmlMember result = parent["Item"].ByIndex[1];

        // assert
        result.Should().BeSameAs(second);
    }

    [TestMethod]
    public void ByIndex_String_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        StormXmlMember result = parent["Flags"].ByIndex["Link"];

        // assert
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void ByIndex_Span_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember link = CreateArrayMember("Flags", ("value", "1"));
        parent.Add(link, "Link");

        // act
        StormXmlMember result = parent["Flags"].ByIndex["Link".AsSpan()];

        // assert
        result.Should().BeSameAs(link);
    }

    [TestMethod]
    public void ByIndex_Int_MissingIndex_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));

        // act
        Func<StormXmlMember> act = () => parent["Item"].ByIndex[1];

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void ByIndex_String_MissingIndex_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlMember> act = () => parent["Flags"].ByIndex["Missing"];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void ByIndex_Span_MissingIndex_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlMember> act = () => parent["Flags"].ByIndex["Missing".AsSpan()];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void ByIndex_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Item"));
        string name = null!;

        // act
        Func<StormXmlMember> act = () => parent["Item"].ByIndex[name];

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ByMember_String_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        StormXmlArray result = parent["Cost"].ByMember["Charge"];

        // assert
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void ByMember_Span_ReturnsMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        StormXmlArray result = parent["Cost"].ByMember["Charge".AsSpan()];

        // assert
        result.Value.RawValue.Should().Be("10");
    }

    [TestMethod]
    public void ByMember_String_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlArray> act = () => parent["Cost"].ByMember["Missing"];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void ByMember_Span_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));

        // act
        Func<StormXmlArray> act = () => parent["Cost"].ByMember["Missing".AsSpan()];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void ByMember_String_Array_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateArrayMember("Flags", ("value", "1")), "Link");

        // act
        Func<StormXmlArray> act = () => parent["Flags"].ByMember["value"];

        // assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void ByMember_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "10")));
        string name = null!;

        // act
        Func<StormXmlArray> act = () => parent["Cost"].ByMember[name];

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Value_RepeatedSingleElement_ReturnsLastValue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Amount", ("value", "1")));

        // act
        parent.Add(CreateMember("Amount", ("value", "2")));

        // assert
        StormXmlArray amount = parent["Amount"];

        amount.Value.RawValue.Should().Be("2");
        amount.ByMember["value"].Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Value_RepeatedSingleElementLastHasNoValue_KeepsEarlierValue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Amount", ("value", "1")));

        // act
        parent.Add(CreateMember("Amount"));

        // assert
        StormXmlArray amount = parent["Amount"];

        amount.Value.RawValue.Should().Be("1");
        amount.ByMember["value"].Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void GetMember_RepeatedSingleElement_MergesLastOverEarlier()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        parent.Add(CreateMember("Cost", ("Charge", "A"), ("Amount", "1")));

        // act
        parent.Add(CreateMember("Cost", ("Amount", "2"), ("Cooldown", "5")));

        // assert
        StormXmlArray cost = parent["Cost"];

        cost.IsArray.Should().BeFalse();
        cost.GetMember("Charge").Value.RawValue.Should().Be("A");
        cost.GetMember("Amount").Value.RawValue.Should().Be("2");
        cost.GetMember("Cooldown").Value.RawValue.Should().Be("5");
    }

    [TestMethod]
    public void GetMember_RepeatedSingleElementWithInnerElements_MergesInnerElements()
    {
        // arrange
        StormXmlMember parent = new("Parent");

        StormXmlMember firstCost = new("Cost");
        firstCost.Add(CreateMember("Charge", ("Link", "A"), ("CountMax", "1")));
        parent.Add(firstCost);

        StormXmlMember secondCost = new("Cost");
        secondCost.Add(CreateMember("Charge", ("CountMax", "3")));

        // act
        parent.Add(secondCost);

        // assert
        StormXmlArray charge = parent["Cost"].ByMember["Charge"];

        charge.GetMember("Link").Value.RawValue.Should().Be("A");
        charge.GetMember("CountMax").Value.RawValue.Should().Be("3");
    }

    [TestMethod]
    public void AddMember_RepeatedArrayElement_StillAppends()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item", ("X", "1"));
        StormXmlMember second = CreateArrayMember("Item", ("X", "2"));
        parent.Add(first);

        // act
        parent.Add(second);

        // assert
        StormXmlArray items = parent["Item"];

        items.GetIndexes().Should().Equal("0", "1");
        items.ByIndex[0].Should().BeSameAs(first);
        items.ByIndex[1].Should().BeSameAs(second);
        first.GetMember("X").Value.RawValue.Should().Be("1");
    }

    // creates a member with the given attributes, using itself as the root for token replacement
    private static StormXmlMember CreateMember(string name, params (string Name, string Value)[] attributes)
    {
        StormXmlMember member = new(name);

        foreach ((string attributeName, string attributeValue) in attributes)
            member.Add(attributeName, attributeValue, member);

        return member;
    }

    private static StormXmlMember CreateArrayMember(string name, params (string Name, string Value)[] attributes)
    {
        StormXmlMember member = CreateMember(name, attributes);
        member.IsArray = true;

        return member;
    }
}
