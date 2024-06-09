using Heroes.XmlData.StormMods;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormStorageTests
{
    [TestMethod]
    public void GetCompleteStormElement_NoElements_ReturnsNull()
    {
        // arrange
        StormStorage stormStorage = new();

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("Effect".AsSpan(), "ZagaraInfestApplyBuffBehavior".AsSpan());

        // assert
        stormElement.Should().BeNull();
    }

    [TestMethod]
    public void GetCompleteStormElement_HasNoParentId_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new();
        stormStorage.AddBaseElementTypes(StormModType.Custom, "Effect", "CEffect");
        stormStorage.AddBaseElementTypes(StormModType.Custom, "Effect", "CEffectApplyBehavior");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
</CEffect>
"),
            "custom");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CEffectApplyBehavior default=""1"">
  <Behavior value=""##id##"" />
  <ValidatorArray value=""##id##TargetFilters"" />
</CEffectApplyBehavior>
"),
            "custom");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CEffectApplyBehavior id=""ZagaraInfestApplyBuffBehavior"">
  <ValidatorArray index=""0"" value=""IsRangedMinion"" />
</CEffectApplyBehavior>
"),
            "custom");

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("Effect".AsSpan(), "ZagaraInfestApplyBuffBehavior".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Count.Should().Be(3);
    }

    [TestMethod]
    public void GetCompleteStormElement_OnlyParentElement_ReturnsStormElement()
    {
        // arrange
        StormStorage stormStorage = new();
        stormStorage.AddBaseElementTypes(StormModType.Custom, "Effect", "CEffectModifyTokenCount");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CEffectModifyTokenCount default=""1"" id=""BaseEffectModifyTokenCount"">
  <Value value=""1"" />
</CEffectModifyTokenCount>
"),
            "custom");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CEffectModifyTokenCount id=""KelThuzadMasterOfTheColdDarkModifyToken"" parent=""BaseEffectModifyTokenCount"">
  <ValidatorArray value=""TargetIsHero"" />
</CEffectModifyTokenCount>
"),
            "custom");

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("Effect".AsSpan(), "KelThuzadMasterOfTheColdDarkModifyToken".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Count.Should().Be(2);
    }

    [TestMethod]
    public void GetCompleteStormElement_BaseElementSameAsTypeElement_ShouldNotHaveDuplicate()
    {
        // arrange
        StormStorage stormStorage = new();
        stormStorage.AddBaseElementTypes(StormModType.Custom, "Unit", "CUnit");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CUnit default=""1"">
  <Name value=""Unit/Name/##id##"" />
</CUnit>
"),
            "custom");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CUnit default=""1"">
  <TauntDoesntStopUnit index=""Cheer"" value=""1"" />
</CUnit>
"),
            "custom");

        stormStorage.AddElement(
            StormModType.Custom,
            XElement.Parse(@"
<CUnit default=""1"" id=""StormBasicHeroicUnit"">
  <FlagArray index=""Unclickable"" value=""0"" />
</CUnit>
"),
            "custom");

        // act
        StormElement? stormElement = stormStorage.GetCompleteStormElement("Unit".AsSpan(), "StormBasicHeroicUnit".AsSpan());

        // assert
        stormElement.Should().NotBeNull();
        stormElement!.OriginalStormXElementValues.Count.Should().Be(3);
    }

    [TestMethod]
    [DataRow("$ZagaraHunterKillerDamage", "71")]
    [DataRow("$ChromieBasicAttackRange", "7")]
    [DataRow("$AzmodanAllShallBurnCastRange", "666")]
    public void TryGetExistingConstantXElementById_WithId_ReturnsResultPath(string id, string value)
    {
        // arrange
        string path = "custom";
        StormStorage stormStorage = new();
        stormStorage.StormCache.ConstantXElementById.Add("$ZagaraHunterKillerDamage", new StormXElementValuePath(XElement.Parse(@"<const id=""$ZagaraHunterKillerDamage"" value=""71"" />"), path));
        stormStorage.StormMapCache.ConstantXElementById.Add("$ChromieBasicAttackRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$ChromieBasicAttackRange"" value=""7"" />"), path));
        stormStorage.StormMapCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""6"" />"), path));
        stormStorage.StormCustomCache.ConstantXElementById.Add("$AzmodanAllShallBurnCastRange", new StormXElementValuePath(XElement.Parse(@"<const id=""$AzmodanAllShallBurnCastRange"" value=""666"" />"), path));

        // act
        bool result = stormStorage.TryGetExistingConstantXElementById(id.AsSpan(), out StormXElementValuePath? resultPath);

        // assert
        result.Should().BeTrue();
        resultPath!.Path.Should().Be(path);
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
        StormStorage stormStorage = new();
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
        StormStorage stormStorage = new();
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
        StormStorage stormStorage = new();

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

        StormStorage stormStorage = new();
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
        StormStorage stormStorage = new();
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
        StormStorage stormStorage = new();
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
            "normal")));

        stormStorage.StormCache.StormElementByElementType.Add("CAbil", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CAbil default=""1"">
  <Name value=""Abil/Name/##id##"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CAbil>
"),
            "normal")));

        stormStorage.StormCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"">
  <Chance value=""1"" />
  <Element2 value=""value2"" />
  <Element3 value=""value3"" />
