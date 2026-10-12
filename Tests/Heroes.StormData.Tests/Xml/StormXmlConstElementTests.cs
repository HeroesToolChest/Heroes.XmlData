namespace Heroes.StormData.Xml.Tests;

[TestClass]
public sealed class StormXmlConstElementTests
{
    [TestMethod]
    public void ElementName_ValidValue_ReturnsCorrectString()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="1777" />
            """);

        // act
        string result = constElement.ElementName;

        // assert
        result.Should().Be("const");
    }

    [TestMethod]
    public void ElementNameWithDeclaration_ValidValue_ReturnsCorrectString()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="1777" />
            """);

        // act
        string result = constElement.ElementNameWithDeclaration;

        // assert
        result.Should().Be("const");
    }

    [TestMethod]
    public void Value_ValidValue_ReturnsCorrectString()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="1777" />
            """);

        // act
        string result = constElement.Value;

        // assert
        result.Should().Be("1777");
    }

    [TestMethod]
    public void EvaluateAsExpression_AttributeSetTo1_ReturnsTrue()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="1777" evaluateAsExpression="1" />
            """);

        // act
        bool result = constElement.EvaluateAsExpression;

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void EvaluateAsExpression_AttributeSetTo0_ReturnsFalse()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="1777" evaluateAsExpression="0" />
            """);

        // act
        bool result = constElement.EvaluateAsExpression;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetValueAsDouble_ValidDoubleValue_ReturnsCorrectDouble()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="17.5" />
            """);

        // act
        double result = constElement.GetValueAsDouble();

        // assert
        result.Should().Be(17.5);
    }

    [TestMethod]
    public void GetValueAsDouble_InvalidDoubleValue_ThrowsHeroesStormDataException()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="NotADouble" />
            """);

        // act
        Action act = () => constElement.GetValueAsDouble();

        // assert
        act.Should().Throw<HeroesStormDataException>()
            .WithMessage("Could not convert value 'NotADouble' to a double.");
    }

    [TestMethod]
    public void GetValueAsInt_ValidIntValue_ReturnsCorretInt()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="17" />
            """);

        // act
        int result = constElement.GetValueAsInt32();

        // assert
        result.Should().Be(17);
    }

    [TestMethod]
    public void GetValueAsInt_InvalidIntValue_ThrowsHeroesStormDataException()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="NotAnInt" />
            """);

        // act
        Action act = () => constElement.GetValueAsInt32();

        // assert
        act.Should().Throw<HeroesStormDataException>()
            .WithMessage("Could not convert value 'NotAnInt' to an int32.");
    }

    [TestMethod]
    public void ElementType_ValidType_ReturnsType()
    {
        // arrange
        StormXmlConstElement constElement = XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="17" />
            """);

        // act
        StormXmlElementType result = constElement.ElementType;

        // assert
        result.Should().Be(StormXmlElementType.Const);
    }
}
