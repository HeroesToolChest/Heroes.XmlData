namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an asset line of text.
/// </summary>
public class StormAssetString : StormIdValueString
{
    internal StormAssetString(string id, string value)
        : base(id, value)
    {
    }
}
