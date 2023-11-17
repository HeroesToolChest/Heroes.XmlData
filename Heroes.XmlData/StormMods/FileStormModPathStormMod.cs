namespace Heroes.XmlData.StormMods;

internal class FileStormModPathStormMod(IHeroesSource heroesSource, string directoryPath)
    : FileStormMod(heroesSource)
{
    protected override string? DirectoryPath => directoryPath;
}
