namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents a storm asset file such as an image file.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormAssetFile
{
    private readonly List<StormPath> _stormPaths = [];

    /// <summary>
    /// Gets the path relative to the root directory.
    /// </summary>
    public string Path => StormPaths[^1].Path;

    /// <summary>
    /// Gets a collection of paths where the asset file resides. The <see cref="Path"/> is from the last entry.
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
