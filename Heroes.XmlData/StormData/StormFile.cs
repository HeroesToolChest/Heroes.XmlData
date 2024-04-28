namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains properties file or directory.
/// </summary>
public record StormFile
{
    /// <summary>
    /// Gets the name of the stormmod.
    /// </summary>
    public required string? StormModName { get; init; }

    /// <summary>
    /// Gets the directory path of the stormmod.
    /// </summary>
    public required string? StormModDirectoryPath { get; init; }

    /// <summary>
    /// Gets the path of the file or directory.
    /// </summary>
    public required string Path { get; init; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path;
    }
}
