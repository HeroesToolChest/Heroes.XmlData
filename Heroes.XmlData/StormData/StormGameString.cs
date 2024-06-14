namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an unparsed gamestring.
/// </summary>
public class StormGameString
{
    private readonly List<string> _paths = [];

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
    /// Gets a collection of file paths where the gamestring resides. <see cref="Value"/> is set from the last path.
    /// </summary>
    public IReadOnlyList<string> Paths => _paths;

    internal void AddPath(string path)
    {
        _paths.Add(path);
    }
}
