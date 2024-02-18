namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource, IFileHeroesSource
{
    public FileHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath)
        : base(stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, bool isMapMod) => StormModFactory.CreateFileStormModInstance(this, directoryPath, isMapMod);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, bool isMapMod) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, isMapMod);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);
}
