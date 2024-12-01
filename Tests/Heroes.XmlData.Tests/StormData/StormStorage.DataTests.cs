using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

public partial class StormStorageTests
{
    [TestMethod]
    [DataRow("$ZagaraHunterKillerDamage", "71")]
    [DataRow("$ChromieBasicAttackRange", "7")]
    [DataRow("$AzmodanAllShallBurnCastRange", "666")]
    public void TryGetFirstExistingConstantXElementById_WithId_ReturnsResultPath(string id, string value)
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse("""<const id="$ZagaraHunterKillerDamage" value="71" />""");
        using XmlObject xmlObject2 = XmlParser.Parse("""<const id="$ChromieBasicAttackRange" value="7" />""");
        using XmlObject xmlObject3 = XmlParser.Parse("""<const id="$AzmodanAllShallBurnCastRange" value="6" />""");
        using XmlObject xmlObject4 = XmlParser.Parse("""<const id="$AzmodanAllShallBurnCastRange" value="666" />""");

        string path = "custom";
        StormStorage stormStorage = new();
        stormStorage.StormCache.ConstantElementById.Add("$ZagaraHunterKillerDamage", new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantElementById.Add("$ChromieBasicAttackRange", new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantElementById.Add("$AzmodanAllShallBurnCastRange", new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath(path)));
        stormStorage.StormCustomCache.ConstantElementById.Add("$AzmodanAllShallBurnCastRange", new StormXmlValuePath(xmlObject4, TestHelpers.GetStormPath(path)));

        // act
        bool result = stormStorage.TryGetFirstConstantElementById(id, out StormXmlValuePath? resultPath);
        bool resultSpan = stormStorage.TryGetFirstConstantElementById(id.AsSpan(), out StormXmlValuePath? resultPathSpan);

        // assert
        result.Should().Be(resultSpan);
        resultPath.Should().BeEquivalentTo(resultPathSpan);
        result.Should().BeTrue();
        resultPath!.StormPath.Path.Should().Be(path);
        XElement.Parse(resultPath.Xml).Attribute("value")!.Value.Should().Be(value);
    }

    [TestMethod]
    public void TryGetFirstExistingConstantXElementById_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetFirstConstantElementById("$Id", out StormXmlValuePath? resultPath);

        // assert
        result.Should().BeFalse();
        resultPath.Should().BeNull();
    }

#if !NET9_0_OR_GREATER
    [TestMethod]
    public void TryGetExistingConstantXElementByIde_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetFirstConstantElementById(null!, out StormXmlValuePath? resultPath);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

