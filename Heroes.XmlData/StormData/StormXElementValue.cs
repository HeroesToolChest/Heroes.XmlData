namespace Heroes.XmlData.StormData;

/// <inheritdoc/>
public record StormXElementValue(XElement Value, string Path)
    : StormValue<XElement>(Value, Path);
