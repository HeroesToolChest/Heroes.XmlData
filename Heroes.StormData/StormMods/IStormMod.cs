namespace Heroes.StormData.StormMods;

/// <summary>
/// Represents a <c>stormmod</c>, a directory of game data that is loaded into a <see cref="StormModContainer"/>.
/// </summary>
internal interface IStormMod
{
    /// <summary>
    /// Gets the name of the <c>stormmod</c>, which is the directory name without its extension
    /// (e.g. <c>heroesdata</c> for <c>heroesdata.stormmod</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the path of the <c>stormmod</c> directory relative to the <c>mods</c> directory
    /// (e.g. <c>heromods\abathur.stormmod</c>).
    /// </summary>
    string RelativeStormModPath { get; }

    /// <summary>
    /// Gets the full path of the <c>stormmod</c> directory, with no trailing directory separator.
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    /// Gets the container that holds the files and elements loaded from this <c>stormmod</c>.
    /// </summary>
    StormModContainer ModContainer { get; }

    void LoadStormFiles();
}
