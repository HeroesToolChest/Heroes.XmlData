namespace Heroes.StormData.Source;

internal abstract class HeroesSourceBase : IHeroesSource
{
    private readonly List<IStormMod> _stormMods = [];
    private readonly List<IStormMod> _stormMapMods = [];
    private readonly List<IStormMod> _stormCustomMods = [];

    private IDepotCache? _depotCache;

    public HeroesSourceBase(string modsDirectoryPath, IStormModFactory stormModFactory)
    {
        ModsDirectoryPath = modsDirectoryPath;
        StormModFactory = stormModFactory;

        AddStormMods();
    }

    public string ModsDirectoryPath { get; }

    public SourceContainer SourceContainer { get; } = new();

    public IStormModFactory StormModFactory { get; }

    public IReadOnlyList<IStormMod> StormMapMods => _stormMapMods;

    public void LoadStormMods()
    {
        foreach (IStormMod stormMod in _stormMods)
        {
            SourceContainer.AddModContainer(stormMod.ModContainer);

            stormMod.LoadStormFiles();
        }
    }

    public void LoadDepotCache()
    {
        _depotCache ??= CreateDepotCache();
        _depotCache.LoadDepotCache();
    }

    public void LoadGamestrings(StormLocale stormLocale)
    {
        SourceContainer.ClearGamestrings();

        foreach (IStormMod stormMod in _stormMods)
            stormMod.LoadStormGameStrings(stormLocale);

        foreach (IStormMod stormMod in _stormMapMods)
            stormMod.LoadStormGameStrings(stormLocale);

        foreach (IStormMod stormMod in _stormCustomMods)
            stormMod.LoadStormGameStrings(stormLocale);
    }

    public bool LoadStormMapMod(string mapTitle)
    {
        if (!TryGetS2MAProperties(mapTitle, out S2MAProperties? s2maProperties))
            return false;

        UnloadStormMapMod();

        IStormMod mapRootMod = GetMpqStormMod(mapTitle, Path.Join(ModsDirectoryPath, s2maProperties.DirectoryPath), StormModType.Map);

        StormMapModResolver stormMapModResolver = new(localFile => GetStormMod(Path.Join(ModsDirectoryPath, localFile), StormModType.Map));

        _stormMapMods.AddRange(stormMapModResolver.Resolve(mapRootMod, s2maProperties.MapDependencies));

        // the resolver returns them in load order, dependencies first and the map root last
        foreach (IStormMod stormMod in _stormMapMods)
        {
            SourceContainer.AddModContainer(stormMod.ModContainer);

            stormMod.LoadStormFiles();
        }

        return true;
    }

    public void UnloadStormMapMod()
    {
        _stormMapMods.Clear();
        SourceContainer.ClearStormMapMods();
    }

    public IEnumerable<string> GetMapTitles() => _depotCache?.Data.S2MAPropertiesByTitle.Keys ?? Enumerable.Empty<string>();

    public bool TryGetS2MAProperties(string mapTitle, [NotNullWhen(true)] out S2MAProperties? s2maProperties)
    {
        if (_depotCache is null)
        {
            s2maProperties = null;
            return false;
        }

        return _depotCache.Data.S2MAPropertiesByTitle.TryGetValue(mapTitle, out s2maProperties);
    }

    public bool FileExists(string path, string? mpqEntryPath)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string fullPath = GetFullPath(path);

        if (string.IsNullOrEmpty(mpqEntryPath))
            return FileExistsCore(fullPath);

        try
        {
            if (!TryOpenFile(fullPath, out Stream? stream))
                return false;

            using MpqHeroesArchive mpqHeroesArchive = OpenMpqArchive(stream);

            return mpqHeroesArchive.FileEntryExists(mpqEntryPath, true);
        }
        catch (Exception ex) when (ex is MpqHeroesToolException or EndOfStreamException or UnauthorizedAccessException)
        {
            // the file is not an MPQ archive that can be read, or the path is a directory
            return false;
        }
    }

    public Stream GetFile(string path, string? mpqEntryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string fullPath = GetFullPath(path);

        if (!TryOpenFile(fullPath, out Stream? stream))
            throw new FileNotFoundException($"Could not find file: {fullPath}", fullPath);

        if (string.IsNullOrEmpty(mpqEntryPath))
            return stream;

        // the decompressed entry does not depend on the archive
        using MpqHeroesArchive mpqHeroesArchive = OpenMpqArchive(stream);

        if (!mpqHeroesArchive.TryGetEntry(mpqEntryPath, out MpqHeroesArchiveEntry? entry, true))
            throw new FileNotFoundException($"Could not find the entry '{mpqEntryPath}' in the MPQ file: {fullPath}", mpqEntryPath);

        return mpqHeroesArchive.DecompressEntry(entry.Value);
    }

    /// <summary>
    /// Gets the full path of a file from a path that is either relative to the <c>mods</c> directory (it may start with <c>mods</c>)
    /// or starts with the <see cref="ModsDirectoryPath"/>.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The full path, which starts with the <see cref="ModsDirectoryPath"/>.</returns>
    protected virtual string GetFullPath(string path)
    {
        int modsDirectoryPathLength = PathNormalizer.GetDirectoryPrefixLength(path, ModsDirectoryPath);

        // the paths in the game data start with "mods", which is the game's own mods directory, not the ModsDirectoryPath
        string relativePath = modsDirectoryPathLength > 0
            ? PathNormalizer.NormalizePath(path.AsSpan(modsDirectoryPathLength))
            : PathNormalizer.NormalizePath(path, SourceConstants.DefaultModsDirectory);

        return Path.Join(ModsDirectoryPath, relativePath);
    }

    /// <summary>
    /// Determines if a file exists.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>.</returns>
    protected abstract bool FileExistsCore(string fullPath);

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <param name="stream">When this method returns, contains the <see cref="Stream"/> of the file, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryOpenFile(string fullPath, [NotNullWhen(true)] out Stream? stream);

    /// <summary>
    /// Gets the <see cref="IStormMod"/> for the specified directory path.
    /// </summary>
    /// <param name="directoryPath">The full path to the directory of the <c>stormmod</c>.</param>
    /// <param name="stormModType">The type of the <c>stormmod</c>.</param>
    /// <returns>The <see cref="IStormMod"/> instance.</returns>
    protected abstract IStormMod GetStormMod(string directoryPath, StormModType stormModType);

    protected abstract IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType);

    /// <summary>
    /// Creates the <see cref="IDepotCache"/> for this source.
    /// </summary>
    /// <returns>The <see cref="IDepotCache"/> instance.</returns>
    protected abstract IDepotCache CreateDepotCache();

    private static MpqHeroesArchive OpenMpqArchive(Stream stream)
    {
        try
        {
            // the stream is disposed when the archive is disposed
            return MpqHeroesFile.Open(stream);
        }
        catch
        {
            // the archive was not created, so the stream would otherwise be left open
            stream.Dispose();
            throw;
        }
    }

    private void AddStormMods()
    {
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.CoreStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesStormModDirectory), StormModType.Base));
        _stormMods.Add(GetStormMod(Path.Combine(ModsDirectoryPath, SourceConstants.HeroesDataStormModDirectory), StormModType.Base));
    }
}
