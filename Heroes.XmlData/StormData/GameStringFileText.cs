namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for an original gamestring from a gamestring file.
/// </summary>
/// <param name="Value">The original text of the gamestring directly from a gamestring file.</param>
/// <param name="StormPath">The file where the gamestring resides from.</param>
internal sealed record GameStringFileText(string Value, StormPath StormPath) : StormStringValue(Value, StormPath);