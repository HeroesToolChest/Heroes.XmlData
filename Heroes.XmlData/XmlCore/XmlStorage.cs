namespace Heroes.XmlData.XmlCore;

internal class XmlStorage
{
    private const string _selfName = "HXD";

    private readonly HashSet<string> _addedXmlFilePathsList = [];
    private readonly Dictionary<string, List<GameStringText>> _gameStringsById = [];

    private readonly XDocument _xml = new();

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

    public void AddXmlFile(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        document.Root.SetAttributeValue($"{_selfName}-FilePath", filePath);

        if (_addedXmlFilePathsList.Contains(filePath))
            return;

        if (_xml.Root is null)
        {
            _xml.Declaration = document.Declaration;
            _xml.Add(new XElement(_selfName));

            _xml.Root!.Add(document.Root);
        }
        else
        {
            _xml.Root.Add(document.Root);
        }

        _addedXmlFilePathsList.Add(filePath);
    }

    public void ClearGameStrings()
    {
        _gameStringsById.Clear();
    }
}