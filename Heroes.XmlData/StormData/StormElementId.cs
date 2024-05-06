namespace Heroes.XmlData.StormData;

/// <summary>
/// A struct for a element name and it's id attribute value.
/// </summary>
public readonly struct StormElementId : IEquatable<StormElementId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StormElementId"/> struct.
    /// </summary>
    /// <param name="elementName">The name of the element (e.g. CEffectDamage).</param>
    /// <param name="id">The value of the id attribute.</param>
    public StormElementId(string elementName, string id)
    {
        ElementName = elementName;
        Id = id;
    }

    /// <summary>
    /// Gets the name of the element (e.g. CEffectDamage).
    /// </summary>
    public string ElementName { get; }

    /// <summary>
    /// Gets the value of the id attribute.
    /// </summary>
    public string Id { get; }

    /// <inheritdoc/>
    public bool Equals(StormElementId other)
    {
        if (!ElementName.Equals(other.ElementName, StringComparison.OrdinalIgnoreCase))
            return false;

        return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not StormElementId)
            return false;

        return Equals((StormElementId)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(ElementName.ToUpperInvariant(), Id.ToUpperInvariant());
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{ElementName}:{Id}";
    }
}
