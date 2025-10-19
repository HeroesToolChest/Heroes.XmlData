namespace Heroes.XmlData.Source;

/// <summary>
/// Represents the .info file in a file-based heroes source.
/// </summary>
internal sealed class InfoFile
{
    /// <summary>
    /// Gets or sets the Heroes of the Storm version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the version is a PTR version.
    /// </summary>
    public bool IsPtr { get; set; }
}
