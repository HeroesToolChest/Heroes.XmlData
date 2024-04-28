namespace Heroes.XmlData;

public class ManualModLoader
{
    public ManualModLoader(string name)
    {
        Name = name;
    }

    public string Name { get; }

    internal Dictionary<StormLocale, List<string>> GameStringsByLocale { get; } = [];

    internal StormLocale GameStringsLocale { get; private set; }

    /// <summary>
    /// Adds a gamestring collection to the custom cache storage. If an id already exists, it will be overridden.
    /// </summary>
    /// <param name="gameStrings">A collection of gamestrings in a format of &lt;id&gt;=&lt;value&gt;.</param>
    /// <param name="stormLocale"></param>
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
}
