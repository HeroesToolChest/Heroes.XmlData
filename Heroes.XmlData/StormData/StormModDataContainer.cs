namespace Heroes.XmlData.StormData;

// class for each stormmod
internal class StormModDataContainer
{
    private const string _selfName = "HXD";

    private readonly string _modsDirectoryPath;
    private readonly StormCache _stormCache;
    private readonly StormModDataProperties _stormModDataProperties;
    private readonly HashSet<string> _addedXmlDataFilePathsList = [];
    private readonly HashSet<string> _addedXmlFontStyleFilePathsList = [];
    private readonly Dictionary<string, GameStringText> _gameStringsById = [];

    private readonly XDocument _xmlData = new();
    private readonly XDocument _xmlFontStyle = new();

    internal StormModDataContainer(StormCache stormCache, string modsDirectoryPath, StormModDataProperties stormModDataProperties)
    {
        _stormCache = stormCache;
        _modsDirectoryPath = modsDirectoryPath;
        _stormModDataProperties = stormModDataProperties;
    }

    public int? BuildId { get; private set; }

    public bool IsMapMod => _stormModDataProperties.IsMapMod;

    public void AddGameStringFile(Stream stream, ReadOnlySpan<char> filePath)
    {
        using StreamReader reader = new(stream);

        while (!reader.EndOfStream)
        {
            ReadOnlySpan<char> lineSpan = reader.ReadLine().AsSpan();

            if (lineSpan.IsEmpty || lineSpan.IsWhiteSpace())
                continue;

            _stormCache.AddGameString(lineSpan, GetModlessPath(filePath), _gameStringsById);
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
            BuildId = result;
    }

    public void ClearGameStrings()
    {
        _gameStringsById.Clear();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _stormModDataProperties.StormModName;
    }

    private static void SetXml(XDocument document, string filePath, HashSet<string> filePaths, XDocument storedXmlDoc)
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

    private string GetModlessPath(string path)
    {
        int indexOfMods = path.IndexOf(_modsDirectoryPath);
        if (indexOfMods < 0)
            return path;

        string modlessPath = path[(indexOfMods + _modsDirectoryPath.Length)..];

        return modlessPath;
    }

    private ReadOnlySpan<char> GetModlessPath(ReadOnlySpan<char> path)
    {
        int indexOfMods = path.IndexOf(_modsDirectoryPath);
        if (indexOfMods < 0)
            return path;

        return path[(indexOfMods + _modsDirectoryPath.Length)..];
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

                _stormCache.StormStyleHexColorValueByName[name] = new StormStringValue(val, filePath);
            }
            else if (elementName.Equals("Style", StringComparison.OrdinalIgnoreCase))
            {
                string? name = element.Attribute("name")?.Value;
                string? textColor = element.Attribute("textcolor")?.Value;

                if (string.IsNullOrEmpty(textColor) || string.IsNullOrEmpty(name))
                    continue;

                _stormCache.StormStyleHexColorValueByName[name] = new StormStringValue(textColor, filePath);

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
                _stormCache.ScaleValueByEntry[new(catalog, entry, field)] = new StormStringValue(value, filePath);
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
                elementStormValues.Add(new StormXElementValue(element, filePath));
            else
                _stormCache.ElementsByElementName.Add(elementName, [new StormXElementValue(element, filePath)]);

            // set consts
            _stormCache.AddConstantElement(element, filePath);
        }
    }
}