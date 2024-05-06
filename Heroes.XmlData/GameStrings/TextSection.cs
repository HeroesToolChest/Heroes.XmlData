namespace Heroes.XmlData.GameStrings;

internal readonly struct TextSection
{
    public TextSection(Range range)
    {
        Range = range;
        IsText = true;
        Type = TextSectionType.Text;
    }

    public TextSection(ValueScale valueScale)
    {
        Type = TextSectionType.Value;
        IsValue = true;
        ValueScale = valueScale;
    }

    public Range? Range { get; }

    public TextSectionType Type { get; }

    public ValueScale? ValueScale { get; }

    [MemberNotNullWhen(true, nameof(ValueScale))]
    public bool IsValue { get; }

    [MemberNotNullWhen(true, nameof(Range))]
    public bool IsText { get; }
}
