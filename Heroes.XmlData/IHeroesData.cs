namespace Heroes.XmlData;

/// <summary>
/// Interface for the methods and properties to access the xml and gamestring data.
/// </summary>
public interface IHeroesData
{
    /// <summary>
    /// Gets the current gamestring localization.
    /// </summary>
    StormLocale? HeroesLocalization { get; }

    /// <summary>
    /// Checks if the gamestring id exists.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <returns><see langword="true"/> if the gamestring is found, otherwise <see langword="false"/>.</returns>
    bool IsGameStringExists(ReadOnlySpan<char> id);

    /// <summary>
    /// Checks if the gamestring id exists.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <returns><see langword="true"/> if the gamestring is found, otherwise <see langword="false"/>.</returns>
    bool IsGameStringExists(string id);

    /// <summary>
    /// Gets the gamestring from a given id.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <returns>The <see cref="GameStringText"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    GameStringText GetGameString(ReadOnlySpan<char> id);

    /// <summary>
    /// Gets the gamestring from a given id.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <returns>The <see cref="GameStringText"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    GameStringText GetGameString(string id);

    /// <summary>
    /// Looks up a gamestring from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <param name="gameStringText">The returning <see cref="GameStringText"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    bool TryGetGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out GameStringText? gameStringText);

    /// <summary>
    /// Looks up a gamestring from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <param name="gameStringText">The returning <see cref="GameStringText"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    bool TryGetGameString(string id, [NotNullWhen(true)] out GameStringText? gameStringText);

    /// <summary>
    /// Checks if the level scaling entry exists.
    /// </summary>
    /// <param name="catalog">The value of the Catalog element.</param>
    /// <param name="entry">The value of the Entry element.</param>
    /// <param name="field">The value of the Field element.</param>
    /// <returns><see langword="true"/> if the entry is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    bool IsLevelScalingEntryExists(string catalog, string entry, string field);

    /// <summary>
    /// Gets the value of the level scaling entry.
    /// </summary>
    /// <param name="catalog">The value of the Catalog element.</param>
    /// <param name="entry">The value of the Entry element.</param>
    /// <param name="field">The value of the Field element.</param>
    /// <returns>The <see cref="StormStringValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">The combination of <paramref name="catalog"/>, <paramref name="entry"/>, and <paramref name="field"/> was not found.</exception>
    StormStringValue GetLevelScalingEntryExists(string catalog, string entry, string field);

    /// <summary>
    /// Looks up a level scaling entry from the given parameters, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="catalog">The value of the Catalog element.</param>
    /// <param name="entry">The value of the Entry element.</param>
    /// <param name="field">The value of the Field element.</param>
    /// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the entry is found.</param>
    /// <returns><see langword="true"/> if the entry is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    bool TryGetLevelScalingEntryExists(string catalog, string entry, string field, out StormStringValue? stormStringValue);

    /// <summary>
    /// Checks if the style name exists.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    bool IsStormStyleHexColorValueExists(ReadOnlySpan<char> name);

    /// <summary>
    /// Checks if the style name of a StyleFile exists.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    bool IsStormStyleHexColorValueExists(string name);

    /// <summary>
    /// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <returns>The <see cref="StormStringValue"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    StormStringValue GetStormStyleHexColorValue(ReadOnlySpan<char> name);

    /// <summary>
    /// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <returns>The <see cref="StormStringValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    StormStringValue GetStormStyleHexColorValue(string name);

    /// <summary>
    /// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, out StormStringValue? stormStringValue);

    /// <summary>
    /// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    bool TryGetStormStyleHexColorValue(string name, out StormStringValue? stormStringValue);

    /// <summary>
    /// Checks if the constant id exists.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <returns><see langword="true"/> if the constant id is found, otherwise <see langword="false"/>.</returns>
    bool IsConstantElementExists(ReadOnlySpan<char> id);

    /// <summary>
    /// Checks if the constant id exists.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <returns><see langword="true"/> if the constant id is found, otherwise <see langword="false"/>.</returns>
    bool IsConstantElementExists(string id);

    /// <summary>
    /// Gets the <see cref="XElement"/> from the constant id.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <returns>The <see cref="StormXElementValue"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    StormXElementValue GetConstantElement(ReadOnlySpan<char> id);

    /// <summary>
    /// Gets the <see cref="XElement"/> from the constant id.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <returns>The <see cref="StormXElementValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    StormXElementValue GetConstantElement(string id);

    /// <summary>
    /// Looks up a constant element from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <param name="stormXElementValue">The returning <see cref="StormXElementValue"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    bool TryGetConstantElement(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValue? stormXElementValue);

    /// <summary>
    /// Looks up a constant element from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <param name="stormXElementValue">The returning <see cref="StormXElementValue"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    bool TryGetConstantElement(string id, [NotNullWhen(true)] out StormXElementValue? stormXElementValue);

    /// <summary>
    /// Checks if the <see cref="XElement"/> name exists.
    /// </summary>
    /// <param name="name">The <see cref="XElement"/> name.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    bool IsElementExists(ReadOnlySpan<char> name);

    /// <summary>
    /// Checks if the <see cref="XElement"/> name exists.
    /// </summary>
    /// <param name="name">The <see cref="XElement"/> name.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    bool IsElementExists(string name);

    /// <summary>
    /// Gets the elements from the given <see cref="XElement"/> name.
    /// </summary>
    /// <param name="name">A character span that contains the <see cref="XElement"/> name.</param>
    /// <returns>A collection of <see cref="StormXElementValue"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    List<StormXElementValue> GetElements(ReadOnlySpan<char> name);

    /// <summary>
    /// Gets the elements from the given <see cref="XElement"/> name.
    /// </summary>
    /// <param name="name">The <see cref="XElement"/> name.</param>
    /// <returns>A collection of <see cref="StormXElementValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    List<StormXElementValue> GetElements(string name);

    /// <summary>
    /// Looks up a collection of <see cref="XElement"/>s from the given <see cref="XElement"/> <paramref name="name"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">A character span that contains the <see cref="XElement"/> name.</param>
    /// <param name="stormXElementValues">The returning collection of <see cref="StormXElementValue"/>s if <paramref name="name"/> is found.</param>
    /// <returns><see langword="true"/> if the element was found; otherwise <see langword="false"/>.</returns>
    bool TryGetElements(ReadOnlySpan<char> name, [NotNullWhen(true)] out List<StormXElementValue>? stormXElementValues);

    /// <summary>
    /// Looks up a collection of <see cref="XElement"/>s from the given <see cref="XElement"/> <paramref name="name"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">The <see cref="XElement"/> name.</param>
    /// <param name="stormXElementValues">The returning collection of <see cref="StormXElementValue"/>s if <paramref name="name"/> is found.</param>
    /// <returns><see langword="true"/> if the element was found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    bool TryGetElements(string name, [NotNullWhen(true)] out List<StormXElementValue>? stormXElementValues);

    internal void SetHeroesLocalization(StormLocale stormLocale);
}
