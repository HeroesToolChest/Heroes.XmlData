namespace Heroes.XmlData.StormMods;

internal interface IStormMod
{
    /// <summary>
    /// Gets the name of this stormmod. Not necessarily the file name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the inner path, after the "mods" and before the "base" directory.
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    /// Gets the type of the mod.
    /// </summary>
    StormModType StormModType { get; }

    IStormModStorage StormModStorage { get; }

    void LoadStormData();

    void LoadStormGameStrings(StormLocale stormLocale);

    IEnumerable<IStormMod> GetStormMapMods(S2MAProperties s2maProperties);

    List<IStormMod> LoadDocumentInfoFile();
}
