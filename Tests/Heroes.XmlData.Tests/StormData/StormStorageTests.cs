using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStorageTests
{
    [TestMethod]
    [DataRow("$ZagaraHunterKillerDamage", "71")]
    [DataRow("$ChromieBasicAttackRange", "7")]
    [DataRow("$AzmodanAllShallBurnCastRange", "666")]
    public void TryGetExistingConstantXElementById_WithId_ReturnsResultPath(string id, string value)
    {
        // arrange
        string path = "custom";
        StormStorage stormStorage = new();
        stormStorage.StormCache.ConstantXElementById.Add("$ZagaraHunterKillerDamage", new StormXElementValuePath(XElement.Parse(@"<const id=""$ZagaraHunterKillerDamage"" value=""71"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantXElementById.Add("$ChromieBasicAttackRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$ChromieBasicAttackRange"" value=""7"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""6"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormCustomCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""666"" />"), TestHelpers.GetStormPath(path)));

        // act
        bool result = stormStorage.TryGetExistingConstantXElementById(id.AsSpan(), out StormXElementValuePath? resultPath);

        // assert
        result.Should().BeTrue();
        resultPath!.StormPath.Path.Should().Be(path);
        resultPath.Value.Attribute("value")!.Value.Should().Be(value);
    }

    [TestMethod]
    public void TryGetExistingConstantXElementById_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetExistingConstantXElementById("$Id", out StormXElementValuePath? resultPath);

        // assert
        result.Should().BeFalse();
        resultPath.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingConstantXElementByIde_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingConstantXElementById(null!, out StormXElementValuePath? resultPath);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

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
        bool result = stormStorage.TryGetExistingDataObjectTypeByElementType(elementType.AsSpan(), out string? resultDataObjectType);

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
        bool result = stormStorage.TryGetExistingDataObjectTypeByElementType("elementType".AsSpan(), out string? resultDataObjectType);

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
        Action result = () => stormStorage.TryGetExistingDataObjectTypeByElementType(null!, out string? resultDataObjectType);

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
        string? result = stormStorage.GetDataObjectTypeByElementType("CEffectDamage".AsSpan());

        // assert
        result.Should().Be("Effect");
    }

    [TestMethod]
    public void GetDataObjectTypeByElementType_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        string? result = stormStorage.GetDataObjectTypeByElementType("CEffectDamage".AsSpan());

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
    [DataRow("Effect", new[] { "CEffectSet" })]
    [DataRow("Actor", new[] { "CActorModel" })]
    [DataRow("Unit", new[] { "CUnit" })]
    [DataRow("Behavior", new[] { "CBehaviorBuff", "CBehaviorAbility" })]
    public void TryGetExistingElementTypesByDataObjectType_DataObjectTypes_ReturnsElementTypes(string dataObjectType, string[] elementTypes)
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
        bool result = stormStorage.TryGetExistingElementTypesByDataObjectType(dataObjectType.AsSpan(), out HashSet<string>? resultElementTypes);

        // assert
        result.Should().BeTrue();
        resultElementTypes!.ToArray().Should().BeEquivalentTo(elementTypes);
    }

    [TestMethod]
    public void TryGetExistingElementTypesByDataObjectType_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool result = stormStorage.TryGetExistingElementTypesByDataObjectType("Behavior".AsSpan(), out HashSet<string>? resultElementTypes);

        // assert
        result.Should().BeFalse();
        resultElementTypes.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingElementTypesByDataObjectType_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingElementTypesByDataObjectType(null!, out HashSet<string>? resultElementTypes);

        // assert
        result.Should().Throw<ArgumentNullException>();
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
        HashSet<string>? result = stormStorage.GetElementTypesByDataObjectType("Effect".AsSpan());

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
        HashSet<string>? result = stormStorage.GetElementTypesByDataObjectType("Effect".AsSpan());

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
        HashSet<string>? result = stormStorage.GetElementTypesByDataObjectType("Effect".AsSpan());

        // assert
        result.Should().Equal("CEffectDamage", "CEffectLaunchMissile");
    }

    [TestMethod]
    public void GetElementTypesByDataObjectType_HasNoDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        HashSet<string>? result = stormStorage.GetElementTypesByDataObjectType("CEffectDamage".AsSpan());

        // assert
        result.Should().BeNull();
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
        HashSet<string>? result = stormStorage.GetElementTypesByDataObjectType("Effect".AsSpan());
        result!.Add("CWhatever");

        // assert
        stormStorage.StormCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectDamage", "CEffectLaunchMissile");
        stormStorage.StormMapCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectLaunchMissile");
        stormStorage.StormCustomCache.ElementTypesByDataObjectType["Effect"].Should().Equal("CEffectSet");
    }

    [TestMethod]
    [DataRow("CAbil", "custom")]
    [DataRow("CEffect", "map")]
    [DataRow("CUnit", "normal")]
    public void TryGetExistingStormElementByElementType_ElementType_ReturnsStormElement(string elementType, string parent)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormCache.StormElementByElementType.Add("CAbil", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CAbil default=""1"">
  <Name value=""Abil/Name/##id##"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CAbil>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CEffect>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"" parent=""map"">
  <Chance value=""1"" />
</CEffect>
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CAbil", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CAbil default=""1"" parent=""custom"">
  <Name value=""Abil/Name/##id##"" />
</CAbil>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        bool result = stormStorage.TryGetExistingStormElementByElementType(elementType.AsSpan(), out StormElement? resultStormElement);

        // assert
        result.Should().BeTrue();
        resultStormElement!.ParentId.Should().Be(parent);
        resultStormElement.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void TryGetExistingStormElementByElementType_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetExistingStormElementByElementType("CEffect", out StormElement? resultStormElement);

        // assert
        result.Should().BeFalse();
        resultStormElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingStormElementByElementType_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingStormElementByElementType(null!, out StormElement? resultStormElement);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.GetXmlData("Element3").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInNormalAndMapCache_ReturnsMergedFromNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInNormalCache_ReturnsMergedFromNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementByElementType_ExistsInMapCache_ReturnsMergedFromMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementByElementType_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementByElementType_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        result!.AddValue(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
  <Element5 value=""value5"" />
  <Element6 value=""value6"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormElementByElementType["CUnit"].XmlDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementByElementType["CUnit"].XmlDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementByElementType["CUnit"].XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    [DataRow("Hero1", "Unit")]
    [DataRow("Hero2", "Unit")]
    [DataRow("Hero3", "Unit")]
    public void TryGetExistingStormElementById_Id_ReturnsStormElement(string id, string dataObjectType)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero2", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero2"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero3", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero3"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        bool result = stormStorage.TryGetExistingStormElementById(id.AsSpan(), dataObjectType.AsSpan(), out StormElement? resultStormElement);

        // assert
        result.Should().BeTrue();
        resultStormElement!.Id.Should().Be(id);
        resultStormElement.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void TryGetExistingStormElementById_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool result = stormStorage.TryGetExistingStormElementById("id".AsSpan(), "Unit", out StormElement? resultStormElement);

        // assert
        result.Should().BeFalse();
        resultStormElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingStormElementById_NullId_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingStormElementById(null!, "dataObjectType", out StormElement? resultStormElement);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void TryGetExistingStormElementById_NullDataObjectType_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingStormElementById("id", null!, out StormElement? resultStormElement);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormElementById_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.GetXmlData("Element3").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormElementById_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementById_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
  <Element5 value=""value5"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        result!.AddValue(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
  <Element5 value=""value5"" />
  <Element6 value=""value6"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    [DataRow("Hero1", "Unit")]
    [DataRow("Hero2", "Unit")]
    [DataRow("Hero3", "Unit")]
    public void TryGetExistingScaleValueStormElementById_Id_ReturnsStormElement(string id, string dataObjectType)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero2", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero2"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero3", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero3"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        bool result = stormStorage.TryGetExistingScaleValueStormElementById(id.AsSpan(), dataObjectType.AsSpan(), out StormElement? resultStormElement);

        // assert
        result.Should().BeTrue();
        resultStormElement!.Id.Should().Be(id);
        resultStormElement.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void TryGetExistingScaleValueStormElementById_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool result = stormStorage.TryGetExistingScaleValueStormElementById("id".AsSpan(), "Unit", out StormElement? resultStormElement);

        // assert
        result.Should().BeFalse();
        resultStormElement.Should().BeNull();
    }

    [TestMethod]
    public void TryGetExistingScaleValueStormElementById_NullId_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingScaleValueStormElementById(null!, "dataObjectType", out StormElement? resultStormElement);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void TryGetExistingScaleValueStormElementById_NullDataObjectType_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetExistingScaleValueStormElementById("id", null!, out StormElement? resultStormElement);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.GetXmlData("Element3").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.GetXmlData("Name").HasValue.Should().BeTrue();
        result.GetXmlData("Element1").HasValue.Should().BeTrue();
        result.GetXmlData("Element2").HasValue.Should().BeTrue();
        result.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetScaleValueStormElementById_HasNoElementType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetScaleValueStormElementById_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element1 value=""value1"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("normal")))
            },
        });

        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("map")))
            },
        });

        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "Hero1", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
  <Element5 value=""value5"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        result!.AddValue(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit id=""Hero1"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
  <Element5 value=""value5"" />
  <Element6 value=""value6"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(3);
        stormStorage.StormMapCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(4);
        stormStorage.StormCustomCache.ScaleValueStormElementsByDataObjectType["Unit"]["Hero1"].XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetCompleteStormElement_NoElements_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("ZagaraInfestApplyBuffBehavior".AsSpan(), "Effect".AsSpan());

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetCompleteStormElement_HasNoParentId_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffect", "Effect");
        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
