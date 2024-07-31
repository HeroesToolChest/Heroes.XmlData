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

        EnumerateGameDataDirectory(gameDataFolder);
    }

    public override void LoadStormLayoutDirectory()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(LayoutDirectoryPath, out CASCFolder? layoutFolder))
            return;

        EnumerateLayoutDirectory(layoutFolder);
    }

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!IsFileExists(filePath))
            return false;

        stream = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(filePath);

        return true;
    }

    protected override bool IsFileExists(string filePath) => HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.FileExists(filePath);

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormModInstance(HeroesSource, path, stormModType);

    private void EnumerateGameDataDirectory(CASCFolder gameDataFolder)
    {
        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            if (!Path.GetExtension(file.Value.FullName.AsSpan()).Equals(XmlFileExtension, StringComparison.OrdinalIgnoreCase))
                continue;

            if (DirectoryPath == HeroesSource.CoreStormModDirectory || DirectoryPath == HeroesSource.HeroesDataStormModDirectory)
                AddXmlFile(file.Value.FullName, true);
            else
                AddXmlFile(file.Value.FullName);
        }

        foreach (KeyValuePair<string, CASCFolder> folder in gameDataFolder.Folders)
        {
            EnumerateGameDataDirectory(folder.Value);
        }
    }

    private void EnumerateLayoutDirectory(CASCFolder layoutFolder)
    {
        foreach (KeyValuePair<string, CASCFile> file in layoutFolder.Files)
        {
            if (!Path.GetExtension(file.Value.FullName.AsSpan()).Equals(StormLayoutFileExtension, StringComparison.OrdinalIgnoreCase))
                continue;

            AddStormLayoutFilePath(file.Value.FullName);
        }

        foreach (KeyValuePair<string, CASCFolder> folder in layoutFolder.Folders)
        {
            EnumerateLayoutDirectory(folder.Value);
        }
    }
}
