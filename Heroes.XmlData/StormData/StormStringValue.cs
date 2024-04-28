namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormStringValue(string Value, string Path)
    : StormValuePath<string>(Value, Path);
