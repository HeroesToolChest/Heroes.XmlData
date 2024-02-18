namespace Heroes.XmlData.StormDepotCache;

internal class S2mapDependencies
{
    public List<MapDependency> MapDependencies { get; set; } = [];

    public List<string> ModifiableDependencies { get; set; } = [];

    public string DocInfoIconFile { get; set; } = string.Empty;

    public string DirectoryPath { get; set; } = string.Empty;

    public override int GetHashCode()
    {
        HashCode hashCode = default;
        foreach (MapDependency item in MapDependencies)
        {
            hashCode.Add(item.GetHashCode());
        }

        foreach (string item in ModifiableDependencies)
        {
            hashCode.Add(item);
        }

        hashCode.Add(DocInfoIconFile);

        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        MapDependency? firstMapDependency = MapDependencies.FirstOrDefault();
        if (firstMapDependency is null)
            return "(NONE)";

        return firstMapDependency.LocalFile;
    }
}