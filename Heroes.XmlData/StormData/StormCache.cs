namespace Heroes.XmlData.StormData;

// used for creating caches from the xml data for faster lookups for certain elements and values
internal class StormCache
{
    /// <summary>
    /// Gets the font style cache. Used to determine color hex values.
    /// </summary>
    public Dictionary<string, StormValue<string>> StormStyleHexColorValueByName { get; } = [];

    /// <summary>
    /// Gets the level scaling cache. Used to determine the scaling value of damage abilities.
    /// </summary>
    public Dictionary<LevelScalingEntry, StormValue<double>> ScaleValueByEntry { get; } = [];

    /// <summary>
    /// Gets the constant xlement cache. Used to get the const elements and their values.
    /// </summary>
    public Dictionary<string, StormXElementValue> ConstantElementById { get; } = [];

    /// <summary>
    /// Gets the elements cache. Elements are grouped up by their element name.
    /// </summary>
    public Dictionary<string, List<StormXElementValue>> ElementsByElementName { get; } = [];
}
