namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents a storm file. Contains all the paths of where the file resides.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormFile
{
    private readonly List<StormPath> _stormPaths = [];

    /// <summary>
    /// Gets the path relative to the root directory.
    /// </summary>
    public string Path => StormPaths[^1].Path;

    /// <summary>
    /// Gets a collection of paths where the file resides. The <see cref="Path"/> is from the last entry.
    /// </summary>
    public IReadOnlyList<StormPath> StormPaths => _stormPaths.AsReadOnly();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return $"{{{System.IO.Path.GetFileName(Path)}}}";
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Path;
    }

    internal void AddPath(StormPath stormPath)
    {
        _stormPaths.Add(stormPath);
    }
}
