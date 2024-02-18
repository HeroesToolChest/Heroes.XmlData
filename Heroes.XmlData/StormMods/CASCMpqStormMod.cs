namespace Heroes.XmlData.StormMods;

internal class CASCMpqStormMod : MpqStormMod<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, bool isMapMod)
        : base(heroesSource, directoryPath, isMapMod)
    {
    }

    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
        : base(heroesSource, name, directoryPath, isMapMod)
    {
    }

    protected override Stream GetMpqFile(string file) => HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file);

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.StormModFactory.CreateCASCMpqStormModInstance(HeroesSource, path, isMapMod);
}
