using Heroes.XmlData.GameStrings;
using System.Diagnostics;

namespace Heroes.XmlData;

/// <summary>
/// Contains the methods and properties to access the xml and gamestring data.
/// </summary>
public class HeroesData
{
    private readonly IStormStorage _stormStorage;

    //private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    //private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private StormLocale? _heroesLocalization;

    internal HeroesData(IStormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    internal HeroesData(StormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    /// <summary>
    /// Gets the current gamestring localization.
    /// </summary>
    public StormLocale? HeroesLocalization => _heroesLocalization;

    [DebuggerHidden]
    internal IStormStorage StormStorage => _stormStorage;

    //public int? GetBuildNumber() => _stormStorage.GetBuildId();

    public List<GameStringText> Test()
    {
        return _stormStorage.Test();
    }

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

    ///// <summary>
    ///// Checks if the level scaling entry exists.
    ///// </summary>
    ///// <param name="catalog">The value of the Catalog element.</param>
    ///// <param name="entry">The value of the Entry element.</param>
    ///// <param name="field">The value of the Field element.</param>
    ///// <returns><see langword="true"/> if the entry is found, otherwise <see langword="false"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    //public bool IsLevelScalingEntryExists(string catalog, string entry, string field)
    //{
    //    ArgumentNullException.ThrowIfNull(catalog);
    //    ArgumentNullException.ThrowIfNull(entry);
    //    ArgumentNullException.ThrowIfNull(field);

    //    LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

    //    return _stormStorage.StormCustomCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) ||
    //        _stormStorage.StormMapCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) ||
    //        _stormStorage.StormCache.ScaleValueByEntry.ContainsKey(levelScalingEntry);
    //}

    ///// <summary>
    ///// Gets the value of the level scaling entry.
    ///// </summary>
    ///// <param name="catalog">The value of the Catalog element.</param>
    ///// <param name="entry">The value of the Entry element.</param>
    ///// <param name="field">The value of the Field element.</param>
    ///// <returns>The <see cref="StormStringValue"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    ///// <exception cref="KeyNotFoundException">The combination of <paramref name="catalog"/>, <paramref name="entry"/>, and <paramref name="field"/> was not found.</exception>
    //public StormStringValue GetLevelScalingEntryExists(string catalog, string entry, string field)
    //{
    //    ArgumentNullException.ThrowIfNull(catalog);
    //    ArgumentNullException.ThrowIfNull(entry);
    //    ArgumentNullException.ThrowIfNull(field);

    //    LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

    //    if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out StormStringValue? stormStringValue))
    //        return stormStringValue;

    //    if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
    //        return stormStringValue;

    //    return _stormStorage.StormCache.ScaleValueByEntry[levelScalingEntry];
    //}

    ///// <summary>
    ///// Looks up a level scaling entry from the given parameters, returning a value that indicates whether such value exists.
    ///// </summary>
    ///// <param name="catalog">The value of the Catalog element.</param>
    ///// <param name="entry">The value of the Entry element.</param>
    ///// <param name="field">The value of the Field element.</param>
    ///// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the entry is found.</param>
    ///// <returns><see langword="true"/> if the entry is found, otherwise <see langword="false"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/>.</exception>
    ///// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
    //public bool TryGetLevelScalingEntryExists(string catalog, string entry, string field, out StormStringValue? stormStringValue)
    //{
    //    ArgumentNullException.ThrowIfNull(catalog);
    //    ArgumentNullException.ThrowIfNull(entry);
    //    ArgumentNullException.ThrowIfNull(field);

    //    LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

    //    if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
    //        return true;

    //    if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
    //        return true;

    //    if (_stormStorage.StormCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
    //        return true;

    //    return false;
    //}

    ///// <summary>
    ///// Checks if the style name exists.
    ///// </summary>
    ///// <param name="name">A character span that contains the style or constanst name.</param>
    ///// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    //public bool IsStormStyleHexColorValueExists(ReadOnlySpan<char> name)
    //{
    //    return IsStormStyleHexColorValueExists(name.ToString());
    //}

    ///// <summary>
    ///// Checks if the style name of a StyleFile exists.
    ///// </summary>
    ///// <param name="name">The style or constanst name.</param>
    ///// <returns><see langword="true"/> if the style name is found, otherwise <see langword="false"/>.</returns>
    //public bool IsStormStyleHexColorValueExists(string name)
    //{
    //    ArgumentNullException.ThrowIfNull(name);

    //    return _stormStorage.StormCustomCache.StormStyleHexColorValueByName.ContainsKey(name) ||
    //        _stormStorage.StormMapCache.StormStyleHexColorValueByName.ContainsKey(name) ||
    //        _stormStorage.StormCache.StormStyleHexColorValueByName.ContainsKey(name);
    //}

    ///// <summary>
    ///// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    ///// </summary>
    ///// <param name="name">A character span that contains the style or constanst name.</param>
    ///// <returns>The <see cref="StormStringValue"/>.</returns>
    ///// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    //public StormStringValue GetStormStyleHexColorValue(ReadOnlySpan<char> name)
    //{
    //    return GetStormStyleHexColorValue(name.ToString());
    //}

    ///// <summary>
    ///// Gets the value from the style name of a StyleFile. The value could be another name that required another lookup.
    ///// </summary>
    ///// <param name="name">The style or constanst name.</param>
    ///// <returns>The <see cref="StormStringValue"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    ///// <exception cref="KeyNotFoundException"><paramref name="name"/> was not found.</exception>
    //public StormStringValue GetStormStyleHexColorValue(string name)
    //{
    //    ArgumentNullException.ThrowIfNull(name);

    //    // custom cache always first
    //    if (_stormStorage.StormCustomCache.StormStyleHexColorValueByName.TryGetValue(name, out StormStringValue? stormStringValue))
    //        return stormStringValue;

    //    // map cache second
    //    if (_stormStorage.StormMapCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
    //        return stormStringValue;

    //    return _stormStorage.StormCache.StormStyleHexColorValueByName[name];
    //}

    ///// <summary>
    ///// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    ///// </summary>
    ///// <param name="name">A character span that contains the style or constanst name.</param>
    ///// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    ///// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    //public bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, out StormStringValue? stormStringValue)
    //{
    //    return TryGetStormStyleHexColorValue(name.ToString(), out stormStringValue);
    //}

    ///// <summary>
    ///// Looks up a value from the style name of a StyleFile, returning a value that indicates whether such value exists.
    ///// </summary>
    ///// <param name="name">The style or constanst name.</param>
    ///// <param name="stormStringValue">The returning <see cref="StormStringValue"/> if the name is found.</param>
    ///// <returns><see langword="true"/> if the name is found, otherwise <see langword="false"/>.</returns>
    ///// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    //public bool TryGetStormStyleHexColorValue(string name, out StormStringValue? stormStringValue)
    //{
    //    return _stormStorage.TryGetStormStyleHexColorValue(name, out stormStringValue);
    //}

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IList<string>? GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType)?.ToList();
    }

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IList<string>? GetElementTypesByDataObjectType(string dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType)?.ToList();
    }

    /// <summary>
    /// Gets the data object type (e.g. Effect) associated with the element type (e.g. CEffectDamage).
    /// </summary>
    /// <param name="elementType">A character span that contains the name of the element.</param>
    /// <returns>The data object type or <see langword="null"/> if not found.</returns>
    public string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType)
    {
        return _stormStorage.GetDataObjectTypeByElementType(elementType);
    }

    /// <summary>
    /// Gets the data object type (e.g. Effect) associated with the element type (e.g. CEffectDamage).
    /// </summary>
    /// <param name="elementType">The name of the element.</param>
    /// <returns>The data object type or <see langword="null"/> if not found.</returns>
    public string? GetDataObjectTypeByElementType(string elementType)
    {
        return _stormStorage.GetDataObjectTypeByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that does not have an id attribute value.
    /// </summary>
    /// <param name="elementType">A character span that contains the name of the element.</param>
    /// <returns>A <see cref="StormElement"/>.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> elementType)
    {
        return _stormStorage.GetStormElementByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that does not have an id attribute value.
    /// </summary>
    /// <param name="elementType">The name of the element.</param>
    /// <returns>A <see cref="StormElement"/>.</returns>
    public StormElement? GetStormElement(string elementType)
    {
        return _stormStorage.GetStormElementByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that has an id attribute value.
    /// </summary>
    /// <param name="id">A character span that contains the id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/>.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that has an id attribute value.
    /// </summary>
    /// <param name="id">The id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/>.</returns>
    public StormElement? GetStormElement(string id, string dataObjectType)
    {
        return _stormStorage.GetStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that is created from a level scaling array element that contains the scaling attribute value.
    /// </summary>
    /// <param name="id">A character span that contains the id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> that contains a scaling value.</returns>
    public StormElement? GetScaleValueStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetScaleValueStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that is created from a level scaling array element that contains the scaling attribute value.
    /// </summary>
    /// <param name="id">The id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> that contains a scaling value.</returns>
    public StormElement? GetScaleValueStormElement(string id, string dataObjectType)
    {
        return _stormStorage.GetScaleValueStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <param name="id">The value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/>.</returns>
    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">A character span that contains the value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/>.</returns>
    public StormElement? GetCompleteStormElement(string dataObjectType, string id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a storm style Constant element.
    /// </summary>
    /// <param name="name">The name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/>.</returns>
    public StormElement? GetStormStyleConstantStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleConstantsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Constant element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/>.</returns>
    public StormElement? GetStormStyleConstantStormElement(string name)
    {
        return _stormStorage.GetStormStyleConstantsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">The name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/>.</returns>
    public StormElement? GetStormStyleStyleStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleStylesByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/>.</returns>
    public StormElement? GetStormStyleStyleStormElement(string name)
    {
        return _stormStorage.GetStormStyleStylesByName(name);
    }

    //private StormElement? MergeUpStormElement(string dataObjectType, string? id, ElementType currentElementType)
    //{
    //    StormElement? stormElement = null;

    //    if (currentElementType == ElementType.Normal && id is not null)
    //        stormElement = GetStormElement(dataObjectType, id);
    //    else if (currentElementType == ElementType.Type)
    //        stormElement = GetStormElement(dataObjectType);
    //    else if (currentElementType == ElementType.Base)
    //        stormElement = GetBaseStormElement(dataObjectType);

    //    if (stormElement is null)
    //        return stormElement;

    //    if (stormElement.HasParentId)
    //    {
    //        // parents
    //        StormElement? parentElement = MergeUpStormElement(dataObjectType, stormElement.ParentId, ElementType.Normal);
    //        parentElement?.AddValue(stormElement);

    //        return parentElement ?? stormElement;
    //    }
    //    else if (currentElementType == ElementType.Normal)
    //    {
    //        // then check the element type, which has no id attribute
    //        StormElement? typeElement = MergeUpStormElement(stormElement.ElementType, null, ElementType.Type);
    //        typeElement?.AddValue(stormElement);

    //        return typeElement ?? stormElement;
    //    }
    //    else if (currentElementType == ElementType.Type)
    //    {
    //        // then check the base element type, may not be the correct one, but close enough
    //        StormElement? baseElement = MergeUpStormElement(stormElement.ElementType, null, ElementType.Base);
    //        baseElement?.AddValue(stormElement);

    //        return baseElement ?? stormElement;
    //    }

    //    return stormElement;
    //}


    //public bool TryGetStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement)
    //{
    //    return _stormStorage.TryGetExistingStormElementsByDataObjectType(dataObjectType, id, out stormElement);
    //}

    //public bool TryGetStormElement(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement)
    //{
    //    return _stormStorage.TryGetExistingStormElementsByDataObjectType(dataObjectType, id, out stormElement);
    //}

    //public bool TryGetStormElement(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out StormElement? stormElement)
    //{
    //    return _stormStorage.TryGetExistingStormElementByElementType(elementType, out stormElement);
    //}

    //public bool TryGetStormElement(string elementType, [NotNullWhen(true)] out StormElement? stormElement)
    //{
    //    return _stormStorage.TryGetExistingStormElementByElementType(elementType, out stormElement);
    //}

    //public bool TryGetLevelScalingValue(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    //{
    //    return _stormStorage.TryGetLevelScalingArrayElement(catalog, entry, field, out stormStringValue);
    //}

    //public bool TryGetLevelScalingValue(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    //{
    //    return _stormStorage.TryGetLevelScalingArrayElement(catalog, entry, field, out stormStringValue);
    //}



    //public double EvaluateConstantElement(XElement constElement)
    //{
    //    return _stormStorage.GetValueFromConstElementAsNumber(constElement);
    //}

    public TooltipDescription ParseGameString(string gameString, StormLocale stormLocale, bool extractFontValues = false)
    {
        return new TooltipDescription(GameStringParser.ParseTooltipDescription(_stormStorage, gameString), stormLocale, extractFontValues);
    }

    internal void SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
