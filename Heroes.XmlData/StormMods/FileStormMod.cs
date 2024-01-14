namespace Heroes.XmlData.StormMods;

internal class FileStormMod : StormMod<IHeroesSource>
{
    public FileStormMod(IHeroesSource heroesSource, string directoryPath)
        : base(heroesSource, directoryPath)
    {
    }

    public FileStormMod(IHeroesSource heroesSource, string name, string directoryPath)
        : base(heroesSource, name, directoryPath)
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
            StormStorage.AddDirectoryNotFound(GameDataDirectoryPath);
            return;
        }

        IEnumerable<string> files = Directory.EnumerateFiles(GameDataDirectoryPath);

        foreach (string file in files)
        {
            AddXmlFile(file);
        }
    }

    protected override IStormMod GetStormMod(string path) => HeroesSource.CreateStormModInstance<FileStormMod>(HeroesSource, path);
}
