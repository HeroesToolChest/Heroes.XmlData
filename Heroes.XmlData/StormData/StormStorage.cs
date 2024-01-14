namespace Heroes.XmlData.StormData;

public class StormStorage : IStormStorage
{
    private readonly List<StormModDataContainer> _stormModContainers = [];

    private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private string? _stormModName;
    private string? _stormModDirectoryPath;

    public StormStorage(int? hotsBuild)
    {
        HotsBuild = hotsBuild;
    }

    public int? HotsBuild { get; }

    public void AddContainer(StormModDataContainer stormModDataContainer)
    {
        _stormModContainers.Add(stormModDataContainer);
    }

    public StormModDataContainer GetContainerInstance(string stormModName, string stormModDirectoryPath)
    {
        _stormModName = stormModName;
        _stormModDirectoryPath = stormModDirectoryPath;

        return new(_stormModName, _stormModDirectoryPath);
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