</CEffect>
"),
            TestHelpers.GetStormPath("custom"))));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffectApplyBehavior", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffectApplyBehavior default=""1"">
  <Behavior value=""##id##"" />
  <ValidatorArray value=""##id##TargetFilters"" />
</CEffectApplyBehavior>
"),
            TestHelpers.GetStormPath("custom"))));

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
        {
            {
                "ZagaraInfestApplyBuffBehavior", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CEffectApplyBehavior id=""ZagaraInfestApplyBuffBehavior"">
  <ValidatorArray index=""0"" value=""IsRangedMinion"" />
</CEffectApplyBehavior>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("ZagaraInfestApplyBuffBehavior".AsSpan(), "Effect".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetCompleteStormElement_OnlyParentElement_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
        {
            {
                "BaseEffectModifyTokenCount", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CEffectModifyTokenCount default=""1"" id=""BaseEffectModifyTokenCount"">
  <Value value=""1"" />
</CEffectModifyTokenCount>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        stormStorage.StormCustomCache.StormElementsByDataObjectType["Effect"].Add(
            "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXElementValuePath(
                XElement.Parse(@"
<CEffectModifyTokenCount id=""KelThuzadMasterOfTheColdDarkModifyToken"" parent=""BaseEffectModifyTokenCount"">
  <ValidatorArray value=""TargetIsHero"" />
</CEffectModifyTokenCount>
"),
                TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken".AsSpan(), "Effect".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetCompleteStormElement_HasParentIdButNotFound_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", new Dictionary<string, StormElement>()
        {
            {
                "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CEffectModifyTokenCount id=""KelThuzadMasterOfTheColdDarkModifyToken"" parent=""BaseEffectModifyTokenCount"">
  <ValidatorArray value=""TargetIsHero"" />
</CEffectModifyTokenCount>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken".AsSpan(), "Effect".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Should().ContainSingle();
    }

    [TestMethod]
    public void GetCompleteStormElement_BaseElementSameAsTypeElement_ShouldNotHaveDuplicate()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CUnit", "Unit");

        StormElement baseStormElement = new(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom")));

        baseStormElement.AddValue(new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"">
  <TauntDoesntStopUnit index=""Cheer"" value=""1"" />
</CUnit>
"),
            TestHelpers.GetStormPath("custom"))));
        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", baseStormElement);

        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", new Dictionary<string, StormElement>()
        {
            {
                "StormBasicHeroicUnit", new StormElement(new StormXElementValuePath(
                    XElement.Parse(@"
<CUnit default=""1"" id=""StormBasicHeroicUnit"">
  <FlagArray index=""Unclickable"" value=""0"" />
</CUnit>
"),
                    TestHelpers.GetStormPath("custom")))
            },
        });

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("StormBasicHeroicUnit".AsSpan(), "Unit".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetBaseStormElement_FoundStormElementFromDataObjectType_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        stormStorage.StormCustomCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
</CEffect>
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetBaseStormElement_DidNotFindStormElementFromDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior".AsSpan());

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetBaseStormElement_DidNotFindFromDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? stormElement = stormStorage.GetBaseStormElement("CEffectApplyBehavior".AsSpan());

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other=""value"" />
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other2=""value2"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("normal"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInCustomCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.XmlDataCount.Should().Be(2);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4ff"" other=""value"" />
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

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
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other=""value"" />
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormStyleConstantElementsByName.Add("TooltipNumbers", new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other=""value"" other2=""value2"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleConstantElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        result!.AddValue(new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other2=""value2"" other3=""value3"" other4=""value4"" other5=""value5"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormStyleConstantElementsByName["TooltipNumbers"].XmlDataCount.Should().Be(2);
        stormStorage.StormMapCache.StormStyleConstantElementsByName["TooltipNumbers"].XmlDataCount.Should().Be(3);
        stormStorage.StormCustomCache.StormStyleConstantElementsByName["TooltipNumbers"].XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" height=""80"" />
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" styleflags=""Shadow"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.XmlDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInNormalCache_ReturnsMergedNormalCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
"),
            TestHelpers.GetStormPath("normal"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.XmlDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInMapCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" height=""80"" />
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInCustomCache_ReturnsMergedMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" styleflags=""Shadow"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_ExistsInNormalAndMapCache_ReturnsMergedNormalAndMapCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" height=""80"" />
"),
            TestHelpers.GetStormPath("map"))));

        // act
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.XmlDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
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
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" />
"),
            TestHelpers.GetStormPath("normal"))));

        stormStorage.StormMapCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" height=""80"" />
"),
            TestHelpers.GetStormPath("map"))));

        stormStorage.StormCustomCache.StormStyleStyleElementsByName.Add("ReticleEnemy", new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" styleflags=""Shadow"" height=""80"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // act
        StormStyleStyleElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        result!.AddValue(new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" styleflags=""Shadow"" shadowoffset=""3"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormStyleStyleElementsByName["ReticleEnemy"].XmlDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormStyleStyleElementsByName["ReticleEnemy"].XmlDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormStyleStyleElementsByName["ReticleEnemy"].XmlDataCount.Should().Be(5);
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
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1".AsSpan());

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
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1".AsSpan());

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
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1".AsSpan());

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
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1".AsSpan());

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
        StormGameString? stormGameString = stormStorage.GetStormGameString("id1".AsSpan());

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
}
