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
        stormElementData.GetElementDataAt("default").HasValue.Should().BeTrue();
        stormElementData.GetElementDataAt("default").RawValue.Should().Be("1");
        stormElementData.GetElementDataAt("name").HasValue.Should().BeTrue();
        stormElementData.GetElementDataAt("name").RawValue.Should().Be("Abil/Name/abil1");
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
        asAttributes.GetElementDataAt("default").RawValue.Should().Be("1");
        asElement.GetElementDataAt("default").RawValue.Should().Be("1");
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
        stormElementDataAsAttributes.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");
        stormElementDataAsElements.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine.dds");

        stormElementDataAsAttributes.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").ElementDataCount.Should().Be(0);
        stormElementDataAsElements.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").ElementDataCount.Should().Be(0);
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
        StormElementData data = new(element);

        // assert
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("Compare").RawValue.Should().Be("GE");
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("CompareValue").RawValue.Should().Be("15");
        data.GetElementDataAt("ConditionalEvents").GetElementDataAt("0").GetElementDataAt("Event").GetElementDataAt("0").GetElementDataAt("Effect").RawValue.Should().Be("KelThuzadMasterOfTheColdDarkTier1ModifyPlayer");
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
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Field").RawValue.Should().Be("AreaArray[0].Radius");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Value").RawValue.Should().Be("1.600000");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("0").GetElementDataAt("Value").GetElementDataAt("0").RawValue.Should().Be("1.600000");

        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Field").RawValue.Should().Be("AnubarakBurrowChargeCursorSplat");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Value").RawValue.Should().Be("0.600000");
        data.GetElementDataAt("AbilityModificationArray").GetElementDataAt("0").GetElementDataAt("Modifications").GetElementDataAt("2").GetElementDataAt("Value").GetElementDataAt("0").RawValue.Should().Be("0.600000");
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
        data.GetElementDataAt("OrderArray").HasNumericalIndex.Should().BeTrue();

        data.GetElementDataAt("OrderArray").GetElementDataAt("0").HasNumericalIndex.Should().BeFalse();
        data.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("OrderArray").GetElementDataAt("0").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine0.dds");

        data.GetElementDataAt("OrderArray").GetElementDataAt("1").HasNumericalIndex.Should().BeFalse();
        data.GetElementDataAt("OrderArray").GetElementDataAt("1").GetElementDataAt("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("OrderArray").GetElementDataAt("1").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine1.dds");

        data.GetElementDataAt("OrderArray").GetElementDataAt("2").HasNumericalIndex.Should().BeFalse();
        data.GetElementDataAt("OrderArray").GetElementDataAt("2").GetElementDataAt("LineTexture").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("OrderArray").GetElementDataAt("2").GetElementDataAt("LineTexture").GetElementDataAt("0").RawValue.Should().Be("Assets\\Textures\\Storm_WayPointLine2.dds");
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
        data.GetElementDataAt("HeroAbilArray").HasNumericalIndex.Should().BeTrue();

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").GetElementDataAt("0").RawValue.Should().Be("KelThuzadDeathAndDecay");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").RawValue.Should().Be("1");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").HasNumericalIndex.Should().BeFalse();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").HasTextIndex.Should().BeTrue();

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").GetElementDataAt("0").RawValue.Should().Be("KelThuzadChains");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").RawValue.Should().Be("1");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").HasNumericalIndex.Should().BeFalse();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").HasTextIndex.Should().BeTrue();
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
        data.GetElementDataAt("HeroAbilArray").HasNumericalIndex.Should().BeTrue();

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").GetElementDataAt("0").RawValue.Should().Be("KelThuzadDeathAndDecay");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").HasTextIndex.Should().BeFalse();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("0").RawValue.Should().Be("1");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").HasTextIndex.Should().BeFalse();

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").GetElementDataAt("0").RawValue.Should().Be("KelThuzadChains");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").HasTextIndex.Should().BeFalse();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("2").RawValue.Should().Be("3");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").HasNumericalIndex.Should().BeTrue();
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").HasTextIndex.Should().BeFalse();
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
        stormElementData.GetElementDataAt("Name").IsConstValue.Should().BeTrue();
        stormElementData.GetElementDataAt("Name").Value.GetString().Should().Be("SomeValue");
    }

    [TestMethod]
    public void StormElementData_ElementHasIntConstAttribute_ReturnsInt()
    {
        // arrange
        XElement xElement = new(
            "CAbil",
            new XAttribute("default", "1"),
            new XElement(
                "Name",
                new XAttribute("value", "$Name"),
                new XAttribute($"{StormModStorage.SelfNameConst}value", "5")));

        // act
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetElementDataAt("Name").IsConstValue.Should().BeTrue();
        stormElementData.GetElementDataAt("Name").Value.GetAsInt().Should().Be(5);
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
        stormElementData.GetElementDataAt("Name").IsConstValue.Should().BeFalse();
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
        StormElementData stormElementData = new(xElement);

        // assert
        stormElementData.GetElementDataAt("Damage").HasHxdScale.Should().BeTrue();
        stormElementData.GetElementDataAt("Damage").HxdScaleValue.GetString().Should().Be("0.1");
        stormElementData.GetElementDataAt("Damage").HxdScaleValue.GetAsDouble().Should().Be(0.1);
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
        StormElementData stormElementData = new(xElement);

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
        StormElementData data = new(element);

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
        StormElementData data = new(element);

        // assert
        data.GetElementDataAt("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[0].Abil[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[0].Flags[ShowInHeroSelect]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByCooldownReduction]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[0].Flags[AffectedByOverdrive]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[1].Abil[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("ShowInHeroSelect").Field.Should().Be("HeroAbilArray[1].Flags[ShowInHeroSelect]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("AffectedByCooldownReduction").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByCooldownReduction]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("AffectedByOverdrive").Field.Should().Be("HeroAbilArray[1].Flags[AffectedByOverdrive]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[2].Abil[0]");
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
        StormElementData data = new(element);

        // assert
        data.GetElementDataAt("HeroAbilArray").Field.Should().Be("HeroAbilArray");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[0].Abil[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[0].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[0].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("0").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[0].Flags[2]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[1].Abil[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[1].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[1].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("1").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[1].Flags[2]");

        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Abil").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[2].Abil[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("0").Field.Should().Be("HeroAbilArray[2].Flags[0]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("1").Field.Should().Be("HeroAbilArray[2].Flags[1]");
        data.GetElementDataAt("HeroAbilArray").GetElementDataAt("2").GetElementDataAt("Flags").GetElementDataAt("2").Field.Should().Be("HeroAbilArray[2].Flags[2]");
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
        stormElementData.GetElementDataAt("Name").Field.Should().Be("Name");
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
        StormElementData data = new(element);

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
        StormElementData data = new(element);

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
        StormElementData data = new(element);

        // act
        Action action = () => data.GetElementDataAt("Does Not Exists");
        Action actionSpan = () => data.GetElementDataAt("Does Not Exists".AsSpan());

        // assert
        action.Should().Throw<KeyNotFoundException>();
        actionSpan.Should().Throw<KeyNotFoundException>();
    }
}