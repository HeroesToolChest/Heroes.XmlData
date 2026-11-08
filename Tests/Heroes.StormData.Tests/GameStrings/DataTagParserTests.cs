using Heroes.StormData.GameStrings;

namespace Heroes.StormData.Tests.GameStrings;

[TestClass]
public class DataTagParserTests
{
    [TestMethod]
    [DataRow("[d ref='Effect,TestDamage,Amount'/]")]
    [DataRow("[d ref=\"Effect,TestDamage,Amount\"/]")]
    [DataRow("[D ref='Effect,TestDamage,Amount'/]")]
    [DataRow("[d ref='Effect,TestDamage,Amount' /]")]
    [DataRow("[d ref='Effect,TestDamage,Amount*2'/]")]
    [DataRow("[d ref='Effect,TestDamage,Amount' player='0'/]")]
    [DataRow("[d ref='Effect,TestDamage,Amount'player='0'/]")] // no space between the attributes
    [DataRow("[d player='0' ref='Effect,TestDamage,Amount'/]")]
    [DataRow("[d ref='Effect,TestDamage,Amount'\tplayer='0'/]")]
    [DataRow("[d  ref='Effect,TestDamage,Amount'  /]")]
    [DataRow("[d ref = 'Effect,TestDamage,Amount'/]")] // spaces around the =
    [DataRow("[d ref='Effect,TestDamage,Amount' ref='Effect,TestDecimal,Amount'/]")] // the same attribute twice
    [DataRow("[d ref='Effect,TestScaling,Amount'/]")]
    [DataRow("[d ref='Effect,TestScaling,Amount*100' precision='1'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' Precision='1'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='3' Precision='1'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' Precision='1' precision='3'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' PRECISION='2'/]")]
    [DataRow("[d Ref='Effect,TestDamage,Amount'/]")] // attribute names ignore case
    [DataRow("[d REF='Effect,TestDamage,Amount' ref='Effect,TestDecimal,Amount'/]")]
    [DataRow("[d Const='$TestConst'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='x'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision=''/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='-1'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='+2'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision=' 2'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2 '/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='02'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='99'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2.0'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2.7'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='-2.7'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2abc'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2 3'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='3e1'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='.5'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision=' -2'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='- 2'/]")]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2147483648'/]")]
    [DataRow("[d const='$TestConst'/]")]
    [DataRow("[d const='$TestConst' precision='1'/]")]
    [DataRow("[d const='$TestConst' ref='Effect,TestDamage,Amount'/]")] // const is used over ref
    [DataRow("[d ref='Effect,TestDamage,Amount' const='$TestConst'/]")]
    [DataRow("[d const='$UnknownConst'/]")]
    [DataRow("[d ref='Effect,UnknownDamage,Amount'/]")]
    [DataRow("[d ref=''/]")]
    [DataRow("[d player='0'/]")]
    [DataRow("[d/]")]
    [DataRow("[d ref=Effect,TestDamage,Amount/]")] // not quoted
    [DataRow("[d ref='Effect,TestDamage,Amount/]")] // no closing quote
    [DataRow("[d ref='Effect,TestDamage,Amount\"/]")] // mismatched quotes
    [DataRow("[d ref='Effect,TestDamage,Amount' extra/]")] // not an attribute
    [DataRow("[d ref/]")]
    [DataRow("[d ='Effect,TestDamage,Amount'/]")]
    [DataRow("[d r ef='Effect,TestDamage,Amount'/]")]
    [DataRow("[ ref='Effect,TestDamage,Amount'/]")] // no tag name
    public void ParseBracketDataTagAndParseDataTag_SameTagAsXml_ReturnSameAsXmlParsing(string bracketDataTag)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // the xml tag the bracket used to be turned into
        string xmlDataTag = $"<{bracketDataTag[1..^1]}>";

        // act
        ValueScale expected = parser.ParseXmlDataTag(xmlDataTag);
        ValueScale bracketResult = parser.ParseBracketDataTag(bracketDataTag);
        ValueScale dataTagResult = parser.ParseDataTag(xmlDataTag);

