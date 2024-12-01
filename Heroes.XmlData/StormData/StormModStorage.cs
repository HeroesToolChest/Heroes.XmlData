using U8Xml;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for an individual storm mod.
/// </summary>
internal class StormModStorage : IStormModStorage
{
    public const string SelfNameConst = $"{HxdConstants.Name}const-";
    public const string SelfNameAsset = $"{HxdConstants.Name}asset-";

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

        if (!_addedAssetsTextFilePathsList.Add(stormPath))
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

    public void AddXmlDataFile(XmlObject xmlObject, StormPath stormPath)
    {
        if (!_addedXmlDataFilePathsList.Add(stormPath))
            return;

        foreach (XmlNode xmlNode in xmlObject.Root.Children)
        {
            if (_stormStorage.AddConstantElement(StormModType, xmlNode, stormPath))
                continue;

            ProcessXmlNode(xmlNode, stormPath);
        }
    }

    public void AddXmlFontStyleFile(XmlObject xmlObject, StormPath stormPath)
    {
        if (!_addedXmlFontStyleFilePathsList.Add(stormPath))
            return;

        _stormStorage.SetStormStyleCache(StormModType, xmlObject, stormPath);
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
            List<XAttribute> attributes = element.Attributes().ToList();

            foreach (XAttribute attribute in attributes)
            {
                ReadOnlySpan<char> attributeSpan = attribute.Value;

                if (attributeSpan.IsEmpty)
                    continue;

                SetConstantAttribute(element, attribute);
                SetAssetAttribute(element, attribute);
            }
        }
    }

    public override string ToString()
    {
        return Name;
    }

    private static bool HasSpecialCharacterInAttribute(XmlAttribute xmlAttribute)
    {
        RawString attributeValue = xmlAttribute.Value;
        return attributeValue.Contains('$') || attributeValue.StartsWith("@");
    }

    private void ProcessXmlNode(XmlNode xmlNode, StormPath stormPath)
    {
        foreach (XmlNode xmlNodeDescendant in xmlNode.Descendants)
        {
            foreach (XmlAttribute xmlAttribute in xmlNodeDescendant.Attributes)
            {
                if (xmlAttribute.Value.IsEmpty)
                    continue;

                if (HasSpecialCharacterInAttribute(xmlAttribute))
                {
                    ProcessSpecialAttributes(xmlNode, stormPath);
                    return;
                }
            }
        }

        // no special characters so just add the xml node.
        _stormStorage.AddLevelScalingArrayElement(StormModType, xmlNode, stormPath);
        _stormStorage.AddElement(StormModType, xmlNode, stormPath);
    }

    private void ProcessSpecialAttributes(XmlNode xmlNode, StormPath stormPath)
    {
        string xmlOfNode = xmlNode.AsRawString().ToString();
        XElement xElement = XElement.Parse(xmlOfNode);

        UpdateAttributes(xElement.DescendantsAndSelf());

        using XmlObject xmlObject = XmlParser.Parse(xElement.ToString());
        _stormStorage.AddLevelScalingArrayElement(StormModType, xmlObject.Root, stormPath);
        _stormStorage.AddElement(StormModType, xmlObject.Root, stormPath);
    }

    private void SetConstantAttribute(XElement element, XAttribute attribute)
    {
        ReadOnlySpan<char> attributeSpan = attribute.Value;

        int indexOfConst = attributeSpan.IndexOf('$');

        if (indexOfConst < 0)
            return;

        ReadOnlySpan<char> attributeOfStartSpan = attributeSpan[indexOfConst..];

        int endIndexOfConst = attributeOfStartSpan.IndexOfAny(" ,.;");
        if (endIndexOfConst < 0)
            endIndexOfConst = attributeOfStartSpan.Length + indexOfConst;
        else
            endIndexOfConst += indexOfConst;

        element.SetAttributeValue($"{SelfNameConst}{attribute.Name}", attribute.Value.Replace(attributeSpan[indexOfConst..endIndexOfConst].ToString(), _stormStorage.GetValueFromConstTextAsText(attributeSpan[indexOfConst..endIndexOfConst])));
    }

    private void SetAssetAttribute(XElement element, XAttribute attribute)
    {
        ReadOnlySpan<char> attributeSpan = attribute.Value;

        if (attributeSpan[0] != '@')
            return;

#if NET9_0_OR_GREATER
        string assetValue = string.Empty;
        if (_stormStorage.TryGetStormAssetStringValue(attributeSpan[1..], out string? value))
            assetValue = value;

        element.SetAttributeValue($"{SelfNameAsset}{attribute.Name}", assetValue);
#else
        element.SetAttributeValue($"{SelfNameAsset}{attribute.Name}", _stormStorage.GetStormAssetString(attributeSpan[1..].ToString())?.Value ?? string.Empty);
#endif
    }
}
