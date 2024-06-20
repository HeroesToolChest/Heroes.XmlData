using Heroes.XmlData.StormMath;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods.
/// </summary>
internal partial class StormStorage : IStormStorage
{
    private readonly StormPath _rootFilePath;

    private readonly List<StormModStorage> _stormModStorages = [];

    private int _loadedMapMods;

    public StormStorage(bool hasRootDefaults = true)
    {
        _rootFilePath = new StormPath()
        {
            StormModName = HxdConstants.Name,
            StormModDirectoryPath = string.Empty,
            Path = $"{HxdConstants.Name}-root",
            PathType = StormPathType.Hxd,
        };

        if (hasRootDefaults)
            AddRootDefaults();
    }

    public StormCache StormCache { get; } = new();

    public StormCache StormMapCache { get; } = new();

    public StormCache StormCustomCache { get; } = new();

    public StormModStorage CreateModStorage(IStormMod stormMod)
    {
        return new(stormMod, this);
    }

    public void AddModStorage(StormModStorage stormModStorage)
    {
        _stormModStorages.Add(stormModStorage);

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

    public bool AddConstantXElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (element.Name.LocalName.Equals("const", StringComparison.OrdinalIgnoreCase))
        {
            string? id = element.Attribute("id")?.Value;

            if (string.IsNullOrEmpty(id))
                return false;

            currentStormCache.ConstantXElementById.TryAdd(id, new StormXElementValuePath(element, stormPath));

            return true;
        }

        return false;
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
            if (TryGetExistingConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
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
            if (TryGetExistingConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
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

    public void AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName)
    {
        if (!elementName.StartsWith('C'))
            return;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (TryGetExistingElementTypesByDataObjectType(dataObjectType, out HashSet<string>? elementTypes))
            elementTypes.Add(elementName);
        else
            currentStormCache.ElementTypesByDataObjectType.Add(dataObjectType, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { elementName });

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);
    }

    public void AddElement(StormModType stormModType, XElement element, StormPath stormPath)
    {
        string elementName = element.Name.LocalName;

        if (elementName.StartsWith('S'))
            return;

        string? idAtt = element.Attribute("id")?.Value;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);
        StormXElementValuePath stormXElementValuePath = new(element, stormPath);

        if (string.IsNullOrEmpty(idAtt))
        {
            if (TryGetExistingStormElementByElementType(elementName, out StormElement? stormElement))
                stormElement.AddValue(stormXElementValuePath);
            else
                currentStormCache.StormElementByElementType.Add(elementName, new StormElement(stormXElementValuePath));
        }
        else
        {
            if (!TryGetExistingDataObjectTypeByElementType(elementName, out string? dataObjectType))
            {
                // didnt find one, so look for an existing match
                string foundExistingDataObjectType = FindExistingDataObjectType(elementName);

                AddBaseElementTypes(stormModType, foundExistingDataObjectType, elementName);

                dataObjectType = foundExistingDataObjectType;
            }

            if (!currentStormCache.StormElementsByDataObjectType.ContainsKey(dataObjectType))
                currentStormCache.StormElementsByDataObjectType.Add(dataObjectType, []);

            if (TryGetExistingStormElementById(idAtt, dataObjectType, out StormElement? stormElement))
                stormElement.AddValue(stormXElementValuePath);
            else
                currentStormCache.StormElementsByDataObjectType[dataObjectType].Add(idAtt, new StormElement(stormXElementValuePath));
        }
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

            currentStormCache.StormStyleConstantsByName[name] = new StormElement(new StormXElementValuePath(element, stormPath));
        }
        else if (elementName.Equals("Style", StringComparison.OrdinalIgnoreCase))
        {
            string? name = element.Attribute("name")?.Value;

            if (string.IsNullOrEmpty(name))
                return;

            currentStormCache.StormStyleStylesByName[name] = new StormElement(new StormXElementValuePath(element, stormPath));
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

    public void BuildDataForScalingAttributes(StormModType stormModType)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        Dictionary<LevelScalingEntry, StormStringValue> scaleValuesByEntry = currentStormCache.ScaleValueByEntry;

        foreach (var scaling in scaleValuesByEntry)
        {
            LevelScalingEntry levelScalingEntry = scaling.Key;
            StormStringValue stormStringValue = scaling.Value;

            StormElement? stormElement = ScaleValueParser.CreateStormElement(this, new LevelScalingEntry(levelScalingEntry.Catalog, levelScalingEntry.Entry, levelScalingEntry.Field), stormStringValue);

            if (stormElement is not null)
            {
                if (!currentStormCache.ScaleValueStormElementsByDataObjectType.ContainsKey(levelScalingEntry.Catalog))
                    currentStormCache.ScaleValueStormElementsByDataObjectType.Add(levelScalingEntry.Catalog, []);

                if (TryGetExistingScaleValueStormElementById(levelScalingEntry.Entry, levelScalingEntry.Catalog, out StormElement? existingStormElement))
                    existingStormElement.AddValue(stormElement);
                else
                    currentStormCache.ScaleValueStormElementsByDataObjectType[levelScalingEntry.Catalog].Add(levelScalingEntry.Entry, stormElement);
            }
        }
    }

    public void ClearGamestrings()
    {
        foreach (StormModStorage stormModStorage in _stormModStorages)
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
        return _stormModStorages.FirstOrDefault()?.BuildId;
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

    private string FindExistingDataObjectType(string elementName)
    {
        // normal cache first
        string? foundExistingDataObjectType = StormCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x));

        foundExistingDataObjectType ??= StormMapCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x));
        foundExistingDataObjectType ??= StormCustomCache.ElementTypesByDataObjectType.Keys.FirstOrDefault(x => elementName.AsSpan()[1..].StartsWith(x)) ??
            throw new HeroesXmlDataException($"Could not find an existing data object type for \"{elementName}\"");

        return foundExistingDataObjectType;
    }

    // i.e DamageResponse.ModifyLimit
    private string? AddDefaultIndexerToMultiFields(ReadOnlySpan<char> field)
    {
        int splitterCount = field.Count('.');
        if (splitterCount < 1)
            return null;

        Span<Range> ranges = stackalloc Range[splitterCount + 1];

        field.Split(ranges, '.');

        Span<char> newFieldBuffer = stackalloc char[field.Length + (ranges.Length * 3)];

        int currentIndex = 0;
        for (int i = 0; i < ranges.Length; i++)
        {
            ReadOnlySpan<char> fieldPart = field[ranges[i]];

            fieldPart.CopyTo(newFieldBuffer[currentIndex..]);
            currentIndex += fieldPart.Length;

            if (fieldPart[^1] != ']')
            {
                if (i + 1 < ranges.Length)
                {
                    "[0].".CopyTo(newFieldBuffer[currentIndex..]);
                    currentIndex += 4;
                }
                else
                {
                    "[0]".CopyTo(newFieldBuffer[currentIndex..]);
                }
            }
        }

        return newFieldBuffer.TrimEnd('\0').ToString();
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
        for (int i = _stormModStorages.Count - 1; i > 0; i--)
        {
            if (_loadedMapMods < 1)
                break;

            if (_stormModStorages[i].StormModType == StormModType.Map)
            {
                _stormModStorages.RemoveAt(i);
                _loadedMapMods--;
            }
        }
    }
}

