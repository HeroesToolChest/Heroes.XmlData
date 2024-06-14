namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a gamestring.
/// </summary>
/// <param name="Value">The value of the gamestring.</param>
/// <param name="StormPath">The file where the gamestring resides from.</param>
public record GameStringText(string Value, StormPath StormPath) : StormStringValue(Value, StormPath);