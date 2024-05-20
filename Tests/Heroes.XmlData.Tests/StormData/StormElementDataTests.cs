namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementDataTests
{
    [TestMethod]
    public void StormElementData_AttributesAndElements_ShouldBeSavedAsSameLevelPair()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "Abil/Name/abil1")));

        // act
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.KeyValueDataPairs["default"].Value.Should().Be("1");
        stormElementData.KeyValueDataPairs["name"].Value.Should().Be("Abil/Name/abil1");
    }

    [TestMethod]
    public void StormElementData_EquivalentAttributAndElement_SameKeyValuePair()
    {
        // arrange
        XElement withAttributes = new(
            "CAbil",
            new XAttribute("default", "1"));

        XElement withElement = new(
            "CAbil",
            new XElement(
                "default",
                new XAttribute("value", "1")));

        // act
        StormElementData asAttributes = new(withAttributes);
        StormElementData asElement = new(withElement);

        // assert
        asAttributes.KeyValueDataPairs["default"].Value.Should().Be("1");
        asElement.KeyValueDataPairs["default"].Value.Should().Be("1");
    }

    [TestMethod]
    public void StormElementDataTest_EquivalentArrayAttributAndElement_SameKeyValuePairs()
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
        StormElementData stormElementDataAsAttributes = new(withAttributes);
        StormElementData stormElementDataAsElements = new(withElements);

        // assert
        stormElementDataAsAttributes.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElementDataAsElements.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");

        stormElementDataAsAttributes.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].KeyValueDataPairs.Count.Should().Be(0);
        stormElementDataAsElements.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].KeyValueDataPairs.Count.Should().Be(0);
    }

    [TestMethod]
    public void StormElementData_ArrayWithTextIndex_InnerDataHasSharedKey()
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["DisableWhileDead"].Value.Should().Be("1");
        data.KeyValueDataPairs["SharedFlags"].KeyValueDataPairs["AllowQuickCastCustomization"].Value.Should().Be("1");
    }

    [TestMethod]
    public void StormElementData_ElementWithNoIndexHasAttributesAndElement_AttributeAndElementOnSameLevelKey()
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["ConditionalEvents"].KeyValueDataPairs["0"].KeyValueDataPairs["Compare"].Value.Should().Be("GE");
        data.KeyValueDataPairs["ConditionalEvents"].KeyValueDataPairs["0"].KeyValueDataPairs["CompareValue"].Value.Should().Be("15");
        data.KeyValueDataPairs["ConditionalEvents"].KeyValueDataPairs["0"].KeyValueDataPairs["Event"].KeyValueDataPairs["0"].KeyValueDataPairs["Effect"].Value.Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["0"].KeyValueDataPairs["Field"].Value.Should().Be("AreaArray[0].Radius");
        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["0"].KeyValueDataPairs["Value"].Value.Should().Be("1.600000");
        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["0"].KeyValueDataPairs["Value"].KeyValueDataPairs["0"].Value.Should().Be("1.600000");

        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["2"].KeyValueDataPairs["Field"].Value.Should().Be("AnubarakBurrowChargeCursorSplat");
        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["2"].KeyValueDataPairs["Value"].Value.Should().Be("0.600000");
        data.KeyValueDataPairs["AbilityModificationArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Modifications"].KeyValueDataPairs["2"].KeyValueDataPairs["Value"].KeyValueDataPairs["0"].Value.Should().Be("0.600000");
    }

    [TestMethod]
    public void StormElementDataTest_NumbericalIndexes_ShouldReturnTrue()
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["OrderArray"].HasNumericalIndex.Should().BeTrue();

        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].HasNumericalIndex.Should().BeFalse();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["0"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine0.dds");

        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["1"].HasNumericalIndex.Should().BeFalse();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["1"].KeyValueDataPairs["LineTexture"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["1"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine1.dds");

        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["2"].HasNumericalIndex.Should().BeFalse();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["2"].KeyValueDataPairs["LineTexture"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["OrderArray"].KeyValueDataPairs["2"].KeyValueDataPairs["LineTexture"].KeyValueDataPairs["0"].Value.Should().Be("Assets\\Textures\\Storm_WayPointLine2.dds");
    }

    [TestMethod]
    public void StormElementDataTest_HasTextInnerArray_IndexedArrayShouldHaveTextIndex()
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["HeroAbilArray"].HasNumericalIndex.Should().BeTrue();

        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Abil"].KeyValueDataPairs["0"].Value.Should().Be("KelThuzadDeathAndDecay");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Abil"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].KeyValueDataPairs["ShowInHeroSelect"].Value.Should().Be("1");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].HasNumericalIndex.Should().BeFalse();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].HasTextIndex.Should().BeTrue();

        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Abil"].KeyValueDataPairs["0"].Value.Should().Be("KelThuzadChains");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Abil"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].KeyValueDataPairs["AffectedByOverdrive"].Value.Should().Be("1");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].HasNumericalIndex.Should().BeFalse();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].HasTextIndex.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementDataTest_HasNumericalIndexInnerArray_IndexedArrayShouldHaveNumericalIndex()
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
        StormElementData data = new(element);

        // assert
        data.KeyValueDataPairs["HeroAbilArray"].HasNumericalIndex.Should().BeTrue();

        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Abil"].KeyValueDataPairs["0"].Value.Should().Be("KelThuzadDeathAndDecay");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Abil"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Abil"].HasTextIndex.Should().BeFalse();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].KeyValueDataPairs["0"].Value.Should().Be("1");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["0"].KeyValueDataPairs["Flags"].HasTextIndex.Should().BeFalse();

        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Abil"].KeyValueDataPairs["0"].Value.Should().Be("KelThuzadChains");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Abil"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Abil"].HasTextIndex.Should().BeFalse();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].KeyValueDataPairs["2"].Value.Should().Be("3");
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].HasNumericalIndex.Should().BeTrue();
        data.KeyValueDataPairs["HeroAbilArray"].KeyValueDataPairs["2"].KeyValueDataPairs["Flags"].HasTextIndex.Should().BeFalse();
    }
}