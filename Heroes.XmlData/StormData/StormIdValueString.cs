namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents a id=value pair found in a text file.
/// </summary>
public class StormIdValueString
{
    private readonly List<StormPath> _stormPaths = [];

    internal StormIdValueString(string id, string value)
    {
        Id = id;
        Value = value;
    }

    /// <summary>
    /// Gets the id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; internal set; }

    /// <summary>
    /// Gets a collection of paths where the id=value resides. <see cref="Value"/> is set from the last path.
    /// </summary>
    public IReadOnlyList<StormPath> StormPaths => _stormPaths.AsReadOnly();

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
