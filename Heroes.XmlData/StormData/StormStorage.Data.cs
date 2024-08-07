﻿using Heroes.XmlData.GameStrings;

namespace Heroes.XmlData.StormData;

/// <content>
/// Contains the methods for obtaining data from the caches.
/// </content>
/// <remarks>
/// <para>The TryGetFirst methods get a reference to a first cache.</para>
/// <para>The Get methods merges the data from all caches (if it can).</para>
/// </remarks>
internal partial class StormStorage
{
    public bool TryGetFirstConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
    {
        return TryGetFirstConstantXElementById(id.ToString(), out stormXElementValuePath);
    }

    public bool TryGetFirstConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
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

    public List<string> GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType)
    {
        return GetElementTypesByDataObjectType(dataObjectType.ToString());
    }

    public List<string> GetElementTypesByDataObjectType(string dataObjectType)
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

        return [.. elementTypes ?? []];
    }

    public bool TryGetFirstDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType)
    {
        return TryGetFirstDataObjectTypeByElementType(elementType.ToString(), out dataObjectType);
    }

    public bool TryGetFirstDataObjectTypeByElementType(string elementType, [NotNullWhen(true)] out string? dataObjectType)
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
        if (TryGetFirstDataObjectTypeByElementType(elementType, out string? dataObjectType))
            return dataObjectType;

        return null;
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

    public StormStyleConstantElement? GetStormStyleConstantElementsByName(ReadOnlySpan<char> name)
    {
        return GetStormStyleConstantElementsByName(name.ToString());
    }

    public StormStyleConstantElement? GetStormStyleConstantElementsByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        StormStyleConstantElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleConstantElementsByName.TryGetValue(name, out StormStyleConstantElement? foundStormElement))
        {
            stormElement ??= new StormStyleConstantElement(foundStormElement);
        }

        if (StormMapCache.StormStyleConstantElementsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleConstantElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleConstantElementsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleConstantElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public StormStyleStyleElement? GetStormStyleStyleElementsByName(ReadOnlySpan<char> name)
    {
        return GetStormStyleStyleElementsByName(name.ToString());
    }

    public StormStyleStyleElement? GetStormStyleStyleElementsByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        StormStyleStyleElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleStyleElementsByName.TryGetValue(name, out StormStyleStyleElement? foundStormElement))
        {
            stormElement ??= new StormStyleStyleElement(foundStormElement);
        }

        if (StormMapCache.StormStyleStyleElementsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleStyleElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleStyleElementsByName.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleStyleElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public StormGameString? GetStormGameString(ReadOnlySpan<char> id)
    {
        return GetStormGameString(id.ToString());
    }

    public StormGameString? GetStormGameString(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        StormGameString? stormGameString = null;

        // normal cache first
        if (StormCache.GameStringsById.TryGetValue(id, out GameStringText? gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        if (StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        if (StormCustomCache.GameStringsById.TryGetValue(id, out gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        static StormGameString Get(string id, StormGameString? stormGameString, GameStringText gameStringText)
        {
            stormGameString ??= new StormGameString(id, gameStringText.Value);
            stormGameString.AddPath(gameStringText.StormPath);
            stormGameString.Value = gameStringText.Value;

            return stormGameString;
        }

        return stormGameString;
    }

    public List<StormGameString> GetStormGameStrings()
    {
        Dictionary<string, StormGameString> stormGameStrings = [];

        foreach (var item in StormCache.GameStringsById)
        {
            AddStormGameString(stormGameStrings, item);
        }

        foreach (var item in StormMapCache.GameStringsById)
        {
            AddStormGameString(stormGameStrings, item);
        }

        foreach (var item in StormCustomCache.GameStringsById)
        {
            AddStormGameString(stormGameStrings, item);
        }

        return stormGameStrings.Select(x => x.Value).ToList();
    }

    public List<string> GetStormElementIds(ReadOnlySpan<char> dataObjectType)
    {
        return GetStormElementIds(dataObjectType.ToString());
    }

    public List<string> GetStormElementIds(string dataObjectType)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);

        HashSet<string> ids = [];

        // normal cache first
        if (StormCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out var foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Values.Select(x => x.Id!));
        }

        if (StormMapCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Values.Select(x => x.Id!));
        }

        if (StormCustomCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Values.Select(x => x.Id!));
        }

        return [.. ids];
    }

    public StormAssetString? GetStormAssetString(ReadOnlySpan<char> id)
    {
        return GetStormAssetString(id.ToString());
    }

    public StormAssetString? GetStormAssetString(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        StormAssetString? stormAssetText = null;

        // normal cache first
        if (StormCache.AssetTextsById.TryGetValue(id, out AssetText? assetText))
            stormAssetText = Get(id, stormAssetText, assetText);

        if (StormMapCache.AssetTextsById.TryGetValue(id, out assetText))
            stormAssetText = Get(id, stormAssetText, assetText);

        if (StormCustomCache.AssetTextsById.TryGetValue(id, out assetText))
            stormAssetText = Get(id, stormAssetText, assetText);

        static StormAssetString Get(string id, StormAssetString? stormAssetText, AssetText assetText)
        {
            stormAssetText ??= new StormAssetString(id, assetText.Value);
            stormAssetText.AddPath(assetText.StormPath);
            stormAssetText.Value = assetText.Value;

            return stormAssetText;
        }

        return stormAssetText;
    }

    public bool TryGetFirstStormLayoutStormPath(string relativePath, [NotNullWhen(true)] out StormPath? stormPath)
    {
        // custom cache always first
        if (StormCustomCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out stormPath))
            return true;

        if (StormMapCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out stormPath))
            return true;

        if (StormCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out stormPath))
            return true;

        return false;
    }

    private static void AddStormGameString(Dictionary<string, StormGameString> stormGameStrings, KeyValuePair<string, GameStringText> item)
    {
        if (stormGameStrings.TryGetValue(item.Key, out StormGameString? existingStormGameString))
        {
            existingStormGameString.Value = item.Value.Value;
            existingStormGameString.AddPath(item.Value.StormPath);
        }
        else
        {
            StormGameString stormGameString = new(item.Key, item.Value.Value);

            stormGameString.AddPath(item.Value.StormPath);

            stormGameStrings.Add(item.Key, stormGameString);
        }
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
