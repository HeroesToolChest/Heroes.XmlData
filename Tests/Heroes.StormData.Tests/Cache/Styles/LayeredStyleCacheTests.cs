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

    // parses the xml into a style constant element and adds it to the style cache of the given layer
    private StormXmlStyleConstantElement AddStyleConstantElement(StormModType stormModType, string xml)
    {
        StormXmlStyleConstantElement element = XmlReaderHelper.GetStormXmlStyleConstantElement(xml);

        GetStyleCache(stormModType).StormXmlStyleConstantElementByName.Dictionary.Add(element.Name, element);

        return element;
    }

    // parses the xml into a style element and adds it to the style cache of the given layer
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
