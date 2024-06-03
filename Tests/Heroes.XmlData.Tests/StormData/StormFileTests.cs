namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormFileTests
{
    [TestMethod]
    public void Equals_DifferentCasing_ShouldBeTrue()
    {
        // arrange
        StormFile stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = "ModName",
            StormModDirectoryPath = "DirectoryPath",
            Path = "SomePath",
        };

        // act
        bool result = stormFile1.Equals(stormFile2);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_HasNullProperties_ShouldBeTrue()
    {
        // arrange
        StormFile stormFile1 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "SomePath",
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
        StormFile stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = "ModName2",
            StormModDirectoryPath = "DirectoryPath2",
            Path = "SomePath2",
        };

        // act
        bool result = stormFile1.Equals(stormFile2);

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_OneHasNull_ShouldBeFalse()
    {
        // arrange
        StormFile stormFile1 = new()
        {
            StormModName = null,
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "SomePath",
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
        StormFile stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = "ModName",
            StormModDirectoryPath = "DirectoryPath",
            Path = "SomePath",
        };

        // act
        int hashCode1 = stormFile1.GetHashCode();
        int hashCode2 = stormFile2.GetHashCode();

        // assert
        hashCode1.Should().Be(hashCode2);
    }

    [TestMethod]
    public void GetHashCode_HasNullProperties_ShouldHaveSameHashCode()
    {
        // arrange
        StormFile stormFile1 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "SomePath",
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
        StormFile stormFile1 = new()
        {
            StormModName = "modname",
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = "ModName2",
            StormModDirectoryPath = "DirectoryPath2",
            Path = "SomePath2",
        };

        // act
        int hashCode1 = stormFile1.GetHashCode();
        int hashCode2 = stormFile2.GetHashCode();

        // assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [TestMethod]
    public void GetHashCode_OneHasNull_ShouldHaveDifferentHashCode()
    {
        // arrange
        StormFile stormFile1 = new()
        {
            StormModName = null,
            StormModDirectoryPath = "directorypath",
            Path = "somepath",
        };

        StormFile stormFile2 = new()
        {
            StormModName = null,
            StormModDirectoryPath = null,
            Path = "SomePath",
        };

        // act
        int hashCode1 = stormFile1.GetHashCode();
        int hashCode2 = stormFile2.GetHashCode();

        // assert
        hashCode1.Should().NotBe(hashCode2);
    }
}
