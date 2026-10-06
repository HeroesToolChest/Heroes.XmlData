namespace Heroes.StormData.StormMods;

internal interface IStormMod
{
    /// <summary>
    /// Gets the name of the <c>stormmod</c>, which is the name of the directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the relative path of the <c>stormmod</c> directory from the root (<c>mods</c>) directory. No leading root directory.
    /// </summary>
    string RelativeStormModPath { get; }

    /// <summary>
    /// Gets the full path of the <c>stormmod</c> directory.
    /// </summary>
    public string DirectoryPath { get; }

    void LoadStormFiles();
}
