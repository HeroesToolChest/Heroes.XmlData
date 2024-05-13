namespace Heroes.XmlData.StormData;

/// <content>
/// Contains the methods for obtaining data from the caches.
/// </content>
internal partial class StormStorage : IStormStorageCacheData
{
    public bool TryGetConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
    {
        return TryGetConstantXElementById(id.ToString(), out stormXElementValuePath);
    }

    public bool TryGetConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (StormCustomCache.ConstantXElementById.TryGetValue(id, out stormXElementValuePath))
            return true;

        if (StormMapCache.ConstantXElementById.TryGetValue(id, out stormXElementValuePath))
            return true;

        if (StormCache.ConstantXElementById.TryGetValue(id, out stormXElementValuePath))
            return true;

        return false;
    }

    public bool TryGetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes)
    {
        return TryGetElementTypesByDataObjectType(dataObjectType.ToString(), out elementTypes);
    }

    public bool TryGetElementTypesByDataObjectType(string dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);

        // custom cache always first
        if (StormCustomCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out elementTypes))
            return true;

        if (StormMapCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out elementTypes))
            return true;

        if (StormCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out elementTypes))
            return true;

        return false;
    }

    public string FindExistingDataObjectType(string elementName)
    {
        // normal cache first
        string? foundExistingDataObjectType = StormCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x));

        foundExistingDataObjectType ??= StormMapCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x));
        foundExistingDataObjectType ??= StormCustomCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x)) ??
            throw new Exception("TODO");

        return foundExistingDataObjectType;
    }

    public bool TryGetDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType)
    {
        return TryGetDataObjectTypeByElementType(elementType.ToString(), out dataObjectType);
    }

    public bool TryGetDataObjectTypeByElementType(string elementType, [NotNullWhen(true)] out string? dataObjectType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        // custom cache always first
        if (StormCustomCache.DataObjectTypeByElementType.TryGetValue(elementType, out dataObjectType))
            return true;

        if (StormMapCache.DataObjectTypeByElementType.TryGetValue(elementType, out dataObjectType))
            return true;

        if (StormCache.DataObjectTypeByElementType.TryGetValue(elementType, out dataObjectType))
            return true;

        return false;
    }

    public bool TryGetStormElementByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetStormElementByElementType(elementType.ToString(), out stormElement);
    }

    public bool TryGetStormElementByElementType(string elementType, [NotNullWhen(true)] out StormElement? stormElement)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        // custom cache always first
        if (StormCustomCache.StormElementByElementType.TryGetValue(elementType, out stormElement))
            return true;

        if (StormMapCache.StormElementByElementType.TryGetValue(elementType, out stormElement))
            return true;

        if (StormCache.StormElementByElementType.TryGetValue(elementType, out stormElement))
            return true;

        return false;
    }

    public bool TryGetStormElementsByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetStormElementsByDataObjectType(dataObjectType.ToString(), id.ToString(), out stormElement);
    }

    public bool TryGetStormElementsByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(id);

        stormElement = null;

        // custom cache always first
        if (StormCustomCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out var stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        if (StormMapCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        if (StormCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        return false;
    }

    public bool TryGetLevelScalingArrayElement(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        return TryGetLevelScalingArrayElement(catalog.ToString(), entry.ToString(), field.ToString(), out stormStringValue);
    }

    public bool TryGetLevelScalingArrayElement(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        // custom cache always first
        if (StormCustomCache.ScaleValueByEntry.TryGetValue(new(catalog, entry, field), out stormStringValue))
            return true;

        if (StormMapCache.ScaleValueByEntry.TryGetValue(new(catalog, entry, field), out stormStringValue))
            return true;

        if (StormCache.ScaleValueByEntry.TryGetValue(new(catalog, entry, field), out stormStringValue))
            return true;

        return false;
    }

    public bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        return TryGetStormStyleHexColorValue(name.ToString(), out stormStringValue);
    }

    public bool TryGetStormStyleHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(name);

        // custom cache always first
        if (StormCustomCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (StormMapCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (StormCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        return false;
    }

    public bool TryGetStormStyleConstantsHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        return TryGetStormStyleConstantsHexColorValue(name.ToString(), out stormStringValue);
    }

    public bool TryGetStormStyleConstantsHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(name);

        // custom cache always first
        if (StormCustomCache.StormStyleConstantsHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (StormMapCache.StormStyleConstantsHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (StormCache.StormStyleConstantsHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        return false;
    }
}
