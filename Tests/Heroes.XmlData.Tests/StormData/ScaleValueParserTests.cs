using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class ScaleValueParserTests
{
    private readonly IStormStorage _stormStorage;

    public ScaleValueParserTests()
    {
        _stormStorage = Substitute.For<IStormStorage>();
    }

    [TestMethod]
    public void CreateStormElement_EntryAndStormElementExists_ReturnsNewScaleStormElement()
    {
        // arrange
        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        _stormStorage.GetCompleteStormElement(levelScalingEntry.Entry, levelScalingEntry.Catalog).Returns(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffectDamage id=""AzmodanDemonicInvasionImpactDamage"">
  <Amount value=""85"" />
  <PeriodicPeriodArray value=""0"" />
  <PeriodicPeriodArray value=""0.25"" />
  <PeriodicPeriodArray value=""0.25"" />
</CEffectDamage>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(_stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.Id.Should().BeNull();
        stormElement.GetXmlData("Amount").ScaleValue.Should().Be("0.040000");
    }

    [TestMethod]
    public void CreateStormElement_EntryAndStormElementDoesNotExists_ReturnsNull()
    {
        // arrange
        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        _stormStorage.GetCompleteStormElement(levelScalingEntry.Entry, levelScalingEntry.Catalog).Returns((StormElement)null!);

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(_stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void CreateStormElement_FieldDoesNotExist_ReturnsNull()
    {
        // arrange
        LevelScalingEntry levelScalingEntry = new("Effect", "AzmodanDemonicInvasionImpactDamage", "Amount");
        StormStringValue stormStringValue = new("0.040000", TestHelpers.GetStormPath("custom"));

        _stormStorage.GetCompleteStormElement(levelScalingEntry.Entry, levelScalingEntry.Catalog).Returns(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffectDamage id=""AzmodanDemonicInvasionImpactDamage"">
  <PeriodicPeriodArray value=""0"" />
  <PeriodicPeriodArray value=""0.25"" />
  <PeriodicPeriodArray value=""0.25"" />
</CEffectDamage>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = ScaleValueParser.CreateStormElement(_stormStorage, levelScalingEntry, stormStringValue);

        // assert
        stormElement.Should().BeNull();
    }
}

