namespace Heroes.StormData.Source;

internal interface IHeroesSource
{
    /// <summary>
    /// Gets the full path to the <c>mods</c> directory.
    /// </summary>
    string ModsDirectoryPath { get; }

    /// <summary>
    /// Gets the container that holds all data across all the mods.
    /// </summary>
    SourceContainer SourceContainer { get; }

    IStormModFactory StormModFactory { get; }

    void LoadStormMods();

    void LoadDepotCache();

    /// <summary>
    /// Removes all the loaded gamestrings, then loads the gamestrings of a locale from the base, map, and custom <c>stormmods</c>.
    /// </summary>
    /// <param name="stormLocale">The locale of the gamestrings to load.</param>
    void LoadGamestrings(StormLocale stormLocale);

    void LoadStormMapMod(string mapTitle);
}
