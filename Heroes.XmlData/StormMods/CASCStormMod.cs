namespace Heroes.XmlData.StormMods;

internal class CASCStormMod : StormMod<ICASCHeroesSource>, IStormMod
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, bool isMapMod)
        : base(cascHeroesSource, directoryPath, isMapMod)
    {
    }

    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string name, string directoryPath, bool isMapMod)
    : base(cascHeroesSource, name, directoryPath, isMapMod)
    {
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
            StormStorage.AddDirectoryNotFound(GameDataDirectoryPath);
            return;
        }

        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            AddXmlFile(file.Value.FullName);
        }
    }

    protected override IStormMod GetStormMod(string path, bool isMapMod) => HeroesSource.CreateStormModInstance<CASCStormMod>(HeroesSource, path, isMapMod);
}
