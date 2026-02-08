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

    public override bool FileExists(string? path, string? mpqPath = null)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (mpqPath is null)
            return CASCHeroesStorage.CASCHandlerWrapper.FileExists(GetValidatedPath(path));
        else
            return IsMpqFileEntryExists(GetValidatedPath(mpqPath), path);
    }

    public override bool FileExists(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return IsMpqFileEntryExists(GetValidatedPath(stormFile.StormPath.StormModPath), stormFile.StormPath.Path);
        else
            return FileExists(stormFile.StormPath.Path);
    }

    public override Stream GetFile(string path, string? mpqPath = null)
    {
        Stream? stream;

        if (mpqPath is null)
        {
            stream = CASCHeroesStorage.CASCHandlerWrapper.OpenFile(GetValidatedPath(path));

            if (stream is null)
                throw new FileNotFoundException("Could not find file", path);

            return stream;
        }
        else
        {
            return GetMpqFileEntry(mpqPath, path);
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
}
