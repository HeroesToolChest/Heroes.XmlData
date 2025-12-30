using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementDataTests
{
    [TestMethod]
    public void StormElementData_Indexers_GetsStormElementData()
    {
        // arrange
        XElement element = new(
            "CAbil",
            new XElement(
                "OrderArray",
                new XAttribute("index", "0"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine.dds")));

        // act
        StormElementData stormElementData = new(null!, element);

        // assert
        stormElementData["OrderArray"]["0".AsSpan()]["LineTexture".AsSpan()].RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElementData["OrderArray".AsSpan()]["0".AsSpan()]["LineTexture".AsSpan()].RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
    }

    [TestMethod]
    public void StormElementData_AttributesAndElements_ShouldBeSavedAsSameLevelXmlData()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "Abil/Name/abil1")));

        // act
        StormElement stormElement = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        StormElementData stormElementData = new(stormElement, xElement);

        // assert
        stormElementData.GetElementDataAt("name").HasValue.Should().BeTrue();
        stormElementData.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/abil1");
    }

    [TestMethod]
    public void StormElementData_EquivalentArrayAttributAndElement_SameXmlData()
    {
        // arrange
        XElement withAttributes = new(
            "CAbil",
            new XElement(
                "OrderArray",
                new XAttribute("index", "0"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine.dds")));

        XElement withElements = new(
            "CAbil",
            new XElement(
                "OrderArray",
                new XElement(
                    "LineTexture",
                    new XAttribute("value", "Assets\\Textures\\Storm_WayPointLine.dds"))));

        // act
        StormElementData stormElementDataAsAttributes = new(null!, withAttributes);
        StormElementData stormElementDataAsElements = new(null!, withElements);

        // assert
        stormElementDataAsAttributes.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElementDataAsElements.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");

        stormElementDataAsAttributes.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").ElementDataCount.Should().Be(0);
        stormElementDataAsElements.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").ElementDataCount.Should().Be(0);
    }

    [TestMethod]
    public void StormElementData_ArrayWithTextIndex_InnerDataHasSharedIndex()
    {
        // arrange
        XElement element = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "SharedFlags",
                new XAttribute("index", "DisableWhileDead"),
                new XAttribute("value", "1")),
            new XElement(
                "SharedFlags",
                new XAttribute("index", "AllowQuickCastCustomization"),
                new XAttribute("value", "1")));

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("SharedFlags").GetElementDataAt("DisableWhileDead").RawValue.Should().Be("1");
        data.GetElementDataAt("SharedFlags").GetElementDataAt("AllowQuickCastCustomization").RawValue.Should().Be("1");
    }

    [TestMethod]
    public void StormElementData_ElementWithNoIndexHasAttributesAndElement_AttributeAndElementOnSameLevelIndex()
    {
        // arrange
        XElement element = new(
            "CBehaviorTokenCounter",
            new XAttribute("id", "KelThuzadMasterOfTheColdDarkToken"),
            new XElement(
                "Max",
                new XAttribute("value", "30")),
            new XElement(
                "ConditionalEvents",
                new XAttribute("Compare", "GE"),
                new XAttribute("CompareValue", "15"),
                new XElement(
                    "Event",
                    new XAttribute("Effect", "KelThuzadMasterOfTheColdDarkTier1ModifyPlayer"))));

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElementData data = new(stormElement, element);

        // assert
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("Compare").RawValue.Should().Be("GE");
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("CompareValue").RawValue.Should().Be("15");
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("Event").GetElementDataAt("Effect").RawValue.Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
    }

    [TestMethod]
    public void StormElementData_ModficationsIsArray_AttributeAndElementOnSameLevelKey()
    {
        // arrange
        XElement element = new(
            "CTalent",
            new XAttribute("id", "AnubarakMasteryEpicenterBurrowCharge"),
            new XElement(
                "AbilityModificationArray",
                new XElement(
                    "Modifications",
                    new XElement(
                        "Field",
                        new XAttribute("value", "AreaArray[0].Radius")),
                    new XElement(
                        "Value",
                        new XAttribute("value", "1.600000"))),
                new XElement(
                    "Modifications",
                    new XElement(
                        "Field",
                        new XAttribute("value", "Chance")),
                    new XElement(
                        "Value",
                        new XAttribute("value", "1.000000"))),
                new XElement(
                    "Modifications",
                    new XElement(
                        "Field",
                        new XAttribute("value", "AnubarakBurrowChargeCursorSplat")),
                    new XElement(
                        "Value",
                        new XAttribute("value", "0.600000")))));

        // act
        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElementData data = new(stormElement, element);

        // assert
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Field").RawValue.Should().Be("AreaArray[0].Radius");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Value").RawValue.Should().Be("1.600000");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Value").RawValue.Should().Be("1.600000");

        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Field").RawValue.Should().Be("AnubarakBurrowChargeCursorSplat");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Value").RawValue.Should().Be("0.600000");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Value").RawValue.Should().Be("0.600000");
    }

    [TestMethod]
    public void StormElementData_NumericalIndexes_ShouldReturnTrue()
    {
        // arrange
        XElement element = new(
            "CAbil",
            new XElement(
                "OrderArray",
                new XAttribute("index", "0"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine0.dds")),
            new XElement(
                "OrderArray",
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine1.dds")),
            new XElement(
                "OrderArray",
                new XAttribute("index", "2"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine2.dds")));

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine0.dds");
        data.GetElementDataAt("OrderArray").GetElementDataAt("1").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine1.dds");
        data.GetElementDataAt("OrderArray").GetElementDataAt("2").GetElementDataAt("LineTexture").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine2.dds");
    }

    [TestMethod]
    public void StormElementData_HasTextInnerArray_IndexedArrayShouldHaveTextIndex()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
</CHero>

");

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").RawValue.Should().Be("KelThuzadDeathAndDecay");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").RawValue.Should().Be("1");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").RawValue.Should().Be("KelThuzadChains");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").RawValue.Should().Be("1");
    }

    [TestMethod]
    public void StormElementData_HasNumericalIndexInnerArray_IndexedArrayShouldHaveNumericalIndex()
    {
        // arrange
        XElement element = XElement.Parse("""
            <CHero id="KelThuzad">
              <HeroAbilArray Abil="KelThuzadDeathAndDecay" Button="KelThuzadDeathAndDecay">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
              </HeroAbilArray>
              <HeroAbilArray Abil="KelThuzadFrostNova" Button="KelThuzadFrostNova">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
              </HeroAbilArray>
              <HeroAbilArray Abil="KelThuzadChains" Button="KelThuzadChains">
                <Flags index="0" value="1" />
                <Flags value="2" />
                <Flags value="3" />
              </HeroAbilArray>
            </CHero>
            """);

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").RawValue.Should().Be("KelThuzadDeathAndDecay");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("0").RawValue.Should().Be("1");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").RawValue.Should().Be("KelThuzadChains");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("2").RawValue.Should().Be("3");
    }

    [TestMethod]
    public void StormElementData_ElementHasConstAttribute_ReturnsConstValue()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "SomeValue")));

        // act
        StormElement stormElement = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        StormElementData stormElementData = new(stormElement, xElement);

        // assert
        stormElementData.GetElementDataAt("Name").Value.GetString().Should().Be("SomeValue");
    }

    [TestMethod]
    public void StormElementData_ElementWithIntAttribute_ReturnsInt()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "5")));

        // act
        StormElement stormElement = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        StormElementData stormElementData = new(stormElement, xElement);

        // assert
        stormElementData.GetElementDataAt("Name").Value.GetInt().Should().Be(5);
    }

    [TestMethod]
    public void HasHxdScale_HasScalingElement_ReturnsTrueForHasScale()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Damage",
                new XElement(
                    ScaleValueParser.ScaleAttributeName,
                    new XAttribute("Value", "0.1"))));

        // act
        StormElement stormElement = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        StormElementData stormElementData = new(stormElement, xElement);

        // assert
        stormElementData.GetElementDataAt("Damage").HasHxdScale.Should().BeTrue();
        stormElementData.GetElementDataAt("Damage").HxdScaleValue.GetString().Should().Be("0.1");
        stormElementData.GetElementDataAt("Damage").HxdScaleValue.GetDouble().Should().Be(0.1);
    }

    [TestMethod]
    public void StormElementData_HasNoScalingElement_ReturnsTrueForHasScale()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Damage",
                new XElement(
                    "other",
                    new XAttribute("Value", "0.1"))));

        // act
        StormElement stormElement = new(new StormXElementValuePath(xElement, TestHelpers.GetStormPath("some\\path")));
        StormElementData stormElementData = new(stormElement, xElement);

        // assert
        stormElementData.GetElementDataAt("Damage").HasHxdScale.Should().BeFalse();
        stormElementData.GetElementDataAt("Damage").HxdScaleValue.GetString().Should().BeEmpty();
    }

    [TestMethod]
    public void Field_NumericalIndexes_ReturnsCorrectFields()
    {
        // arrange
        XElement element = new(
            "CAbil",
            new XElement(
                "OrderArray",
                new XAttribute("index", "0"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine0.dds")),
            new XElement(
                "OrderArray",
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine1.dds")),
            new XElement(
                "OrderArray",
                new XAttribute("index", "2"),
                new XAttribute("LineTexture", "Assets\\Textures\\Storm_WayPointLine2.dds")));

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("OrderArray").Field.Should().Be("OrderArray");
        data.GetElementDataAt("OrderArray").GetElementDataAt("0").Field.Should().Be("OrderArray[0]");
        data.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").Field.Should().Be("OrderArray[0].LineTexture");
        data.GetElementDataAt("OrderArray").GetElementDataAt("1").Field.Should().Be("OrderArray[1]");
        data.GetElementDataAt("OrderArray").GetElementDataAt("1").GetElementDataAt("LineTexture").Field.Should().Be("OrderArray[1].LineTexture");
        data.GetElementDataAt("OrderArray").GetElementDataAt("2").Field.Should().Be("OrderArray[2]");
        data.GetElementDataAt("OrderArray").GetElementDataAt("2").GetElementDataAt("LineTexture").Field.Should().Be("OrderArray[2].LineTexture");
    }

    [TestMethod]
    public void Field_HasTextInnerArray_ReturnsCorrectFields()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
</CHero>

");

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[0].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[0].Flags[ShowInHeroSelect]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByCooldownReduction]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByOverdrive]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[1].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[1].Flags[ShowInHeroSelect]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByCooldownReduction]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByOverdrive]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[2].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[2].Flags[ShowInHeroSelect]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[2].Flags[AffectedByCooldownReduction]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[2].Flags[AffectedByOverdrive]");
    }

    [TestMethod]
    public void Field_HasNumericalIndexInnerArray_ReturnsCorrectFields()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""0"" value=""1"" />
    <Flags value=""2"" />
    <Flags value=""3"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""0"" value=""1"" />
    <Flags value=""2"" />
    <Flags value=""3"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""0"" value=""1"" />
    <Flags value=""2"" />
    <Flags value=""3"" />
  </HeroAbilArray>
