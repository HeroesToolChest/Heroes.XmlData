using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormXmlValuePathTests
{
    [TestMethod]
    public void Xml_StoreXmlObjectAsBytes_ReturnsOriginalXmlString()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Catalog>
                <CReward default="1">
                    <Name value="Reward/Name/##id##"/>
                    <Description value="Reward/Description/##id##"/>
                    <DescriptionUnearned value="Reward/DescriptionUnearned/##id##"/>
                    <HyperlinkId value="##id##"/>
                    <RewardDisplayType value="Flyout"/>
                </CReward>
            </Catalog>
            """);
        StormXmlValuePath stormXmlValuePath = new(xmlObject.AsRawString().ToArray(), TestHelpers.GetStormPath("test"));

        // act
        string xmlAsString = stormXmlValuePath.Xml;

        // assert
        xmlAsString.Should().Be(xmlObject.AsRawString().ToString());
    }
}
