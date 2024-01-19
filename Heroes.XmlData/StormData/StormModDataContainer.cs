namespace Heroes.XmlData.StormData;

// class for each stormmod
internal class StormModDataContainer
{
    private const string _selfName = "HXD";

    private readonly StormCache _stormCache;
    private readonly string _modsDirectoryPath;
    private readonly HashSet<string> _addedXmlDataFilePathsList = [];
    private readonly HashSet<string> _addedXmlFontStyleFilePathsList = [];
    private readonly Dictionary<string, List<GameStringText>> _gameStringsById = [];

    private readonly XDocument _xmlData = new();
    private readonly XDocument _xmlFontStyle = new();

    private int? _buildId;

    public StormModDataContainer(StormCache stormCache, string modsDirectoryPath, string stormModeName, string stormModDirectoryPath)
    {
        _stormCache = stormCache;
        _modsDirectoryPath = modsDirectoryPath;
        StormModName = stormModeName;
        StormModDiretoryPath = stormModDirectoryPath;
    }

    public string StormModName { get; }

    public string StormModDiretoryPath { get; }

    public int? BuildId => _buildId;

    public void AddGameStringFile(Stream stream, ReadOnlySpan<char> filePath)
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
                FilePath = GetModlessPath(filePath),
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

        string modlessPath = GetModlessPath(filePath);

        SetElementCaches(document, modlessPath);
        SetLevelScalingArrayCache(document, modlessPath);

        SetXml(document, modlessPath, _addedXmlDataFilePathsList, _xmlData);
    }

    public void AddXmlFontStyleFile(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        string modlessPath = GetModlessPath(filePath);

        SetFontStyleCache(document, modlessPath);

        SetXml(document, modlessPath, _addedXmlFontStyleFilePathsList, _xmlFontStyle);
    }

    public void AddBuildIdFile(Stream stream)
    {
        using StreamReader reader = new(stream);

        string? buildText = reader.ReadLine()?.TrimStart('B');

        if (int.TryParse(buildText, out int result))
            _buildId = result;
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

    private void SetXml(XDocument document, string filePath, HashSet<string> filePaths, XDocument storedXmlDoc)
    {
        document.Root!.SetAttributeValue($"{_selfName}-FilePath", filePath);

        if (filePaths.Contains(filePath))
            return;

        if (storedXmlDoc.Root is null)
        {
            storedXmlDoc.Declaration = document.Declaration;
            storedXmlDoc.Add(new XElement(_selfName));

            storedXmlDoc.Root!.Add(document.Root);
        }
        else
        {
            storedXmlDoc.Root.Add(document.Root);
        }

        filePaths.Add(filePath);
    }

    private string GetModlessPath(ReadOnlySpan<char> path)
    {
        int indexOfMods = path.IndexOf(_modsDirectoryPath);

        ReadOnlySpan<char> modlessPath = path[(indexOfMods + _modsDirectoryPath.Length)..];

        return modlessPath.ToString();
    }

    private void SetFontStyleCache(XDocument document, string filePath)
    {
        foreach (XElement element in document.Root!.Elements())
        {
            string elementName = element.Name.LocalName;
            if (elementName.Equals("Constant", StringComparison.OrdinalIgnoreCase))
            {
                string? name = element.Attribute("name")?.Value;
                string? val = element.Attribute("val")?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val))
                    continue;

                _stormCache.StormStyleHexColorValueByName[name] = new StormValue<string>(filePath, val);
            }
            else if (elementName.Equals("Style", StringComparison.OrdinalIgnoreCase))
            {
                string? name = element.Attribute("name")?.Value;
                string? textColor = element.Attribute("textcolor")?.Value;

                if (string.IsNullOrEmpty(textColor) || string.IsNullOrEmpty(name))
                    continue;

                _stormCache.StormStyleHexColorValueByName[name] = new StormValue<string>(filePath, textColor);

                // TODO: needed anymore?
                // if (textColor[0] == '#') // variable
                // {
                //     if (_stormStyleHexColorValueByName.TryGetValue(textColor.TrimStart('#'), out string? hexValue))
                //     {
                //         _stormStyleHexColorValueByName.TryAdd(name, hexValue);
                //     }
                // }
                // else if (!textColor.Contains(',', StringComparison.OrdinalIgnoreCase))
                // {
                //     _stormStyleHexColorValueByName.TryAdd(name, textColor);
                // }
            }
        }
    }

    private void SetLevelScalingArrayCache(XDocument document, string filePath)
    {
        foreach (XElement levelScalingArrayElement in document.Root!.Descendants("LevelScalingArray"))
        {
            foreach (XElement modificationElement in levelScalingArrayElement.Elements("Modifications"))
            {
                string? catalog = modificationElement.Element("Catalog")?.Attribute("value")?.Value;
                string? entry = modificationElement.Element("Entry")?.Attribute("value")?.Value;
                string? field = modificationElement.Element("Field")?.Attribute("value")?.Value;
                string? value = modificationElement.Element("Value")?.Attribute("value")?.Value;

                if (string.IsNullOrEmpty(value) || catalog is null || entry is null || field is null)
                    continue;

                // add data without index
                // TODO:
                // if (field.Contains(']', StringComparison.OrdinalIgnoreCase))
                //    _scaleValueByLookupId[(catalog, entry, Regex.Replace(field, @"\[.*?\]", string.Empty))] = double.Parse(value);
                _stormCache.ScaleValueByEntry[new(catalog, entry, field)] = new StormValue<double>(filePath, double.Parse(value));
            }
        }
    }

    private void SetElementCaches(XDocument document, string filePath)
    {
        foreach (XElement element in document.Root!.Elements())
        {
            string elementName = element.Name.LocalName;

            // set elements
            if (_stormCache.ElementsByElementName.TryGetValue(elementName, out List<StormXElementValue>? elementStormValues))
                elementStormValues.Add(new StormXElementValue(filePath, element));
            else
                _stormCache.ElementsByElementName.Add(elementName, [new StormXElementValue(filePath, element)]);

            // set consts
            if (elementName.Equals("const", StringComparison.OrdinalIgnoreCase))
            {
                string? id = element.Attribute("id")?.Value;

                if (string.IsNullOrEmpty(id))
                    continue;

                _stormCache.ConstantElementById[id] = new StormXElementValue(filePath, element);
            }
        }
    }
}