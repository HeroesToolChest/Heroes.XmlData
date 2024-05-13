namespace Heroes.XmlData.GameStrings;

internal readonly struct TextSectionValueScale : ITextSection
{
    public TextSectionValueScale(ValueScale valueScale)
    {
        Type = TextSectionType.Value;

        ValueScale = valueScale;
    }

    public TextSectionType Type { get; }

    public ValueScale ValueScale { get; }
}
