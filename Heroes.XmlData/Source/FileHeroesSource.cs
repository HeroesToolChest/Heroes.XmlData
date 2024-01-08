namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource
{
    public FileHeroesSource(HeroesData heroesData, string modsDirectoryPath)
        : base(heroesData, modsDirectoryPath)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath) => CreateStormModInstance<FileStormMod>(this, directoryPath);

    protected override IStormMod GetMpqStormMod(string directoryPath, string name) => CreateStormModInstance<FileMpqStormMod>(this, directoryPath, name);

    protected override IDepotCache GetDepotCache() => new FileDepotCache(this);

    protected override bool CasingExists() => Directory.Exists(TestCasingDirectoryPath);
}
