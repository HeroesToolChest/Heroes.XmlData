namespace Heroes.StormData.Xml;

public readonly struct StormXmlValue
{
    internal StormXmlValue(string rawValue)
    {
        RawValue = rawValue;
    }

    public string RawValue { get; }

    public static implicit operator string(StormXmlValue stormValue) => stormValue.RawValue;

    public override string ToString() => RawValue;
}
