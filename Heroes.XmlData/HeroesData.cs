using Heroes.XmlData.GameStrings;
using Heroes.XmlData.StormData;

namespace Heroes.XmlData;

/// <summary>
/// Contains the methods and properties to access the xml and gamestring data.
/// </summary>
public class HeroesData
{
    private readonly IStormStorage _stormStorage;

    private readonly IStormStorageCacheData _stormStorageCacheData;

    //private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    //private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private StormLocale? _heroesLocalization;

    internal HeroesData(IStormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    internal HeroesData(IStormStorageCacheData stormStorageCacheData)
    {
        _stormStorageCacheData = stormStorageCacheData;
    }

    /// <summary>
    /// Gets the current gamestring localization.
    /// </summary>
    public StormLocale? HeroesLocalization => _heroesLocalization;

    //public int? GetBuildNumber() => _stormStorage.GetBuildId();

    /// <summary>
    /// Checks if the gamestring id exists.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <returns><see langword="true"/> if the gamestring is found, otherwise <see langword="false"/>.</returns>
    public bool IsGameStringExists(ReadOnlySpan<char> id)
    {
        return IsGameStringExists(id.ToString());
    }

    /// <summary>
    /// Checks if the gamestring id exists.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <returns><see langword="true"/> if the gamestring is found, otherwise <see langword="false"/>.</returns>
    public bool IsGameStringExists(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _stormStorage.StormCustomCache.GameStringsById.ContainsKey(id) ||
            _stormStorage.StormMapCache.GameStringsById.ContainsKey(id) ||
            _stormStorage.StormCache.GameStringsById.ContainsKey(id);
    }

    /// <summary>
    /// Gets the unparsed gamestring from a given id.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <returns>The <see cref="GameStringText"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    public GameStringText GetGameString(ReadOnlySpan<char> id)
    {
        return GetGameString(id.ToString());
    }

    /// <summary>
    /// Gets the unparsed gamestring from a given id.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <returns>The <see cref="GameStringText"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    public GameStringText GetGameString(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.GameStringsById.TryGetValue(id, out GameStringText? gameStringText))
            return gameStringText;

        // map cache second
        if (_stormStorage.StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            return gameStringText;

        return _stormStorage.StormCache.GameStringsById[id];
    }

    /// <summary>
    /// Looks up the unparsed gamestring from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">A character span that contains the gamestring id.</param>
    /// <param name="gameStringText">The returning <see cref="GameStringText"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out GameStringText? gameStringText)
    {
        return TryGetGameString(id.ToString(), out gameStringText);
    }

    /// <summary>
    /// Looks up the unparsed gamestring from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">The gamestring id.</param>
    /// <param name="gameStringText">The returning <see cref="GameStringText"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    public bool TryGetGameString(string id, [NotNullWhen(true)] out GameStringText? gameStringText)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        // map cache second
        if (_stormStorage.StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        if (_stormStorage.StormCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        return false;
    }

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
    public bool IsLevelScalingEntryExists(string catalog, string entry, string field)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        return _stormStorage.StormCustomCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) ||
            _stormStorage.StormMapCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) ||
            _stormStorage.StormCache.ScaleValueByEntry.ContainsKey(levelScalingEntry);
    }

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
    public StormStringValue GetLevelScalingEntryExists(string catalog, string entry, string field)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out StormStringValue? stormStringValue))
            return stormStringValue;

        if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return stormStringValue;

        return _stormStorage.StormCache.ScaleValueByEntry[levelScalingEntry];
    }

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
    public bool TryGetLevelScalingEntryExists(string catalog, string entry, string field, out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        if (_stormStorage.StormCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if the style name exists.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    public bool IsStormStyleHexColorValueExists(ReadOnlySpan<char> name)
    {
        return IsStormStyleHexColorValueExists(name.ToString());
    }

    /// <summary>
    /// Checks if the style name of a StyleFile exists.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    public bool IsStormStyleHexColorValueExists(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _stormStorage.StormCustomCache.StormStyleHexColorValueByName.ContainsKey(name) ||
            _stormStorage.StormMapCache.StormStyleHexColorValueByName.ContainsKey(name) ||
            _stormStorage.StormCache.StormStyleHexColorValueByName.ContainsKey(name);
    }

    /// <summary>
    /// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <returns>The <see cref="StormStringValue"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    public StormStringValue GetStormStyleHexColorValue(ReadOnlySpan<char> name)
    {
        return GetStormStyleHexColorValue(name.ToString());
    }

    /// <summary>
    /// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <returns>The <see cref="StormStringValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    public StormStringValue GetStormStyleHexColorValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        // custom cache always first
        if (_stormStorage.StormCustomCache.StormStyleHexColorValueByName.TryGetValue(name, out StormStringValue? stormStringValue))
            return stormStringValue;

        // map cache second
        if (_stormStorage.StormMapCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return stormStringValue;

        return _stormStorage.StormCache.StormStyleHexColorValueByName[name];
    }

    /// <summary>
    /// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">A character span that contains the style or constanst name.</param>
    /// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, out StormStringValue? stormStringValue)
    {
        return TryGetStormStyleHexColorValue(name.ToString(), out stormStringValue);
    }

    /// <summary>
    /// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="name">The style or constanst name.</param>
    /// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    /// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetStormStyleHexColorValue(string name, out StormStringValue? stormStringValue)
    {
        return _stormStorageCacheData.TryGetStormStyleHexColorValue(name, out stormStringValue);
    }

    /// <summary>
    /// Checks if the constant id exists.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <returns><see langword="true"/> if the constant id is found, otherwise <see langword="false"/>.</returns>
    public bool IsConstantXElementExists(ReadOnlySpan<char> id)
    {
        return IsConstantXElementExists(id.ToString());
    }

    /// <summary>
    /// Checks if the constant id exists.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <returns><see langword="true"/> if the constant id is found, otherwise <see langword="false"/>.</returns>
    public bool IsConstantXElementExists(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _stormStorage.StormCustomCache.ConstantXElementById.ContainsKey(id) ||
            _stormStorage.StormMapCache.ConstantXElementById.ContainsKey(id) ||
            _stormStorage.StormCache.ConstantXElementById.ContainsKey(id);
    }

    /// <summary>
    /// Gets the <see cref="XElement"/> from the constant id.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <returns>The <see cref="StormXElementValuePath"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    public StormXElementValuePath GetConstantXElement(ReadOnlySpan<char> id)
    {
        return GetConstantXElement(id.ToString());
    }

    /// <summary>
    /// Gets the <see cref="XElement"/> from the constant id.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <returns>The <see cref="StormXElementValuePath"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> was not found.</exception>
    public StormXElementValuePath GetConstantXElement(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.ConstantXElementById.TryGetValue(id, out StormXElementValuePath? stormXElementValue))
            return stormXElementValue;

        // map cache second
        if (_stormStorage.StormMapCache.ConstantXElementById.TryGetValue(id, out stormXElementValue))
            return stormXElementValue;

        return _stormStorage.StormCache.ConstantXElementById[id];
    }

    /// <summary>
    /// Looks up a constant element from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">A character span that contains the constant id.</param>
    /// <param name="stormXElementValue">The returning <see cref="StormXElementValuePath"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetConstantXElement(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValue)
    {
        return _stormStorageCacheData.TryGetConstantXElementById(id, out stormXElementValue);
    }

    /// <summary>
    /// Looks up a constant element from the given <paramref name="id"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="id">The constant id.</param>
    /// <param name="stormXElementValue">The returning <see cref="StormXElementValuePath"/> if <paramref name="id"/> is found.</param>
    /// <returns><see langword="true"/> if the value was found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    public bool TryGetConstantXElement(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValue)
    {
        return _stormStorageCacheData.TryGetConstantXElementById(id, out stormXElementValue);
    }

    public bool TryGetStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return _stormStorageCacheData.TryGetStormElementsByDataObjectType(dataObjectType, id, out stormElement);
    }

    public bool TryGetStormElement(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return _stormStorageCacheData.TryGetStormElementsByDataObjectType(dataObjectType, id, out stormElement);
    }

    public bool TryGetLevelScalingValue(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        return _stormStorageCacheData.TryGetLevelScalingArrayElement(catalog, entry, field, out stormStringValue);
    }

    public bool TryGetLevelScalingValue(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        return _stormStorageCacheData.TryGetLevelScalingArrayElement(catalog, entry, field, out stormStringValue);
    }

    public double EvaluateConstantElement(XElement constElement)
    {
        return _stormStorageCacheData.GetValueFromConstElementAsNumber(constElement);
    }

    public TooltipDescription ParseGameString(string gameString, StormLocale stormLocale)
    {
        return new TooltipDescription(GameStringParser.ParseTooltipDescription(gameString, this), stormLocale);
    }

    internal void SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
