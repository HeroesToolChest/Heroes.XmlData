namespace Heroes.XmlData;

/// <summary>
/// Represents options for configuring the CascLib library.
/// </summary>
public class CascLibOptions
{
    /// <summary>
    /// Gets or sets the path where cached data will be stored. The default value is <c>cache</c>.
    /// </summary>
    public string CachePath { get; set; } = "cache";
}
