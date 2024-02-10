namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a value.
/// </summary>
/// <typeparam name="T">The type of <paramref name="Value"/>.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="Path">The relative file path where the value resides from.</param>
public abstract record StormValue<T>(T Value, string Path)
    where T : class;