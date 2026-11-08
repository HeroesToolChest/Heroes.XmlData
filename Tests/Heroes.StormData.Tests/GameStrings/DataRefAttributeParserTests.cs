using Heroes.StormData.GameStrings;

namespace Heroes.StormData.Tests.GameStrings;

[TestClass]
public class DataRefAttributeParserTests
{
    [TestMethod]
    public void Parse_HasValueSmallEnoughForExponentNotation_ComputesValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();

        // act
        ValueScale result = parser.Parse("0.00001*100000");

        // assert
        result.Value.Should().Be(1);
    }

    [TestMethod]
    public void Parse_CurrentCultureUsesCommaDecimalSeparator_ComputesValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            ValueScale result = parser.Parse("1.5*2");

            // assert
            result.Value.Should().Be(3);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    [DataRow(0.04)]
    [DataRow(0.14)] // 0.14 + 1 is 1.1400000000000001
    [DataRow(0.57)] // 0.57 + 1 is 1.5699999999999998
    [DataRow(-0.04)] // multiplier is 0.96
    public void CalculateExpression_ValueHasScaling_ReturnsValueAndScaling(double scaling)
    {
        // arrange
        List<TextSectionItem> dataRefParts = [new TextSectionItem(new ValueScale(5, scaling))];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(string.Empty, dataRefParts);

        // assert
        result.Value.Should().Be(5);
        result.Scaling.Should().BeApproximately(scaling, 0.000001);
    }

    [TestMethod]
    public void CalculateExpression_OperatorBetweenValuesWithScaling_ReturnsCombinedScaling()
    {
        // arrange
        string dRef = "a+b";
        List<TextSectionItem> dataRefParts =
        [
            new TextSectionItem(new ValueScale(5, 0.14)),
            new TextSectionItem(new Range(1, 2)), // +
            new TextSectionItem(new ValueScale(3)),
        ];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(dRef, dataRefParts);

        // assert
        result.Value.Should().Be(8);
        result.Scaling.Should().BeApproximately(0.0875, 0.000001); // (5 * 1.14 + 3) / 8 - 1
    }

    [TestMethod]
    public void CalculateExpression_BaseValueIsZero_ReturnsNoScaling()
    {
        // arrange
        List<TextSectionItem> dataRefParts = [new TextSectionItem(new ValueScale(0, 0.04))];

        // act
        ValueScale result = DataRefAttributeParser.CalculateExpression(string.Empty, dataRefParts);

        // assert
        result.Value.Should().Be(0);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow("5", 5)]
    [DataRow(" 5 ", 5)]
    [DataRow("-5", -5)]
    [DataRow("0.5", 0.5)]
    [DataRow(".5", 0.5)]
    [DataRow("0", 0)]
    public void Parse_IsNumber_ReturnsNumber(string dRef, double expected)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expected);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow("2+3", 5)]
    [DataRow(" 2 + 3 ", 5)]
    [DataRow("10-4", 6)]
    [DataRow("(2+3)*4", 20)]
    [DataRow("17/34*100", 50)]
    [DataRow("-(2-5)", 3)]
    public void Parse_IsMathExpressionOfNumbers_ComputesValue(string dRef, double expected)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expected);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow("$BehaviorStackCount:KelThuzadMasterOfTheColdDarkToken$", 0)]
    [DataRow("$BehaviorStackCount:KelThuzadMasterOfTheColdDarkToken$+5", 5)]
    [DataRow(" $Var$ * 10", 0)]
    public void Parse_HasDollarVariable_VariableIsZero(string dRef, double expected)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser();

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("Effect,TestDamage,Amount")]
    [DataRow("Effect, TestDamage , Amount")]
    [DataRow(" Effect,TestDamage,Amount ")]
    public void Parse_IsCatalogReference_ReturnsFieldValue(string dRef)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(120);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow("Effect")]
    [DataRow("Effect,TestDamage")]
    [DataRow("Effect,,Amount")]
    public void Parse_CatalogReferenceHasLessThanThreeParts_ReturnsZero(string dRef)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(0);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow("Effect,UnknownDamage,Amount")] // entry not found
    [DataRow("Behavior,TestDamage,Amount")] // entry is in a different catalog
    [DataRow("Unknown,TestDamage,Amount")] // catalog not found
    [DataRow("Effect,testdamage,Amount")] // ids are case-sensitive
    [DataRow("Effect,TestDamage,Unknown")] // field not found
    [DataRow("Effect,TestDamage,Kind")] // field is not a number
    [DataRow("Effect,TestDamage,Amount.Unknown")] // nested field not found
    [DataRow("Effect,TestDamage,[0]")] // indexer with no field name
    public void Parse_CatalogReferenceDoesNotResolveToNumber_ReturnsZero(string dRef)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
              <Kind value="Spell" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(0);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    public void Parse_CatalogReferenceDoesNotResolveInExpression_IsTreatedAsZero()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount+Effect,UnknownDamage,Amount");

        // assert
        result.Value.Should().Be(120);
    }

    [TestMethod]
    [DataRow("Effect,TestPersistent,PeriodicPeriodArray[0]", 0.5)]
    [DataRow("Effect,TestPersistent,PeriodicPeriodArray[2]", 1.5)]
    [DataRow("Effect,TestPersistent,PeriodicPeriodArray", 0.5)] // no indexer uses the first entry
    [DataRow("Effect,TestPersistent,Cost[0].CooldownTimeUse", 6)]
    [DataRow("Effect,TestPersistent,Cost.CooldownTimeUse", 6)]
    [DataRow("Effect,TestPersistent,MultiplicativeModifierArray[Greed].Modifier", 0.15)]
    [DataRow("Effect,TestPersistent,MultiplicativeModifierArray[Headshot].Modifier", 0.75)]
    [DataRow("Effect,TestPersistent,VitalMaxArray[Shields]", 126)]
    [DataRow("Effect,TestPersistent,VitalMaxArray[1]", 126)] // ordinal of the Shields named index
    public void Parse_CatalogReferenceHasIndexedOrNestedField_ReturnsFieldValue(string dRef, double expected)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectCreatePersistent id="TestPersistent">
              <PeriodicPeriodArray value="0.5" />
              <PeriodicPeriodArray value="1" />
              <PeriodicPeriodArray value="1.5" />
              <Cost>
                <CooldownTimeUse value="6" />
              </Cost>
              <MultiplicativeModifierArray index="Greed" Modifier="0.15" />
              <MultiplicativeModifierArray index="Headshot" Modifier="0.75" />
              <VitalMaxArray index="Life" value="1000" />
              <VitalMaxArray index="Shields" value="126" />
            </CEffectCreatePersistent>
            """);

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_CatalogReferenceFieldIsOnParent_ReturnsParentFieldValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage default="1" id="TestDamageParent">
              <Amount value="80" />
            </CEffectDamage>
            """,
            """
            <CEffectDamage id="TestDamage" parent="TestDamageParent" />
            """);

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(80);
    }

    [TestMethod]
    public void Parse_CatalogReferenceFieldOverridesParent_ReturnsChildFieldValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage default="1" id="TestDamageParent">
              <Amount value="80" />
            </CEffectDamage>
            """,
            """
            <CEffectDamage id="TestDamage" parent="TestDamageParent">
              <Amount value="95" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(95);
    }

    [TestMethod]
    public void Parse_CatalogReferenceIsInHigherDataStorageLayer_ReturnsHigherLayerValue()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();

        AddEffect(
            layeredDataStorage.GetDataStorage(StormModType.Base),
            """
            <CEffectDamage id="TestDamage">
              <Amount value="100" />
            </CEffectDamage>
            """);
        AddEffect(
            layeredDataStorage.GetDataStorage(StormModType.Custom),
            """
            <CEffectDamage id="TestDamage">
              <Amount value="150" />
            </CEffectDamage>
            """);

        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(150);
    }

    [TestMethod]
    public void Parse_CatalogReferenceHasLevelScaling_ReturnsValueAndScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(120);
        result.Scaling.Should().BeApproximately(0.04, 0.000001);
    }

    [TestMethod]
    [DataRow("-Effect,TestDamage,Amount", -120)]
    [DataRow("Effect,TestDamage,Amount*100", 12000)]
    [DataRow("100*Effect,TestDamage,Amount", 12000)]
    [DataRow("Effect,TestDamage,Amount / 2", 60)]
    [DataRow("(Effect,TestDamage,Amount)", 120)]
    [DataRow("Effect, TestDamage, Amount", 120)]
    public void Parse_CatalogReferenceWithLevelScalingIsMultipliedOrNegated_KeepsScaling(string dRef, double expectedValue)
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expectedValue);
        result.Scaling.Should().BeApproximately(0.04, 0.000001);
    }

    [TestMethod]
    public void Parse_CatalogReferenceWithLevelScalingIsAddedToNumber_ScalingIsRelativeToTotal()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount+80");

        // assert
        result.Value.Should().Be(200);
        result.Scaling.Should().BeApproximately(0.024, 0.000001); // (120 * 1.04 + 80) / 200 - 1
    }

    [TestMethod]
    public void Parse_CatalogReferenceWithLevelScalingIsDividedByItself_HasNoScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount/Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(1);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    public void Parse_LevelScalingIsForDifferentField_HasNoScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Other", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(120);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    public void Parse_LevelScalingValueIsNotANumber_HasNoScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "abc");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamage,Amount");

        // assert
        result.Value.Should().Be(120);
        result.Scaling.Should().BeNull();
    }

    [TestMethod]
    public void Parse_LevelScalingFieldIsSpelledDifferentlyForSameNode_ReturnsScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        AddEffect(
            dataStorage,
            """
            <CEffectModifyUnit id="TestModifyUnit">
              <Cost>
                <CooldownTimeUse value="6" />
              </Cost>
            </CEffectModifyUnit>
            """);
        AddScaling(dataStorage, "TestModifyUnit", "Cost.CooldownTimeUse", "0.050000");

        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestModifyUnit,Cost[0].CooldownTimeUse");

        // assert
        result.Value.Should().Be(6);
        result.Scaling.Should().BeApproximately(0.05, 0.000001);
    }

    [TestMethod]
    public void Parse_TwoCatalogReferencesWithDifferentScaling_ReturnsCombinedScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        AddEffect(
            dataStorage,
            """
            <CEffectDamage id="TestDamageA">
              <Amount value="100" />
            </CEffectDamage>
            """);
        AddEffect(
            dataStorage,
            """
            <CEffectDamage id="TestDamageB">
              <Amount value="50" />
            </CEffectDamage>
            """);
        AddScaling(dataStorage, "TestDamageA", "Amount", "0.040000");
        AddScaling(dataStorage, "TestDamageB", "Amount", "0.100000");

        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("Effect,TestDamageA,Amount+Effect,TestDamageB,Amount");

        // assert
        result.Value.Should().Be(150);
        result.Scaling.Should().BeApproximately(0.06, 0.000001); // (100 * 1.04 + 50 * 1.1) / 150 - 1
    }

    [TestMethod]
    [DataRow("[d ref='Effect,TestDamage,Amount'/]", 120)]
    [DataRow("[D ref='Effect,TestDamage,Amount'/]", 120)]
    [DataRow("[d ref='Effect,TestDamage,Amount*2'/]", 240)]
    [DataRow("[d ref='Effect,TestDamage,Amount' player='0'/]", 120)]
    [DataRow("10*[d ref='Effect,TestDamage,Amount'/]", 1200)]
    [DataRow("[d ref='Effect,TestDamage,Amount'/]+[d ref='Effect,TestDamage,Amount'/]", 240)]
    [DataRow("[d ref='Effect,TestDamage,Amount'/]/Effect,TestDamage,Amount", 1)]
    public void Parse_HasBracketDataRef_ComputesValue(string dRef, double expected)
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse(dRef);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_BracketDataRefHasPrecision_ValueIsRounded()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="1.23456" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse("[d ref='Effect,TestDamage,Amount' precision='2'/]");

        // assert
        result.Value.Should().Be(1.23);
    }

    [TestMethod]
    public void Parse_BracketDataRefHasNoPrecision_ValueIsRoundedToWholeNumber()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="1.23456" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse("[d ref='Effect,TestDamage,Amount'/]");

        // assert
        result.Value.Should().Be(1);
    }

    [TestMethod]
    public void Parse_BracketDataRefHasLevelScaling_ReturnsScaling()
    {
        // arrange
        LayeredDataStorage layeredDataStorage = CreateTestDamageWithScalingStorage("Amount", "0.040000");
        DataRefAttributeParser parser = new(layeredDataStorage, new DataTagParser(layeredDataStorage));

        // act
        ValueScale result = parser.Parse("[d ref='Effect,TestDamage,Amount'/]");

        // assert
        result.Value.Should().Be(120);
        result.Scaling.Should().BeApproximately(0.04, 0.000001);
    }

    [TestMethod]
    public void Parse_BracketDataRefHasSpaceBeforeIt_ComputesValue()
    {
        // arrange
        DataRefAttributeParser parser = CreateParser(
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """,
            """
            <CEffectDamage id="TestCount">
              <Count value="4" />
            </CEffectDamage>
            """);

        // act
        ValueScale result = parser.Parse("(Effect,TestDamage,Amount* [d ref='Effect,TestCount,Count' player='0'/])/Effect,TestDamage,Amount*100");

        // assert
        result.Value.Should().Be(400);
    }

    private static DataRefAttributeParser CreateParser(params string[] effectXmls)
    {
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        foreach (string effectXml in effectXmls)
            AddEffect(dataStorage, effectXml);

        return new DataRefAttributeParser(layeredDataStorage, new DataTagParser(layeredDataStorage));
    }

    // Effect,TestDamage,Amount of 120 with a level scaling on the given field
    private static LayeredDataStorage CreateTestDamageWithScalingStorage(string scalingField, string scalingValue)
    {
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        AddEffect(
            dataStorage,
            """
            <CEffectDamage id="TestDamage">
              <Amount value="120" />
            </CEffectDamage>
            """);
        AddScaling(dataStorage, "TestDamage", scalingField, scalingValue);

        return layeredDataStorage;
    }

    private static void AddEffect(DataStorage dataStorage, string xml)
    {
        StormXmlElement element = XmlReaderHelper.GetStormXmlElement(xml);

        dataStorage.AddBaseClassCatalogName(XmlReaderHelper.GetStormXmlElement($"<{element.ElementName} />"), "Effect");
        dataStorage.AddElementWithId(element, "Effect");
    }

    private static void AddScaling(DataStorage dataStorage, string entry, string field, string value)
    {
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            $"""
            <CHero id="TestHero">
              <LevelScalingArray Ability="TestAbil">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="{entry}" />
                  <Field value="{field}" />
                  <Value value="{value}" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));
    }
}
