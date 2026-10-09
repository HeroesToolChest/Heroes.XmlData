namespace Heroes.StormData.Tests.Xml;

[TestClass]
public class StormXmlElementTests
{
    [TestMethod]
    public void AddValue_MergingSingleElement_DataValuesAreMerged()
    {
        // arrange
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbil default="1">
              <Name value="Abil/Name/##id##" />
              <TechPlayer value="Upkeep" />
              <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage" />
              <OrderArray>
                <Color value="255,0,255,0" />
                <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3" />
                <LineTexture value="Assets\Textures\WayPointLine.dds" />
              </OrderArray>
              <SharedFlags index="DisableWhileDead" value="1" />
              <SharedFlags index="AllowQuickCastCustomization" value="1" />
              <SharedFlags index="TargetCursorVisibleInBlackMask" value="1" />
            </CAbil>
            """);

        StormXmlElement mergingElement = XmlReaderHelper.GetStormXmlElement(
            """
            <CAbilEffectInstant default="1">
              <CmdButtonArray index="Execute" AutoQueueId="Spell">
                <Flags index="Continuous" value="1" />
              </CmdButtonArray>
              <OrderArray index="0" LineTexture="Assets\Textures\Storm_WayPointLine.dds" />
              <Flags index="AllowMovement" value="1" />
              <Flags index="WaitToSpend" value="0" />
              <Flags index="ValidateButtonState" value="1" />
              <SharedFlags index="DisableWhileDead" value="0" />
            </CAbilEffectInstant>
            """);

        // act
        element.Merge(mergingElement);

        // assert
        element.ElementName.Should().Be("AbilEffectInstant");
        element.ElementType.Should().Be(StormXmlElementType.Class);
        element.Id.Should().BeNull();
        element.Parent.Should().BeNull();
        element.IsDefault.Should().BeTrue();
        element.Members["Name"].Value.Value.RawValue.Should().Be("Abil/Name/##id##");
        element.Members["OrderArray"].ByIndex[0]["Color"].Value.Value.RawValue.Should().Be("255,0,255,0");
        element.Members["OrderArray"].ByIndex[0]["Model"].Value.Value.RawValue.Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        element.Members["OrderArray"].ByIndex[0]["LineTexture"].Value.Value.RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        element.Members["SharedFlags"].ByIndex["DisableWhileDead"].Value.Value.RawValue.Should().Be("0");
        element.Members["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.Value.RawValue.Should().Be("1");
        element.Members["SharedFlags"].ByIndex["TargetCursorVisibleInBlackMask"].Value.Value.RawValue.Should().Be("1");
        element.Members["Flags"].ByIndex["AllowMovement"].Value.Value.RawValue.Should().Be("1");
        element.Members["Flags"].ByIndex["WaitToSpend"].Value.Value.RawValue.Should().Be("0");
        element.Members["Flags"].ByIndex["ValidateButtonState"].Value.Value.RawValue.Should().Be("1");
        element.Members["CmdButtonArray"].ByIndex["Execute"]["AutoQueueId"].Value.Value.RawValue.Should().Be("Spell");
        element.Members["CmdButtonArray"].ByIndex["Execute"]["Flags"].ByIndex["Continuous"].Value.Value.RawValue.Should().Be("1");
    }
}
