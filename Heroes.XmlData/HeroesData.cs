using Heroes.XmlData.GameStrings;

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
    /// Gets the build number.
    /// </summary>
    public int? Build => _stormStorage.GetBuildId();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal IStormStorage StormStorage => _stormStorage;

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IEnumerable<string> GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType).AsReadOnly();
    }

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IEnumerable<string> GetElementTypesByDataObjectType(string dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType).AsReadOnly();
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
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> elementType)
    {
        return _stormStorage.GetStormElementByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that does not have an id attribute value.
    /// </summary>
    /// <param name="elementType">The name of the element.</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(string elementType)
    {
        return _stormStorage.GetStormElementByElementType(elementType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that has an id attribute value.
    /// </summary>
    /// <param name="id">A character span that contains the id of element.</param>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that represents an element that has an id attribute value.
    /// </summary>
    /// <param name="id">The id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetStormElement(string id, string dataObjectType)
    {
        return _stormStorage.GetStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that is created from a level scaling array element that contains the scaling attribute value.
    /// </summary>
    /// <param name="id">A character span that contains the id of element.</param>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> that contains a scaling value or <see langword="null"/> if not found.</returns>
    public StormElement? GetScaleValueStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetScaleValueStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that is created from a level scaling array element that contains the scaling attribute value.
    /// </summary>
    /// <param name="id">The id of element.</param>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A <see cref="StormElement"/> that contains a scaling value or <see langword="null"/> if not found.</returns>
    public StormElement? GetScaleValueStormElement(string id, string dataObjectType)
    {
        return _stormStorage.GetScaleValueStormElementById(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <param name="id">A character span that contains the value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormElement? GetCompleteStormElement(string dataObjectType, string id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a storm style Constant element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleConstantElement? GetStormStyleConstantStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleConstantElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Constant element.
    /// </summary>
    /// <param name="name">The name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleConstantElement? GetStormStyleConstantStormElement(string name)
    {
        return _stormStorage.GetStormStyleConstantElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleStyleElement? GetStormStyleStyleStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleStyleElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">The name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/> or <see langword="null"/> if not found.</returns>
    public StormStyleStyleElement? GetStormStyleStyleStormElement(string name)
    {
        return _stormStorage.GetStormStyleStyleElementsByName(name);
    }

    /// <summary>
    /// Gets an unparsed gamestring.
    /// </summary>
    /// <param name="id">A character span that contains the id of the gamestring.</param>
    /// <returns>An unparsed gamestring or <see langword="null"/> if not found.</returns>
    public StormGameString? GetStormGameString(ReadOnlySpan<char> id)
    {
        return _stormStorage.GetStormGameString(id);
    }

    /// <summary>
    /// Gets an unparsed gamestring.
    /// </summary>
    /// <param name="id">The id of the gamestring.</param>
    /// <returns>An unparsed gamestring or <see langword="null"/> if not found.</returns>
    public StormGameString? GetStormGameString(string id)
    {
        return _stormStorage.GetStormGameString(id);
    }

    /// <summary>
    /// Gets an asset text string.
    /// </summary>
    /// <param name="id">A character span that contains the id of the asset.</param>
    /// <returns>The value of the asset or <see langword="null"/> if not found.</returns>
    public StormAssetString? GetStormAssetString(ReadOnlySpan<char> id)
    {
        return _stormStorage.GetStormAssetString(id);
    }

    /// <summary>
    /// Gets an asset text string.
    /// </summary>
    /// <param name="id">The id of the asset.</param>
    /// <returns>The value of the asset or <see langword="null"/> if not found.</returns>
    public StormAssetString? GetStormAssetText(string id)
    {
        return _stormStorage.GetStormAssetString(id);
    }

    /// <summary>
    /// Get a collection of all the unparsed gamestrings.
    /// </summary>
    /// <returns>A collection of unparsed gamestrings.</returns>
    public IEnumerable<StormGameString> GetStormGameStrings()
    {
        return _stormStorage.GetStormGameStrings().AsReadOnly();
    }

    /// <summary>
    /// Parses and evaluates a gamestring into a <see cref="TooltipDescription"/>.
    /// </summary>
    /// <param name="gameString">The gamestring to parse.</param>
    /// <param name="gameStringLocale">The localization of the <paramref name="gameString"/>.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, then the font style and constant tags will have their val values saved in <see cref="TooltipDescription.FontStyleValues"/> and  <see cref="TooltipDescription.FontStyleConstantValues"/>.
    /// If not needing the output with color tags, then set to <see langword="false"/> for faster parsing performance.
    /// </param>
    /// <returns>A parsed <see cref="TooltipDescription"/>.</returns>
    public TooltipDescription ParseGameString(string gameString, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        return new TooltipDescription(GameStringParser.ParseTooltipDescription(_stormStorage, gameString), gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// Parses and evaluates a gamestring into a <see cref="TooltipDescription"/>.
    /// </summary>
    /// <param name="stormGameString">The gamestring to parse.</param>
    /// <param name="gameStringLocale">The localization of the <paramref name="stormGameString"/>.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, then the font style and constant tags will have their val values saved in <see cref="TooltipDescription.FontStyleValues"/> and  <see cref="TooltipDescription.FontStyleConstantValues"/>.
    /// If not needing the output with color tags, then set to <see langword="false"/> for faster parsing performance.
    /// </param>
    /// <returns>A parsed <see cref="TooltipDescription"/>.</returns>
    public TooltipDescription ParseGameString(StormGameString stormGameString, StormLocale gameStringLocale = StormLocale.ENUS, bool extractFontValues = false)
    {
        return new TooltipDescription(GameStringParser.ParseTooltipDescription(_stormStorage, stormGameString.Value), gameStringLocale, extractFontValues);
    }

    /// <summary>
    /// Gets a collection of <see cref="StormElement"/> ids by the data object type.
    /// </summary>
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
    /// <returns>A collection of <see cref="StormElement"/> ids.</returns>
    public IEnumerable<string> GetStormElementIds(ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetStormElementIds(dataObjectType).AsReadOnly();
    }

    /// <summary>
    /// Gets a collection of <see cref="StormElement"/> ids by the data object type.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A collection of <see cref="StormElement"/> ids.</returns>
    public IEnumerable<string> GetStormElementIds(string dataObjectType)
    {
        return _stormStorage.GetStormElementIds(dataObjectType).AsReadOnly();
    }

    /// <summary>
    /// Determines if an asset file exists.
    /// </summary>
    /// <param name="path">A character span that contains the path of the asset, which should start with the asset directory.</param>
    /// <returns><see langword="true"/> is the file exists, otherwise <see langword="false"/>.</returns>
    public bool StormAssestFileExists(ReadOnlySpan<char> path)
    {
        return _stormStorage.StormAssetFileExists(path);
    }

    /// <summary>
    /// Determines if an asset file exists.
    /// </summary>
    /// <param name="path">The path of the asset, which should start with the asset directory.</param>
    /// <returns><see langword="true"/> is the file exists, otherwise <see langword="false"/>.</returns>
    public bool StormAssetFileExists(string path)
    {
        return _stormStorage.StormAssetFileExists(path);
    }

    /// <summary>
    /// Gets a storm asset file that is from the assets directory.
    /// </summary>
    /// <param name="path">A character span that contains the path of the asset, which should start with the asset directory.</param>
    /// <returns>A <see cref="StormAssetFile"/> or <see langword="null"/> if not found.</returns>
    public StormAssetFile? GetStormAssetFile(ReadOnlySpan<char> path)
    {
        return _stormStorage.GetStormAssetFile(path);
    }

    /// <summary>
    /// Gets a storm asset file that is from the assets directory.
    /// </summary>
    /// <param name="path">The path of the asset, which should start with the asset directory.</param>
    /// <returns>A <see cref="StormAssetFile"/> or <see langword="null"/> if not found.</returns>
    public StormAssetFile? GetStormAssetFile(string path)
    {
        return _stormStorage.GetStormAssetFile(path);
    }

    /// <summary>
    /// Gets a collection of not found files.
    /// </summary>
    /// <returns>A collection of not found <see cref="StormPath"/>s that represents not found files.</returns>
    public IEnumerable<StormPath> GetNotFoundFiles()
    {
        foreach (StormPath item in StormStorage.StormCache.NotFoundFilesList)
        {
            yield return item;
        }

        foreach (StormPath item in StormStorage.StormMapCache.NotFoundFilesList)
        {
            yield return item;
        }

        foreach (StormPath item in StormStorage.StormCustomCache.NotFoundFilesList)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Gets a collection of not found directories.
    /// </summary>
    /// <returns>A collection of not found <see cref="StormPath"/>s that represents not found directories.</returns>
    public IEnumerable<StormPath> GetNotFoundDirectories()
    {
        foreach (StormPath item in StormStorage.StormCache.NotFoundDirectoriesList)
        {
            yield return item;
        }

        foreach (StormPath item in StormStorage.StormMapCache.NotFoundDirectoriesList)
        {
            yield return item;
        }

        foreach (StormPath item in StormStorage.StormCustomCache.NotFoundDirectoriesList)
        {
            yield return item;
        }
    }

    internal void SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
