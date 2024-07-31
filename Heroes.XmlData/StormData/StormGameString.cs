namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an unparsed gamestring.
/// </summary>
public class StormGameString : StormIdValueString
{
    internal StormGameString(string id, string value)
        : base(id, value)
    {
    }
}
