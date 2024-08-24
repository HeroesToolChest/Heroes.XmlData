namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementValueTests
{
    [TestMethod]
    public void GetString_GetValueWithReplacements_ReturnsValue()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad" unitName="KT">
  <Name value="Abil/Name/##id##/other##unitName##yes"/>
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name/KelThuzad/otherKTyes");
    }

    [TestMethod]
    public void GetString_GetValueWithReplacementThatDoesNotExists_ReturnsValue()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Name value="Abil/Name/##id##/other##unitName##yes"/>
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name/KelThuzad/other##unitName##yes");
    }

    [TestMethod]
    public void GetString_GetValueWithNoReplacements_ReturnsValue()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Name value="Abil/Name"/>
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        string value = stormElementData.GetElementDataAt("Name").Value.GetString();

        // assert
        value.Should().Be("Abil/Name");
    }

    [TestMethod]
    public void GetAsInt_ValueIsAnInt_ReturnsInt()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5" />
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        int value = stormElementData.GetElementDataAt("Value").Value.GetAsInt();

        // assert
        value.Should().Be(5);
    }

    [TestMethod]
    public void GetAsInt_ValueIsNotAnInt_ThrowsException()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5a" />
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        Action act = () => stormElementData.GetElementDataAt("Value").Value.GetAsInt();

        // assert
        act.Should().Throw<HeroesXmlDataException>();
    }

    [TestMethod]
    public void TryGetAsInt32_ValueIsAnInt_ReturnsTrue()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5" />
</CHero>
""");
        StormElementData stormElementData = new(element);

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
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5a" />
</CHero>
""");
        StormElementData stormElementData = new(element);

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
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5.1" />
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        double value = stormElementData.GetElementDataAt("Value").Value.GetAsDouble();

        // assert
        value.Should().Be(5.1);
    }

    [TestMethod]
    public void GetAsDouble_ValueIsNotADouble_ThrowsException()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5.1a" />
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        Action act = () => stormElementData.GetElementDataAt("Value").Value.GetAsDouble();

        // assert
        act.Should().Throw<HeroesXmlDataException>();
    }

    [TestMethod]
    public void TryGetAsDouble_ValueIsADouble_ReturnsTrue()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5.1" />
</CHero>
""");
        StormElementData stormElementData = new(element);

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
        XElement element = XElement.Parse(
"""
<CHero id="KelThuzad">
  <Value value="5.1a" />
</CHero>
""");
        StormElementData stormElementData = new(element);

        // act
        bool result = stormElementData.GetElementDataAt("Value").Value.TryGetAsDouble(out double value);

        // assert
        result.Should().BeFalse();
        value.Should().Be(0);
    }
}
