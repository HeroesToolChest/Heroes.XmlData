namespace Heroes.XmlData.Tests.StormData;

[TestClass]
public class StormModStorageTests
{
    private readonly IStormMod _stormMod;
    private readonly IStormStorage _stormStorage;

    public StormModStorageTests()
    {
        _stormMod = Substitute.For<IStormMod>();
        _stormStorage = Substitute.For<IStormStorage>();
    }

    [TestMethod]
    public void AddDirectoryNotFound_HasDirectories_AddsDirectories()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        StormPath stormPath1 = TestHelpers.GetStormPath("test1");
        StormPath stormPath2 = TestHelpers.GetStormPath("test2");

        // act
        stormModStorage.AddDirectoryNotFound(stormPath1);
        stormModStorage.AddDirectoryNotFound(stormPath2);

        // assert
        stormModStorage.NotFoundDirectories.Should().HaveCount(2);
        stormModStorage.NotFoundDirectories.Should().BeEquivalentTo(new[] { stormPath1, stormPath2 });
        _stormStorage.Received().AddDirectoryNotFound(Arg.Any<StormModType>(), stormPath1);
        _stormStorage.Received().AddDirectoryNotFound(Arg.Any<StormModType>(), stormPath2);
    }

    [TestMethod]
    public void AddFileNotFound_HasFile_AddsFile()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        StormPath stormPath1 = TestHelpers.GetStormPath("test1");
        StormPath stormPath2 = TestHelpers.GetStormPath("test2");

        // act
        stormModStorage.AddFileNotFound(stormPath1);
        stormModStorage.AddFileNotFound(stormPath2);

        // assert
        stormModStorage.NotFoundFiles.Should().HaveCount(2);
        stormModStorage.NotFoundFiles.Should().BeEquivalentTo(new[] { stormPath1, stormPath2 });

        _stormStorage.Received().AddFileNotFound(Arg.Any<StormModType>(), stormPath1);
        _stormStorage.Received().AddFileNotFound(Arg.Any<StormModType>(), stormPath2);
    }

    [TestMethod]
    public void AddGameString_HasGamestrings_AddsGamestrings()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        StormPath stormPath1 = TestHelpers.GetStormPath("test1");
        StormPath stormPath2 = TestHelpers.GetStormPath("test2");
        StormPath stormPath3 = TestHelpers.GetStormPath("test3");

        GameStringText gameStringText1 = new("value1", stormPath1);
        GameStringText gameStringText2 = new("value2", stormPath2);
        GameStringText gameStringText3 = new("value2", stormPath3);

        // act
        stormModStorage.AddGameString("id1", gameStringText1);
        stormModStorage.AddGameString("id1", gameStringText2);
        stormModStorage.AddGameString("id2", gameStringText3);

        // assert
        stormModStorage.GameStringsById.Should().BeEquivalentTo(new Dictionary<string, GameStringText>
        {
            { "id1", gameStringText2 },
            { "id2", gameStringText3 },
        });
        _stormStorage.Received().AddGameString(Arg.Any<StormModType>(), "id1", gameStringText1);
        _stormStorage.Received().AddGameString(Arg.Any<StormModType>(), "id1", gameStringText2);
        _stormStorage.Received().AddGameString(Arg.Any<StormModType>(), "id2", gameStringText3);
    }

    [TestMethod]
    public void AddAssetText_HasAssets_AddsAssetsText()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        StormPath stormPath1 = TestHelpers.GetStormPath("test1");
        StormPath stormPath2 = TestHelpers.GetStormPath("test2");
        StormPath stormPath3 = TestHelpers.GetStormPath("test3");

        AssetText assetText1 = new("value1", stormPath1);
        AssetText assetText2 = new("value2", stormPath2);
        AssetText assetText3 = new("value2", stormPath3);

        // act
        stormModStorage.AddAssetText("id1", assetText1);
        stormModStorage.AddAssetText("id1", assetText2);
        stormModStorage.AddAssetText("id2", assetText3);

        // assert
        stormModStorage.AssetTextsById.Should().BeEquivalentTo(new Dictionary<string, AssetText>
        {
            { "id1", assetText2 },
            { "id2", assetText3 },
        });
        _stormStorage.Received().AddAssetText(Arg.Any<StormModType>(), "id1", assetText1);
        _stormStorage.Received().AddAssetText(Arg.Any<StormModType>(), "id1", assetText2);
        _stormStorage.Received().AddAssetText(Arg.Any<StormModType>(), "id2", assetText3);
    }

    [TestMethod]
    public void AddGameStringFile_HasFile_AddGamestrings()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, new StormStorage(false));

        StormPath stormPath1 = TestHelpers.GetStormPath("test1");

        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        writer.WriteLine("id1=value1");
        writer.WriteLine("id1=value2");
        writer.WriteLine("id2=value3");
        writer.WriteLine(string.Empty);
        writer.WriteLine(" ");
        writer.WriteLine("id");
        writer.WriteLine("id=");
        writer.Flush();
        stream.Position = 0;

        // act
        stormModStorage.AddGameStringFile(stream, stormPath1);

        // assert
        stormModStorage.AddedGameStringFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormModStorage.GameStringsById.Should().BeEquivalentTo(new Dictionary<string, GameStringText>
        {
            { "id1", new("value2", stormPath1) },
            { "id2", new("value3", stormPath1) },
            { "id", new(string.Empty, stormPath1) },
        });
    }

    [TestMethod]
    public void AddGameStringFile_HasAlreadyExistingFile_DoNotAddGamestrings()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, new StormStorage(false));

        StormPath stormPath1 = TestHelpers.GetStormPath("test1");

        using MemoryStream stream1 = new();
        using StreamWriter writer1 = new(stream1);
        writer1.WriteLine("id1=value1");
        writer1.Flush();
        stream1.Position = 0;

        using MemoryStream stream2 = new();
        using StreamWriter writer2 = new(stream2);
        writer2.WriteLine("id2=value2");
        writer2.Flush();
        stream2.Position = 0;

        // act
        stormModStorage.AddGameStringFile(stream1, stormPath1);
        stormModStorage.AddGameStringFile(stream2, stormPath1);

        // assert
        stormModStorage.AddedGameStringFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormModStorage.GameStringsById.Should().BeEquivalentTo(new Dictionary<string, GameStringText>
        {
            { "id1", new("value1", stormPath1) },
        });
    }

    [TestMethod]
    public void AddAssetsTextFile_HasFile_AddAssetTexts()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, new StormStorage(false));

        StormPath stormPath1 = TestHelpers.GetStormPath("test1");

        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        writer.WriteLine("id1=value1");
        writer.WriteLine("id1=value2");
        writer.WriteLine("id2=value3");
        writer.WriteLine(string.Empty);
        writer.WriteLine(" ");
        writer.WriteLine("id");
        writer.WriteLine("id=");
        writer.Flush();
        stream.Position = 0;

        // act
        stormModStorage.AddAssetsTextFile(stream, stormPath1);

        // assert
        stormModStorage.AddedAssetsFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormModStorage.AssetTextsById.Should().BeEquivalentTo(new Dictionary<string, AssetText>
        {
            { "id1", new("value2", stormPath1) },
            { "id2", new("value3", stormPath1) },
            { "id", new(string.Empty, stormPath1) },
        });
    }

    [TestMethod]
    public void AddAssetsTextFile_HasAlreadyExistingFile_DoNotAddAssetTexts()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, new StormStorage(false));

        StormPath stormPath1 = TestHelpers.GetStormPath("test1");

        using MemoryStream stream1 = new();
        using StreamWriter writer1 = new(stream1);
        writer1.WriteLine("id1=value1");
        writer1.Flush();
        stream1.Position = 0;

        using MemoryStream stream2 = new();
        using StreamWriter writer2 = new(stream2);
        writer2.WriteLine("id2=value2");
        writer2.Flush();
        stream2.Position = 0;

        // act
        stormModStorage.AddAssetsTextFile(stream1, stormPath1);
        stormModStorage.AddAssetsTextFile(stream2, stormPath1);

        // assert
        stormModStorage.AddedAssetsFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormModStorage.AssetTextsById.Should().BeEquivalentTo(new Dictionary<string, AssetText>
        {
            { "id1", new("value1", stormPath1) },
        });
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=true")]
    public void AddXmlDataFile_XDocumentWithBaseElementTypes_AddsElements()
    {
        // arrange
        string fileName = "behaviordata.xml";

        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
            XElement.Parse(@"
<Catalog>
  <CBehaviorBuff default=""1"">
    <InfoFlags index=""Hidden"" value=""1"" />
  </CBehaviorBuff>
  <CBehaviorBuff default=""1"">
    <InfoFlags index=""Hidden"" value=""1"" />
  </CBehaviorBuff>
</Catalog>"));

        StormPath stormPath1 = TestHelpers.GetStormPath(fileName);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, true);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(2).AddConstantXElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.Received(2).AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "CBehaviorBuff");
        _stormStorage.Received(2).AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=true")]
    public void AddXmlDataFile_XDocumentFileNameDoesNotEndWithDataXml_AddsElements()
    {
        // arrange
        string fileName = "behavior.xml";

        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
            XElement.Parse(@"
<Catalog>
  <CBehaviorBuff default=""1"">
    <InfoFlags index=""Hidden"" value=""1"" />
  </CBehaviorBuff>
</Catalog>"));

        StormPath stormPath1 = TestHelpers.GetStormPath(fileName);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, true);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.DidNotReceive().AddBaseElementTypes(Arg.Any<StormModType>(), Arg.Any<string>(), Arg.Any<string>());
        _stormStorage.DidNotReceive().AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=true")]
    public void AddXmlDataFile_XDocumentWithBaseConstElements_AddsElements()
    {
        // arrange
        string fileName = "behaviordata.xml";

        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XElement element1 = XElement.Parse("<const id=\"$ChromieBasicAttackRange\" value=\"7\" />");
        XElement element2 = XElement.Parse("<const id=\"$ChromieBasicAttackDamage\" value=\"82\" />");
        XElement element3 = XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""1"" />
</CBehaviorBuff>
");

        XDocument xmlDoc = new(
            new XElement(
                "Catalog",
                element1,
                element2,
                element3));

        StormPath stormPath1 = TestHelpers.GetStormPath(fileName);

        _stormStorage.AddConstantXElement(Arg.Any<StormModType>(), element1, stormPath1).Returns(true);
        _stormStorage.AddConstantXElement(Arg.Any<StormModType>(), element2, stormPath1).Returns(true);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, true);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(3).AddConstantXElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.Received(1).AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "CBehaviorBuff");
        _stormStorage.Received(1).AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=true")]
    public void AddXmlDataFile_XDocumentFileIsEmpty_AddsDefaultElement()
    {
        // arrange
        string fileName = "behaviordata.xml";

        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
            XElement.Parse(@"
<Catalog>
</Catalog>"));

        StormPath stormPath1 = TestHelpers.GetStormPath(fileName);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, true);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(1).AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "Cbehavior");
        _stormStorage.DidNotReceive().AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=true")]
    public void AddXmlDataFile_XDocumentWithBaseConstValues_AddsElementsWithUpdatedConstValue()
    {
        // arrange
        string fileName = "behaviordata.xml";

        StormStorage stormStorage = new(false);
        StormModStorage stormModStorage = new(_stormMod, stormStorage);

        XElement element1 = XElement.Parse("<const id=\"$ChromieBasicAttackRange\" value=\"7\" />");
        XElement element3 = XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""$ChromieBasicAttackRange"" />
</CBehaviorBuff>
");

        XDocument xmlDoc = new(
            new XElement(
                "Catalog",
                element1,
                element3));

        StormPath stormPath1 = TestHelpers.GetStormPath(fileName);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, true);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormStorage.StormCache.StormElementByElementType["CBehaviorBuff"].GetXmlData("InfoFlags").GetXmlData("Hidden").Value.Should().Be("$ChromieBasicAttackRange");
        stormStorage.StormCache.StormElementByElementType["CBehaviorBuff"].GetXmlData("InfoFlags").GetXmlData("Hidden").ConstValue.Should().Be("7");
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=false")]
    public void AddXmlDataFile_XDocumentWithElements_AddsElements()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
            XElement.Parse(@"
    <Catalog>
      <CBehaviorBuff default=""1"">
        <InfoFlags index=""Hidden"" value=""1"" />
      </CBehaviorBuff>
      <CBehaviorBuff default=""1"">
        <InfoFlags index=""Hidden"" value=""1"" />
      </CBehaviorBuff>
    </Catalog>"));

        StormPath stormPath1 = TestHelpers.GetStormPath("behaviordata.xml");

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, false);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(2).AddConstantXElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.Received(2).AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.DidNotReceive().AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "CBehaviorBuff");
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=false")]
    public void AddXmlDataFile_XDocumentWithConstElements_AddsElements()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XElement element1 = XElement.Parse("<const id=\"$ChromieBasicAttackRange\" value=\"7\" />");
        XElement element2 = XElement.Parse("<const id=\"$ChromieBasicAttackDamage\" value=\"82\" />");
        XElement element3 = XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""1"" />
