namespace Heroes.XmlData;

public class HeroesData : IHeroesData
{
    private const string _selfName = "HXD";

    private readonly HashSet<string> _addedXmlFilePathsList = [];
    private readonly Dictionary<string, List<(string GameString, string FilePath)>> _gameStringsById = [];

    private readonly HashSet<string> _notFoundDirectoriesList = [];
    private readonly HashSet<string> _notFoundFilesList = [];

    private XDocument _xmlGameData = new();

    public HeroesData(int? hotsBuild = null)
    {
        HotsBuild = hotsBuild;
    }

    public int? HotsBuild { get; }

    public HeroesLocalization HeroesLocalization { get; private set; }

    public void AddXmlFile(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        document.Root.SetAttributeValue($"{_selfName}-FilePath", filePath);

        if (_addedXmlFilePathsList.Contains(filePath))
            return;

        if (_xmlGameData.Root is null)
        {
            _xmlGameData.Declaration = document.Declaration;
            _xmlGameData.Add(new XElement(_selfName));

            _xmlGameData.Root!.Add(document.Root);
        }
        else
        {
            _xmlGameData.Root.Add(document.Root);
        }

        _addedXmlFilePathsList.Add(filePath);
    }

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

            var item = (splitLine[1], filePath);

            if (_gameStringsById.TryGetValue(splitLine[0], out var gameStrings))
            {
                gameStrings.Add(item);
            }
            else
            {
                _gameStringsById.Add(splitLine[0],
                [
                    item,
                ]);
            }
        }
    }

    void IHeroesData.AddDirectoryNotFound(string directoryPath)
    {
        _notFoundDirectoriesList.Add(directoryPath);
    }

    void IHeroesData.AddFileNotFound(string filePath)
    {
        _notFoundFilesList.Add(filePath);
    }

    void IHeroesData.ClearGamestrings()
    {
        _gameStringsById.Clear();
    }

    internal void SetHeroesLocalization(HeroesLocalization localization)
    {
        HeroesLocalization = localization;
    }
}
