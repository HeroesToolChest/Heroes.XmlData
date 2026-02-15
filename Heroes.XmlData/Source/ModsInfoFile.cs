namespace Heroes.XmlData.Source;

/// <summary>
/// Represents the hdp.info file in a file-based heroes source.
/// </summary>
public sealed class ModsInfoFile
{
    /// <summary>
    /// Gets or sets the Heroes of the Storm version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the version is a PTR version.
    /// </summary>
    public bool IsPtr { get; set; }

    /// <summary>
    /// Gets or sets the Heroes Data Parser version used to extract the files.
    /// </summary>
    public string HdpVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time of the extraction.
    /// </summary>
    public DateTimeOffset ExtractedDate { get; set; }
}
