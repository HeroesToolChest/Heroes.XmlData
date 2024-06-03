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
    public virtual bool Equals(StormFile? other)
    {
        if (other is null)
            return false;

        if (StormModName.AsSpan().Equals(other.StormModName.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
            StormModDirectoryPath.AsSpan().Equals(other.StormModDirectoryPath.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
            Path.AsSpan().Equals(other.Path.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(StormModName?.ToUpperInvariant(), StormModDirectoryPath?.ToUpperInvariant(), Path.ToUpperInvariant());
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path;
    }
}
