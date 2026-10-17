namespace Heroes.StormData.Xml.Tests;

[TestClass]
public sealed class StormXmlMemberTests
{
    [TestMethod]
    public void Constructor_SetsName()
    {
        // arrange & act
        StormXmlMember member = new("CAbil");

        // assert
        member.Name.Should().Be("CAbil");
    }

    [TestMethod]
    public void Constructor_Defaults_AreEmpty()
    {
        // arrange & act
        StormXmlMember member = new("CAbil");

        // assert
        member.Count.Should().Be(0);
        member.IsArray.Should().BeFalse();
        member.Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void AddAttribute_NewName_AddsMemberWithValue()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        member.Add("Link", "Abil/Test", member);

        // assert
        member.Count.Should().Be(1);
        member.ContainsMember("Link").Should().BeTrue();
        member.GetMember("Link").Value.RawValue.Should().Be("Abil/Test");
        member.GetMember("Link").IsArray.Should().BeFalse();
    }

    [TestMethod]
    public void AddAttribute_SameNameTwice_OverwritesValueAndKeepsCount()
    {
        // arrange
        StormXmlMember member = new("Element");
        member.Add("Link", "first", member);

        // act
        member.Add("Link", "second", member);

        // assert
        member.Count.Should().Be(1);
        member.GetMember("Link").Value.RawValue.Should().Be("second");
    }

    [TestMethod]
    public void AddAttribute_DifferentNames_CountsEachName()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        member.Add("A", "1", member);
        member.Add("B", "2", member);

