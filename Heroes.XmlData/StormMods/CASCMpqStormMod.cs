namespace Heroes.XmlData.StormMods;

internal class CASCMpqStormMod : MpqStormMod<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
    }

    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType)
    {
    }

    protected override Stream GetMpqFile(string file) => HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormModInstance(HeroesSource, path, stormModType);
}
