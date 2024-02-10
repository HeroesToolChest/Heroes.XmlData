namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormStringValue(string Value, string Path)
    : StormValue<string>(Value, Path);
