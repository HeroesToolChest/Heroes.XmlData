namespace Heroes.StormData.Cache.Files;

/// <summary>
/// Looks up files across the base, map, and custom <see cref="FileCache"/> layers.
/// <para>The <c>TryGetFirst</c> methods get a reference from the first layer that has it (custom, then map, then base).</para>
/// </summary>
internal sealed class LayeredFileCache
{
    private readonly FileCache _baseFileCache;
    private readonly FileCache _mapFileCache;
    private readonly FileCache _customFileCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredFileCache"/> class.
    /// </summary>
    /// <param name="baseFileCache">The <see cref="FileCache"/> of the base data storage.</param>
    /// <param name="mapFileCache">The <see cref="FileCache"/> of the map data storage.</param>
    /// <param name="customFileCache">The <see cref="FileCache"/> of the custom data storage.</param>
    public LayeredFileCache(FileCache baseFileCache, FileCache mapFileCache, FileCache customFileCache)
    {
        _baseFileCache = baseFileCache;
        _mapFileCache = mapFileCache;
        _customFileCache = customFileCache;
    }

    /// <summary>
    /// Tries to retrieve the first <c>.stormlayout</c> file by its relative <c>ui</c> path from the data storages.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormdata</c> directory, which starts with <c>ui</c> (e.g. <c>ui/layout/hero/herotalents.stormlayout</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/> associated with the specified path, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstStormLayoutFileByRelativePath(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile)
    {
        return TryGetFirstFileByRelativePath(relativePath, static fileCache => fileCache.StormLayoutFilesByRelativePath, out stormFile);
    }

    /// <summary>
    /// Tries to retrieve the first asset file (e.g. a <c>.dds</c> file) by its relative <c>assets</c> path from the data storages.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormassets</c> directory, which starts with <c>assets</c> (e.g. <c>assets/textures/storm_ui_icon_abathur.dds</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/> associated with the specified path, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFirstAssetFileByRelativePath(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile)
    {
        return TryGetFirstFileByRelativePath(relativePath, static fileCache => fileCache.AssetFilesByRelativePath, out stormFile);
    }

    // the file caches are keyed by normalized paths (see PathNormalizer), so the path is normalized the same way before the lookup
    private bool TryGetFirstFileByRelativePath(
        ReadOnlySpan<char> relativePath,
        Func<FileCache, Dictionary<string, StormFile>.AlternateLookup<ReadOnlySpan<char>>> getFilesByRelativePath,
        [NotNullWhen(true)] out StormFile? stormFile)
    {
        const int maxStackLength = 256;

        char[]? rentedBuffer = null;

        Span<char> buffer = relativePath.Length <= maxStackLength
            ? stackalloc char[maxStackLength]
            : (rentedBuffer = ArrayPool<char>.Shared.Rent(relativePath.Length));

        try
        {
            Span<char> normalizedPath = buffer[..relativePath.Length];
            relativePath.CopyTo(normalizedPath);
            PathNormalizer.NormalizePath(normalizedPath);

            // custom first
            if (getFilesByRelativePath(_customFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            if (getFilesByRelativePath(_mapFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            if (getFilesByRelativePath(_baseFileCache).TryGetValue(normalizedPath, out stormFile))
                return true;

            return false;
        }
        finally
        {
            if (rentedBuffer is not null)
                ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }
}
