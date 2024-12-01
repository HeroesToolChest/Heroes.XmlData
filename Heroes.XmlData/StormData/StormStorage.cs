using Heroes.XmlData.StormMath;
using System.Text;
using U8Xml;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods.
/// </summary>
internal partial class StormStorage : IStormStorage
{
    private readonly StormPath _rootFilePath;
    private int _loadedMapMods;

    public StormStorage(bool hasRootDefaults = true)
    {
        _rootFilePath = new StormPath()
        {
            StormModName = HxdConstants.Name,
            StormModPath = $"{HxdConstants.Name}-mod",
            Path = $"{HxdConstants.Name}-root",
            PathType = StormPathType.Hxd,
        };

        if (hasRootDefaults)
            AddRootDefaults();
    }

    public StormCache StormCache { get; } = new();

    public StormCache StormMapCache { get; } = new();

    public StormCache StormCustomCache { get; } = new();

    public List<IStormModStorage> StormModStorages { get; } = [];

    public IStormModStorage CreateModStorage(IStormMod stormMod)
    {
        return new StormModStorage(stormMod, this);
    }

    public void AddModStorage(IStormModStorage stormModStorage)
    {
        StormModStorages.Add(stormModStorage);

        if (stormModStorage.StormModType == StormModType.Map)
            _loadedMapMods++;
    }

