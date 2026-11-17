namespace Heroes.StormData.DepotCache.Maps;

internal sealed record MapDependency
{
    public required string BnetName { get; init; }

    public required int BnetVersionMajor { get; init; }

    public required int BnetVersionMinor { get; init; }

    public required int BnetNamespace { get; init; }

    public required string LocalFile { get; init; }
}
