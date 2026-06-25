namespace Heroes.XmlData.Source;

internal sealed class CASCHeroesSource : HeroesSource, ICASCHeroesSource
{
    private readonly ICASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, ICASCHeroesStorage cascHeroesStorage, IProgressReporter? progressReporter)
        : base(stormStorage, stormModFactory, depotCacheFactory, progressReporter)
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public ICASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    public override bool FileExists(string? path, string? mpqEntryPath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (mpqEntryPath is null)
            return CASCHeroesStorage.CASCHandlerWrapper.FileExists(GetValidatedPath(path));
        else
            return IsMpqFileEntryExists(GetValidatedPath(path), mpqEntryPath);
    }

    public override bool FileExists(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return IsMpqFileEntryExists(GetValidatedPath(stormFile.StormPath.StormModPath), stormFile.StormPath.Path);
        else
            return FileExists(stormFile.StormPath.Path);
    }

    public override Stream GetFile(string path, string? mpqEntryPath = null)
    {
        Stream? stream;

        if (mpqEntryPath is null)
        {
            stream = CASCHeroesStorage.CASCHandlerWrapper.OpenFile(GetValidatedPath(path));

            return stream is null ? throw new FileNotFoundException("Could not find file", path) : stream;
        }
        else
        {
            return GetMpqFileEntry(path, mpqEntryPath);
        }
    }

    public override Stream GetFile(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return GetMpqFileEntry(stormFile.StormPath.StormModPath, stormFile.StormPath.Path);
        else
            return GetFile(stormFile.StormPath.Path);
    }

    public CASCFolder GetCASCFolder(string? directory = null)
    {
        if (string.IsNullOrEmpty(directory))
            return CASCHeroesStorage.CASCFolderRoot;

        if (!CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(directory, out CASCFolder? folder))
            throw new DirectoryNotFoundException($"Could not find folder: {directory}");

        return folder;
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType, IProgressReporter? progressReporter = null) => StormModFactory.CreateCASCStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateCASCMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateCASCDepotCache(this);

    protected override string GetValidatedPath(string path)
    {
        if (!path.StartsWith(DefaultModsDirectory))
            path = Path.Join(ModsBaseDirectoryPath, path);

        return path;
    }

    protected override bool IsMpqFileEntryExists(string mpqPath, string entryPath)
    {
        if (!FileExists(mpqPath))
            return false;

        using MpqHeroesArchive mpqFile = MpqHeroesFile.Open(GetFile(mpqPath));
        return mpqFile.FileEntryExists(entryPath);
    }
}
