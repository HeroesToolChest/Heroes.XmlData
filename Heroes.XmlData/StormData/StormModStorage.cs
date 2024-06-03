namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for an individual storm mod.
/// </summary>
internal class StormModStorage
{
    public const string SelfNameConst = $"{HxdConstants.Name}const-";

    private readonly IStormMod _stormMod;
    private readonly IStormStorage _stormStorage;

    private readonly string _modsBaseDirectoryPath; // base directory of selected mods, C:\path\to\mods_11111

    private readonly HashSet<StormFile> _notFoundDirectoriesList = [];
    private readonly HashSet<StormFile> _notFoundFilesList = [];

    private readonly HashSet<StormFile> _addedXmlDataFilePathsList = [];
    private readonly HashSet<StormFile> _addedXmlFontStyleFilePathsList = [];
    private readonly HashSet<string> _addedGameStringFilePathsList = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, GameStringText> _gameStringsById = [];

    internal StormModStorage(IStormMod stormMod, IStormStorage stormStorage, string modsBaseDirectoryPath)
    {
        _stormMod = stormMod;
        _stormStorage = stormStorage;
        _modsBaseDirectoryPath = modsBaseDirectoryPath;
    }

    public int? BuildId { get; private set; }

    public StormModType StormModType => _stormMod.StormModType;

    public void AddDirectoryNotFound(StormFile requiredStormDirectory)
    {
        _notFoundDirectoriesList.Add(requiredStormDirectory);
        _stormStorage.AddDirectoryNotFound(StormModType, requiredStormDirectory);
    }

    public void AddFileNotFound(StormFile requiredStormFile)
    {
        _notFoundFilesList.Add(requiredStormFile);
        _stormStorage.AddFileNotFound(StormModType, requiredStormFile);
    }

    public void AddGameString(string id, GameStringText gameStringText)
    {
        _gameStringsById[id] = gameStringText;

        _stormStorage.AddGameString(StormModType, id, gameStringText);
    }

    public void AddGameStringFile(Stream stream, ReadOnlySpan<char> filePath)
    {
        using StreamReader reader = new(stream);

        if (!_addedGameStringFilePathsList.Add(filePath.ToString()))
            return;

        while (!reader.EndOfStream)
        {
            ReadOnlySpan<char> lineSpan = reader.ReadLine().AsSpan();

            if (lineSpan.IsEmpty || lineSpan.IsWhiteSpace())
                continue;

            (string Id, GameStringText GameStringText)? gameStringWithId = _stormStorage.GetGameStringWithId(lineSpan, GetModlessPath(filePath));

            if (gameStringWithId is not null)
            {
                AddGameString(gameStringWithId.Value.Id, gameStringWithId.Value.GameStringText);
            }
        }
    }

    public void AddXmlDataFile(XDocument document, StormFile stormFile, bool isBaseGameDataDirectory)
    {
        if (document.Root is null)
            return;

        if (!_addedXmlDataFilePathsList.Add(stormFile))
            return;

        string modlessPath = GetModlessPath(stormFile.Path);

        if (isBaseGameDataDirectory)
            SetElementsForDataObjectTypes(document, modlessPath);
        else
            SetElements(document, modlessPath);
    }

    public void AddXmlFontStyleFile(XDocument document, StormFile stormFile)
    {
        if (document.Root is null)
            return;

        if (!_addedXmlFontStyleFilePathsList.Add(stormFile))
            return;

        string modlessPath = GetModlessPath(stormFile.Path);

        _stormStorage.SetFontStyleCache(StormModType, document, modlessPath);
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
        return _stormMod.Name;
    }

    public void UpdateConstantAttributes(IEnumerable<XElement> elements)
    {
        foreach (XElement element in elements)
        {
            List<XAttribute> attributes = element.Attributes().ToList();

            foreach (XAttribute attribute in attributes)
            {
                ReadOnlySpan<char> attributeSpan = attribute.Value;

                if (!attributeSpan.IsEmpty)
                {
                    int indexOfConst = attributeSpan.IndexOf('$');
                    if (indexOfConst > -1)
                    {
                        ReadOnlySpan<char> attributeOfStartSpan = attributeSpan[indexOfConst..];

                        int endIndexOfConst = attributeOfStartSpan.IndexOfAny(" ,.;");
                        if (endIndexOfConst < 0)
                            endIndexOfConst = attributeOfStartSpan.Length + indexOfConst;
                        else
                            endIndexOfConst += indexOfConst;

                        element.SetAttributeValue($"{SelfNameConst}{attribute.Name}", attribute.Value.Replace(attributeSpan[indexOfConst..endIndexOfConst].ToString(), _stormStorage.GetValueFromConstTextAsText(attributeSpan[indexOfConst..endIndexOfConst])));
                    }
                }
            }
        }
    }

    private static string? SetDataObjectTypes(string filePath)
    {
        ReadOnlySpan<char> fileNameSpan = Path.GetFileName(filePath.AsSpan());

        int index = fileNameSpan.LastIndexOf("data.xml", StringComparison.OrdinalIgnoreCase);
        if (index > 1)
        {
            string objectType = fileNameSpan[..index].ToString();

            return objectType;
        }

        return null;
    }

    private void SetElementsForDataObjectTypes(XDocument document, string filePath)
    {
        if (document.Root is null)
            return;

        string? dataObjectType = SetDataObjectTypes(filePath);

        if (string.IsNullOrWhiteSpace(dataObjectType))
            return;

        int count = 0;

        foreach (XElement element in document.Root.Elements())
        {
            if (_stormStorage.AddConstantXElement(StormModType, element, filePath))
                continue;

            UpdateConstantAttributes(element.DescendantsAndSelf());
            string elementName = element.Name.LocalName;

            _stormStorage.AddBaseElementTypes(StormModType, dataObjectType, elementName);
            _stormStorage.AddElement(StormModType, element, filePath);

            count++;
        }

        // file is emtpy, add a default element type for the dataObjectType
        if (count < 1)
            _stormStorage.AddBaseElementTypes(StormModType, dataObjectType, $"C{dataObjectType}");
    }

    private void SetElements(XDocument document, string filePath)
    {
        foreach (XElement element in document.Root!.Elements())
        {
            if (_stormStorage.AddConstantXElement(StormModType, element, filePath))
                continue;

            UpdateConstantAttributes(element.DescendantsAndSelf());

            _stormStorage.AddLevelScalingArrayElement(StormModType, element, filePath);
            _stormStorage.AddElement(StormModType, element, filePath);
        }
    }

    private string GetModlessPath(string path)
    {
        return GetModlessPath(path.AsSpan()).ToString();
    }

    private ReadOnlySpan<char> GetModlessPath(ReadOnlySpan<char> path)
    {
        int indexOfMods = path.IndexOf(_modsBaseDirectoryPath);
        if (indexOfMods < 0)
            return path;

        return path[(indexOfMods + _modsBaseDirectoryPath.Length)..];
    }
}
