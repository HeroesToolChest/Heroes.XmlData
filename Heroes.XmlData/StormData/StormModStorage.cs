namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for an individual storm mod.
/// </summary>
internal sealed class StormModStorage : IStormModStorage
{
    private readonly IStormMod _stormMod;
    private readonly IStormStorage _stormStorage;

    private readonly HashSet<StormPath> _notFoundDirectoriesList = [];
    private readonly HashSet<StormPath> _notFoundFilesList = [];

    private readonly HashSet<StormPath> _addedXmlDataFilePathsList = [];
    private readonly HashSet<StormPath> _addedXmlFontStyleFilePathsList = [];
    private readonly HashSet<StormPath> _addedGameStringFilePathsList = [];
    private readonly HashSet<StormPath> _addedAssetsTextFilePathsList = [];

    private readonly HashSet<StormPath> _foundLayoutFilePathsList = [];
    private readonly HashSet<StormPath> _foundAssetFilePathsList = [];

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

    public IEnumerable<StormPath> AddedAssetsTextFilePaths => _addedAssetsTextFilePathsList;

    public IEnumerable<StormPath> FoundLayoutFilePaths => _foundLayoutFilePathsList;

    public IEnumerable<StormPath> FoundAssetFilePaths => _foundAssetFilePathsList;

    public Dictionary<string, GameStringFileText> GameStringsById { get; } = [];

    public Dictionary<string, AssetText> AssetTextsById { get; } = [];

    public int NumberOfNotFoundDirectories => _notFoundDirectoriesList.Count;

    public int NumberOfNotFoundFiles => _notFoundFilesList.Count;

    public int NumberOfXmlDataFiles => _addedXmlDataFilePathsList.Count;

    public int NumberOfXmlFontStyleFiles => _addedXmlFontStyleFilePathsList.Count;

    public int NumberOfGameStringFiles => _addedGameStringFilePathsList.Count;

    public int NumberOfAssetsTextFiles => _addedAssetsTextFilePathsList.Count;

    public int NumberOfLayoutFiles => _foundLayoutFilePathsList.Count;

    public int NumberOfAssetFiles => _foundAssetFilePathsList.Count;

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

    public void AddGameString(string id, GameStringFileText gameStringText)
    {
        GameStringsById[id] = gameStringText;

        _stormStorage.AddGameString(StormModType, id, gameStringText);
    }

    public void AddAssetText(string id, AssetText assetText)
    {
        AssetTextsById[id] = assetText;

        _stormStorage.AddAssetText(StormModType, id, assetText);
    }

