namespace Heroes.XmlData.StormData;

// class for overall storage for all stormmods
internal class StormStorage : IStormStorage
{
    private readonly List<StormModDataContainer> _stormModContainers = [];
    private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private int _loadedMapMods;

    public StormCache StormCache { get; } = new();

    public StormCache StormMapCache { get; } = new();

    public void AddContainer(StormModDataContainer stormModDataContainer)
    {
        _stormModContainers.Add(stormModDataContainer);

        if (stormModDataContainer.IsMapMod)
            _loadedMapMods++;
    }

    public StormModDataContainer CreateContainerInstance(string modsDirectoryPath, StormModDataProperties stormStorageProperties)
    {
        if (stormStorageProperties.IsMapMod is false)
            return new(StormCache, modsDirectoryPath, stormStorageProperties);
        else
            return new(StormMapCache, modsDirectoryPath, stormStorageProperties);
    }

    public void AddDirectoryNotFound(string directory, string stormModName, string stormModDirectoryPath)
    {
        _notFoundDirectoriesList.Add(new RequiredStormFile()
        {
            StormModName = stormModName,
            StormModDirectoryPath = stormModDirectoryPath,
            Path = directory,
        });
    }

    public void AddFileNotFound(string notFoundFile, string stormModName, string stormModDirectoryPath)
    {
        _notFoundFilesList.Add(new RequiredStormFile()
        {
            StormModName = stormModName,
            StormModDirectoryPath = stormModDirectoryPath,
            Path = notFoundFile,
        });
    }

    public void ClearGamestrings()
    {
        _stormModContainers.ForEach(x => x.ClearGameStrings());
        StormCache.GameStringsById.Clear();
        StormMapCache.GameStringsById.Clear();
    }

    public void ClearStormMapMods()
    {
        ClearStormMapContainers();
        StormMapCache.Clear();
    }

    public int? GetBuildId()
    {
        return _stormModContainers.FirstOrDefault()?.BuildId;
    }

    private void ClearStormMapContainers()
    {
        // stormmap mods are always at the end
        for (int i = _stormModContainers.Count - 1; i > 0; i--)
        {
            if (_loadedMapMods < 1)
                break;

            if (_stormModContainers[i].IsMapMod)
            {
                _stormModContainers.RemoveAt(i);
                _loadedMapMods--;
            }
        }
    }
}

