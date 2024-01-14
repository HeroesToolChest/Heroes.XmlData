namespace Heroes.XmlData.StormData;

internal record GameStringText
{
    /// <summary>
    /// Gets the value of the gamestring.
    /// </summary>
    public required string GameStringValue { get; init; }

    /// <summary>
    /// Gets the file path where the gamestring resides from.
    /// </summary>
    public required string FilePath { get; init; }
}
