namespace Heroes.StormData.GameStrings;

internal readonly struct FieldNode
{
    private readonly StormXmlMember? _member;
    private readonly StormXmlArray? _array;

    public FieldNode(StormXmlMember member) => _member = member;

    public FieldNode(StormXmlArray array) => _array = array;

    public bool IsEmpty => _member is null && _array is null;

    public StormXmlValue Value => _member?.Value ?? _array?.Value ?? default;

    /// <summary>
    /// Determines whether this node and <paramref name="other"/> point to the same <see cref="StormXmlMember"/> or <see cref="StormXmlArray"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="FieldNode"/> to compare with.</param>
    /// <returns><see langword="true"/> if both point to the same instance; otherwise, <see langword="false"/>. Two empty nodes are not the same node.</returns>
    public bool IsSameNode(FieldNode other) =>
        !IsEmpty &&
        ReferenceEquals(_member, other._member) &&
        ReferenceEquals(_array, other._array);

    public bool TryGetMember(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlArray? array)
    {
        if (_member is not null)
            return _member.TryGetMember(name, out array);

        if (_array is not null)
            return _array.TryGetMember(name, out array);

        array = null;

        return false;
    }
}
