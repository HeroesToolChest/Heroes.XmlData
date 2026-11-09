namespace Heroes.StormData.Cache.Elements;

// the Field and Value of a LevelScalingArray modification
internal readonly struct LevelScalingFieldValue
{
    public LevelScalingFieldValue(string field, string value)
    {
        Field = field;
        Value = value;
    }

    public string Field { get; }

    public string Value { get; }
}
