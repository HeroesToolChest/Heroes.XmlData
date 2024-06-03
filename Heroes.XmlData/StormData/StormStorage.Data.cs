using Heroes.XmlData.GameStrings;

namespace Heroes.XmlData.StormData;

/// <content>
/// Contains the methods for obtaining data from the caches.
/// </content>
internal partial class StormStorage
{
    public bool TryGetExistingConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
    {
        return TryGetExistingConstantXElementById(id.ToString(), out stormXElementValuePath);
    }

    public bool TryGetExistingConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
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

    public StormXElementValuePath? GetConstantXElementById(ReadOnlySpan<char> id)
    {
        return GetConstantXElementById(id.ToString());
    }

    public StormXElementValuePath? GetConstantXElementById(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (TryGetExistingConstantXElementById(id, out StormXElementValuePath? stormXElementValuePath))
            return new StormXElementValuePath(stormXElementValuePath.Value, stormXElementValuePath.Path);

        return null;
    }

    public bool TryGetExistingElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes)
    {
        return TryGetExistingElementTypesByDataObjectType(dataObjectType.ToString(), out elementTypes);
    }

    public bool TryGetExistingElementTypesByDataObjectType(string dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes)
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

    public HashSet<string>? GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return GetElementTypesByDataObjectType(dataObjectType.ToString());
    }

    public HashSet<string>? GetElementTypesByDataObjectType(string dataObjectType)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);

        HashSet<string>? elementTypes = null;

        // custom cache always first
        if (StormCustomCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out HashSet<string>? foundElementTypes))
        {
            elementTypes ??= new(foundElementTypes);
        }

        if (StormMapCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out foundElementTypes))
        {
            if (elementTypes is null)
                elementTypes = new(foundElementTypes);
            else
                elementTypes.UnionWith(foundElementTypes);
        }

        if (StormCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out foundElementTypes))
        {
            if (elementTypes is null)
                elementTypes = new(foundElementTypes);
            else
                elementTypes.UnionWith(foundElementTypes);
        }

        return elementTypes;
    }

    public bool TryGetExistingDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType)
    {
        return TryGetExistingDataObjectTypeByElementType(elementType.ToString(), out dataObjectType);
    }

    public bool TryGetExistingDataObjectTypeByElementType(string elementType, [NotNullWhen(true)] out string? dataObjectType)
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

    public string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType)
    {
        return GetDataObjectTypeByElementType(elementType.ToString());
    }

    public string? GetDataObjectTypeByElementType(string elementType)
    {
        if (TryGetExistingDataObjectTypeByElementType(elementType, out string? dataObjectType))
            return dataObjectType;

        return null;
    }

    public bool TryGetExistingStormElementByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetExistingStormElementByElementType(elementType.ToString(), out stormElement);
    }

    public bool TryGetExistingStormElementByElementType(string elementType, [NotNullWhen(true)] out StormElement? stormElement)
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

    public StormElement? GetStormElementByElementType(ReadOnlySpan<char> elementType)
    {
        return GetStormElementByElementType(elementType.ToString());
    }

    public StormElement? GetStormElementByElementType(string elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormElementByElementType.TryGetValue(elementType, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormElementByElementType.TryGetValue(elementType, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormElementByElementType.TryGetValue(elementType, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public bool TryGetExistingStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetExistingStormElementByDataObjectType(dataObjectType.ToString(), id.ToString(), out stormElement);
    }

    public bool TryGetExistingStormElementByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement)
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

    public StormElement? GetStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return GetStormElementByDataObjectType(dataObjectType.ToString(), id.ToString());
    }

    public StormElement? GetStormElementByDataObjectType(string dataObjectType, string id)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(id);

        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out var foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public bool TryGetExistingScaleValueStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetExistingScaleValueStormElementByDataObjectType(dataObjectType.ToString(), id.ToString(), out stormElement);
    }

    public bool TryGetExistingScaleValueStormElementByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(id);

        stormElement = null;

        // custom cache always first
        if (StormCustomCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out var stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        if (StormMapCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        if (StormCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out stormElementById) &&
            stormElementById.TryGetValue(id, out stormElement))
            return true;

        return false;
    }

    public StormElement? GetScaleValueStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return GetScaleValueStormElementByDataObjectType(dataObjectType.ToString(), id.ToString());
    }

    public StormElement? GetScaleValueStormElementByDataObjectType(string dataObjectType, string id)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(id);

        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out var foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.ScaleValueStormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id)
    {
        return GetCompleteStormElement(dataObjectType.ToString(), id.ToString());
    }

    public StormElement? GetCompleteStormElement(string dataObjectType, string id)
    {
        return MergeUpStormElement(dataObjectType, id, ElementType.Normal);
    }

    public StormElement? GetBaseStormElement(ReadOnlySpan<char> elementType)
    {
        return GetBaseStormElement(elementType.ToString());
    }

    public StormElement? GetBaseStormElement(string elementType)
    {
        string? dataObjectType = GetDataObjectTypeByElementType(elementType);

        if (!string.IsNullOrEmpty(dataObjectType))
        {
            // best guess
            return GetStormElementByElementType($"C{dataObjectType}");
        }

        return null;
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

    public List<GameStringText> Test()
    {
        return StormCache.GameStringsById.Values.ToList();
    }

    private StormElement? MergeUpStormElement(string dataObjectType, string? id, ElementType currentElementType)
    {
        StormElement? stormElement = null;

        if (currentElementType == ElementType.Normal && id is not null)
            stormElement = GetStormElementByDataObjectType(dataObjectType, id);
        else if (currentElementType == ElementType.Type)
            stormElement = GetStormElementByElementType(dataObjectType);
        else if (currentElementType == ElementType.Base)
            stormElement = GetBaseStormElement(dataObjectType);

        if (stormElement is null)
            return stormElement;

        if (stormElement.HasParentId)
        {
            // parents
            StormElement? parentElement = MergeUpStormElement(dataObjectType, stormElement.ParentId, ElementType.Normal);
            parentElement?.AddValue(stormElement);

            return parentElement ?? stormElement;
        }
        else if (currentElementType == ElementType.Normal)
        {
            // then check the element type, which has no id attribute
            StormElement? typeElement = MergeUpStormElement(stormElement.ElementType, null, ElementType.Type);
            typeElement?.AddValue(stormElement);

            return typeElement ?? stormElement;
        }
        else if (currentElementType == ElementType.Type)
        {
            // then check the base element type, may not be the correct one, but close enough
            StormElement? baseElement = MergeUpStormElement(stormElement.ElementType, null, ElementType.Base);
            baseElement?.AddValue(stormElement);

            return baseElement ?? stormElement;
        }

        return stormElement;
    }
}
