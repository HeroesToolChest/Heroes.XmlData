namespace Heroes.StormData.Tests;

public static class StormFileHelper
{
    public static StormFile CreateStormFile()
    {
        return new StormFile()
        {
            StormModName = "TestMod",
            StormModPath = "TestModPath",
        };
    }
}