</CBehaviorBuff>
");

        XDocument xmlDoc = new(
            new XElement(
                "Catalog",
                element1,
                element2,
                element3));

        StormPath stormPath1 = TestHelpers.GetStormPath("behaviordata.xml");

        _stormStorage.AddConstantXElement(Arg.Any<StormModType>(), element1, stormPath1).Returns(true);
        _stormStorage.AddConstantXElement(Arg.Any<StormModType>(), element2, stormPath1).Returns(true);

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, false);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(3).AddConstantXElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.Received(1).AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.DidNotReceive().AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "CBehaviorBuff");
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=false")]
    public void AddXmlDataFile_XDocumentWithConstValues_AddsElementsWithUpdatedConstValue()
    {
        // arrange
        StormStorage stormStorage = new(false);
        StormModStorage stormModStorage = new(_stormMod, stormStorage);

        XElement element1 = XElement.Parse("<const id=\"$ChromieBasicAttackRange\" value=\"7\" />");
        XElement element3 = XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""$ChromieBasicAttackRange"" />
</CBehaviorBuff>
");

        XDocument xmlDoc = new(
            new XElement(
                "Catalog",
                element1,
                element3));

        StormPath stormPath1 = TestHelpers.GetStormPath("behaviordata.xml");

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, false);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        stormStorage.StormCache.StormElementByElementType["CBehaviorBuff"].GetXmlData("InfoFlags").GetXmlData("Hidden").Value.Should().Be("$ChromieBasicAttackRange");
        stormStorage.StormCache.StormElementByElementType["CBehaviorBuff"].GetXmlData("InfoFlags").GetXmlData("Hidden").ConstValue.Should().Be("7");
    }

    [TestMethod]
    [TestCategory("IsBaseGameDataDirectory=false")]
    public void AddXmlDataFile_FilePathAlreadyExists_ReturnAndDoNothing()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XElement element = XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" />
