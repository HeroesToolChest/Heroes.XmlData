using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStyleStyleElementTests
{
    [TestMethod]
    public void Name_HasNameAttribute_ReturnsName()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="TimerTextBottom" template="TimerText" vjustify="Bottom" hjustify="Center" />
            """);

        StormStyleStyleElement stormStyleStyleElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

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
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style template="TimerText" vjustify="Bottom" hjustify="Center" />
            """);

        StormStyleStyleElement stormStyleStyleElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

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
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="TimerTextBottom" template="TimerText" vjustify="Bottom" hjustify="Center" />
            """);

        StormStyleStyleElement stormStyleStyleElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

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
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="TimerTextBottom" vjustify="Bottom" hjustify="Center" />
            """);

        StormStyleStyleElement stormStyleStyleElement = new(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("some\\path")));

        // act
        string? resultValue = stormStyleStyleElement.Template;
        bool result = stormStyleStyleElement.HasTemplate;

        // assert
        result.Should().BeFalse();
        resultValue.Should().BeNull();
    }
}