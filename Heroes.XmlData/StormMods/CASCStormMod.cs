namespace Heroes.XmlData.StormMods;

internal class CASCStormMod : StormMod<ICASCHeroesSource>, IStormMod
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
        : base(cascHeroesSource, directoryPath, stormModType, StormPathType.CASC)
    {
    }

    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string name, string directoryPath, StormModType stormModType)
    : base(cascHeroesSource, name, directoryPath, stormModType, StormPathType.CASC)
    {
    }

    public override void LoadGameDataDirectory()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(GameDataDirectoryPath, out CASCFolder? gameDataFolder))
        {
            StormModStorage.AddDirectoryNotFound(new StormPath()
            {
                StormModName = Name,
                Path = GameDataDirectoryPath,
                PathType = StormPathType.CASC,
            });

            return;
        }

        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            if (DirectoryPath == HeroesSource.CoreStormModDirectory || DirectoryPath == HeroesSource.HeroesDataStormModDirectory)
                AddXmlFile(file.Value.FullName, true);
            else
                AddXmlFile(file.Value.FullName);
        }
    }

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.FileExists(filePath))
            return false;

        stream = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(filePath);

        return true;
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormModInstance(HeroesSource, path, stormModType);
}
