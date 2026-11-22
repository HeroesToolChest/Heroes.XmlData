namespace Heroes.StormData;

/// <summary>
/// Represents the <c>hdp.info</c> file in a file-based heroes source.
/// </summary>
public sealed class ModsInfoFile
{
    /// <summary>
    /// Gets the Heroes of the Storm version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets a value indicating whether the version is a PTR version.
    /// </summary>
    public bool IsPtr { get; init; }

    /// <summary>
    /// Gets the Heroes Data Parser version used to extract the files.
    /// </summary>
    public string HdpVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the date and time of the extraction.
    /// </summary>
    public DateTimeOffset ExtractedDate { get; init; }
}
