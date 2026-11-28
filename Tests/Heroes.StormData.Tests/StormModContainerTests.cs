using System.Collections.ObjectModel;
using System.Text;

namespace Heroes.StormData.Tests;

[TestClass]
public sealed class StormModContainerTests
{
    private readonly SourceContainer _sourceContainer = new();

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void Constructor_StormModType_SetsStormModTypeAndEmptyFileLists(StormModType stormModType)
    {
        // act
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);

        // assert
        stormModContainer.StormModType.Should().Be(stormModType);
        stormModContainer.LoadedXmlFiles.Should().BeEmpty();
        stormModContainer.LoadedAssetsTextFiles.Should().BeEmpty();
        stormModContainer.LoadedAssetFiles.Should().BeEmpty();
        stormModContainer.LoadedStormLayoutFiles.Should().BeEmpty();
        stormModContainer.LoadedGameStringFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void Constructor_FileLists_AreReadOnlyCollections()
    {
        // act
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        // assert
        stormModContainer.LoadedXmlFiles.Should().BeOfType<ReadOnlyCollection<StormFile>>();
        stormModContainer.LoadedAssetsTextFiles.Should().BeOfType<ReadOnlyCollection<StormFile>>();
        stormModContainer.LoadedAssetFiles.Should().BeOfType<ReadOnlyCollection<StormFile>>();
        stormModContainer.LoadedStormLayoutFiles.Should().BeOfType<ReadOnlyCollection<StormFile>>();
        stormModContainer.LoadedGameStringFiles.Should().BeOfType<ReadOnlyCollection<StormFile>>();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadXmlFile_HasConstElements_AddsConstElementsToItsDataStorageOnly(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        StormFile stormFile = CreateStormFile("EffectData.xml");

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <const id="$Const1" value="1" />
            <const id="$Const2" value="2" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, stormFile);

        // assert
        GetElementCache(stormModType).StormXmlConstElementById.Dictionary.Keys.Should().BeEquivalentTo("$Const1", "$Const2");
        stormModContainer.LoadedXmlFiles.Should().Equal(stormFile);

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetElementCache(otherStormModType).StormXmlConstElementById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementWithNoId_AddsElementByClassElementName()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Map);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CUnit><LifeMax value="100" /></CUnit>""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("UnitData.xml"));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Map);
        elementCache.StormXmlElementByClassElement.Dictionary.Should().ContainKey("Unit")
            .WhoseValue.Members["LifeMax"].Value.RawValue.Should().Be("100");
        elementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementNameStartsWithCatalogFromFileName_AddsBaseClassCatalogName()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectDamage default="1" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("EffectData.xml"));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.ClassElementsByCatalog.Should().ContainKey("Effect").WhoseValue.Should().BeEquivalentTo("EffectDamage");
        elementCache.CatalogByClassElement.Dictionary.Should().ContainKey("EffectDamage").WhoseValue.Should().Be("Effect");
    }

    [TestMethod]
    [DataRow("UnitData.xml")]
    [DataRow("Data.xml")]
    [DataRow("Effects.xml")]
    [DataRow("MuradinData.xml")]
    public void LoadXmlFile_ClassElementInFileOfOtherCatalog_AddsCatalogNameFromClassName(string fileName)
    {
        // arrange - the catalog comes from the class name, not the file name
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectDamage default="1" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile(fileName));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.ClassElementsByCatalog.Should().ContainKey("Effect").WhoseValue.Should().BeEquivalentTo("EffectDamage");
        elementCache.CatalogByClassElement.Dictionary.Should().ContainKey("EffectDamage").WhoseValue.Should().Be("Effect");
        elementCache.StormXmlElementByClassElement.Dictionary.Should().ContainKey("EffectDamage");
    }

    [TestMethod]
    [DataRow("effectdata.xml")]
    [DataRow("EffectData.xml")]
    [DataRow("EFFECTDATA.XML")]
    public void LoadXmlFile_FileNameInAnyCasing_CatalogNameHasGameDataCasing(string fileName)
    {
        // arrange - the files are lowercase in CASC and extracted data, the catalog name should still be Effect
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectDamage id="AbathurSymbioteStabDamage" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile(fileName));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.CatalogByClassElement.Dictionary.Should().ContainKey("EffectDamage").WhoseValue.Should().BeSameAs("Effect");
        elementCache.StormXmlElementsByIdByCatalog.Dictionary.Keys.Should().Equal("Effect");
    }

    [TestMethod]
    public void LoadXmlFile_ClassOnlyInHeroFile_AddsElementUnderCatalogFromClassName()
    {
        // arrange - like CEffectModifyCatalogNumeric, which is only in the hero files (e.g. muradindata.xml), never in effectdata.xml
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectModifyCatalogNumeric id="MuradinModifyCatalog" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("MuradinData.xml"));

        // assert
        GetElementCache(StormModType.Base).StormXmlElementsByIdByCatalog.Dictionary.Should().ContainKey("Effect")
            .WhoseValue.Dictionary.Keys.Should().Equal("MuradinModifyCatalog");
    }

    [TestMethod]
    [DataRow("CRequirementEq", "RequirementNode")]
    [DataRow("CRequirementCountBehavior", "RequirementNode")]
    [DataRow("CRequirementNode", "RequirementNode")]
    [DataRow("CRequirement", "Requirement")]
    public void LoadXmlFile_RequirementClasses_AddsElementUnderRequirementOrRequirementNodeCatalog(string elementName, string expectedCatalogName)
    {
        // arrange - the requirement nodes are the only classes that don't start with their catalog's name
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader($"""<{elementName} id="TestRequirement" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("RequirementNodeData.xml"));

        // assert
        GetElementCache(StormModType.Base).StormXmlElementsByIdByCatalog.Dictionary.Keys.Should().Equal(expectedCatalogName);
    }

    [TestMethod]
    public void LoadXmlFile_ClassNotOfKnownCatalogButStartsWithFileCatalog_AddsCatalogNameFromFileWithElementNameCasing()
    {
        // arrange - a class of a catalog the game data doesn't have, which only the file name gives
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CWidgetButton id="TestWidget" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("widgetdata.xml"));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.CatalogByClassElement.Dictionary.Should().ContainKey("WidgetButton").WhoseValue.Should().Be("Widget");
        elementCache.StormXmlElementsByIdByCatalog.Dictionary.Keys.Should().Equal("Widget");
    }

    [TestMethod]
    public void LoadXmlFile_ClassNotOfKnownCatalogButInLowerLayer_AddsElementUnderLowerLayerCatalog()
    {
        // arrange - the base layer has the class from its file name, the map's file name doesn't
        StormModContainer baseStormModContainer = new(_sourceContainer, StormModType.Base);
        StormModContainer mapStormModContainer = new(_sourceContainer, StormModType.Map);

        using XmlReader baseXmlReader = XmlReaderHelper.CreateXmlReader("""<CWidgetButton default="1" />""");
        baseStormModContainer.LoadXmlFile(baseXmlReader, CreateStormFile("WidgetData.xml"));

        using XmlReader mapXmlReader = XmlReaderHelper.CreateXmlReader("""<CWidgetButton id="MapWidget" />""");

        // act
        mapStormModContainer.LoadXmlFile(mapXmlReader, CreateStormFile("MapData.xml"));

        // assert
        GetElementCache(StormModType.Map).StormXmlElementsByIdByCatalog.Dictionary.Keys.Should().Equal("Widget");
    }

    [TestMethod]
    public void LoadXmlFile_SameClassMoreThanOnce_AddsClassToCatalogOnce()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage default="1" />
            <CEffectDamage id="Damage1" />
            <CEffectDamage id="Damage2" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("EffectData.xml"));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.ClassElementsByCatalog.Should().ContainSingle().Which.Value.Should().BeEquivalentTo("EffectDamage");
        elementCache.StormXmlElementsByIdByCatalog.Dictionary["Effect"].Dictionary.Keys.Should().BeEquivalentTo("Damage1", "Damage2");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadXmlFile_HasAbilLinks_AddsAbilLinksToItsDataStorageOnly(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CAbilEffectTarget default="1" />
            <CUnit id="HeroAlarak">
              <AbilArray Link="AlarakSadism" />
            </CUnit>
            <CAbilEffectTarget id="AlarakDiscordStrike" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("AlarakData.xml"));

        // assert
        GetDataStorage(stormModType).AbilLinkCache.Entries.Should().Equal(
            new AbilLinkEntry("CAbilEffectTarget", true),
            new AbilLinkEntry("AlarakSadism", false),
            new AbilLinkEntry("AlarakDiscordStrike", false));

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetDataStorage(otherStormModType).AbilLinkCache.Entries.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_MapElementHasBaseParentWithToken_AddsAbilLinkOfMapElementAfterBaseAbilLinks()
    {
        // arrange
        StormModContainer baseStormModContainer = new(_sourceContainer, StormModType.Base);
        StormModContainer mapStormModContainer = new(_sourceContainer, StormModType.Map);

        using XmlReader baseXmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorRange id="RangeAbil">
              <?token id="abil" type="CAbilLink"?>
              <Abil Link="##abil##" />
            </CActorRange>
            <CAbilAttack id="attack" />
            """);

        using XmlReader mapXmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorRange id="MapRange" parent="RangeAbil" abil="MapAbility" />
            <CUnit id="MapUnit">
              <AbilArray Link="attack" />
            </CUnit>
            """);

        // act
        mapStormModContainer.LoadXmlFile(mapXmlReader, CreateStormFile("ActorData.xml"));
        baseStormModContainer.LoadXmlFile(baseXmlReader, CreateStormFile("ActorData.xml"));

        using XmlReader secondMapXmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorRange id="MapRange" parent="RangeAbil" abil="MapAbility" />
            """);

        mapStormModContainer.LoadXmlFile(secondMapXmlReader, CreateStormFile("ActorData.xml"));

        // assert
        // the map element had no parent to get the ability link from until the base data was loaded
        _sourceContainer.LayeredDataStorage.AbilLinks.GetAbilLinkIds().Should().Equal("attack", "MapAbility");
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementWithIdAndKnownBaseClass_AddsElementUnderCatalog()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CEffectDamage default="1" />
            <CEffectDamage id="AbathurSymbioteStabDamage">
              <Amount value="80" />
            </CEffectDamage>
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("EffectData.xml"));

        // assert
        GetElementCache(StormModType.Base).StormXmlElementsByIdByCatalog.Dictionary.Should().ContainKey("Effect")
            .WhoseValue.Dictionary.Should().ContainKey("AbathurSymbioteStabDamage")
            .WhoseValue.Members["Amount"].Value.RawValue.Should().Be("80");
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementWithIdAndBaseClassInLowerLayer_AddsElementUnderCatalogInItsDataStorage()
    {
        // arrange
        StormModContainer baseStormModContainer = new(_sourceContainer, StormModType.Base);
        StormModContainer mapStormModContainer = new(_sourceContainer, StormModType.Map);

        using XmlReader baseXmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectDamage default="1" />""");
        baseStormModContainer.LoadXmlFile(baseXmlReader, CreateStormFile("EffectData.xml"));

        // the map file name has no catalog, so the catalog can only come from the base layer
        using XmlReader mapXmlReader = XmlReaderHelper.CreateXmlReader("""<CEffectDamage id="MapDamage" />""");

        // act
        mapStormModContainer.LoadXmlFile(mapXmlReader, CreateStormFile("MapData.xml"));

        // assert
        GetElementCache(StormModType.Map).StormXmlElementsByIdByCatalog.Dictionary.Should().ContainKey("Effect")
            .WhoseValue.Dictionary.Keys.Should().Equal("MapDamage");
        GetElementCache(StormModType.Base).StormXmlElementsByIdByCatalog.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementWithIdAndUnknownBaseClass_AddsElementUnderEmptyCatalog()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        // a class that starts with no catalog name, in a file whose name has no catalog
        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<CNotACatalogClass id="Abathur" />""");

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("Units.xml"));

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().ContainKey(string.Empty)
            .WhoseValue.Dictionary.Keys.Should().Equal("Abathur");
        elementCache.CatalogByClassElement.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_ActorWithUnitName_AddsIdByUnitNameUnderCatalog()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorUnit id="HeroMuradinActor" parent="StormHero" unitName="HeroMuradin" />
            <CActorMissile id="MuradinStormBoltMissile" unitName="MuradinStormBoltMissileUnit" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("actordata.xml"));

        // assert
        Dictionary<string, List<string>> idsByUnitName = GetElementCache(StormModType.Base).IdsByUnitNameByCatalog.Dictionary.Should().ContainKey("Actor").WhoseValue.Dictionary;
        idsByUnitName.Should().ContainKey("HeroMuradin").WhoseValue.Should().Equal("HeroMuradinActor");
        idsByUnitName.Should().ContainKey("MuradinStormBoltMissileUnit").WhoseValue.Should().Equal("MuradinStormBoltMissile");
    }

    [TestMethod]
    [DataRow("""<CActorUnit id="HeroMuradinActor" />""")]
    [DataRow("""<CActorUnit id="HeroMuradinActor" unitName="" />""")]
    [DataRow("""<CActorUnit default="1" unitName="HeroMuradin" />""")]
    public void LoadXmlFile_NoUnitNameOrNoId_DoesNotAddIdByUnitName(string xml)
    {
        // arrange - a default element (no id) has no id to look up
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(xml);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("actordata.xml"));

        // assert
        GetElementCache(StormModType.Base).IdsByUnitNameByCatalog.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadXmlFile_LaterDefinitionWithoutUnitName_KeepsUnitName()
    {
        // arrange - an override that leaves out the unitName
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorUnit id="HeroMuradinActor" unitName="HeroMuradin" />
            <CActorUnit id="HeroMuradinActor"><Scale value="1.1" /></CActorUnit>
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("actordata.xml"));

        // assert
        GetElementCache(StormModType.Base).IdsByUnitNameByCatalog.Dictionary["Actor"].Dictionary.Should().ContainKey("HeroMuradin")
            .WhoseValue.Should().Equal("HeroMuradinActor");
    }

    [TestMethod]
    public void LoadXmlFile_TwoActorsWithSameUnitName_AddsBothInLoadOrder()
    {
        // arrange - like DeadHeroResurrectSpirit in the game data
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CActorUnit id="PVEResurrectSpirit" unitName="DeadHeroResurrectSpirit" />
            <CActorUnit id="DeadHeroResurrectSpirit" unitName="DeadHeroResurrectSpirit" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("actordata.xml"));

        // assert
        GetElementCache(StormModType.Base).IdsByUnitNameByCatalog.Dictionary["Actor"].Dictionary["DeadHeroResurrectSpirit"].Should()
            .Equal("PVEResurrectSpirit", "DeadHeroResurrectSpirit");
    }

    [TestMethod]
    public void LoadXmlFile_ClassElementWithIdHasLevelScalingArray_AddsScalingFieldValues()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <CHero id="Abathur">
              <LevelScalingArray Ability="AbathurSymbiote">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="AbathurSymbioteStabDamage" />
                  <Field value="Amount" />
                  <Value value="0.04" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("HeroData.xml"));

        // assert
        GetElementCache(StormModType.Base).ScalingFieldValueByIdByCatalog.Dictionary.Should().ContainKey("Effect")
            .WhoseValue.Dictionary.Should().ContainKey("AbathurSymbioteStabDamage")
            .WhoseValue.Should().Equal(new LevelScalingFieldValue("Amount", "0.04"));
    }

    [TestMethod]
    public void LoadXmlFile_NonClassElements_AreNotAddedButFileIsListed()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile = CreateStormFile("EffectData.xml");

        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader(
            """
            <SEffectStruct id="Struct1" />
            <EEffectEnum id="Enum1" />
            <Effect id="Unknown1" />
            """);

        // act
        stormModContainer.LoadXmlFile(xmlReader, stormFile);

        // assert
        ElementCache elementCache = GetElementCache(StormModType.Base);
        elementCache.StormXmlElementByClassElement.Dictionary.Keys.Should().NotContain(["EffectStruct", "EffectEnum", "ffect"]);
        elementCache.StormXmlElementsByIdByCatalog.Dictionary.Should().BeEmpty();
        elementCache.ClassElementsByCatalog.Should().BeEmpty();

        stormModContainer.LoadedXmlFiles.Should().Equal(stormFile);
    }

    [TestMethod]
    public void LoadXmlFile_MultipleFiles_ListsFilesInLoadOrder()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile unitDataFile = CreateStormFile("UnitData.xml");
        StormFile abilDataFile = CreateStormFile("AbilData.xml");

        using XmlReader unitXmlReader = XmlReaderHelper.CreateXmlReader("""<const id="$Unit" value="1" />""");
        using XmlReader abilXmlReader = XmlReaderHelper.CreateXmlReader("""<const id="$Abil" value="1" />""");

        // act
        stormModContainer.LoadXmlFile(unitXmlReader, unitDataFile);
        stormModContainer.LoadXmlFile(abilXmlReader, abilDataFile);

        // assert
        stormModContainer.LoadedXmlFiles.Should().Equal(unitDataFile, abilDataFile);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadFontStylesFile_HasConstantAndStyle_AddsThemToItsDataStorageOnly(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);

        using XmlReader xmlReader = XmlReader.Create(new StringReader(
            """
            <StyleFile>
              <Constant name="ColorGray" val="a7a7a7" />
              <Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="#ColorGray" />
            </StyleFile>
            """));

        // act
        stormModContainer.LoadFontStylesFile(xmlReader, CreateStormFile("FontStyles.StormStyle"));

        // assert
        StyleCache styleCache = GetDataStorage(stormModType).StyleCache;
        styleCache.StormXmlStyleConstantElementByName.Dictionary.Should().ContainKey("ColorGray")
            .WhoseValue.Value.Should().Be("a7a7a7");
        styleCache.StormXmlStyleElementByName.Dictionary.Should().ContainKey("ReticleEnemy")
            .WhoseValue.Template.Should().Be("Storm_Tutorial_Reticle_Text");

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
        {
            GetDataStorage(otherStormModType).StyleCache.StormXmlStyleConstantElementByName.Dictionary.Should().BeEmpty();
            GetDataStorage(otherStormModType).StyleCache.StormXmlStyleElementByName.Dictionary.Should().BeEmpty();
        }
    }

    [TestMethod]
    public void LoadFontStylesFile_SameStyleNameInTwoFiles_MergesStyle()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using XmlReader xmlReader1 = XmlReader.Create(new StringReader("""<StyleFile><Style name="Title" template="Base" /></StyleFile>"""));
        using XmlReader xmlReader2 = XmlReader.Create(new StringReader("""<StyleFile><Style name="Title" textcolor="ffffff" /></StyleFile>"""));

        // act
        stormModContainer.LoadFontStylesFile(xmlReader1, CreateStormFile("FontStyles.StormStyle"));
        stormModContainer.LoadFontStylesFile(xmlReader2, CreateStormFile("FontStyles.StormStyle"));

        // assert
        StormXmlStyleElement styleElement = GetDataStorage(StormModType.Base).StyleCache.StormXmlStyleElementByName.Dictionary.Should().ContainSingle().Which.Value;
        styleElement.Template.Should().Be("Base");
        styleElement.Members["textcolor"].Value.RawValue.Should().Be("ffffff");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadAssetsTextFile_HasAssetTexts_AddsAssetTextsToItsDataStorageOnly(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        StormFile stormFile = CreateStormFile("Assets.txt");

        using MemoryStream stream = CreateStream("UI/HeroPortrait=Assets\\Textures\\portrait.dds\nUI/Empty=\nUI/HeroPortrait=Assets\\Textures\\new.dds");

        // act
        stormModContainer.LoadAssetsTextFile(stream, stormFile);

        // assert
        Dictionary<string, StringValue> assetTexts = GetTextCache(stormModType).AssetTextsById.Dictionary;
        assetTexts.Should().HaveCount(2);
        assetTexts["UI/HeroPortrait"].Should().Be(new StringValue("Assets\\Textures\\new.dds", stormFile));
        assetTexts["UI/Empty"].Should().Be(new StringValue(string.Empty, stormFile));

        stormModContainer.LoadedAssetsTextFiles.Should().Equal(stormFile);
        GetTextCache(stormModType).GameStringsById.Dictionary.Should().BeEmpty();

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetTextCache(otherStormModType).AssetTextsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void AddAssetFile_RelativePath_AddsFileToItsDataStorageAndList(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        StormFile stormFile = CreateStormFile("portrait.dds");
        string relativePath = Path.Combine("assets", "textures", "portrait.dds");

        // act
        stormModContainer.AddAssetFile(relativePath, stormFile);

        // assert
        GetDataStorage(stormModType).FileCache.AssetFilesByRelativePath.Dictionary.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<string, StormFile>(relativePath, stormFile));
        stormModContainer.LoadedAssetFiles.Should().Equal(stormFile);

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetDataStorage(otherStormModType).FileCache.AssetFilesByRelativePath.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void AddAssetFile_SameRelativePathTwice_ReplacesFileAndListsBoth()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile1 = CreateStormFile("portrait.dds");
        StormFile stormFile2 = CreateStormFile("Portrait.dds");
        string relativePath = Path.Combine("assets", "textures", "portrait.dds");

        // act
        stormModContainer.AddAssetFile(relativePath, stormFile1);
        stormModContainer.AddAssetFile(relativePath.ToUpperInvariant(), stormFile2);

        // assert
        GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary.Should().ContainSingle()
            .Which.Value.Should().BeSameAs(stormFile2);
        stormModContainer.LoadedAssetFiles.Should().Equal(stormFile1, stormFile2);
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void AddStormLayoutFile_RelativePath_AddsFileToItsDataStorageAndList(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        StormFile stormFile = CreateStormFile("GameUI.StormLayout");
        string relativePath = Path.Combine("ui", "layout", "gameui.stormlayout");

        // act
        stormModContainer.AddStormLayoutFile(relativePath, stormFile);

        // assert
        GetDataStorage(stormModType).FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<string, StormFile>(relativePath, stormFile));
        stormModContainer.LoadedStormLayoutFiles.Should().Equal(stormFile);

        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetDataStorage(otherStormModType).FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void AddStormLayoutFile_SameRelativePathTwice_ReplacesFileAndListsBoth()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile1 = CreateStormFile("GameUI.StormLayout");
        StormFile stormFile2 = CreateStormFile("GameUI.StormLayout");
        string relativePath = Path.Combine("ui", "layout", "gameui.stormlayout");

        // act
        stormModContainer.AddStormLayoutFile(relativePath, stormFile1);
        stormModContainer.AddStormLayoutFile(relativePath, stormFile2);

        // assert
        GetDataStorage(StormModType.Base).FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().ContainSingle()
            .Which.Value.Should().BeSameAs(stormFile2);
        stormModContainer.LoadedStormLayoutFiles.Should().Equal(stormFile1, stormFile2);
    }

    [TestMethod]
    public void ClearGameStrings_HasLoadedFiles_ClearsOnlyGameStringFileList()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);

        using MemoryStream gameStringStream = CreateStream("id1=value1");
        using MemoryStream assetsTextStream = CreateStream("asset1=value1");
        using XmlReader xmlReader = XmlReaderHelper.CreateXmlReader("""<const id="$Const1" value="1" />""");

        stormModContainer.LoadGameStringFile(gameStringStream, CreateStormFile("GameStrings.txt"));
        stormModContainer.LoadAssetsTextFile(assetsTextStream, CreateStormFile("Assets.txt"));
        stormModContainer.LoadXmlFile(xmlReader, CreateStormFile("EffectData.xml"));
        stormModContainer.AddAssetFile("assets\\a.dds", CreateStormFile("a.dds"));
        stormModContainer.AddStormLayoutFile("ui\\a.stormlayout", CreateStormFile("a.stormlayout"));

        // act
        stormModContainer.ClearGameStrings();

        // assert
        stormModContainer.LoadedGameStringFiles.Should().BeEmpty();
        stormModContainer.LoadedAssetsTextFiles.Should().ContainSingle();
        stormModContainer.LoadedXmlFiles.Should().ContainSingle();
        stormModContainer.LoadedAssetFiles.Should().ContainSingle();
        stormModContainer.LoadedStormLayoutFiles.Should().ContainSingle();

        // the gamestrings are in the shared data storage, which is cleared by the SourceContainer
        GetTextCache(StormModType.Base).GameStringsById.Dictionary.Keys.Should().Equal("id1");
    }

    [TestMethod]
    [DataRow(StormModType.Base)]
    [DataRow(StormModType.Map)]
    [DataRow(StormModType.Custom)]
    public void LoadGameStringFile_HasGameStrings_AddsGameStringsToItsDataStorage(StormModType stormModType)
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, stormModType);
        StormFile stormFile = StormFileHelper.CreateStormFile();

        using MemoryStream stream = CreateStream("Button/Name/Id1=Value1\r\nButton/Tooltip/Id2=<d ref=\"Effect,Damage,Amount\"/> damage\n");

        // act
        stormModContainer.LoadGameStringFile(stream, stormFile);

        // assert
        Dictionary<string, StringValue> gameStrings = GetTextCache(stormModType).GameStringsById.Dictionary;
        gameStrings.Should().HaveCount(2);
        gameStrings["Button/Name/Id1"].Should().Be(new StringValue("Value1", stormFile));
        gameStrings["Button/Tooltip/Id2"].Should().Be(new StringValue("<d ref=\"Effect,Damage,Amount\"/> damage", stormFile));

        stormModContainer.LoadedGameStringFiles.Should().Equal(stormFile);

        // the other layers and the asset texts are not changed
        foreach (StormModType otherStormModType in Enum.GetValues<StormModType>().Where(x => x != stormModType))
            GetTextCache(otherStormModType).GameStringsById.Dictionary.Should().BeEmpty();

        GetTextCache(stormModType).AssetTextsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadGameStringFile_HasDuplicateIdsAndBlankLines_KeepsLastValueAndSkipsBlankIds()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile = StormFileHelper.CreateStormFile();

        using MemoryStream stream = CreateStream("id1=value1\nid1=value2\nid2=value3\n\n \nid\nid=\n=noid\nid3=a=b");

        // act
        stormModContainer.LoadGameStringFile(stream, stormFile);

        // assert
        Dictionary<string, StringValue> gameStrings = GetTextCache(StormModType.Base).GameStringsById.Dictionary;
        gameStrings.Keys.Should().BeEquivalentTo("id1", "id2", "id", "id3");
        gameStrings["id1"].Value.Should().Be("value2");
        gameStrings["id2"].Value.Should().Be("value3");
        gameStrings["id"].Value.Should().BeEmpty();
        gameStrings["id3"].Value.Should().Be("a=b");
    }

    [TestMethod]
    public void LoadGameStringFile_HasUtf8BomAndNonAsciiText_ReadsText()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile = StormFileHelper.CreateStormFile();

        byte[] bytes = [.. Encoding.UTF8.Preamble, .. Encoding.UTF8.GetBytes("id1=Dégâts 한국어")];
        using MemoryStream stream = new(bytes);

        // act
        stormModContainer.LoadGameStringFile(stream, stormFile);

        // assert
        GetTextCache(StormModType.Base).GameStringsById.Dictionary.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<string, StringValue>("id1", new StringValue("Dégâts 한국어", stormFile)));
    }

    [TestMethod]
    public void LoadGameStringFile_LaterFileHasSameId_ReplacesValue()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        StormFile stormFile1 = StormFileHelper.CreateStormFile();
        StormFile stormFile2 = StormFileHelper.CreateStormFile();

        using MemoryStream stream1 = CreateStream("id1=value1\nid2=value2");
        using MemoryStream stream2 = CreateStream("id1=new");

        // act
        stormModContainer.LoadGameStringFile(stream1, stormFile1);
        stormModContainer.LoadGameStringFile(stream2, stormFile2);

        // assert
        Dictionary<string, StringValue> gameStrings = GetTextCache(StormModType.Base).GameStringsById.Dictionary;
        gameStrings["id1"].Should().Be(new StringValue("new", stormFile2));
        gameStrings["id2"].Should().Be(new StringValue("value2", stormFile1));

        stormModContainer.LoadedGameStringFiles.Should().Equal(stormFile1, stormFile2);
    }

    [TestMethod]
    public void ClearGameStrings_HasLoadedGameStringFile_ClearsLoadedGameStringFiles()
    {
        // arrange
        StormModContainer stormModContainer = new(_sourceContainer, StormModType.Base);
        _sourceContainer.AddModContainer(stormModContainer);

        using MemoryStream stream = CreateStream("id1=value1");
        stormModContainer.LoadGameStringFile(stream, StormFileHelper.CreateStormFile());

        // act
        _sourceContainer.ClearGameStrings();

        // assert
        stormModContainer.LoadedGameStringFiles.Should().BeEmpty();
        GetTextCache(StormModType.Base).GameStringsById.Dictionary.Should().BeEmpty();
    }

    private static MemoryStream CreateStream(string text) => new(Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// Creates a <see cref="StormFile"/> for a file in the <c>gamedata</c> directory. The catalog name of an XML file comes from its file name.
    /// </summary>
    /// <param name="fileName">The file name (e.g. <c>EffectData.xml</c>).</param>
    /// <returns>The <see cref="StormFile"/>.</returns>
    private static StormFile CreateStormFile(string fileName)
    {
        string relativePath = Path.Combine("test.stormmod", "base.stormdata", "GameData", fileName);

        return new StormFile()
        {
            StormModName = "test",
            RelativePath = relativePath,
            FullPath = relativePath,
        };
    }

    private DataStorage GetDataStorage(StormModType stormModType) => _sourceContainer.LayeredDataStorage.GetDataStorage(stormModType);

    private ElementCache GetElementCache(StormModType stormModType) => GetDataStorage(stormModType).ElementCache;

    private TextCache GetTextCache(StormModType stormModType) => GetDataStorage(stormModType).TextCache;
}
