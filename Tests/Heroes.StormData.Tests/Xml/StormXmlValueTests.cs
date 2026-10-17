using System.Globalization;

namespace Heroes.StormData.Xml.Tests;

[TestClass]
public sealed class StormXmlValueTests
{
    [TestMethod]
    public void HasValue_DefaultStruct_ReturnsFalse()
    {
        // arrange
        StormXmlValue value = default;

        // act
        bool result = value.HasValue;

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void HasValue_EmptyString_ReturnsTrue()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(string.Empty);

        // act
        bool result = value.HasValue;

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void RawValue_DefaultStruct_ReturnsEmptyString()
    {
        // arrange
        StormXmlValue value = default;

        // act
        string result = value.RawValue;

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void RawValue_WithToken_DoesNotReplaceToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("Deals ##damage## damage", ("damage", "100"));

        // act
        string result = value.RawValue;

        // assert
        result.Should().Be("Deals ##damage## damage");
    }

    [TestMethod]
    public void ImplicitStringConversion_WithToken_ReturnsRawValue()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a##", ("a", "A"));

        // act
        string result = value;

        // assert
        result.Should().Be("##a##");
    }

    [TestMethod]
    public void ImplicitStringConversion_DefaultStruct_ReturnsEmptyString()
    {
        // arrange
        StormXmlValue value = default;

        // act
        string result = value;

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetString_DefaultStruct_ReturnsEmptyString()
    {
        // arrange
        StormXmlValue value = default;

        // act
        string result = value.GetString();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("plain text")]
    [DataRow("Level #3")]
    [DataRow("#")]
    [DataRow("trailing #")]
    [DataRow("a # b # c")]
    public void GetString_NoDoubleHash_ReturnsRawValue(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw, ("b", "B"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be(raw);
    }

    [TestMethod]
    public void GetString_SingleToken_ReplacesToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##damage##", ("damage", "100"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("100");
    }

    [TestMethod]
    public void GetString_TokenWithSurroundingText_ReplacesToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("Deals ##damage## damage to enemies.", ("damage", "100"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("Deals 100 damage to enemies.");
    }

    [TestMethod]
    public void GetString_MultipleTokens_ReplacesAllTokens()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a## and ##b## and ##c##", ("a", "A"), ("b", "B"), ("c", "C"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("A and B and C");
    }

    [TestMethod]
    public void GetString_AdjacentTokens_ReplacesAllTokens()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a####b##", ("a", "A"), ("b", "B"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("AB");
    }

