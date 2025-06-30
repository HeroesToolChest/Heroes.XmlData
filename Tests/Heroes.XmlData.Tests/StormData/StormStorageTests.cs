using Heroes.XmlData.Tests;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStorageTests
{
    [TestMethod]
    [DataRow("$ZagaraHunterKillerDamage", "71")]
    [DataRow("$ChromieBasicAttackRange", "7")]
    [DataRow("$AzmodanAllShallBurnCastRange", "666")]
    public void TryGetFirstExistingConstantXElementById_WithId_ReturnsResultPath(string id, string value)
    {
        // arrange
        string path = "custom";
        StormStorage stormStorage = new();
        stormStorage.StormCache.ConstantXElementById.Add("$ZagaraHunterKillerDamage", new StormXElementValuePath(XElement.Parse(@"<const id=""$ZagaraHunterKillerDamage"" value=""71"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantXElementById.Add("$ChromieBasicAttackRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$ChromieBasicAttackRange"" value=""7"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormMapCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""6"" />"), TestHelpers.GetStormPath(path)));
        stormStorage.StormCustomCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""666"" />"), TestHelpers.GetStormPath(path)));

        // act
        bool result = stormStorage.TryGetFirstConstantXElementById(id, out StormXElementValuePath? resultPath);
        bool resultSpan = stormStorage.TryGetFirstConstantXElementById(id.AsSpan(), out StormXElementValuePath? resultPathSpan);

        // assert
        result.Should().Be(resultSpan);
        resultPath.Should().BeEquivalentTo(resultPathSpan);
        result.Should().BeTrue();
        resultPath!.StormPath.Path.Should().Be(path);
        resultPath.Value.Attribute("value")!.Value.Should().Be(value);
    }

    [TestMethod]
    public void TryGetFirstExistingConstantXElementById_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetFirstConstantXElementById("$Id", out StormXElementValuePath? resultPath);

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
        Action result = () => stormStorage.TryGetFirstConstantXElementById(null!, out StormXElementValuePath? resultPath);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

#endif
    [TestMethod]
    [DataRow("CEffectDamage", "Effect")]
    [DataRow("CEffectLaunchMissile", "Effect")]
    [DataRow("CActorModel", "Actor")]
    [DataRow("CUnit", "Unit")]
    public void TryGetFirstDataObjectTypeByElementType_ElementTypes_ReturnsDataObjectType(string elementType, string dataObjectType)
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
    public void TryGetFirstDataObjectTypeByElementType_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetFirstDataObjectTypeByElementType("elementType", out string? resultDataObjectType);

        // assert
        result.Should().BeFalse();
        resultDataObjectType.Should().BeNull();
    }

#if !NET9_0_OR_GREATER
    [TestMethod]
    public void TryGetFirstDataObjectTypeByElementType_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetFirstDataObjectTypeByElementType(null!, out string? resultDataObjectType);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }
#endif

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

        stormStorage.StormCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            TestHelpers.GetStormPath("normal"))));

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

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            TestHelpers.GetStormPath("map"))));

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
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit");

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
        stormStorage.StormCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementByElementType["CUnit"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void GetStormElementById_ExistsInAllCaches_ReturnsMergedFromAllThree()
    {
        // arrange
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

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

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif
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

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1", "Unit");

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
        stormStorage.StormCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormMapCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(4);
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.ElementDataCount.Should().Be(5);
    }

    [TestMethod]
    public void StormElementExists_ExistsInAllCaches_ReturnsTrue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

        // act
        bool result = stormStorage.StormElementExists("Hero1", "Unit");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementExists_ExistsInNormalCache_ReturnsTrue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        // act
        bool result = stormStorage.StormElementExists("Hero1", "Unit");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementExists_ExistsInMapCache_ReturnsTrue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        // act
        bool result = stormStorage.StormElementExists("Hero1", "Unit");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementExists_ExistsInNormalAndMapCache_ReturnsTrue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        // act
        bool result = stormStorage.StormElementExists("Hero1", "Unit");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void StormElementExists_HasNoElementId_ReturnsFalse()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool result = stormStorage.StormElementExists("Hero1", "Unit");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null, "")]
    [DataRow("", null)]
    [DataRow(null, null)]
    public void StormElementExists_NullParams_ThrowsException(string id, string dataObjectType)
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.StormElementExists(id, dataObjectType);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [DataRow("HeroDVaMech", "Actor", "HeroDVaMech")]
    [DataRow("HeroChenEarth", "Actor", "ChenEarthUnit")]
    public void TryGetFirstStormElementIdByUnitName_UnitNamesAndDataObjectType_ReturnsId(string unitName, string dataObjectType, string id)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCache.UnitNamesByDataObjectType.Add(dataObjectType, new Dictionary<string, string>()
        {
            { unitName, id },
        });

        // act
        bool result = stormStorage.TryGetFirstStormElementIdByUnitName(unitName, dataObjectType, out string? resultId);

        // assert
        result.Should().BeTrue();
        resultId.Should().Be(id);
    }

    [TestMethod]
    [DataRow("HeroDVaMech", "Actor", "HeroDVaMech")]
    [DataRow("HeroChenEarth", "Actor", "ChenEarthUnit")]
    public void TryGetFirstStormElementIdByUnitName_MapCacheOnly_ReturnsId(string unitName, string dataObjectType, string id)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormMapCache.UnitNamesByDataObjectType.Add(dataObjectType, new Dictionary<string, string>()
        {
            { unitName, id },
        });

        // act
        bool result = stormStorage.TryGetFirstStormElementIdByUnitName(unitName, dataObjectType, out string? resultId);

        // assert
        result.Should().BeTrue();
        resultId.Should().Be(id);
    }

    [TestMethod]
    [DataRow("HeroDVaMech", "Actor", "HeroDVaMech")]
    [DataRow("HeroChenEarth", "Actor", "ChenEarthUnit")]
    public void TryGetFirstStormElementIdByUnitName_CustomCacheOnly_ReturnsId(string unitName, string dataObjectType, string id)
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.UnitNamesByDataObjectType.Add(dataObjectType, new Dictionary<string, string>()
        {
            { unitName, id },
        });

        // act
        bool result = stormStorage.TryGetFirstStormElementIdByUnitName(unitName, dataObjectType, out string? resultId);

        // assert
        result.Should().BeTrue();
        resultId.Should().Be(id);
    }

    [TestMethod]
    public void TryGetFirstStormElementIdByUnitName_NoneFound_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        bool result = stormStorage.TryGetFirstStormElementIdByUnitName("unitName", "dataObjectType", out string? resultId);

        // assert
        result.Should().BeFalse();
        resultId.Should().BeNull();
    }

    [TestMethod]
    public void TryGetFirstStormElementIdByUnitName_NullParam_ThrowsException()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        Action result = () => stormStorage.TryGetFirstStormElementIdByUnitName(null!, null!, out string? resultId);

        // assert
        result.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void GetStormElementIdByUnitName_HasDataObjectTypeAndId_ReturnsResult()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.UnitNamesByDataObjectType.Add("Actor", new Dictionary<string, string>()
        {
            { "unitName", "id" },
        });

        // act
        string? result = stormStorage.GetStormElementIdByUnitName("unitName", "Actor");

        // assert
        result.Should().Be("id");
    }

    [TestMethod]
    public void GetStormElementIdByUnitName_HasNoDataObjectType_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        string? result = stormStorage.GetStormElementIdByUnitName("unitName", "Actor");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementIdByUnitName_HasDataObjectTypeByNotUnitName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCustomCache.UnitNamesByDataObjectType.Add("Actor", []);

        // act
        string? result = stormStorage.GetStormElementIdByUnitName("unitName", "Actor");

        // assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetStormElementIdByUnitName_ModifiedReturnValue_ShouldNotModifiedInternalData()
    {
        // arrange
        string newValue = "modified";

        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.UnitNamesByDataObjectType.Add("Actor", new Dictionary<string, string>()
        {
            { "unitName", "id" },
        });

        // act
        string? result = stormStorage.GetStormElementIdByUnitName("unitName", "Actor");
        result = newValue;

        // assert
        stormStorage.StormCustomCache.UnitNamesByDataObjectType["Actor"]["unitName"].Should().NotBe(newValue);
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
        StormElement? result = stormStorage.GetScaleValueStormElementById("Hero1", "Unit");

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

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById);
#endif

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("ZagaraInfestApplyBuffBehavior", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement.ElementType.Should().Be("CEffectApplyBehavior");
        stormElement!.OriginalXElements.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetCompleteStormElement_OnlyParentElement_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById);
