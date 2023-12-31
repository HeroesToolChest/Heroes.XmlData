namespace Heroes.XmlData.StormMods;

internal class CASCStormModPathStormMod : CASCStormMod
{
    private readonly string _directoryPath;

    public CASCStormModPathStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath)
        : base(cascHeroesSource)
    {
        _directoryPath = directoryPath;
    }

    protected override string? DirectoryPath => _directoryPath;
}
