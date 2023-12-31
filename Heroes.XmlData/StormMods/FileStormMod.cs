namespace Heroes.XmlData.StormMods;

internal abstract class FileStormMod : StormMod<IHeroesSource>
{
    public FileStormMod(IHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    protected override void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        HeroesData.AddMainXmlFile(document, xmlFilePath);
    }

    protected override bool ValidateXmlFile(string xmlFilePath, [NotNullWhen(true)] out XDocument? document, bool isRequired = true)
    {
        document = null;

        if (!IsXmlFile(xmlFilePath))
            return false;

        if (!File.Exists(xmlFilePath))
        {
            if (isRequired)
                HeroesData.AddFileNotFound(xmlFilePath);

            return false;
        }

        document = XDocument.Load(xmlFilePath);

        return true;
    }

    protected override bool ValidateGameStringFile(HeroesLocalization localization, [NotNullWhen(true)] out Stream? stream, out string path)
    {
        stream = null;
        path = GetGameStringFilePath(localization);

        if (!File.Exists(path))
        {
            HeroesData.AddFileNotFound(path);
            return false;
        }

        stream = File.OpenRead(path);

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
}
