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

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!File.Exists(filePath))
        {
            return false;
        }

        stream = File.OpenRead(filePath);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        if (!Directory.Exists(GameDataDirectoryPath))
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

        IEnumerable<string> files = Directory.EnumerateFiles(GameDataDirectoryPath);

        foreach (string file in files)
        {
            if (DirectoryPath == HeroesSource.CoreStormModDirectory || DirectoryPath == HeroesSource.HeroesDataStormModDirectory)
                AddXmlFile(file, true);
            else
                AddXmlFile(file);
        }
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateFileStormModInstance(HeroesSource, path, stormModType);
}
