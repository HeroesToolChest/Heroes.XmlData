namespace Heroes.StormData.StormMods;

internal sealed class StormModFactory : IStormModFactory
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="StormModFactory"/> class that uses the real file system.
    /// </summary>
    public StormModFactory()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormModFactory"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system that is shared by every file based <c>stormmod</c> this factory creates.</param>
    public StormModFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IStormMod CreateFileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileStormMod(_fileSystem, heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateCASCStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new FileMpqStormMod(_fileSystem, heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateFileMpqStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    {
        return new FileMpqStormMod(_fileSystem, heroesSource, directoryPath, stormModType, name);
    }

    public IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string directoryPath, StormModType stormModType)
    {
        return new CASCMpqStormMod(heroesSource, directoryPath, stormModType);
    }

    public IStormMod CreateCASCMpqStormMod(ICASCHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
    {
        return new CASCMpqStormMod(heroesSource, directoryPath, stormModType, name);
    }
}
