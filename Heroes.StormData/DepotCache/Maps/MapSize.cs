namespace Heroes.StormData.DepotCache.Maps;

/// <summary>
/// The size of a map.
/// </summary>
/// <param name="Width">The width of the map.</param>
/// <param name="Height">The height of the map.</param>
internal readonly record struct MapSize(double Width, double Height)
{
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{{{Width}, {Height}}}");
}
