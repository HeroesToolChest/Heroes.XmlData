namespace Heroes.XmlData.StormDepotCache;

// s2ma is the map mod file
internal class S2MAProperties : S2mapDependencies
{
    public S2MVProperties? S2MVProperties { get; set; }

    /// <summary>
    /// Gets or sets the map id found in a map's stormmod mapscript.galaxy file. This id is found in a replay's tracker events. This is sometimes not set in the mapscript.galaxy file.
    /// </summary>
    public string? MapId { get; set; }

    public override string ToString()
    {
        List<string> files = [];

        foreach (MapDependency mapDependency in MapDependencies)
        {
            files.Add(Path.GetFileNameWithoutExtension(mapDependency.LocalFile));
        }

        return string.Join(',', files);
    }
}
