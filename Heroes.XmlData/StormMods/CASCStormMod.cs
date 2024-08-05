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

        IEnumerable<string> files = gameDataFolder.Files.Select(x => x.Value)
            .Where(x => Path.GetExtension(x.FullName).Equals(XmlFileExtension, StringComparison.OrdinalIgnoreCase))
            .Select(x => PathHelper.NormalizePath(x.FullName))
            .OrderBy(x => x);

        LoadGameDataFiles(files);
    }

    public override void LoadStormLayoutDirectory()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(LayoutDirectoryPath, out CASCFolder? layoutFolder))
            return;

        IEnumerable<string> files = EnumerateDirectory(layoutFolder)
            .Where(x => Path.GetExtension(x.FullName).Equals(StormLayoutFileExtension, StringComparison.OrdinalIgnoreCase))
            .Select(x => PathHelper.NormalizePath(x.FullName))
            .OrderBy(x => x);

        LoadStormLayoutFiles(files);
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

    private static IEnumerable<CASCFile> EnumerateDirectory(CASCFolder gameDataFolder)
    {
        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            yield return file.Value;
        }

        foreach (KeyValuePair<string, CASCFolder> folder in gameDataFolder.Folders)
        {
            foreach (CASCFile file in EnumerateDirectory(folder.Value))
            {
                yield return file;
            }
        }
    }
}
