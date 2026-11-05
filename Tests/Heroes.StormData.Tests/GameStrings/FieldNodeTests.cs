namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
internal sealed class FieldNodeTests
{
    [TestMethod]
    public void Default_IsEmptyAndHasNoValue()
    {
        // arrange
        FieldNode fieldNode = default;

        // act
        bool isEmpty = fieldNode.IsEmpty;
        StormXmlValue value = fieldNode.Value;

        // assert
        isEmpty.Should().BeTrue();
        value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void Default_TryGetMember_ReturnsFalse()
    {
        // arrange
        FieldNode fieldNode = default;

        // act
        bool result = fieldNode.TryGetMember("Amount", out StormXmlArray? array);

        // assert
        result.Should().BeFalse();
        array.Should().BeNull();
    }

    [TestMethod]
    public void MemberConstructor_IsNotEmpty()
    {
        // arrange
        StormXmlElement element = CreateElement("""<AmountArray value="120" />""");

        // act
        FieldNode fieldNode = new(element.Members["AmountArray"].ByIndex[0]);

        // assert
        fieldNode.IsEmpty.Should().BeFalse();
    }

    [TestMethod]
    public void MemberConstructor_Value_ReturnsMemberValue()
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <AmountArray value="120" />
            <AmountArray value="140" />
            """);

        FieldNode fieldNode = new(element.Members["AmountArray"].ByIndex[1]);

        // act
        StormXmlValue value = fieldNode.Value;

        // assert
        value.HasValue.Should().BeTrue();
        value.RawValue.Should().Be("140");
    }

    [TestMethod]
    public void MemberConstructor_MemberWithNoValueAttribute_HasNoValue()
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <CostArray>
              <CooldownTimeUse value="5" />
            </CostArray>
            """);

        FieldNode fieldNode = new(element.Members["CostArray"].ByIndex[0]);

        // act
        StormXmlValue value = fieldNode.Value;

