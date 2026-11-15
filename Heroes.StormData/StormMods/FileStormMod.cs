namespace Heroes.StormData.StormMods;

internal sealed class FileStormMod : StormModBase<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType)
    {
        _fileSystem = fileSystem;
    }

    protected override void LoadGameDataDirectory()
    {
        if (!_fileSystem.Directory.Exists(GameDataDirectoryPath))
            return;

        IEnumerable<string> files = _fileSystem.Directory.EnumerateFiles(GameDataDirectoryPath, $"*{XmlFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadGameDataFiles(files);
    }

    protected override void LoadStormLayoutDirectory()
    {
        if (!_fileSystem.Directory.Exists(LayoutDirectoryPath))
            return;

        IEnumerable<string> files = _fileSystem.Directory.EnumerateFiles(LayoutDirectoryPath, $"*{StormLayoutFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadStormLayoutFiles(files);
    }

    protected override void LoadAssetsDirectory()
    {
        if (!_fileSystem.Directory.Exists(AssetsDirectoryPath))
            return;

        IEnumerable<string> files = _fileSystem.Directory.EnumerateFiles(AssetsDirectoryPath, $"*{DDSFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadAssetFiles(files);
    }

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        // open the file directly instead of checking if it exists first, which saves a file system call for every file.
        try
        {
            stream = _fileSystem.File.OpenRead(path);
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
