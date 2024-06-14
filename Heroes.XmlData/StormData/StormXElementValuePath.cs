namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormXElementValuePath(XElement Value, StormPath StormPath)
    : StormValuePath<XElement>(Value, StormPath);
