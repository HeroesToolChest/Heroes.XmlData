namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains map data that is pulled from the s2ma and s2mv files.
/// It also provides the file paths to the s2ma and s2mv file.
/// </summary>
public class StormMap
{
    /// <summary>
    /// Gets the name of the storm map in enUS.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a dictionary of the map name by their locale.
    /// </summary>
    public required IReadOnlyDictionary<StormLocale, string> NameByLocale { get; init; }

    /// <summary>
    /// Gets the map id. This id is found in a replay's tracker events. It is not always set.
    /// </summary>
    public required string MapId { get; init; }

    /// <summary>
    /// Gets the map link id. This is the id found in the xml files for CMap.
    /// <para>
    /// This is not unique. Cursed Hollow and Cursed Hollow (Sandbox) share the same map link.
    /// </para>
    /// </summary>
    public required string MapLink { get; init; }

    /// <summary>
    /// Gets the map size.
    /// </summary>
    public required (double X, double Y) MapSize { get; init; }

    /// <summary>
    /// Gets the file path of the image used for preview of the map on the replay screen.
    /// This is pulled from the s2mv file.
    /// <para>
    /// The file path can be relative to the s2ma file (the mpq file) or the root directory.
    /// </para>
    /// </summary>
    public required string ReplayPreviewImagePath { get; init; }

    /// <summary>
    /// Gets the file path of the background image used during the map loading screen.
    /// The is pulled from the s2mv file.
    /// <para>
    /// This not always set or is not the correct image. The actual image is actually set from the map's storm layout file.
    /// </para>
    /// <para>
    /// The file path can be relative to the s2ma file (the mpq file) or the root directory.</para>
    /// </summary>
    public required string LoadingScreenImagePath { get; init; }

    /// <summary>
    /// Gets the relative path to the storm layout file.
    /// </summary>
    public required string LayoutFilePath { get; init; }

    /// <summary>
    /// Gets the reference to the loading screen frame in the layout file.
    /// </summary>
    public required string LayoutLoadingScreenFrame { get; init; }

    /// <summary>
    /// Gets the relative path of the s2ma (mpq) file.
    /// </summary>
    public required string S2MAFilePath { get; init; }

    /// <summary>
    /// Gets the relative path of the s2mv (json) file.
    /// </summary>
    public required string S2MVFilePath { get; init; }
}
