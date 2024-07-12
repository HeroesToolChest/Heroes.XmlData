namespace Heroes.XmlData.StormData;

/// <summary>
/// Combined storage for the types of <see cref="StormModType"/>s.
/// </summary>
internal class StormCache
{
    /// <summary>
    /// Gets a collection of directories that were not found.
    /// </summary>
    public HashSet<StormPath> NotFoundDirectoriesList { get; } = [];

    /// <summary>
    /// Gets a collection of files that were not found.
    /// </summary>
    public HashSet<StormPath> NotFoundFilesList { get; } = [];

    /// <summary>
    /// Gets a collection of level scaling entries that could not be found.
    /// </summary>
    public List<KeyValuePair<LevelScalingEntry, StormStringValue>> NotFoundScaleValuesList { get; } = [];

    /// <summary>
    /// Gets a dictionary of gamestrings by id.
    /// </summary>
    public Dictionary<string, GameStringText> GameStringsById { get; } = [];

    /// <summary>
    /// Gets the a collection of element types (e.g. CEffectDamage) from a given data object type (e.g. Effect).
    /// </summary>
    public Dictionary<string, HashSet<string>> ElementTypesByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a data object type (e.g. Effect) from a given element type (e.g. CEffectDamage).
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
    /// Gets a dictionary of <see cref="StormElement"/>s by their id attribute by their data object type (e.g. Effect).
    /// These storm elements are specially created from a LevelScalingArray element which contains the scaling value.
    /// </summary>
    public Dictionary<string, Dictionary<string, StormElement>> ScaleValueStormElementsByDataObjectType { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a dictionary of Storm Style styles by name.
    /// </summary>
    public Dictionary<string, StormStyleStyleElement> StormStyleStyleElementsByName { get; } = [];

    /// <summary>
    /// Gets a dictionary of Storm Style constants by name.
    /// </summary>
    public Dictionary<string, StormStyleConstantElement> StormStyleConstantElementsByName { get; } = [];

    /// <summary>
    /// Gets the level scaling cache. Used to determine the scaling value of damage abilities.
    /// </summary>
    public Dictionary<LevelScalingEntry, StormStringValue> ScaleValueByEntry { get; } = [];

    public void Clear()
    {
        NotFoundDirectoriesList.Clear();
        NotFoundFilesList.Clear();
        NotFoundScaleValuesList.Clear();
        GameStringsById.Clear();
        ElementTypesByDataObjectType.Clear();
        DataObjectTypeByElementType.Clear();
        ConstantXElementById.Clear();
        StormElementByElementType.Clear();
        StormElementsByDataObjectType.Clear();
        ScaleValueStormElementsByDataObjectType.Clear();
        StormStyleStyleElementsByName.Clear();
        StormStyleConstantElementsByName.Clear();
        ScaleValueByEntry.Clear();
    }
}