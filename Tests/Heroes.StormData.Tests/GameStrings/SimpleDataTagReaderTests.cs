namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public class SimpleDataTagReaderTests
{
    [TestMethod]
    [DataRow("d ref='Effect,Damage,Amount'")]
    [DataRow("d ref=\"Effect,Damage,Amount\"")]
    [DataRow("D ref='Effect,Damage,Amount'")]
    [DataRow("d  ref = 'Effect,Damage,Amount'  ")]
    [DataRow("d\tref='Effect,Damage,Amount'")]
    [DataRow("d player='0' ref='Effect,Damage,Amount'")]
    [DataRow("d ref='Effect,Damage,Amount'player='0'")] // no space between the attributes
    [DataRow("d Ref='Effect,Damage,Amount'")]
    [DataRow("d REF='Effect,Damage,Amount'")]
    [DataRow("d ref='Effect,Damage,Amount' ref='Effect,Other,Amount'")] // the first ref is used
    public void TryRead_HasRef_ReadsRef(string tagContent)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead(tagContent, out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.HasRef.Should().BeTrue();
        attributes.Ref.ToString().Should().Be("Effect,Damage,Amount");
        attributes.HasConst.Should().BeFalse();
        attributes.Precision.Should().Be(0);
    }

    [TestMethod]
    [DataRow("d const='$Const'")]
    [DataRow("d CONST='$Const'")]
    [DataRow("d const='$Const' const='$Other'")]
    public void TryRead_HasConst_ReadsConst(string tagContent)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead(tagContent, out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.HasConst.Should().BeTrue();
        attributes.ConstId.ToString().Should().Be("$Const");
        attributes.HasRef.Should().BeFalse();
    }

    [TestMethod]
    public void TryRead_HasConstAndRef_ReadsBoth()
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead("d ref='Effect,Damage,Amount' const='$Const'", out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.HasConst.Should().BeTrue();
        attributes.ConstId.ToString().Should().Be("$Const");
        attributes.HasRef.Should().BeTrue();
        attributes.Ref.ToString().Should().Be("Effect,Damage,Amount");
    }

    [TestMethod]
    [DataRow("d", false, false)]
    [DataRow("d ", false, false)]
    [DataRow("d player='0'", false, false)]
    [DataRow("d ref=''", false, true)]
    public void TryRead_SimpleTagWithoutConstOrRef_ReturnsTrue(string tagContent, bool hasConst, bool hasRef)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead(tagContent, out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.HasConst.Should().Be(hasConst);
        attributes.HasRef.Should().Be(hasRef);
        attributes.Ref.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("2", 2)]
    [DataRow("+2", 2)]
    [DataRow("-1", -1)]
    [DataRow("02", 2)]
    [DataRow("2.7", 2)] // the digits at the start, like the xml parser
    [DataRow("2abc", 2)]
    [DataRow("2 ", 2)]
    [DataRow(" 2", 0)] // a leading space isn't a number
    [DataRow(".5", 0)]
    [DataRow("x", 0)]
    [DataRow("", 0)]
    [DataRow("2147483648", 0)] // too big for an int
    public void TryRead_HasPrecision_ReadsPrecision(string precision, int expected)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead($"d ref='Effect,Damage,Amount' precision='{precision}'", out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.Precision.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("d precision='2' Precision='1'", 2)]
    [DataRow("d Precision='1' precision='2'", 1)] // the first precision is used, whatever its case
    [DataRow("d PRECISION='3'", 3)]
    public void TryRead_PrecisionInAnyCase_UsesFirstPrecision(string tagContent, int expected)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead(tagContent, out DataTagAttributes attributes);

        // assert
        result.Should().BeTrue();
        attributes.Precision.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("")] // no tag name
    [DataRow(" ref='Effect,Damage,Amount'")]
    [DataRow("c ref='Effect,Damage,Amount'")] // not a d tag
    [DataRow("dd ref='Effect,Damage,Amount'")]
    [DataRow("d data-ref='Effect,Damage,Amount'")] // attribute name that isn't only letters
    [DataRow("d ref='Effect,Damage,Amount&amp;'")] // value with an xml entity
    [DataRow("d ref='Effect,Damage,Amount<'")]
    [DataRow("d ref=Effect,Damage,Amount")] // not quoted
    [DataRow("d ref='Effect,Damage,Amount")] // no closing quote
    [DataRow("d ref='Effect,Damage,Amount\"")] // mismatched quotes
    [DataRow("d ref='Effect,Damage,Amount' extra")] // not an attribute
    [DataRow("d ref")]
    [DataRow("d ='Effect,Damage,Amount'")]
    [DataRow("d r ef='Effect,Damage,Amount'")]
    public void TryRead_NotASimpleTag_ReturnsFalse(string tagContent)
    {
        // arrange
        // act
        bool result = SimpleDataTagReader.TryRead(tagContent, out DataTagAttributes attributes);

        // assert
        result.Should().BeFalse();
        attributes.HasConst.Should().BeFalse();
        attributes.HasRef.Should().BeFalse();
    }
}
