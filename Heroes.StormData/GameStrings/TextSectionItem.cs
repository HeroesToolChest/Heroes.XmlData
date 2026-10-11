namespace Heroes.StormData.GameStrings;

internal readonly struct TextSectionItem
{
    public TextSectionItem(Range range)
    {
        Type = TextSectionType.Text;
        Range = range;
    }

    public TextSectionItem(ValueScale valueScale, bool isPercent = false)
    {
        Type = TextSectionType.Value;
        ValueScale = valueScale;
        IsPercent = isPercent;
    }

    public TextSectionType Type { get; }

    public Range Range { get; }

    public ValueScale ValueScale { get; }

    public bool IsPercent { get; }
}
