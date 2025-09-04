namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for an asset text form an asset text file.
/// </summary>
/// <param name="Value">The value of the asset.</param>
/// <param name="StormPath">The file where the asset resides from.</param>
internal sealed record AssetText(string Value, StormPath StormPath) : StormStringValue(Value, StormPath);