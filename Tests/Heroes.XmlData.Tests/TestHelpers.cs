namespace Heroes.XmlData.Tests;

internal static class TestHelpers
{
    public static StormPath GetStormPath(string path) => new()
    {
        StormModName = "test",
        StormModDirectoryPath = Path.Combine("test", "test"),
        Path = path,
        PathType = StormPathType.Hxd,
    };
}
