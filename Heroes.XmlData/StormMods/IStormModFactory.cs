namespace Heroes.XmlData.StormMods;

internal interface IStormModFactory
{
    IStormMod CreateFileStormModInstance(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType);

    IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    IStormMod CreateCASCStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType);

    IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType);

    IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType);
}
