namespace Heroes.StormData.Cache.Files;

/// <summary>
/// Represents a cache for storing and retrieving files from the data storage.
/// </summary>
internal sealed class FileCache
{
    /// <summary>
    /// Gets the <c>.stormlayout</c> files by their relative <c>ui</c> path (relative path will start with ui).
    /// </summary>
    public Dictionary<string, StormFile>.AlternateLookup<ReadOnlySpan<char>> StormLayoutFilesByRelativePath { get; } =
        new Dictionary<string, StormFile>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets the assets files (the .dds files) by their relative <c>assets</c> path (relative path will start with assets).
    /// </summary>
    public Dictionary<string, StormFile>.AlternateLookup<ReadOnlySpan<char>> AssetFilesByRelativePath { get; } =
        new Dictionary<string, StormFile>(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();
}
