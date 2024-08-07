﻿namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for an individual storm mod.
/// </summary>
internal class StormModStorage : IStormModStorage
{
    public const string SelfNameConst = $"{HxdConstants.Name}const-";

    private readonly IStormMod _stormMod;
    private readonly IStormStorage _stormStorage;

    private readonly HashSet<StormPath> _notFoundDirectoriesList = [];
    private readonly HashSet<StormPath> _notFoundFilesList = [];

    private readonly HashSet<StormPath> _addedXmlDataFilePathsList = [];
    private readonly HashSet<StormPath> _addedXmlFontStyleFilePathsList = [];
    private readonly HashSet<StormPath> _addedGameStringFilePathsList = [];
    private readonly HashSet<StormPath> _addedAssetsFilePathsList = [];

    private readonly HashSet<StormPath> _foundLayoutFilePathsList = [];

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

    public IEnumerable<StormPath> AddedAssetsFilePaths => _addedAssetsFilePathsList;

    public IEnumerable<StormPath> FoundLayoutFilePaths => _foundLayoutFilePathsList;

    public Dictionary<string, GameStringText> GameStringsById { get; } = [];

    public Dictionary<string, AssetText> AssetTextsById { get; } = [];

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

    public void AddAssetText(string id, AssetText assetText)
    {
        AssetTextsById[id] = assetText;

        _stormStorage.AddAssetText(StormModType, id, assetText);
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

            (string Id, GameStringText StormStringValue)? stormStringValue = _stormStorage.GetGameStringWithId(lineSpan, stormPath);

            if (stormStringValue is not null)
            {
                AddGameString(stormStringValue.Value.Id, stormStringValue.Value.StormStringValue);
            }
        }
    }

    public void AddAssetsTextFile(Stream stream, StormPath stormPath)
    {
        using StreamReader reader = new(stream);

        if (!_addedAssetsFilePathsList.Add(stormPath))
            return;

        while (!reader.EndOfStream)
        {
            ReadOnlySpan<char> lineSpan = reader.ReadLine().AsSpan();

            if (lineSpan.IsEmpty || lineSpan.IsWhiteSpace())
                continue;

            (string Id, AssetText StormStringValue)? stormStringValue = _stormStorage.GetAssetWithId(lineSpan, stormPath);

            if (stormStringValue is not null)
            {
                AddAssetText(stormStringValue.Value.Id, stormStringValue.Value.StormStringValue);
            }
        }
    }

    public void AddXmlDataFile(XDocument document, StormPath stormPath)
    {
        if (document.Root is null)
            return;

        if (!_addedXmlDataFilePathsList.Add(stormPath))
            return;

        foreach (XElement element in document.Root.Elements())
        {
            if (_stormStorage.AddConstantXElement(StormModType, element, stormPath))
                continue;

            UpdateConstantAttributes(element.DescendantsAndSelf());

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

        if (int.TryParse(buildText, out int result))
            BuildId = result;
    }

    public void AddStormLayoutFilePath(string relativePath, StormPath stormPath)
    {
        _foundLayoutFilePathsList.Add(stormPath);
        _stormStorage.AddStormLayoutFilePath(StormModType, relativePath, stormPath);
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

                if (attributeSpan.IsEmpty)
                    continue;

                int indexOfConst = attributeSpan.IndexOf('$');

                if (indexOfConst <= -1)
                    continue;

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

    public override string ToString()
    {
        return Name;
    }
}
