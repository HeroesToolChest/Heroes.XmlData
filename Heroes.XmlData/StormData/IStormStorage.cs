namespace Heroes.XmlData.StormData;

internal interface IStormStorage
{
    StormCache StormCache { get; }

    StormCache StormMapCache { get; }

    StormCache StormCustomCache { get; }

    List<IStormModStorage> StormModStorages { get; }

    IStormModStorage CreateModStorage(IStormMod stormMod);

    void AddModStorage(IStormModStorage stormModStorage);

    void AddDirectoryNotFound(StormModType stormModType, StormPath stormDirectory);

    void AddFileNotFound(StormModType stormModType, StormPath stormFile);

    void AddGameString(StormModType stormModType, string id, GameStringText gameStringText);

    void AddAssetText(StormModType stormModType, string id, AssetText assetText);

    (string Id, GameStringText GameStringText)? GetGameStringWithId(ReadOnlySpan<char> gamestring, StormPath stormPath);

    (string Id, AssetText AssetText)? GetAssetWithId(ReadOnlySpan<char> asset, StormPath stormPath);

    void AddStormLayoutFilePath(StormModType stormModType, string relativePath, StormPath stormPath);

    void AddAssetFilePath(StormModType stormModType, string relativePath, StormPath stormPath);

    bool AddConstantXElement(StormModType stormModType, XElement element, StormPath stormPath);

    double GetValueFromConstElementAsNumber(XElement constElement);

    string GetValueFromConstTextAsText(ReadOnlySpan<char> text);

    double GetValueFromConstTextAsNumber(ReadOnlySpan<char> text);

    void AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName);

    void AddElement(StormModType stormModType, XElement element, StormPath stormPath);

    void SetStormStyleCache(StormModType stormModType, XDocument document, StormPath stormPath);

    void AddLevelScalingArrayElement(StormModType stormModType, XElement element, StormPath stormPath);

    void AddStormStyleElement(StormModType stormModType, XElement element, StormPath stormPath);

    void BuildDataForScalingAttributes(StormModType stormModType);

    void ClearGamestrings();

    void ClearStormMapMods();

    int? GetBuildId();

    bool TryGetFirstConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    List<string> GetElementTypesByDataObjectType(string dataObjectType);

    bool TryGetFirstDataObjectTypeByElementType(string elementType, [NotNullWhen(true)] out string? dataObjectType);

    string? GetDataObjectTypeByElementType(string elementType);

    StormElement? GetStormElementByElementType(string elementType);

    StormElement? GetStormElementById(string id, string dataObjectType);

    StormElement? GetScaleValueStormElementById(string id, string dataObjectType);

    StormElement? GetCompleteStormElement(string id, string dataObjectType);

    StormElement? GetBaseStormElement(string elementType);

    StormStyleConstantElement? GetStormStyleConstantElementsByName(string name);

    StormStyleStyleElement? GetStormStyleStyleElementsByName(string name);

    StormGameString? GetStormGameString(string id);

    List<StormGameString> GetStormGameStrings();

    List<string> GetStormElementIds(string dataObjectType, StormCacheType stormCacheType = StormCacheType.All);

    StormAssetString? GetStormAssetString(string id);

#if NET9_0_OR_GREATER
    bool TryGetStormAssetStringValue(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value);
#endif

    bool TryGetFirstStormLayoutStormPath(string relativePath, [NotNullWhen(true)] out StormPath? stormPath);

    bool StormLayoutFileExists(string? relativePath);

    StormFile? GetStormLayoutFile(string? relativePath);

    bool StormAssetFileExists(string? relativePath);

    StormFile? GetStormAssetFile(string? relativePath);
}