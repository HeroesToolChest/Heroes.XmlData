namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for an individual storm mod.
/// </summary>
internal class StormModStorage
{
    public const string SelfNameConst = $"{HxdConstants.Name}const-";

    private readonly IStormMod _stormMod;
    private readonly IStormStorage _stormStorage;

    private readonly HashSet<StormPath> _notFoundDirectoriesList = [];
    private readonly HashSet<StormPath> _notFoundFilesList = [];

    private readonly HashSet<StormPath> _addedXmlDataFilePathsList = [];
    private readonly HashSet<StormPath> _addedXmlFontStyleFilePathsList = [];
    private readonly HashSet<StormPath> _addedGameStringFilePathsList = [];

    internal StormModStorage(IStormMod stormMod, IStormStorage stormStorage)
    {
        _stormMod = stormMod;
        _stormStorage = stormStorage;
    }

    public int? BuildId { get; private set; }

    public string Name => _stormMod.Name;

    public StormModType StormModType => _stormMod.StormModType;

    public IEnumerable<StormPath> NotFoundDirectories => _notFoundDirectoriesList;

    public IEnumerable<StormPath> NotFoundFiles => _notFoundFilesList;

    public IEnumerable<StormPath> AddedXmlDataFilePaths => _addedXmlDataFilePathsList;

    public IEnumerable<StormPath> AddedXmlFontStyleFilePaths => _addedXmlFontStyleFilePathsList;

    public IEnumerable<StormPath> AddedGameStringFilePaths => _addedGameStringFilePathsList;

    public Dictionary<string, GameStringText> GameStringsById { get; } = [];

    public void AddDirectoryNotFound(StormPath requiredStormDirectory)
    {
        _notFoundDirectoriesList.Add(requiredStormDirectory);
        _stormStorage.AddDirectoryNotFound(StormModType, requiredStormDirectory);
    }

    public void AddFileNotFound(StormPath requiredStormFile)
    {
        _notFoundFilesList.Add(requiredStormFile);
        _stormStorage.AddFileNotFound(StormModType, requiredStormFile);
    }

    public void AddGameString(string id, GameStringText gameStringText)
    {
        GameStringsById[id] = gameStringText;

        _stormStorage.AddGameString(StormModType, id, gameStringText);
    }

    public void AddGameStringFile(Stream stream, StormPath stormPath)
    {
        using StreamReader reader = new(stream);

        if (!_addedGameStringFilePathsList.Add(stormPath))
            return;

        while (!reader.EndOfStream)
        {
            ReadOnlySpan<char> lineSpan = reader.ReadLine().AsSpan();

            if (lineSpan.IsEmpty || lineSpan.IsWhiteSpace())
                continue;

            (string Id, GameStringText GameStringText)? gameStringWithId = _stormStorage.GetGameStringWithId(lineSpan, stormPath);

            if (gameStringWithId is not null)
            {
                AddGameString(gameStringWithId.Value.Id, gameStringWithId.Value.GameStringText);
            }
        }
    }

    public void AddXmlDataFile(XDocument document, StormPath stormPath, bool isBaseGameDataDirectory)
    {
        if (document.Root is null)
            return;

        if (!_addedXmlDataFilePathsList.Add(stormPath))
            return;

        if (isBaseGameDataDirectory)
            SetElementsForDataObjectTypes(document, stormPath);
        else
            SetElements(document, stormPath);
    }

    public void AddXmlFontStyleFile(XDocument document, StormPath stormPath)
    {
        if (document.Root is null)
            return;

        if (!_addedXmlFontStyleFilePathsList.Add(stormPath))
            return;

        _stormStorage.SetStormStyleCache(StormModType, document, stormPath);
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
        GameStringsById.Clear();
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

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
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

    private void SetElementsForDataObjectTypes(XDocument document, StormPath stormPath)
    {
        if (document.Root is null)
            return;

        string? dataObjectType = SetDataObjectTypes(stormPath.Path);

        if (string.IsNullOrWhiteSpace(dataObjectType))
            return;

        int count = 0;

        foreach (XElement element in document.Root.Elements())
        {
            if (_stormStorage.AddConstantXElement(StormModType, element, stormPath))
                continue;

            UpdateConstantAttributes(element.DescendantsAndSelf());
            string elementName = element.Name.LocalName;

            _stormStorage.AddBaseElementTypes(StormModType, dataObjectType, elementName);
            _stormStorage.AddElement(StormModType, element, stormPath);

            count++;
        }

        // file is emtpy, add a default element type for the dataObjectType
        if (count < 1)
            _stormStorage.AddBaseElementTypes(StormModType, dataObjectType, $"C{dataObjectType}");
    }

    private void SetElements(XDocument document, StormPath filePath)
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
}
