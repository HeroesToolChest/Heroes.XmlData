namespace Heroes.XmlData.StormData;

/// <summary>
/// Combined storage for the types of <see cref="StormModType"/>s.
/// </summary>
internal class StormCache
{
    /// <summary>
    /// Gets a collection of directories that were not found.
    /// </summary>
    public HashSet<StormFile> NotFoundDirectoriesList { get; } = [];

    /// <summary>
    /// Gets a collection of files that were not found.
    /// </summary>
    public HashSet<StormFile> NotFoundFilesList { get; } = [];

    /// <summary>
    /// Gets a dictionary of gamestrings by id.
    /// </summary>
    public Dictionary<string, GameStringText> GameStringsById { get; } = [];

    /// <summary>
    /// Gets the a collection of element types (e.g. CEffectDamage) from a given data object type (e.g. Effect).
    /// </summary>
    public Dictionary<string, HashSet<string>> ElementTypesByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a data object type (e.g. Effect) from a given element type (e.g CEffectDamage).
    /// </summary>
    public Dictionary<string, string> DataObjectTypeByElementType { get; } = [];

    /// <summary>
    /// Gets a dictionary of const elements by id.
    /// </summary>
    public Dictionary<string, StormXElementValuePath> ConstantXElementById { get; } = [];

    /// <summary>
    /// Gets a dictionary of <see cref="StormElement"/>s that have no id attribute by their element name.
    /// </summary>
    public Dictionary<string, StormElement> StormElementByElementType { get; } = [];

    /// <summary>
    /// Gets a dictionary of <see cref="StormElement"/>s by their id attribute by their data object type (e.g. Effect).
    /// </summary>
    public Dictionary<string, Dictionary<string, StormElement>> StormElementsByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the font style cache. Used to determine color hex values.
    /// </summary>
    public Dictionary<string, StormStringValue> StormStyleHexColorValueByName { get; } = [];

    /// <summary>
    /// Gets the level scaling cache. Used to determine the scaling value of damage abilities.
    /// </summary>
    public Dictionary<LevelScalingEntry, StormStringValue> ScaleValueByEntry { get; } = [];

    public void Clear()
    {
        NotFoundDirectoriesList.Clear();
        NotFoundFilesList.Clear();
        GameStringsById.Clear();
        ElementTypesByDataObjectType.Clear();
        DataObjectTypeByElementType.Clear();
        ConstantXElementById.Clear();
        StormElementByElementType.Clear();
        StormElementsByDataObjectType.Clear();
        StormStyleHexColorValueByName.Clear();
        ScaleValueByEntry.Clear();
    }
}