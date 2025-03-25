namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents a storm file. Contains all the paths of where the file resides.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormFile
{
    private readonly List<StormPath> _stormPaths = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StormFile"/> class.
    /// </summary>
    /// <param name="stormPath">The location of the path.</param>
    public StormFile(StormPath stormPath)
    {
        ArgumentNullException.ThrowIfNull(stormPath);

        AddPath(stormPath);
    }

    /// <summary>
    /// Gets the paths of this file.
    /// </summary>
    public StormPath StormPath => StormPaths[^1];

    /// <summary>
    /// Gets a collection of paths where the file resides.
    /// </summary>
    public IReadOnlyList<StormPath> StormPaths => _stormPaths.AsReadOnly();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return $"{{{Path.GetFileName(StormPath.Path)}}}";
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return StormPath.Path;
    }

    /// <summary>
    /// Adds an additional path where the file file resides in.
    /// </summary>
    /// <param name="stormPath">The location of the path.</param>
    public void AddPath(StormPath stormPath)
    {
        _stormPaths.Add(stormPath);
    }
}
