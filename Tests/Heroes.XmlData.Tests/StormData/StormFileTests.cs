namespace Heroes.XmlData.StormData.Tests;

[TestClass]
public class StormFileTests
{
    [TestMethod]
    public void ToString_GetString_ReturnsPath()
    {
        // arrange
        StormFile stormFile = new(new StormPath()
        {
            Path = "path",
            PathType = StormPathType.File,
            StormModName = "name",
            StormModPath = "modpath",
        });

        // act
        string result = stormFile.ToString();

        // assert
        result.Should().Be(stormFile.StormPath.Path);
    }

    [TestMethod]
    public void AddPath_AddsStormPaths_HasCount()
    {
        // arrange
        StormFile stormFile = new(new StormPath()
        {
            Path = "path",
            PathType = StormPathType.File,
            StormModName = "name",
            StormModPath = "modpath",
        });

        // act
        stormFile.AddPath(new StormPath()
        {
            Path = "path2",
            PathType = StormPathType.File,
            StormModName = "name2",
            StormModPath = "modpath2",
        });
        stormFile.AddPath(new StormPath()
        {
            Path = "path3",
            PathType = StormPathType.File,
            StormModName = "name3",
            StormModPath = "modpath3",
        });

        // assert
        stormFile.StormPaths.Should().HaveCount(3);
    }

    [TestMethod]
    public void StormPath_HasStormPaths_ReturnsLastPath()
    {
        // arrange
        StormFile stormFile = new(new()
        {
            Path = "path",
            PathType = StormPathType.File,
            StormModName = "name",
            StormModPath = "modpath",
        });

        StormPath stormPath2 = new()
        {
            Path = "path2",
            PathType = StormPathType.File,
            StormModName = "name2",
            StormModPath = "modpath2",
        };

        stormFile.AddPath(stormPath2);

        // act
        StormPath result = stormFile.StormPath;

        // assert
        result.Should().Be(stormPath2);
    }

    [TestMethod]
    public void StormPath_HasNoStormPaths_()
    {
        // arrange
        // act
        Action act = static () => new StormFile(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}