namespace Heroes.StormData.DepotCache.Maps;

/// <summary>
/// The properties shared by the <c>.s2mv</c> and <c>.s2ma</c> files.
/// </summary>
internal abstract class S2MapPropertiesBase
{
    /// <summary>
    /// Gets the <c>stormmod</c> dependencies of the map.
    /// </summary>
    public List<MapDependency> MapDependencies { get; init; } = [];

    /// <summary>
    /// Gets the modifiable dependencies of the map.
    /// </summary>
    public List<string> ModifiableDependencies { get; init; } = [];

    /// <summary>
    /// Gets the icon file from the document info.
    /// </summary>
    public string DocInfoIconFile { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the file, relative to the <c>mods</c> directory.
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets a key built from the dependencies and the icon file. A <see cref="S2MAProperties"/> and its matching <see cref="S2MVProperties"/> have the same key.
    /// </summary>
    /// <returns>The dependency key.</returns>
    public string GetDependencyKey()
    {
        StringBuilder stringBuilder = new();

        foreach (MapDependency mapDependency in MapDependencies)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"dep:{mapDependency.BnetName}/{mapDependency.BnetVersionMajor}.{mapDependency.BnetVersionMinor}/{mapDependency.BnetNamespace},{mapDependency.LocalFile}\n");
        }

        foreach (string modifiableDependency in ModifiableDependencies)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"mod:{modifiableDependency}\n");
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"icon:{DocInfoIconFile}");

        return stringBuilder.ToString();
    }
}
