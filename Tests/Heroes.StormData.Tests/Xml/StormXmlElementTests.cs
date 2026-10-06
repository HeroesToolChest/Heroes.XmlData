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
            <Catalog>
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
            </Catalog>
            """);

        StormXmlElement mergingElement = XmlReaderHelper.GetStormXmlElement(
            """
            <Catalog>
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
            </Catalog>
            """);

        // act
        element.Merge(mergingElement);

        // assert
        element.ElementName.Should().Be("AbilEffectInstant");
        element.ElementType.Should().Be(StormXmlElementType.Class);
        element.Id.Should().BeNull();
        element.Parent.Should().BeNull();
        element.IsDefault.Should().BeTrue();
        element.Data["Name"].Value.ToString().Should().Be("Abil/Name/##id##");
        element.Data["OrderArray"].ByIndex[0]["Color"].Value.ToString().Should().Be("255,0,255,0");
        element.Data["OrderArray"].ByIndex[0]["Model"].Value.ToString().Should().Be("Assets\\UI\\Feedback\\WayPointConfirm\\WayPointConfirm.m3");
        element.Data["OrderArray"].ByIndex[0]["LineTexture"].Value.ToString().Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        element.Data["SharedFlags"].ByIndex["DisableWhileDead"].Value.ToString().Should().Be("0");
        element.Data["SharedFlags"].ByIndex["AllowQuickCastCustomization"].Value.ToString().Should().Be("1");
        element.Data["SharedFlags"].ByIndex["TargetCursorVisibleInBlackMask"].Value.ToString().Should().Be("1");
        element.Data["Flags"].ByIndex["AllowMovement"].Value.ToString().Should().Be("1");
        element.Data["Flags"].ByIndex["WaitToSpend"].Value.ToString().Should().Be("0");
        element.Data["Flags"].ByIndex["ValidateButtonState"].Value.ToString().Should().Be("1");
        element.Data["CmdButtonArray"].ByIndex["Execute"]["AutoQueueId"].Value.ToString().Should().Be("Spell");
        element.Data["CmdButtonArray"].ByIndex["Execute"]["Flags"].ByIndex["Continuous"].Value.ToString().Should().Be("1");
    }
}
