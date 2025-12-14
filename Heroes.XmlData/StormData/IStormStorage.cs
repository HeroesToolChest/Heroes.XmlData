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

    void AddGameString(StormModType stormModType, string id, GameStringFileText gameStringText);

    void AddAssetText(StormModType stormModType, string id, AssetText assetText);

    (string Id, GameStringFileText GameStringText)? GetGameStringWithId(string gamestring, StormPath stormPath);

    (string Id, AssetText AssetText)? GetAssetWithId(string asset, StormPath stormPath);

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

    void ClearCustomMods();

    int? GetBuildId();

    bool TryGetFirstConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    List<string> GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType);

    bool TryGetFirstDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType);

    string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType);

    StormElement? GetStormElementByElementType(ReadOnlySpan<char> elementType);

    StormElement? GetStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    bool StormElementExists(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    bool TryGetFirstStormElementIdByUnitName(ReadOnlySpan<char> unitName, ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out string? id);

    string? GetStormElementIdByUnitName(ReadOnlySpan<char> unitName, ReadOnlySpan<char> dataObjectType);

    StormElement? GetScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    StormElement? GetCompleteStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    StormElement? GetBaseStormElement(ReadOnlySpan<char> elementType);

    StormStyleConstantElement? GetStormStyleConstantElementsByName(ReadOnlySpan<char> name);

    StormStyleStyleElement? GetStormStyleStyleElementsByName(ReadOnlySpan<char> name);

    StormGameString? GetStormGameString(string id);

    string? GetStormGameString(ReadOnlySpan<char> id);

    List<StormGameString> GetStormGameStrings();

    List<string> GetStormElementIds(string dataObjectType, StormCacheType stormCacheType = StormCacheType.All);

    StormAssetString? GetStormAssetString(string id);

    string? GetStormAssetString(ReadOnlySpan<char> id);

    bool TryGetStormAssetStringValue(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? value);

    bool TryGetFirstStormLayoutStormPath(string relativePath, [NotNullWhen(true)] out StormPath? stormPath);

    bool StormLayoutFileExists(string? relativePath);

    StormFile? GetStormLayoutFile(string? relativePath);

    bool StormAssetFileExists(string? relativePath);

    StormFile? GetStormAssetFile(string? relativePath);
}