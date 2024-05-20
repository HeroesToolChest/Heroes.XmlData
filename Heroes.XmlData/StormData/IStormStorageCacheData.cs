namespace Heroes.XmlData.StormData;

internal interface IStormStorageCacheData
{
    bool TryGetConstantXElementById(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    bool TryGetConstantXElementById(string id, [NotNullWhen(true)] out StormXElementValuePath? stormXElementValuePath);

    bool TryGetStormElementsByDataObjectType(ReadOnlySpan<char> dataObjectType, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetStormElementsByDataObjectType(string dataObjectType, string id, [NotNullWhen(true)] out StormElement? stormElement);

    bool TryGetLevelScalingArrayElement(ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetLevelScalingArrayElement(string catalog, string entry, string field, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleConstantsHexColorValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    bool TryGetStormStyleConstantsHexColorValue(string name, [NotNullWhen(true)] out StormStringValue? stormStringValue);

    double GetValueFromConstElementAsNumber(XElement constElement);

    List<GameStringText> Test();
}
