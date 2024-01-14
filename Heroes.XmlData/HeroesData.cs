using Heroes.XmlData.StormData;

namespace Heroes.XmlData;

public class HeroesData : IHeroesData
{
    private readonly IStormStorage _stormStorage;

    //private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    //private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private string? _stormModName;
    private string? _stormModDirectoryPath;

    public HeroesData(IStormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    public int? HotsBuild { get; }

    public HeroesLocalization? HeroesLocalization { get; private set; }

    //public void AddXmlStorage(StormModDataContainer xmlStorage)
    //{
    //    _xmlGameData.AddContainer(xmlStorage);
    //}

    //void IHeroesData.SetCurrentStormMod(string name, string directoryPath)
    //{
    //    _stormModName = name;
    //    _stormModDirectoryPath = directoryPath;
    //}

    //void IHeroesData.AddDirectoryNotFound(string notFoundDirectory)
    //{
    //    _notFoundDirectoriesList.Add(new RequiredStormFile()
    //    {
    //        StormModName = _stormModName,
    //        StormModDirectoryPath = _stormModDirectoryPath,
    //        Path = notFoundDirectory,
    //    });
    //}

    //void IHeroesData.AddFileNotFound(string notFoundFile)
    //{
    //    _notFoundFilesList.Add(new RequiredStormFile()
    //    {
    //        StormModName = _stormModName,
    //        StormModDirectoryPath = _stormModDirectoryPath,
    //        Path = notFoundFile,
    //    });
    //}

    //void IHeroesData.ClearGamestrings()
    //{
    //    _xmlGameData.ClearGamestrings();
    //}

    //internal void SetHeroesLocalization(HeroesLocalization localization)
    //{
    //    HeroesLocalization = localization;
    //}
}
