namespace Heroes.XmlData.StormData;

internal interface IStormStorageCacheData
{
    bool TryGetStormElementsByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetStormElementsByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetLevelScalingArrayElement(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetLevelScalingArrayElement(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue);
}
