namespace Heroes.XmlData.StormData;

internal readonly struct StormElementId : IEquatable<StormElementId>
{
    public StormElementId(string elementName, string id)
    {
        ElementName = elementName;
        Id = id;
    }

    public string ElementName { get; }

    public string Id { get; }

    public bool Equals(StormElementId other)
    {
        if (!ElementName.Equals(other.ElementName, StringComparison.OrdinalIgnoreCase))
            return false;

        return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not StormElementId)
            return false;

        return Equals((StormElementId)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ElementName.ToUpperInvariant(), Id.ToUpperInvariant());
    }

    public override string ToString()
    {
        return $"{ElementName}:{Id}";
    }
}
