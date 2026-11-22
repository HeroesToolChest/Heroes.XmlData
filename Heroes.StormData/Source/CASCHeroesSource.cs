namespace Heroes.StormData.Source;

internal sealed class CASCHeroesSource : HeroesSourceBase, ICASCHeroesSource
{
    private readonly ICASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(string modsDirectoryPath, IStormModFactory stormModFactory, ICASCHeroesStorage cascHeroesStorage)
        : base(modsDirectoryPath, stormModFactory)
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public ICASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    public CASCFolder GetCASCFolder(string? directory = null)
    {
        if (string.IsNullOrEmpty(directory))
            return CASCHeroesStorage.CASCFolderRoot;

        if (!CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(directory, out CASCFolder? folder))
            throw new DirectoryNotFoundException($"Could not find folder: {directory}");

        return folder;
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateCASCStormMod(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateCASCMpqStormMod(this, name, directoryPath, stormModType);

    protected override IDepotCache CreateDepotCache() => new CASCDepotCache(this);

    protected override bool FileExistsCore(string fullPath) => CASCHeroesStorage.CASCHandlerWrapper.FileExists(fullPath);

    protected override bool TryOpenFile(string fullPath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        ICASCHandlerWrapper cascHandlerWrapper = CASCHeroesStorage.CASCHandlerWrapper;

        // check first, otherwise OpenFile throws a FileNotFoundException for a missing file
        if (!cascHandlerWrapper.FileExists(fullPath))
            return false;

        stream = cascHandlerWrapper.OpenFile(fullPath);

        return stream is not null;
    }
}
