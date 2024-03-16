namespace Heroes.XmlData.StormMods;

internal class StormModFactory : IStormModFactory
{
    public IStormMod CreateFileStormModInstance(IFileHeroesSource heroesSource, string directoryPath, bool isMapMod)
    {
        return new FileStormMod(heroesSource, directoryPath, isMapMod);
    }

    public IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string directoryPath, bool isMapMod)
    {
        return new FileMpqStormMod(heroesSource, directoryPath, isMapMod);
    }

    public IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
    {
        return new FileMpqStormMod(heroesSource, name, directoryPath, isMapMod);
    }

    public IStormMod CreateCASCStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, bool isMapMod)
    {
        return new CASCStormMod(heroesSource, directoryPath, isMapMod);
    }

    public IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, bool isMapMod)
    {
        return new CASCStormMod(heroesSource, directoryPath, isMapMod);
    }

    public IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
    {
        return new CASCStormMod(heroesSource, name, directoryPath, isMapMod);
    }
}
