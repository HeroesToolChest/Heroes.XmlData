namespace Heroes.XmlData.StormData;

public class StormModDataContainer
{
    private const string _selfName = "HXD";

    private readonly string _stormModName;
    private readonly string _stormModDirectoryPath;

    private readonly HashSet<string> _addedXmlDataFilePathsList = [];
    private readonly HashSet<string> _addedXmlFontStyleFilePathsList = [];
    private readonly Dictionary<string, List<GameStringText>> _gameStringsById = [];

    private readonly XDocument _xmlData = new();
    private readonly XDocument _xmlFontStyle = new();

    public StormModDataContainer(string stormModeName, string stormModDirectoryPath)
    {
        _stormModName = stormModeName;
        _stormModDirectoryPath = stormModDirectoryPath;
    }

    public string StormModName => _stormModName;

    public string StormModDiretoryPath => _stormModDirectoryPath;

    public void AddGameStringFile(Stream stream, string filePath)
    {
        using StreamReader reader = new(stream);

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] splitLine = line.Split('=', 2, StringSplitOptions.None);

            if (splitLine.Length != 2)
                continue;

            GameStringText gameStringText = new()
            {
                GameStringValue = splitLine[1],
                FilePath = filePath,
            };

            if (_gameStringsById.TryGetValue(splitLine[0], out List<GameStringText>? gameStringTexts))
                gameStringTexts.Add(gameStringText);
            else
                _gameStringsById.Add(splitLine[0], [gameStringText]);
        }
    }

    public void AddXmlDataFile(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        SetXml(document, filePath, _addedXmlDataFilePathsList, _xmlData);
    }

    public void AddXmlFontStyleFile(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        SetXml(document, filePath, _addedXmlFontStyleFilePathsList, _xmlFontStyle);
    }

    public void ClearGameStrings()
    {
        _gameStringsById.Clear();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return StormModName;
    }

    private static void SetXml(XDocument document, string filePath, HashSet<string> filePaths, XDocument xmlDoc)
    {
        document.Root!.SetAttributeValue($"{_selfName}-FilePath", filePath);

        if (filePaths.Contains(filePath))
            return;

        if (xmlDoc.Root is null)
        {
            xmlDoc.Declaration = document.Declaration;
            xmlDoc.Add(new XElement(_selfName));

            xmlDoc.Root!.Add(document.Root);
        }
        else
        {
            xmlDoc.Root.Add(document.Root);
        }

        filePaths.Add(filePath);
    }
}