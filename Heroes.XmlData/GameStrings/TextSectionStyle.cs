namespace Heroes.XmlData.GameStrings;

internal readonly struct TextSectionStyle : ITextSection
{
    public TextSectionStyle(Range valRange)
    {
        Type = TextSectionType.Value;

        ValRange = valRange;
    }

    public TextSectionType Type { get; }

    public Range ValRange { get; }
}
