namespace Heroes.StormData.Xml;

/// <summary>
/// Represents an array of properties for an element. Can be indexed by name or by numerical index.
/// </summary>
public sealed class StormXmlArray
{
    // the properties in the array
    private readonly List<StormXmlProperty> _stormXmlProperties = [];

    // some properties in the array can be indexed by name
    private readonly Dictionary<string, int> _indexByPropertyName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> _indexByPropertyNameAltLookup;

    private readonly bool _isArray;

    internal StormXmlArray(bool isArray)
    {
        _indexByPropertyNameAltLookup = _indexByPropertyName.GetAlternateLookup<ReadOnlySpan<char>>();

        _isArray = isArray;
    }

    public StormXmlValue? Value { get; internal set; }

    /// <summary>
    /// Gets a lookup for properties by index.
    /// </summary>
    public IndexLookup ByIndex => new(this);

    /// <summary>
    /// Gets a lookup for properties by name.
    /// This lookup is only valid when this instance represents a single property rather than an array.
    /// </summary>
    public PropertyLookup ByProperty => new(this);

    ///// <summary>
    ///// Gets the number of items in the array.
    ///// </summary>
    //public int Count => _items.Count;

    /// <summary>
    /// Gets the property with the specified name of the single underlying property.
    /// This lookup is only valid when this instance represents a single property rather than an array.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance represents an array, or if the property was not found.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlArray GetProperty(string name)
    {
        if (TryGetProperty(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The property '{name}' was not found.");
    }

    /// <summary>
    /// Gets the property with the specified name of the single underlying property.
    /// This lookup is only valid when this instance represents a single property rather than an array.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance represents an array, or if the property was not found.</exception>
    public StormXmlArray GetProperty(ReadOnlySpan<char> name)
    {
        if (TryGetProperty(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The property '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the property with the specified name of the single underlying property.
    /// This lookup is only valid when this instance represents a single property rather than an array.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetProperty(string name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        ArgumentNullException.ThrowIfNull(name);

        return TryGetProperty(name.AsSpan(), out property);
    }

    /// <summary>
    /// Tries to get the property with the specified name of the single underlying property.
    /// This lookup is only valid when this instance represents a single property rather than an array.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="property">When this method returns, contains the property with the specified name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetProperty(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        if (!_isArray)
        {
            if (_stormXmlProperties[0].TryGetProperty(name, out property))
            {
                return true;
            }
        }

        property = null;
        return false;
    }

    /// <summary>
    /// Gets the property at the specified index by numerical index.
    /// </summary>
    /// <param name="index">The numerical index of the property.</param>
    /// <returns>The property at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    public StormXmlProperty GetByIndex(int index)
    {
        if (TryGetByIndex(index, out StormXmlProperty? property))
            return property;

        throw new KeyNotFoundException($"The index '{index}' was not found.");
    }

    /// <summary>
    /// Gets the property at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The property at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlProperty GetByIndex(string name)
    {
        if (TryGetByIndex(name, out StormXmlProperty? property))
            return property;

        throw new KeyNotFoundException($"The index '{name}' was not found.");
    }

    /// <summary>
    /// Gets the property at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The property at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    public StormXmlProperty GetByIndex(ReadOnlySpan<char> name)
    {
        if (TryGetByIndex(name, out StormXmlProperty? property))
            return property;

        throw new KeyNotFoundException($"The index '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the property at the specified index by numerical index.
    /// </summary>
    /// <param name="index">The numerical index of the property.</param>
    /// <param name="property">When this method returns, contains the property at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetByIndex(int index, [NotNullWhen(true)] out StormXmlProperty? property)
    {
        if (_isArray)
        {
            if (index >= 0 && index < _stormXmlProperties.Count)
            {
                property = _stormXmlProperties[index];
                return true;
            }
        }

        property = null;
        return false;
    }

    /// <summary>
    /// Tries to get the property at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <param name="property">When this method returns, contains the property at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetByIndex(string name, [NotNullWhen(true)] out StormXmlProperty? property)
    {
        ArgumentNullException.ThrowIfNull(name);

        return TryGetByIndex(name.AsSpan(), out property);
    }

    /// <summary>
    /// Tries to get the property at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <param name="property">When this method returns, contains the property at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetByIndex(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlProperty? property)
    {
        if (_isArray)
        {
            if (int.TryParse(name, out int numericalIndex))
            {
                if (numericalIndex >= 0 && numericalIndex < _stormXmlProperties.Count)
                {
                    property = _stormXmlProperties[numericalIndex];
                    return true;
                }
            }
            else if (_indexByPropertyNameAltLookup.TryGetValue(name, out int index))
            {
                property = _stormXmlProperties[index];
                return true;
            }
        }

        property = null;
        return false;
    }

    /// <summary>
    /// Determines whether the specified numerical index exists in the array.
    /// </summary>
    /// <param name="index">The numerical index of the property.</param>
    /// <returns><see langword="true"/> if the index exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsIndex(int index)
    {
        if (!_isArray)
            return false;

        return index >= 0 && index < _stormXmlProperties.Count;
    }

    /// <summary>
    /// Determines whether the specified index exists in the array.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns><see langword="true"/> if the index exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool ContainsIndex(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return ContainsIndex(name.AsSpan());
    }

    /// <summary>
    /// Determines whether the specified index exists in the array.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns><see langword="true"/> if the index exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsIndex(ReadOnlySpan<char> name)
    {
        if (!_isArray)
            return false;

        if (int.TryParse(name, out int numericalIndex))
        {
            return numericalIndex >= 0 && numericalIndex < _stormXmlProperties.Count;
        }

        return _indexByPropertyNameAltLookup.ContainsKey(name);
    }

    // TODO: rename to SetValue
    internal void AddProperty(string value)
    {
        Value = new StormXmlValue(value);
    }

    internal void AddProperty(StormXmlProperty property)
    {
        //if (property.IsArray)
            _stormXmlProperties.Add(property);

    }

    internal void SetPropertyByIndex(int index, StormXmlProperty property)
    {
        while (_stormXmlProperties.Count < index)
        {
            _stormXmlProperties.Add(new StormXmlProperty(property.Name));
        }

        if (_stormXmlProperties.Count == index)
            _stormXmlProperties.Add(property);
        else
            _stormXmlProperties[index] = property;
    }

    internal void SetPropertyByNamedIndex(string name, StormXmlProperty property)
    {
        if (_indexByPropertyName.TryGetValue(name, out int i))
        {
            _stormXmlProperties[i] = property;
        }
        else
        {
            _indexByPropertyName[name] = _stormXmlProperties.Count;
            _stormXmlProperties.Add(property);
        }
    }

    internal void Merge(StormXmlArray other)
    {
        if (ReferenceEquals(this, other))
            return;

        if (other.Value is not null)
            Value = other.Value;

        if (other._stormXmlProperties.Count < 1)
            return;

        // the named indexes are stored as name -> position, so invert them to resolve a position back to its name
        Dictionary<int, string>? namedIndexesByPosition = null;

        if (other._indexByPropertyName.Count > 0)
        {
            namedIndexesByPosition = new Dictionary<int, string>(other._indexByPropertyName.Count);

            foreach ((string namedIndex, int position) in other._indexByPropertyName)
            {
                namedIndexesByPosition[position] = namedIndex;
            }
        }

        for (int i = 0; i < other._stormXmlProperties.Count; i++)
        {
            StormXmlProperty? otherProperty = other._stormXmlProperties[i];

            if (namedIndexesByPosition is not null && namedIndexesByPosition.TryGetValue(i, out string? namedIndex))
                MergePropertyByNamedIndex(namedIndex, otherProperty);
            else
                MergePropertyByIndex(i, otherProperty);
        }
    }

    private void MergePropertyByIndex(int index, StormXmlProperty otherProperty)
    {
        if (index < _stormXmlProperties.Count && _stormXmlProperties[index] is StormXmlProperty existingProperty)
            existingProperty.Merge(otherProperty);
        else
            SetPropertyByIndex(index, otherProperty);
    }

    private void MergePropertyByNamedIndex(string namedIndex, StormXmlProperty otherProperty)
    {
        if (_indexByPropertyName.TryGetValue(namedIndex, out int index) && _stormXmlProperties[index] is StormXmlProperty existingProperty)
            existingProperty.Merge(otherProperty);
        else
            SetPropertyByNamedIndex(namedIndex, otherProperty);
    }

    /// <summary>
    /// Provides a lookup for properties by index.
    /// </summary>
    /// <param name="owner">The <see cref="StormXmlArray"/> instance that owns the properties.</param>
    public readonly struct IndexLookup(StormXmlArray owner)
    {
        /// <summary>
        /// Gets the property at the specified index by name.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <returns>The property at the specified index.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
        public StormXmlProperty this[string name] => owner.GetByIndex(name);

        /// <summary>
        /// Gets the property at the specified index by name.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <returns>The property at the specified index.</returns>
        public StormXmlProperty this[ReadOnlySpan<char> name] => owner.GetByIndex(name);

        /// <summary>
        /// Gets the property at the specified index by numerical index.
        /// </summary>
        /// <param name="index">The numerical index of the property.</param>
        /// <returns>The property at the specified index.</returns>
        public StormXmlProperty this[int index] => owner.GetByIndex(index);
    }

    /// <summary>
    /// Provides a lookup for properties by name.
    /// </summary>
    /// <param name="owner">The <see cref="StormXmlArray"/> instance that owns the properties.</param>
    public readonly struct PropertyLookup(StormXmlArray owner)
    {
        /// <summary>
        /// Gets the property with the specified name of the single underlying element.
        /// This lookup is only valid when this instance represents a single property rather than an array.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property with the specified name.</returns>
        public StormXmlArray this[string name] => owner.GetProperty(name);

        /// <summary>
        /// Gets the property with the specified name of the single underlying element.
        /// This lookup is only valid when this instance represents a single property rather than an array.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property with the specified name.</returns>
        public StormXmlArray this[ReadOnlySpan<char> name] => owner.GetProperty(name);
    }
}
