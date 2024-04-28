namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormXElementValuePath(XElement Value, string Path)
    : StormValuePath<XElement>(Value, Path);
