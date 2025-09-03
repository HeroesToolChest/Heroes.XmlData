namespace Heroes.XmlData.StormData;

internal interface IStormModStorage
{
    IEnumerable<StormPath> AddedGameStringFilePaths { get; }

    IEnumerable<StormPath> AddedXmlDataFilePaths { get; }

    IEnumerable<StormPath> AddedXmlFontStyleFilePaths { get; }

    IEnumerable<StormPath> AddedAssetsTextFilePaths { get; }

    IEnumerable<StormPath> FoundLayoutFilePaths { get; }

    IEnumerable<StormPath> FoundAssetFilePaths { get; }

    int NumberOfNotFoundDirectories { get; }

    int NumberOfNotFoundFiles { get; }

    int NumberOfXmlDataFiles { get; }

    int NumberOfXmlFontStyleFiles { get; }

    int NumberOfGameStringFiles { get; }

    int NumberOfAssetsTextFiles { get; }

    int NumberOfLayoutFiles { get; }

    int NumberOfAssetFiles { get; }

    int? BuildId { get; }

    Dictionary<string, GameStringFileText> GameStringsById { get; }

    string Name { get; }

    IEnumerable<StormPath> NotFoundDirectories { get; }

    IEnumerable<StormPath> NotFoundFiles { get; }

    StormModType StormModType { get; }

    void AddBuildIdFile(Stream stream);

    void AddAssetsTextFile(Stream stream, StormPath stormPath);

    void AddDirectoryNotFound(StormPath requiredStormDirectory);

    void AddFileNotFound(StormPath requiredStormFile);

    void AddGameString(string id, GameStringFileText gameStringText);

    void AddGameStringFile(Stream stream, StormPath stormPath);

    void AddXmlDataFile(XDocument document, StormPath stormPath);

    void AddStormLayoutFilePath(string relativePath, StormPath stormPath);

    void AddAssetFilePath(string relativePath, StormPath stormPath);

    void AddXmlFontStyleFile(XDocument document, StormPath stormPath);

    void ClearGameStrings();

    void UpdateAttributes(IEnumerable<XElement> elements);

    string ToString();
}