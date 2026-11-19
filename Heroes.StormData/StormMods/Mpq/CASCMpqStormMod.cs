namespace Heroes.StormData.StormMods.Mpq;

internal sealed class CASCMpqStormMod : MpqStormModBase<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
    }

    protected override bool TryOpenMpqFile([NotNullWhen(true)] out Stream? stream)
    {
        stream = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(DirectoryPath);

        return stream is not null;
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormMod(HeroesSource, path, stormModType);
}
