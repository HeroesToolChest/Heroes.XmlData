namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource
{
    public FileHeroesSource(IStormStorage stormStorage, string modsDirectoryPath)
        : base(stormStorage, modsDirectoryPath)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, bool isMapMod) => CreateStormModInstance<FileStormMod>(this, directoryPath, isMapMod);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, bool isMapMod) => CreateStormModInstance<FileMpqStormMod>(this, name, directoryPath, isMapMod);

    protected override IDepotCache GetDepotCache() => new FileDepotCache(this);
}
