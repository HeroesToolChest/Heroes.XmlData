namespace Heroes.XmlData.StormMods;

internal class CASCStormMod : StormMod<ICASCHeroesSource>, IStormMod
{
    private readonly string _directoryPath;
    private readonly string? _name;

    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath)
        : base(cascHeroesSource)
    {
        _directoryPath = directoryPath;
    }

    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, string name)
    : base(cascHeroesSource)
    {
        _directoryPath = directoryPath;
        _name = name;
    }

    public override string DirectoryPath => _directoryPath;

    public override string Name => _name is null ? base.Name : _name;

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!HeroesSource.CASCHeroesStorage.CASCHandler.FileExists(filePath))
            return false;

        stream = HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(filePath);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(GameDataDirectoryPath, out CASCFolder? gameDataFolder))
        {
            HeroesData.AddDirectoryNotFound(GameDataDirectoryPath);
            return;
        }

        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            AddXmlFile(file.Value.FullName);
        }
    }

    protected override IStormMod GetStormMod(string path) => HeroesSource.CreateStormModInstance<CASCStormMod>(HeroesSource, path);
}
