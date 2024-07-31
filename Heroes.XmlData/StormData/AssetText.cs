namespace Heroes.XmlData.StormData;

/// <summary>
/// Containst the properties for an asset text.
/// </summary>
/// <param name="Value">The value of the asset.</param>
/// <param name="StormPath">The file where the asset resides from.</param>
public record AssetText(string Value, StormPath StormPath) : StormStringValue(Value, StormPath);