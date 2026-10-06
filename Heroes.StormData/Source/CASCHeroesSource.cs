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
}
