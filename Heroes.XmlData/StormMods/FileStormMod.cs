using System.IO.Abstractions;

namespace Heroes.XmlData.StormMods;

internal class FileStormMod : StormMod<IFileHeroesSource>
{
    public FileStormMod(IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType, StormPathType.File)
    {
    }

    public FileStormMod(IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType, StormPathType.File)
    {
    }

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string directoryPath, StormModType stormModType)
        : base(fileSystem, heroesSource, directoryPath, stormModType, StormPathType.File)
    {
    }

    public FileStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(fileSystem, heroesSource, name, directoryPath, stormModType, StormPathType.File)
    {
    }

    public override void LoadGameDataDirectory()
    {
        if (!FileSystem.Directory.Exists(GameDataDirectoryPath))
        {
            StormModStorage.AddDirectoryNotFound(new StormPath()
            {
                StormModName = Name,
                Path = GameDataDirectoryPath,
                PathType = StormPathType.File,
            });

            return;
        }

        IEnumerable<string> files = FileSystem.Directory.EnumerateFiles(GameDataDirectoryPath, $"*{XmlFileExtension}", new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
        }).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadGameDataFiles(files);
    }

    public override void LoadStormLayoutDirectory()
    {
        if (!FileSystem.Directory.Exists(LayoutDirectoryPath))
            return;

        IEnumerable<string> files = FileSystem.Directory.EnumerateFiles(LayoutDirectoryPath, $"*{StormLayoutFileExtension}", new EnumerationOptions()
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

        stream = FileSystem.File.OpenRead(filePath);

        return true;
    }

    protected override bool IsFileExists(string filePath) => FileSystem.File.Exists(filePath);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
