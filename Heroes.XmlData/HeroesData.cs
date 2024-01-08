namespace Heroes.XmlData;

public class HeroesData : IHeroesData
{
    private readonly XmlGameData _xmlGameData = new();
    //private const string _selfName = "HXD";

    //private readonly HashSet<string> _addedXmlFilePathsList = [];
    //private readonly Dictionary<string, List<(string GameString, string FilePath)>> _gameStringsById = [];

    private readonly HashSet<string> _notFoundDirectoriesList = [];
    private readonly HashSet<string> _notFoundFilesList = [];

    //private XDocument _xmlGameData = new();

    public HeroesData(int? hotsBuild = null)
    {
        HotsBuild = hotsBuild;
    }

    public int? HotsBuild { get; }

    public HeroesLocalization? HeroesLocalization { get; private set; }

    public void AddXmlStorage(XmlStorage xmlStorage)
    {
        _xmlGameData.Add(xmlStorage);
    }

    //public void AddMainXmlFile(XDocument document, string filePath)
    //{
    //    _xmlGameData.XmlMainData.AddXmlFile(document, filePath);
    //}

    //public void AddMainGameStringFile(Stream stream, string filePath)
    //{
    //    _xmlGameData.XmlMainData.AddGameStringFile(stream, filePath);
    //}

    void IHeroesData.AddDirectoryNotFound(string directoryPath)
    {
        _notFoundDirectoriesList.Add(directoryPath);
    }

    void IHeroesData.AddFileNotFound(string filePath)
    {
        _notFoundFilesList.Add(filePath);
    }

    void IHeroesData.ClearGamestrings()
    {
        _xmlGameData.ClearGamestrings();
    }

    internal void SetHeroesLocalization(HeroesLocalization localization)
    {
        HeroesLocalization = localization;
    }
}
