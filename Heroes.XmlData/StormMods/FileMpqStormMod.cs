namespace Heroes.XmlData.StormMods;

internal class FileMpqStormMod : MpqStormMod<FileHeroesSource>
{
    public FileMpqStormMod(FileHeroesSource heroesSource, string directoryPath, bool isMapMod)
        : base(heroesSource, directoryPath, isMapMod)
    {
    }

    public FileMpqStormMod(FileHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
        : base(heroesSource, name, directoryPath, isMapMod)
    {
    }

    protected override Stream GetMpqFile(string file) => File.OpenRead(file);

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.CreateStormModInstance<FileStormMod>(HeroesSource, path, isMapMod);
}
