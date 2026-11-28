namespace Heroes.StormData.Cache.Elements.Tests;

[TestClass]
public sealed class AbilLinkCacheTests
{
    private readonly AbilLinkCache _abilLinkCache = new();

    [TestMethod]
    public void AddId_NewIds_AddsEntriesInOrderAndChangesVersion()
    {
        // arrange
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.AddId("attack");
        _abilLinkCache.AddId("AlarakDiscordStrike");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("attack", false), new AbilLinkEntry("AlarakDiscordStrike", false));
        _abilLinkCache.Version.Should().NotBe(version);
    }

    [TestMethod]
    public void AddId_SameIdAgain_DoesNotAddOrChangeVersion()
    {
        // arrange
        _abilLinkCache.AddId("attack");
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.AddId("attack");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("attack", false));
        _abilLinkCache.Version.Should().Be(version);
    }

    [TestMethod]
    public void AddId_IdsDifferingOnlyByCasing_AddsBoth()
    {
        // act - ids are case-sensitive
        _abilLinkCache.AddId("attack");
        _abilLinkCache.AddId("Attack");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("attack", false), new AbilLinkEntry("Attack", false));
    }

    [TestMethod]
    public void AddId_IdAsSpanSlice_AddsOnlyTheSlice()
    {
        // act - like the ability link of an ability command, AlarakDiscordStrike,Execute
        _abilLinkCache.AddId("AlarakDiscordStrike,Execute".AsSpan(0, 19));

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("AlarakDiscordStrike", false));
    }

    [TestMethod]
    public void AddDefaultElement_NewElements_AddsEntriesInOrderAndChangesVersion()
    {
        // arrange
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.AddDefaultElement("CAbil");
        _abilLinkCache.AddDefaultElement("CAbilEffectTarget");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("CAbil", true), new AbilLinkEntry("CAbilEffectTarget", true));
        _abilLinkCache.Version.Should().NotBe(version);
    }

    [TestMethod]
    public void AddDefaultElement_SameElementAgain_DoesNotAddOrChangeVersion()
    {
        // arrange
        _abilLinkCache.AddDefaultElement("CAbilEffectTarget");
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.AddDefaultElement("CAbilEffectTarget");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("CAbilEffectTarget", true));
        _abilLinkCache.Version.Should().Be(version);
    }

    [TestMethod]
    public void AddIdAndAddDefaultElement_SameValue_AddsBoth()
    {
        // act - the ids and the default element names are kept apart
        _abilLinkCache.AddDefaultElement("CAbilEffectTarget");
        _abilLinkCache.AddId("CAbilEffectTarget");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("CAbilEffectTarget", true), new AbilLinkEntry("CAbilEffectTarget", false));
    }

    [TestMethod]
    public void TryGetTemplate_TemplateSet_ReturnsTrueAndTemplate()
    {
        // arrange
        AbilLinkTemplate template = CreateTemplate();
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", template);

        // act
        bool result = _abilLinkCache.TryGetTemplate("Actor", "RangeAbil", out AbilLinkTemplate? foundTemplate);

        // assert
        result.Should().BeTrue();
        foundTemplate.Should().BeSameAs(template);
    }

    [TestMethod]
    [DataRow("Effect", "RangeAbil")]
    [DataRow("actor", "RangeAbil")]
    [DataRow("Actor", "rangeabil")]
    [DataRow("Actor", "OtherRange")]
    public void TryGetTemplate_NoTemplateForCatalogAndId_ReturnsFalseAndNull(string catalogName, string id)
    {
        // arrange - the catalog name and id are both case-sensitive, the collector always passes the catalog name in the game data's casing
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", CreateTemplate());

        // act
        bool result = _abilLinkCache.TryGetTemplate(catalogName, id, out AbilLinkTemplate? template);

        // assert
        result.Should().BeFalse();
        template.Should().BeNull();
    }

    [TestMethod]
    public void SetTemplate_SameIdInTwoCatalogs_KeepsBoth()
    {
        // arrange
        AbilLinkTemplate actorTemplate = CreateTemplate();
        AbilLinkTemplate effectTemplate = CreateTemplate();

        // act
        _abilLinkCache.SetTemplate("Actor", "AlarakDiscordStrike", actorTemplate);
        _abilLinkCache.SetTemplate("Effect", "AlarakDiscordStrike", effectTemplate);

        // assert
        _abilLinkCache.TryGetTemplate("Actor", "AlarakDiscordStrike", out AbilLinkTemplate? foundActorTemplate).Should().BeTrue();
        foundActorTemplate.Should().BeSameAs(actorTemplate);
        _abilLinkCache.TryGetTemplate("Effect", "AlarakDiscordStrike", out AbilLinkTemplate? foundEffectTemplate).Should().BeTrue();
        foundEffectTemplate.Should().BeSameAs(effectTemplate);
    }

    [TestMethod]
    public void SetTemplate_SameCatalogAndIdAgain_ReplacesTemplate()
    {
        // arrange
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", CreateTemplate());
        AbilLinkTemplate laterTemplate = CreateTemplate();

        // act
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", laterTemplate);

        // assert
        _abilLinkCache.TryGetTemplate("Actor", "RangeAbil", out AbilLinkTemplate? template).Should().BeTrue();
        template.Should().BeSameAs(laterTemplate);
    }

    [TestMethod]
    public void SetTemplate_TemplateSet_DoesNotChangeEntriesOrVersion()
    {
        // arrange - the templates are not entries
        _abilLinkCache.AddId("attack");
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", CreateTemplate());

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("attack", false));
        _abilLinkCache.Version.Should().Be(version);
    }

    [TestMethod]
    public void Clear_HasEntriesAndTemplates_RemovesThemAndChangesVersion()
    {
        // arrange
        _abilLinkCache.AddDefaultElement("CAbil");
        _abilLinkCache.AddId("attack");
        _abilLinkCache.SetTemplate("Actor", "RangeAbil", CreateTemplate());
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.Clear();

        // assert - so a new map's element can't have a parent from the previous map
        _abilLinkCache.Entries.Should().BeEmpty();
        _abilLinkCache.TryGetTemplate("Actor", "RangeAbil", out _).Should().BeFalse();
        _abilLinkCache.Version.Should().BeGreaterThan(version);
    }

    [TestMethod]
    public void Clear_NothingAdded_StillChangesVersion()
    {
        // arrange
        int version = _abilLinkCache.Version;

        // act
        _abilLinkCache.Clear();

        // assert - the version only goes up, so the sum of the versions across the data storages can't land on an earlier value
        _abilLinkCache.Version.Should().BeGreaterThan(version);
    }

    [TestMethod]
    public void Clear_ThenSameIdAndDefaultElementAgain_AddsThemAgain()
    {
        // arrange
        _abilLinkCache.AddDefaultElement("CAbil");
        _abilLinkCache.AddId("attack");
        _abilLinkCache.Clear();

        // act
        _abilLinkCache.AddId("attack");
        _abilLinkCache.AddDefaultElement("CAbil");

        // assert
        _abilLinkCache.Entries.Should().Equal(new AbilLinkEntry("attack", false), new AbilLinkEntry("CAbil", true));
    }

    private static AbilLinkTemplate CreateTemplate() =>
        new(null, [new KeyValuePair<string, string>("abil", string.Empty)], [new AbilLinkField("Abil", "Link", "##abil##")]);
}
