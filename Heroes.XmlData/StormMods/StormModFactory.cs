namespace Heroes.XmlData.StormMods;

internal class StormModFactory : IStormModFactory
{
    public IStormMod CreateFileStormModInstance(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileMpqStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    {
        return new FileMpqStormMod(heroesSource, name, directoryPath, stormModType);
    }

    public IStormMod CreateCASCStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCMpqStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    {
        return new CASCMpqStormMod(heroesSource, name, directoryPath, stormModType);
    }
}
