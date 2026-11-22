namespace Heroes.StormData.StormMods.Mpq;

/// <summary>
/// Represents a base class for a <c>stormmod</c> that is an MPQ archive (e.g. a <c>.s2ma</c> or <c>.stormmap</c> file).
/// </summary>
/// <remarks>
/// The paths used by <see cref="StormModBase{T}"/> are full paths that start with <see cref="StormModBase{T}.DirectoryPath"/>, which is the path of the archive file.
/// The part after it is the path of the file inside the archive.
/// The archive is only open while <see cref="LoadStormFiles"/>, <see cref="LoadStormGameStrings"/>, or <see cref="GetMapDependencies"/> runs.
/// </remarks>
/// <typeparam name="T">The type of the <see cref="IHeroesSource"/> source.</typeparam>
internal abstract class MpqStormModBase<T> : StormModBase<T>
    where T : IHeroesSource
{
    private MpqHeroesArchive? _mpqHeroesArchive;
    private MpqFolder? _mpqFolderRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpqStormModBase{T}"/> class.
    /// </summary>
    /// <param name="heroesSource">The source of the data.</param>
    /// <param name="directoryPath">The full path of the MPQ archive file.</param>
    /// <param name="stormModType">Type of the <c>stormmod</c>.</param>
    /// <param name="name">The name of the <c>stormmod</c> (e.g. the map title). If <see langword="null"/>, the archive file name without its extension is used.</param>
    protected MpqStormModBase(T heroesSource, string directoryPath, StormModType stormModType, string? name = null)
        : base(heroesSource, directoryPath, stormModType, name)
    {
    }

    protected override bool IsMpqArchive => true;

    public override void LoadStormFiles()
    {
        if (!TryOpenArchive(out bool openedArchive))
            return;

        try
        {
            base.LoadStormFiles();
        }
        finally
        {
            if (openedArchive)
                CloseArchive();
        }
    }

    public override void LoadStormGameStrings(StormLocale stormLocale)
    {
        if (!TryOpenArchive(out bool openedArchive))
            return;

        try
        {
            base.LoadStormGameStrings(stormLocale);
        }
        finally
        {
            if (openedArchive)
                CloseArchive();
        }
    }

    public override IReadOnlyList<MapDependency> GetMapDependencies()
    {
        if (!TryOpenArchive(out bool openedArchive))
            return [];

        try
        {
            return base.GetMapDependencies();
        }
        finally
        {
            if (openedArchive)
                CloseArchive();
        }
    }

    protected override void LoadGameDataDirectory()
    {
        if (!TryGetArchiveFolder(GameDataDirectoryPath, out MpqFolder? gameDataFolder))
            return;

        // only the files in the directory, not the subdirectories
        LoadGameDataFiles(GetFullPaths(gameDataFolder.Files.Values, XmlFileExtension));
    }

    protected override void LoadStormLayoutDirectory()
    {
        if (!TryGetArchiveFolder(LayoutDirectoryPath, out MpqFolder? layoutFolder))
            return;

        LoadStormLayoutFiles(GetFullPaths(layoutFolder.EnumerateFiles(), StormLayoutFileExtension));
    }

    protected override void LoadAssetsDirectory()
    {
        if (!TryGetArchiveFolder(AssetsDirectoryPath, out MpqFolder? assetsFolder))
            return;

        LoadAssetFiles(GetFullPaths(assetsFolder.EnumerateFiles(), DDSFileExtension));
    }

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (_mpqHeroesArchive is null || !TryGetArchivePath(path, out ReadOnlySpan<char> archivePath))
            return false;

        // the lookup is case-insensitive and normalizeSlashes handles '/' as the separator
        if (!_mpqHeroesArchive.TryGetEntry(archivePath, out MpqHeroesArchiveEntry? entry, true))
            return false;

        stream = _mpqHeroesArchive.DecompressEntry(entry.Value);
        return true;
    }

    /// <summary>
    /// Opens the MPQ archive file at <see cref="StormModBase{T}.DirectoryPath"/>.
    /// </summary>
    /// <param name="stream">When this method returns, contains the <see cref="Stream"/> of the archive file, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the archive file exists; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryOpenMpqFile([NotNullWhen(true)] out Stream? stream);

    private static string ToPlatformSeparators(string path) =>
        path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

    private static MpqFolder CreateFolderRoot(MpqHeroesArchive mpqHeroesArchive)
    {
        MpqFolder root = new("root");

        foreach (MpqHeroesArchiveEntry entry in mpqHeroesArchive.MpqArchiveEntries)
        {
            // the name is unknown when the archive has no listfile entry for it
            if (!string.IsNullOrWhiteSpace(entry.FileName))
                root.AddFile(entry.FileName);
        }

        return root;
    }

    /// <summary>
    /// Opens the archive if it is not already open.
    /// </summary>
    /// <param name="openedArchive">When this method returns, contains <see langword="true"/> if this call opened the archive and so should close it.</param>
    /// <returns><see langword="true"/> if the archive is open; otherwise <see langword="false"/>.</returns>
    private bool TryOpenArchive(out bool openedArchive)
    {
        openedArchive = false;

        if (_mpqHeroesArchive is not null)
            return true;

        if (!TryOpenMpqFile(out Stream? stream))
            return false;

        try
        {
            // the stream is disposed when the archive is disposed
            _mpqHeroesArchive = MpqHeroesFile.Open(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }

        openedArchive = true;
        return true;
    }

    private void CloseArchive()
    {
        _mpqHeroesArchive?.Dispose();
        _mpqHeroesArchive = null;
    }

    /// <summary>
    /// Gets a folder in the archive from its full path. The folder tree is created from the archive the first time it is needed.
    /// </summary>
    /// <param name="path">The full path of the folder, starting with <see cref="StormModBase{T}.DirectoryPath"/>.</param>
    /// <param name="folder">When this method returns, contains the folder, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the folder exists; otherwise <see langword="false"/>.</returns>
    private bool TryGetArchiveFolder(string path, [NotNullWhen(true)] out MpqFolder? folder)
    {
        folder = null;

        if (_mpqHeroesArchive is null || !TryGetArchivePath(path, out ReadOnlySpan<char> archivePath))
            return false;

        _mpqFolderRoot ??= CreateFolderRoot(_mpqHeroesArchive);

        return _mpqFolderRoot.TryGetFolder(archivePath.ToString(), out folder);
    }

    /// <summary>
    /// Gets the path inside the archive from a full path.
    /// </summary>
    /// <param name="path">The full path, starting with <see cref="StormModBase{T}.DirectoryPath"/>.</param>
    /// <param name="archivePath">When this method returns, contains the part of <paramref name="path"/> after <see cref="StormModBase{T}.DirectoryPath"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is in this archive; otherwise <see langword="false"/>.</returns>
    private bool TryGetArchivePath(string path, out ReadOnlySpan<char> archivePath)
    {
        archivePath = default;

        if (path.Length <= DirectoryPath.Length + 1 ||
            !path.StartsWith(DirectoryPath, StringComparison.OrdinalIgnoreCase) ||
            path[DirectoryPath.Length] is not ('/' or '\\'))
        {
            return false;
        }

        archivePath = path.AsSpan(DirectoryPath.Length + 1);
        return true;
    }

    /// <summary>
    /// Gets the full paths of the files with an extension, sorted the same way as <see cref="FileStormMod"/>.
    /// </summary>
    /// <param name="files">The files in the archive.</param>
    /// <param name="extension">The file extension to keep.</param>
    /// <returns>The full paths, starting with <see cref="StormModBase{T}.DirectoryPath"/>.</returns>
    private IEnumerable<string> GetFullPaths(IEnumerable<MpqFile> files, string extension) => files
        .Where(x => Path.GetExtension(x.Name).Equals(extension, StringComparison.OrdinalIgnoreCase))
        .Select(x => Path.Join(DirectoryPath, ToPlatformSeparators(x.FullName)))
        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
}
