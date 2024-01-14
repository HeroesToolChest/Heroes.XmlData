namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource
{
    public FileHeroesSource(IStormStorage stormStorage, string modsDirectoryPath)
        : base(stormStorage, modsDirectoryPath)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath) => CreateStormModInstance<FileStormMod>(this, directoryPath);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath) => CreateStormModInstance<FileMpqStormMod>(this, name, directoryPath);

    protected override IDepotCache GetDepotCache() => new FileDepotCache(this);
}
