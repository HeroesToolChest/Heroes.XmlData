namespace Heroes.XmlData;

public class ManualModLoader
{
    public ManualModLoader(string name)
    {
        Name = name;
    }

    public string Name { get; }

    internal Dictionary<StormLocale, List<string>> GameStringsByLocale { get; } = [];

    internal List<XElement> ConstantXElements { get; } = [];

    internal Dictionary<string, HashSet<string>> ElementNamesByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal List<XElement> Elements { get; } = [];

    internal List<XElement> LevelScalingElements { get; } = [];

    internal StormLocale GameStringsLocale { get; private set; }

    /// <summary>
    /// Adds an unparsed gamestring collection to the custom cache storage. If an id already exists, it will be overridden.
    /// </summary>
    /// <param name="gameStrings">A collection of unparsed gamestrings in a format of &lt;id&gt;=&lt;value&gt;.</param>
    /// <param name="stormLocale">The localization of the gamestrings.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddGameStrings(IEnumerable<string> gameStrings, StormLocale stormLocale)
    {
        GameStringsLocale = stormLocale;

        if (GameStringsByLocale.TryGetValue(stormLocale, out List<string>? gamestrings))
            gamestrings.AddRange(gameStrings);
        else
            GameStringsByLocale[stormLocale] = gameStrings.ToList();

        return this;
    }

    /// <summary>
    /// Adds a collection of constant <see cref="XElement"/>s to the custom cache storage.
    /// </summary>
    /// <param name="elements">A collection of constant <see cref="XElement"/>s.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddConstantXElements(IEnumerable<XElement> elements)
    {
        ConstantXElements.AddRange(elements);

        return this;
    }

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

    public ManualModLoader AddElements(IEnumerable<XElement> elements)
    {
        Elements.AddRange(elements);

        return this;
    }

    public ManualModLoader AddLevelScalingElements(IEnumerable<XElement> elements)
    {
        LevelScalingElements.AddRange(elements);

        return this;
    }
}
