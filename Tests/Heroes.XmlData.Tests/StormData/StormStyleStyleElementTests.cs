using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStyleStyleElementTests
{
    [TestMethod]
    public void Name_HasNameAttribute_ReturnsName()
    {
        // arrange
        XElement element = XElement.Parse(@"
  <Style name=""TimerTextBottom"" template=""TimerText"" vjustify=""Bottom"" hjustify=""Center"" />
");
        StormStyleStyleElement stormStyleStyleElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleStyleElement.Name;
        bool result = stormStyleStyleElement.HasName;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("TimerTextBottom");
    }

    [TestMethod]
    public void Name_HasNoNameAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
  <Style template=""TimerText"" vjustify=""Bottom"" hjustify=""Center"" />
");
        StormStyleStyleElement stormStyleStyleElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleStyleElement.Name;
        bool result = stormStyleStyleElement.HasName;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }

    [TestMethod]
    public void Name_HasTemplateAttribute_ReturnsTemplate()
    {
        // arrange
        XElement element = XElement.Parse(@"
  <Style name=""TimerTextBottom"" template=""TimerText"" vjustify=""Bottom"" hjustify=""Center"" />
");
        StormStyleStyleElement stormStyleStyleElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleStyleElement.Template;
        bool result = stormStyleStyleElement.HasTemplate;

        // assert
        result.Should().BeTrue();
        resultValue.Should().Be("TimerText");
    }

    [TestMethod]
    public void Name_HasNoTemplateAttribute_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
  <Style name=""TimerTextBottom"" vjustify=""Bottom"" hjustify=""Center"" />
");
        StormStyleStyleElement stormStyleStyleElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleStyleElement.Template;
        bool result = stormStyleStyleElement.HasTemplate;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }
}