    public void AddDirectoryNotFound(StormModType stormModType, StormPath stormDirectory)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.NotFoundDirectoriesList.Add(stormDirectory);
    }

    public void AddFileNotFound(StormModType stormModType, StormPath stormFile)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.NotFoundFilesList.Add(stormFile);
    }

    public void AddGameString(StormModType stormModType, string id, GameStringText gameStringText)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.GameStringsById[id] = gameStringText;
    }

    public (string Id, GameStringText GameStringText)? GetGameStringWithId(ReadOnlySpan<char> gamestring, StormPath stormPath)
    {
        Span<Range> ranges = stackalloc Range[2];

        gamestring.Split(ranges, '=', StringSplitOptions.None);

        if (gamestring[ranges[0]].IsEmpty || gamestring[ranges[0]].IsWhiteSpace())
            return null;

        GameStringText gameStringText = new(gamestring[ranges[1]].ToString(), stormPath);

        string id = gamestring[ranges[0]].ToString();

        return (id, gameStringText);
    }

    public void AddAssetText(StormModType stormModType, string id, AssetText assetText)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.AssetTextsById[id] = assetText;
    }

    public (string Id, AssetText AssetText)? GetAssetWithId(ReadOnlySpan<char> asset, StormPath stormPath)
    {
        Span<Range> ranges = stackalloc Range[2];

        asset.Split(ranges, '=', StringSplitOptions.None);

        if (asset[ranges[0]].IsEmpty || asset[ranges[0]].IsWhiteSpace())
            return null;

        AssetText assetText = new(asset[ranges[1]].ToString(), stormPath);

        string id = asset[ranges[0]].ToString();

        return (id, assetText);
    }

    public void AddStormLayoutFilePath(StormModType stormModType, string relativePath, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        // if duplicate just override it
        currentStormCache.UiStormPathsByRelativeUiPath[relativePath] = stormPath;
    }

    public void AddAssetFilePath(StormModType stormModType, string relativePath, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        // if duplicate just override it
        currentStormCache.AssetFilesByRelativeAssetsPath[relativePath] = stormPath;
    }

    public bool AddConstantElement(StormModType stormModType, XmlNode xmlNode, StormPath stormPath)
    {
        if (xmlNode.Name != "const")
            return false;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (!xmlNode.TryFindAttribute("id", out XmlAttribute xmlAttribute) || xmlAttribute.Value.IsEmpty)
            return false;

        currentStormCache.ConstantElementById.TryAdd(xmlAttribute.Value.ToString(), new StormXmlValuePath(xmlNode, stormPath));

        return true;
    }

    public string GetValueFromConstElement(XmlNode constNode)
    {
        if (!constNode.TryFindAttribute("value", out XmlAttribute valueAttribute) || valueAttribute.Value.IsEmpty)
            return string.Empty;

        if (constNode.TryFindAttribute("evaluateAsExpression", out XmlAttribute evaluateAsExpressionAttribute) &&
            evaluateAsExpressionAttribute.Value.TryToInt32(out int expressionValue) && expressionValue == 1)
        {
            Span<char> valueCharBuffer = stackalloc char[valueAttribute.Value.GetCharCount()];
            Encoding.UTF8.GetChars(valueAttribute.Value.AsSpan(), valueCharBuffer);

            return HeroesPrefixNotation.Compute(this, valueCharBuffer).ToString();
        }
        else if (valueAttribute.Value.TryToFloat64(out double value))
        {
            return value.ToString();
        }

        return valueAttribute.Value.ToString();
    }

    public double GetValueFromConstElementAsNumber(XmlNode constNode)
    {
        if (!constNode.TryFindAttribute("value", out XmlAttribute valueAttribute) || valueAttribute.Value.IsEmpty)
            return 0;

        if (constNode.TryFindAttribute("evaluateAsExpression", out XmlAttribute evaluateAsExpressionAttribute) &&
            evaluateAsExpressionAttribute.Value.TryToInt32(out int expressionValue) && expressionValue == 1)
        {
            Span<char> valueCharBuffer = stackalloc char[valueAttribute.Value.GetCharCount()];
            Encoding.UTF8.GetChars(valueAttribute.Value.AsSpan(), valueCharBuffer);

            return HeroesPrefixNotation.Compute(this, valueCharBuffer);
        }
        else if (valueAttribute.Value.TryToFloat64(out double value))
        {
            return value;
        }

        return 0;
    }

    public string GetValueFromConstTextAsText(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return string.Empty;

        if (text[0] == '$')
        {
            if (TryGetFirstConstantElementById(text, out StormXmlValuePath? stormXmlValuePath))
            {
                using XmlObject xmlObject = XmlParser.Parse(stormXmlValuePath.Value);
                return GetValueFromConstElement(xmlObject.Root);
            }
        }
        else if (double.TryParse(text, out double result))
        {
            return result.ToString();
        }

        return text.ToString();
    }

    public double GetValueFromConstTextAsNumber(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return 0;

        if (text[0] == '$')
        {
            if (TryGetFirstConstantElementById(text, out StormXmlValuePath? stormXmlValuePath))
            {
                using XmlObject xmlObject = XmlParser.Parse(stormXmlValuePath.Value);
                return GetValueFromConstElementAsNumber(xmlObject.Root);
            }
        }
        else if (double.TryParse(text, out double result))
        {
            return result;
        }

        return 0;
    }

    public void AddElement(StormModType stormModType, XmlNode xmlNode, StormPath stormPath)
    {
        string elementName = xmlNode.Name.ToString();

        if (!elementName.StartsWith('C'))
            return;

        string? dataObjectType = GetDataObjectTypeFromFileName(stormPath.Path);

        StormCache currentStormCache = GetCurrentStormCache(stormModType);
        StormXmlValuePath stormXmlValuePath = new(xmlNode, stormPath);

        if (!string.IsNullOrWhiteSpace(dataObjectType) && elementName.AsSpan(1).StartsWith(dataObjectType, StringComparison.OrdinalIgnoreCase))
        {
            AddBaseElementType(elementName, dataObjectType, currentStormCache);
        }

        if (!xmlNode.TryFindAttribute("id", out XmlAttribute idAttribute) || idAttribute.Value.IsEmpty)
        {
            AddElementWithNoId(elementName, currentStormCache, stormXmlValuePath);
        }
        else
        {
            if (!TryGetFirstDataObjectTypeByElementType(elementName, out string? existingDataObjectType))
            {
                // didnt find one, so look for an existing match
                string foundExistingDataObjectType = FindExistingDataObjectType(elementName);

                AddBaseElementTypes(stormModType, foundExistingDataObjectType, elementName);

                existingDataObjectType = foundExistingDataObjectType;
            }

            string idAtt = idAttribute.Value.ToString();
            if (currentStormCache.StormElementsByDataObjectType.TryGetValue(existingDataObjectType, out var stormElementById))
            {
                if (stormElementById.TryGetValue(idAtt, out StormElement? stormElement))
                    stormElement.AddValue(stormXmlValuePath);
                else
                    stormElementById[idAtt] = new StormElement(stormXmlValuePath);
            }
            else
            {
                currentStormCache.StormElementsByDataObjectType[existingDataObjectType] = new()
                {
                    { idAtt, new StormElement(stormXmlValuePath) },
                };
            }
        }
    }

    public void AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName)
    {
        if (!elementName.StartsWith('C'))
            return;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (currentStormCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out HashSet<string>? elementTypes))
            elementTypes.Add(elementName);
        else
            currentStormCache.ElementTypesByDataObjectType.Add(dataObjectType, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { elementName });

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);
    }

    public void SetStormStyleCache(StormModType stormModType, XmlObject xmlObject, StormPath stormPath)
    {
        foreach (XmlNode xmlNode in xmlObject.Root.Children)
        {
            AddStormStyleElement(stormModType, xmlNode, stormPath);
        }
    }

    public void AddStormStyleElement(StormModType stormModType, XmlNode xmlNode, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        RawString nodeName = xmlNode.Name;
        if (nodeName == "Constant")
        {
            if (!xmlNode.TryFindAttribute("name", out XmlAttribute attribute))
                return;

            RawString nameValue = attribute.Value;
            if (nameValue.IsEmpty)
                return;

            currentStormCache.StormStyleConstantElementsByName[nameValue.ToString()] = new StormStyleConstantElement(new StormXmlValuePath(xmlNode, stormPath));
        }
        else if (nodeName == "Style")
        {
            if (!xmlNode.TryFindAttribute("name", out XmlAttribute attribute))
                return;

            RawString nameValue = attribute.Value;
            if (nameValue.IsEmpty)
                return;

            currentStormCache.StormStyleStyleElementsByName[nameValue.ToString()] = new StormStyleStyleElement(new StormXmlValuePath(xmlNode, stormPath));
        }
    }

    public void AddLevelScalingArrayElement(StormModType stormModType, XmlNode xmlNode, StormPath stormPath)
    {
        if (xmlNode.Name != "LevelScalingArray")
            return;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        foreach (XmlNode levelScalingArrayNode in xmlNode.Children)
        {
            if (levelScalingArrayNode.Name != "Modifications")
                continue;

            if ((!levelScalingArrayNode.TryFindChild("Catalog", out XmlNode catalogNode) || !catalogNode.TryFindAttribute("value", out XmlAttribute catalogValueAttribute) || catalogValueAttribute.Value.IsEmpty) ||
                (!levelScalingArrayNode.TryFindChild("Entry", out XmlNode entryNode) || !entryNode.TryFindAttribute("value", out XmlAttribute entryValueAttribute) || entryValueAttribute.Value.IsEmpty) ||
                (!levelScalingArrayNode.TryFindChild("Field", out XmlNode fieldNode) || !fieldNode.TryFindAttribute("value", out XmlAttribute fieldValueAttribute) || fieldValueAttribute.Value.IsEmpty) ||
                (!levelScalingArrayNode.TryFindChild("Value", out XmlNode valueNode) || !valueNode.TryFindAttribute("value", out XmlAttribute valueValueAttribute) || valueValueAttribute.Value.IsEmpty))
                continue;

            StormStringValue stormStringValue = new(valueValueAttribute.Value.ToString(), stormPath);

            currentStormCache.ScaleValueByEntry[new LevelScalingEntry(catalogValueAttribute.Value.ToString(), entryValueAttribute.Value.ToString(), fieldValueAttribute.Value.ToString())] = stormStringValue;
        }
    }

    // create storm elements based on the scaling data so when performing lookups for scaling an entry can be found
    public void BuildDataForScalingAttributes(StormModType stormModType)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        Dictionary<LevelScalingEntry, StormStringValue> scaleValuesByEntry = currentStormCache.ScaleValueByEntry;

        foreach (var scaling in scaleValuesByEntry)
        {
            LevelScalingEntry levelScalingEntry = scaling.Key;
            StormStringValue stormStringValue = scaling.Value;

            StormElement? stormElement = ScaleValueParser.CreateStormElement(this, new LevelScalingEntry(levelScalingEntry.Catalog, levelScalingEntry.Entry, levelScalingEntry.Field), stormStringValue);
            if (stormElement is null)
            {
                currentStormCache.NotFoundScaleValuesList.Add(scaling);
                continue;
            }

            if (!currentStormCache.ScaleValueStormElementsByDataObjectType.ContainsKey(levelScalingEntry.Catalog))
                currentStormCache.ScaleValueStormElementsByDataObjectType.Add(levelScalingEntry.Catalog, []);

            if (currentStormCache.ScaleValueStormElementsByDataObjectType.TryGetValue(levelScalingEntry.Catalog, out var stormElementById) &&
                stormElementById.TryGetValue(levelScalingEntry.Entry, out StormElement? existingStormElement))
                existingStormElement.AddValue(stormElement);
            else
                currentStormCache.ScaleValueStormElementsByDataObjectType[levelScalingEntry.Catalog].Add(levelScalingEntry.Entry, stormElement);
        }
    }

    public void ClearGamestrings()
    {
        foreach (IStormModStorage stormModStorage in StormModStorages)
            stormModStorage.ClearGameStrings();

        StormCache.GameStringsById.Clear();
        StormMapCache.GameStringsById.Clear();
        StormCustomCache.GameStringsById.Clear();
    }

    public void ClearStormMapMods()
    {
        ClearStormMapContainers();
        StormMapCache.Clear();
    }

    public int? GetBuildId()
    {
        return StormModStorages.FirstOrDefault()?.BuildId;
    }

    private static string? GetDataObjectTypeFromFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        int index = fileName.LastIndexOf("data.xml", StringComparison.OrdinalIgnoreCase);
        if (index > 1)
        {
            return fileName[..index];
        }

        return null;
    }

    private static void AddBaseElementType(string elementName, string dataObjectType, StormCache currentStormCache)
    {
        if (currentStormCache.ElementTypesByDataObjectType.TryGetValue(dataObjectType, out HashSet<string>? elementTypes))
            elementTypes.Add(elementName);
        else
            currentStormCache.ElementTypesByDataObjectType.Add(dataObjectType, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { elementName });

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);
    }

    private static void AddElementWithNoId(string elementName, StormCache currentStormCache, StormXmlValuePath stormXmlValuePath)
    {
        if (currentStormCache.StormElementByElementType.TryGetValue(elementName, out StormElement? stormElement))
            stormElement.AddValue(stormXmlValuePath);
        else
            currentStormCache.StormElementByElementType.Add(elementName, new StormElement(stormXmlValuePath));
    }

    private void AddRootDefaults()
    {
        List<(string DataObjectType, string ElementName)> defaultBaseElementTypes = StormDefaultData.DefaultBaseElementsTypes;
        List<string> defaultElements = StormDefaultData.DefaultXmlElements;

        foreach ((string dataObjectType, string elementName) in defaultBaseElementTypes)
        {
            AddBaseElementTypes(StormModType.Normal, dataObjectType, elementName);
        }

        foreach (string element in defaultElements)
        {
            using XmlObject xmlObject = XmlParser.Parse(element);
            AddElement(StormModType.Normal, xmlObject.Root, _rootFilePath);
        }
    }

    // find the nearest match, good enough
    private string FindExistingDataObjectType(string elementName)
    {
        // normal cache first
        string? foundExistingDataObjectType = StormCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan(1).StartsWith(x, StringComparison.OrdinalIgnoreCase));

        foundExistingDataObjectType ??= StormMapCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan(1).StartsWith(x, StringComparison.OrdinalIgnoreCase));
        foundExistingDataObjectType ??= StormCustomCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan(1).StartsWith(x, StringComparison.OrdinalIgnoreCase)) ??
            throw new HeroesXmlDataException($"Could not find an existing data object type for \"{elementName}\"");

        return foundExistingDataObjectType;
    }

    private StormCache GetCurrentStormCache(StormModType stormModType) => stormModType switch
    {
        StormModType.Normal => StormCache,
        StormModType.Map => StormMapCache,
        StormModType.Custom => StormCustomCache,
        _ => StormCustomCache,
    };

    private void ClearStormMapContainers()
    {
        // stormmap mods are always at the end
        for (int i = StormModStorages.Count - 1; i > 0; i--)
        {
            if (_loadedMapMods < 1)
                break;

            if (StormModStorages[i].StormModType == StormModType.Map)
            {
                StormModStorages.RemoveAt(i);
                _loadedMapMods--;
            }
        }
    }
}

