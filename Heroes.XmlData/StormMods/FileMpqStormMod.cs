using System.IO.Abstractions;

namespace Heroes.XmlData.StormMods;

internal class FileMpqStormMod : MpqStormMod<IFileHeroesSource>
{
    public FileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
    }

    public FileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType)
    {
    }

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(fileSystem, heroesSource, directoryPath, stormModType)
    {
    }

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(fileSystem, heroesSource, name, directoryPath, stormModType)
    {
    }

    protected override Stream GetMpqFile(string file) => FileSystem.File.OpenRead(file);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
