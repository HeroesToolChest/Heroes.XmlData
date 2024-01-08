namespace Heroes.XmlData.StormDepotCache;

internal readonly record struct Point(double X, double Y)
{
    public override string ToString()
    {
        return $"{{{X}, {Y}}}";
    }

    public override int GetHashCode()
    {
        return $"{X}, {Y}".GetHashCode();
    }
}
