namespace Heroes.XmlData.StormData;

/// <summary>
/// The types of storm caches.
/// </summary>
[Flags]
public enum StormCacheType
{
    /// <summary>
    /// Indicates the normal cache.
    /// </summary>
    Normal = 1 << 0,

    /// <summary>
    /// Indicates the map cache.
    /// </summary>
    Map = 1 << 1,

    /// <summary>
    /// Indicates the custom cache.
    /// </summary>
    Custom = 1 << 2,

    /// <summary>
    /// Indicates all the caches.
    /// </summary>
    All = Normal | Map | Custom,
}
