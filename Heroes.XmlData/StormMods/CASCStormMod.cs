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

    protected override void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        XmlStorage.AddXmlFile(document, xmlFilePath);
    }

    protected override bool ValidateXmlFile(string xmlFilePath, [NotNullWhen(true)] out XDocument? document, bool isRequired = true)
    {
        document = null;

        if (!IsXmlFile(xmlFilePath))
            return false;

        if (!HeroesSource.CASCHeroesStorage.CASCHandler.FileExists(xmlFilePath))
        {
            if (isRequired)
                HeroesData.AddFileNotFound(xmlFilePath);

            return false;
        }

        using Stream fileStream = HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(xmlFilePath);

        document = XDocument.Load(fileStream);

        return true;
    }

    protected override bool ValidateGameStringFile(HeroesLocalization localization, [NotNullWhen(true)] out Stream? stream, out string path)
    {
        stream = null;
        path = GetGameStringFilePath(localization);

        if (!HeroesSource.CASCHeroesStorage.CASCHandler.FileExists(path))
        {
            HeroesData.AddFileNotFound(path);
            return false;
        }

        stream = HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(path);

        return true;
    }

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
