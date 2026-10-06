namespace Heroes.StormData.StormMods.Mpq;

internal sealed class FileMpqStormMod : MpqStormModBase<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileMpqStormMod(IFileHeroesSource heroesSource, string path, StormModType stormModType)
        : this(new FileSystem(), heroesSource, path, stormModType)
    {
    }

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string path, StormModType stormModType)
        : base(heroesSource, path, stormModType)
    {
        _fileSystem = fileSystem;
    }
}
