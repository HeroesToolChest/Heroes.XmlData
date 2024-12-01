using Heroes.XmlData.Tests;
using U8Xml;

namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public partial class StormStorageTests
{
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

        using XmlObject xmlObject = XmlParser.Parse(element);

        // act
        string result = stormStorage.GetValueFromConstElement(xmlObject.Root);

        // assert
        result.Should().Be(computedValue);
    }

    [TestMethod]
    public void GetValueFromConstElement_HasExpressionAndIs1_ReturnsValueAsString()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var2" value="7" />""");
        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantElementById.Add("$Var2", new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("path")));

        using XmlObject element = XmlParser.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="1" />""");

        // act
        string result = stormStorage.GetValueFromConstElement(element.Root);

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

        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="0" />""");

        // act
        string result = stormStorage.GetValueFromConstElement(xmlObject.Root);

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

        using XmlObject xmlObject = XmlParser.Parse(element);

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(xmlObject.Root);

        // assert
        result.Should().Be(computedValue);
    }

    [TestMethod]
    public void GetValueFromConstElementAsNumber_HasExpressionAndIs1_ReturnsValueAsString()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var2" value="7" />""");

        StormStorage stormStorage = new(false);
        stormStorage.StormCache.ConstantElementById.Add("$Var2", new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("path")));

        using XmlObject element = XmlParser.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="1" />""");

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(element.Root);

        // assert
        result.Should().Be(9.125);
    }

    [TestMethod]
    public void GetValueFromConstElementAsNumber_HasExpressionAndIs0_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var1" value="+($Var2 2.125)" evaluateAsExpression="0" />""");

        // act
        double result = stormStorage.GetValueFromConstElementAsNumber(xmlObject.Root);

        // assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void GetValueFromConstTextAsText_HasVar_ReturnsValueAsString()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var1" value="7" />""");

        stormStorage.StormCache.ConstantElementById.Add("$Var1", new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("path")));

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

        using XmlObject xmlObject = XmlParser.Parse("""<const id="$Var1" value="7" />""");

        stormStorage.StormCache.ConstantElementById.Add("$Var1", new StormXmlValuePath(xmlObject, TestHelpers.GetStormPath("path")));

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

    [TestMethod]
    public void SetStormStyleCache_HasConstantsAndStyles_AddsToCache()
    {
        // arrange
        StormStorage stormStorage = new(false);

        using XmlObject xmlObject = XmlParser.Parse(
            """
            <StyleFile>
                <Constant name="ColorGray" val="a7a7a7" />
                <Constant name="Color2" val="bbbbbb" />
                <Style name="ColorGray" val="a7a7a7" />
                <Style name="Color2" val="bbbbbb" />
                <Other name="ColorGray" val="a7a7a7" />
                <Other name="Color2" val="bbbbbb" />
            </StyleFile>
            """);

        // act
        stormStorage.SetStormStyleCache(StormModType.Normal, xmlObject, TestHelpers.GetStormPath("normal"));

        // assert
        stormStorage.StormCache.StormStyleConstantElementsByName.Should().HaveCount(2);
        stormStorage.StormCache.StormStyleStyleElementsByName.Should().HaveCount(2);
    }
}
