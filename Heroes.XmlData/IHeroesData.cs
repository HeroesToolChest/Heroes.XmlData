using Heroes.XmlData.StormData;

namespace Heroes.XmlData;

internal interface IHeroesData
{
    int? HotsBuild { get; }

    HeroesLocalization? HeroesLocalization { get; }

    //void AddXmlStorage(StormModDataContainer xmlStorage);

    //void AddMainXmlFile(XDocument document, string filePath);

    //void AddMainGameStringFile(Stream stream, string filePath);

    //void AddMapXmlFile(XDocument document, string filePath);

    //void AddMapGameStringFile(Stream stream, string filePath);

    //internal void SetCurrentStormMod(string name, string directoryPath);

    //internal void AddDirectoryNotFound(string notFoundDirectory);

    //internal void AddFileNotFound(string notFoundFile);

    //internal void ClearGamestrings();
}