        // assert
        member.Count.Should().Be(2);
    }

    [TestMethod]
    public void AddAttribute_NamesAreCaseSensitive()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        member.Add("value", "lower", member);
        member.Add("Value", "upper", member);

        // assert
        member.Count.Should().Be(2);
        member.GetMember("value").Value.RawValue.Should().Be("lower");
        member.GetMember("Value").Value.RawValue.Should().Be("upper");
    }

    [TestMethod]
    public void AddAttribute_ValueName_SetsMemberValue()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        member.Add("value", "100", member);

        // assert
        member.Value.HasValue.Should().BeTrue();
        member.Value.RawValue.Should().Be("100");
    }

    [TestMethod]
    public void AddAttribute_ValueNameTwice_UpdatesMemberValue()
    {
        // arrange
        StormXmlMember member = new("Element");
        member.Add("value", "100", member);

        // act
        member.Add("value", "200", member);

        // assert
        member.Value.RawValue.Should().Be("200");
    }

    [TestMethod]
    [DataRow("Value")]
    [DataRow("VALUE")]
    [DataRow("Link")]
    public void AddAttribute_NotExactlyValueName_DoesNotSetMemberValue(string name)
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        member.Add(name, "100", member);

        // assert
        member.Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void AddAttribute_ValueWithToken_ResolvesAgainstRootMember()
    {
        // arrange
        StormXmlMember root = CreateMember("Root", ("id", "Jaina"));
        StormXmlMember member = new("Name");

        // act
        member.Add("value", "Abil/Name/##id##", root);

        // assert
        member.Value.RawValue.Should().Be("Abil/Name/##id##");
        member.Value.GetString().Should().Be("Abil/Name/Jaina");
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NoIndex_AddsMemberUnderItsName()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember child = CreateMember("Child", ("Attr", "x"));

        // act
        parent.Add(child);

        // assert
        parent.Count.Should().Be(1);
        parent.ContainsMember("Child").Should().BeTrue();
        parent["Child"].IsArray.Should().BeFalse();
        parent["Child"].GetMember("Attr").Value.RawValue.Should().Be("x");
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_SameNameMultipleTimes_CountsNameOnce()
    {
        // arrange
        StormXmlMember parent = new("Parent");

        // act
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));
        parent.Add(CreateArrayMember("Item"));

        // assert
        parent.Count.Should().Be(1);
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_DifferentNames_CountsEachName()
    {
        // arrange
        StormXmlMember parent = new("Parent");

        // act
        parent.Add(new StormXmlMember("First"));
        parent.Add(new StormXmlMember("Second"));

        // assert
        parent.Count.Should().Be(2);
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_ArrayMember_CreatesArrayEntry()
    {
        // arrange
        StormXmlMember parent = new("Parent");

        // act
        parent.Add(CreateArrayMember("Item"));

        // assert
        parent["Item"].IsArray.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NonArrayMemberWithValueAttribute_CopiesValueToEntry()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember child = CreateMember("Child", ("value", "5"));

        // act
        parent.Add(child);

        // assert
        parent["Child"].Value.HasValue.Should().BeTrue();
        parent["Child"].Value.RawValue.Should().Be("5");
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NonArrayMemberWithoutValueAttribute_EntryHasNoValue()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember child = CreateMember("Child", ("Other", "5"));

        // act
        parent.Add(child);

        // assert
        parent["Child"].Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_ArrayMemberWithValueAttribute_DoesNotCopyValueToEntry()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember child = CreateArrayMember("Child", ("value", "5"));

        // act
        parent.Add(child);

        // assert
        parent["Child"].Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NoIndexMultipleTimes_AppendsInOrder()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");

        // act
        parent.Add(first);
        parent.Add(second);

        // assert
        parent["Item"].ByIndex[0].Should().BeSameAs(first);
        parent["Item"].ByIndex[1].Should().BeSameAs(second);
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NumericalIndex_SetsMemberAtIndex()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember first = CreateArrayMember("Item");
        StormXmlMember second = CreateArrayMember("Item");

        // act
        parent.Add(first, "0");
        parent.Add(second, "1");

        // assert
        parent["Item"].ByIndex[0].Should().BeSameAs(first);
        parent["Item"].ByIndex[1].Should().BeSameAs(second);
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NumericalIndexWithGap_FillsGapWithPlaceholders()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember item = CreateArrayMember("Item", ("X", "1"));

        // act
        parent.Add(item, "2");

        // assert
        StormXmlArray array = parent["Item"];

        array.ContainsIndex(0).Should().BeTrue();
        array.ContainsIndex(1).Should().BeTrue();
        array.ContainsIndex(2).Should().BeTrue();
        array.ContainsIndex(3).Should().BeFalse();

        array.ByIndex[0].Name.Should().Be("Item");
        array.ByIndex[0].Count.Should().Be(0);
        array.ByIndex[1].Count.Should().Be(0);
        array.ByIndex[2].Should().BeSameAs(item);
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_ExistingNumericalIndex_ReplacesMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember original = CreateArrayMember("Item", ("X", "1"));
        StormXmlMember replacement = CreateArrayMember("Item", ("X", "2"));

        parent.Add(original, "0");

        // act
        parent.Add(replacement, "0");

        // assert
        parent["Item"].ByIndex[0].Should().BeSameAs(replacement);
        parent["Item"].ContainsIndex(1).Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NamedIndex_SetsMemberByName()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember flag = CreateArrayMember("Flags", ("value", "1"));

        // act
        parent.Add(flag, "DisableWhileDead");

        // assert
        parent["Flags"].ByIndex["DisableWhileDead"].Should().BeSameAs(flag);
        parent["Flags"].ContainsIndex("DisableWhileDead").Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_ExistingNamedIndex_ReplacesMember()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember original = CreateArrayMember("Flags", ("value", "1"));
        StormXmlMember replacement = CreateArrayMember("Flags", ("value", "0"));

        parent.Add(original, "Link");

        // act
        parent.Add(replacement, "Link");

        // assert
        parent["Flags"].ByIndex["Link"].Should().BeSameAs(replacement);
        parent["Flags"].ContainsIndex(1).Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Add(member, index)")]
    public void AddMember_NamedIndexes_AlsoGetPositionalIndexInInsertionOrder()
    {
        // arrange
        StormXmlMember parent = new("Parent");
        StormXmlMember a = CreateArrayMember("Flags");
        StormXmlMember b = CreateArrayMember("Flags");

        // act
        parent.Add(a, "A");
        parent.Add(b, "B");

        // assert
        parent["Flags"].ByIndex[0].Should().BeSameAs(a);
        parent["Flags"].ByIndex[1].Should().BeSameAs(b);
    }

    [TestMethod]
    public void Indexer_String_ExistingMember_ReturnsMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        StormXmlArray result = member["Link"];

        // assert
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void Indexer_String_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        Func<StormXmlArray> act = () => member["Missing"];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void Indexer_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember member = new("Element");
        string name = null!;

        // act
        Func<StormXmlArray> act = () => member[name];

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Indexer_Span_ExistingMember_ReturnsMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        StormXmlArray result = member["Link".AsSpan()];

        // assert
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void Indexer_Span_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        Func<StormXmlArray> act = () => member["Missing".AsSpan()];

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void GetMember_String_ExistingMember_ReturnsMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        StormXmlArray result = member.GetMember("Link");

        // assert
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void GetMember_String_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        Func<StormXmlArray> act = () => member.GetMember("Missing");

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void GetMember_Span_ExistingMember_ReturnsMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        StormXmlArray result = member.GetMember("Link".AsSpan());

        // assert
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void GetMember_Span_MissingMember_ThrowsKeyNotFoundException()
    {
        // arrange
        StormXmlMember member = new("Element");

        // act
        Func<StormXmlArray> act = () => member.GetMember("Missing".AsSpan());

        // assert
        act.Should().Throw<KeyNotFoundException>().Where(ex => ex.Message.Contains("Missing"));
    }

    [TestMethod]
    public void TryGetMember_String_ExistingMember_ReturnsTrue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = member.TryGetMember("Link", out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void TryGetMember_String_MissingMember_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = member.TryGetMember("Missing", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_DifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = member.TryGetMember("link", out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember member = new("Element");
        string name = null!;

        // act
        Func<bool> act = () => member.TryGetMember(name, out _);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void TryGetMember_Span_ExistingMember_ReturnsTrue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = member.TryGetMember("Link".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.RawValue.Should().Be("Abil/Test");
    }

    [TestMethod]
    public void TryGetMember_Span_MissingMember_ReturnsFalseAndNull()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool found = member.TryGetMember("Missing".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [TestMethod]
    public void TryGetMember_Span_SliceOfLargerString_FindsMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));
        const string text = "xxLinkyy";

        // act
        bool found = member.TryGetMember(text.AsSpan(2, 4), out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void ContainsMember_String_ExistingMember_ReturnsTrue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool result = member.ContainsMember("Link");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsMember_String_MissingMember_ReturnsFalse()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool result = member.ContainsMember("Missing");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsMember_String_Null_ThrowsArgumentNullException()
    {
        // arrange
        StormXmlMember member = new("Element");
        string name = null!;

        // act
        Func<bool> act = () => member.ContainsMember(name);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void ContainsMember_Span_ExistingMember_ReturnsTrue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool result = member.ContainsMember("Link".AsSpan());

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsMember_Span_MissingMember_ReturnsFalse()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "Abil/Test"));

        // act
        bool result = member.ContainsMember("Missing".AsSpan());

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Merge_SameInstance_DoesNothing()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("value", "1"), ("Link", "a"));

        // act
        member.Merge(member);

        // assert
        member.Count.Should().Be(2);
        member.Value.RawValue.Should().Be("1");
        member.GetMember("Link").Value.RawValue.Should().Be("a");
    }

    [TestMethod]
    public void Merge_OtherHasValue_ReplacesValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("value", "1"));
        StormXmlMember other = CreateMember("Element", ("value", "2"));

        // act
        member.Merge(other);

        // assert
        member.Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_OtherHasNoValue_KeepsExistingValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("value", "1"));
        StormXmlMember other = CreateMember("Element", ("Link", "a"));

        // act
        member.Merge(other);

        // assert
        member.Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void Merge_OtherHasValueAndThisHasNone_TakesOtherValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "a"));
        StormXmlMember other = CreateMember("Element", ("value", "2"));

        // act
        member.Merge(other);

        // assert
        member.Value.HasValue.Should().BeTrue();
        member.Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_DifferentName_TakesNameFromOther()
    {
        // arrange
        StormXmlMember member = new("First");
        StormXmlMember other = new("Second");

        // act
        member.Merge(other);

        // assert
        member.Name.Should().Be("Second");
    }

    [TestMethod]
    public void Merge_NewAttribute_IsAdded()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"));
        StormXmlMember other = CreateMember("Element", ("B", "2"));

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(2);
        member.GetMember("A").Value.RawValue.Should().Be("1");
        member.GetMember("B").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_ExistingAttribute_OverwritesValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "original"));
        StormXmlMember other = CreateMember("Element", ("Link", "overridden"));

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(1);
        member.GetMember("Link").Value.RawValue.Should().Be("overridden");
    }

    [TestMethod]
    public void Merge_OtherIsEmpty_LeavesMemberUnchanged()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"));
        StormXmlMember other = new("Element");

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(1);
        member.GetMember("A").Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void Merge_ArrayElementAtSameIndex_MergesIntoExistingElement()
    {
        // arrange
        StormXmlMember existingItem = CreateArrayMember("Items", ("X", "1"));
        StormXmlMember member = new("Parent");
        member.Add(existingItem, "0");

        StormXmlMember otherItem = CreateArrayMember("Items", ("Y", "2"));
        StormXmlMember other = new("Parent");
        other.Add(otherItem, "0");

        // act
        member.Merge(other);

        // assert
        StormXmlMember merged = member["Items"].ByIndex[0];

        merged.Should().BeSameAs(existingItem);
        merged.GetMember("X").Value.RawValue.Should().Be("1");
        merged.GetMember("Y").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_ArrayElementAtSameIndex_OverwritesConflictingAttribute()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Items", ("X", "original")), "0");

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Items", ("X", "overridden")), "0");

        // act
        member.Merge(other);

        // assert
        member["Items"].ByIndex[0].GetMember("X").Value.RawValue.Should().Be("overridden");
    }

    [TestMethod]
    public void Merge_ArrayElementAtNewIndex_AppendsElement()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Items", ("X", "1")), "0");

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Items", ("Y", "2")), "1");

        // act
        member.Merge(other);

        // assert
        StormXmlArray items = member["Items"];

        items.ContainsIndex(0).Should().BeTrue();
        items.ContainsIndex(1).Should().BeTrue();
        items.ContainsIndex(2).Should().BeFalse();

        items.ByIndex[0].ContainsMember("X").Should().BeTrue();
        items.ByIndex[0].ContainsMember("Y").Should().BeFalse();
        items.ByIndex[1].GetMember("Y").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_NamedIndexElement_MergesIntoExistingElementWithSameName()
    {
        // arrange
        StormXmlMember existingFlag = CreateArrayMember("Flags", ("X", "1"));
        StormXmlMember member = new("Parent");
        member.Add(existingFlag, "Link");

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Flags", ("Y", "2")), "Link");

        // act
        member.Merge(other);

        // assert
        StormXmlMember merged = member["Flags"].ByIndex["Link"];

        merged.Should().BeSameAs(existingFlag);
        merged.GetMember("X").Value.RawValue.Should().Be("1");
        merged.GetMember("Y").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_NewNamedIndexElement_IsAddedAndExistingOneIsKept()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Flags", ("X", "1")), "A");

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Flags", ("Y", "2")), "B");

        // act
        member.Merge(other);

        // assert
        StormXmlArray flags = member["Flags"];

        flags.ByIndex["A"].GetMember("X").Value.RawValue.Should().Be("1");
        flags.ByIndex["B"].GetMember("Y").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_NewArrayMember_IsAdded()
    {
        // arrange
        StormXmlMember member = CreateMember("Parent", ("A", "1"));

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Items", ("Y", "2")), "0");

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(2);
        member["Items"].IsArray.Should().BeTrue();
        member["Items"].ByIndex[0].GetMember("Y").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Clone_ChildMembers_AreDeepCopied()
    {
        // arrange
        StormXmlMember item = CreateArrayMember("Items", ("X", "1"));
        StormXmlMember member = new("Parent");
        member.Add(item, "0");

        // act
        StormXmlMember clone = member.Clone();

        // assert
        StormXmlMember clonedItem = clone["Items"].ByIndex[0];

        clonedItem.Should().NotBeSameAs(item);
        clonedItem.Name.Should().Be("Items");
        clonedItem.IsArray.Should().BeTrue();
        clonedItem.GetMember("X").Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void Clone_ChildMembers_ModifyingCloneDoesNotAffectOriginal()
    {
        // arrange
        StormXmlMember item = CreateArrayMember("Items", ("X", "1"));
        StormXmlMember member = new("Parent");
        member.Add(item, "0");

        StormXmlMember clone = member.Clone();

        // act
        clone["Items"].ByIndex[0].Add("X", "changed", clone);

        // assert
        item.GetMember("X").Value.RawValue.Should().Be("1");
        clone["Items"].ByIndex[0].GetMember("X").Value.RawValue.Should().Be("changed");
    }

    [TestMethod]
    public void Clone_NamedIndexes_ArePreserved()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Flags", ("X", "1")), "A");
        member.Add(CreateArrayMember("Flags", ("X", "2")), "B");

        // act
        StormXmlMember clone = member.Clone();

        // assert
        clone["Flags"].ByIndex["A"].GetMember("X").Value.RawValue.Should().Be("1");
        clone["Flags"].ByIndex["B"].GetMember("X").Value.RawValue.Should().Be("2");
        clone["Flags"].ByIndex[1].GetMember("X").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Clone_ValueWithToken_ResolvesAgainstCloneInsteadOfOriginal()
    {
        // arrange
        StormXmlMember member = new("Name");
        member.Add("a", "A", member);
        member.Add("value", "##a##", member);

        StormXmlMember clone = member.Clone();

        // act
        clone.Add("a", "B", clone);

        // assert
        member.Value.GetString().Should().Be("A");
        clone.Value.GetString().Should().Be("B");
    }

    [TestMethod]
    public void Clone_WithExplicitRootMember_ValueResolvesAgainstThatRoot()
    {
        // arrange
        StormXmlMember root = CreateMember("Root", ("a", "FromRoot"));
        StormXmlMember member = new("Name");
        member.Add("value", "##a##", member);

        // act
        StormXmlMember clone = member.Clone(root);

        // assert
        clone.Value.RawValue.Should().Be("##a##");
        clone.Value.GetString().Should().Be("FromRoot");
    }

    [TestMethod]
    public void Merge_NewAttribute_DoesNotShareArrayInstanceWithOther()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"));
        StormXmlMember other = CreateMember("Element", ("B", "2"));

        // act
        member.Merge(other);

        // assert
        member.GetMember("B").Should().NotBeSameAs(other.GetMember("B"));
    }

    [TestMethod]
    public void Merge_NewAttribute_ChangingOtherAfterMergeDoesNotAffectMergedMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"));
        StormXmlMember other = CreateMember("Element", ("B", "2"));

        member.Merge(other);

        // act
        other.Add("B", "changed", other);

        // assert
        member.GetMember("B").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Merge_NewArrayMember_AddingToOtherAfterMergeDoesNotAffectMergedMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Parent", ("A", "1"));

        StormXmlMember other = new("Parent");
        other.Add(CreateArrayMember("Items", ("X", "1")), "0");

        member.Merge(other);

        // act
        other.Add(CreateArrayMember("Items", ("X", "2")), "1");

        // assert
        member["Items"].ContainsIndex(1).Should().BeFalse();
    }

    [TestMethod]
    public void Merge_OtherValueWithToken_ResolvesAgainstMergedMemberInsteadOfOther()
    {
        // arrange
        StormXmlMember member = CreateMember("Name", ("id", "MemberId"));

        StormXmlMember other = new("Name");
        other.Add("value", "Abil/Name/##id##", other);

        // act
        member.Merge(other);

        // assert
        member.Value.GetString().Should().Be("Abil/Name/MemberId");
    }

    [TestMethod]
    public void Remove_ExistingMember_ReturnsTrueAndRemovesMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"), ("B", "2"));

        // act
        bool result = member.Remove("A");

        // assert
        result.Should().BeTrue();
        member.Count.Should().Be(1);
        member.ContainsMember("A").Should().BeFalse();
        member.TryGetMember("A", out _).Should().BeFalse();
    }

    [TestMethod]
    public void Remove_ExistingMember_LeavesOtherMembersUnchanged()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"), ("B", "2"));

        // act
        member.Remove("A");

        // assert
        member.GetMember("B").Value.RawValue.Should().Be("2");
    }

    [TestMethod]
    public void Remove_MissingMember_ReturnsFalseAndLeavesMembersUnchanged()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("A", "1"));

        // act
        bool result = member.Remove("Missing");

        // assert
        result.Should().BeFalse();
        member.Count.Should().Be(1);
        member.GetMember("A").Value.RawValue.Should().Be("1");
    }

    [TestMethod]
    public void Remove_DifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("parent", "TestParent"));

        // act
        bool result = member.Remove("Parent");

        // assert
        result.Should().BeFalse();
        member.ContainsMember("parent").Should().BeTrue();
    }

    [TestMethod]
    public void Remove_ThenAddSameName_AddsNewMember()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("Link", "first"));
        member.Remove("Link");

        // act
        member.Add("Link", "second", member);

        // assert
        member.Count.Should().Be(1);
        member.GetMember("Link").Value.RawValue.Should().Be("second");
    }

    [TestMethod]
    public void Remove_ArrayMember_RemovesWholeArray()
    {
        // arrange
        StormXmlMember member = new("Parent");
        member.Add(CreateArrayMember("Items"), "0");
        member.Add(CreateArrayMember("Items"), "1");

        // act
        bool result = member.Remove("Items");

        // assert
        result.Should().BeTrue();
        member.Count.Should().Be(0);
        member.ContainsMember("Items").Should().BeFalse();
    }

    [TestMethod]
    public void Remove_ValueName_ClearsMemberValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("value", "100"), ("Link", "a"));

        // act
        bool result = member.Remove("value");

        // assert
        result.Should().BeTrue();
        member.ContainsMember("value").Should().BeFalse();
        member.Value.HasValue.Should().BeFalse();
        member.Value.RawValue.Should().BeEmpty();
        member.GetMember("Link").Value.RawValue.Should().Be("a");
    }

    [TestMethod]
    public void AddToken_NewToken_AddsMemberNamedAfterTokenId()
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // assert
        member.Count.Should().Be(1);
        member.ContainsMember("heroid").Should().BeTrue();
        member.GetMember("heroid").Value.HasValue.Should().BeTrue();
        member.GetMember("heroid").Value.RawValue.Should().Be("Jaina");
        member.GetMember("heroid").IsArray.Should().BeFalse();
    }

    [TestMethod]
    public void AddToken_NewToken_DoesNotSetMemberValue()
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // assert
        member.Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void AddToken_EmptyValue_AddsMemberWithEmptyValue()
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken("heroid", string.Empty, string.Empty));

        // assert
        member.ContainsMember("heroid").Should().BeTrue();
        member.GetMember("heroid").Value.HasValue.Should().BeTrue();
        member.GetMember("heroid").Value.RawValue.Should().BeEmpty();
    }

    [TestMethod]
    public void AddToken_DifferentIds_CountsEachToken()
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));
        member.AddToken(new StormXmlToken("talentid", "CTalentLink", "Frostbolt"));

        // assert
        member.Count.Should().Be(2);
        member.GetMember("heroid").Value.RawValue.Should().Be("Jaina");
        member.GetMember("talentid").Value.RawValue.Should().Be("Frostbolt");
    }

    [TestMethod]
    public void AddToken_SameIdTwice_OverwritesValueAndKeepsCount()
    {
        // arrange
        StormXmlMember member = new("CHero");
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Thrall"));

        // assert
        member.Count.Should().Be(1);
        member.GetMember("heroid").Value.RawValue.Should().Be("Thrall");
    }

    [TestMethod]
    public void AddToken_IdsAreCaseSensitive()
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "lower"));
        member.AddToken(new StormXmlToken("HeroId", "CHeroLink", "mixed"));

        // assert
        member.Count.Should().Be(2);
        member.GetMember("heroid").Value.RawValue.Should().Be("lower");
        member.GetMember("HeroId").Value.RawValue.Should().Be("mixed");
    }

    [TestMethod]
    public void AddToken_ExistingAttributeWithSameName_OverwritesAttributeValue()
    {
        // arrange
        StormXmlMember member = CreateMember("CHero", ("heroid", "FromAttribute"));

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "FromToken"));

        // assert
        member.Count.Should().Be(1);
        member.GetMember("heroid").Value.RawValue.Should().Be("FromToken");
    }

    [TestMethod]
    public void AddToken_TokenMember_CanBeFoundBySpan()
    {
        // arrange
        StormXmlMember member = new("CHero");
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        bool found = member.TryGetMember("heroid".AsSpan(), out StormXmlArray? result);

        // assert
        found.Should().BeTrue();
        result.Should().NotBeNull();
        result.Value.RawValue.Should().Be("Jaina");
    }

    [TestMethod]
    public void AddToken_ValueDefinedBeforeToken_ResolvesTokenInGetString()
    {
        // arrange
        StormXmlMember member = new("Name");
        member.Add("value", "Abil/Name/##heroid##", member);

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // assert
        member.Value.RawValue.Should().Be("Abil/Name/##heroid##");
        member.Value.GetString().Should().Be("Abil/Name/Jaina");
    }

    [TestMethod]
    public void AddToken_ChildMemberValueWithRootToken_ResolvesAgainstRoot()
    {
        // arrange
        StormXmlMember root = new("CHero");
        root.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        StormXmlMember child = new("Name");
        child.Add("value", "Abil/Name/##heroid##", root);

        // act
        string result = child.Value.GetString();

        // assert
        result.Should().Be("Abil/Name/Jaina");
    }

    [TestMethod]
    public void AddToken_SameIdTwice_ValueResolvesToLatestToken()
    {
        // arrange
        StormXmlMember member = new("Name");
        member.Add("value", "##heroid##", member);
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Thrall"));

        // assert
        member.Value.GetString().Should().Be("Thrall");
    }

    [TestMethod]
    [TestCategory("AddToken")]
    public void AddToken_ThenClone_CloneContainsTokenMember()
    {
        // arrange
        StormXmlMember member = new("CHero");
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        StormXmlMember clone = member.Clone();

        // assert
        clone.ContainsMember("heroid").Should().BeTrue();
        clone.GetMember("heroid").Should().NotBeSameAs(member.GetMember("heroid"));
        clone.GetMember("heroid").Value.RawValue.Should().Be("Jaina");
    }

    [TestMethod]
    [TestCategory("AddToken")]
    public void AddToken_OnCloneAfterClone_DoesNotAffectOriginal()
    {
        // arrange
        StormXmlMember member = new("Name");
        member.Add("value", "##heroid##", member);
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        StormXmlMember clone = member.Clone();

        // act
        clone.AddToken(new StormXmlToken("heroid", "CHeroLink", "Thrall"));

        // assert
        member.Value.GetString().Should().Be("Jaina");
        clone.Value.GetString().Should().Be("Thrall");
    }

    [TestMethod]
    public void AddToken_OtherHasToken_MergeAddsTokenMember()
    {
        // arrange
        StormXmlMember member = CreateMember("CHero", ("A", "1"));

        StormXmlMember other = new("CHero");
        other.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(2);
        member.GetMember("heroid").Value.RawValue.Should().Be("Jaina");
    }

    [TestMethod]
    public void AddToken_OtherHasSameTokenId_MergeOverwritesTokenValue()
    {
        // arrange
        StormXmlMember member = new("CHero");
        member.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        StormXmlMember other = new("CHero");
        other.AddToken(new StormXmlToken("heroid", "CHeroLink", "Thrall"));

        // act
        member.Merge(other);

        // assert
        member.Count.Should().Be(1);
        member.GetMember("heroid").Value.RawValue.Should().Be("Thrall");
    }

    [TestMethod]
    public void AddToken_OnlyMergedMemberHasToken_ValueResolvesAgainstMergedMember()
    {
        // arrange
        StormXmlMember member = new("Name");
        member.Add("value", "Abil/Name/##heroid##", member);

        StormXmlMember other = new("Name");
        other.AddToken(new StormXmlToken("heroid", "CHeroLink", "Jaina"));

        // act
        member.Merge(other);

        // assert
        member.Value.GetString().Should().Be("Abil/Name/Jaina");
    }

    [TestMethod]
    [DataRow("id")]
    [DataRow("parent")]
    [DataRow("default")]
    [DataRow("value")]
    public void AddToken_ReservedId_IsIgnored(string id)
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken(id, "CHeroLink", "Jaina"));

        // assert
        member.Count.Should().Be(0);
        member.ContainsMember(id).Should().BeFalse();
        member.Value.HasValue.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("id")]
    [DataRow("parent")]
    [DataRow("default")]
    [DataRow("value")]
    public void AddToken_ReservedId_DoesNotOverwriteExistingAttribute(string id)
    {
        // arrange
        StormXmlMember member = CreateMember("Element", (id, "original"));

        // act
        member.AddToken(new StormXmlToken(id, "CHeroLink", "Jaina"));

        // assert
        member.Count.Should().Be(1);
        member.GetMember(id).Value.RawValue.Should().Be("original");
    }

    [TestMethod]
    public void AddToken_IdIsValue_KeepsExistingMemberValue()
    {
        // arrange
        StormXmlMember member = CreateMember("Element", ("value", "1330"));

        // act
        member.AddToken(new StormXmlToken("value", "CHeroLink", "Jaina"));

        // assert
        member.Value.RawValue.Should().Be("1330");
        member.GetMember("value").Value.RawValue.Should().Be("1330");
    }

    [TestMethod]
    [DataRow("Id")]
    [DataRow("Parent")]
    [DataRow("Default")]
    [DataRow("Value")]
    [DataRow("VALUE")]
    public void AddToken_ReservedIdDifferentCase_IsAdded(string id)
    {
        // arrange
        StormXmlMember member = new("CHero");

        // act
        member.AddToken(new StormXmlToken(id, "CHeroLink", "Jaina"));

        // assert
        member.Count.Should().Be(1);
        member.GetMember(id).Value.RawValue.Should().Be("Jaina");
        member.Value.HasValue.Should().BeFalse();
    }

    // creates a member with the given attributes, using itself as the root for token replacement
    private static StormXmlMember CreateMember(string name, params (string Name, string Value)[] attributes)
    {
        StormXmlMember member = new(name);

        foreach ((string attributeName, string attributeValue) in attributes)
            member.Add(attributeName, attributeValue, member);

        return member;
    }

    // same as CreateMember, but flagged as an array element
    private static StormXmlMember CreateArrayMember(string name, params (string Name, string Value)[] attributes)
    {
        StormXmlMember member = CreateMember(name, attributes);
        member.IsArray = true;

        return member;
    }
}
