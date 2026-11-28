namespace Heroes.StormData.Cache.Elements.Tests;

[TestClass]
public sealed class ElementCatalogNamesTests
{
    [TestMethod]
    [DataRow("EffectDamage", "Effect")]
    [DataRow("AbilEffectTarget", "Abil")]
    [DataRow("BehaviorBuff", "Behavior")]
    [DataRow("ActorUnit", "Actor")]
    [DataRow("Unit", "Unit")]
    [DataRow("Hero", "Hero")]
    [DataRow("ValidatorUnitCompareField", "Validator")]
    [DataRow("EffectModifyCatalogNumeric", "Effect")]
    public void TryGetCatalogName_ClassStartsWithCatalogName_ReturnsCatalogName(string elementName, string expected)
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("Soundtrack", "Soundtrack")]
    [DataRow("SoundExclusivity", "SoundExclusivity")]
    [DataRow("SoundRandom", "Sound")]
    [DataRow("TerrainTex", "TerrainTex")]
    [DataRow("TerrainObject", "TerrainObject")]
    [DataRow("TerrainFoo", "Terrain")]
    [DataRow("AchievementTerm", "AchievementTerm")]
    [DataRow("AchievementTermCombine", "AchievementTerm")]
    [DataRow("Achievement", "Achievement")]
    [DataRow("GameUI", "GameUI")]
    [DataRow("GameQuestUI", "GameQuestUI")]
    [DataRow("Game", "Game")]
    [DataRow("HeroMastery", "HeroMastery")]
    [DataRow("HeroRole", "HeroRole")]
    [DataRow("HeroAbil", "Hero")]
    [DataRow("ItemContainer", "ItemContainer")]
    [DataRow("ItemClass", "ItemClass")]
    [DataRow("ItemAbil", "Item")]
    public void TryGetCatalogName_MoreThanOneCatalogNameMatches_ReturnsLongest(string elementName, string expected)
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("RequirementAnd")]
    [DataRow("RequirementEq")]
    [DataRow("RequirementCountBehavior")]
    [DataRow("RequirementGTE")]
    [DataRow("RequirementNode")]
    public void TryGetCatalogName_RequirementNodeClass_ReturnsRequirementNode(string elementName)
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("RequirementNode");
    }

    [TestMethod]
    public void TryGetCatalogName_Requirement_ReturnsRequirement()
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName("Requirement", out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Requirement");
    }

    [TestMethod]
    [DataRow("effectdamage", "Effect")]
    [DataRow("EFFECTDAMAGE", "Effect")]
    [DataRow("dspreverb", "DSP")]
    [DataRow("vodefinition", "VODefinition")]
    [DataRow("requirementeq", "RequirementNode")]
    public void TryGetCatalogName_ElementNameInOtherCasing_ReturnsGameDataCasing(string elementName, string expected)
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("NotACatalogClass")]
    [DataRow("Widget")]
    [DataRow("Eff")]
    [DataRow("")]
    public void TryGetCatalogName_NoCatalogNameMatches_ReturnsFalse(string elementName)
    {
        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeFalse();
        catalogName.Should().BeNull();
    }

    [TestMethod]
    public void TryGetCatalogName_ElementNameAsSpanSlice_ReturnsCatalogName()
    {
        // arrange - the element name without its leading C
        ReadOnlySpan<char> elementName = "CEffectDamage".AsSpan(1);

        // act
        bool result = ElementCatalogNames.TryGetCatalogName(elementName, out string? catalogName);

        // assert
        result.Should().BeTrue();
        catalogName.Should().Be("Effect");
    }

    [TestMethod]
    public void TryGetCatalogName_EveryCatalogName_IsDistinctIgnoringCaseAndIsItsOwnCatalog()
    {
        // assert - each catalog name is found as its own catalog, so no shorter name hides it
        StormXmlConstants.CatalogNames.Should().OnlyHaveUniqueItems(x => x.ToUpperInvariant());

        foreach (string name in StormXmlConstants.CatalogNames)
        {
            ElementCatalogNames.TryGetCatalogName(name, out string? catalogName).Should().BeTrue();
            catalogName.Should().Be(name);
        }
    }
}
