
namespace Heroes.StormData.Tests;

[TestClass]
public class StromClassTests
{
    [TestMethod]
    public void Add__()
    {
        //using XmlReader reader = XmlReaderHelper.CreateXmlReader(
        //    """            <?xml version="1.0" encoding="us-ascii"?>
        //    <Catalog>
        //        <CEffect default="1">
        //          <Chance value="1" />
        //          <Marker Link="Effect/##id##" />
        //          <DamageModifierSource Value="Unknown" />
        //        </CEffect>
        //        <CEffectAbortMissile default="1">
        //          <WhichUnit Value="Target" />
        //        </CEffectAbortMissile>
        //        <CEffectApplyBehavior default="1">
        //          <Behavior value="##id##" />
        //          <ValidatorArray value="##id##TargetFilters" />
        //          <WhichUnit Value="Target" />
        //          <Count value="1" />
        //        </CEffectApplyBehavior>
        //    </Catalog>
        //    """);

        //StormXmlElement stormXmlElement = StormXmlElement.Create(reader);


        //TempParser.Test();
        // arrange
        //XElement element = XElement.Parse(
        //    """
        //<CAbil default="1">
        //    <Name value="Abil/Name/##id##"/>
        //    <TechPlayer value="Upkeep"/>
        //    <TargetMessage value="Abil/TargetMessage/DefaultTargetMessage"/>
        //    <OrderArray>
        //        <Color value="255,0,255,0"/>
        //        <Model value="Assets\UI\Feedback\WayPointConfirm\WayPointConfirm.m3"/>
        //        <LineTexture value="Assets\Textures\WayPointLine.dds"/>
        //    </OrderArray>
        //    <SharedFlags index="DisableWhileDead" value="1"/>
        //    <SharedFlags index="AllowQuickCastCustomization" value="1"/>
        //    <SharedFlags index="TargetCursorVisibleInBlackMask" value="1"/>
        //</CAbil>
        //""");

        //StormXmlClass stormClass = new("CAbil");

        //// act
        //stormClass.Add("default", new StormXmlValue("1"));
        //stormClass.Add("TechPlayer", new StormXmlValue("Upkeep"));

        //// assert
        //stormClass.Name.Should().Be("CAbil");
    }

    //[TestMethod]
    //public void Add_NamedIndex_()
    //{
    //    // arrange
    //    StormXmlClass stormClass = new("CAbil");

    //    // act
    //    stormClass.Add("SharedFlags", new StormXmlValue("1"), "DisableWhileDead");
    //    stormClass.Add("SharedFlags", new StormXmlValue("0"), "AllowQuickCastCustomization");
    //    stormClass.Add("SharedFlags", new StormXmlValue("1"), "TargetCursorVisibleInBlackMask");

    //    // assert
    //    //StormArray? stormArray = stormClass.GetArray("SharedFlags");
    //    stormArray.Should().NotBeNull();

    //    stormArray["DisableWhileDead"].ToString().Should().Be("1");
    //    stormArray["AllowQuickCastCustomization"].ToString().Should().Be("0");
    //    stormArray["TargetCursorVisibleInBlackMask"].ToString().Should().Be("1");
    //}

    //        XElement mergingElement = XElement.Parse(@"
    //<CAbilEffectInstant default=""1"">
    //  <CmdButtonArray index=""Execute"" AutoQueueId=""Spell"">
    //    <Flags index=""Continuous"" value=""1"" />
    //    <Flags index=""Option"" value=""0"" />
    //  </CmdButtonArray>
    //</CAbilEffectInstant>
    //");
}
