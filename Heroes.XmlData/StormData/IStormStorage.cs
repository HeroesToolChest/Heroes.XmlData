namespace Heroes.XmlData.StormData;

internal interface IStormStorage
{
    StormCache StormCache { get; }

    StormCache StormMapCache { get; }

    StormCache StormCustomCache { get; }

    void AddContainer(StormModDataContainer stormModDataContainer);

    void AddDirectoryNotFound(string directory, string stormModName, string stormModDirectoryPath);

    void AddFileNotFound(string notFoundFile, string stormModName, string stormModDirectoryPath);

    void ClearGamestrings();

    void ClearStormMapMods();

    int? GetBuildId();

    StormModDataContainer CreateContainerInstance(string modsDirectoryPath, StormModDataProperties stormStorageProperties);
}