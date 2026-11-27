namespace Heroes.StormData.Cache.Styles.Tests;

[TestClass]
internal sealed class LayeredStyleCacheTests
{
    private readonly StyleCache _baseStyleCache = new();
    private readonly StyleCache _mapStyleCache = new();
    private readonly StyleCache _customStyleCache = new();
    private readonly LayeredStyleCache _layeredStyleCache;

    public LayeredStyleCacheTests()
    {
        _layeredStyleCache = new LayeredStyleCache(_baseStyleCache, _mapStyleCache, _customStyleCache);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void GetStormXmlStyleConstantElementByName_ElementInSingleLayer_ReturnsCachedInstance(StormModType stormModType)
    {
        // arrange
        StormXmlStyleConstantElement expected = AddStyleConstantElement(stormModType, """<Constant name="ColorGray" val="a7a7a7" />""");

        // act
        StormXmlStyleConstantElement? result = _layeredStyleCache.GetStormXmlStyleConstantElementByName("ColorGray", out bool isCopy);

        // assert
        result.Should().BeSameAs(expected);
        isCopy.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlStyleConstantElementByName_NameNotInAnyLayer_ReturnsNull()
    {
        // arrange
        AddStyleConstantElement(StormModType.Base, """<Constant name="ColorGray" val="a7a7a7" />""");

        // act
        StormXmlStyleConstantElement? result = _layeredStyleCache.GetStormXmlStyleConstantElementByName("ColorWhite", out bool isCopy);

        // assert
        result.Should().BeNull();
        isCopy.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlStyleConstantElementByName_NameWithDifferentCasing_ReturnsNull()
    {
        // arrange
        AddStyleConstantElement(StormModType.Base, """<Constant name="ColorGray" val="a7a7a7" />""");

        // act
        StormXmlStyleConstantElement? result = _layeredStyleCache.GetStormXmlStyleConstantElementByName("colorgray");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlStyleConstantElementByName_InAllLayers_ReturnsMergedCopyWithCustomValue()
    {
        // arrange
        StormXmlStyleConstantElement baseElement = AddStyleConstantElement(StormModType.Base, """<Constant name="ColorGray" val="111111" />""");
        StormXmlStyleConstantElement mapElement = AddStyleConstantElement(StormModType.Map, """<Constant name="ColorGray" val="222222" />""");
        StormXmlStyleConstantElement customElement = AddStyleConstantElement(StormModType.Custom, """<Constant name="ColorGray" val="333333" />""");

        // act
        StormXmlStyleConstantElement? result = _layeredStyleCache.GetStormXmlStyleConstantElementByName("ColorGray", out bool isCopy);

        // assert
        result.Should().NotBeNull();
        isCopy.Should().BeTrue();
        result.Should().NotBeSameAs(baseElement);
        result.Value.Should().Be("333333");
        result.StormFiles.Should().HaveCount(3);

        baseElement.Value.Should().Be("111111");
        mapElement.Value.Should().Be("222222");
        customElement.Value.Should().Be("333333");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void GetStormXmlStyleElementByName_ElementInSingleLayer_ReturnsCachedInstance(StormModType stormModType)
    {
        // arrange
        StormXmlStyleElement expected = AddStyleElement(stormModType, """<Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetStormXmlStyleElementByName("ReticleEnemy", out bool isCopy);

        // assert
        result.Should().BeSameAs(expected);
        isCopy.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlStyleElementByName_NameNotInAnyLayer_ReturnsNull()
    {
        // arrange
        AddStyleElement(StormModType.Base, """<Style name="ReticleEnemy" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetStormXmlStyleElementByName("ReticleAlly", out bool isCopy);

        // assert
        result.Should().BeNull();
        isCopy.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlStyleElementByName_NameWithDifferentCasing_ReturnsNull()
    {
        // arrange
        AddStyleElement(StormModType.Base, """<Style name="ReticleEnemy" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetStormXmlStyleElementByName("reticleenemy");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormXmlStyleElementByName_InMultipleLayers_ReturnsMergedCopyAndDoesNotModifyCachedElements()
    {
        // arrange
        StormXmlStyleElement baseElement = AddStyleElement(StormModType.Base, """<Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" />""");
        StormXmlStyleElement mapElement = AddStyleElement(StormModType.Map, """<Style name="ReticleEnemy" fontflags="Shadow" />""");
        StormXmlStyleElement customElement = AddStyleElement(StormModType.Custom, """<Style name="ReticleEnemy" textcolor="255,0,0,255" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetStormXmlStyleElementByName("ReticleEnemy", out bool isCopy);

        // assert
        result.Should().NotBeNull();
        isCopy.Should().BeTrue();
        result.Should().NotBeSameAs(baseElement);
        result.Template.Should().Be("Storm_Tutorial_Reticle_Text");
        result.Members["textcolor"].Value.RawValue.Should().Be("255,0,0,255");
        result.Members["fontflags"].Value.RawValue.Should().Be("Shadow");
        result.StormFiles.Should().HaveCount(3);

        baseElement.Members["textcolor"].Value.RawValue.Should().Be("255,255,255,255");
        baseElement.Members.ContainsMember("fontflags").Should().BeFalse();
        mapElement.HasTemplate.Should().BeFalse();
        customElement.Members.ContainsMember("fontflags").Should().BeFalse();
    }

    [TestMethod]
    public void GetStormXmlStyleElementByName_InMultipleLayers_ReturnsNewInstanceEachCall()
    {
        // arrange
        AddStyleElement(StormModType.Base, """<Style name="ReticleEnemy" textcolor="255,255,255,255" />""");
        AddStyleElement(StormModType.Custom, """<Style name="ReticleEnemy" textcolor="255,0,0,255" />""");

        // act
        StormXmlStyleElement? first = _layeredStyleCache.GetStormXmlStyleElementByName("ReticleEnemy");
        StormXmlStyleElement? second = _layeredStyleCache.GetStormXmlStyleElementByName("ReticleEnemy");

        // assert
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        first.Should().NotBeSameAs(second);
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_StyleHasNoTemplate_ReturnsStoredInstance()
    {
        // arrange
        StormXmlStyleElement element = AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" textcolor="#ColorWhite" height="16" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("StandardTooltip");

        // assert
        result.Should().BeSameAs(element);
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_StyleHasTemplate_IncludesTemplateAttributes()
    {
        // arrange
        AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" textcolor="#ColorWhite" height="16" hjustify="Left" />""");
        AddStyleElement(StormModType.Base, """<Style name="TimerTitle" template="StandardTooltip" height="20" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TimerTitle");

        // assert - the style's own height overrides the template's, the others are inherited
        result.Should().NotBeNull();
        result.Name.Should().Be("TimerTitle");
        result.Template.Should().Be("StandardTooltip");
        result.Members["textcolor"].Value.RawValue.Should().Be("#ColorWhite");
        result.Members["hjustify"].Value.RawValue.Should().Be("Left");
        result.Members["height"].Value.RawValue.Should().Be("20");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateChain_MergesEveryTemplateWithNearestWinning()
    {
        // arrange - TooltipSubscript -> StandardTooltip -> StandardBase
        AddStyleElement(StormModType.Base, """<Style name="StandardBase" textcolor="255,255,255,255" height="12" vjustify="Top" />""");
        AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" template="StandardBase" height="16" hjustify="Left" />""");
        AddStyleElement(StormModType.Base, """<Style name="TooltipSubscript" template="StandardTooltip" textcolor="#ColorGray" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TooltipSubscript");

        // assert
        result.Should().NotBeNull();
        result.Name.Should().Be("TooltipSubscript");
        result.Template.Should().Be("StandardTooltip");
        result.Members["textcolor"].Value.RawValue.Should().Be("#ColorGray");
        result.Members["height"].Value.RawValue.Should().Be("16");
        result.Members["hjustify"].Value.RawValue.Should().Be("Left");
        result.Members["vjustify"].Value.RawValue.Should().Be("Top");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_StyleHasTemplate_DoesNotModifyStoredElements()
    {
        // arrange
        StormXmlStyleElement template = AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" textcolor="#ColorWhite" />""");
        StormXmlStyleElement style = AddStyleElement(StormModType.Base, """<Style name="TimerTitle" template="StandardTooltip" height="20" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TimerTitle");

        // assert
        result.Should().NotBeSameAs(template);
        result.Should().NotBeSameAs(style);
        template.Name.Should().Be("StandardTooltip");
        template.Members.ContainsMember("height").Should().BeFalse();
        style.Members.ContainsMember("textcolor").Should().BeFalse();
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateInOtherLayer_IncludesTemplateAttributes()
    {
        // arrange - a map style whose template is in the base stormmods
        AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" textcolor="#ColorWhite" />""");
        AddStyleElement(StormModType.Map, """<Style name="MapTimer" template="StandardTooltip" height="20" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("MapTimer");

        // assert
        result.Should().NotBeNull();
        result.Members["textcolor"].Value.RawValue.Should().Be("#ColorWhite");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateOverriddenInHigherLayer_UsesOverriddenTemplateAttributes()
    {
        // arrange - the custom layer changes the template's color
        AddStyleElement(StormModType.Base, """<Style name="StandardTooltip" textcolor="#ColorWhite" />""");
        AddStyleElement(StormModType.Custom, """<Style name="StandardTooltip" textcolor="#ColorRed" />""");
        AddStyleElement(StormModType.Base, """<Style name="TimerTitle" template="StandardTooltip" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TimerTitle");

        // assert
        result.Should().NotBeNull();
        result.Members["textcolor"].Value.RawValue.Should().Be("#ColorRed");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateNotFound_ReturnsStyleWithoutTemplateAttributes()
    {
        // arrange
        StormXmlStyleElement element = AddStyleElement(StormModType.Base, """<Style name="TimerTitle" template="MissingTemplate" height="20" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TimerTitle");

        // assert
        result.Should().BeSameAs(element);
        result.Template.Should().Be("MissingTemplate");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateCycle_StopsAtCycle()
    {
        // arrange - A -> B -> A
        AddStyleElement(StormModType.Base, """<Style name="StyleA" template="StyleB" height="10" />""");
        AddStyleElement(StormModType.Base, """<Style name="StyleB" template="StyleA" textcolor="#ColorWhite" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("StyleA");

        // assert
        result.Should().NotBeNull();
        result.Name.Should().Be("StyleA");
        result.Template.Should().Be("StyleB");
        result.Members["height"].Value.RawValue.Should().Be("10");
        result.Members["textcolor"].Value.RawValue.Should().Be("#ColorWhite");
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_TemplateIsItself_ReturnsStyle()
    {
        // arrange
        StormXmlStyleElement element = AddStyleElement(StormModType.Base, """<Style name="StyleA" template="StyleA" height="10" />""");

        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("StyleA");

        // assert
        result.Should().BeSameAs(element);
    }

    [TestMethod]
    public void GetMergedStormXmlStyleElementByName_NameNotInAnyLayer_ReturnsNull()
    {
        // act
        StormXmlStyleElement? result = _layeredStyleCache.GetMergedStormXmlStyleElementByName("TimerTitle");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void ContainsStormXmlStyleElementByName_ElementInSingleLayer_ReturnsTrue(StormModType stormModType)
    {
        // arrange
        AddStyleElement(stormModType, """<Style name="TimerTitle" height="20" />""");

        // act
        bool result = _layeredStyleCache.ContainsStormXmlStyleElementByName("TimerTitle");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("OtherStyle")]
    [DataRow("timertitle")]
    [DataRow("")]
    public void ContainsStormXmlStyleElementByName_NameNotFound_ReturnsFalse(string name)
    {
        // arrange
        AddStyleElement(StormModType.Base, """<Style name="TimerTitle" height="20" />""");

        // act
        bool result = _layeredStyleCache.ContainsStormXmlStyleElementByName(name);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void ContainsStormXmlStyleConstantElementByName_ElementInSingleLayer_ReturnsTrue(StormModType stormModType)
    {
        // arrange
        AddStyleConstantElement(stormModType, """<Constant name="ColorGray" val="a7a7a7" />""");

        // act
        bool result = _layeredStyleCache.ContainsStormXmlStyleConstantElementByName("ColorGray");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("ColorWhite")]
    [DataRow("colorgray")]
    [DataRow("")]
    public void ContainsStormXmlStyleConstantElementByName_NameNotFound_ReturnsFalse(string name)
    {
        // arrange
        AddStyleConstantElement(StormModType.Base, """<Constant name="ColorGray" val="a7a7a7" />""");

        // act
        bool result = _layeredStyleCache.ContainsStormXmlStyleConstantElementByName(name);

        // assert
        result.Should().BeFalse();
    }

    // parses the XML into a style constant element and adds it to the style cache of the given layer
    private StormXmlStyleConstantElement AddStyleConstantElement(StormModType stormModType, string xml)
    {
        StormXmlStyleConstantElement element = XmlReaderHelper.GetStormXmlStyleConstantElement(xml);

        GetStyleCache(stormModType).StormXmlStyleConstantElementByName.Dictionary.Add(element.Name, element);

        return element;
    }

    // parses the XML into a style element and adds it to the style cache of the given layer
    private StormXmlStyleElement AddStyleElement(StormModType stormModType, string xml)
    {
        StormXmlStyleElement element = XmlReaderHelper.GetStormXmlStyleElement(xml);

        GetStyleCache(stormModType).StormXmlStyleElementByName.Dictionary.Add(element.Name, element);

        return element;
    }

    private StyleCache GetStyleCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => _baseStyleCache,
        StormModType.Map => _mapStyleCache,
        StormModType.Custom => _customStyleCache,
        _ => _customStyleCache,
    };
}
