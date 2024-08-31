using System.IO.Abstractions;

namespace Heroes.XmlData.StormMods;

internal class FileStormMod : StormMod<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType, StormPathType.File)
    {
        _fileSystem = new FileSystem();
    }

    public FileStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType, StormPathType.File)
    {
        _fileSystem = new FileSystem();
    }

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType, StormPathType.File)
    {
        _fileSystem = fileSystem;
    }

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType, StormPathType.File)
    {
        _fileSystem = fileSystem;
    }

    public override void LoadGameDataDirectory()
    {
        if (!_fileSystem.Directory.Exists(GameDataDirectoryPath))
        {
            StormModStorage.AddDirectoryNotFound(new StormPath()
            {
                StormModName = Name,
                Path = GameDataDirectoryPath,
                PathType = StormPathType.File,
            });

            return;
        }

        IEnumerable<string> files = _fileSystem.Directory.EnumerateFiles(GameDataDirectoryPath, $"*{XmlFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadGameDataFiles(files);
    }

    public override void LoadStormLayoutDirectory()
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

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!IsFileExists(filePath))
        {
            return false;
        }

        stream = _fileSystem.File.OpenRead(filePath);

        return true;
    }

    protected override bool IsFileExists(string filePath) => _fileSystem.File.Exists(filePath);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
