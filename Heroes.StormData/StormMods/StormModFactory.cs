namespace Heroes.StormData.StormMods;

internal sealed class StormModFactory : IStormModFactory
{
    public IStormMod CreateFileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateCASCStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileMpqStormMod(heroesSource, directoryPath, stormModType);
    }

    //public IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    //{
    //    return new FileMpqStormMod(heroesSource, name, directoryPath, stormModType);
    //}

    public IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCMpqStormMod(heroesSource, directoryPath, stormModType);
    }

    //public IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    //{
    //    return new CASCMpqStormMod(heroesSource, name, directoryPath, stormModType);
    //}
}