</CHero>

");

        // act
        StormElementData data = new(null!, element);

        // assert
        data.GetElementDataAt("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[0].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[0].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[0].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[0].Flags[2]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[1].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[1].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[1].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[1].Flags[2]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").Field.Should().Be("HeroAbilArray[2].Abil");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[2].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[2].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[2].Flags[2]");
    }

    [TestMethod]
    public void TryGetElementDataAt_HasData_ReturnsStormElementData()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
</CHero>

");
        StormElementData data = new(null!, element);

        // act
        bool result = data.TryGetElementDataAt("HeroAbilArray", out StormElementData? _);
        bool resultAsSpan = data.TryGetElementDataAt("HeroAbilArray".AsSpan(), out StormElementData? _);

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
    }

    [TestMethod]
    public void TryGetElementDataAt_HasNoData_ReturnsNull()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
</CHero>
");
        StormElementData data = new(null!, element);

        // act
        bool result = data.TryGetElementDataAt("Damage", out StormElementData? stormElementData);
        bool resultAsSpan = data.TryGetElementDataAt("Damage".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
        stormElementData.Should().BeNull();
        stormElementDataAsSpan.Should().BeNull();
    }

    [TestMethod]
    public void GetElementDataAt_HasNoData_ThrowsException()
    {
        // arrange
        XElement element = XElement.Parse(@"
<CHero id=""KelThuzad"">
  <HeroAbilArray Abil=""KelThuzadDeathAndDecay"" Button=""KelThuzadDeathAndDecay"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadFrostNova"" Button=""KelThuzadFrostNova"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
  <HeroAbilArray Abil=""KelThuzadChains"" Button=""KelThuzadChains"">
    <Flags index=""ShowInHeroSelect"" value=""1"" />
    <Flags index=""AffectedByCooldownReduction"" value=""1"" />
    <Flags index=""AffectedByOverdrive"" value=""1"" />
  </HeroAbilArray>
</CHero>
");
        StormElementData data = new(null!, element);

        // act
        Action action = () => data.GetElementDataAt("Does Not Exists");
        Action actionSpan = () => data.GetElementDataAt("Does Not Exists".AsSpan());

        // assert
        action.Should().Throw<KeyNotFoundException>();
        actionSpan.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void ElementDataIndexes_OnRootElement_ReturnsAllIndexes()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CBundle id="AllStarsBundle">
    <ProductId value="22143"/>
    <Universe value="Heroes"/>
    <HeroArray value="Raynor"/>
    <HeroArray value="Azmodan"/>
    <SkinArray Hero="Raynor" Skin="RaynorPatriot"/>
    <SkinArray Hero="Azmodan" Skin="AzmodunkBundleProduct"/>
</CBundle>
""");
        StormElementData data = new(null!, element);

        // act
        List<string> results = [.. data.GetElementDataIndexes()];

        // assert
        results.Should().SatisfyRespectively(
            first =>
            {
                first.Should().BeEquivalentTo("id");
            },
            second =>
            {
                second.Should().BeEquivalentTo("ProductId");
            },
            third =>
            {
                third.Should().BeEquivalentTo("Universe");
            },
            fourth =>
            {
                fourth.Should().BeEquivalentTo("HeroArray");
            },
            fifth =>
            {
                fifth.Should().BeEquivalentTo("SkinArray");
            });
    }

    [TestMethod]
    public void ElementDataIndexes_OnArrayElement_ReturnsIndexes()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CBundle id="AllStarsBundle">
    <ProductId value="22143"/>
    <Universe value="Heroes"/>
    <HeroArray value="Raynor"/>
    <HeroArray value="Azmodan"/>
    <SkinArray Hero="Raynor" Skin="RaynorPatriot"/>
    <SkinArray Hero="Azmodan" Skin="AzmodunkBundleProduct"/>
</CBundle>
""");
        StormElementData data = new(null!, element);

        // act
        List<string> results = [.. data.GetElementDataAt("heroarray").GetElementDataIndexes()];

        // assert
        results.Should().SatisfyRespectively(
            first =>
            {
                first.Should().BeEquivalentTo("0");
            },
            second =>
            {
                second.Should().BeEquivalentTo("1");
            });
    }

    [TestMethod]
    public void ElementData_OnRootArray_ReturnsKeyValueCollection()
    {
        // arrange
        XElement element = XElement.Parse(
"""
<CBundle id="AllStarsBundle">
    <ProductId value="22143"/>
    <Universe value="Heroes"/>
    <HeroArray value="Raynor"/>
    <HeroArray value="Azmodan"/>
    <SkinArray Hero="Raynor" Skin="RaynorPatriot"/>
    <SkinArray Hero="Azmodan" Skin="AzmodunkBundleProduct"/>
</CBundle>
""");
        StormElementData data = new(null!, element);

        // act
        List<KeyValuePair<string, StormElementData>> result = [.. data.GetElementData()];

        // assert
        result.Should().HaveCount(5);
    }

    [TestMethod]
    public void ElementData_ArrayDataIndexDoesNotStartAtZero_ReturnsCorrectDataElements()
    {
        // arrange
        XElement element = XElement.Parse(
            """
            <CUnit id="HeroAbathur">
                <CardLayouts index="0">
                  <LayoutButtons index="1" Face="Attack" Type="AbilCmd" AbilCmd="attack,Execute" Slot="Attack" />
                  <LayoutButtons index="2" Face="AcquireMove" Type="AbilCmd" AbilCmd="move,AcquireMove" Slot="Attack" />
                  <LayoutButtons Face="AbathurSymbiote" Type="AbilCmd" AbilCmd="AbathurSymbiote,Execute" Slot="Ability1" />
                  <LayoutButtons Face="AbathurToxicNest" Type="AbilCmd" AbilCmd="AbathurToxicNest,Execute" Slot="Ability2" />
                  <LayoutButtons Face="AbathurDeepTunnel" Type="AbilCmd" AbilCmd="AbathurDeepTunnel,Execute" Slot="Mount" />
                </CardLayouts>
            </CUnit>
            """);

        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElementData data = new(stormElement, element);

        // act
        List<KeyValuePair<string, StormElementData>> result = [.. data.GetElementDataAt("CardLayouts").GetElementDataAt("0").GetElementDataAt("LayoutButtons").GetElementData()];

        // assert
        result.Should().HaveCount(5);
        result[0].Value.GetElementDataAt("Face").Value.GetString().Should().Be("Attack");
        result[1].Value.GetElementDataAt("Face").Value.GetString().Should().Be("AcquireMove");
        result[2].Value.GetElementDataAt("Face").Value.GetString().Should().Be("AbathurSymbiote");
        result[3].Value.GetElementDataAt("Face").Value.GetString().Should().Be("AbathurToxicNest");
        result[4].Value.GetElementDataAt("Face").Value.GetString().Should().Be("AbathurDeepTunnel");
    }

    [TestMethod]
    public void ElementData_HasMultipleTooltipAppenders_ReturnsTooltipAppendersAsArrayElements()
    {
        // arrange
        XElement element = XElement.Parse(
            """
            <CButton id="SamuroAdvancingStrikes" parent="StormButtonParent">
              <TooltipAppender Validator="SamuroHasDeflectionTalent" Face="SamuroDeflectionTalent" />
              <TooltipAppender Validator="SamuroHasPressTheAttack" Face="SamuroPressTheAttack" />
              <TooltipAppender Validator="SamuroHasBlademastersPursuit" Face="SamuroBlademastersPursuitTalent" />
              <Icon value="Assets\Textures\storm_ui_icon_samuro_flowingstrikes.dds" />
              <HotkeyAlias value="SamuroMirrorImage" />
              <TooltipCooldownOverrideText value="SamuroImageTransmission" />
            </CButton>
            """);

        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));
        StormElementData data = new(stormElement, element);

        // act
        List<KeyValuePair<string, StormElementData>> result = [.. data.GetElementDataAt("TooltipAppender").GetElementData()];

        // assert
        result.Should().HaveCount(3);
        result[0].Value.GetElementDataAt("Face").Value.GetString().Should().Be("SamuroDeflectionTalent");
        result[1].Value.GetElementDataAt("Face").Value.GetString().Should().Be("SamuroPressTheAttack");
        result[2].Value.GetElementDataAt("Face").Value.GetString().Should().Be("SamuroBlademastersPursuitTalent");

        result[0].Value.GetElementDataAt("Validator").Value.GetString().Should().Be("SamuroHasDeflectionTalent");
        result[1].Value.GetElementDataAt("Validator").Value.GetString().Should().Be("SamuroHasPressTheAttack");
        result[2].Value.GetElementDataAt("Validator").Value.GetString().Should().Be("SamuroHasBlademastersPursuit");
    }

    [TestMethod]
    public void ElementData_HasValueAsAnElement_ReturnsFromGetRawValueOnRootElement()
    {
        // arrange
        XElement element = XElement.Parse(
            """
            <CEffectLaunchMissile id="ChromieSandBlastLaunchMissile">
              <ValidatorArray index="0" value="CasterNotDead" />
              <ImpactLocation>
                <ProjectionSourceValue value="OriginPoint" />
                <Value value="TargetPoint" />
                <ProjectionTargetValue value="TargetPoint" />
                <UsesLineDash value="1" />
                <LineDashType value="AllowedInUnpathable" />
                <ProjectionDistanceScale value="15" />
              </ImpactLocation>
            </CEffectLaunchMissile>
            """);

        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        StormElementData data = new(stormElement, element);

        // assert
        data.GetElementDataAt("ImpactLocation").RawValue.Should().Be("TargetPoint");
        data.GetElementDataAt("ImpactLocation").GetElementDataAt("Value").RawValue.Should().Be("TargetPoint");
        data.GetElementDataAt("ImpactLocation").GetElementDataAt("ProjectionDistanceScale").RawValue.Should().Be("15");
    }

    [TestMethod]
    public void ElementData_CUserInstances_ReturnsCorrectValues()
    {
        // arrange
        XElement element = XElement.Parse(
            """
            <CUser id="EndOfMatchGeneralAward">
              <Instances Id="Generic Instance"/>
              <Instances Id="Experienced">
                <Fixed Fixed="12335">
                  <Field Id="Base"/>
                </Fixed>
                <GameLink GameLink="EndOfMatchAwardMostXPContributionBoolean">
                  <Field Id="Score Value Boolean"/>
                </GameLink>
                <String String="01">
                  <Field Id="Award Badge Index"/>
                </String>
                <String String="true">
                  <Field Id="Present as Ratio"/>
                </String>
                <Text Text="UserData/EndOfMatchGeneralAward/Experienced_Award Name">
                  <Field Id="Award Name"/>
                </Text>
                <Text Text="UserData/EndOfMatchGeneralAward/Experienced_Description">
                  <Field Id="Description"/>
                </Text>
                <Text Text="UserData/EndOfMatchGeneralAward/Experienced_Tooltip Text">
                  <Field Id="Tooltip Text"/>
                </Text>
              </Instances>
              <Instances Id="Master of the Curse">
                <Fixed Fixed="1498">
                  <Field Id="Base"/>
                </Fixed>
                <GameLink GameLink="EndOfMatchAwardMostCurseDamageDoneBoolean">
                  <Field Id="Score Value Boolean"/>
                </GameLink>
                <String String="16">
                  <Field Id="Award Badge Index"/>
                </String>
                <String String="true">
                  <Field Id="Present as Ratio"/>
                </String>
                <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Award Name">
                  <Field Id="Award Name"/>
                </Text>
                <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Description">
                  <Field Id="Description"/>
                </Text>
                <Text Text="UserData/EndOfMatchMapSpecificAward/Master of the Curse_Tooltip Text">
                  <Field Id="Tooltip Text"/>
                </Text>
              </Instances>
            </CUser>
            """);

        StormElement stormElement = new(new StormXElementValuePath(element, TestHelpers.GetStormPath("some\\path")));

        // act
        StormElementData data = new(stormElement, element);

        // assert
        data["Instances"]["1"]["String"]["0"]["String"].RawValue.Should().Be("01");
        data["Instances"]["1"]["Text"]["0"]["Text"].RawValue.Should().Be("UserData/EndOfMatchGeneralAward/Experienced_Award Name");
        data["Instances"]["1"]["Text"]["0"]["Field"].RawValue.Should().Be("Award Name");
    }
}