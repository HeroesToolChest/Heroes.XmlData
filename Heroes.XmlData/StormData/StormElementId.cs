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

    /// <summary>
    /// Compares two <see cref="StormElementId"/> objects for equality.
    /// </summary>
    /// <param name="left">The left parameter.</param>
    /// <param name="right">The right parameter.</param>
    /// <returns><see langword="true"/> if equal otherwise <see langword="false"/>.</returns>
    public static bool operator ==(StormElementId left, StormElementId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="StormElementId"/> objects for inequality.
    /// </summary>
    /// <param name="left">The left parameter.</param>
    /// <param name="right">The right parameter.</param>
    /// <returns><see langword="true"/> if not equal otherwise <see langword="false"/>.</returns>
    public static bool operator !=(StormElementId left, StormElementId right)
    {
        return !(left == right);
    }

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
