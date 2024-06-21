namespace Heroes.XmlData.StormData;

internal interface IStormStorage
{
    StormCache StormCache { get; }

    StormCache StormMapCache { get; }

    StormCache StormCustomCache { get; }

    StormModStorage CreateModStorage(IStormMod stormMod);

    void AddModStorage(StormModStorage stormModStorage);

    void AddDirectoryNotFound(StormModType stormModType, StormPath stormDirectory);

    void AddFileNotFound(StormModType stormModType, StormPath stormFile);

    void AddGameString(StormModType stormModType, string id, GameStringText gameStringText);

    (string Id, GameStringText GameStringText)? GetGameStringWithId(ReadOnlySpan<char> gamestring, StormPath stormPath);

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

    bool TryGetExistingConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    bool TryGetExistingConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

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

    bool TryGetExistingStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetExistingStormElementById(string id, string dataObjectType, [NotNullWhen(true)] out StormElement? stormElement);

    StormElement? GetStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    StormElement? GetStormElementById(string id, string dataObjectType);

    bool TryGetExistingScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetExistingScaleValueStormElementById(string id, string dataObjectType, [NotNullWhen(true)] out StormElement? stormElement);

    StormElement? GetScaleValueStormElementById(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    StormElement? GetScaleValueStormElementById(string id, string dataObjectType);

    StormElement? GetCompleteStormElement(ReadOnlySpan<char> id, ReadOnlySpan<char> dataObjectType);

    StormElement? GetCompleteStormElement(string id, string dataObjectType);

    StormElement? GetBaseStormElement(ReadOnlySpan<char> elementType);

    StormElement? GetBaseStormElement(string elementType);

    StormStyleConstantElement? GetStormStyleConstantElementsByName(ReadOnlySpan<char> name);

    StormStyleConstantElement? GetStormStyleConstantElementsByName(string name);

    StormStyleStyleElement? GetStormStyleStyleElementsByName(ReadOnlySpan<char> name);

    StormStyleStyleElement? GetStormStyleStyleElementsByName(string name);

    StormGameString? GetStormGameString(ReadOnlySpan<char> id);

    StormGameString? GetStormGameString(string id);

    List<StormGameString> GetStormGameStrings();
}