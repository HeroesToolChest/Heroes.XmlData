namespace Heroes.StormData.DepotCache.Maps;

/// <summary>
/// S2MV properties extracted from s2mv files which are json files.
/// </summary>
internal sealed class S2MVProperties : S2MapPropertiesBase
{
    /// <summary>
    /// Gets a map id found in the s2mv file. This is not unique.
    /// </summary>
    // MapInfo:Properties:Loading:MapLink
    public string MapLink { get; init; } = string.Empty;

    /// <summary>
    /// Gets the map name in enUS. This is more unique than <see cref="MapLink"/>.
    /// </summary>
    // UsedStrings:DocInfo/Name:enUS
    public string HeaderTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the map by the locale.
    /// </summary>
    // UsedStrings:DocInfo/Name
    public Dictionary<StormLocale, string> NameByStormLocale { get; init; } = [];

    // MapInfo:Properties:Size
    public MapSize MapSize { get; init; }

    // MapInfo:Properties:PreviewLargeImage
    public string PreviewLargeImage { get; init; } = string.Empty;

    // MapInfo:Properties:Loading:Image
    public string LoadingImage { get; init; } = string.Empty;

    // MapInfo:Properties:Loading:CustomLayout
    public string CustomLayout { get; init; } = string.Empty;

    // MapInfo:Properties:Loading:CustomFrame
    // a reference to a frame in the CustomLayout file, not a file path, so it is kept as is (not normalized)
    public string CustomFrame { get; init; } = string.Empty;
}
