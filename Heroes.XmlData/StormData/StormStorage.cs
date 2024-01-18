namespace Heroes.XmlData.StormData;

internal class StormStorage : IStormStorage
{
    private readonly string _modsDirectoryPath;
    private readonly int? _hotsBuild;

    private readonly List<StormModDataContainer> _stormModContainers = [];

    private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private readonly StormCache _stormCache = new();
    private readonly StormCache _stormMapCache = new();

    private string? _stormModName;
    private string? _stormModDirectoryPath;

    public StormStorage(string modsDirectoryPath, int? hotsBuild)
    {
        _modsDirectoryPath = modsDirectoryPath;
        _hotsBuild = hotsBuild;
    }

    public int? HotsBuild => _hotsBuild;

    public string ModsDirectoryPath => _modsDirectoryPath;

    public void AddContainer(StormModDataContainer stormModDataContainer)
    {
        _stormModContainers.Add(stormModDataContainer);
    }

    public StormModDataContainer GetContainerInstance(string stormModName, string stormModDirectoryPath, bool useMapCache = false)
    {
        _stormModName = stormModName;
        _stormModDirectoryPath = stormModDirectoryPath;

        if (useMapCache is false)
            return new(_stormCache, _modsDirectoryPath, _stormModName, _stormModDirectoryPath);
        else
            return new(_stormMapCache, _modsDirectoryPath, _stormModName, _stormModDirectoryPath);
    }

    public void AddDirectoryNotFound(string directory)
    {
        _notFoundDirectoriesList.Add(new RequiredStormFile()
        {
            StormModName = _stormModName,
            StormModDirectoryPath = _stormModDirectoryPath,
            Path = directory,
        });
    }

    public void AddFileNotFound(string notFoundFile)
    {
        _notFoundFilesList.Add(new RequiredStormFile()
        {
            StormModName = _stormModName,
            StormModDirectoryPath = _stormModDirectoryPath,
            Path = notFoundFile,
        });
    }

    public void ClearGamestrings()
    {
        _stormModContainers.ForEach(x => x.ClearGameStrings());
    }
}