#endif

#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Effect"].Dictionary.Add(
            "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXElementValuePath(
                XElement.Parse(@"
                    <CEffectModifyTokenCount id=""KelThuzadMasterOfTheColdDarkModifyToken"" parent=""BaseEffectModifyTokenCount"">
                      <ValidatorArray value=""TargetIsHero"" />
                    </CEffectModifyTokenCount>
                    "),
                TestHelpers.GetStormPath("custom"))));
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Effect"].Add(
            "KelThuzadMasterOfTheColdDarkModifyToken", new StormElement(new StormXElementValuePath(
                XElement.Parse(@"
                    <CEffectModifyTokenCount id=""KelThuzadMasterOfTheColdDarkModifyToken"" parent=""BaseEffectModifyTokenCount"">
                      <ValidatorArray value=""TargetIsHero"" />
                    </CEffectModifyTokenCount>
                    "),
                TestHelpers.GetStormPath("custom"))));
#endif

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXElements.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetCompleteStormElement_HasParentIdButNotFound_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectModifyTokenCount", "Effect");

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Effect", newStormElementById);
#endif

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("KelThuzadMasterOfTheColdDarkModifyToken", "Effect");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXElements.Should().ContainSingle();
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

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("StormBasicHeroicUnit", "Unit");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXElements.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetCompleteStormElement_BaseElementHasAParentIdThatIsSameAsId_ShouldReturnAllProperties()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CLootChest", "LootChest");

        StormElement baseStormElement = new(new StormXElementValuePath(
            XElement.Parse(
                """
                <CLootChest default="1" id="LootChestChristmas2018Common" parent="LootChestChristmas2018Parent">
                  <Other value="10" />
                  <MaxRerolls value="2" />
                </CLootChest>
                """),
            TestHelpers.GetStormPath("custom")));

        baseStormElement.AddValue(new StormElement(new StormXElementValuePath(
            XElement.Parse(
                """
                <CLootChest id="LootChestChristmas2018Common" parent="LootChestChristmas2018Common">
                  <MaxRerolls value="3" />
                </CLootChest>
                """),
            TestHelpers.GetStormPath("custom"))));

        Dictionary<string, StormElement> newStormElementById = new()
        {
            {
                "BaseLootChest", new StormElement(new StormXElementValuePath(
                    XElement.Parse(
                        """
                        <CLootChest default="1" id="BaseLootChest">
                          <Name value="LootChest/Name/##id##" />
                          <HyperlinkId value="##id##" />
                          <MaxRerolls value="5" />
                          <TypeDescription value="LootChestCommon" />
                        </CLootChest>
                        """),
                    TestHelpers.GetStormPath("custom")))
            },
            {
                "LootChestChristmas2018Parent", new StormElement(new StormXElementValuePath(
                    XElement.Parse(
                        """
                        <CLootChest default="1" id="LootChestChristmas2018Parent" parent="BaseLootChest">
                          <TypeDescription value="LootChestChristmas2018Common" />
                          <EventName value="Toys18" />
                        </CLootChest>
                        """),
                    TestHelpers.GetStormPath("custom")))
            },
            {
                "LootChestChristmas2018Common", baseStormElement
            },
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("LootChest", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("LootChest", newStormElementById);
#endif

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("LootChestChristmas2018Common", "LootChest");

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalXElements.Should().HaveCount(4);
        stormElement.DataValues.GetElementDataAt("id").Value.GetString().Should().Be("LootChestChristmas2018Common");
        stormElement.DataValues.GetElementDataAt("parent").Value.GetString().Should().Be("LootChestChristmas2018Parent");
        stormElement.DataValues.GetElementDataAt("MaxRerolls").Value.GetInt().Should().Be(3);
        stormElement.DataValues.GetElementDataAt("EventName").Value.GetString().Should().Be("Toys18");
    }

    [TestMethod]
    public void GetBaseStormElement_FoundStormElementFromDataObjectType_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new(false);

        stormStorage.StormCustomCache.DataObjectTypeByElementType.Add("CEffectApplyBehavior", "Effect");

        stormStorage.StormCustomCache.StormElementByElementType.Add("Ceffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
</CEffect>
"),
            TestHelpers.GetStormPath("custom"))));

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
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormStyleConstantElement? stormStyleConstantElementAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(4);
        stormStyleConstantElementAsSpan.Should().NotBeNull();
        stormStyleConstantElementAsSpan!.DataValues.ElementDataCount.Should().Be(4);
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
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormStyleConstantElement? stormStyleConstantElementAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
        stormStyleConstantElementAsSpan.Should().NotBeNull();
        stormStyleConstantElementAsSpan!.DataValues.ElementDataCount.Should().Be(2);
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
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormStyleConstantElement? stormStyleConstantElementAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
        stormStyleConstantElementAsSpan.Should().NotBeNull();
        stormStyleConstantElementAsSpan!.DataValues.ElementDataCount.Should().Be(2);
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
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormStyleConstantElement? stormStyleConstantElementAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(2);
        stormStyleConstantElementAsSpan.Should().NotBeNull();
        stormStyleConstantElementAsSpan!.DataValues.ElementDataCount.Should().Be(2);
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
        StormStyleConstantElement? stormStyleConstantElement = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormStyleConstantElement? stormStyleConstantElementAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        stormStyleConstantElement.Should().NotBeNull();
        stormStyleConstantElement!.DataValues.ElementDataCount.Should().Be(3);
        stormStyleConstantElementAsSpan.Should().NotBeNull();
        stormStyleConstantElementAsSpan!.DataValues.ElementDataCount.Should().Be(3);
    }

    [TestMethod]
    public void GetStormStyleConstantElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");
        StormElement? resultAsSpan = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers".AsSpan());

        // assert
        result.Should().BeEquivalentTo(resultAsSpan);
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
        StormStyleConstantElement? result = stormStorage.GetStormStyleConstantElementsByName("TooltipNumbers");

        result!.AddValue(new StormStyleConstantElement(new StormXElementValuePath(
            XElement.Parse(@"
<Constant name=""TooltipNumbers"" val=""bfd4fd"" other2=""value2"" other3=""value3"" other4=""value4"" other5=""value5"" />
"),
            TestHelpers.GetStormPath("custom"))));

        // assert
        stormStorage.StormCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(2);
        stormStorage.StormMapCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(3);
        stormStorage.StormCustomCache.StormStyleConstantElementsByName["TooltipNumbers"].DataValues.ElementDataCount.Should().Be(4);
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
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormStyleStyleElement? stormStyleStyleElementAsSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(5);
        stormStyleStyleElementAsSpan.Should().NotBeNull();
        stormStyleStyleElementAsSpan!.DataValues.ElementDataCount.Should().Be(5);
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
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormStyleStyleElement? stormStyleStyleElementAsSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(3);
        stormStyleStyleElementAsSpan.Should().NotBeNull();
        stormStyleStyleElementAsSpan!.DataValues.ElementDataCount.Should().Be(3);
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
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormStyleStyleElement? stormStyleStyleElementAsSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
        stormStyleStyleElementAsSpan.Should().NotBeNull();
        stormStyleStyleElementAsSpan!.DataValues.ElementDataCount.Should().Be(4);
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
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormStyleStyleElement? stormStyleStyleElementAsSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
        stormStyleStyleElementAsSpan.Should().NotBeNull();
        stormStyleStyleElementAsSpan!.DataValues.ElementDataCount.Should().Be(4);
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
        StormStyleStyleElement? stormStyleStyleElement = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormStyleStyleElement? stormStyleStyleElementAsSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

        // assert
        stormStyleStyleElement.Should().NotBeNull();
        stormStyleStyleElement!.DataValues.ElementDataCount.Should().Be(4);
        stormStyleStyleElementAsSpan.Should().NotBeNull();
        stormStyleStyleElementAsSpan!.DataValues.ElementDataCount.Should().Be(4);
    }

    [TestMethod]
    public void GetStormStyleStyleElementsByName_HasNoName_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");
        StormElement? resultSpan = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy".AsSpan());

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
        StormStyleStyleElement? result = stormStorage.GetStormStyleStyleElementsByName("ReticleEnemy");

        result!.AddValue(new StormStyleStyleElement(new StormXElementValuePath(
            XElement.Parse(@"
<Style name=""ReticleEnemy"" template=""Storm_Tutorial_Reticle_Text"" textcolor=""255,255,255,255"" styleflags=""Shadow"" shadowoffset=""3"" />
"),
            TestHelpers.GetStormPath("custom"))));

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

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
        StormStorage stormStorage = new(false);

        Dictionary<string, StormElement> newStormElementById = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCache.StormElementsByDataObjectType.Add("Unit", newStormElementById);
#endif

        Dictionary<string, StormElement> newStormElementById2 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormMapCache.StormElementsByDataObjectType.Add("Unit", newStormElementById2);
#endif

        Dictionary<string, StormElement> newStormElementById3 = new()
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
        };
