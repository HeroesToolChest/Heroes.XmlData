namespace Heroes.XmlData;

/// <summary>
/// The type of source data.
/// </summary>
public enum HeroesXmlLoaderType
{
    /// <summary>
    /// Indicates an unknown type.
    /// </summary>
    Unknown,

    /// <summary>
    /// Indicates the source of data is from files.
    /// </summary>
    File,

    /// <summary>
    /// Indicates the source of data is from CASC.
    /// </summary>
    CASC,
}
