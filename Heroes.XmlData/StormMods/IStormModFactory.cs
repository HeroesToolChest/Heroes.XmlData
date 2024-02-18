namespace Heroes.XmlData.StormMods;

internal interface IStormModFactory
{
    IStormMod CreateFileStormModInstance(IFileHeroesSource heroesSource, string directoryPath, bool isMapMod);

    IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string name, string directoryPath, bool isMapMod);

    IStormMod CreateFileMpqStormModInstance(IFileHeroesSource heroesSource, string directoryPath, bool isMapMod);

    IStormMod CreateCASCStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, bool isMapMod);

    IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string name, string directoryPath, bool isMapMod);

    IStormMod CreateCASCMpqStormModInstance(ICASCHeroesSource heroesSource, string directoryPath, bool isMapMod);
}
