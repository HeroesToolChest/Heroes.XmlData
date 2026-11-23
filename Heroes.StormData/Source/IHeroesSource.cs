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

    /// <summary>
    /// Gets the <c>stormmods</c> of the loaded map in load order, the dependencies first and the map's own <c>stormmod</c> last. Empty if no map is loaded.
    /// </summary>
    IReadOnlyList<IStormMod> StormMapMods { get; }

    void LoadStormMods();

    void LoadDepotCache();

    /// <summary>
    /// Removes all the loaded gamestrings, then loads the gamestrings of a locale from the base, map, and custom <c>stormmods</c>.
    /// If loading throws, all the gamestrings are removed again, so none from part of the locale are left.
    /// </summary>
    /// <param name="stormLocale">The locale of the gamestrings to load.</param>
    void LoadGameStrings(StormLocale stormLocale);

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

    /// <summary>
    /// Gets the <see cref="S2MAProperties"/> of a map from the depot cache. Only maps that have a matching <see cref="S2MVProperties"/> are found.
    /// </summary>
    /// <param name="mapTitle">The map's title (enUS). Is case-insensitive.</param>
    /// <param name="s2maProperties">When this method returns, contains the <see cref="S2MAProperties"/> of the map, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the map was found; otherwise <see langword="false"/>, which is always the case until the depot cache is loaded.</returns>
    bool TryGetS2MAProperties(string mapTitle, [NotNullWhen(true)] out S2MAProperties? s2maProperties);

    /// <summary>
    /// Determines if a file, or an entry in an MPQ archive file, exists.
    /// </summary>
    /// <param name="path">
    /// The file path, relative to the <c>mods</c> directory (it may start with <c>mods</c>) or starting with the <see cref="ModsDirectoryPath"/>.
    /// For a file source, it may also be an absolute path.
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/>, or <see langword="null"/> or empty if <paramref name="path"/> is the file itself.</param>
    /// <returns>
    /// <see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="path"/> being <see langword="null"/> or whitespace,
    /// <paramref name="path"/> being a directory, and a file at <paramref name="path"/> that is not an MPQ archive when <paramref name="mpqEntryPath"/> is given.
    /// </returns>
    bool FileExists(string? path, string? mpqEntryPath);

    /// <summary>
    /// Opens a file, or an entry in an MPQ archive file, for reading.
    /// </summary>
    /// <param name="path">
    /// The file path, relative to the <c>mods</c> directory (it may start with <c>mods</c>) or starting with the <see cref="ModsDirectoryPath"/>.
    /// For a file source, it may also be an absolute path.
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/>, or <see langword="null"/> or empty if <paramref name="path"/> is the file itself.</param>
    /// <returns>The <see cref="Stream"/> of the file.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="FileNotFoundException">
    /// The file was not found (in the same cases that <see cref="FileExists(string?, string?)"/> returns <see langword="false"/>).
    /// </exception>
    /// <exception cref="MpqHeroesToolException">The entry exists, but could not be decompressed.</exception>
    Stream GetFile(string path, string? mpqEntryPath);

    /// <summary>
    /// Opens a file, or an entry in an MPQ archive file, for reading, if it exists.
    /// </summary>
    /// <param name="path">
    /// The file path, relative to the <c>mods</c> directory (it may start with <c>mods</c>) or starting with the <see cref="ModsDirectoryPath"/>.
    /// For a file source, it may also be an absolute path.
    /// </param>
    /// <param name="mpqEntryPath">The path of an entry in the MPQ archive file at <paramref name="path"/>, or <see langword="null"/> or empty if <paramref name="path"/> is the file itself.</param>
    /// <param name="stream">When this method returns, contains the <see cref="Stream"/> of the file, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the file exists; otherwise <see langword="false"/>, in the same cases as <see cref="FileExists(string?, string?)"/>.
    /// </returns>
    /// <exception cref="MpqHeroesToolException">The entry exists, but could not be decompressed.</exception>
    bool TryGetFile(string? path, string? mpqEntryPath, [NotNullWhen(true)] out Stream? stream);
}
