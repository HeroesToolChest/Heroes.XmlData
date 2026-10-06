namespace Heroes.StormData.StormMods;

internal sealed class FileStormMod : StormModBase<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
        _fileSystem = new FileSystem();
    }

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
        _fileSystem = fileSystem;
    }

    public override void LoadGameDataDirectory()
    {
        if (!_fileSystem.Directory.Exists(GameDataDirectoryPath))
        {
            // TODO: log
            return;
        }

        IEnumerable<string> files = _fileSystem.Directory.EnumerateFiles(GameDataDirectoryPath, $"*{XmlFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadGameDataFiles(files);
    }

    protected override bool IsFileExists(string filePath) => _fileSystem.File.Exists(filePath);

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!IsFileExists(path))
            return false;

        stream = _fileSystem.File.OpenRead(path);
        return true;
    }
}
