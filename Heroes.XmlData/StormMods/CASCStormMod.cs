using CASCLib;

namespace Heroes.XmlData.StormMods;

internal abstract class CASCStormMod : StormMod<ICASCHeroesSource>, IStormMod
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource)
        : base(cascHeroesSource)
    {
    }

    protected override void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        HeroesData.AddXmlFile(document, xmlFilePath);
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

        if (!File.Exists(path))
        {
            HeroesData.AddFileNotFound(path);
            return false;
        }

        stream = HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(path);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        CASCFolder gameDataFolder = HeroesSource.CASCHeroesStorage.CASCFolderRoot.GetFolder(GameDataDirectoryPath);

        if (gameDataFolder is null)
        {
            HeroesData.AddDirectoryNotFound(GameDataDirectoryPath);
            return;
        }

        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            AddXmlFile(file.Value.FullName);
        }
    }
}
