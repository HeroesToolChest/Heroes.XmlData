namespace Heroes.XmlData;

internal interface IHeroesData
{
    int? HotsBuild { get; }

    HeroesLocalization? HeroesLocalization { get; }

    void AddXmlFile(XDocument document, string filePath);

    void AddGameStringFile(Stream stream, string filePath);

    internal void AddDirectoryNotFound(string directoryPath);

    internal void AddFileNotFound(string filePath);

    internal void ClearGamestrings();
}
