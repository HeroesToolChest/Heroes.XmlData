namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormElementDataTests
{
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
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetXmlData("default").HasValue.Should().BeTrue();
        stormElementData.GetXmlData("default").Value.Should().Be("1");
        stormElementData.GetXmlData("name").HasValue.Should().BeTrue();
        stormElementData.GetXmlData("name").Value.Should().Be("Abil/Name/abil1");
    }

    [TestMethod]
    public void StormElementData_EquivalentAttributAndElement_SameXmlData()
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
        asAttributes.GetXmlData("default").Value.Should().Be("1");
        asElement.GetXmlData("default").Value.Should().Be("1");
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
        StormElementData stormElementDataAsAttributes = new(withAttributes);
        StormElementData stormElementDataAsElements = new(withElements);

        // assert
        stormElementDataAsAttributes.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElementDataAsElements.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");

        stormElementDataAsAttributes.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").XmlDataCount.Should().Be(0);
        stormElementDataAsElements.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").XmlDataCount.Should().Be(0);
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("SharedFlags").GetXmlData("DisableWhileDead").Value.Should().Be("1");
        data.GetXmlData("SharedFlags").GetXmlData("AllowQuickCastCustomization").Value.Should().Be("1");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("ConditionalEvents").GetXmlData("0").GetXmlData("Compare").Value.Should().Be("GE");
        data.GetXmlData("ConditionalEvents").GetXmlData("0").GetXmlData("CompareValue").Value.Should().Be("15");
        data.GetXmlData("ConditionalEvents").GetXmlData("0").GetXmlData("Event").GetXmlData("0").GetXmlData("Effect").Value.Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
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
        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("0").GetXmlData("Field").Value.Should().Be("AreaArray[0].Radius");
        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("0").GetXmlData("Value").Value.Should().Be("1.600000");
        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("0").GetXmlData("Value").GetXmlData("0").Value.Should().Be("1.600000");

        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("2").GetXmlData("Field").Value.Should().Be("AnubarakBurrowChargeCursorSplat");
        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("2").GetXmlData("Value").Value.Should().Be("0.600000");
        data.GetXmlData("AbilityModificationArray").GetXmlData("0").GetXmlData("Modifications").GetXmlData("2").GetXmlData("Value").GetXmlData("0").Value.Should().Be("0.600000");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("OrderArray").HasNumericalIndex.Should().BeTrue();

        data.GetXmlData("OrderArray").GetXmlData("0").HasNumericalIndex.Should().BeFalse();
        data.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine0.dds");

        data.GetXmlData("OrderArray").GetXmlData("1").HasNumericalIndex.Should().BeFalse();
        data.GetXmlData("OrderArray").GetXmlData("1").GetXmlData("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("OrderArray").GetXmlData("1").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine1.dds");

        data.GetXmlData("OrderArray").GetXmlData("2").HasNumericalIndex.Should().BeFalse();
        data.GetXmlData("OrderArray").GetXmlData("2").GetXmlData("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("OrderArray").GetXmlData("2").GetXmlData("LineTexture").GetXmlData("0").Value.Should().Be("Assets\\Textures\\Storm_WayPointLine2.dds");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("HeroAbilArray").HasNumericalIndex.Should().BeTrue();

        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").GetXmlData("0").Value.Should().Be("KelThuzadDeathAndDecay");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("ShowInHeroSelect").Value.Should().Be("1");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").HasNumericalIndex.Should().BeFalse();
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").HasTextIndex.Should().BeTrue();

        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").GetXmlData("0").Value.Should().Be("KelThuzadChains");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("AffectedByOverdrive").Value.Should().Be("1");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").HasNumericalIndex.Should().BeFalse();
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").HasTextIndex.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementData_HasNumericalIndexInnerArray_IndexedArrayShouldHaveNumericalIndex()
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
        data.GetXmlData("HeroAbilArray").HasNumericalIndex.Should().BeTrue();

        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").GetXmlData("0").Value.Should().Be("KelThuzadDeathAndDecay");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").HasTextIndex.Should().BeFalse();
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("0").Value.Should().Be("1");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").HasTextIndex.Should().BeFalse();

        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").GetXmlData("0").Value.Should().Be("KelThuzadChains");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").HasTextIndex.Should().BeFalse();
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("2").Value.Should().Be("3");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").HasNumericalIndex.Should().BeTrue();
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").HasTextIndex.Should().BeFalse();
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
                new XAttribute("value", "$Name"),
                new XAttribute($"{StormModStorage.SelfNameConst}value", "SomeValue")));

        // act
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetXmlData("Name").HasConstValue.Should().BeTrue();
        stormElementData.GetXmlData("Name").ConstValue.Should().Be("SomeValue");
    }

    [TestMethod]
    public void StormElementData_ElementHasConstAttributeThatIsEmtpy_ReturnsNoConstValue()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "$Name"),
                new XAttribute($"{StormModStorage.SelfNameConst}value", string.Empty)));

        // act
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetXmlData("Name").HasConstValue.Should().BeFalse();
    }

    [TestMethod]
    public void StormElementData_HasScalingElement_ReturnsTrueForHasScale()
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
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetXmlData("Damage").HasHxdScale.Should().BeTrue();
        stormElementData.GetXmlData("Damage").ScaleValue.Should().Be("0.1");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("OrderArray").Field.Should().Be("OrderArray");
        data.GetXmlData("OrderArray").GetXmlData("0").Field.Should().Be("OrderArray[0]");
        data.GetXmlData("OrderArray").GetXmlData("0").GetXmlData("LineTexture").Field.Should().Be("OrderArray[0].LineTexture");
        data.GetXmlData("OrderArray").GetXmlData("1").Field.Should().Be("OrderArray[1]");
        data.GetXmlData("OrderArray").GetXmlData("1").GetXmlData("LineTexture").Field.Should().Be("OrderArray[1].LineTexture");
        data.GetXmlData("OrderArray").GetXmlData("2").Field.Should().Be("OrderArray[2]");
        data.GetXmlData("OrderArray").GetXmlData("2").GetXmlData("LineTexture").Field.Should().Be("OrderArray[2].LineTexture");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[0].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[0].Flags[ShowInHeroSelect]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByCooldownReduction]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByOverdrive]");

        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[1].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[1].Flags[ShowInHeroSelect]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByCooldownReduction]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByOverdrive]");

        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[2].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[2].Flags[ShowInHeroSelect]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[2].Flags[AffectedByCooldownReduction]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[2].Flags[AffectedByOverdrive]");
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
        StormElementData data = new(element);

        // assert
        data.GetXmlData("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[0].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("0").Field.Should().Be("HeroAbilArray[0].Flags[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("1").Field.Should().Be("HeroAbilArray[0].Flags[1]");
        data.GetXmlData("HeroAbilArray").GetXmlData("0").GetXmlData("Flags").GetXmlData("2").Field.Should().Be("HeroAbilArray[0].Flags[2]");

        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[1].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("0").Field.Should().Be("HeroAbilArray[1].Flags[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("1").Field.Should().Be("HeroAbilArray[1].Flags[1]");
        data.GetXmlData("HeroAbilArray").GetXmlData("1").GetXmlData("Flags").GetXmlData("2").Field.Should().Be("HeroAbilArray[1].Flags[2]");

        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Abil").GetXmlData("0").Field.Should().Be("HeroAbilArray[2].Abil[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("0").Field.Should().Be("HeroAbilArray[2].Flags[0]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("1").Field.Should().Be("HeroAbilArray[2].Flags[1]");
        data.GetXmlData("HeroAbilArray").GetXmlData("2").GetXmlData("Flags").GetXmlData("2").Field.Should().Be("HeroAbilArray[2].Flags[2]");
    }

    [TestMethod]
    public void Field_ElementHasConstAttribute_ReturnsCorrectField()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "$Name"),
                new XAttribute($"{StormModStorage.SelfNameConst}value", "SomeValue")));

        // act
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetXmlData("Name").Field.Should().Be("Name");
    }

    [TestMethod]
    public void TryGetXmlData_HasData_ReturnsStormElementData()
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
        StormElementData data = new(element);

        // act
        bool result = data.TryGetXmlData("HeroAbilArray", out StormElementData? stormElementData);
        bool resultAsSpan = data.TryGetXmlData("HeroAbilArray".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeTrue();
        resultAsSpan.Should().BeTrue();
        stormElementData!.GetXmlData().ToList().Should().HaveCount(3);
        stormElementDataAsSpan!.GetXmlData().ToList().Should().HaveCount(3);
    }

    [TestMethod]
    public void TryGetXmlData_HasNoData_ReturnsNull()
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
        StormElementData data = new(element);

        // act
        bool result = data.TryGetXmlData("Damage", out StormElementData? stormElementData);
        bool resultAsSpan = data.TryGetXmlData("Damage".AsSpan(), out StormElementData? stormElementDataAsSpan);

        // assert
        result.Should().BeFalse();
        resultAsSpan.Should().BeFalse();
        stormElementData.Should().BeNull();
        stormElementDataAsSpan.Should().BeNull();
    }
}