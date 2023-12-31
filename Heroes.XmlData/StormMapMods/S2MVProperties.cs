namespace Heroes.XmlData.StormMapMods;

internal class S2MVProperties : S2mapDependencies
{
    public string MapLink { get; set; } = string.Empty;

    public string HeaderTitle { get; set; } = string.Empty;

    public Point MapSize { get; set; }

    public string LoadingImage { get; set; } = string.Empty;

    public override string ToString()
    {
        return HeaderTitle;
    }
}
