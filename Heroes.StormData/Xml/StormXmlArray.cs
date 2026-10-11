namespace Heroes.StormData.Xml;

/// <summary>
/// Represents an array of members for an element. Can be indexed by name or by numerical index.
/// </summary>
public sealed class StormXmlArray
{
    // the properties in the array
    private readonly List<StormXmlMember> _stormXmlMembers = [];

    // some members in the array can be indexed by name
    private readonly Dictionary<string, int> _indexByMemberName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> _indexByMemberNameAltLookup;

    internal StormXmlArray(bool isArray)
    {
        _indexByMemberNameAltLookup = _indexByMemberName.GetAlternateLookup<ReadOnlySpan<char>>();

        IsArray = isArray;
    }

    public StormXmlValue Value { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this instance represents an array of members.
    /// </summary>
    public bool IsArray { get; }

    /// <summary>
    /// Gets a lookup for members by index.
    /// </summary>
    public IndexLookup ByIndex => new(this);

    /// <summary>
    /// Gets a lookup for members by name.
    /// This lookup is only valid when this instance represents a single member rather than an array.
    /// </summary>
    public MemberLookup ByMember => new(this);

    /// <summary>
    /// Gets the member with the specified name of the single underlying member.
    /// This lookup is only valid when this instance represents a single member rather than an array.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <returns>The member with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance represents an array, or if the member was not found.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlArray GetMember(string name)
    {
        if (TryGetMember(name, out StormXmlArray? member))
            return member;

        throw new KeyNotFoundException($"The member '{name}' was not found.");
    }

    /// <summary>
    /// Gets the member with the specified name of the single underlying member.
    /// This lookup is only valid when this instance represents a single member rather than an array.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <returns>The member with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance represents an array, or if the member was not found.</exception>
    public StormXmlArray GetMember(ReadOnlySpan<char> name)
    {
        if (TryGetMember(name, out StormXmlArray? member))
            return member;

        throw new KeyNotFoundException($"The member '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the member with the specified name of the single underlying member.
    /// This lookup is only valid when this instance represents a single member rather than an array.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <param name="member">When this method returns, contains the member with the specified name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetMember(string name, [NotNullWhen(true)] out StormXmlArray? member)
    {
        ArgumentNullException.ThrowIfNull(name);

        return TryGetMember(name.AsSpan(), out member);
    }

    /// <summary>
    /// Tries to get the member with the specified name of the single underlying member.
    /// This lookup is only valid when this instance represents a single member rather than an array.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <param name="member">When this method returns, contains the member with the specified name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetMember(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlArray? member)
    {
        if (!IsArray)
        {
            if (_stormXmlMembers[0].TryGetMember(name, out member))
            {
                return true;
            }
        }

        member = null;
        return false;
    }

    /// <summary>
    /// Gets the member at the specified index by numerical index.
    /// </summary>
    /// <param name="index">The numerical index of the member.</param>
    /// <returns>The member at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    public StormXmlMember GetByIndex(int index)
    {
        if (TryGetByIndex(index, out StormXmlMember? member))
            return member;

        throw new KeyNotFoundException($"The index '{index}' was not found.");
    }

    /// <summary>
    /// Gets the member at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The member at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlMember GetByIndex(string name)
    {
        if (TryGetByIndex(name, out StormXmlMember? member))
            return member;

        throw new KeyNotFoundException($"The index '{name}' was not found.");
    }

    /// <summary>
    /// Gets the member at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The member at the specified index.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if this instance does not represent an array, or if the index was not found.</exception>
    public StormXmlMember GetByIndex(ReadOnlySpan<char> name)
    {
        if (TryGetByIndex(name, out StormXmlMember? member))
            return member;

        throw new KeyNotFoundException($"The index '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the member at the specified index by numerical index.
    /// </summary>
    /// <param name="index">The numerical index of the member.</param>
    /// <param name="member">When this method returns, contains the member at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetByIndex(int index, [NotNullWhen(true)] out StormXmlMember? member)
    {
        if (IsArray)
        {
            if (index >= 0 && index < _stormXmlMembers.Count)
            {
                member = _stormXmlMembers[index];
                return true;
            }
        }

        member = null;
        return false;
    }

    /// <summary>
    /// Tries to get the member at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <param name="member">When this method returns, contains the member at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetByIndex(string name, [NotNullWhen(true)] out StormXmlMember? member)
    {
        ArgumentNullException.ThrowIfNull(name);

        return TryGetByIndex(name.AsSpan(), out member);
    }

    /// <summary>
    /// Tries to get the member at the specified index by name.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <param name="member">When this method returns, contains the member at the specified index, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetByIndex(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlMember? member)
    {
        if (IsArray)
        {
            if (int.TryParse(name, out int numericalIndex))
            {
                if (numericalIndex >= 0 && numericalIndex < _stormXmlMembers.Count)
                {
                    member = _stormXmlMembers[numericalIndex];
                    return true;
                }
            }
            else if (_indexByMemberNameAltLookup.TryGetValue(name, out int index))
            {
                member = _stormXmlMembers[index];
                return true;
            }
        }

        member = null;
        return false;
    }

    /// <summary>
    /// Determines whether the specified numerical index exists in the array.
    /// </summary>
    /// <param name="index">The numerical index of the member.</param>
    /// <returns><see langword="true"/> if the index exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsIndex(int index)
    {
        if (!IsArray)
            return false;

        return index >= 0 && index < _stormXmlMembers.Count;
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
        if (!IsArray)
            return false;

        if (int.TryParse(name, out int numericalIndex))
        {
            return numericalIndex >= 0 && numericalIndex < _stormXmlMembers.Count;
        }

        return _indexByMemberNameAltLookup.ContainsKey(name);
    }

    internal void SetValue(string value, StormXmlMember rootMember)
    {
        Value = new StormXmlValue(value, rootMember);
    }

    internal void AddMember(StormXmlMember member)
    {
        _stormXmlMembers.Add(member);
    }

    internal void SetMemberByIndex(int index, StormXmlMember member)
    {
        while (_stormXmlMembers.Count < index)
        {
            _stormXmlMembers.Add(new StormXmlMember(member.Name));
        }

        if (_stormXmlMembers.Count == index)
            _stormXmlMembers.Add(member);
        else
            _stormXmlMembers[index] = member;
    }

    internal void SetMemberByNamedIndex(string name, StormXmlMember member)
    {
        if (_indexByMemberName.TryGetValue(name, out int i))
        {
            _stormXmlMembers[i] = member;
        }
        else
        {
            _indexByMemberName[name] = _stormXmlMembers.Count;
            _stormXmlMembers.Add(member);
        }
    }

    internal void Merge(StormXmlArray other)
    {
        if (ReferenceEquals(this, other))
            return;

        if (other.Value.HasValue)
            Value = other.Value;

        if (other._stormXmlMembers.Count < 1)
            return;

        // the named indexes are stored as name -> position, so invert them to resolve a position back to its name
        Dictionary<int, string>? namedIndexesByPosition = null;

        if (other._indexByMemberName.Count > 0)
        {
            namedIndexesByPosition = new Dictionary<int, string>(other._indexByMemberName.Count);

            foreach ((string namedIndex, int position) in other._indexByMemberName)
            {
                namedIndexesByPosition[position] = namedIndex;
            }
        }

        for (int i = 0; i < other._stormXmlMembers.Count; i++)
        {
            StormXmlMember? otherMember = other._stormXmlMembers[i];

            if (namedIndexesByPosition is not null && namedIndexesByPosition.TryGetValue(i, out string? namedIndex))
                MergeMemberByNamedIndex(namedIndex, otherMember);
            else
                MergeMemberByIndex(i, otherMember);
        }
    }

    private void MergeMemberByIndex(int index, StormXmlMember otherMember)
    {
        if (index < _stormXmlMembers.Count && _stormXmlMembers[index] is StormXmlMember existingMember)
            existingMember.Merge(otherMember);
        else
            SetMemberByIndex(index, otherMember);
    }

    private void MergeMemberByNamedIndex(string namedIndex, StormXmlMember otherMember)
    {
        if (_indexByMemberName.TryGetValue(namedIndex, out int index) && _stormXmlMembers[index] is StormXmlMember existingMember)
            existingMember.Merge(otherMember);
        else
            SetMemberByNamedIndex(namedIndex, otherMember);
    }

    /// <summary>
    /// Provides a lookup for members by index.
    /// </summary>
    /// <param name="owner">The <see cref="StormXmlArray"/> instance that owns the members.</param>
    public readonly struct IndexLookup(StormXmlArray owner)
    {
        /// <summary>
        /// Gets the member at the specified index by name.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <returns>The member at the specified index.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
        public StormXmlMember this[string name] => owner.GetByIndex(name);

        /// <summary>
        /// Gets the member at the specified index by name.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <returns>The member at the specified index.</returns>
        public StormXmlMember this[ReadOnlySpan<char> name] => owner.GetByIndex(name);

        /// <summary>
        /// Gets the member at the specified index by numerical index.
        /// </summary>
        /// <param name="index">The numerical index of the member.</param>
        /// <returns>The member at the specified index.</returns>
        public StormXmlMember this[int index] => owner.GetByIndex(index);
    }

    /// <summary>
    /// Provides a lookup for members by name.
    /// </summary>
    /// <param name="owner">The <see cref="StormXmlArray"/> instance that owns the members.</param>
    public readonly struct MemberLookup(StormXmlArray owner)
    {
        /// <summary>
        /// Gets the member with the specified name of the single underlying element.
        /// This lookup is only valid when this instance represents a single member rather than an array.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <returns>The member with the specified name.</returns>
        public StormXmlArray this[string name] => owner.GetMember(name);

        /// <summary>
        /// Gets the member with the specified name of the single underlying element.
        /// This lookup is only valid when this instance represents a single member rather than an array.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <returns>The member with the specified name.</returns>
        public StormXmlArray this[ReadOnlySpan<char> name] => owner.GetMember(name);
    }
}
