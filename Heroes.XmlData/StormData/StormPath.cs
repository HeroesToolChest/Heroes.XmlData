﻿namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a file or directory.
/// </summary>
public record StormPath
{
    /// <summary>
    /// Gets the name of the stormmod that this file or directory resides in.
    /// </summary>
    public required string StormModName { get; init; }

    /// <summary>
    /// Gets the relative path of the file or directory.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the type of the path or directory.
    /// </summary>
    public required StormPathType PathType { get; init; }

    /// <inheritdoc/>
    public virtual bool Equals(StormPath? other)
    {
        if (other is null)
            return false;

        return StormModName.AsSpan().Equals(other.StormModName.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
            Path.AsSpan().Equals(other.Path.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
            PathType.Equals(other.PathType);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(StormModName?.ToUpperInvariant(), Path.ToUpperInvariant(), PathType);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path;
    }
}
