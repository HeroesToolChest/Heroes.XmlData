namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a gamestring.
/// </summary>
/// <param name="Value">The value of the gamestring.</param>
/// <param name="FilePath">The file path where the gamestring resides from.</param>
public record GameStringText(string Value, string FilePath) : StormStringValue(Value, FilePath);