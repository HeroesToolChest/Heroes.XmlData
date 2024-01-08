namespace Heroes.XmlData.StormDepotCache;

internal class S2MVProperties : S2mapDependencies
{
    /// <summary>
    /// Gets or sets a map id found in the s2mv file. This is not unique.
    /// </summary>
    public string MapLink { get; set; } = string.Empty;

    public string HeaderTitle { get; set; } = string.Empty;

    public Point MapSize { get; set; }

    public string LoadingImage { get; set; } = string.Empty;

    public override string ToString()
    {
        return HeaderTitle;
    }
}
