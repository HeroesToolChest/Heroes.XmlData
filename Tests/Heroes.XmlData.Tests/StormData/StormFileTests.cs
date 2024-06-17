namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormFileTests
{
    [TestMethod]
    public void Equals_DifferentCasing_ShouldBeTrue()
    {
        // arrange
        StormPath stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
            PathType = StormPathType.Hxd,
        };

        StormPath stormFile2 = new()
        {
            StormModName = "ModName",
            StormModDirectoryPath = "DirectoryPath",
            Path = "SomePath",
            PathType = StormPathType.Hxd,
        };

        // act
        bool result = stormFile1.Equals(stormFile2);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentValues_ShouldBeFalse()
    {
        // arrange
        StormPath stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
            PathType = StormPathType.Hxd,
        };

        StormPath stormFile2 = new()
        {
            StormModName = "ModName2",
            StormModDirectoryPath = "DirectoryPath2",
            Path = "SomePath2",
            PathType = StormPathType.Hxd,
        };

        // act
        bool result = stormFile1.Equals(stormFile2);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_DifferentCasing_ShouldHaveSameHashCode()
    {
        // arrange
        StormPath stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
            PathType = StormPathType.Hxd,
        };

        StormPath stormFile2 = new()
        {
            StormModName = "ModName",
            StormModDirectoryPath = "DirectoryPath",
            Path = "SomePath",
            PathType = StormPathType.Hxd,
        };

        // act
        int hashCode1 = stormFile1.GetHashCode();
        int hashCode2 = stormFile2.GetHashCode();

        // assert
        hashCode1.Should().Be(hashCode2);
    }

    [TestMethod]
    public void GetHashCode_DifferentValues_ShouldHaveDifferentHashCode()
    {
        // arrange
        StormPath stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
            PathType = StormPathType.Hxd,
        };

        StormPath stormFile2 = new()
        {
            StormModName = "ModName2",
            StormModDirectoryPath = "DirectoryPath2",
            Path = "SomePath2",
            PathType = StormPathType.Hxd,
        };

        // act
        int hashCode1 = stormFile1.GetHashCode();
        int hashCode2 = stormFile2.GetHashCode();

        // assert
        hashCode1.Should().NotBe(hashCode2);
    }
}
