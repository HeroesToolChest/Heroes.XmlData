namespace Heroes.XmlData;

/// <summary>
/// Used to load a custom mod.
/// </summary>
public class ManualModLoader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManualModLoader"/> class.
    /// </summary>
    /// <param name="name">A name to be given to this mod.</param>
    public ManualModLoader(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of this mod loader.
    /// </summary>
    public string Name { get; }

    internal Dictionary<StormLocale, List<string>> GameStringsByLocale { get; } = [];

    internal List<XElement> ConstantXElements { get; } = [];

    internal Dictionary<string, HashSet<string>> ElementNamesByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal List<XElement> Elements { get; } = [];

    internal List<XElement> LevelScalingArrayElements { get; } = [];

    internal List<XElement> StormStyleElements { get; } = [];

    /// <summary>
    /// Adds an unparsed gamestring collection to the custom cache storage. If an id already exists, it will be overridden.
    /// </summary>
    /// <param name="gameStrings">A collection of unparsed gamestrings in a format of &lt;id&gt;=&lt;value&gt;.</param>
    /// <param name="stormLocale">The localization of the gamestrings.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddGameStrings(IEnumerable<string> gameStrings, StormLocale stormLocale)
    {
        if (GameStringsByLocale.TryGetValue(stormLocale, out List<string>? gamestrings))
            gamestrings.AddRange(gameStrings);
        else
            GameStringsByLocale[stormLocale] = gameStrings.ToList();

        return this;
    }

    /// <summary>
    /// Adds a collection of constant <see cref="XElement"/>s to the custom cache storage.
    /// </summary>
    /// <param name="elements">A collection of const <see cref="XElement"/>s (e.g. &lt;const id="" /&gt;).</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddConstantXElements(IEnumerable<XElement> elements)
    {
        ConstantXElements.AddRange(elements);

        return this;
    }

    /// <summary>
    /// Adds a collection of base element types to the custom cache storage.
    /// </summary>
    /// <param name="elements">A collection of tuples consisting of the base type (e.g. Effect) and a name of an element (e.g. CEffectDamage).</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddBaseElementTypes(IEnumerable<(string BaseType, string ElementName)> elements)
    {
        foreach ((string baseType, string elementName) in elements)
        {
            if (ElementNamesByDataObjectType.TryGetValue(baseType, out HashSet<string>? value))
                value.Add(elementName);
            else
                ElementNamesByDataObjectType[baseType] = [elementName];
        }

        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="XElement"/>s to the custom cache storage.
    /// Use <see cref="AddBaseElementTypes(IEnumerable{ValueTuple{string, string}})"/> to add in the base element types.
    /// </summary>
    /// <param name="elements">A collection of <see cref="XElement"/>s.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddElements(IEnumerable<XElement> elements)
    {
        Elements.AddRange(elements);

        return this;
    }

    /// <summary>
    /// Adds a collection of level scaling array <see cref="XElement"/>s to the custom cache storage.
    /// </summary>
    /// <param name="elements">A collection of level scaling array <see cref="XElement"/>s (e.g. &lt;LevelScalingArray /&gt;).</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddLevelScalingArrayElements(IEnumerable<XElement> elements)
    {
        LevelScalingArrayElements.AddRange(elements);

        return this;
    }

    /// <summary>
    /// Adds a collection of storm style <see cref="XElement"/>s to the custom cache storage.
    /// </summary>
    /// <param name="elements">A collection of storm style <see cref="XElement"/>s (e.g. &lt;Constant name="" /&gt; or &lt;Style name="" /&gt;).</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddStormStyleElements(IEnumerable<XElement> elements)
    {
        StormStyleElements.AddRange(elements);

        return this;
    }
}
