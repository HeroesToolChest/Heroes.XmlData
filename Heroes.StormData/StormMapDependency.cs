namespace Heroes.StormData;

/// <summary>
/// Contains data for a <c>stormmod</c> that is part of a loaded <c>mapmod</c>, either one of the map's dependencies or the map itself.
/// </summary>
public sealed record StormMapDependency
{
    /// <summary>
    /// Gets the name of the <c>stormmod</c>. For the map itself, this is the map's title (<c>enUS</c>).
    /// Otherwise, it is the directory name without its extension (e.g. <c>volskayasound</c>).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the path relative to the root directory (<see cref="HeroesLoader.RootDirectory"/>).
    /// For a dependency, this is the path of its <c>.stormmod</c> directory. For the map itself, this is the path of its <c>.s2ma</c> file.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the map's own <c>stormmod</c> (its <c>.s2ma</c> file) instead of one of its dependencies.
    /// </summary>
    public bool IsMap { get; init; }
}
