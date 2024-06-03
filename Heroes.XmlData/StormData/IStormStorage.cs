namespace Heroes.XmlData.StormData;

internal interface IStormStorage
{
    StormCache StormCache { get; }

    StormCache StormMapCache { get; }

    StormCache StormCustomCache { get; }

    StormModStorage CreateModStorage(IStormMod stormMod, string modsDirectoryPath);

    void AddModStorage(StormModStorage stormModStorage);

    void AddDirectoryNotFound(StormModType stormModType, StormFile stormDirectory);

    void AddFileNotFound(StormModType stormModType, StormFile stormFile);

    void AddGameString(StormModType stormModType, string id, GameStringText gameStringText);

    (string Id, GameStringText GameStringText)? GetGameStringWithId(ReadOnlySpan<char> gamestring, ReadOnlySpan<char> path);

    bool AddConstantXElement(StormModType stormModType, XElement element, string path);

    double GetValueFromConstElementAsNumber(XElement constElement);

    string GetValueFromConstTextAsText(ReadOnlySpan<char> text);

    double GetValueFromConstTextAsNumber(ReadOnlySpan<char> text);

    void AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName);

    void AddElement(StormModType stormModType, XElement element, string filePath);

    void SetFontStyleCache(StormModType stormModType, XDocument document, string filePath);

    void AddLevelScalingArrayElement(StormModType stormModType, XElement element, string filePath);

    void AddStormStyleHexColor(StormModType stormModType, XElement element, string filePath);

    void BuildDataForScalingAttributes(StormModType stormModType);

    void ClearGamestrings();

    void ClearStormMapMods();

    int? GetBuildId();

    bool TryGetExistingConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    bool TryGetExistingConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    StormXElementValuePath? GetConstantXElementById(ReadOnlySpan<char> id);

    StormXElementValuePath? GetConstantXElementById(string id);

    bool TryGetExistingElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes);

    bool TryGetExistingElementTypesByDataObjectType(string dataObjectType, [NotNullWhen(true)] out HashSet<string>? elementTypes);

    HashSet<string>? GetElementTypesByDataObjectType(ReadOnlySpan<char> dataObjectType);

    HashSet<string>? GetElementTypesByDataObjectType(string dataObjectType);

    bool TryGetExistingDataObjectTypeByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out string? dataObjectType);

    bool TryGetExistingDataObjectTypeByElementType(string elementType, [NotNullWhen(true)] out string? dataObjectType);

    string? GetDataObjectTypeByElementType(ReadOnlySpan<char> elementType);

    string? GetDataObjectTypeByElementType(string elementType);

    bool TryGetExistingStormElementByElementType(ReadOnlySpan<char> elementType, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetExistingStormElementByElementType(string elementType, [NotNullWhen(true)] out StormElement? stormElement);

    StormElement? GetStormElementByElementType(ReadOnlySpan<char> elementType);

    StormElement? GetStormElementByElementType(string elementType);

    bool TryGetExistingStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetExistingStormElementByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement);

    StormElement? GetStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id);

    StormElement? GetStormElementByDataObjectType(string dataObjectType, string id);

    bool TryGetExistingScaleValueStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetExistingScaleValueStormElementByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement);

    StormElement? GetScaleValueStormElementByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id);

    StormElement? GetScaleValueStormElementByDataObjectType(string dataObjectType, string id);

    StormElement? GetCompleteStormElement(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id);

    StormElement? GetCompleteStormElement(string dataObjectType, string id);

    StormElement? GetBaseStormElement(ReadOnlySpan<char> elementType);

    StormElement? GetBaseStormElement(string elementType);

    bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleConstantsHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleConstantsHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetLevelScalingArrayElement(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetLevelScalingArrayElement(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    List<GameStringText> Test();
}