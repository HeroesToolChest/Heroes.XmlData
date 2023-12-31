namespace Heroes.XmlData.StormMapMods;

internal record MapDependency
{
    public required string BnetName { get; init; } = string.Empty;

    public required int BnetVersionMajor { get; init; }

    public required int BnetVersionMinor { get; init; }

    public required int BnetNamespace { get; init; }

    public required string LocalFile { get; init; } = string.Empty;
}
