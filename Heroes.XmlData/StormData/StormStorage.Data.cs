namespace Heroes.XmlData.StormData;

/// <content>
/// Contains the methods for obtaining data from the caches.
/// </content>
/// <remarks>
/// <para>The TryGetFirst methods get a reference to the first cache.</para>
/// <para>The Get methods merges the data from all caches (if it can).</para>
/// </remarks>
internal sealed partial class StormStorage
{
    public bool TryGetFirstConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath)
    {
        // custom cache always first
        if (StormCustomCache.ConstantXElementByIdAltLookup.TryGetValue(id, out stormXElementValuePath))
            return true;

        if (StormMapCache.ConstantXElementByIdAltLookup.TryGetValue(id, out stormXElementValuePath))
            return true;

        if (StormCache.ConstantXElementByIdAltLookup.TryGetValue(id, out stormXElementValuePath))
            return true;

        return false;
    }

    public List<string> GetElementTypesByDataObjectType(string dataObjectType)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);

        HashSet<string>? elementTypes = null;

        // custom cache always first
        if (StormCustomCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out HashSet<string>? foundElementTypes))
        {
            elementTypes ??= [.. foundElementTypes];
        }

        if (StormMapCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out foundElementTypes))
        {
            if (elementTypes is null)
                elementTypes = [.. foundElementTypes];
            else
                elementTypes.UnionWith(foundElementTypes);
        }

        if (StormCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out foundElementTypes))
        {
            if (elementTypes is null)
                elementTypes = [.. foundElementTypes];
            else
                elementTypes.UnionWith(foundElementTypes);
        }

        return [.. elementTypes ?? []];
    }

    public bool TryGetFirstDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType)
    {
        // custom cache always first
        if (StormCustomCache.DataObjectTypeByElementTypeAltLookup.TryGetValue(elementType, out dataObjectType))
            return true;

        if (StormMapCache.DataObjectTypeByElementTypeAltLookup.TryGetValue(elementType, out dataObjectType))
            return true;

        if (StormCache.DataObjectTypeByElementTypeAltLookup.TryGetValue(elementType, out dataObjectType))
            return true;

        return false;
    }

    public string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType)
    {
        if (TryGetFirstDataObjectTypeByElementType(elementType, out string? dataObjectType))
            return dataObjectType;

        return null;
    }

    public StormElement? GetStormElementByElementType(ReadOnlySpan<char> elementType)
    {
        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormElementByElementTypeAltLookup.TryGetValue(elementType, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormElementByElementTypeAltLookup.TryGetValue(elementType, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormElementByElementTypeAltLookup.TryGetValue(elementType, out foundStormElement))
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
        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.StormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out var foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.StormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
    }

    public bool StormElementExists(string id, string dataObjectType)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(id);

        // normal cache first
        if (StormCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out var foundStormElementById) &&
            foundStormElementById.ContainsKey(id))
        {
            return true;
        }

        if (StormMapCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.ContainsKey(id))
        {
            return true;
        }

        if (StormCustomCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.ContainsKey(id))
        {
            return true;
        }

        return false;
    }

    public bool TryGetFirstStormElementIdByUnitName(string unitName, string dataObjectType, [NotNullWhen(true)] out string? id)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);
        ArgumentNullException.ThrowIfNull(unitName);

        id = null;

        // custom cache first
        if (StormCustomCache.UnitNamesByDataObjectType.TryGetValue(dataObjectType, out var idsByUnitName) &&
            idsByUnitName.TryGetValue(unitName, out id))
            return true;

        if (StormMapCache.UnitNamesByDataObjectType.TryGetValue(dataObjectType, out idsByUnitName) &&
            idsByUnitName.TryGetValue(unitName, out id))
            return true;

        if (StormCache.UnitNamesByDataObjectType.TryGetValue(dataObjectType, out idsByUnitName) &&
            idsByUnitName.TryGetValue(unitName, out id))
            return true;

        return false;
    }

    public string? GetStormElementIdByUnitName(string unitName, string dataObjectType)
    {
        if (TryGetFirstStormElementIdByUnitName(unitName, dataObjectType, out string? id))
            return id;

        return null;
    }

    public StormElement? GetScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType)
    {
        StormElement? stormElement = null;

        // normal cache first
        if (StormCache.ScaleValueStormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out var foundStormElementById) &&
            foundStormElementById.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(id, out StormElement? foundStormElement))
        {
            stormElement ??= new StormElement(foundStormElement);
        }

        if (StormMapCache.ScaleValueStormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(id, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.ScaleValueStormElementsByDataObjectTypeAltLookup.TryGetValue(dataObjectType, out foundStormElementById) &&
            foundStormElementById.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(id, out foundStormElement))
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
        return MergeUpStormElement(dataObjectType, id, ElementType.Normal);
    }

    public StormElement? GetBaseStormElement(ReadOnlySpan<char> elementType)
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
        StormStyleConstantElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleConstantElementsByNameAltLookup.TryGetValue(name, out StormStyleConstantElement? foundStormElement))
        {
            stormElement ??= new StormStyleConstantElement(foundStormElement);
        }

        if (StormMapCache.StormStyleConstantElementsByNameAltLookup.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleConstantElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleConstantElementsByNameAltLookup.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleConstantElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
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
        StormStyleStyleElement? stormElement = null;

        // normal cache first
        if (StormCache.StormStyleStyleElementsByNameAltLookup.TryGetValue(name, out StormStyleStyleElement? foundStormElement))
        {
            stormElement ??= new StormStyleStyleElement(foundStormElement);
        }

        if (StormMapCache.StormStyleStyleElementsByNameAltLookup.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleStyleElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        if (StormCustomCache.StormStyleStyleElementsByNameAltLookup.TryGetValue(name, out foundStormElement))
        {
            if (stormElement is null)
                stormElement = new StormStyleStyleElement(foundStormElement);
            else
                stormElement.AddValue(foundStormElement);
        }

        return stormElement;
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

    public StormGameString? GetStormGameString(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        StormGameString? stormGameString = null;

        // normal cache first
        if (StormCache.GameStringsById.TryGetValue(id, out GameStringFileText? gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        if (StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        if (StormCustomCache.GameStringsById.TryGetValue(id, out gameStringText))
            stormGameString = Get(id, stormGameString, gameStringText);

        static StormGameString Get(string id, StormGameString? stormGameString, GameStringFileText gameStringText)
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

        return [.. stormGameStrings.Select(x => x.Value)];
    }

    public List<string> GetStormElementIds(string dataObjectType, StormCacheType stormCacheType = StormCacheType.All)
    {
        ArgumentNullException.ThrowIfNull(dataObjectType);

        HashSet<string> ids = [];

        // normal cache first
        if (stormCacheType.HasFlag(StormCacheType.Normal) && StormCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out var foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Dictionary.Values.Select(x => x.Id)!);
        }

        if (stormCacheType.HasFlag(StormCacheType.Map) && StormMapCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Dictionary.Values.Select(x => x.Id)!);
        }

        if (stormCacheType.HasFlag(StormCacheType.Custom) && StormCustomCache.StormElementsByDataObjectType.TryGetValue(dataObjectType, out foundStormElementById))
        {
            ids.UnionWith(foundStormElementById.Dictionary.Values.Select(x => x.Id)!);
        }

        return [.. ids];
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

    public bool TryGetStormAssetStringValue(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value)
    {
        value = null;

        // custom cache first
        if (StormCustomCache.AssetTextsByIdAltLookup.TryGetValue(id, out AssetText? assetText))
        {
            value = assetText.Value;
            return true;
        }

        if (StormMapCache.AssetTextsByIdAltLookup.TryGetValue(id, out assetText))
        {
            value = assetText.Value;
            return true;
        }

        if (StormCache.AssetTextsByIdAltLookup.TryGetValue(id, out assetText))
        {
            value = assetText.Value;
            return true;
        }

        return false;
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

    public bool StormLayoutFileExists(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;

        relativePath = PathHelper.NormalizePath(relativePath);

        // custom cache always first
        if (StormCustomCache.UiStormPathsByRelativeUiPath.ContainsKey(relativePath))
            return true;

        if (StormMapCache.UiStormPathsByRelativeUiPath.ContainsKey(relativePath))
            return true;

        if (StormCache.UiStormPathsByRelativeUiPath.ContainsKey(relativePath))
            return true;

        return false;
    }

    public StormFile? GetStormLayoutFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        relativePath = PathHelper.NormalizePath(relativePath);

        StormFile? stormLayoutFile = null;

        // normal cache first
        if (StormCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out StormPath? stormPath))
            stormLayoutFile = GetStormFile(stormLayoutFile, stormPath);

        if (StormMapCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out stormPath))
            stormLayoutFile = GetStormFile(stormLayoutFile, stormPath);

        if (StormCustomCache.UiStormPathsByRelativeUiPath.TryGetValue(relativePath, out stormPath))
            stormLayoutFile = GetStormFile(stormLayoutFile, stormPath);

        return stormLayoutFile;
    }

    public bool StormAssetFileExists(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;

        relativePath = PathHelper.NormalizePath(relativePath);

        // custom cache always first
        if (StormCustomCache.AssetFilesByRelativeAssetsPath.ContainsKey(relativePath))
            return true;

        if (StormMapCache.AssetFilesByRelativeAssetsPath.ContainsKey(relativePath))
            return true;

        if (StormCache.AssetFilesByRelativeAssetsPath.ContainsKey(relativePath))
            return true;

        return false;
    }

    public StormFile? GetStormAssetFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        relativePath = PathHelper.NormalizePath(relativePath);

        StormFile? stormImage = null;

        // normal cache first
        if (StormCache.AssetFilesByRelativeAssetsPath.TryGetValue(relativePath, out StormPath? stormPath))
            stormImage = GetStormFile(stormImage, stormPath);

        if (StormMapCache.AssetFilesByRelativeAssetsPath.TryGetValue(relativePath, out stormPath))
            stormImage = GetStormFile(stormImage, stormPath);

        if (StormCustomCache.AssetFilesByRelativeAssetsPath.TryGetValue(relativePath, out stormPath))
            stormImage = GetStormFile(stormImage, stormPath);

        return stormImage;
    }

    private static void AddStormGameString(Dictionary<string, StormGameString> stormGameStrings, KeyValuePair<string, GameStringFileText> item)
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

    private static StormFile GetStormFile(StormFile? stormFile, StormPath stormPath)
    {
        if (stormFile is null)
            stormFile = new(stormPath);
        else
            stormFile.AddPath(stormPath);

        return stormFile;
    }

    // recursivley travels through the storm element's parents and then add the elements up the chain
    private StormElement? MergeUpStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, ElementType currentElementType)
    {
        StormElement? stormElement = null;

        if (currentElementType == ElementType.Normal && !id.IsEmpty)
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
