namespace Heroes.StormData.DepotCache.Maps;

/// <summary>
/// S2MA are the <c>mapmod</c> files. These are in mpq format.
/// </summary>
internal sealed class S2MAProperties : S2MapPropertiesBase
{
    /// <summary>
    /// Gets or sets the matching <see cref="Maps.S2MVProperties"/>, if one was found.
    /// </summary>
    public S2MVProperties? S2MVProperties { get; set; }

    /// <summary>
    /// Gets the map id found in a map's stormmod mapscript.galaxy file. This id is found in a replay's tracker events. This is sometimes not set in the mapscript.galaxy file.
    /// </summary>
    public string? MapId { get; init; }
}