        // assert
        bracketResult.Value.Should().Be(expected.Value);
        bracketResult.Scaling.Should().Be(expected.Scaling);
        dataTagResult.Value.Should().Be(expected.Value);
        dataTagResult.Scaling.Should().Be(expected.Scaling);
    }

    [TestMethod]
    [DataRow("<d ref=\"Effect,TestDamage,Amount\"/>")]
    [DataRow("<d ref=\"Effect,TestDamage,Amount\" precision=\"2\"/>")]
    [DataRow("<d ref=\"Effect,TestDecimal,Amount\"precision=\"2\"/>")] // no space before precision, as in some game strings
    [DataRow("<d ref=\"Effect,TestDamage,Amount\"player=\"0\"/>")]
    [DataRow("<d ref=\"Effect,TestDamage,Amount\" Player=\"0\"/>")]
    [DataRow("<d ref=\"Effect,TestDamage,Amount \"/>")] // space at the end of the value
    [DataRow("<d ref=\"Effect,TestDamage,Amount * 100\"/>")]
    [DataRow("<d ref=\"Effect,TestScaling,Amount\"/>")]
    [DataRow("<d ref=\"100*(1+Effect,TestDecimal,Amount)“/>")] // closed with a curly quote, as in a game string
    [DataRow("<d ref=\"Effect,TestDamage,Amount\">")] // not self-closing
    [DataRow("<d ref=\"Effect,TestDamage,Amount\"></d>")]
    [DataRow("<d\nref=\"Effect,TestDamage,Amount\"\n/>")]
    [DataRow("<d ref=\"Effect,TestDamage,Amount&amp;\"/>")] // xml entity
    [DataRow("<d const=\"$TestConst&#46;\"/>")]
    [DataRow("<d ref=\"Effect,TestDamage,Amount<\"/>")]
    [DataRow("<d data-ref=\"Effect,TestDamage,Amount\"/>")] // attribute name that isn't only letters
    [DataRow("<c val=\"#TooltipNumbers\"/>")] // a different tag
    [DataRow("<dd ref=\"Effect,TestDamage,Amount\"/>")]
    [DataRow("<d/>")]
    [DataRow("<d />")]
    [DataRow("<>")]
    [DataRow("<d")]
    [DataRow("")]
    public void ParseDataTag_GameStringTag_ReturnsSameAsXmlParsing(string dataTag)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale expected = parser.ParseXmlDataTag(dataTag);
        ValueScale result = parser.ParseDataTag(dataTag);

        // assert
        result.Value.Should().Be(expected.Value);
        result.Scaling.Should().Be(expected.Scaling);
    }

    [TestMethod]
    [DataRow("<d ref=\"Effect,TestDamage,Amount\"/>", 120)]
    [DataRow("<d ref=\"Effect,TestDecimal,Amount\"precision=\"2\"/>", 1.23)]
    [DataRow("<d const=\"$TestConst\" precision=\"1\"/>", 7.2)]
    [DataRow("<D ref='Effect,TestDamage,Amount*2'/>", 240)]
    [DataRow("<d ref=\"Effect,TestDamage,Amount&amp;\"/>", 0)] // parsed as xml
    public void ParseDataTag_Tag_ReturnsValue(string dataTag, double expected)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale result = parser.ParseDataTag(dataTag);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    public void ParseDataTag_TagParsedAsXmlWithValueLongerThanStackBuffer_ReturnsValue()
    {
        // arrange
        // not self-closing, so it is parsed as xml, and its 599 character ref is decoded into a rented buffer
        DataTagParser parser = new(CreateStorage());
        string dRef = string.Join('+', Enumerable.Repeat("1", 300));

        // act
        ValueScale result = parser.ParseDataTag($"<d ref=\"{dRef}\"></d>");

        // assert
        result.Value.Should().Be(300);
    }

    [TestMethod]
    public void ParseBracketDataTag_TagParsedAsXmlLongerThanStackBuffer_ReturnsValue()
    {
        // arrange
        // the &amp; makes it parsed as xml, so the bracket is copied into a rented buffer as <d .../>
        DataTagParser parser = new(CreateStorage());
        string dRef = string.Join('+', Enumerable.Repeat("1", 300));

        // act
        ValueScale result = parser.ParseBracketDataTag($"[d ref='{dRef}' note='&amp;'/]");

        // assert
        result.Value.Should().Be(300);
    }

    [TestMethod]
    [DataRow("d Ref='Effect,TestDamage,Amount'", 120)]
    [DataRow("d REF='Effect,TestDamage,Amount'", 120)]
    [DataRow("d Const='$TestConst'", 7)]
    [DataRow("d ref='Effect,TestDecimal,Amount' PRECISION='2'", 1.23)]
    [DataRow("d ref='Effect,TestDecimal,Amount' Precision='1'", 1.2)]
    [DataRow("d ref='Effect,TestDecimal,Amount' precision='3' Precision='1'", 1.235)] // the first precision is used
    [DataRow("d ref='Effect,TestDecimal,Amount' Precision='1' precision='3'", 1.2)]
    [DataRow("d Ref='Effect,TestDamage,Amount' ref='Effect,TestDecimal,Amount'", 120)] // the first ref is used
    [DataRow("D REF='Effect,TestDamage,Amount'", 120)]
    public void ParseDataTagAndParseBracketDataTag_AttributeNamesInAnyCase_ReadsAttributes(string tagContent, double expected)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale dataTagResult = parser.ParseDataTag($"<{tagContent}/>");
        ValueScale bracketResult = parser.ParseBracketDataTag($"[{tagContent}/]");

        // assert
        dataTagResult.Value.Should().Be(expected);
        bracketResult.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("<d Ref=\"Effect,TestDamage,Amount\" player=\"&amp;\"/>", 120)]
    [DataRow("<d ref=\"Effect,TestDecimal,Amount\" PRECISION=\"2\" player=\"&amp;\"/>", 1.23)]
    [DataRow("<d CONST=\"$TestConst\" player=\"&amp;\"/>", 7)]
    [DataRow("<d Ref=\"Effect,TestDamage,Amount\"></d>", 120)] // not self-closing
    public void ParseDataTag_AttributeNameInOtherCaseInTagParsedAsXml_IsRead(string dataTag, double expected)
    {
        // arrange
        // the &amp; or the closing tag makes these tags parsed as xml, which also ignores the case of attribute names
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale result = parser.ParseDataTag(dataTag);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("[d ref='Effect,TestDamage,Amount'/]", 120)]
    [DataRow("[d ref='Effect,TestDamage,Amount'player='0'/]", 120)]
    [DataRow("[d ref='Effect,TestDecimal,Amount'/]", 1)]
    [DataRow("[d ref='Effect,TestDecimal,Amount' precision='2'/]", 1.23)]
    [DataRow("[d ref='Effect,TestDecimal,Amount' Precision='3'/]", 1.235)]
    [DataRow("[d const='$TestConst' precision='1'/]", 7.2)]
    [DataRow("[d const='$TestConst' ref='Effect,TestDamage,Amount'/]", 7)]
    [DataRow("[d ref=Effect,TestDamage,Amount/]", 0)]
    public void ParseBracketDataTag_Tag_ReturnsValue(string bracketDataTag, double expected)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale result = parser.ParseBracketDataTag(bracketDataTag);

        // assert
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    public void ParseBracketDataTag_RefHasLevelScaling_ReturnsScaling()
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale result = parser.ParseBracketDataTag("[d ref='Effect,TestScaling,Amount'/]");

        // assert
        result.Value.Should().Be(50);
        result.Scaling.Should().Be(0.04);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("[d")]
    [DataRow("[d ref='Effect,TestDamage,Amount'")]
    [DataRow("d ref='Effect,TestDamage,Amount'/]")]
    public void ParseBracketDataTag_NotABracketDataTag_ReturnsZero(string bracketDataTag)
    {
        // arrange
        DataTagParser parser = new(CreateStorage());

        // act
        ValueScale result = parser.ParseBracketDataTag(bracketDataTag);

        // assert
        result.Value.Should().Be(0);
        result.Scaling.Should().BeNull();
    }

    // Effect,TestDamage,Amount 120, Effect,TestDecimal,Amount 1.23456, Effect,TestScaling,Amount 50 with a 0.04 scaling, and $TestConst 7.25
    private static LayeredDataStorage CreateStorage()
    {
        LayeredDataStorage layeredDataStorage = new();
        DataStorage dataStorage = layeredDataStorage.GetDataStorage(StormModType.Custom);

        dataStorage.AddBaseClassCatalogName(XmlReaderHelper.GetStormXmlElement("<CEffectDamage />"), "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="TestDamage">
                  <Amount value="120" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="TestDecimal">
                  <Amount value="1.23456" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddElementWithId(
            XmlReaderHelper.GetStormXmlElement(
                """
                <CEffectDamage id="TestScaling">
                  <Amount value="50" />
                </CEffectDamage>
                """),
            "Effect");
        dataStorage.AddLevelScalingElementValue(XmlReaderHelper.GetStormXmlElement(
            """
            <CHero id="TestHero">
              <LevelScalingArray Ability="TestAbil">
                <Modifications>
                  <Catalog value="Effect" />
                  <Entry value="TestScaling" />
                  <Field value="Amount" />
                  <Value value="0.040000" />
                </Modifications>
              </LevelScalingArray>
            </CHero>
            """).Members.GetMember("LevelScalingArray"));
        dataStorage.AddConstElement(XmlReaderHelper.GetStormXmlConstElement(
            """
            <const id="$TestConst" value="7.25" />
            """));

        return layeredDataStorage;
    }
}
