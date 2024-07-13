using Heroes.XmlData.GameStrings;
using System.Diagnostics;

namespace Heroes.XmlData;

/// <summary>
/// Contains the methods and properties to access the xml and gamestring data.
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
    public IList<string> GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType).ToList();
    }

    /// <summary>
    /// Get a collection of all the element types (e.g. CEffectDamage) associated with the data object type (e.g. Effect).
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <returns>A collection of all the element types of the data object type.</returns>
    public IList<string> GetElementTypesByDataObjectType(string dataObjectType)
    {
        return _stormStorage.GetElementTypesByDataObjectType(dataObjectType).ToList();
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
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
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
    /// <param name="dataObjectType">A character span that contains the type of the element name (e.g. Effect).</param>
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
    /// <param name="id">A character span that contains the value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/>.</returns>
    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets a <see cref="StormElement"/> that has been merged from the base element to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="dataObjectType">The type of the element name (e.g. Effect).</param>
    /// <param name="id">The value of an id attribute.</param>
    /// <returns>A merged from base element <see cref="StormElement"/>.</returns>
    public StormElement? GetCompleteStormElement(string dataObjectType, string id)
    {
        return _stormStorage.GetCompleteStormElement(id, dataObjectType);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a storm style Constant element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/>.</returns>
    public StormStyleConstantElement? GetStormStyleConstantStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleConstantElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Constant element.
    /// </summary>
    /// <param name="name">The name of the Constant element.</param>
    /// <returns>A StormStyle Constant <see cref="StormElement"/>.</returns>
    public StormStyleConstantElement? GetStormStyleConstantStormElement(string name)
    {
        return _stormStorage.GetStormStyleConstantElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">A character span that contains the name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/>.</returns>
    public StormStyleStyleElement? GetStormStyleStyleStormElement(ReadOnlySpan<char> name)
    {
        return _stormStorage.GetStormStyleStyleElementsByName(name);
    }

    /// <summary>
    /// Gets <see cref="StormElement"/> that represents a StormStyle Style element.
    /// </summary>
    /// <param name="name">The name of the Style element.</param>
    /// <returns>A StormStyle Style <see cref="StormElement"/>.</returns>
    public StormStyleStyleElement? GetStormStyleStyleStormElement(string name)
    {
        return _stormStorage.GetStormStyleStyleElementsByName(name);
    }

    /// <summary>
    /// Gets an unparsed gamestring.
    /// </summary>
    /// <param name="id">A character span that contains the id of the gamestring.</param>
    /// <returns>An unparsed gamestring.</returns>
    public StormGameString? GetStormGameString(ReadOnlySpan<char> id)
    {
        return _stormStorage.GetStormGameString(id);
    }

    /// <summary>
    /// Gets an unparsed gamestring.
    /// </summary>
    /// <param name="id">The id of the gamestring.</param>
    /// <returns>An unparsed gamestring.</returns>
    public StormGameString? GetStormGameString(string id)
    {
        return _stormStorage.GetStormGameString(id);
    }

    /// <summary>
    /// Get a collection of all the unparsed gamestrings.
    /// </summary>
    /// <returns>A collection of unparsed gamestrings.</returns>
    public IReadOnlyList<StormGameString> GetStormGameStrings()
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

    internal void SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
