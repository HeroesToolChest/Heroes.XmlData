namespace Heroes.XmlData.StormMods;

internal class FileMpqStormMod : MpqStormMod<IFileHeroesSource>
{
    public FileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, bool isMapMod)
        : base(heroesSource, directoryPath, isMapMod)
    {
    }

    public FileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
        : base(heroesSource, name, directoryPath, isMapMod)
    {
    }

    protected override Stream GetMpqFile(string file) => File.OpenRead(file);

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, isMapMod);
}
