namespace Heroes.XmlData.Tests;

internal static class TestHelpers
{
    public static StormPath GetStormPath(string path) => new()
    {
        StormModName = "test",
        Path = path,
        PathType = StormPathType.Hxd,
    };
}
