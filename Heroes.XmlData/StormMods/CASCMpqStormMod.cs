namespace Heroes.XmlData.StormMods;

internal class CASCMpqStormMod : MpqStormMod<CASCHeroesSource>
{
    public CASCMpqStormMod(CASCHeroesSource heroesSource, string directoryPath, bool isMapMod)
        : base(heroesSource, directoryPath, isMapMod)
    {
    }

    public CASCMpqStormMod(CASCHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
        : base(heroesSource, name, directoryPath, isMapMod)
    {
    }

    protected override Stream GetMpqFile(string file) => HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file);

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.CreateStormModInstance<CASCStormMod>(HeroesSource, path, isMapMod);
}
