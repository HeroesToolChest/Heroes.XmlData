namespace Heroes.XmlData.StormData;

// used for creating caches from the xml data for faster lookups for certain elements and values
// includes the gamestrings for all stormmods as well
internal class StormCache
{
    /// <summary>
    /// Gets the font style cache. Used to determine color hex values.
    /// </summary>
    public Dictionary<string, StormStringValue> StormStyleHexColorValueByName { get; } = [];

    /// <summary>
    /// Gets the level scaling cache. Used to determine the scaling value of damage abilities.
    /// </summary>
    public Dictionary<LevelScalingEntry, StormStringValue> ScaleValueByEntry { get; } = [];

    /// <summary>
    /// Gets the constant xlement cache. Used to get the const elements and their values.
    /// </summary>
    public Dictionary<string, StormXElementValue> ConstantElementById { get; } = [];

    /// <summary>
    /// Gets the elements cache. Elements are grouped up by their element name.
    /// </summary>
    public Dictionary<string, List<StormXElementValue>> ElementsByElementName { get; } = [];

    /// <summary>
    /// Gets all the gamestrings cache.
    /// </summary>
    public Dictionary<string, GameStringText> GameStringsById { get; } = [];

    public void AddGameString(ReadOnlySpan<char> gamestring, ReadOnlySpan<char> path)
    {
        AddGameString(gamestring, path, null);
    }

    public void AddGameString(ReadOnlySpan<char> gamestring, ReadOnlySpan<char> path, Dictionary<string, GameStringText>? containerDictionary)
    {
        Span<Range> ranges = stackalloc Range[2];

        gamestring.Split(ranges, '=', StringSplitOptions.None);

        if (gamestring[ranges[0]].IsEmpty || gamestring[ranges[0]].IsWhiteSpace())
            return;

        GameStringText gameStringText = new(gamestring[ranges[1]].ToString(), path.ToString());

        string id = gamestring[ranges[0]].ToString();

        GameStringsById[id] = gameStringText;

        if (containerDictionary is not null)
            containerDictionary[id] = gameStringText;
    }

    public void AddConstantElement(XElement element, string path)
    {
        if (element.Name.LocalName.Equals("const", StringComparison.OrdinalIgnoreCase))
        {
            string? id = element.Attribute("id")?.Value;

            if (string.IsNullOrEmpty(id))
                return;

            ConstantElementById[id] = new StormXElementValue(element, path);
        }
    }

    public void Clear()
    {
        StormStyleHexColorValueByName.Clear();
        ScaleValueByEntry.Clear();
        ConstantElementById.Clear();
        ElementsByElementName.Clear();
        GameStringsById.Clear();
    }
}