namespace Heroes.StormData.DepotCache;

internal abstract class DepotCacheBase<T> : IDepotCache
    where T : IHeroesSource
{
    public DepotCacheBase(T heroesSource)
        : this(new FileSystem(), heroesSource)
    {
    }

    public DepotCacheBase(IFileSystem fileSystem, T heroesSource)
    {
        FileSystem = fileSystem;
        HeroesSource = heroesSource;

        DepotCacheDirectoryPath = Path.Combine(HeroesSource.ModsDirectoryPath, SourceConstants.CoreStormModDirectory, SourceConstants.BaseStormDataDirectory, SourceConstants.DepotCacheDirectory);
    }

    /// <inheritdoc/>
    public DepotCacheData Data { get; } = new();

    protected string DepotCacheDirectoryPath { get; }

    protected IFileSystem FileSystem { get; }

    protected T HeroesSource { get; }

    /// <inheritdoc/>
    public void LoadDepotCache()
    {
        Data.Clear();

        FindMapRootData();
    }

    protected static bool IsS2MVFile(string filePath) => Path.GetExtension(filePath).Equals(".s2mv", StringComparison.OrdinalIgnoreCase);

    protected static bool IsS2MAFile(string filePath) => Path.GetExtension(filePath).Equals(".s2ma", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Finds the <c>.s2mv</c> and <c>.s2ma</c> files. All the <c>.s2mv</c> files must be added (<see cref="AddS2MVFile"/>) before the <c>.s2ma</c> files (<see cref="AddS2maFile"/>).
    /// </summary>
    protected abstract void FindMapRootData();

    /// <summary>
    /// Loads a <c>.s2mv</c> file and adds it to <see cref="Data"/> if it is a map root file.
    /// </summary>
    /// <param name="s2mvFile">The stream of the file.</param>
    /// <param name="filePath">The path of the file.</param>
    protected void AddS2MVFile(Stream s2mvFile, string filePath)
    {
        if (!S2MVFileParser.TryParse(s2mvFile, out S2MVProperties? s2mvProperties))
            return;

        s2mvProperties.DirectoryPath = PathNormalizer.NormalizePath(filePath, HeroesSource.ModsDirectoryPath);

        Data.AddS2MV(filePath, s2mvProperties);
    }

    /// <summary>
    /// Loads a <c>.s2ma</c> file and adds it to <see cref="Data"/> if it is a map stormmod file.
    /// </summary>
    /// <param name="s2maFile">The stream of the file.</param>
    /// <param name="filePath">The path of the file.</param>
    protected void AddS2maFile(Stream s2maFile, string filePath)
    {
        if (!S2MAFileParser.TryParse(s2maFile, out S2MAProperties? s2maProperties))
            return;

        s2maProperties.DirectoryPath = PathNormalizer.NormalizePath(filePath, HeroesSource.ModsDirectoryPath);

        Data.AddS2MA(filePath, s2maProperties);
    }
}
