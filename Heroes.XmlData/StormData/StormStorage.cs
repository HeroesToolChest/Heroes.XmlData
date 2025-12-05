using System.Runtime.InteropServices;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods.
/// </summary>
internal sealed partial class StormStorage : IStormStorage
{
    private readonly StormPath _rootFilePath;

    private int _loadedNormalMods;
    private int _loadedMapMods;
    private int _loadedCustomMods;

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

    private bool HasLoadedNormalMods => _loadedNormalMods > 0;

    private bool HasLoadedMapMods => _loadedMapMods > 0;

    private bool HasLoadedCustomMods => _loadedCustomMods > 0;

    public IStormModStorage CreateModStorage(IStormMod stormMod)
    {
        return new StormModStorage(stormMod, this);
    }

    public void AddModStorage(IStormModStorage stormModStorage)
    {
        if (stormModStorage.StormModType == StormModType.Normal)
        {
            if (HasLoadedMapMods)
            {
                int insertIndex = StormModStorages.FindIndex(x => x.StormModType == StormModType.Map);
                StormModStorages.Insert(insertIndex, stormModStorage);
            }
            else if (HasLoadedCustomMods)
            {
                int insertIndex = StormModStorages.FindIndex(x => x.StormModType == StormModType.Custom);
                StormModStorages.Insert(insertIndex, stormModStorage);
            }
            else
            {
                StormModStorages.Add(stormModStorage);
            }

            _loadedNormalMods++;
        }
        else if (stormModStorage.StormModType == StormModType.Map)
        {
            if (HasLoadedCustomMods)
            {
                int insertIndex = StormModStorages.FindIndex(x => x.StormModType == StormModType.Custom);
                StormModStorages.Insert(insertIndex, stormModStorage);
            }
            else
            {
                StormModStorages.Add(stormModStorage);
            }

            _loadedMapMods++;
        }
        else if (stormModStorage.StormModType == StormModType.Custom)
        {
            StormModStorages.Add(stormModStorage);

            _loadedCustomMods++;
        }
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

    public void AddGameString(StormModType stormModType, string id, GameStringFileText gameStringText)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.GameStringsById[id] = gameStringText;
    }

    public (string Id, GameStringFileText GameStringText)? GetGameStringWithId(string gamestring, StormPath stormPath)
    {
        ReadOnlySpan<char> gamestringSpan = gamestring.AsSpan();
        Span<Range> ranges = stackalloc Range[2];

        gamestringSpan.Split(ranges, '=', StringSplitOptions.None);

        if (gamestringSpan[ranges[0]].IsEmpty || gamestringSpan[ranges[0]].IsWhiteSpace())
            return null;

        GameStringFileText gameStringText = new(gamestringSpan[ranges[1]].ToString(), stormPath);

        string id = gamestringSpan[ranges[0]].ToString();

        return (id, gameStringText);
    }

