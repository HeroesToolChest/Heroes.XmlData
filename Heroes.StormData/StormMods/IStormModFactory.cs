namespace Heroes.StormData.StormMods;

internal interface IStormModFactory
{
    IStormMod CreateFileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    IStormMod CreateCASCStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    //IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType);

    IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    //IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType);

    IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType);
}
