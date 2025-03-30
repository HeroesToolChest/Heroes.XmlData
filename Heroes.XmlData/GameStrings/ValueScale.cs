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

    /// <summary>
    /// Gets the total number of digits of <see cref="Value"/>. Includes the negative sign and fractional digits.
    /// </summary>
    /// <returns>The total number of digits.</returns>
    public int TotalValueDigits()
    {
        return MathHelpers.GetCountOfDigits(Value);
    }

    /// <summary>
    /// Gets the total number of digits of <see cref="Scaling"/>. Includes the negative sign and fractional digits.
    /// </summary>
    /// <returns>The total number of digits.</returns>
    public int TotalScalingDigits()
    {
        if (Scaling is null)
            return 0;

        return MathHelpers.GetCountOfDigits(Scaling.Value);
    }

    private static double ValidateValue(double value)
    {
        if (value > GameStringParser.MaxValueSize)
            return GameStringParser.MaxValueSize;
        else
            return value;
    }
}
