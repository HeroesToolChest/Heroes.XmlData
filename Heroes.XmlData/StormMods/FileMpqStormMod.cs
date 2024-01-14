namespace Heroes.XmlData.StormMods;

internal class FileMpqStormMod : MpqStormMod<FileHeroesSource>
{
    public FileMpqStormMod(FileHeroesSource heroesSource, string directoryPath)
        : base(heroesSource, directoryPath)
    {
    }

    public FileMpqStormMod(FileHeroesSource heroesSource, string name, string directoryPath)
        : base(heroesSource, name, directoryPath)
    {
    }

    protected override Stream GetMpqFile(string file) => File.OpenRead(file);

    protected override IStormMod GetStormMod(string path) => HeroesSource.CreateStormModInstance<FileStormMod>(HeroesSource, path);
}
