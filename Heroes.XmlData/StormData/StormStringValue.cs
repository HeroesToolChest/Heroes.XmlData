namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormStringValue(string Value, StormPath StormPath)
    : StormValuePath<string>(Value, StormPath);
