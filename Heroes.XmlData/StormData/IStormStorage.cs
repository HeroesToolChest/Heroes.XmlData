namespace Heroes.XmlData.StormData;

internal interface IStormStorage
{
    int? HotsBuild { get; }

    void AddContainer(StormModDataContainer stormModDataContainer);

    void AddDirectoryNotFound(string directory);

    void AddFileNotFound(string notFoundFile);

    void ClearGamestrings();

    StormModDataContainer GetContainerInstance(string stormModName, string stormModDirectoryPath, bool useMapCache = false);
}