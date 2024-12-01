using System.Runtime.InteropServices;
using System.Text;

namespace Heroes.XmlData.Helpers.Tests;

[TestClass]
public class PathHelperTests
{
    [TestMethod]
    public void NormalizePathSpan_EmptyInput_EmptyOutput()
    {
        // arrange
        Span<char> path = new(string.Empty.ToCharArray());

        // act
        PathHelper.NormalizePath(path);

        // assert
        path.ToString().Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathSpan_BackslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        Span<char> path = new("This\\IS\\path".ToCharArray());

        // act
        PathHelper.NormalizePath(path);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            path.ToString().Should().Be("this\\is\\path");
        else
            path.ToString().Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathSpan_ForwardslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        Span<char> path = new("This/IS/path".ToCharArray());

        // act
        PathHelper.NormalizePath(path);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            path.ToString().Should().Be("this\\is\\path");
        else
            path.ToString().Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpan_EmptyInput_EmptyOutput()
    {
        // arrange
        ReadOnlySpan<char> path = string.Empty;

        // act
        string result = PathHelper.NormalizePath(path);

        // assert
        result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpan_BackslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        ReadOnlySpan<char> path = "This\\IS\\path";

        // act
        string result = PathHelper.NormalizePath(path);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpan_ForwardslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        ReadOnlySpan<char> path = "This/IS/path";

        // act
        string result = PathHelper.NormalizePath(path);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByte_EmptyInput_EmptyOutput()
    {
        // arrange
        ReadOnlySpan<byte> path = [];

        // act
        string result = PathHelper.NormalizePath(path);

        // assert
        result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByte_BackslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        byte[] buffer = new byte[12];
        Encoding.UTF8.GetBytes("This\\IS\\path", buffer);

        // act
        string result = PathHelper.NormalizePath(buffer);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByte_ForwardslashPath_AllLowercaseWithOSDirectorySeparatorChar()
    {
        // arrange
        byte[] buffer = new byte[12];
        Encoding.UTF8.GetBytes("This/IS/path", buffer);

        // act
        string result = PathHelper.NormalizePath(buffer);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanModsDirectory_EmptyInput_EmptyOutput()
    {
        // arrange
        ReadOnlySpan<char> path = string.Empty;

        // act
        string result = PathHelper.NormalizePath(path, string.Empty);

        // assert
        result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanModsDirectory_WithModsPathInBeginning_ResultWithoutModsDictory()
    {
        // arrange
        ReadOnlySpan<char> path = "mods\\path\\This\\IS\\path";
        string modsPath = "mods\\path\\";

        // act
        string result = PathHelper.NormalizePath(path, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanModsDirectory_WithModsPathNotInBeginning_ResultIsEmpty()
    {
        // arrange
        ReadOnlySpan<char> path = "This\\IS\\path\\mods\\path";
        string modsPath = "mods\\path";

        // act
        string result = PathHelper.NormalizePath(path, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be(string.Empty);
        else
            result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanModsDirectory_NoModsPathInPath_ResultIsNormalize()
    {
        // arrange
        ReadOnlySpan<char> path = "This\\IS\\path";
        string modsPath = "mods\\path";

        // act
        string result = PathHelper.NormalizePath(path, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByteModsDirectory_EmptyInput_EmptyOutput()
    {
        // arrange
        ReadOnlySpan<byte> path = [];

        // act
        string result = PathHelper.NormalizePath(path, string.Empty);

        // assert
        result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByteModsDirectory_WithModsPathInBeginning_ResultWithoutModsDictory()
    {
        // arrange
        byte[] buffer = new byte[22];
        Encoding.UTF8.GetBytes("mods\\path\\This\\IS\\path", buffer);

        string modsPath = "mods\\path\\";

        // act
        string result = PathHelper.NormalizePath(buffer, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByteModsDirectory_WithModsPathNotInBeginning_ResultIsEmpty()
    {
        // arrange
        byte[] buffer = new byte[22];
        Encoding.UTF8.GetBytes("This\\IS\\path\\mods\\path", buffer);

        string modsPath = "mods\\path";

        // act
        string result = PathHelper.NormalizePath(buffer, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be(string.Empty);
        else
            result.Should().Be(string.Empty);
    }

    [TestMethod]
    public void NormalizePathReadOnlySpanByteModsDirectory_NoModsPathInPath_ResultIsNormalize()
    {
        // arrange
        byte[] buffer = new byte[12];
        Encoding.UTF8.GetBytes("This\\IS\\path", buffer);

        string modsPath = "mods\\path";

        // act
        string result = PathHelper.NormalizePath(buffer, modsPath);

        // assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result.Should().Be("this\\is\\path");
        else
            result.Should().Be("this/is/path");
    }
}
