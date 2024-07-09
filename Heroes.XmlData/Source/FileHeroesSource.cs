namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource, IFileHeroesSource
{
    public FileHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IBackgroundWorkerEx? backgroundWorkerEx)
        : base(stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath, backgroundWorkerEx)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType, BackgroundWorkerEx? backgroundWorkerEx = null) => StormModFactory.CreateFileStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);
}
