namespace Heroes.StormData.Source;

internal sealed class FileHeroesSource : HeroesSourceBase, IFileHeroesSource
{
    private readonly IFileSystem _fileSystem;

    public FileHeroesSource(string modsDirectoryPath, IStormModFactory stormModFactory)
        : this(new FileSystem(), modsDirectoryPath, stormModFactory)
    {
    }

    public FileHeroesSource(IFileSystem fileSystem, string modsDirectoryPath, IStormModFactory stormModFactory)
        : base(modsDirectoryPath, stormModFactory)
    {
        _fileSystem = fileSystem;
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateFileStormMod(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormMod(this, name, directoryPath, stormModType);

    protected override IDepotCache CreateDepotCache() => new FileDepotCache(this);

    protected override string GetFullPath(string path)
    {
        // an absolute path outside the mods directory is used as is
        if (_fileSystem.Path.IsPathFullyQualified(path) && PathNormalizer.GetDirectoryPrefixLength(path, ModsDirectoryPath) == 0)
            return path;

        return base.GetFullPath(path);
    }

    protected override bool FileExistsCore(string fullPath) => _fileSystem.File.Exists(fullPath);

    protected override bool TryOpenFile(string fullPath, [NotNullWhen(true)] out Stream? stream)
    {
        // a directory is not a file, opening it would throw an UnauthorizedAccessException, the same as a file that cannot be read
        if (_fileSystem.Directory.Exists(fullPath))
        {
            stream = null;

            return false;
        }

        // open the file directly instead of checking if it exists first
        try
        {
            stream = _fileSystem.File.OpenRead(fullPath);

            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            stream = null;

            return false;
        }
    }
}
