namespace Heroes.StormData.Cache.Strings;

/// <summary>
/// Represents a cache for storing and retrieving strings and text from the data storage.
/// </summary>
internal sealed class TextCache
{
    /// <summary>
    /// Gets a dictionary of gamestrings by id.
    /// </summary>
    public Dictionary<string, StringValue>.AlternateLookup<ReadOnlySpan<char>> GameStringsById { get; } =
        new Dictionary<string, StringValue>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Gets a dictionary of assets text values by id.
    /// </summary>
    public Dictionary<string, StringValue>.AlternateLookup<ReadOnlySpan<char>> AssetTextsById { get; } =
        new Dictionary<string, StringValue>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
}
