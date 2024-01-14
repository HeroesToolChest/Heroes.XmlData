namespace Heroes.XmlData.StormMods;

internal class CASCMpqStormMod : MpqStormMod<CASCHeroesSource>
{
    public CASCMpqStormMod(CASCHeroesSource heroesSource, string directoryPath)
        : base(heroesSource, directoryPath)
    {
    }

    public CASCMpqStormMod(CASCHeroesSource heroesSource, string name, string directoryPath)
        : base(heroesSource, name, directoryPath)
    {
    }

    protected override Stream GetMpqFile(string file) => HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file);

    protected override IStormMod GetStormMod(string path) => HeroesSource.CreateStormModInstance<CASCStormMod>(HeroesSource, path);

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        throw new NotImplementedException();
    }
}
