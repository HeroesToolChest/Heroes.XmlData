namespace Heroes.StormData.Source;

internal sealed class FileHeroesSource : HeroesSourceBase, IFileHeroesSource
{
    public FileHeroesSource(string modsDirectoryPath, IStormModFactory stormModFactory)
        : base(modsDirectoryPath, stormModFactory)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateFileStormMod(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormMod(this, name, directoryPath, stormModType);

    protected override IDepotCache CreateDepotCache() => new FileDepotCache(this);
}
