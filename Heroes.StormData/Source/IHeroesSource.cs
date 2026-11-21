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

    /// <summary>
    /// Unloads the current map <c>stormmods</c>, then loads the map <c>stormmods</c> of the given map title.
    /// The depot cache must be loaded first.
    /// </summary>
    /// <param name="mapTitle">The map's title (enUS). Is not case-sensitive.</param>
    /// <returns><see langword="true"/> if the map was found and loaded; otherwise <see langword="false"/> and the current map <c>stormmods</c> are left loaded.</returns>
    bool LoadStormMapMod(string mapTitle);

    /// <summary>
    /// Unloads the map <c>stormmods</c> and all their data.
    /// </summary>
    void UnloadStormMapMod();

    /// <summary>
    /// Gets the titles (enUS) of the maps found in the depot cache. This is empty until the depot cache is loaded.
    /// </summary>
    /// <returns>A collection of map titles.</returns>
    IEnumerable<string> GetMapTitles();
}
