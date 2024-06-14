namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an unparsed gamestring.
/// </summary>
public class StormGameString
{
    private readonly List<StormPath> _stormPaths = [];

    internal StormGameString(string id, string value)
    {
        Id = id;
        Value = value;
    }

    /// <summary>
    /// Gets the id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the unparsed gamestring.
    /// </summary>
    public string Value { get; internal set; }

    /// <summary>
    /// Gets a collection of paths where the gamestring resides. <see cref="Value"/> is set from the last path.
    /// </summary>
    public IReadOnlyList<StormPath> StormPaths => _stormPaths;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Id}={Value}";
    }

    internal void AddPath(StormPath stormPath)
    {
        _stormPaths.Add(stormPath);
    }
}
