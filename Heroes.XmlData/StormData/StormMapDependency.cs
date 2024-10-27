namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains data for a mod map dependency.
/// </summary>
public class StormMapDependency
{
    /// <summary>
    /// Gets the name of the storm map in enUS.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the relative path.
    /// </summary>
    public required string DirectoryPath { get; init; }
}
