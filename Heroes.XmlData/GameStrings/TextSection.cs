namespace Heroes.XmlData.GameStrings;

internal readonly struct TextSection : ITextSection
{
    public TextSection(Range range)
    {
        Range = range;
        Type = TextSectionType.Text;
    }

    public Range Range { get; }

    public TextSectionType Type { get; }
}