#endif
    [TestMethod]
    [DataRow("CEffectDamage", "Effect")]
    [DataRow("CEffectLaunchMissile", "Effect")]
    [DataRow("CActorModel", "Actor")]
    [DataRow("CUnit", "Unit")]
    public void TryGetExistingDataObjectTypeByElementType_ElementTypes_ReturnsDataObjectType(string elementType, string dataObjectType)
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CEffectLaunchMissile", "Effect");
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CUnit", "Unit");
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CActorModel", "Actor");
        stormStorage.StormMapCache.DataObjectTypeByElementType.Add("CEffectLaunchMissile", "Effect");
        stormStorage.StormMapCache.DataObjectTypeByElementType.Add("CUnit", "Unit");
        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");

        // act
        bool result = stormStorage.TryGetFirstDataObjectTypeByElementType(elementType, out string? resultDataObjectType);

        // assert
        result.Should().BeTrue();
        resultDataObjectType.Should().Be(dataObjectType);
    }

    [TestMethod]
    public void TryGetExistingDataObjectTypeByElementType_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetFirstDataObjectTypeByElementType("elementType", out string? resultDataObjectType);

        // assert
        result.Should().BeFalse();
        resultDataObjectType.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingDataObjectTypeByElementType_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetFirstDataObjectTypeByElementType(null!, out string? resultDataObjectType);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetDataObjectTypeByElementType_HasElementType_ReturnsResult()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");

        // act
        string? result = stormStorage.GetDataObjectTypeByElementType("CEffectDamage");

        // assert
        result.Should().Be("Effect");
    }

    [TestMethod]
    public void GetDataObjectTypeByElementType_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        string? result = stormStorage.GetDataObjectTypeByElementType("CEffectDamage");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetDataObjectTypeByElementType_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        string newValue = "modified";

        StormStorage stormStorage = new(false);
        stormStorage.StormCache.DataObjectTypeByElementType.Add("CEffectDamage", "Effect");

        // act
        string? result = stormStorage.GetDataObjectTypeByElementType("CEffectDamage");
        result = newValue;

        // assert
        stormStorage.StormCache.DataObjectTypeByElementType["CEffectDamage"].Should().NotBe(newValue);
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_DataObjectTypeInAllThreeCaches_ReturnsElements()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectDamage", "CEffectLaunchMissile"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Actor", ["CActorModel"]);

        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectLaunchMissile"]);
        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);

        stormStorage.StormCustomCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectSet"]);
        stormStorage.StormCustomCache.ElementTypesByDataObjectType.Add("Behavior", ["CBehaviorBuff", "CBehaviorAbility"]);

        // act
        List<string> result = stormStorage.GetElementTypesByDataObjectType("Effect");

        // assert
        result.Should().Equal("CEffectSet", "CEffectLaunchMissile", "CEffectDamage");
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_DataObjectTypeNotInCustomCache_ReturnsElements()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectDamage", "CEffectLaunchMissile"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Actor", ["CActorModel"]);

        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectLaunchMissile"]);
        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);

        // act
        List<string> result = stormStorage.GetElementTypesByDataObjectType("Effect");

        // assert
        result.Should().Equal("CEffectLaunchMissile", "CEffectDamage");
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_DataObjectTypeOnlyInNormalCache_ReturnsElements()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectDamage", "CEffectLaunchMissile"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Actor", ["CActorModel"]);

        // act
        List<string> result = stormStorage.GetElementTypesByDataObjectType("Effect");

        // assert
        result.Should().Equal("CEffectDamage", "CEffectLaunchMissile");
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_HasNoDataObjectType_ReturnsEmpty()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        List<string> result = stormStorage.GetElementTypesByDataObjectType("Effect");

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectDamage", "CEffectLaunchMissile"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);
        stormStorage.StormCache.ElementTypesByDataObjectType.Add("Actor", ["CActorModel"]);

        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectLaunchMissile"]);
        stormStorage.StormMapCache.ElementTypesByDataObjectType.Add("Unit", ["CUnit"]);

        stormStorage.StormCustomCache.ElementTypesByDataObjectType.Add("Effect", ["CEffectSet"]);
        stormStorage.StormCustomCache.ElementTypesByDataObjectType.Add("Behavior", ["CBehaviorBuff", "CBehaviorAbility"]);

        // act
        List<string>? result = stormStorage.GetElementTypesByDataObjectType("Effect");
        result!.Add("CWhatever");

        // assert
        stormStorage.StormCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectDamage", "CEffectLaunchMissile");
        stormStorage.StormMapCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectLaunchMissile");
        stormStorage.StormCustomCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectSet");
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
              <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));
        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element3").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInNormalAndMapCache_ReturnsMergedFromNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInNormalCache_ReturnsMergedFromNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("normal"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");
        StormElement? resultSpan = stormStorage.GetStormElementByElementType("CUnit");

        // assert
        result.Should().BeEquivalentTo(resultSpan, options => options.Excluding(e => e.Type.IsByRefLike));
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInMapCache_ReturnsMergedFromMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementByElementType_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementByElementType_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

        using XmlObject addXmlObject = XmlParser.Parse(
            """
            <CUnit default="1" parent="normal">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
                <Element5 value="value5" />
                <Element6 value="value6" />
            </CUnit>
            """);
        result!.AddValue(new StormElement(new StormXmlValuePath(addXmlObject, TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element3").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("normal")))
                },
            });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map")))
                },
            });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementById_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementById_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
                <Element3 value="value3" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
                <Element5 value="value5" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

        using XmlObject addXml = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
                <Element5 value="value5" />
                <Element6 value="value6" />
            </CUnit>
            """);
        result!.AddValue(new StormElement(new StormXmlValuePath(addXml, TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element3").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("normal")))
                },
            });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map")))
                },
            });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        // assert
        result!.DataValues.GetElementDataAt("Name").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element1").HasValue.Should().BeTrue();
        result.DataValues.GetElementDataAt("Element2").HasValue.Should().BeTrue();
        result.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element1 value="value1" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element2 value="value2" />
                <Element3 value="value3" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
                <Element5 value="value5" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

        using XmlObject addXml = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
                <Element3 value="value3" />
                <Element4 value="value4" />
                <Element5 value="value5" />
                <Element6 value="value6" />
            </CUnit>
            """);

        result!.AddValue(new StormElement(new StormXmlValuePath(addXml, TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetCompleteStormElement_NoElements_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetCompleteStormElement_HasNoParentId_ReturnsStormElement()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CEffect default="1">
                <Chance value="1" />
            </CEffect>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CEffectApplyBehavior default="1">
                <Behavior value="##id##" />
                <ValidatorArray value="##id##TargetFilters" />
            </CEffectApplyBehavior>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CEffectApplyBehavior id="ZagaraInfestApplyBuffBehavior">
                <ValidatorArray index="0" value="IsRangedMinion" />
            </CEffectApplyBehavior>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffect", "Effect");
        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("custom"))));
        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffectApplyBehavior", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("custom"))));
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
            {
                {
                    "ZagaraInfestApplyBuffBehavior", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXmlElements.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetCompleteStormElement_OnlyParentElement_ReturnsStormElement()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CEffectModifyTokenCount default="1" id="BaseEffectModifyTokenCount">
                <Value value="1" />
            </CEffectModifyTokenCount>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CEffectModifyTokenCount id="KelThuzadMasterOfTheColdDarkModifyToken" parent="BaseEffectModifyTokenCount">
                <ValidatorArray value="TargetIsHero" />
            </CEffectModifyTokenCount>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
            {
                {
                    "BaseEffectModifyTokenCount", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("custom")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType["Effect"].Add(
            "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXmlElements.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetCompleteStormElement_HasParentIdButNotFound_ReturnsStormElement()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <CEffectModifyTokenCount id="KelThuzadMasterOfTheColdDarkModifyToken" parent="BaseEffectModifyTokenCount">
                <ValidatorArray value="TargetIsHero" />
            </CEffectModifyTokenCount>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
            {
                {
                    "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXmlElements.Should().ContainSingle();
    }

    [TestMethod]
    public void GetCompleteStormElement_BaseElementSameAsTypeElement_ShouldNotHaveDuplicate()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit default="1">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit default="1">
                <TauntDoesntStopUnit index="Cheer" value="1" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit default="1">
                <TauntDoesntStopUnit index="Cheer" value="1" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CUnit", "Unit");

        StormElement baseStormElement = new(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("custom")));
        baseStormElement.AddValue(new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("custom"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", baseStormElement);
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "StormBasicHeroicUnit", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("StormBasicHeroicUnit", "Unit");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXmlElements.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetBaseStormElement_FoundStormElementFromDataObjectType_ReturnsStormElement()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
                <CEffect default="1">
                  <Chance value="1" />
                </CEffect>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");
        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.DataValues.ElementDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetBaseStormElement_DidNotFindStormElementFromDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior");

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetBaseStormElement_DidNotFindFromDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior");

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" other="value" />
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" other2="value2" />
            """);
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));
        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("normal"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInCustomCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4ff" other="value" />
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_NullDataObjectType_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.GetStormStyleConstantElementsByName(null!);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" other="value" />
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" other="value" other2="value2" />
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));
        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleConstantElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        using XmlObject addXml = XmlParser.Parse(
            """
            <Constant name="TooltipNumbers" val="bfd4fd" other2="value2" other3="value3" other4="value4" other5="value5" />
            """);

        result!.AddValue(new StormStyleConstantElement(new StormXmlValuePath(addXml, TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(2);
        stormStorage.StormMapCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormCustomCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" height="80" />
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" styleflags="Shadow" />
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));
        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" />
            """);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("normal"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" height="80" />
            """);

        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("map"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInCustomCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" styleflags="Shadow" />
            """);

        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" height="80" />
            """);
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormElement? resultSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        // assert
        result.Should().BeEquivalentTo(resultSpan);
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_NullDataObjectType_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.GetStormStyleStyleElementsByName(null!);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" />
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" height="80" />
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" styleflags="Shadow" height="80" />
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal"))));
        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map"))));
        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        using XmlObject addXml = XmlParser.Parse(
            """
            <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" styleflags="Shadow" shadowoffset="3" />
            """);

        result!.AddValue(new StormStyleStyleElement(new StormXmlValuePath(addXml, TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormStyleStyleElementsByName["ReticleEnemy"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormStyleStyleElementsByName["ReticleEnemy"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormStyleStyleElementsByName["ReticleEnemy"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormGameString_AllThreeCaches_MergesFromAll()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormCache.GameStringsById.Add("id1", new GameStringText("If Chomp hits a Hero", TestHelpers.GetStormPath("normal")));
        stormStorage.StormMapCache.GameStringsById.Add("id1", new GameStringText("Shadow Waltz deals an increased", TestHelpers.GetStormPath("map")));
        stormStorage.StormCustomCache.GameStringsById.Add("id1", new GameStringText("After a short delay", TestHelpers.GetStormPath("custom")));

        // assert
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1");

        stormGameString.Should().NotBeNull();
        stormGameString!.Id.Should().Be("id1");
        stormGameString.Value.Should().Be("After a short delay");
        stormGameString.StormPaths.Should().HaveCount(3);
        stormGameString.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormGameString_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormCache.GameStringsById.Add("id1", new GameStringText("If Chomp hits a Hero", TestHelpers.GetStormPath("normal")));

        // assert
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1");

        stormGameString.Should().NotBeNull();
        stormGameString!.Id.Should().Be("id1");
        stormGameString.Value.Should().Be("If Chomp hits a Hero");
        stormGameString.StormPaths.Should().ContainSingle();
        stormGameString.StormPaths[^1].Path.Should().Be("normal");
    }

    [TestMethod]
    public void GetStormGameString_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormMapCache.GameStringsById.Add("id1", new GameStringText("Shadow Waltz deals an increased", TestHelpers.GetStormPath("map")));

        // assert
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1");

        stormGameString.Should().NotBeNull();
        stormGameString!.Id.Should().Be("id1");
        stormGameString.Value.Should().Be("Shadow Waltz deals an increased");
        stormGameString.StormPaths.Should().ContainSingle();
        stormGameString.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void GetStormGameString_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormCustomCache.GameStringsById.Add("id1", new GameStringText("After a short delay", TestHelpers.GetStormPath("custom")));

        // assert
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1");

        stormGameString.Should().NotBeNull();
        stormGameString!.Id.Should().Be("id1");
        stormGameString.Value.Should().Be("After a short delay");
        stormGameString.StormPaths.Should().ContainSingle();
        stormGameString.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormGameString_NormalAndMapCache_MergeFromNormalAndMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormCache.GameStringsById.Add("id1", new GameStringText("If Chomp hits a Hero", TestHelpers.GetStormPath("normal")));
        stormStorage.StormMapCache.GameStringsById.Add("id1", new GameStringText("Shadow Waltz deals an increased", TestHelpers.GetStormPath("map")));

        // assert
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1");

        stormGameString.Should().NotBeNull();
        stormGameString!.Id.Should().Be("id1");
        stormGameString.Value.Should().Be("Shadow Waltz deals an increased");
        stormGameString.StormPaths.Should().HaveCount(2);
        stormGameString.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void GetStormGameStrings_AllThreeCaches_ReturnsFromAll()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.StormCache.GameStringsById.Add("id1", new GameStringText("If Chomp hits a Hero", TestHelpers.GetStormPath("normal")));
        stormStorage.StormCache.GameStringsById.Add("id2", new GameStringText("Spawn an Overgrowth that", TestHelpers.GetStormPath("normal")));

        stormStorage.StormMapCache.GameStringsById.Add("id1", new GameStringText("Shadow Waltz deals an increased", TestHelpers.GetStormPath("map")));
        stormStorage.StormMapCache.GameStringsById.Add("id3", new GameStringText("Hitting a Warp Rift", TestHelpers.GetStormPath("map")));

        stormStorage.StormCustomCache.GameStringsById.Add("id1", new GameStringText("After a short delay", TestHelpers.GetStormPath("custom")));
        stormStorage.StormCustomCache.GameStringsById.Add("id4", new GameStringText("Raynor and all nearby allied", TestHelpers.GetStormPath("custom")));

        // assert
        List<StormGameString> stormGameString = stormStorage.GetStormGameStrings();

        stormGameString.Should().SatisfyRespectively(
            first =>
            {
                first.Id.Should().Be("id1");
                first.Value.Should().Be("After a short delay");
                first.StormPaths.Should().SatisfyRespectively(
                    firstPath =>
                    {
                        firstPath.Path.Should().Be("normal");
                    },
                    secondPath =>
                    {
                        secondPath.Path.Should().Be("map");
                    },
                    thirdPath =>
                    {
                        thirdPath.Path.Should().Be("custom");
                    });
            },
            second =>
            {
                second.Id.Should().Be("id2");
                second.Value.Should().Be("Spawn an Overgrowth that");
                second.StormPaths.Should().SatisfyRespectively(
                    firstPath =>
                    {
                        firstPath.Path.Should().Be("normal");
                    });
            },
            third =>
            {
                third.Id.Should().Be("id3");
                third.Value.Should().Be("Hitting a Warp Rift");
                third.StormPaths.Should().SatisfyRespectively(
                    firstPath =>
                    {
                        firstPath.Path.Should().Be("map");
                    });
            },
            fourth =>
            {
                fourth.Id.Should().Be("id4");
                fourth.Value.Should().Be("Raynor and all nearby allied");
                fourth.StormPaths.Should().SatisfyRespectively(
                    firstPath =>
                    {
                        firstPath.Path.Should().Be("custom");
                    });
            });
    }

    [TestMethod]
    public void GetStormElementIds_HasStormElements_ReturnsIds()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero2">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero3">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero2", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero3", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        List<string> ids = stormStorage.GetStormElementIds("Unit");

        // assert
        ids.Should().HaveCount(3)
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().Be("Hero1");
                },
                second =>
                {
                    second.Should().Be("Hero2");
                },
                third =>
                {
                    third.Should().Be("Hero3");
                });
    }

    [TestMethod]
    public void GetStormElementIds_OnlySelectFromNormal_ReturnsIdFromNormal()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero2">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero3">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero2", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero3", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        List<string> ids = stormStorage.GetStormElementIds("Unit", StormCacheType.Normal);

        // assert
        ids.Should().ContainSingle()
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().Be("Hero1");
                });
    }

    [TestMethod]
    public void GetStormElementIds_OnlySelectFromMap_ReturnsIdFromMap()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero2">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero3">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero2", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero3", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        List<string> ids = stormStorage.GetStormElementIds("Unit", StormCacheType.Map);

        // assert
        ids.Should().ContainSingle()
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().Be("Hero2");
                });
    }

    [TestMethod]
    public void GetStormElementIds_OnlySelectFromCustom_ReturnsIdFromCustom()
    {
        // arrange
        using XmlObject xmlObject1 = XmlParser.Parse(
            """
            <CUnit id="Hero1">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject2 = XmlParser.Parse(
            """
            <CUnit id="Hero2">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        using XmlObject xmlObject3 = XmlParser.Parse(
            """
            <CUnit id="Hero3">
                <Name value="Unit/Name/##id##" />
            </CUnit>
            """);

        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero1", new StormElement(new StormXmlValuePath(xmlObject1, TestHelpers.GetStormPath("normal")))
                },
            });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero2", new StormElement(new StormXmlValuePath(xmlObject2, TestHelpers.GetStormPath("map")))
                },
            });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
            {
                {
                    "Hero3", new StormElement(new StormXmlValuePath(xmlObject3, TestHelpers.GetStormPath("custom")))
                },
            });

        // act
        List<string> ids = stormStorage.GetStormElementIds("Unit", StormCacheType.Custom);

        // assert
        ids.Should().ContainSingle()
            .And
            .SatisfyRespectively(
                first =>
                {
                    first.Should().Be("Hero3");
                });
    }

    [TestMethod]
    public void GetStormElementIds_HasNoStormElements_ReturnsEmpty()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        List<string> ids = stormStorage.GetStormElementIds("Unit");

        // assert
        ids.Should().BeEmpty();
    }

    [TestMethod]
    public void GetStormAssetString_AllThreeCaches_MergesFromAll()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetTextsById.Add("id1", new AssetText("storm_ui_ingame_tooltipframe.dds", TestHelpers.GetStormPath("normal")));
        stormStorage.StormMapCache.AssetTextsById.Add("id1", new AssetText("storm_standardbuttonmini_gold_normal.dds", TestHelpers.GetStormPath("map")));
        stormStorage.StormCustomCache.AssetTextsById.Add("id1", new AssetText("storm_tutorial_veteran.ogv", TestHelpers.GetStormPath("custom")));

        // act
        StormAssetString? stormAssetString = stormStorage.GetStormAssetString("id1");

        // assert
        stormAssetString.Should().NotBeNull();
        stormAssetString!.Id.Should().Be("id1");
        stormAssetString.Value.Should().Be("storm_tutorial_veteran.ogv");
        stormAssetString.StormPaths.Should().HaveCount(3);
        stormAssetString.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormAssetString_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetTextsById.Add("id1", new AssetText("storm_ui_ingame_tooltipframe.dds", TestHelpers.GetStormPath("normal")));

        // act
        StormAssetString? stormAssetString = stormStorage.GetStormAssetString("id1");

        // assert
        stormAssetString.Should().NotBeNull();
        stormAssetString!.Id.Should().Be("id1");
        stormAssetString.Value.Should().Be("storm_ui_ingame_tooltipframe.dds");
        stormAssetString.StormPaths.Should().ContainSingle();
        stormAssetString.StormPaths[^1].Path.Should().Be("normal");
    }

    [TestMethod]
    public void GetStormAssetString_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.AssetTextsById.Add("id1", new AssetText("storm_standardbuttonmini_gold_normal.dds", TestHelpers.GetStormPath("map")));

        // act
        StormAssetString? stormAssetString = stormStorage.GetStormAssetString("id1");

        // assert
        stormAssetString.Should().NotBeNull();
        stormAssetString!.Id.Should().Be("id1");
        stormAssetString.Value.Should().Be("storm_standardbuttonmini_gold_normal.dds");
        stormAssetString.StormPaths.Should().ContainSingle();
        stormAssetString.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void GetStormAssetString_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.AssetTextsById.Add("id1", new AssetText("storm_tutorial_veteran.ogv", TestHelpers.GetStormPath("custom")));

        // act
        StormAssetString? stormAssetString = stormStorage.GetStormAssetString("id1");

        // assert
        stormAssetString.Should().NotBeNull();
        stormAssetString!.Id.Should().Be("id1");
        stormAssetString.Value.Should().Be("storm_tutorial_veteran.ogv");
        stormAssetString.StormPaths.Should().ContainSingle();
        stormAssetString.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormAssetString_NormalAndMapCache_MergeFromNormalAndMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetTextsById.Add("id1", new AssetText("storm_standardbuttonmini_gold_normal.dds", TestHelpers.GetStormPath("normal")));
        stormStorage.StormMapCache.AssetTextsById.Add("id1", new AssetText("storm_tutorial_veteran.ogv", TestHelpers.GetStormPath("map")));

        // act
        StormAssetString? stormAssetString = stormStorage.GetStormAssetString("id1");

        // assert
        stormAssetString.Should().NotBeNull();
        stormAssetString!.Id.Should().Be("id1");
        stormAssetString.Value.Should().Be("storm_tutorial_veteran.ogv");
        stormAssetString.StormPaths.Should().HaveCount(2);
        stormAssetString.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void StormLayoutFileExists_AllThreeCaches_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));
        stormStorage.StormCustomCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("custom"));

        // act
        bool exists = stormStorage.StormLayoutFileExists(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));

        // act
        bool exists = stormStorage.StormLayoutFileExists(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));

        // act
        bool exists = stormStorage.StormLayoutFileExists(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("custom"));

        // act
        bool exists = stormStorage.StormLayoutFileExists(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormLayoutFileExists_InNoCache_ReturnFalse()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));

        // act
        bool exists = stormStorage.StormLayoutFileExists(Path.Join("ui", "layout", "item2.stormlayout"));

        // assert
        exists.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormLayoutFile_AllThreeCaches_MergesFromAll()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));
        stormStorage.StormCustomCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("custom"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("custom");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().HaveCount(3);
        stormAssetFile.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormLayoutFile_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("normal");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("normal");
    }

    [TestMethod]
    public void GetStormLayoutFile_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("map");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void GetStormLayoutFile_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("custom"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("custom");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormLayoutFile_NormalAndMapCache_MergeFromNormalAndMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "item1.stormlayout"), TestHelpers.GetStormPath("map"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(Path.Join("ui", "layout", "item1.stormlayout"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("map");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().HaveCount(2);
        stormAssetFile.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void StormAssetFileExists_AllThreeCaches_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));
        stormStorage.StormCustomCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("custom"));

        // act
        bool exists = stormStorage.StormAssetFileExists(Path.Join("assets", "item1.dds"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormAssetFileExists_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));

        // act
        bool exists = stormStorage.StormAssetFileExists(Path.Join("assets", "item1.dds"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormAssetFileExists_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));

        // act
        bool exists = stormStorage.StormAssetFileExists(Path.Join("assets", "item1.dds"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormAssetFileExists_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("custom"));

        // act
        bool exists = stormStorage.StormAssetFileExists(Path.Join("assets", "item1.dds"));

        // assert
        exists.Should().BeTrue();
    }

    [TestMethod]
    public void StormAssetFileExists_InNoCache_ReturnFalse()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));

        // act
        bool exists = stormStorage.StormAssetFileExists(Path.Join("assets", "item2.dds"));

        // assert
        exists.Should().BeFalse();
    }

    [TestMethod]
    public void GetStormAssetFile_AllThreeCaches_MergesFromAll()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));
        stormStorage.StormCustomCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("custom"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(Path.Join("assets", "item1.dds"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("custom");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().HaveCount(3);
        stormAssetFile.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormAssetFile_InNormalCache_ReturnsFromNormal()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(Path.Join("assets", "item1.dds"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("normal");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("normal");
    }

    [TestMethod]
    public void GetStormAssetFile_InMapCache_ReturnsFromMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(Path.Join("assets", "item1.dds"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("map");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("map");
    }

    [TestMethod]
    public void GetStormAssetFile_InCustomCache_ReturnsFromCustom()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("custom"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(Path.Join("assets", "item1.dds"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("custom");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().ContainSingle();
        stormAssetFile.StormPaths[^1].Path.Should().Be("custom");
    }

    [TestMethod]
    public void GetStormAssetFile_NormalAndMapCache_MergeFromNormalAndMap()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("normal"));
        stormStorage.StormMapCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "item1.dds"), TestHelpers.GetStormPath("map"));

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(Path.Join("assets", "item1.dds"));

        // assert
        stormAssetFile.Should().NotBeNull();
        stormAssetFile!.StormPath.Path.Should().Be("map");
        stormAssetFile.ToString().Should().Be(stormAssetFile.StormPath.Path);
        stormAssetFile.StormPaths.Should().HaveCount(2);
        stormAssetFile.StormPaths[^1].Path.Should().Be("map");
    }
}