    public void AddAssetText(StormModType stormModType, string id, AssetText assetText)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.AssetTextsById[id] = assetText;
    }

    public (string Id, AssetText AssetText)? GetAssetWithId(string asset, StormPath stormPath)
    {
        ReadOnlySpan<char> assetSpan = asset.AsSpan();
        Span<Range> ranges = stackalloc Range[2];

        assetSpan.Split(ranges, '=', StringSplitOptions.None);

        if (assetSpan[ranges[0]].IsEmpty || assetSpan[ranges[0]].IsWhiteSpace())
            return null;

        AssetText assetText = new(assetSpan[ranges[1]].ToString(), stormPath);

        string id = assetSpan[ranges[0]].ToString();

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

    public bool AddConstantXElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        if (!element.Name.LocalName.Equals("const", StringComparison.OrdinalIgnoreCase))
            return false;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        string? id = element.Attribute("id")?.Value;

        if (string.IsNullOrEmpty(id))
            return false;

        currentStormCache.ConstantXElementById.TryAdd(id, new StormXElementValuePath(element, stormPath));

        return true;
    }

    public string GetValueFromConstElement(XElement constElement)
    {
        string? valueAttribute = constElement.Attribute("value")?.Value;
        string? isExpressionAttribute = constElement.Attribute("evaluateAsExpression")?.Value;

        if (string.IsNullOrWhiteSpace(valueAttribute))
            return valueAttribute ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(isExpressionAttribute) && isExpressionAttribute == "1")
        {
            return HeroesPrefixNotation.Compute(this, valueAttribute).ToString();
        }
        else if (double.TryParse(valueAttribute, out double value))
        {
            return value.ToString();
        }

        return valueAttribute;
    }

    public double GetValueFromConstElementAsNumber(XElement constElement)
    {
        string? valueAttribute = constElement.Attribute("value")?.Value;
        string? isExpressionAttribute = constElement.Attribute("evaluateAsExpression")?.Value;

        if (string.IsNullOrWhiteSpace(valueAttribute))
            return 0;

        if (!string.IsNullOrWhiteSpace(isExpressionAttribute) && isExpressionAttribute == "1")
        {
            return HeroesPrefixNotation.Compute(this, valueAttribute);
        }
        else if (double.TryParse(valueAttribute, out double value))
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
            if (TryGetFirstConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
            {
                return GetValueFromConstElement(stormXElementValuePath.Value);
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
            if (TryGetFirstConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
            {
                return GetValueFromConstElementAsNumber(stormXElementValuePath.Value);
            }
        }
        else if (double.TryParse(text, out double result))
        {
            return result;
        }

        return 0;
    }

    public void AddElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        string elementName = element.Name.LocalName;

        if (!elementName.StartsWith('C'))
            return;

        string? idAtt = element.Attribute("id")?.Value;
        string? unitNameAtt = element.Attribute("unitName")?.Value;
        string? dataObjectType = GetDataObjectTypeFromFileName(stormPath.Path);

        StormCache currentStormCache = GetCurrentStormCache(stormModType);
        StormXElementValuePath stormXElementValuePath = new(element, stormPath);

        if (!string.IsNullOrWhiteSpace(dataObjectType) && elementName.AsSpan(1).StartsWith(dataObjectType, StringComparison.OrdinalIgnoreCase))
        {
            AddBaseElementType(elementName, dataObjectType, currentStormCache);
        }

        if (string.IsNullOrEmpty(idAtt))
        {
            AddElementWithNoId(elementName, currentStormCache, stormXElementValuePath);
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

            ref var stormElementById = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.StormElementsByDataObjectType, existingDataObjectType, out bool dataObjectExists);

            if (!dataObjectExists)
            {
                Dictionary<string, StormElement> newStormElementById = new()
                {
                    { idAtt, new StormElement(stormXElementValuePath) },
                };
                stormElementById = newStormElementById.GetAlternateLookup<ReadOnlySpan<char>>();
            }
            else
            {
                ref var stormElement = ref CollectionsMarshal.GetValueRefOrAddDefault(stormElementById!, idAtt, out bool elementExists);

                if (!elementExists)
                    stormElement = new StormElement(stormXElementValuePath);
                else
                    stormElement!.AddValue(stormXElementValuePath);
            }

            // unit name
            if (!string.IsNullOrEmpty(unitNameAtt))
            {
                ref Dictionary<string, string>? idsByUnitName = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.UnitNamesByDataObjectType, existingDataObjectType, out bool unitNameDictExists);

                if (!unitNameDictExists)
                    idsByUnitName = [];

                idsByUnitName![unitNameAtt] = idAtt;
            }
        }
    }

    public void AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName)
    {
        if (!elementName.StartsWith('C'))
            return;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        ref HashSet<string>? elementTypes = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.ElementTypesByDataObjectType, dataObjectType, out bool exists);

        if (!exists)
            elementTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        elementTypes!.Add(elementName);

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);
    }

    public void SetStormStyleCache(StormModType stormModType, XDocument document, StormPath stormPath)
    {
        foreach (XElement element in document.Root!.Elements())
        {
            AddStormStyleElement(stormModType, element, stormPath);
        }
    }

    public void AddStormStyleElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        string elementName = element.Name.LocalName;
        if (elementName.Equals("Constant", StringComparison.OrdinalIgnoreCase))
        {
            string? name = element.Attribute("name")?.Value;

            if (string.IsNullOrEmpty(name))
                return;

            currentStormCache.StormStyleConstantElementsByName[name] = new StormStyleConstantElement(new StormXElementValuePath(element, stormPath));
        }
        else if (elementName.Equals("Style", StringComparison.OrdinalIgnoreCase))
        {
            string? name = element.Attribute("name")?.Value;

            if (string.IsNullOrEmpty(name))
                return;

            currentStormCache.StormStyleStyleElementsByName[name] = new StormStyleStyleElement(new StormXElementValuePath(element, stormPath));
        }
    }

    public void AddLevelScalingArrayElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        foreach (XElement levelScalingArrayElement in element.DescendantsAndSelf("LevelScalingArray"))
        {
            foreach (XElement modificationElement in levelScalingArrayElement.Elements("Modifications"))
            {
                string? catalog = modificationElement.Element("Catalog")?.Attribute("value")?.Value;
                string? entry = modificationElement.Element("Entry")?.Attribute("value")?.Value;
                string? field = modificationElement.Element("Field")?.Attribute("value")?.Value;
                string? value = modificationElement.Element("Value")?.Attribute("value")?.Value;

                if (string.IsNullOrEmpty(value) || catalog is null || entry is null || field is null)
                    continue;

                StormStringValue stormStringValue = new(value, stormPath);

                currentStormCache.ScaleValueByEntry[new(catalog, entry, field)] = stormStringValue;
            }
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

            ref Dictionary<string, StormElement>? stormElementById = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.ScaleValueStormElementsByDataObjectType, levelScalingEntry.Catalog, out bool catalogExists);

            if (!catalogExists)
                stormElementById = [];

            ref StormElement? existingStormElement = ref CollectionsMarshal.GetValueRefOrAddDefault(stormElementById!, levelScalingEntry.Entry, out bool entryExists);

            if (!entryExists)
                existingStormElement = stormElement;
            else
                existingStormElement!.AddValue(stormElement);
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

    public void ClearCustomMods()
    {
        StormModStorages.RemoveAll(x => x.StormModType == StormModType.Custom);
        StormCustomCache.Clear();

        _loadedCustomMods = 0;
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
        ref HashSet<string>? elementTypes = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.ElementTypesByDataObjectType, dataObjectType, out bool exists);

        if (!exists)
            elementTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        elementTypes!.Add(elementName);

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);
    }

    private static void AddElementWithNoId(string elementName, StormCache currentStormCache, StormXElementValuePath stormXElementValuePath)
    {
        ref StormElement? stormElement = ref CollectionsMarshal.GetValueRefOrAddDefault(currentStormCache.StormElementByElementType, elementName, out bool exists);

        if (!exists)
            stormElement = new StormElement(stormXElementValuePath);
        else
            stormElement!.AddValue(stormXElementValuePath);
    }

    private void AddRootDefaults()
    {
        List<(string DataObjectType, string ElementName)> defaultBaseElementTypes = StormDefaultData.DefaultBaseElementsTypes;
        List<XElement> defaultXElements = StormDefaultData.DefaultXElements;

        foreach ((string dataObjectType, string elementName) in defaultBaseElementTypes)
        {
            AddBaseElementTypes(StormModType.Normal, dataObjectType, elementName);
        }

        foreach (XElement element in defaultXElements)
        {
            AddElement(StormModType.Normal, element, _rootFilePath);
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
        // stormmap mods are always at the end (or before customs) and grouped together
        for (int i = StormModStorages.Count - 1; i >= 0; i--)
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

