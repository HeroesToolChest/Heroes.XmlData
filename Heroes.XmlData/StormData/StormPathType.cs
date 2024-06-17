namespace Heroes.XmlData.StormData;

/// <summary>
/// The type of the file or directory.
/// </summary>
public enum StormPathType
{
    /// <summary>
    /// Indicates the file or directory is programmically added.
    /// </summary>
    Hxd,

    /// <summary>
    /// Indicates the file or directory exists on disk as a physical file or directory.
    /// </summary>
    File,

    /// <summary>
    /// Indicates the file or directory is from the CASC storage.
    /// </summary>
    CASC,

    /// <summary>
    /// Indicates the file or directory is from a MPQ file.
    /// </summary>
    MPQ,
}
