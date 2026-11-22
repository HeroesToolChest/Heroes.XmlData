namespace Heroes.StormData.StormMods.Mpq;

internal sealed class CASCMpqStormMod : MpqStormModBase<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType, string? name = null)
        : base(heroesSource, directoryPath, stormModType, name)
    {
    }

    protected override bool TryOpenMpqFile([NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        ICASCHandlerWrapper cascHandlerWrapper = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper;

        // check first, otherwise OpenFile throws a FileNotFoundException for a missing file (CASCConfig.ThrowOnFileNotFound)
        if (!cascHandlerWrapper.FileExists(DirectoryPath))
            return false;

        stream = cascHandlerWrapper.OpenFile(DirectoryPath);

        return stream is not null;
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormMod(HeroesSource, path, stormModType);
}
