using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class ScaleValueParserTests
{
    public ScaleValueParserTests()
    {
    }

    [TestMethod]
    public void CreateStormElement_EntryAndStormElementExists_ReturnsNewScaleStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="AzmodanDemonicInvasionImpactDamage">
              <Amount value="85" />
              <PeriodicPeriodArray value="0" />
              <PeriodicPeriodArray value="0.25" />
              <PeriodicPeriodArray value="0.25" />
            </CEffectDamage>
            """);

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
        {
            {
                "AzmodanDemonicInvasionImpactDamage", new StormElement(new StormXmlValuePath(xmlObject.Root, TestHelpers.GetStormPath("custom")))
            },
        });

        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.Id.Should().BeNull();
        stormElement.DataValues.GetElementDataAt("Amount").HxdScaleValue.GetString().Should().Be("0.040000");
        stormElement.DataValues.GetElementDataAt("Amount").HxdScaleValue.GetAsDouble().Should().Be(0.040000);
    }

    [TestMethod]
    public void CreateStormElement_EntryAndStormElementDoesNotExists_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void CreateStormElement_FieldDoesNotExist_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectDamage id="AzmodanDemonicInvasionImpactDamage">
              <PeriodicPeriodArray value="0" />
              <PeriodicPeriodArray value="0.25" />
              <PeriodicPeriodArray value="0.25" />
            </CEffectDamage>
            """);

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
        {
            {
                "AzmodanDemonicInvasionImpactDamage", new StormElement(new StormXmlValuePath(xmlObject.Root, TestHelpers.GetStormPath("custom")))
            },
        });

        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().BeNull();
    }
}

