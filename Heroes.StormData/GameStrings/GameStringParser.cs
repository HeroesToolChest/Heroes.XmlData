namespace Heroes.StormData.GameStrings;

internal sealed class GameStringParser
{
    private readonly DataTagParser _dataTagParser;

    public GameStringParser(LayeredDataStorage layeredDataStorage)
    {
        _dataTagParser = new DataTagParser(layeredDataStorage);
    }

    public string ParseText(ReadOnlySpan<char> text)
    {
        TextParser textParser = new(_dataTagParser, text);
        List<TextSectionItem> textStack = [];

        textParser.ConstructTextStack(textStack);
        return textParser.BuildDescription(textStack);
    }
}
