namespace Heroes.XmlData;

public class HeroesData : IHeroesData
{
    private readonly XmlGameData _xmlGameData = new();
    //private const string _selfName = "HXD";

    //private readonly HashSet<string> _addedXmlFilePathsList = [];
    //private readonly Dictionary<string, List<(string GameString, string FilePath)>> _gameStringsById = [];

    private readonly HashSet<XmlPathLocation> _notFoundDirectoriesList = [];
    private readonly HashSet<XmlPathLocation> _notFoundFilesList = [];

    private string? _stormModName;
    private string? _stormModDirectoryPath;

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

    void IHeroesData.SetCurrentStormMod(string name, string directoryPath)
    {
        _stormModName = name;
        _stormModDirectoryPath = directoryPath;
    }

    void IHeroesData.AddDirectoryNotFound(string notFoundDirectory)
    {
        _notFoundDirectoriesList.Add(new XmlPathLocation()
        {
            StormModName = _stormModName,
            StormModDirectoryPath = _stormModDirectoryPath,
            Path = notFoundDirectory,
        });
    }

    void IHeroesData.AddFileNotFound(string notFoundFile)
    {
        _notFoundFilesList.Add(new XmlPathLocation()
        {
            StormModName = _stormModName,
            StormModDirectoryPath = _stormModDirectoryPath,
            Path = notFoundFile,
        });
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
