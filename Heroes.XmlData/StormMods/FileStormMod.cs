namespace Heroes.XmlData.StormMods;

internal class FileStormMod : StormMod<IHeroesSource>
{
    public FileStormMod(IHeroesSource heroesSource, string directoryPath, bool isMapMod)
        : base(heroesSource, directoryPath, isMapMod)
    {
    }

    public FileStormMod(IHeroesSource heroesSource, string name, string directoryPath, bool isMapMod)
        : base(heroesSource, name, directoryPath, isMapMod)
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
            StormStorage.AddDirectoryNotFound(GameDataDirectoryPath, Name, DirectoryPath);
            return;
        }

        IEnumerable<string> files = Directory.EnumerateFiles(GameDataDirectoryPath);

        foreach (string file in files)
        {
            AddXmlFile(file);
        }
    }

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.CreateStormModInstance<FileStormMod>(HeroesSource, path, isMapMod);
}
