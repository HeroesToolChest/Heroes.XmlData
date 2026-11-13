using System.Text;

namespace Heroes.StormData.Text.Tests;

[TestClass]
public class KeyValueTextReaderTests
{
    [TestMethod]
    public void TryRead_SingleLine_ReturnsKeyAndValue()
    {
        // arrange
        using MemoryStream stream = ToStream("Assets/Texture=hero_portrait.dds");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("Assets/Texture", "hero_portrait.dds"));
    }

    [TestMethod]
    [DataRow("a=1\nb=2\nc=3")]
    [DataRow("a=1\r\nb=2\r\nc=3")]
    [DataRow("a=1\nb=2\r\nc=3")]
    [DataRow("a=1\nb=2\nc=3\n")]
    [DataRow("a=1\r\nb=2\r\nc=3\r\n")]
    public void TryRead_MultipleLines_ReturnsEachLine(string text)
    {
        // arrange
        using MemoryStream stream = ToStream(text);

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("a", "1"), ("b", "2"), ("c", "3"));
    }

    [TestMethod]
    public void TryRead_NoDelimiter_ReturnsKeyWithEmptyValue()
    {
        // arrange
        using MemoryStream stream = ToStream("idvalue");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("idvalue", string.Empty));
    }

    [TestMethod]
    public void TryRead_EmptyValue_ReturnsKeyWithEmptyValue()
    {
        // arrange
        using MemoryStream stream = ToStream("id=");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("id", string.Empty));
    }

    [TestMethod]
    public void TryRead_ValueHasDelimiter_SplitsOnFirstDelimiterOnly()
    {
        // arrange
        using MemoryStream stream = ToStream("Abil/Name/Fireball=Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,Damage,Amount\"/></c> damage");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("Abil/Name/Fireball", "Deals <c val=\"#TooltipNumbers\"><d ref=\"Effect,Damage,Amount\"/></c> damage"));
    }

    [TestMethod]
    [DataRow("=value")]
    [DataRow(" =value")]
    [DataRow("\t=value")]
    [DataRow("   ")]
    [DataRow("")]
    [DataRow("\r")]
    public void TryRead_EmptyOrWhiteSpaceKey_IsSkipped(string line)
    {
        // arrange
        using MemoryStream stream = ToStream($"a=1\n{line}\nb=2");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("a", "1"), ("b", "2"));
    }

    [TestMethod]
    public void TryRead_KeyAndValueHaveSurroundingWhiteSpace_AreNotTrimmed()
    {
        // arrange
        using MemoryStream stream = ToStream(" id = value ");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal((" id ", " value "));
    }

    [TestMethod]
    public void TryRead_HasBom_BomIsStripped()
    {
        // arrange
        using MemoryStream stream = ToStream("id=value", includeBom: true);

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("id", "value"));
    }

    [TestMethod]
    public void TryRead_BomBytesAfterFirstLine_AreNotStripped()
    {
        // arrange
        using MemoryStream stream = ToStream("a=1\n﻿b=2");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("a", "1"), ("﻿b", "2"));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void TryRead_EmptyStream_ReturnsFalse(bool includeBom)
    {
        // arrange
        using MemoryStream stream = ToStream(string.Empty, includeBom);
        using KeyValueTextReader reader = new(stream);

        // act
        bool result = reader.TryRead(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value);

        // assert
        result.Should().BeFalse();
        key.IsEmpty.Should().BeTrue();
        value.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void TryRead_OnlyBlankLines_ReturnsFalse()
    {
        // arrange
        using MemoryStream stream = ToStream("\n\r\n  \n=\n");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public void TryRead_AfterEndOfStream_KeepsReturningFalse()
    {
        // arrange
        using MemoryStream stream = ToStream("id=value");
        using KeyValueTextReader reader = new(stream);

        reader.TryRead(out _, out _);

        // act
        bool firstResult = reader.TryRead(out _, out _);
        bool secondResult = reader.TryRead(out _, out _);

        // assert
        firstResult.Should().BeFalse();
        secondResult.Should().BeFalse();
    }

    [TestMethod]
    public void TryRead_MultiByteUtf8_DecodesKeyAndValue()
    {
        // arrange
        using MemoryStream stream = ToStream("Unit/Name/Hero=영웅 Héros 英雄 🔥\nキー=値");

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("Unit/Name/Hero", "영웅 Héros 英雄 🔥"), ("キー", "値"));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(16)]
    [DataRow(32)]
    [DataRow(100)]
    public void TryRead_LinesCrossBufferBoundaries_ReturnsEveryLine(int bufferSize)
    {
        // arrange
        (string Text, List<(string Key, string Value)> Expected) data = CreateLines(200);
        using MemoryStream stream = ToStream(data.Text, includeBom: true);

        // act
        List<(string Key, string Value)> results = ReadAll(stream, bufferSize);

        // assert
        results.Should().Equal(data.Expected);
    }

    [TestMethod]
    public void TryRead_LineLongerThanBuffer_GrowsBufferAndReturnsLine()
    {
        // arrange
        string longValue = new('x', 5000);
        using MemoryStream stream = ToStream($"a=1\nlong={longValue}\nb=2");

        // act
        List<(string Key, string Value)> results = ReadAll(stream, bufferSize: 16);

        // assert
        results.Should().Equal(("a", "1"), ("long", longValue), ("b", "2"));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(7)]
    public void TryRead_StreamReturnsPartialReads_ReturnsEveryLine(int maxBytesPerRead)
    {
        // arrange
        (string Text, List<(string Key, string Value)> Expected) data = CreateLines(50);
        using PartialReadStream stream = new(Encoding.UTF8.GetBytes(data.Text), maxBytesPerRead);

        // act
        List<(string Key, string Value)> results = ReadAll(stream, bufferSize: 16);

        // assert
        results.Should().Equal(data.Expected);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public void TryRead_BomSplitAcrossPartialReads_BomIsStripped(int maxBytesPerRead)
    {
        // arrange
        byte[] bytes = [.. Encoding.UTF8.Preamble, .. Encoding.UTF8.GetBytes("id=value")];
        using PartialReadStream stream = new(bytes, maxBytesPerRead);

        // act
        List<(string Key, string Value)> results = ReadAll(stream);

        // assert
        results.Should().Equal(("id", "value"));
    }

    private static List<(string Key, string Value)> ReadAll(Stream stream, int bufferSize = 64 * 1024)
    {
        using KeyValueTextReader reader = new(stream, bufferSize);

        List<(string Key, string Value)> results = [];

        while (reader.TryRead(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value))
        {
            results.Add((Encoding.UTF8.GetString(key), Encoding.UTF8.GetString(value)));
        }

        return results;
    }

    private static MemoryStream ToStream(string text, bool includeBom = false)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        return includeBom
            ? new MemoryStream([.. Encoding.UTF8.Preamble, .. bytes])
            : new MemoryStream(bytes);
    }

    // lines of varying lengths with a mix of line endings, so the line breaks land at many different buffer offsets
    private static (string Text, List<(string Key, string Value)> Expected) CreateLines(int count)
    {
        StringBuilder sb = new();
        List<(string Key, string Value)> expected = [];

        for (int i = 0; i < count; i++)
        {
            string key = $"Key/{i}";
            string value = $"Value {new string((char)('a' + (i % 26)), i % 37)} é{i}";

            sb.Append(key).Append('=').Append(value).Append(i % 3 == 0 ? "\r\n" : "\n");
            expected.Add((key, value));
        }

        return (sb.ToString(), expected);
    }

    // returns at most a set number of bytes per read, like a network or decompression stream can
    private sealed class PartialReadStream(byte[] data, int maxBytesPerRead) : MemoryStream(data)
    {
        public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, maxBytesPerRead));

        public override int Read(Span<byte> buffer) => base.Read(buffer[..Math.Min(buffer.Length, maxBytesPerRead)]);
    }
}
