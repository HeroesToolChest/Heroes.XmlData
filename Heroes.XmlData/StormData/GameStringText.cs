namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a gamestring.
/// </summary>
/// <param name="Value">The value of the gamestring.</param>
/// <param name="Path">The file path where the gamestring resides from.</param>
public record GameStringText(string Value, string Path) : StormStringValue(Value, Path);