using System.Runtime.InteropServices;

namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a member, that is either an element or attribute, for an XML element.
/// </summary>
public sealed class StormXmlMember
{
    private readonly Dictionary<string, StormXmlArray> _membersByName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StormXmlArray>.AlternateLookup<ReadOnlySpan<char>> _membersByNameAltLookup;
    private Dictionary<string, StormXmlToken> _tokens = new(StringComparer.Ordinal);

    internal StormXmlMember(string name)
    {
        _membersByNameAltLookup = _membersByName.GetAlternateLookup<ReadOnlySpan<char>>();

        Name = name;
    }

    /// <summary>
    /// Gets the name of the member. Can either be the element or attribute name.
    /// </summary>
    public string Name { get; private set; }

    public StormXmlValue Value { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the member is in an array.
    /// </summary>
    public bool IsArray { get; internal set; }

    /// <summary>
    /// Gets the number of unique members the member has (elements that are arrays are counted as one single item).
    /// </summary>
    public int Count => _membersByName.Count;

    /// <summary>
    /// Gets the member with the specified name.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <returns>The member with the specified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public StormXmlArray this[string name] => GetMember(name);

    /// <summary>
    /// Gets the member with the specified name.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <returns>The member with the specified name.</returns>
    public StormXmlArray this[ReadOnlySpan<char> name] => GetMember(name);

    /// <summary>
    /// Gets the member with the specified name.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <returns>The member with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the member with the specified name does not exist.</exception>
    public StormXmlArray GetMember(string name)
    {
        if (TryGetMember(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The member '{name}' was not found.");
    }

    /// <summary>
    /// Gets the member with the specified name.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <returns>The member with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the member with the specified name does not exist.</exception>
    public StormXmlArray GetMember(ReadOnlySpan<char> name)
    {
        if (TryGetMember(name, out StormXmlArray? property))
            return property;

        throw new KeyNotFoundException($"The member '{name}' was not found.");
    }

    /// <summary>
    /// Tries to get the member with the specified name, returning a value indicating whether the member exists.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <param name="property">When this method returns, contains the member with the specified name, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member with the specified name exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool TryGetMember(string name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        return _membersByName.TryGetValue(name, out property);
    }

    /// <summary>
    /// Tries to get the member with the specified name, returning a value indicating whether the member exists.
    /// </summary>
    /// <param name="name">The name of the member to get.</param>
    /// <param name="property">When this method returns, contains the member with the specified name, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the member with the specified name exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetMember(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlArray? property)
    {
        return _membersByNameAltLookup.TryGetValue(name, out property);
    }

    /// <summary>
    /// Determines whether the member with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the member to check.</param>
    /// <returns><see langword="true"/> if the member with the specified name exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public bool ContainsMember(string name)
    {
        return _membersByName.ContainsKey(name);
    }

    /// <summary>
    /// Determines whether the member with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the member to check.</param>
    /// <returns><see langword="true"/> if the member with the specified name exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsMember(ReadOnlySpan<char> name)
    {
        return _membersByNameAltLookup.ContainsKey(name);
    }

    /// <summary>
    /// Adds a member to the current member, optionally specifying an index for array members.
    /// </summary>
    /// <param name="member">The member to add.</param>
    /// <param name="index">The index of the member if it is part of an array; can be a numerical index or a named index.</param>
    internal void Add(StormXmlMember member, string? index = null)
    {
        ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_membersByName, member.Name, out bool arrayExists);

        if (!arrayExists || currentArray is null)
        {
            currentArray = new StormXmlArray(member.IsArray);
        }

        if (!member.IsArray && member.TryGetMember(StormXmlConstants.ValueAttributeName, out StormXmlArray? valueProperty))
        {
            currentArray.Value = valueProperty.Value;
        }

        if (index is null)
            currentArray.AddMember(member);
        else if (int.TryParse(index, out int numericalIndex))
            currentArray.SetMemberByIndex(numericalIndex, member);
        else
            currentArray.SetMemberByNamedIndex(index, member);
    }

    /// <summary>
    /// Adds a member value to the current member.
    /// </summary>
    /// <param name="name">The name of the member to add.</param>
    /// <param name="value">The value of the member to add.</param>
    /// <param name="rootMember">The root member of the XML hierarchy.</param>
    internal void Add(string name, string value, StormXmlMember rootMember)
    {
        ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_membersByName, name, out bool arrayExists);

        if (!arrayExists || currentArray is null)
        {
            currentArray = new StormXmlArray(false);
        }

        currentArray.SetValue(value, rootMember);

        if (name.Equals(StormXmlConstants.ValueAttributeName, StringComparison.Ordinal))
        {
            Value = currentArray.Value;
        }
    }

    internal void AddToken(StormXmlToken token)
    {
        if (StormXmlConstants.ReservedTokenIds.Contains(token.Id))
            return;

        _tokens[token.Id] = token;

        Add(token.Id, token.Value, this);
    }

    internal void Merge(StormXmlMember other, StormXmlMember? rootMember = null)
    {
        if (ReferenceEquals(this, other))
            return;

        rootMember ??= this;

        if (other.Value.HasValue)
            Value = new StormXmlValue(other.Value.RawValue, rootMember);

        Name = other.Name;

        foreach ((string name, StormXmlArray otherArray) in other._membersByName)
        {
            ref StormXmlArray? currentArray = ref CollectionsMarshal.GetValueRefOrAddDefault(_membersByName, name, out bool arrayExists);

            if (!arrayExists || currentArray is null)
                currentArray = otherArray.Clone(rootMember);
            else
                currentArray.Merge(otherArray, rootMember);
        }

        foreach ((string id, StormXmlToken token) in other._tokens)
        {
            _tokens[id] = token;
        }
    }

    internal StormXmlMember Clone(StormXmlMember? rootMember = null)
    {
        StormXmlMember clone = new(Name)
        {
            IsArray = IsArray,
        };

        rootMember ??= clone;

        foreach ((string name, StormXmlArray stormXmlArray) in _membersByName)
            clone._membersByName[name] = stormXmlArray.Clone(rootMember);

        if (Value.HasValue)
            clone.Value = new StormXmlValue(Value.RawValue, rootMember);

        clone._tokens = new Dictionary<string, StormXmlToken>(_tokens, StringComparer.Ordinal);

        return clone;
    }

    /// <summary>
    /// Removes the member with the specified name, if it exists.
    /// </summary>
    /// <param name="name">The name of the member to remove.</param>
    /// <returns><see langword="true"/> if the member was removed; otherwise, <see langword="false"/>.</returns>
    internal bool Remove(string name)
    {
        bool removed = _membersByName.Remove(name);

        if (removed && name.Equals(StormXmlConstants.ValueAttributeName, StringComparison.Ordinal))
            Value = default;

        return removed;
    }
}
