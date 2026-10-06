namespace Heroes.StormData.Source;

internal sealed class FileHeroesSource : HeroesSourceBase, IFileHeroesSource
{
    public FileHeroesSource(string modsDirectoryPath, IStormModFactory stormModFactory)
        : base(modsDirectoryPath, stormModFactory)
    {
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateFileStormMod(this, directoryPath, stormModType);
}
