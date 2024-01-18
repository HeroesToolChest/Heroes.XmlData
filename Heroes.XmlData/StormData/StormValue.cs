namespace Heroes.XmlData.StormData;

internal record StormValue<T>(string Path, T Value)
    where T : IConvertible;
