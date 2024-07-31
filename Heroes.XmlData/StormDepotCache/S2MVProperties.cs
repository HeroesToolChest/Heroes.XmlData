namespace Heroes.XmlData.StormDepotCache;

// s2mv files are json files
internal class S2MVProperties : S2mapDependencies
{
    /// <summary>
    /// Gets or sets a map id found in the s2mv file. This is not unique.
    /// </summary>
    // MapInfo:Properties:Loading:MapLink
    public string MapLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the map name in enUS. This is more unique than <see cref="MapLink"/>.
    /// </summary>
    // UsedStrings:DocInfo/Name:enUS
    public string HeaderTitle { get; set; } = string.Empty;

    // MapInfo:Properties:Size
    public Point MapSize { get; set; }

    // MapInfo:Properties:PreviewLargeImage
    public string PreviewLargeImage { get; set; } = string.Empty;

    // MapInfo:Properties:Loading:Image
    public string LoadingImage { get; set; } = string.Empty;

    // MapInfo:Properties:Loading:CustomLayout
    public string CustomLayout { get; set; } = string.Empty;

    // MapInfo:Properties:Loading:CustomFrame
    public string CustomFrame { get; set; } = string.Empty;

    public override string ToString()
    {
        return HeaderTitle;
    }
}
