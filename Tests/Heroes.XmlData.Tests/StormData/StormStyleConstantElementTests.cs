using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStyleConstantElementTests
{
    [TestMethod]
    public void Name_HasNameAttribute_ReturnsName()
    {
        // arrange
        XElement element = XElement.Parse(@"
<Constant name=""PingAlly"" val=""3184ff"" />
");
        StormStyleConstantElement stormStyleConstantElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleConstantElement.Name;
        bool result = stormStyleConstantElement.HasName;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("PingAlly");
    }

    [TestMethod]
    public void Name_HasNoNameAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<Constant val=""3184ff"" />
");
        StormStyleConstantElement stormStyleConstantElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleConstantElement.Name;
        bool result = stormStyleConstantElement.HasName;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }

    [TestMethod]
    public void Val_HasValAttribute_ReturnsValue()
    {
        // arrange
        XElement element = XElement.Parse(@"
<Constant name=""PingAlly"" val=""3184ff"" />
");
        StormStyleConstantElement stormStyleConstantElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleConstantElement.Val;
        bool result = stormStyleConstantElement.HasVal;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("3184ff");
    }

    [TestMethod]
    public void Val_HasNoValAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<Constant name=""PingAlly"" />
");
        StormStyleConstantElement stormStyleConstantElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleConstantElement.Val;
        bool result = stormStyleConstantElement.HasVal;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }
}