#if NET9_0_OR_GREATER
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3.GetAlternateLookup<ReadOnlySpan<char>>());
#else
        stormStorage.StormCustomCache.StormElementsByDataObjectType.Add("Unit", newStormElementById3);
#endif

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
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void StormLayoutFileExists_FilePathEmpty_ReturnsFalse(string emptyPath)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool exists = stormStorage.StormLayoutFileExists(emptyPath);

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
        stormStorage.StormCustomCache.UiStormPathsByRelativeUiPath.Add(Path.Join("ui", "layout", "ITEM1.stormlayout"), TestHelpers.GetStormPath("custom"));

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
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void GetStormLayoutFile_FilePathEmpty_ReturnsNull(string emptyPath)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormFile? stormAssetFile = stormStorage.GetStormLayoutFile(emptyPath);

        // assert
        stormAssetFile.Should().BeNull();
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
        stormStorage.StormCustomCache.AssetFilesByRelativeAssetsPath.Add(Path.Join("assets", "ITEM1.dds"), TestHelpers.GetStormPath("custom"));

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
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void StormAssetFileExists_FilePathEmpty_ReturnFalse(string emptyPath)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        bool exists = stormStorage.StormAssetFileExists(emptyPath);

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

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void GetStormAssetFile_FilePathEmpty_ReturnNull(string emptyPath)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        StormFile? stormAssetFile = stormStorage.GetStormAssetFile(emptyPath);

        // assert
        stormAssetFile.Should().BeNull();
    }

    [TestMethod]
    public void GetGameStringWithId_HasBothIdAndValue_ReturnsIdAndValue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, GameStringText GameStringText)? gamestring = stormStorage.GetGameStringWithId("id=value", TestHelpers.GetStormPath("normal"));

        // assert
        gamestring.HasValue.Should().BeTrue();
        gamestring!.Value.Id.Should().Be("id");
        gamestring.Value.GameStringText.Value.Should().Be("value");
    }

    [TestMethod]
    public void GetGameStringWithId_HasNoDelimeter_ReturnsIdAndNoValue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, GameStringText GameStringText)? gamestring = stormStorage.GetGameStringWithId("idvalue", TestHelpers.GetStormPath("normal"));

        // assert
        gamestring.HasValue.Should().BeTrue();
        gamestring!.Value.Id.Should().Be("idvalue");
        gamestring.Value.GameStringText.Value.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGameStringWithId_NoIdAndHasValue_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, GameStringText GameStringText)? gamestring = stormStorage.GetGameStringWithId("=value", TestHelpers.GetStormPath("normal"));
        (string Id, GameStringText GameStringText)? gamestringSpace = stormStorage.GetGameStringWithId(" =value", TestHelpers.GetStormPath("normal"));

        // assert
        gamestring.HasValue.Should().BeFalse();
        gamestringSpace.Should().BeEquivalentTo(gamestring);
    }

    [TestMethod]
    public void GetGameStringWithId_NoValueAndHasId_ReturnsIdAndValueEmpty()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, GameStringText GameStringText)? gamestring = stormStorage.GetGameStringWithId("id=", TestHelpers.GetStormPath("normal"));

        // assert
        gamestring.HasValue.Should().BeTrue();
        gamestring!.Value.Id.Should().Be("id");
        gamestring.Value.GameStringText.Value.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAssetWithId_HasBothIdAndValue_ReturnsIdAndValue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, AssetText AssetText)? assetText = stormStorage.GetAssetWithId("id=value", TestHelpers.GetStormPath("normal"));

        // assert
        assetText.HasValue.Should().BeTrue();
        assetText!.Value.Id.Should().Be("id");
        assetText.Value.AssetText.Value.Should().Be("value");
    }

    [TestMethod]
    public void GetAssetWithId_HasNoDelimeter_ReturnsIdAndNoValue()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, AssetText AssetText)? assetText = stormStorage.GetAssetWithId("idvalue", TestHelpers.GetStormPath("normal"));

        // assert
        assetText.HasValue.Should().BeTrue();
        assetText!.Value.Id.Should().Be("idvalue");
        assetText.Value.AssetText.Value.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAssetWithId_NoIdAndHasValue_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, AssetText AssetText)? assetText = stormStorage.GetAssetWithId("=value", TestHelpers.GetStormPath("normal"));
        (string Id, AssetText AssetText)? assetTextSpace = stormStorage.GetAssetWithId(" =value", TestHelpers.GetStormPath("normal"));

        // assert
        assetText.HasValue.Should().BeFalse();
        assetTextSpace.Should().BeEquivalentTo(assetText);
    }

    [TestMethod]
    public void GetAssetWithId_NoValueAndHasId_ReturnsIdAndValueEmpty()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        (string Id, AssetText AssetText)? assetText = stormStorage.GetAssetWithId("id=", TestHelpers.GetStormPath("normal"));

        // assert
        assetText.HasValue.Should().BeTrue();
        assetText!.Value.Id.Should().Be("id");
        assetText.Value.AssetText.Value.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("""<const id="$Var1" value="7" />""", "7")]
    [DataRow("""<const id="$Var1" value="" />""", "")]
    [DataRow("""<const id="$Var1" value=" " />""", " ")]
    [DataRow("""<const id="$Var1" />""", "")]
    public void GetValueFromConstElement_NoExpressions_ReturnsValueAsString(string element, string computedValue)
    {
        // arrange
        StormStorage stormStorage = new(false);

        XElement xElement = XElement.Parse(element);

        // act
        string result = stormStorage.GetValueFromConstElement(xElement);

        // assert
        result.Should().Be(computedValue);
    }

    [TestMethod]
    public void GetValueFromConstElement_HasExpressionAndIs1_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantXElementById.Add("$Var2", new StormXElementValuePath(XElement.Parse("""<const id="$Var2" value="7" />"""), TestHelpers.GetStormPath("path")));
        XElement xElement = XElement.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="1" />""");

        // act
        string result = stormStorage.GetValueFromConstElement(xElement);

        // assert
        result.Should().Be("9.125");
    }

    [TestMethod]
    public void AddStormLayoutFilePath_AddingSingleLayout_AddsOneLayout()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.AddStormLayoutFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // assert
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().ContainSingle();
    }

    [TestMethod]
    public void AddStormLayoutFilePath_AddingDuplicateSingleLayout_StillOnlyOneLayout()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.AddStormLayoutFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // act
        stormStorage.AddStormLayoutFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // assert
        stormStorage.StormCache.UiStormPathsByRelativeUiPath.Should().ContainSingle();
    }

    [TestMethod]
    public void AddAssetFilePath_AddingSingleAssetFilePath_AddsOneAssetFilePath()
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        stormStorage.AddAssetFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // assert
        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Should().ContainSingle();
    }

    [TestMethod]
    public void AddAssetFilePath_AddingDuplicateAssetFilePath_StillOnlyOneAssetFilePath()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.AddAssetFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // act
        stormStorage.AddAssetFilePath(StormModType.Normal, "this/is/a/path/file.txt", TestHelpers.GetStormPath("normal"));

        // assert
        stormStorage.StormCache.AssetFilesByRelativeAssetsPath.Should().ContainSingle();
    }

    [TestMethod]
    public void GetValueFromConstElement_HasExpressionAndIs0_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);

        XElement xElement = XElement.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="0" />""");

        // act
        string result = stormStorage.GetValueFromConstElement(xElement);

        // assert
        result.Should().Be("+($Var2 2.125)");
    }

    [TestMethod]
    [DataRow("""<const id="$Var1" value="7" />""", 7)]
    [DataRow("""<const id="$Var1" value="" />""", 0)]
    [DataRow("""<const id="$Var1" value=" " />""", 0)]
    [DataRow("""<const id="$Var1" />""", 0)]
    public void GetValueFromConstElementAsNumber_NoExpressions_ReturnsValue(string element, double computedValue)
    {
        // arrange
        StormStorage stormStorage = new(false);

        XElement xElement = XElement.Parse(element);

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(xElement);

        // assert
        result.Should().Be(computedValue);
    }

    [TestMethod]
    public void GetValueFromConstElementAsNumber_HasExpressionAndIs1_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantXElementById.Add("$Var2", new StormXElementValuePath(XElement.Parse("""<const id="$Var2" value="7" />"""), TestHelpers.GetStormPath("path")));

        XElement xElement = XElement.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="1" />""");

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(xElement);

        // assert
        result.Should().Be(9.125);
    }

    [TestMethod]
    public void GetValueFromConstElementAsNumber_HasExpressionAndIs0_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);

        XElement xElement = XElement.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="0" />""");

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(xElement);

        // assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void GetValueFromConstTextAsText_HasVar_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantXElementById.Add("$Var1", new StormXElementValuePath(XElement.Parse("""<const id="$Var1" value="7" />"""), TestHelpers.GetStormPath("path")));

        // act
        string result = stormStorage.GetValueFromConstTextAsText("$Var1");

        // assert
        result.Should().Be("7");
    }

    [TestMethod]
    [DataRow("", "")]
    [DataRow(" ", " ")]
    [DataRow("1", "1")]
    [DataRow("1.1", "1.1")]
    [DataRow(" 1.1 ", "1.1")]
    [DataRow("a", "a")]
    [DataRow(" a ", " a ")]
    public void GetValueFromConstTextAsText_NoVar_ReturnsValueAsString(string text, string returnValue)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        string result = stormStorage.GetValueFromConstTextAsText(text);

        // assert
        result.Should().Be(returnValue);
    }

    [TestMethod]
    public void GetValueFromConstTextAsNumber_HasVar_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantXElementById.Add("$Var1", new StormXElementValuePath(XElement.Parse("""<const id="$Var1" value="7" />"""), TestHelpers.GetStormPath("path")));

        // act
        double result = stormStorage.GetValueFromConstTextAsNumber("$Var1");

        // assert
        result.Should().Be(7);
    }

    [TestMethod]
    [DataRow("", 0)]
    [DataRow(" ", 0)]
    [DataRow("1", 1)]
    [DataRow("1.1", 1.1)]
    [DataRow(" 1.1 ", 1.1)]
    [DataRow("a", 0)]
    [DataRow(" a ", 0)]
    public void GetValueFromConstTextAsNumber_NoVar_ReturnsValueAsString(string text, double returnValue)
    {
        // arrange
        StormStorage stormStorage = new(false);

        // act
        double result = stormStorage.GetValueFromConstTextAsNumber(text);

        // assert
        result.Should().Be(returnValue);
    }
}
