namespace Heroes.XmlData.GameStrings;

internal readonly struct TextSectionValueScale : ITextSection
{
    public TextSectionValueScale(ValueScale valueScale, bool isPercent = false)
    {
        Type = TextSectionType.Value;

        ValueScale = valueScale;
        IsPercent = isPercent;
    }

    public TextSectionType Type { get; }

    public ValueScale ValueScale { get; }

    public bool IsPercent { get; }
}
