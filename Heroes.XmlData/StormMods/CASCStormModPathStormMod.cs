namespace Heroes.XmlData.StormMods;

internal class CASCStormModPathStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath)
    : CASCStormMod(cascHeroesSource)
{
    protected override string? DirectoryPath => directoryPath;
}
