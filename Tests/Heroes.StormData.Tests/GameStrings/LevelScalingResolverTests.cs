namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class LevelScalingResolverTests
{
    private const string Catalog = "Unit";
    private const string Entry = "HeroMuradin";

    private readonly LayeredDataStorage _layeredDataStorage = new();

    public LevelScalingResolverTests()
    {
        DataStorage dataStorage = _layeredDataStorage.GetDataStorage(StormModType.Base);

        dataStorage.AddBaseClassCatalogName(XmlReaderHelper.GetStormXmlElement("<CUnit />"), Catalog);
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CUnit id="HeroMuradin">
                  <LifeMax value="2765" />
                  <LifeRegenRate value="5.7617" />
                  <VitalMaxArray index="Energy" value="500" />
                  <Modification>
                    <DamageDealtScaled index="Spell" value="0.1" />
                  </Modification>
                </CUnit>
                """),
            Catalog);
    }

    [TestMethod]
    public void TryGetScaling_FieldIsSameText_ReturnsScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    public void TryGetScaling_EntryHasNoScaling_ReturnsFalseAndZero()
    {
        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeFalse();
        scaling.Should().Be(0);
    }

    [TestMethod]
    public void TryGetScaling_EntryHasScalingForOtherField_ReturnsFalseAndZero()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));

        // act
        bool result = TryGetScaling("LifeRegenRate", out double scaling);

        // assert
        result.Should().BeFalse();
        scaling.Should().Be(0);
    }

    [TestMethod]
    public void TryGetScaling_EachFieldOfEntry_ReturnsItsOwnScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"), ("LifeRegenRate", "0.045000"));

        // act
        bool lifeResult = TryGetScaling("LifeMax", out double lifeScaling);
        bool regenResult = TryGetScaling("LifeRegenRate", out double regenScaling);

        // assert
        lifeResult.Should().BeTrue();
        lifeScaling.Should().Be(0.04);
        regenResult.Should().BeTrue();
        regenScaling.Should().Be(0.045);
    }

    [TestMethod]
    [DataRow("LifeMax[0]")]
    [DataRow(" LifeMax ")]
    public void TryGetScaling_FieldIsOtherSpellingOfSameValue_ReturnsScaling(string field)
    {
        // arrange - LifeMax is a single value, so an index or surrounding spaces still resolve to it
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));

        // act
        bool result = TryGetScaling(field, out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    public void TryGetScaling_ScalingFieldIsOtherSpellingOfSameValue_ReturnsScaling()
    {
        // arrange - the scaling's Field has the index, the field asked for doesn't
        AddScaling(StormModType.Base, ("LifeMax[0]", "0.040000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    [DataRow("VitalMaxArray[2]")]
    [DataRow("VitalMaxArray[Energy]")]
    public void TryGetScaling_NamedIndexAndItsNumber_ReturnsScaling(string field)
    {
        // arrange - Energy is the third vital (Life, Shields, Energy), so [2] and [Energy] are the same value
        AddScaling(StormModType.Base, ("VitalMaxArray[Energy]", "0.050000"));

        // act
        bool result = TryGetScaling(field, out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.05);
    }

    [TestMethod]
    public void TryGetScaling_NestedField_ReturnsScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("Modification.DamageDealtScaled[Spell]", "0.025000"));

        // act
        bool result = TryGetScaling("Modification.DamageDealtScaled[Spell]", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.025);
    }

    [TestMethod]
    public void TryGetScaling_FieldNotOnElementButSameText_ReturnsScaling()
    {
        // arrange - the field doesn't resolve on the element, but the text is the same
        AddScaling(StormModType.Base, ("Speed", "0.010000"));

        // act
        bool result = TryGetScaling("Speed", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.01);
    }

    [TestMethod]
    public void TryGetScaling_FieldAndScalingFieldBothNotOnElement_ReturnsFalse()
    {
        // arrange - neither resolves, and two empty nodes are not the same node, so they don't match
        AddScaling(StormModType.Base, ("Speed", "0.010000"));

        // act
        bool result = TryGetScaling("Acceleration", out double scaling);

        // assert
        result.Should().BeFalse();
        scaling.Should().Be(0);
    }

    [TestMethod]
    public void TryGetScaling_FieldWithDifferentCasing_ReturnsFalse()
    {
        // arrange - the text match is ordinal, and the member names are case-sensitive
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));

        // act
        bool result = TryGetScaling("lifemax", out _);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void TryGetScaling_CatalogWithDifferentCasing_ReturnsScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        StormXmlElement mergedElement = GetMergedElement();

        // act
        bool result = LevelScalingResolver.TryGetScaling(
            _layeredDataStorage.Elements, mergedElement, "UNIT", Entry, "LifeMax", DataRefFieldParser.ParseField(mergedElement, "LifeMax"), out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    public void TryGetScaling_EntryWithDifferentCasing_ReturnsFalse()
    {
        // arrange - the entry is an id, which is case-sensitive
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        StormXmlElement mergedElement = GetMergedElement();

        // act
        bool result = LevelScalingResolver.TryGetScaling(
            _layeredDataStorage.Elements, mergedElement, Catalog, "heromuradin", "LifeMax", DataRefFieldParser.ParseField(mergedElement, "LifeMax"), out _);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void TryGetScaling_SameFieldInBaseAndMap_ReturnsMapScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        AddScaling(StormModType.Map, ("LifeMax", "0.050000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.05);
    }

    [TestMethod]
    public void TryGetScaling_OtherSpellingInHigherLayer_ReturnsHigherLayerScaling()
    {
        // arrange - two spellings of the same value, the later (map) one wins
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        AddScaling(StormModType.Map, ("LifeMax[0]", "0.050000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.05);
    }

    [TestMethod]
    public void TryGetScaling_OnlyLowerLayerHasField_ReturnsLowerLayerScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        AddScaling(StormModType.Custom, ("LifeRegenRate", "0.050000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("0.04x")]
    [DataRow("")]
    public void TryGetScaling_ValueIsNotNumber_ReturnsFalseAndZero(string value)
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", value));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeFalse();
        scaling.Should().Be(0);
    }

    [TestMethod]
    [DataRow("4E-2")]
    [DataRow("0.04")]
    [DataRow(".04")]
    public void TryGetScaling_ValueInOtherNumberFormat_ReturnsScaling(string value)
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", value));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(0.04);
    }

    [TestMethod]
    public void TryGetScaling_NegativeValue_ReturnsNegativeScaling()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "-0.020000"));

        // act
        bool result = TryGetScaling("LifeMax", out double scaling);

        // assert
        result.Should().BeTrue();
        scaling.Should().Be(-0.02);
    }

    [TestMethod]
    public void TryGetScaling_CurrentCultureUsesCommaDecimalSeparator_ParsesInvariantValue()
    {
        // arrange
        AddScaling(StormModType.Base, ("LifeMax", "0.040000"));
        CultureInfo originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            bool result = TryGetScaling("LifeMax", out double scaling);

            // assert
            result.Should().BeTrue();
            scaling.Should().Be(0.04);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private StormXmlElement GetMergedElement() => _layeredDataStorage.GetMergedStormXmlElementById(Entry, Catalog)!;

    // resolves the field on the merged element and looks up its scaling, as a data reference does
    private bool TryGetScaling(string field, out double scaling)
    {
        StormXmlElement mergedElement = GetMergedElement();

        return LevelScalingResolver.TryGetScaling(
            _layeredDataStorage.Elements, mergedElement, Catalog, Entry, field, DataRefFieldParser.ParseField(mergedElement, field), out scaling);
    }

    // adds a LevelScalingArray with a modification of Unit,HeroMuradin for each field, the same shape as in the game data
    private void AddScaling(StormModType stormModType, params (string Field, string Value)[] modifications)
    {
        string modificationsXml = string.Concat(modifications.Select(x =>
            $"""
            <Modifications>
              <Catalog value="{Catalog}" />
              <Entry value="{Entry}" />
              <Field value="{x.Field}" />
              <Value value="{x.Value}" />
            </Modifications>
            """));

        StormXmlElement hero = XmlReaderHelper.GetStormXmlElement($"""<CHero id="Muradin"><LevelScalingArray Ability="MuradinStormBolt">{modificationsXml}</LevelScalingArray></CHero>""");

        _layeredDataStorage.GetDataStorage(stormModType).AddLevelScalingElementValue(hero.Members.GetMember("LevelScalingArray"));
    }
}
