namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the properties for a value.
/// </summary>
/// <typeparam name="T">The type of <paramref name="Value"/>.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="StormPath">The relative file where the value resides from.</param>
public abstract record StormValuePath<T>(T Value, StormPath StormPath)
    where T : class;