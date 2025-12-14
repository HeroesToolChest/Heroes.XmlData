namespace Heroes.XmlData;

/// <summary>
/// Contains the loaded heroes data, usually from the xml data files.
/// </summary>
public class HeroesData
{
    private readonly IStormStorage _stormStorage;

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

    /// <summary>
    /// Gets the loaded heroes build number.
    /// </summary>
    public int? Build => _stormStorage.GetBuildId();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal IStormStorage StormStorage => _stormStorage;

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IEnumerable<string> GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType).AsReadOnly();
    }

    /// <summary>
    /// Gets the data object type (e.g. Effect) associated with the element type (e.g. CEffectDamage).
    /// </summary>
    /// <param name="elementType">The name of the element.</param>
    /// <returns>The data object type or <see langword="null"/> if not found.</returns>
    public string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType)
    {
        return _stormStorage.GetDataObjectTypeByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that does not have an id attribute value.
    /// </summary>
    /// <param name="elementType">The name of the element.</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> elementType)
    {
        return _stormStorage.GetStormElementByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that has an id attribute value.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The id of element.</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.GetStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Determines if a storm element with an id attribute exists.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The id of element.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    public bool StormElementExists(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.StormElementExists(id, dataObjectType);
    }

    /// <summary>
    /// Gets a storm element attribute id by the unit name attribute.
    /// </summary>
    /// <param name="unitName">The unit name of an element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>The id of the element or <see langword="null"/> if not found.</returns>
    public string? GetStormElementIdByUnitName(ReadOnlySpan<char> unitName, ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetStormElementIdByUnitName(unitName, dataObjectType);
    }

    /// <summary>
    /// Gets the scaling value for an element value.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The id of the element.</param>
    /// <param name="elementName">An element name (e.g. LifeMax).</param>
    /// <returns>The scaling value or <see langword="null"/> if not found.</returns>
    public double? GetScalingValue(string dataObjectType, string id, string elementName)
    {
        StormElement? stormElement = GetScaleValueStormElement(dataObjectType, id);
        if (stormElement is not null)
        {
            if (stormElement.DataValues.TryGetElementDataAt(elementName, out StormElementData? data))
                return data.HxdScaleValue.GetDouble();
        }

        return null;
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>'s element.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The value of an id attribute.</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents a StormStyle Constant element.
    /// </summary>
    /// <param name="name">The name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleConstantElement? GetStormStyleConstantStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleConstantElementsByName(name);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">The name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleStyleElement? GetStormStyleStyleStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleStyleElementsByName(name);
    }

    /// <summary>
    /// Gets an unparsed gamestring (from a gamestrings.txt file).
    /// </summary>
    /// <param name="id">The id of the gamestring.</param>
    /// <returns>An unparsed gamestring or <see langword="null"/> if not found.</returns>
    public StormGameString? GetStormGameString(string id)
    {
        return _stormStorage.GetStormGameString(id);
    }

    /// <summary>
    /// Gets an asset text string (from an assets.txt file).
    /// </summary>
    /// <param name="id">The id of the asset.</param>
    /// <returns>The text of the asset or <see langword="null"/> if not found.</returns>
    public StormAssetString? GetStormAssetString(string id)
    {
        return _stormStorage.GetStormAssetString(id);
    }

    /// <summary>
    /// Get a collection of all the unparsed gamestrings (from the gamestrings.txt files).
    /// </summary>
    /// <returns>A collection of unparsed gamestrings.</returns>
    public IEnumerable<StormGameString> GetStormGameStrings()
    {
        return _stormStorage.GetStormGameStrings().AsReadOnly();
    }

    /// <summary>
    /// Parses and evaluates an unparsed gamestring into a <see cref="GameStringText"/>.
    /// </summary>
    /// <param name="gameString">The gamestring to parse.</param>
    /// <param name="gameStringLocale">The localization of the <paramref name="gameString"/>. Used only for scaling text.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, then the font style and constant tags will have their val values saved in <see cref="GameStringText.FontStyleValues"/> and  <see cref="GameStringText.FontStyleConstantValues"/>.
    /// If not needing the output with color tags, then set to <see langword="false"/> for faster parsing performance.
    /// </param>
    /// <returns>A parsed <see cref="GameStringText"/>.</returns>
    public GameStringText ParseGameString(string gameString, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        return new GameStringText(GameStringParser.ParseGameStringText(_stormStorage, gameString), gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// Parses and evaluates an unparsed gamestring into a <see cref="GameStringText"/>.
    /// </summary>
    /// <param name="stormGameString">The gamestring to parse.</param>
    /// <param name="gameStringLocale">The localization of the <paramref name="stormGameString"/>. Used only for scaling text.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, then the font style and constant tags will have their val values saved in <see cref="GameStringText.FontStyleValues"/> and  <see cref="GameStringText.FontStyleConstantValues"/>.
    /// If not needing the output with color tags, then set to <see langword="false"/> for faster parsing performance.
    /// </param>
    /// <returns>A parsed <see cref="GameStringText"/>.</returns>
    public GameStringText ParseGameString(StormGameString stormGameString, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        return new GameStringText(GameStringParser.ParseGameStringText(_stormStorage, stormGameString.Value), gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// Gets a collection of <see cref="StormElement"/> ids by the data object type.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="stormCacheType">The internal caches to get obtain the ids from. It will always be in the following order: <see cref="StormCacheType.Normal"/>, <see cref="StormCacheType.Map"/>, and then <see cref="StormCacheType.Custom"/>.</param>
    /// <returns>A collection of <see cref="StormElement"/> ids.</returns>
    public IEnumerable<string> GetStormElementIds(string dataObjectType, StormCacheType stormCacheType = StormCacheType.All)
    {
        return _stormStorage.GetStormElementIds(dataObjectType, stormCacheType).AsReadOnly();
    }

    /// <summary>
    /// Determines if an asset file (images) exists.
    /// </summary>
    /// <param name="path">The path of the asset, which should start with the "asset" directory. Is not case-sensitive.</param>
    /// <returns><see langword="true"/> if the file exists, otherwise <see langword="false"/>.</returns>
    public bool StormAssetFileExists(string? path)
    {
        return _stormStorage.StormAssetFileExists(path);
    }

    /// <summary>
    /// Gets a storm asset file (images) that is from the assets directory.
    /// </summary>
    /// <param name="path">The path of the asset, which should start with the "asset" directory. Is not case-sensitive.</param>
    /// <returns>A <see cref="StormFile"/> or <see langword="null"/> if not found.</returns>
    public StormFile? GetStormAssetFile(string? path)
    {
        return _stormStorage.GetStormAssetFile(path);
    }

    /// <summary>
    /// Determines if a layout file exists.
    /// </summary>
    /// <param name="path">The path of the asset, which should start with the "ui" directory. Is not case-sensitive.</param>
    /// <returns><see langword="true"/> if the file exists, otherwise <see langword="false"/>.</returns>
    public bool StormLayoutFileExists(string? path)
    {
        return _stormStorage.StormLayoutFileExists(path);
    }

    /// <summary>
    /// Gets a storm layout file that is from the ui directory.
    /// </summary>
    /// <param name="path">The path of the layout file, which should start with the "ui" directory. Is not case-sensitive.</param>
    /// <returns>A <see cref="StormFile"/> or <see langword="null"/> if not found.</returns>
    public StormFile? GetStormLayoutFile(string? path)
    {
        return _stormStorage.GetStormLayoutFile(path);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that is created from a level scaling array element that contains the scaling attribute value.<br/>
    /// <br/>
    /// The scaling value will be in the <see cref="StormElementData.HxdScaleValue"/>.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The id of the element.</param>
    /// <returns>A <see cref="StormElement"/> that contains a scaling value or <see langword="null"/> if not found.</returns>
    internal StormElement? GetScaleValueStormElement(string dataObjectType, string id)
    {
        return _stormStorage.GetScaleValueStormElementById(id, dataObjectType);
    }

    internal void SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
