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

    string GetValueFromConstTextAsText(ReadOnlySpan<char> text);

    double GetValueFromConstTextAsNumber(ReadOnlySpan<char> text);

    bool AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName);

    bool AddElement(StormModType stormModType, XElement element, string filePath);

    void SetFontStyleCache(StormModType stormModType, XDocument document, string filePath);

    void SetLevelScalingArrayCache(StormModType stormModType, XElement element, string filePath);

    void ClearGamestrings();

    void ClearStormMapMods();

    int? GetBuildId();
}