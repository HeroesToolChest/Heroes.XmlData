namespace Heroes.XmlData.GameStrings;

internal readonly struct ValueScale
{
    public ValueScale(double value)
    {
        Value = ValidateValue(value);
    }

    public ValueScale(double value, double scaling)
    {
        Value = ValidateValue(value);
        Scaling = scaling;
    }

    public double Value { get; }

    public double? Scaling { get; }

    private static double ValidateValue(double value)
    {
        if (value > GameStringParser.MaxValueSize)
            return GameStringParser.MaxValueSize;
        else
            return value;
    }
}
