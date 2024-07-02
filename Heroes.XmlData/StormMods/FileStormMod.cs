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
                StormModDirectoryPath = DirectoryPath,
                StormModName = Name,
                Path = GameDataDirectoryPath,
                PathType = StormPathType.File,
            });

            return;
        }

        IEnumerable<string> files = FileSystem.Directory.EnumerateFiles(GameDataDirectoryPath);

        foreach (string file in files)
        {
            if (DirectoryPath == HeroesSource.CoreStormModDirectory || DirectoryPath == HeroesSource.HeroesDataStormModDirectory)
                AddXmlFile(file, true);
            else
                AddXmlFile(file);
        }
    }

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!FileSystem.File.Exists(filePath))
        {
            return false;
        }

        stream = FileSystem.File.OpenRead(filePath);

        return true;
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
