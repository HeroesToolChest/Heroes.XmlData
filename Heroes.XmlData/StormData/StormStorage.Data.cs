using Heroes.XmlData.GameStrings;

namespace Heroes.XmlData.StormData;

/// <content>
/// Contains the methods for obtaining data from the caches.
/// </content>
/// <remarks>
/// The TryGet methods get a reference to a single cache, where as the Get methods merges the data from all caches (if it can).
/// </remarks>
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

    public bool TryGetExistingStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetExistingStormElementById(id.ToString(), dataObjectType.ToString(), out stormElement);
    }

    public bool TryGetExistingStormElementById(string id, string dataObjectType, [NotNullWhen(true)] out StormElement? stormElement)
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

    public StormElement? GetStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return GetStormElementById(id.ToString(), dataObjectType.ToString());
    }

    public StormElement? GetStormElementById(string id, string dataObjectType)
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

    public bool TryGetExistingScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out StormElement? stormElement)
    {
        return TryGetExistingScaleValueStormElementById(id.ToString(), dataObjectType.ToString(), out stormElement);
    }

    public bool TryGetExistingScaleValueStormElementById(string id, string dataObjectType, [NotNullWhen(true)] out StormElement? stormElement)
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

    public StormElement? GetScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return GetScaleValueStormElementById(id.ToString(), dataObjectType.ToString());
    }

    public StormElement? GetScaleValueStormElementById(string id, string dataObjectType)
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

    public StormElement? GetCompleteStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        return GetCompleteStormElement(id.ToString(), dataObjectType.ToString());
    }

    public StormElement? GetCompleteStormElement(string id, string dataObjectType)
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

    public StormElement? GetStormStyleConstantsByName(ReadOnlySpan<char> name)
    {
        return GetStormStyleConstantsByName(name.ToString());
    }

    public StormElement? GetStormStyleConstantsByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleConstantsByName.TryGetValue(name, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormStyleConstantsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleConstantsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public StormElement? GetStormStyleStylesByName(ReadOnlySpan<char> name)
    {
        return GetStormStyleStylesByName(name.ToString());
    }

    public StormElement? GetStormStyleStylesByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleStylesByName.TryGetValue(name, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormStyleStylesByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleStylesByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public List<GameStringText> Test()
    {
        return StormCache.GameStringsById.Values.ToList();
    }

    private StormElement? MergeUpStormElement(string dataObjectType, string? id, ElementType currentElementType)
    {
        StormElement? stormElement = null;

        if (currentElementType == ElementType.Normal && id is not null)
            stormElement = GetStormElementById(id, dataObjectType);
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

            if (baseElement is not null && !baseElement.ElementType.Equals(stormElement.ElementType, StringComparison.OrdinalIgnoreCase))
                baseElement.AddValue(stormElement);

            return baseElement ?? stormElement;
        }

        return stormElement;
    }
}
