namespace Heroes.XmlData;

/// <summary>
/// Contains properties for a missing directory or file.
/// </summary>
public record XmlPathLocation
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
    /// Gets the missing location path.
    /// </summary>
    public required string Path { get; init; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path;
    }
}
