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
    /// Gets a value indicating whether this mod is a map mod.
    /// </summary>
    bool IsMapMod { get; }

    void LoadStormData();

    void LoadStormGameStrings(StormLocale stormLocale);

    IEnumerable<IStormMod> GetStormMapMods(S2MAProperties s2maProperties);

    List<IStormMod> LoadDocumentInfoFile();
}
