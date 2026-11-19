namespace Heroes.StormData.StormMods.Mpq;

internal sealed class FileMpqStormMod : MpqStormModBase<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
        _fileSystem = fileSystem;
    }

    protected override bool TryOpenMpqFile([NotNullWhen(true)] out Stream? stream)
    {
        // open the file directly instead of checking if it exists first
        try
        {
            stream = _fileSystem.File.OpenRead(DirectoryPath);

            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            stream = null;

            return false;
        }
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormMod(HeroesSource, path, stormModType);
}
