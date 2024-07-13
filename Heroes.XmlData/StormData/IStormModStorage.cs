namespace Heroes.XmlData.StormData;

internal interface IStormModStorage
{
    IEnumerable<StormPath> AddedGameStringFilePaths { get; }

    IEnumerable<StormPath> AddedXmlDataFilePaths { get; }

    IEnumerable<StormPath> AddedXmlFontStyleFilePaths { get; }

    int? BuildId { get; }

    Dictionary<string, GameStringText> GameStringsById { get; }

    string Name { get; }

    IEnumerable<StormPath> NotFoundDirectories { get; }

    IEnumerable<StormPath> NotFoundFiles { get; }

    StormModType StormModType { get; }

    void AddBuildIdFile(Stream stream);

    void AddDirectoryNotFound(StormPath requiredStormDirectory);

    void AddFileNotFound(StormPath requiredStormFile);

    void AddGameString(string id, GameStringText gameStringText);

    void AddGameStringFile(Stream stream, StormPath stormPath);

    void AddXmlDataFile(XDocument document, StormPath stormPath, bool isBaseGameDataDirectory);

    void AddXmlFontStyleFile(XDocument document, StormPath stormPath);

    void ClearGameStrings();

    void UpdateConstantAttributes(IEnumerable<XElement> elements);

    string ToString();
}