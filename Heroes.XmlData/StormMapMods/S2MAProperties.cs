namespace Heroes.XmlData.StormMapMods;

internal class S2MAProperties : S2mapDependencies
{
    public S2MVProperties? S2MVProperties { get; set; }

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
