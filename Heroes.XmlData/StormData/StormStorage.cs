using Heroes.XmlData.StormMath;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods.
/// </summary>
internal partial class StormStorage : IStormStorage
{
    private readonly List<StormModStorage> _stormModStorages = [];

    private int _loadedMapMods;

    public StormCache StormCache { get; } = new();

    public StormCache StormMapCache { get; } = new();

    public StormCache StormCustomCache { get; } = new();

    public StormModStorage CreateModStorage(IStormMod stormMod, string modsDirectoryPath)
    {
        return new(stormMod, this, modsDirectoryPath);
    }

    public void AddModStorage(StormModStorage stormModStorage)
    {
        _stormModStorages.Add(stormModStorage);

        if (stormModStorage.StormModType == StormModType.Map)
            _loadedMapMods++;
    }

    public void AddDirectoryNotFound(StormModType stormModType, StormFile stormDirectory)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.NotFoundDirectoriesList.Add(stormDirectory);
    }

    public void AddFileNotFound(StormModType stormModType, StormFile stormFile)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.NotFoundFilesList.Add(stormFile);
    }

    public void AddGameString(StormModType stormModType, string id, GameStringText gameStringText)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        currentStormCache.GameStringsById[id] = gameStringText;
    }

    public (string Id, GameStringText GameStringText)? GetGameStringWithId(ReadOnlySpan<char> gamestring, ReadOnlySpan<char> path)
    {
        Span<Range> ranges = stackalloc Range[2];

        gamestring.Split(ranges, '=', StringSplitOptions.None);

        if (gamestring[ranges[0]].IsEmpty || gamestring[ranges[0]].IsWhiteSpace())
            return null;

        GameStringText gameStringText = new(gamestring[ranges[1]].ToString(), path.ToString());

        string id = gamestring[ranges[0]].ToString();

        return (id, gameStringText);
    }

    public bool AddConstantXElement(StormModType stormModType, XElement element, string path)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (element.Name.LocalName.Equals("const", StringComparison.OrdinalIgnoreCase))
        {
            string? id = element.Attribute("id")?.Value;

            if (string.IsNullOrEmpty(id))
                return false;

            currentStormCache.ConstantXElementById.TryAdd(id, new StormXElementValuePath(element, path));

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
            if (TryGetConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
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
            if (TryGetConstantXElementById(text, out StormXElementValuePath? stormXElementValuePath))
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

    public bool AddBaseElementTypes(StormModType stormModType, string dataObjectType, string elementName)
    {
        if (!elementName.StartsWith('C'))
            return false;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        if (TryGetElementTypesByDataObjectType(dataObjectType, out HashSet<string>? elementTypes))
            elementTypes.Add(elementName);
        else
            currentStormCache.ElementTypesByDataObjectType.Add(dataObjectType, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { elementName });

        currentStormCache.DataObjectTypeByElementType.TryAdd(elementName, dataObjectType);

        return true;
    }

    public bool AddElement(StormModType stormModType, XElement element, string filePath)
    {
        string elementName = element.Name.LocalName;

        if (elementName.StartsWith('S'))
            return false;

        string? idAtt = element.Attribute("id")?.Value;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);
        StormXElementValuePath stormXElementValuePath = new(element, filePath);

        if (string.IsNullOrEmpty(idAtt))
        {
            if (TryGetStormElementByElementType(elementName, out StormElement? stormElement))
                stormElement.AddValue(stormXElementValuePath);
            else
                currentStormCache.StormElementByElementType.Add(elementName, new StormElement(stormXElementValuePath));
        }
        else
        {
            if (!TryGetDataObjectTypeByElementType(elementName, out string? dataObjectType))
            {
                // didnt find one, so look for an existing match
                string foundExistingDataObjectType = FindExistingDataObjectType(elementName);

                AddBaseElementTypes(stormModType, foundExistingDataObjectType, elementName);

                dataObjectType = foundExistingDataObjectType;
            }

            if (!currentStormCache.StormElementsByDataObjectType.ContainsKey(dataObjectType))
                currentStormCache.StormElementsByDataObjectType.Add(dataObjectType, []);

            if (TryGetStormElementsByDataObjectType(dataObjectType, idAtt, out StormElement? stormElement))
                stormElement.AddValue(stormXElementValuePath);
            else
                currentStormCache.StormElementsByDataObjectType[dataObjectType].Add(idAtt, new StormElement(stormXElementValuePath));
        }

        return true;
    }

    public void SetFontStyleCache(StormModType stormModType, XDocument document, string filePath)
    {
        foreach (XElement element in document.Root!.Elements())
        {
            AddStormStyleHexColor(stormModType, element, filePath);
        }
    }

    public void AddStormStyleHexColor(StormModType stormModType, XElement element, string filePath)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);

        string elementName = element.Name.LocalName;
        if (elementName.Equals("Constant", StringComparison.OrdinalIgnoreCase))
        {
            string? name = element.Attribute("name")?.Value;
            string? val = element.Attribute("val")?.Value;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val))
                return;

            currentStormCache.StormStyleConstantsHexColorValueByName[name] = new StormStringValue(val, filePath);
        }
        else if (elementName.Equals("Style", StringComparison.OrdinalIgnoreCase))
        {
            string? name = element.Attribute("name")?.Value;
            string? textColor = element.Attribute("textcolor")?.Value;

            if (string.IsNullOrEmpty(textColor) || string.IsNullOrEmpty(name))
                return;

            //if (textColor[0] == '#')
            //{
            //    if (TryGetStormStyleHexColorValue(textColor[1..], out StormStringValue? stormStringValue))
            //    {

            //    }
            //   // if (TryGetStormStyleConstantsHexColorValue(textColor[1..], out StormStringValue? stormStringValue))
            //   //     currentStormCache.StormStyleHexColorValueByName[name] = new StormStringValue(stormStringValue.Value, filePath);
            //}
            //else
            //{
            currentStormCache.StormStyleHexColorValueByName[name] = new StormStringValue(textColor, filePath);
            //}

            // TODO: needed anymore?
            // else if (!textColor.Contains(',', StringComparison.OrdinalIgnoreCase))
            // {
            //     _stormStyleHexColorValueByName.TryAdd(name, textColor);
            // }
        }
    }

    public void AddLevelScalingArrayElement(StormModType stormModType, XElement element, string filePath)
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

                StormStringValue stormStringValue = new(value, filePath);

                currentStormCache.ScaleValueByEntry[new(catalog, entry, field)] = stormStringValue;

                string? newField = AddDefaultIndexerToMultiFields(field);

                if (!string.IsNullOrWhiteSpace(newField))
                {
                    currentStormCache.ScaleValueByEntry[new(catalog, entry, newField)] = stormStringValue;
                }
            }
        }
    }

    public void ClearGamestrings()
    {
        _stormModStorages.ForEach(x => x.ClearGameStrings());

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

    // DamageResponse.ModifyLimit
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

        return newFieldBuffer.TrimEnd('.').ToString();
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