</CEffect>
"),
            "normal")));

        stormStorage.StormMapCache.StormElementByElementType.Add("CEffect", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CEffect default=""1"" parent=""map"">
  <Chance value=""1"" />
</CEffect>
"),
            "map")));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CAbil", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CAbil default=""1"" parent=""custom"">
  <Name value=""Abil/Name/##id##"" />
</CAbil>
"),
            "custom")));

        // act
        bool result = stormStorage.TryGetExistingStormElementByElementType(elementType.AsSpan(), out StormElement? resultStormElement);

        // assert
        result.Should().BeTrue();
        resultStormElement!.ParentId.Should().Be(parent);
        resultStormElement.DataValues.KeyValueDataPairs.Count().Should().Be(3);
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
            "normal")));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            "map")));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
</CUnit>
"),
            "custom")));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element3"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(5);
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
            "normal")));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            "map")));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(4);
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
            "normal")));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(3);
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
            "map")));

        // act
        StormElement? result = stormStorage.GetStormElementByElementType("CUnit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(4);
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
            "normal")));

        stormStorage.StormMapCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element2 value=""value2"" />
</CUnit>
"),
            "map")));

        stormStorage.StormCustomCache.StormElementByElementType.Add("CUnit", new StormElement(new StormXElementValuePath(
            XElement.Parse(@"
<CUnit default=""1"" parent=""normal"">
  <Name value=""Unit/Name/##id##"" />
  <Element3 value=""value3"" />
  <Element4 value=""value4"" />
</CUnit>
"),
            "custom")));

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
            "custom")));

        // assert
        stormStorage.StormCache.StormElementByElementType["CUnit"].DataValues.KeyValueDataPairs.Count.Should().Be(3);
        stormStorage.StormMapCache.StormElementByElementType["CUnit"].DataValues.KeyValueDataPairs.Count.Should().Be(4);
        stormStorage.StormCustomCache.StormElementByElementType["CUnit"].DataValues.KeyValueDataPairs.Count.Should().Be(5);
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
                    "normal"))
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
                    "map"))
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
                    "custom"))
            },
        });

        // act
        bool result = stormStorage.TryGetExistingStormElementById(id.AsSpan(), dataObjectType.AsSpan(), out StormElement? resultStormElement);

        // assert
        result.Should().BeTrue();
        resultStormElement!.Id.Should().Be(id);
        resultStormElement.DataValues.KeyValueDataPairs.Count().Should().Be(2);
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
                    "normal"))
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
                    "map"))
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
                    "custom"))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element1"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element3"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(5);
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
                    "normal"))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element1"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(3);
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
                    "map"))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(3);
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
                    "normal"))
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
                    "map"))
            },
        });

        // act
        StormElement? result = stormStorage.GetStormElementById("Hero1".AsSpan(), "Unit".AsSpan());

        // assert
        result!.DataValues.KeyValueDataPairs["Name"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element1"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs["Element2"].HasValue.Should().BeTrue();
        result.DataValues.KeyValueDataPairs.Count.Should().Be(4);
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
                    "normal"))
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
                    "map"))
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
                    "custom"))
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
            "custom")));

        // assert
        stormStorage.StormCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.KeyValueDataPairs.Count.Should().Be(3);
        stormStorage.StormMapCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.KeyValueDataPairs.Count.Should().Be(4);
        stormStorage.StormCustomCache.StormElementsByDataObjectType["Unit"]["Hero1"].DataValues.KeyValueDataPairs.Count.Should().Be(5);
    }
}