        // assert
        value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void MemberConstructor_TryGetMember_ExistingMember_ReturnsSameArray()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members);

        // act
        bool result = fieldNode.TryGetMember("Amount", out StormXmlArray? array);

        // assert
        result.Should().BeTrue();
        array.Should().BeSameAs(element.Members["Amount"]);
        array.Value.RawValue.Should().Be("120");
    }

    [TestMethod]
    public void MemberConstructor_TryGetMember_MissingMember_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members);

        // act
        bool result = fieldNode.TryGetMember("DoesNotExist", out StormXmlArray? array);

        // assert
        result.Should().BeFalse();
        array.Should().BeNull();
    }

    [TestMethod]
    public void MemberConstructor_TryGetMember_NameWithDifferentCasing_ReturnsFalse()
    {
        // arrange - member names are stored with StringComparer.Ordinal
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members);

        // act
        bool result = fieldNode.TryGetMember("AMOUNT", out StormXmlArray? array);

        // assert
        result.Should().BeFalse();
        array.Should().BeNull();
    }

    [TestMethod]
    public void MemberConstructor_TryGetMember_NameAsSpanSlice_ReturnsArray()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members);

        ReadOnlySpan<char> name = "xxAmountyy".AsSpan(2, 6);

        // act
        bool result = fieldNode.TryGetMember(name, out StormXmlArray? array);

        // assert
        result.Should().BeTrue();
        array.Should().BeSameAs(element.Members["Amount"]);
    }

    [TestMethod]
    public void ArrayConstructor_IsNotEmpty()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        // act
        FieldNode fieldNode = new(element.Members["Amount"]);

        // assert
        fieldNode.IsEmpty.Should().BeFalse();
    }

    [TestMethod]
    public void ArrayConstructor_SingleMember_Value_ReturnsValue()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members["Amount"]);

        // act
        StormXmlValue value = fieldNode.Value;

        // assert
        value.HasValue.Should().BeTrue();
        value.RawValue.Should().Be("120");
    }

    [TestMethod]
    public void ArrayConstructor_Attribute_Value_ReturnsAttributeValue()
    {
        // arrange - an attribute on the element becomes a single member with only a value
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement("""<CEffectDamage id="TestDamage" Kind="Spell" />""");

        FieldNode fieldNode = new(element.Members["Kind"]);

        // act
        StormXmlValue value = fieldNode.Value;

        // assert
        value.RawValue.Should().Be("Spell");
    }

    [TestMethod]
    public void ArrayConstructor_IsArray_HasNoValue()
    {
        // arrange - an array has no value of its own, only its members do
        StormXmlElement element = CreateElement("""<AmountArray value="120" />""");

        FieldNode fieldNode = new(element.Members["AmountArray"]);

        // act
        StormXmlValue value = fieldNode.Value;

        // assert
        value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void ArrayConstructor_SingleMember_TryGetMember_ReturnsNestedMember()
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <Cost>
              <CooldownTimeUse value="5" />
            </Cost>
            """);

        FieldNode fieldNode = new(element.Members["Cost"]);

        // act
        bool result = fieldNode.TryGetMember("CooldownTimeUse", out StormXmlArray? array);

        // assert
        result.Should().BeTrue();
        array.Should().BeSameAs(element.Members["Cost"].ByMember["CooldownTimeUse"]);
        array.Value.RawValue.Should().Be("5");
    }

    [TestMethod]
    public void ArrayConstructor_IsArray_TryGetMember_ReturnsFalse()
    {
        // arrange - members of an array are reached through an index, not by name
        StormXmlElement element = CreateElement("""
            <CostArray>
              <CooldownTimeUse value="5" />
            </CostArray>
            """);

        FieldNode fieldNode = new(element.Members["CostArray"]);

        // act
        bool result = fieldNode.TryGetMember("CooldownTimeUse", out StormXmlArray? array);

        // assert
        result.Should().BeFalse();
        array.Should().BeNull();
    }

    [TestMethod]
    public void ArrayConstructor_Attribute_TryGetMember_ReturnsFalse()
    {
        // arrange - an attribute has a value but no members
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement("""<CEffectDamage id="TestDamage" Kind="Spell" />""");

        FieldNode fieldNode = new(element.Members["Kind"]);

        // act
        bool result = fieldNode.TryGetMember("Kind", out StormXmlArray? array);

        // assert
        result.Should().BeFalse();
        array.Should().BeNull();
    }

    [TestMethod]
    public void IsSameNode_SameMember_ReturnsTrue()
    {
        // arrange
        StormXmlElement element = CreateElement("""<AmountArray value="120" />""");
        StormXmlMember member = element.Members["AmountArray"].ByIndex[0];

        FieldNode fieldNode = new(member);
        FieldNode otherFieldNode = new(member);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsSameNode_SameArray_ReturnsTrue()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");
        StormXmlArray array = element.Members["Amount"];

        FieldNode fieldNode = new(array);
        FieldNode otherFieldNode = new(array);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsSameNode_DifferentMembersOfSameArray_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <AmountArray value="120" />
            <AmountArray value="120" />
            """);

        FieldNode fieldNode = new(element.Members["AmountArray"].ByIndex[0]);
        FieldNode otherFieldNode = new(element.Members["AmountArray"].ByIndex[1]);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert - same value, but different instances
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSameNode_DifferentArrays_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <Amount value="120" />
            <Radius value="120" />
            """);

        FieldNode fieldNode = new(element.Members["Amount"]);
        FieldNode otherFieldNode = new(element.Members["Radius"]);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSameNode_ArrayAndItsMember_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = CreateElement("""<AmountArray value="120" />""");

        FieldNode arrayFieldNode = new(element.Members["AmountArray"]);
        FieldNode memberFieldNode = new(element.Members["AmountArray"].ByIndex[0]);

        // act
        bool result = arrayFieldNode.IsSameNode(memberFieldNode);
        bool reverseResult = memberFieldNode.IsSameNode(arrayFieldNode);

        // assert
        result.Should().BeFalse();
        reverseResult.Should().BeFalse();
    }

    [TestMethod]
    public void IsSameNode_SameXmlParsedTwice_ReturnsFalse()
    {
        // arrange - the comparison is by instance, not by value
        StormXmlElement element = CreateElement("""<Amount value="120" />""");
        StormXmlElement otherElement = CreateElement("""<Amount value="120" />""");

        FieldNode fieldNode = new(element.Members["Amount"]);
        FieldNode otherFieldNode = new(otherElement.Members["Amount"]);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSameNode_BothDefault_ReturnsFalse()
    {
        // arrange
        FieldNode fieldNode = default;
        FieldNode otherFieldNode = default;

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert - two empty nodes are not a match
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsSameNode_DefaultAndNotEmpty_ReturnsFalse()
    {
        // arrange
        StormXmlElement element = CreateElement("""<Amount value="120" />""");

        FieldNode emptyFieldNode = default;
        FieldNode fieldNode = new(element.Members["Amount"]);

        // act
        bool result = emptyFieldNode.IsSameNode(fieldNode);
        bool reverseResult = fieldNode.IsSameNode(emptyFieldNode);

        // assert
        result.Should().BeFalse();
        reverseResult.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("AmountArray", "AmountArray[0]")]
    [DataRow("AmountArray[0]", "AmountArray[0]")]
    [DataRow("Cost.CooldownTimeUse", "Cost.CooldownTimeUse")]
    public void IsSameNode_FieldsResolvingToSameNode_ReturnsTrue(string field, string otherField)
    {
        // arrange - different spellings of a field that resolve to the same node
        StormXmlElement element = CreateElement("""
            <AmountArray value="120" />
            <Cost>
              <CooldownTimeUse value="5" />
            </Cost>
            """);

        FieldNode fieldNode = DataRefFieldParser.ParseField(element, field);
        FieldNode otherFieldNode = DataRefFieldParser.ParseField(element, otherField);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("AmountArray[0]", "AmountArray[1]")]
    [DataRow("AmountArray[0]", "Radius")]
    [DataRow("DoesNotExist", "AlsoDoesNotExist")]
    public void IsSameNode_FieldsResolvingToDifferentNodes_ReturnsFalse(string field, string otherField)
    {
        // arrange
        StormXmlElement element = CreateElement("""
            <AmountArray value="120" />
            <AmountArray value="120" />
            <Radius value="120" />
            """);

        FieldNode fieldNode = DataRefFieldParser.ParseField(element, field);
        FieldNode otherFieldNode = DataRefFieldParser.ParseField(element, otherField);

        // act
        bool result = fieldNode.IsSameNode(otherFieldNode);

        // assert
        result.Should().BeFalse();
    }

    // wraps the inner xml in an element with an id
    private static StormXmlElement CreateElement(string innerXml)
    {
        return XmlReaderHelper.GetStormXmlElement($"""<CEffectDamage id="TestDamage">{innerXml}</CEffectDamage>""");
    }
}
