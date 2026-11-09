namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Default values the game uses when the XML data does not set them.
/// </summary>
internal static class StormDefaultData
{
    /// <summary>
    /// Gets the XML that holds the default values. The game data merges over these.
    /// </summary>
    public const string DefaultXml =
        """
        <Catalog>
          <CBehaviorBuff>
            <DamageResponse ModifyLimit="0" ModifyFraction="1" />
          </CBehaviorBuff>
        </Catalog>
        """;

    /// <summary>
    /// Gets the file that the default elements are from.
    /// </summary>
    public static StormFile StormFile { get; } = new()
    {
        StormModName = HsdConstants.Name,
        StormModPath = $"{HsdConstants.Name}-defaults",
    };
}
