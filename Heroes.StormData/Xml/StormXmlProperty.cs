using System.Runtime.InteropServices;

namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a property, element or attribute, for an XML element.
/// </summary>
public sealed class StormXmlProperty
{
    private readonly Dictionary<string, StormXmlArray> _propertiesByName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StormXmlArray>.AlternateLookup<ReadOnlySpan<char>> _propertiesByNameAltLookup;

    internal StormXmlProperty(string name)
    {
        _propertiesByNameAltLookup = _propertiesByName.GetAlternateLookup<ReadOnlySpan<char>>();

        Name = name;
    }

    /// <summary>
    /// Gets the name of the property. Can either be either from an element or attribute name.
    /// </summary>
    public string Name { get; private set; }

    public StormXmlValue? Value { get; private set; }

    // TODO: set to private set;
    public bool IsArray { get; internal set; }

    /// <summary>
    /// Gets the number of unique properties in the current property (elements that are arrays count as 1).
    /// </summary>
    public int Count => _propertiesByName.Count;

    /// <summary>
    /// Gets the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The property with the specified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlArray this[string name] => GetProperty(name);

    /// <summary>
    /// Gets the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The property with the specified name.</returns>
    public StormXmlArray this[ReadOnlySpan<char> name] => GetProperty(name);

    /// <summary>
    /// Gets the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The property with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the property with the specified name does not exist.</exception>
    public StormXmlArray GetProperty(string name)
    {
        if (TryGetProperty(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The property '{name}' was not found.");
    }

    /// <summary>
    /// Gets the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The property with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the property with the specified name does not exist.</exception>
    public StormXmlArray GetProperty(ReadOnlySpan<char> name)
    {
        if (TryGetProperty(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The property '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the property with the specified name, returning a value indicating whether the property exists.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property with the specified name exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetProperty(string name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        return _propertiesByName.TryGetValue(name, out property);
    }

    /// <summary>
    /// Tries to get the property with the specified name, returning a value indicating whether the property exists.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property with the specified name exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetProperty(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        return _propertiesByNameAltLookup.TryGetValue(name, out property);
    }

    /// <summary>
    /// Determines whether the property with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the property to check.</param>
    /// <returns><see langword="true"/> if the property with the specified name exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool ContainsProperty(string name)
    {
        return _propertiesByName.ContainsKey(name);
    }

    /// <summary>
    /// Determines whether the property with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the property to check.</param>
    /// <returns><see langword="true"/> if the property with the specified name exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsProperty(ReadOnlySpan<char> name)
    {
        return _propertiesByNameAltLookup.ContainsKey(name);
    }

    /// <summary>
    /// Adds a property to the current property, optionally specifying an index for array properties.
    /// </summary>
    /// <param name="property">The property to add.</param>
    /// <param name="index">The index of the property if it is part of an array; can be a numerical index or a named index.</param>
    internal void Add(StormXmlProperty property, string? index = null)
    {
        ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_propertiesByName, property.Name, out bool arrayExists);

        if (!arrayExists || currentArray is null)
        {
            currentArray = new StormXmlArray(property.IsArray);
        }

        if (!property.IsArray && property.TryGetProperty(StormXmlConstants.ValueAttributeName, out StormXmlArray? valueProperty))
        {
            currentArray.Value = valueProperty.Value;
        }

        if (index is null)
            currentArray.AddProperty(property);
        else if (int.TryParse(index, out int numericalIndex))
            currentArray.SetPropertyByIndex(numericalIndex, property);
        else
            currentArray.SetPropertyByNamedIndex(index, property);
    }

    /// <summary>
    /// Adds a property value to the current property.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="value">The value of the property to add.</param>
    internal void Add(string name, string value)
    {
        ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_propertiesByName, name, out bool arrayExists);

        if (!arrayExists || currentArray is null)
        {
            currentArray = new StormXmlArray(false);
        }

        currentArray.AddProperty(value);

        if (name == StormXmlConstants.ValueAttributeName)
        {
            Value = currentArray.Value;
        }
    }

    internal void Merge(StormXmlProperty other)
    {
        if (ReferenceEquals(this, other))
            return;

        if (other.Value is not null)
            Value = other.Value;

        Name = other.Name;

        foreach ((string name, StormXmlArray otherArray) in other._propertiesByName)
        {
            ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_propertiesByName, name, out bool arrayExists);

            if (!arrayExists || currentArray is null)
                currentArray = otherArray;
            else
                currentArray.Merge(otherArray);
        }
    }
}
