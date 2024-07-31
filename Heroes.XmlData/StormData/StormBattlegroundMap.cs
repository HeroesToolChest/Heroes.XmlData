namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the map specific data. This data is pulled from the s2ma and s2mv files.
/// </summary>
public class StormBattlegroundMap
{
    /// <summary>
    /// Gets the name of the storm map in enUS.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the map id. This id is also found in a replay's tracker events. Not is not always set.
    /// </summary>
    public required string MapId { get; init; }

    /// <summary>
    /// Gets the map link. This is not unique.
    /// </summary>
    public required string MapLink { get; init; }

    /// <summary>
    /// Gets the map size.
    /// </summary>
    public required (double X, double Y) MapSize { get; init; }

    /// <summary>
    /// Gets the file name of the image used for preview of the map on the replay screen.
    /// This is pulled from the s2mv file. The image usually exists in the s2ma file.
    /// </summary>
    public required string ReplayPreviewImage { get; init; }

    /// <summary>
    /// Gets the file name of the background image used during the map loading screen.
    /// The is pulled from the s2mv file. This not always set or is not the correct image. The actual image is actually set from the map's storm layout file.
    /// </summary>
    public required string LoadingScreenImage { get; init; }
}
