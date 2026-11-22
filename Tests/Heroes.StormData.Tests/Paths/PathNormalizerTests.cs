namespace Heroes.StormData.Paths.Tests;

[TestClass]
public sealed class PathNormalizerTests
{
    private static readonly char _directorySeperatorChar = Path.DirectorySeparatorChar;

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void NormalizePath_EmptyOrWhiteSpace_ReturnsEmpty(string path)
    {
        // act
        string result = PathNormalizer.NormalizePath(path);

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void NormalizePath_NullString_ReturnsEmpty()
    {
        // arrange
        string? path = null;

        // act
        string result = PathNormalizer.NormalizePath(path);

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("This\\IS\\path")]
    [DataRow("This/IS/path")]
    [DataRow("This/IS\\path")]
    public void NormalizePath_MixedCaseAndSeparators_LowercaseWithOSSeparator(string path)
    {
        // act
        string result = PathNormalizer.NormalizePath(path);

        // assert
        result.Should().Be($"this{_directorySeperatorChar}is{_directorySeperatorChar}path");
    }

    [TestMethod]
    public void NormalizePath_VeryLongPath_DoesNotOverflowStack()
    {
        // arrange
        string path = new('A', 10_000_000);

        // act
        string result = PathNormalizer.NormalizePath(path);

        // assert
        result.Should().HaveLength(path.Length).And.StartWith("aaaa");
    }

    [TestMethod]
    [DataRow("", "mods")]
    [DataRow("   ", "mods")]
    public void NormalizePathModsDirectory_EmptyOrWhiteSpace_ReturnsEmpty(string path, string modsDirectory)
    {
        // act
        string result = PathNormalizer.NormalizePath(path, modsDirectory);

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("mods\\HeroesData.StormMod\\Base.StormData", "mods")]
    [DataRow("Mods/HeroesData.StormMod/Base.StormData", "mods")]
    [DataRow("mods/HeroesData.StormMod/Base.StormData", "mods\\")]
    [DataRow("mods\\HeroesData.StormMod\\Base.StormData", "MODS/")]
    [DataRow("mods//HeroesData.StormMod\\Base.StormData", "mods")]
    public void NormalizePathModsDirectory_StartsWithModsDirectory_ModsDirectoryAndSeparatorRemoved(string path, string modsDirectory)
    {
        // act
        string result = PathNormalizer.NormalizePath(path, modsDirectory);

        // assert
        result.Should().Be($"heroesdata.stormmod{_directorySeperatorChar}base.stormdata");
    }

    [TestMethod]
    [DataRow("C:\\Games\\Heroes\\mods\\HeroesData.StormMod\\Base.StormData", "C:/Games/Heroes/mods")]
    [DataRow("C:/Games/Heroes/mods/HeroesData.StormMod/Base.StormData", "C:\\Games\\Heroes\\mods\\")]
    public void NormalizePathModsDirectory_FullModsDirectoryPathWithDifferentSeparators_ModsDirectoryRemoved(string path, string modsDirectory)
    {
        // act
        string result = PathNormalizer.NormalizePath(path, modsDirectory);

        // assert
        result.Should().Be($"heroesdata.stormmod{_directorySeperatorChar}base.stormdata");
    }

    [TestMethod]
    [DataRow("GameData/GameModsData.xml", "gamedata/gamemodsdata.xml")]
    [DataRow("GameData/Mods/AbilData.xml", "gamedata/mods/abildata.xml")]
    [DataRow("ModsData/AbilData.xml", "modsdata/abildata.xml")]
    [DataRow("GameData/AbilData.xml", "gamedata/abildata.xml")]
    public void NormalizePathModsDirectory_ModsDirectoryNotAtStartOrNotWholeSegment_PathOnlyNormalized(string path, string expected)
    {
        // act
        string result = PathNormalizer.NormalizePath(path, "mods");

        // assert
        result.Should().Be(expected.Replace('/', _directorySeperatorChar));
    }

    [TestMethod]
    [DataRow("mods")]
    [DataRow("mods/")]
    public void NormalizePathModsDirectory_PathIsModsDirectory_ReturnsEmpty(string path)
    {
        // act
        string result = PathNormalizer.NormalizePath(path, "mods");

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    public void NormalizePathModsDirectory_EmptyModsDirectory_PathOnlyNormalized(string modsDirectory)
    {
        // act
        string result = PathNormalizer.NormalizePath("Mods/AbilData.xml", modsDirectory);

        // assert
        result.Should().Be($"mods{_directorySeperatorChar}abildata.xml");
    }

    [TestMethod]
    [DataRow("This\\IS\\path")]
    [DataRow("This/IS/path")]
    [DataRow("This/IS\\path")]
    public void NormalizePathSpan_MixedCaseAndSeparators_ModifiedInPlaceToLowercaseWithOSSeparator(string path)
    {
        // arrange
        Span<char> buffer = path.ToCharArray();

        // act
        PathNormalizer.NormalizePath(buffer);

        // assert
        buffer.ToString().Should().Be($"this{_directorySeperatorChar}is{_directorySeperatorChar}path");
    }

    [TestMethod]
    public void NormalizePathSpan_EmptySpan_DoesNotThrow()
    {
        // act
        Action act = () => PathNormalizer.NormalizePath(Span<char>.Empty);

        // assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void NormalizePathSpan_WhiteSpace_IsLeftUnchanged()
    {
        // arrange - unlike the string overload, whitespace is not turned into an empty path
        Span<char> buffer = "   ".ToCharArray();

        // act
        PathNormalizer.NormalizePath(buffer);

        // assert
        buffer.ToString().Should().Be("   ");
    }

    [TestMethod]
    public void NormalizePathSpan_SliceOfLargerBuffer_OnlySliceIsModified()
    {
        // arrange
        char[] buffer = "KEEP|Assets/Textures/Icon.DDS|KEEP".ToCharArray();
        Span<char> slice = buffer.AsSpan(5, "Assets/Textures/Icon.DDS".Length);

        // act
        PathNormalizer.NormalizePath(slice);

        // assert
        new string(buffer).Should().Be($"KEEP|assets{_directorySeperatorChar}textures{_directorySeperatorChar}icon.dds|KEEP");
    }

    [TestMethod]
    [DataRow("GameData/Mods/AbilData.xml")]
    [DataRow("UI\\Layout\\Hero\\HeroTalents.StormLayout")]
    [DataRow("assets/textures\\storm_ui_icon_abathur.DDS")]
    [DataRow("mods//HeroesData.StormMod\\Base.StormData")]
    public void NormalizePathSpan_NonWhiteSpacePath_SameResultAsStringOverload(string path)
    {
        // arrange
        Span<char> buffer = path.ToCharArray();

        // act
        PathNormalizer.NormalizePath(buffer);

        // assert
        buffer.ToString().Should().Be(PathNormalizer.NormalizePath(path.AsSpan()));
    }

    [TestMethod]
    public void NormalizePathSpan_NonAsciiCharacters_LowercasedInvariant()
    {
        // arrange
        Span<char> buffer = "Ü/É\\Ñ".ToCharArray(); // Ü/É\Ñ

        // act
        PathNormalizer.NormalizePath(buffer);

        // assert
        buffer.ToString().Should().Be($"ü{_directorySeperatorChar}é{_directorySeperatorChar}ñ"); // ü/é/ñ
    }

    [TestMethod]
    [DataRow("mods/core.stormmod/gamedata.xml", "mods", "core.stormmod/gamedata.xml")]
    [DataRow("MODS\\Core.StormMod", "mods", "Core.StormMod")]
    [DataRow("mods/core.stormmod", "Mods\\", "core.stormmod")]
    [DataRow("mods/core.stormmod", "mods//", "core.stormmod")]
    [DataRow("hots_data/core.stormmod", "hots_data", "core.stormmod")]
    [DataRow("C:\\Games\\Heroes\\mods\\core.stormmod", "C:/Games/Heroes/mods", "core.stormmod")]
    [DataRow("C:/Games/Heroes/mods/core.stormmod", "c:\\games\\heroes\\mods\\", "core.stormmod")]
    public void GetDirectoryPrefixLength_StartsWithDirectory_ReturnsLengthOfDirectoryAndSeparator(string filePath, string directory, string expectedRemainder)
    {
        // act
        int result = PathNormalizer.GetDirectoryPrefixLength(filePath, directory);

        // assert
        filePath[result..].Should().Be(expectedRemainder);
    }

    [TestMethod]
    [DataRow("mods//core.stormmod", "core.stormmod")]
    [DataRow("mods/\\/core.stormmod", "core.stormmod")]
    public void GetDirectoryPrefixLength_MultipleSeparatorsAfterDirectory_AllSeparatorsIncluded(string filePath, string expectedRemainder)
    {
        // act
        int result = PathNormalizer.GetDirectoryPrefixLength(filePath, "mods");

        // assert
        filePath[result..].Should().Be(expectedRemainder);
    }

    [TestMethod]
    [DataRow("mods", 4)]
    [DataRow("MODS", 4)]
    [DataRow("mods/", 5)]
    [DataRow("mods\\\\", 6)]
    public void GetDirectoryPrefixLength_PathIsDirectory_ReturnsWholePathLength(string filePath, int expected)
    {
        // act
        int result = PathNormalizer.GetDirectoryPrefixLength(filePath, "mods");

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("modsdata/abildata.xml", "mods")]
    [DataRow("mods.stormmod/abildata.xml", "mods")]
    [DataRow("gamedata/mods/abildata.xml", "mods")]
    [DataRow("mod", "mods")]
    [DataRow("", "mods")]
    [DataRow("C:\\Games\\Heroes\\modsdata\\core.stormmod", "C:\\Games\\Heroes\\mods")]
    [DataRow("D:\\Games\\Heroes\\mods\\core.stormmod", "C:\\Games\\Heroes\\mods")]
    public void GetDirectoryPrefixLength_DoesNotStartWithDirectoryAsWholeSegment_ReturnsZero(string filePath, string directory)
    {
        // act
        int result = PathNormalizer.GetDirectoryPrefixLength(filePath, directory);

        // assert
        result.Should().Be(0);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/")]
    [DataRow("\\\\")]
    public void GetDirectoryPrefixLength_EmptyDirectory_ReturnsZero(string directory)
    {
        // act
        int result = PathNormalizer.GetDirectoryPrefixLength("/mods/abildata.xml", directory);

        // assert
        result.Should().Be(0);
    }
}
