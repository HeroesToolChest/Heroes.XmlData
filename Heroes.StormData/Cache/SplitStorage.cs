namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a storage container for different types of data storage, including base, map, and custom data storage.
/// </summary>
internal class SplitStorage
{
    public DataStorage BaseDataStorage { get; } = new();

    public DataStorage MapDataStorage { get; } = new();

    public DataStorage CustomDataStorage { get; } = new();

    public DataStorage GetDataStorage(StormModType stormModType) => stormModType switch
    {
        StormModType.Base => BaseDataStorage,
        StormModType.Map => MapDataStorage,
        StormModType.Custom => CustomDataStorage,
        _ => CustomDataStorage,
    };
}