    public async Task AddGameStringFile(Stream stream, StormPath stormPath)
    {
        if (!_addedGameStringFilePathsList.Add(stormPath))
            return;

        PipeReader reader = PipeReader.Create(stream);
        bool isFirstRead = true;

        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            StripBOM(ref isFirstRead, ref buffer);

            while (TryReadLine(ref buffer, result.IsCompleted, out ReadOnlySequence<byte> line))
            {
                (string Id, GameStringFileText GameStringText)? gameStringWithId = _stormStorage.GetGameStringWithId(line, stormPath);

                if (gameStringWithId.HasValue)
                    AddGameString(gameStringWithId.Value.Id, gameStringWithId.Value.GameStringText);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    public async Task AddAssetsTextFile(Stream stream, StormPath stormPath)
    {
        if (!_addedAssetsTextFilePathsList.Add(stormPath))
            return;

        PipeReader reader = PipeReader.Create(stream);
        bool isFirstRead = true;

        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            StripBOM(ref isFirstRead, ref buffer);

            while (TryReadLine(ref buffer, result.IsCompleted, out ReadOnlySequence<byte> line))
            {
                (string Id, AssetText AssetText)? assetWithId = _stormStorage.GetAssetWithId(line, stormPath);

                if (assetWithId.HasValue)
                    AddAssetText(assetWithId.Value.Id, assetWithId.Value.AssetText);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    public void AddXmlDataFile(XDocument document, StormPath stormPath)
    {
        XElement? root = document.Root;
        if (root is null)
            return;

        if (!_addedXmlDataFilePathsList.Add(stormPath))
            return;

        foreach (XElement element in root.Elements())
        {
            if (_stormStorage.AddConstantXElement(StormModType, element, stormPath))
                continue;

            UpdateAttributes(element.DescendantsAndSelf());

            _stormStorage.AddLevelScalingArrayElement(StormModType, element, stormPath);
            _stormStorage.AddElement(StormModType, element, stormPath);
        }
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

        if (int.TryParse(buildText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            BuildId = result;
    }

    public void AddStormLayoutFilePath(string relativePath, StormPath stormPath)
    {
        _foundLayoutFilePathsList.Add(stormPath);
        _stormStorage.AddStormLayoutFilePath(StormModType, relativePath, stormPath);
    }

    public void AddAssetFilePath(string relativePath, StormPath stormPath)
    {
        _foundAssetFilePathsList.Add(stormPath);
        _stormStorage.AddAssetFilePath(StormModType, relativePath, stormPath);
    }

    public void ClearGameStrings()
    {
        GameStringsById.Clear();
        _addedGameStringFilePathsList.Clear();
    }

    public void UpdateAttributes(IEnumerable<XElement> elements)
    {
        foreach (XElement element in elements)
        {
            foreach (XAttribute attribute in element.Attributes())
            {
                ReadOnlySpan<char> attributeSpan = attribute.Value;

                if (attributeSpan.IsEmpty)
                    continue;

                SetConstantAttribute(element, attribute, attributeSpan);
                SetAssetAttribute(element, attribute, attributeSpan);
            }
        }
    }

    public override string ToString()
    {
        return Name;
    }

    private static void StripBOM(ref bool isFirstRead, ref ReadOnlySequence<byte> buffer)
    {
        if (!isFirstRead)
            return;

        isFirstRead = false;

        if (buffer.Length >= 3 && buffer.Slice(0, 3).FirstSpan.SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }))
            buffer = buffer.Slice(3);
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, bool isCompleted, out ReadOnlySequence<byte> line)
    {
        SequencePosition? lineEndingPosition = buffer.PositionOf((byte)'\n');

        if (lineEndingPosition is not null)
        {
            line = buffer.Slice(0, lineEndingPosition.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, lineEndingPosition.Value));
        }
        else if (isCompleted && !buffer.IsEmpty)
        {
            // last line in the file without a trailing '\n'
            line = buffer;
            buffer = buffer.Slice(buffer.End);
        }
        else
        {
            line = default;
            return false;
        }

        // remove trailing '\r'
        if (!line.IsEmpty && line.Slice(line.Length - 1, 1).FirstSpan[0] == (byte)'\r')
            line = line.Slice(0, line.Length - 1);

        return true;
    }

    private void SetConstantAttribute(XElement element, XAttribute attribute, ReadOnlySpan<char> attributeSpan)
    {
        int indexOfConst = attributeSpan.IndexOf('$');

        if (indexOfConst < 0)
            return;

        ReadOnlySpan<char> fromConst = attributeSpan[indexOfConst..];

        int endIndexOfConst = fromConst.IndexOfAny(" ,.;");
        ReadOnlySpan<char> constSpan = endIndexOfConst < 0
            ? fromConst
            : fromConst[..endIndexOfConst];

        string constKey = constSpan.ToString();
        string resolvedValue = _stormStorage.GetValueFromConstTextAsText(constSpan);

        element.SetAttributeValue(attribute.Name, attribute.Value.Replace(constKey, resolvedValue));
    }

    private void SetAssetAttribute(XElement element, XAttribute attribute, ReadOnlySpan<char> attributeSpan)
    {
        if (attributeSpan[0] != '@')
            return;

        string assetValue = string.Empty;
        if (_stormStorage.TryGetStormAssetStringValue(attributeSpan[1..], out string? value))
            assetValue = value;

        element.SetAttributeValue(attribute.Name, assetValue);
    }
}
