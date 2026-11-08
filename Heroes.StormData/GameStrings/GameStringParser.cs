namespace Heroes.StormData.GameStrings;

internal sealed class GameStringParser
{
    private readonly DataTagParser _dataTagParser;

    public GameStringParser(LayeredDataStorage layeredDataStorage)
    {
        _dataTagParser = new DataTagParser(layeredDataStorage);
    }

    /// <summary>
    /// Parses the data tags in <paramref name="text"/>, writing their numbers in the format of <paramref name="stormLocale"/>.
    /// </summary>
    /// <param name="text">The gamestring text.</param>
    /// <param name="stormLocale">The locale of the gamestring.</param>
    /// <returns>The parsed text.</returns>
    public string ParseText(ReadOnlySpan<char> text, StormLocale stormLocale)
    {
        return new TextParser(_dataTagParser, text, StormLocaleCulture.GetCultureInfo(stormLocale)).Parse();
    }
}
