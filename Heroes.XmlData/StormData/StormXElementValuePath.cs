namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public sealed record StormXElementValuePath(XElement Value, StormPath StormPath)
    : StormValuePath<XElement>(Value, StormPath);
