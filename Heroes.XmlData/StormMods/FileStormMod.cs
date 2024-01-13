namespace Heroes.XmlData.StormMods;

internal class FileStormMod : StormMod<IHeroesSource>
{
    private readonly string _directoryPath;
    private readonly string? _name;

    public FileStormMod(IHeroesSource heroesSource, string directoryPath)
        : base(heroesSource)
    {
        _directoryPath = directoryPath;
    }

    public FileStormMod(IHeroesSource heroesSource, string directoryPath, string name)
        : base(heroesSource)
    {
        _directoryPath = directoryPath;
        _name = name;
    }

    public override string DirectoryPath => _directoryPath;

    public override string Name => _name is null ? base.Name : _name;

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
            HeroesData.AddDirectoryNotFound(GameDataDirectoryPath);
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
