using System.IO.Abstractions;

namespace Heroes.XmlData.StormMods;

internal sealed class FileMpqStormMod : MpqStormMod<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : this(new FileSystem(), heroesSource, directoryPath, stormModType)
    {
    }

    public FileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : this(new FileSystem(), heroesSource, name, directoryPath, stormModType)
    {
    }

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
        _fileSystem = fileSystem;
    }

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType)
    {
        _fileSystem = fileSystem;
    }

    protected override Stream GetMpqFile(string file) => _fileSystem.File.OpenRead(file);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