    [TestMethod]
    public void GetString_SameTokenRepeated_ReplacesEachOccurrence()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a##-##a##-##a##", ("a", "x"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("x-x-x");
    }

    [TestMethod]
    public void GetString_TokensAtStartAndEnd_ReplacesTokens()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a## middle ##b##", ("a", "start"), ("b", "end"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("start middle end");
    }

    [TestMethod]
    public void GetString_SingleHashBeforeToken_KeepsHashAndReplacesToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("#1 ##a##", ("a", "A"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("#1 A");
    }

    [TestMethod]
    public void GetString_MissingToken_LeavesTokenText()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("Deals ##damage## damage", ("other", "1"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("Deals ##damage## damage");
    }

    [TestMethod]
    public void GetString_NoTokensInRoot_LeavesTokenText()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("Deals ##damage## damage");

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("Deals ##damage## damage");
    }

    [TestMethod]
    public void GetString_SomeTokensMissing_ReplacesOnlyExistingTokens()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a## ##missing## ##b##", ("a", "A"), ("b", "B"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("A ##missing## B");
    }

    [TestMethod]
    public void GetString_TokenNameDifferentCase_LeavesTokenText()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##Name##", ("name", "x"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("##Name##");
    }

    [TestMethod]
    public void GetString_EmptyReplacementValue_RemovesToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("a##x##b", ("x", string.Empty));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("ab");
    }

    [TestMethod]
    public void GetString_OnlyHashes_ReturnsRawValue()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("####", ("a", "A"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("####");
    }

    [TestMethod]
    public void GetString_ReplacementLongerThanRawValue_ReturnsFullReplacement()
    {
        // arrange
        string replacement = new('x', 10_000);
        StormXmlValue value = CreateStormXmlValue("start ##a## end", ("a", replacement));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be($"start {replacement} end");
    }

    [TestMethod]
    public void GetString_ManyLongReplacements_ReturnsAllReplacements()
    {
        // arrange
        const string replacement = "a fairly long replacement value for the token";

        string raw = string.Concat(Enumerable.Repeat("##a##", 200));
        string expected = string.Concat(Enumerable.Repeat(replacement, 200));

        StormXmlValue value = CreateStormXmlValue(raw, ("a", replacement));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void GetString_CalledAfterLongerValue_DoesNotLeakPooledData()
    {
        // arrange
        StormXmlValue longValue = CreateStormXmlValue("##a##", ("a", new string('z', 500)));
        StormXmlValue shortValue = CreateStormXmlValue("##a##", ("a", "ab"));

        // act
        string longResult = longValue.GetString();
        string shortResult = shortValue.GetString();

        // assert
        longResult.Should().Be(new string('z', 500));
        shortResult.Should().Be("ab");
    }

    [TestMethod]
    public void GetString_ReplacementContainsToken_DoesNotReplaceRecursively()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a##", ("a", "##b##"), ("b", "B"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("##b##");
    }

    [TestMethod]
    [DataRow("abc ## def")]
    [DataRow("##")]
    [DataRow("text ##unclosed")]
    public void GetString_UnclosedToken_ReturnsRawValue(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw, ("unclosed", "x"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be(raw);
    }

    [TestMethod]
    public void ToString_WithToken_ReturnsSameAsGetString()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("Deals ##damage## damage", ("damage", "100"));

        // act
        string result = value.ToString();

        // assert
        result.Should().Be("Deals 100 damage");
        result.Should().Be(value.GetString());
    }

    [TestMethod]
    public void GetString_ValidTokenFollowedByUnclosedToken_ReplacesOnlyValidToken()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a## then ## unclosed", ("a", "A"));

        // act
        string result = value.GetString();

        // assert
        result.Should().Be("A then ## unclosed");
    }

    [TestMethod]
    [DataRow("0", 0d)]
    [DataRow("1", 1d)]
    [DataRow("1.5", 1.5d)]
    [DataRow("-2.25", -2.25d)]
    [DataRow("+3.5", 3.5d)]
    [DataRow(".5", 0.5d)]
    [DataRow("1e3", 1000d)]
    [DataRow("1.5E2", 150d)]
    [DataRow("  3.5  ", 3.5d)]
    [DataRow("1000000", 1_000_000d)]
    public void GetDouble_ValidValue_ReturnsParsedValue(string raw, double expected)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        double result = value.GetDouble();

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("1,5")]
    [DataRow("1.2.3")]
    [DataRow("$5")]
    public void GetDouble_InvalidValue_ThrowsHeroesStormDataException(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        Func<double> act = value.GetDouble;

        // assert
        act.Should().Throw<HeroesStormDataException>()
            .Where(ex => ex.Message.Contains(raw))
            .WithInnerException<FormatException>();
    }

    [TestMethod]
    public void GetDouble_DefaultStruct_ThrowsHeroesStormDataException()
    {
        // arrange
        StormXmlValue value = default;

        // act
        Func<double> act = value.GetDouble;

        // assert
        act.Should().Throw<HeroesStormDataException>();
    }

    [TestMethod]
    public void GetDouble_NonInvariantCurrentCulture_ParsesWithInvariantCulture()
    {
        // arrange
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        StormXmlValue value = CreateStormXmlValue("1.5");

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // act
            double result = value.GetDouble();

            // assert
            result.Should().Be(1.5d);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    [DataRow("0", 0d)]
    [DataRow("1.5", 1.5d)]
    [DataRow("-2.25", -2.25d)]
    [DataRow("1e3", 1000d)]
    [DataRow("  3.5  ", 3.5d)]
    public void TryGetDouble_ValidValue_ReturnsTrueAndValue(string raw, double expected)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        bool success = value.TryGetDouble(out double result);

        // assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("1,5")]
    [DataRow("1.2.3")]
    public void TryGetDouble_InvalidValue_ReturnsFalseAndZero(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        bool success = value.TryGetDouble(out double result);

        // assert
        success.Should().BeFalse();
        result.Should().Be(0d);
    }

    [TestMethod]
    public void TryGetDouble_DefaultStruct_ReturnsFalseAndZero()
    {
        // arrange
        StormXmlValue value = default;

        // act
        bool success = value.TryGetDouble(out double result);

        // assert
        success.Should().BeFalse();
        result.Should().Be(0d);
    }

    [TestMethod]
    [DataRow("0", 0)]
    [DataRow("42", 42)]
    [DataRow("-7", -7)]
    [DataRow("+3", 3)]
    [DataRow("  12  ", 12)]
    [DataRow("2147483647", int.MaxValue)]
    [DataRow("-2147483648", int.MinValue)]
    public void GetInt32_ValidValue_ReturnsParsedValue(string raw, int expected)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        int result = value.GetInt32();

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("1.5")]
    [DataRow("1,000")]
    [DataRow("0x10")]
    public void GetInt32_InvalidValue_ThrowsHeroesStormDataException(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        Func<int> act = value.GetInt32;

        // assert
        act.Should().Throw<HeroesStormDataException>()
            .Where(ex => ex.Message.Contains(raw))
            .WithInnerException<FormatException>();
    }

    [TestMethod]
    public void GetInt32_DefaultStruct_ThrowsHeroesStormDataException()
    {
        // arrange
        StormXmlValue value = default;

        // act
        Func<int> act = () => value.GetInt32();

        // assert
        act.Should().Throw<HeroesStormDataException>();
    }

    [TestMethod]
    [DataRow("0", 0)]
    [DataRow("42", 42)]
    [DataRow("-7", -7)]
    [DataRow("  12  ", 12)]
    [DataRow("2147483647", int.MaxValue)]
    public void TryGetInt32_ValidValue_ReturnsTrueAndValue(string raw, int expected)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        bool success = value.TryGetInt32(out int result);

        // assert
        success.Should().BeTrue();
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("1.5")]
    [DataRow("1,000")]
    [DataRow("2147483648")]
    public void TryGetInt32_InvalidValue_ReturnsFalseAndZero(string raw)
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue(raw);

        // act
        bool success = value.TryGetInt32(out int result);

        // assert
        success.Should().BeFalse();
        result.Should().Be(0);
    }

    [TestMethod]
    public void TryGetInt32_DefaultStruct_ReturnsFalseAndZero()
    {
        // arrange
        StormXmlValue value = default;

        // act
        bool success = value.TryGetInt32(out int result);

        // assert
        success.Should().BeFalse();
        result.Should().Be(0);
    }

    [TestMethod]
    public void Equals_ObjectSameRawValue_ReturnsTrue()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        object right = CreateStormXmlValue("abc");

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_ObjectDifferentRawValue_ReturnsFalse()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        object right = CreateStormXmlValue("abd");

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_ObjectDifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        object right = CreateStormXmlValue("ABC");

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_ObjectDifferentRootMembers_ComparesRawValuesOnly()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("##a##", ("a", "A"));
        object right = CreateStormXmlValue("##a##", ("a", "different"));

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_ObjectBothDefault_ReturnsTrue()
    {
        // arrange
        StormXmlValue left = default;
        object right = default(StormXmlValue);

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_ObjectDefaultAndEmptyString_ReturnsFalse()
    {
        // arrange
        StormXmlValue left = default;
        object right = CreateStormXmlValue(string.Empty);

        // act
        bool result = left.Equals(right);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_StringMatchingRawValue_ReturnsTrue()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("abc");

        // act
        bool result = value.Equals("abc");

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_StringDifferentCase_ReturnsFalse()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("abc");

        // act
        bool result = value.Equals("ABC");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_StringMatchingRawValueWithToken_ComparesRawValue()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("##a##", ("a", "A"));

        // act
        bool rawResult = value.Equals("##a##");
        bool replacedResult = value.Equals("A");

        // assert
        rawResult.Should().BeTrue();
        replacedResult.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_StringEmptyAndDefaultStruct_ReturnsTrue()
    {
        // arrange
        StormXmlValue value = default;

        // act
        bool result = value.Equals(string.Empty);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_Null_ReturnsFalse()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("abc");

        // act
        bool result = value.Equals(null);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_UnrelatedType_ReturnsFalse()
    {
        // arrange
        StormXmlValue value = CreateStormXmlValue("1");

        // act
        bool result = value.Equals(1);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void EqualityOperator_SameRawValue_ReturnsTrue()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        StormXmlValue right = CreateStormXmlValue("abc");

        // act
        bool equal = left == right;
        bool notEqual = left != right;

        // assert
        equal.Should().BeTrue();
        notEqual.Should().BeFalse();
    }

    [TestMethod]
    public void EqualityOperator_DifferentRawValue_ReturnsFalse()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        StormXmlValue right = CreateStormXmlValue("xyz");

        // act
        bool equal = left == right;
        bool notEqual = left != right;

        // assert
        equal.Should().BeFalse();
        notEqual.Should().BeTrue();
    }

    [TestMethod]
    public void EqualityOperator_BothDefault_ReturnsTrue()
    {
        // arrange
        StormXmlValue left = default;
        StormXmlValue right = default;

        // act
        bool equal = left == right;
        bool notEqual = left != right;

        // assert
        equal.Should().BeTrue();
        notEqual.Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_SameRawValue_ReturnsSameHashCode()
    {
        // arrange
        StormXmlValue left = CreateStormXmlValue("abc");
        StormXmlValue right = CreateStormXmlValue("abc", ("x", "y"));

        // act
        int leftHashCode = left.GetHashCode();
        int rightHashCode = right.GetHashCode();

        // assert
        leftHashCode.Should().Be(rightHashCode);
    }

    [TestMethod]
    public void GetHashCode_DefaultStruct_ReturnsSameHashCodeAsEmptyString()
    {
        // arrange
        StormXmlValue defaultValue = default;
        StormXmlValue emptyValue = CreateStormXmlValue(string.Empty);

        // act
        int defaultHashCode = defaultValue.GetHashCode();
        int emptyHashCode = emptyValue.GetHashCode();

        // assert
        defaultHashCode.Should().Be(emptyHashCode);
    }

    // builds a root member that holds the token replacement values (##name## => value)
    private static StormXmlMember CreateRoot(params (string Name, string Value)[] tokens)
    {
        StormXmlMember root = new("Root");

        foreach ((string name, string value) in tokens)
            root.Add(name, value, root);

        return root;
    }

    private static StormXmlValue CreateStormXmlValue(string rawValue, params (string Name, string Value)[] tokens)
    {
        return new StormXmlValue(rawValue, CreateRoot(tokens));
    }
}
