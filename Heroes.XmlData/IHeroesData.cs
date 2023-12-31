namespace Heroes.XmlData;

internal interface IHeroesData
{
    int? HotsBuild { get; }

    HeroesLocalization? HeroesLocalization { get; }

    void AddMainXmlFile(XDocument document, string filePath);

    void AddMainGameStringFile(Stream stream, string filePath);

    void AddMapXmlFile(XDocument document, string filePath);

    void AddMapGameStringFile(Stream stream, string filePath);

    internal void AddDirectoryNotFound(string directoryPath);

    internal void AddFileNotFound(string filePath);

    internal void ClearGamestrings();
}