</CBehaviorBuff>
");

        XDocument xmlDoc = new(
            new XElement(
                "Catalog",
                element));

        StormPath stormPath1 = TestHelpers.GetStormPath("behaviordata.xml");

        // act
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, false);
        stormModStorage.AddXmlDataFile(xmlDoc, stormPath1, false);

        // assert
        stormModStorage.AddedXmlDataFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(1).AddConstantXElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.Received(1).AddElement(Arg.Any<StormModType>(), Arg.Any<XElement>(), stormPath1);
        _stormStorage.DidNotReceive().AddBaseElementTypes(Arg.Any<StormModType>(), "behavior", "CBehaviorBuff");
    }

    [TestMethod]
    public void AddXmlFontStyleFile_HasNewStormStyleDoc_AddsStormStyles()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
    XElement.Parse(@"
    <StyleFile>
      <Constant name=""ColorGray"" val=""a7a7a7"" />
    </StyleFile>"));

        StormPath stormPath1 = TestHelpers.GetStormPath("fontstyle.stormstyle");

        // act
        stormModStorage.AddXmlFontStyleFile(xmlDoc, stormPath1);

        // assert
        stormModStorage.AddedXmlFontStyleFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(1).SetStormStyleCache(Arg.Any<StormModType>(), xmlDoc, stormPath1);
    }

    [TestMethod]
    public void AddXmlFontStyleFile_FilePathAlreadyExists_ReturnAndDoNothing()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        XDocument xmlDoc = new(
    XElement.Parse(@"
    <StyleFile>
      <Constant name=""ColorGray"" val=""a7a7a7"" />
    </StyleFile>"));

        StormPath stormPath1 = TestHelpers.GetStormPath("fontstyle.stormstyle");

        // act
        stormModStorage.AddXmlFontStyleFile(xmlDoc, stormPath1);
        stormModStorage.AddXmlFontStyleFile(xmlDoc, stormPath1);

        // assert
        stormModStorage.AddedXmlFontStyleFilePaths.Should().BeEquivalentTo(new[] { stormPath1 });
        _stormStorage.Received(1).SetStormStyleCache(Arg.Any<StormModType>(), xmlDoc, stormPath1);
    }

    [TestMethod]
    [DataRow("B343", 343)]
    [DataRow("1", 1)]
    public void AddBuildIdFile_HasBuildNumber_SetsBuildNumber(string text, int number)
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        using MemoryStream memoryStream = new();
        using StreamWriter writer = new(memoryStream);

        writer.WriteLine(text);
        writer.Flush();
        memoryStream.Position = 0;

        // act
        stormModStorage.AddBuildIdFile(memoryStream);

        // assert
        stormModStorage.BuildId.Should().Be(number);
    }

    [TestMethod]
    public void AddBuildIdFile_NoBuildNumber_DoesNotSet()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        using MemoryStream memoryStream = new();
        using StreamWriter writer = new(memoryStream);

        writer.WriteLine("abc");
        writer.Flush();
        memoryStream.Position = 0;

        // act
        stormModStorage.AddBuildIdFile(memoryStream);

        // assert
        stormModStorage.BuildId.Should().BeNull();
    }

    [TestMethod]
    public void AddBuildIdFile_EmtpyStream_DoesNotSet()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);
        using MemoryStream memoryStream = new();
        using StreamWriter writer = new(memoryStream);

        // act
        stormModStorage.AddBuildIdFile(memoryStream);

        // assert
        stormModStorage.BuildId.Should().BeNull();
    }

    [TestMethod]
    public void AddStormLayoutFile_HasFilePath_AddLayoutFilePath()
    {
        // arrange
        StormModStorage stormModStorage = new(_stormMod, _stormStorage);

        string relativePath = Path.Join("ui", "this", "file");
        StormPath stormPath = TestHelpers.GetStormPath(relativePath);

        // act
        stormModStorage.AddStormLayoutFilePath(relativePath, stormPath);

        // assert
        stormModStorage.FoundLayoutFilePaths.Should().BeEquivalentTo(new[] { stormPath });
        _stormStorage.Received().AddStormLayoutFilePath(Arg.Any<StormModType>(), relativePath, stormPath);
    }

    [TestMethod]
    public void UpdateConstantAttributes_Elements_AttributesWithConstantsAreUpdated()
    {
        // arrange
        List<XElement> elements =
        [
            XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""$ChromieBasicAttackRange"" />
  <InfoFlags index=""Hidden"" value=""5"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange,xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange.xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange;xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange xyz"" />
</CBehaviorBuff>
    "),
        ];

        StormStorage stormStorage = new(false);
        stormStorage.AddConstantXElement(StormModType.Normal, XElement.Parse("<const id=\"$ChromieBasicAttackRange\" value=\"7\" />"), TestHelpers.GetStormPath("test"));

        StormModStorage stormModStorage = new(_stormMod, stormStorage);

        // act
        stormModStorage.UpdateConstantAttributes(elements.DescendantsAndSelf());

        // assert
        elements.Should().BeEquivalentTo(new XElement[]
        {
            XElement.Parse(@"
<CBehaviorBuff default=""1"">
  <InfoFlags index=""Hidden"" value=""$ChromieBasicAttackRange"" Hxdconst-value=""7"" />
  <InfoFlags index=""Hidden"" value=""5"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange"" Hxdconst-value=""xyz 7"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange,xyz"" Hxdconst-value=""xyz 7,xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange.xyz"" Hxdconst-value=""xyz 7.xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange;xyz"" Hxdconst-value=""xyz 7;xyz"" />
  <InfoFlags index=""Hidden"" value=""xyz $ChromieBasicAttackRange xyz"" Hxdconst-value=""xyz 7 xyz"" />
</CBehaviorBuff>
    "),
        });
    }
}
