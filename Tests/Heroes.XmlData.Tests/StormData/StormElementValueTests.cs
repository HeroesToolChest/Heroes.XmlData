using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementValueTests
{
    [TestMethod]
    public void GetString_GetValueWithReplacements_ReturnsValue()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad" unitName="KT">
              <Name value="Abil/Name/##id##/other##unitName##yes"/>
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name/KelThuzad/otherKTyes");
    }

    [TestMethod]
    public void GetString_GetValueWithReplacementThatDoesNotExists_ReturnsValue()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Name value="Abil/Name/##id##/other##unitName##yes"/>
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name/KelThuzad/other##unitName##yes");
    }

    [TestMethod]
    public void GetString_GetValueWithNoReplacements_ReturnsValue()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Name value="Abil/Name"/>
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name");
    }

    [TestMethod]
    public void GetAsInt_ValueIsAnInt_ReturnsInt()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """"
            <CHero id="KelThuzad">
              <Value value="5" />
            </CHero>
            """");

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        int value = stormElementData.GetElementDataAt("Value").Value.GetAsInt();

        // assert
        value.Should().Be(5);
    }

    [TestMethod]
    public void GetAsInt_ValueIsNotAnInt_ThrowsException()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5a" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        Action act = () => stormElementData.GetElementDataAt("Value").Value.GetAsInt();

        // assert
        act.Should().Throw<HeroesXmlDataException>();
    }

    [TestMethod]
    public void TryGetAsInt32_ValueIsAnInt_ReturnsTrue()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        bool result = stormElementData.GetElementDataAt("Value").Value.TryGetAsInt32(out int value);

        // assert
        result.Should().BeTrue();
        value.Should().Be(5);
    }

    [TestMethod]
    public void TryGetAsInt32_ValueIsNotAnInt_ReturnsFalse()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5a" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        bool result = stormElementData.GetElementDataAt("Value").Value.TryGetAsInt32(out int value);

        // assert
        result.Should().BeFalse();
        value.Should().Be(0);
    }

    [TestMethod]
    public void GetAsDouble_ValueIsADouble_ReturnsDouble()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5.1" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        double value = stormElementData.GetElementDataAt("Value").Value.GetAsDouble();

        // assert
        value.Should().Be(5.1);
    }

    [TestMethod]
    public void GetAsDouble_ValueIsNotADouble_ThrowsException()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5.1a" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        Action act = () => stormElementData.GetElementDataAt("Value").Value.GetAsDouble();

        // assert
        act.Should().Throw<HeroesXmlDataException>();
    }

    [TestMethod]
    public void TryGetAsDouble_ValueIsADouble_ReturnsTrue()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5.1" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        bool result = stormElementData.GetElementDataAt("Value").Value.TryGetAsDouble(out double value);

        // assert
        result.Should().BeTrue();
        value.Should().Be(5.1);
    }

    [TestMethod]
    public void TryGetAsDouble_ValueIsADouble_ReturnsFalse()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CHero id="KelThuzad">
              <Value value="5.1a" />
            </CHero>
            """);

        StormElementData stormElementData = new(xmlObject.Root);

        // act
        bool result = stormElementData.GetElementDataAt("Value").Value.TryGetAsDouble(out double value);

        // assert
        result.Should().BeFalse();
        value.Should().Be(0);
    }
}
