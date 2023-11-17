namespace Heroes.XmlData.Source;

internal interface IFileHeroesSource : IHeroesSource
{
    void LoadXmlFile(string filePath);
}
