namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource, IFileHeroesSource
{
    public FileHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath)
        : base(stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateFileStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);
}
