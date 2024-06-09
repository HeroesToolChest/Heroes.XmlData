using Heroes.XmlData.StormMath;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Storage for all storm mods.
/// </summary>
internal partial class StormStorage : IStormStorage
{
    private const string _rootFilePath = $"{HxdConstants.Name}-root";

    private readonly List<StormModStorage> _stormModStorages = [];

    private int _loadedMapMods;

    public StormStorage(bool hasRootDefaults = true)
    {
        if (hasRootDefaults)
            AddRootDefaults();
    }

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

    public void AddElement(StormModType stormModType, XElement element, string filePath)
    {
        string elementName = element.Name.LocalName;

        if (elementName.StartsWith('S'))
            return;

        string? idAtt = element.Attribute("id")?.Value;

        StormCache currentStormCache = GetCurrentStormCache(stormModType);
        StormXElementValuePath stormXElementValuePath = new(element, filePath);

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

                //string? newField = AddDefaultIndexerToMultiFields(field);

                //if (!string.IsNullOrWhiteSpace(newField))
                //{
                //    currentStormCache.ScaleValueByEntry[new(catalog, entry, newField)] = stormStringValue;
                //}



                //AddScaleValueData(currentStormCache, catalog, entry, field, stormStringValue);

                //string? newField = AddDefaultIndexerToMultiFields(field);

                //if (!string.IsNullOrWhiteSpace(newField))
                //{
                //    AddScaleValueData(currentStormCache, catalog, entry, newField, stormStringValue);
                //}
            }
        }


    }

    public void BuildDataForScalingAttributes(StormModType stormModType)
    {
        StormCache currentStormCache = GetCurrentStormCache(stormModType);


        //var scalingStormElements = currentStormCache.ScaleValueStormElementsByDataObjectType;

        //foreach (var scalingElement in scalingStormElements)
        //{
        //    foreach (var stormElmement in scalingElement.Value)
        //    {

        //    }
        //}
        Dictionary<LevelScalingEntry, StormStringValue> scaleValuesByEntry = currentStormCache.ScaleValueByEntry;

        foreach (var scaling in scaleValuesByEntry)
        {
            LevelScalingEntry levelScalingEntry = scaling.Key;
            StormStringValue stormStringValue = scaling.Value;

            //AddScaleValueData(currentStormCache, levelScalingEntry, stormStringValue);

            StormElement? stormElement = ScaleValueParser.CreateStormElement(this, new LevelScalingEntry(levelScalingEntry.Catalog, levelScalingEntry.Entry, levelScalingEntry.Field), stormStringValue);

            if (stormElement is not null)
            {
                if (!currentStormCache.ScaleValueStormElementsByDataObjectType.ContainsKey(levelScalingEntry.Catalog))
                    currentStormCache.ScaleValueStormElementsByDataObjectType.Add(levelScalingEntry.Catalog, []);

                if (TryGetExistingScaleValueStormElementByDataObjectType(levelScalingEntry.Catalog, levelScalingEntry.Entry, out StormElement? existingStormElement))
                    existingStormElement.AddValue(stormElement);
                else
                    currentStormCache.ScaleValueStormElementsByDataObjectType[levelScalingEntry.Catalog].Add(levelScalingEntry.Entry, stormElement);
            }

            //string? newField = AddDefaultIndexerToMultiFields(levelScalingEntry.Field);

            //if (!string.IsNullOrWhiteSpace(newField))
            //{
            //    AddScaleValueData(currentStormCache, new LevelScalingEntry(levelScalingEntry.Catalog, levelScalingEntry.Entry, newField), stormStringValue);
            //}

            //if (!_scaleValueParser.CreateStormElement(item.Key, item.Value))
            //{
            //    currentStormCache.ScaleValuesNotFoundList.Add(item);
            //}
        }

        //currentStormCache.ScaleValueByEntry.Clear();

        //void AddScaleValueData(StormCache currentStormCache, LevelScalingEntry levelScalingEntry, StormStringValue stormStringValue)
        //{
        //    StormElement? stormElement = ScaleValueParser.CreateStormElement(this, new LevelScalingEntry(levelScalingEntry.Catalog, levelScalingEntry.Entry, levelScalingEntry.Field), stormStringValue);

        //    if (stormElement is not null)
        //    {
        //        if (!currentStormCache.ScaleValueStormElementsByDataObjectType.ContainsKey(levelScalingEntry.Catalog))
        //            currentStormCache.ScaleValueStormElementsByDataObjectType.Add(levelScalingEntry.Catalog, []);

        //        if (TryGetExistingScaleValueStormElementByDataObjectType(levelScalingEntry.Catalog, levelScalingEntry.Entry, out StormElement? existingStormElement))
        //            existingStormElement.AddValue(stormElement);
        //        else
        //            currentStormCache.ScaleValueStormElementsByDataObjectType[levelScalingEntry.Catalog].Add(levelScalingEntry.Entry, stormElement);
        //    }
        //}
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

    //private static void AddScalingValue(LevelScalingEntry levelScalingEntry, StormElement stormElement, StormStringValue stormStringValue)
    //{
    //    StormElementData currentElementData = stormElement.DataValues;
    //    ReadOnlySpan<char> fieldSpan = levelScalingEntry.Field;
    //    int splitterCount = fieldSpan.Count('.');

    //    Span<Range> xmlParts = stackalloc Range[splitterCount + 1];

    //    fieldSpan.Split(xmlParts, '.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    //    DataRefParser.UpdateStormElementDataToLastFieldPart(currentElementData, levelScalingEntry.Field, xmlParts);

    //    if (currentElementData.HasConstValue)
    //    {
    //        //return GetValueScale(currentElementData.ConstValue, fullPartSpan, xmlParts);
    //    }
    //    else if (currentElementData.HasValue)
    //    {
    //        if (currentElementData.HasTextIndex)
    //            return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts, currentElementData.KeyValueDataPairs.First().Key);
    //        else
    //            return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts);
    //    }
    //    else if (stormElement.HasParentId)
    //    {
    //        // check the parents
    //        return ParseStormElement(fullPartSpan, stormElement.ParentId, xmlParts);
    //    }
    //    else if (currentElementType == ElementType.Normal)
    //    {
    //        // then check the element type, which has no id attribute
    //        return ParseStormElementType(stormElement.ElementType, fullPartSpan, xmlParts);
    //    }
    //    else if (currentElementType == ElementType.Type)
    //    {
    //        // then check the base element type, may not be the correct one, but close enough
    //        return ParseBaseElementType(stormElement.ElementType, fullPartSpan, xmlParts);
    //    }

    //    stormElement.DataValues.KeyValueDataPairs.Add(_scaleAttributeName, new StormElementData(stormStringValue.Value));
    //}

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
            throw new Exception("TODO");

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

