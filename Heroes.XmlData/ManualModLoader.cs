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

    // image files
    internal HashSet<string> AssetFilePaths { get; } = [];

    internal List<StormMap> StormMaps { get; } = [];

    internal HashSet<string> LayoutFilePaths { get; } = [];

    // from the assest.txt files
    internal HashSet<string> AssetTexts { get; } = [];

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
            GameStringsByLocale[stormLocale] = [.. gameStrings];

        return this;
    }

    /// <summary>
    /// Adds a collection of constant <see cref="XElement"/>s (e.g. &lt;const id="" /&gt;) to the custom cache storage.
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

    /// <summary>
    /// Adds a collection of asset file paths (images) to the custom cache storage.
    /// </summary>
    /// <param name="filePaths">
    /// A collection of file paths (images). Normally asset file paths should begin with the "asset" directory, but in this
    /// case since they are being added manually, the file path should be a real path that exists on disk.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddAssetFilePaths(IEnumerable<string> filePaths)
    {
        AssetFilePaths.UnionWith(filePaths);

        return this;
    }

    /// <summary>
    /// Adds a collection of <see cref="StormMap"/>s (data from s2mv and s2ma, not xml files).
    /// </summary>
    /// <param name="stormMaps">A collection of storm maps.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddStormMaps(IEnumerable<StormMap> stormMaps)
    {
        StormMaps.AddRange(stormMaps);

        return this;
    }

    /// <summary>
    /// Adds a collection of layout file paths to the custom cache storage.
    /// </summary>
    /// <param name="filePaths">
    /// A collection of layout file paths. Normally layout file paths should begin with the "UI" directory, but in this
    /// case since they are being added manually, the file path should be a real path that exists on disk.
    /// </param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddLayoutFilePaths(IEnumerable<string> filePaths)
    {
        LayoutFilePaths.UnionWith(filePaths);

        return this;
    }

    /// <summary>
    /// Adds a collection of assets to the custom cache storage. If an id already exists, it will be overridden.
    /// </summary>
    /// <param name="assets">A collection assets texts in a format of &lt;id&gt;=&lt;value&gt;.</param>
    /// <returns>The current <see cref="ManualModLoader"/> instance.</returns>
    public ManualModLoader AddAssetTexts(IEnumerable<string> assets)
    {
        AssetTexts.UnionWith(assets);

        return this;
    }
}
