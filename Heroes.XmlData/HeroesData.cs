namespace Heroes.XmlData;

/// <summary>
/// Contains the methods and properties to access the xml and gamestring data.
/// </summary>
public class HeroesData : IHeroesData
{
    private readonly IStormStorage _stormStorage;

    //private readonly HashSet<RequiredStormFile> _notFoundDirectoriesList = [];
    //private readonly HashSet<RequiredStormFile> _notFoundFilesList = [];

    private StormLocale? _heroesLocalization;

    internal HeroesData(IStormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    /// <inheritdoc/>
    public StormLocale? HeroesLocalization => _heroesLocalization;

    //public int? GetBuildNumber() => _stormStorage.GetBuildId();

    /// <inheritdoc/>
    public bool IsGameStringExists(ReadOnlySpan<char> id)
    {
        return IsGameStringExists(id.ToString());
    }

    /// <inheritdoc/>
    public bool IsGameStringExists(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _stormStorage.StormCustomCache.GameStringsById.ContainsKey(id) ||
            _stormStorage.StormMapCache.GameStringsById.ContainsKey(id) ||
            _stormStorage.StormCache.GameStringsById.ContainsKey(id);
    }

    /// <inheritdoc/>
    public GameStringText GetGameString(ReadOnlySpan<char> id)
    {
        return GetGameString(id.ToString());
    }

    /// <inheritdoc/>
    public GameStringText GetGameString(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.GameStringsById.TryGetValue(id, out GameStringText? gameStringText))
            return gameStringText;

        // map cache second
        if (_stormStorage.StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            return gameStringText;

        return _stormStorage.StormCache.GameStringsById[id];
    }

    /// <inheritdoc/>
    public bool TryGetGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out GameStringText? gameStringText)
    {
        return TryGetGameString(id.ToString(), out gameStringText);
    }

    /// <inheritdoc/>
    public bool TryGetGameString(string id, [NotNullWhen(true)] out GameStringText? gameStringText)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        // map cache second
        if (_stormStorage.StormMapCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        if (_stormStorage.StormCache.GameStringsById.TryGetValue(id, out gameStringText))
            return true;

        return false;
    }

    /// <inheritdoc/>
    public bool IsLevelScalingEntryExists(string catalog, string entry, string field)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        return _stormStorage.StormCustomCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) || 
            _stormStorage.StormMapCache.ScaleValueByEntry.ContainsKey(levelScalingEntry) ||
            _stormStorage.StormCache.ScaleValueByEntry.ContainsKey(levelScalingEntry);
    }

    /// <inheritdoc/>
    public StormStringValue GetLevelScalingEntryExists(string catalog, string entry, string field)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out StormStringValue? stormStringValue))
            return stormStringValue;

        if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return stormStringValue;

        return _stormStorage.StormCache.ScaleValueByEntry[levelScalingEntry];
    }

    /// <inheritdoc/>
    public bool TryGetLevelScalingEntryExists(string catalog, string entry, string field, out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(field);

        LevelScalingEntry levelScalingEntry = new(catalog, entry, field);

        if (_stormStorage.StormCustomCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        if (_stormStorage.StormMapCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        if (_stormStorage.StormCache.ScaleValueByEntry.TryGetValue(levelScalingEntry, out stormStringValue))
            return true;

        return false;
    }

    /// <inheritdoc/>
    public bool IsStormStyleHexColorValueExists(ReadOnlySpan<char> name)
    {
        return IsStormStyleHexColorValueExists(name.ToString());
    }

    /// <inheritdoc/>
    public bool IsStormStyleHexColorValueExists(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _stormStorage.StormCustomCache.StormStyleHexColorValueByName.ContainsKey(name) ||
            _stormStorage.StormMapCache.StormStyleHexColorValueByName.ContainsKey(name) ||
            _stormStorage.StormCache.StormStyleHexColorValueByName.ContainsKey(name);
    }

    /// <inheritdoc/>
    public StormStringValue GetStormStyleHexColorValue(ReadOnlySpan<char> name)
    {
        return GetStormStyleHexColorValue(name.ToString());
    }

    /// <inheritdoc/>
    public StormStringValue GetStormStyleHexColorValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        // custom cache always first
        if (_stormStorage.StormCustomCache.StormStyleHexColorValueByName.TryGetValue(name, out StormStringValue? stormStringValue))
            return stormStringValue;

        // map cache second
        if (_stormStorage.StormMapCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return stormStringValue;

        return _stormStorage.StormCache.StormStyleHexColorValueByName[name];
    }

    /// <inheritdoc/>
    public bool TryGetStormStyleHexColorValue(ReadOnlySpan<char> name, out StormStringValue? stormStringValue)
    {
        return TryGetStormStyleHexColorValue(name.ToString(), out stormStringValue);
    }

    /// <inheritdoc/>
    public bool TryGetStormStyleHexColorValue(string name, out StormStringValue? stormStringValue)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_stormStorage.StormCustomCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (_stormStorage.StormMapCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        if (_stormStorage.StormCache.StormStyleHexColorValueByName.TryGetValue(name, out stormStringValue))
            return true;

        return false;
    }

    /// <inheritdoc/>
    public bool IsConstantElementExists(ReadOnlySpan<char> id)
    {
        return IsConstantElementExists(id.ToString());
    }

    /// <inheritdoc/>
    public bool IsConstantElementExists(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _stormStorage.StormCustomCache.ConstantElementById.ContainsKey(id) ||
            _stormStorage.StormMapCache.ConstantElementById.ContainsKey(id) ||
            _stormStorage.StormCache.ConstantElementById.ContainsKey(id);
    }

    /// <inheritdoc/>
    public StormXElementValue GetConstantElement(ReadOnlySpan<char> id)
    {
        return GetConstantElement(id.ToString());
    }

    /// <inheritdoc/>
    public StormXElementValue GetConstantElement(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.ConstantElementById.TryGetValue(id, out StormXElementValue? stormXElementValue))
            return stormXElementValue;

        // map cache second
        if (_stormStorage.StormMapCache.ConstantElementById.TryGetValue(id, out stormXElementValue))
            return stormXElementValue;

        return _stormStorage.StormCache.ConstantElementById[id];
    }

    /// <inheritdoc/>
    public bool TryGetConstantElement(ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXElementValue? stormXElementValue)
    {
        return TryGetConstantElement(id.ToString(), out stormXElementValue);
    }

    /// <inheritdoc/>
    public bool TryGetConstantElement(string id, [NotNullWhen(true)] out StormXElementValue? stormXElementValue)
    {
        ArgumentNullException.ThrowIfNull(id);

        // custom cache always first
        if (_stormStorage.StormCustomCache.ConstantElementById.TryGetValue(id, out stormXElementValue))
            return true;

        // map cache second
        if (_stormStorage.StormMapCache.ConstantElementById.TryGetValue(id, out stormXElementValue))
            return true;

        if (_stormStorage.StormCache.ConstantElementById.TryGetValue(id, out stormXElementValue))
            return true;

        return false;
    }

    /// <inheritdoc/>
    public bool IsElementExists(ReadOnlySpan<char> name)
    {
        return IsElementExists(name.ToString());
    }

    /// <inheritdoc/>
    public bool IsElementExists(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _stormStorage.StormCustomCache.ElementsByElementName.ContainsKey(name) ||
            _stormStorage.StormMapCache.ElementsByElementName.ContainsKey(name) ||
            _stormStorage.StormCache.ElementsByElementName.ContainsKey(name);
    }

    /// <inheritdoc/>
    public List<StormXElementValue> GetElements(ReadOnlySpan<char> name)
    {
        return GetElements(name.ToString());
    }

    /// <inheritdoc/>
    public List<StormXElementValue> GetElements(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!IsElementExists(name))
            throw new KeyNotFoundException(name);

        List<StormXElementValue> elements = [];

        if (_stormStorage.StormCache.ElementsByElementName.TryGetValue(name, out List<StormXElementValue>? stormXElementValues))
            elements.AddRange(stormXElementValues);

        if (_stormStorage.StormMapCache.ElementsByElementName.TryGetValue(name, out stormXElementValues))
            elements.AddRange(stormXElementValues);

        if (_stormStorage.StormCustomCache.ElementsByElementName.TryGetValue(name, out stormXElementValues))
            elements.AddRange(stormXElementValues);

        return elements;
    }

    /// <inheritdoc/>
    public bool TryGetElements(ReadOnlySpan<char> name, [NotNullWhen(true)] out List<StormXElementValue>? stormXElementValues)
    {
        return TryGetElements(name.ToString(), out stormXElementValues);
    }

    /// <inheritdoc/>
    public bool TryGetElements(string name, [NotNullWhen(true)] out List<StormXElementValue>? stormXElementValues)
    {
        ArgumentNullException.ThrowIfNull(name);

        stormXElementValues = null;
        bool success = false;

        if (_stormStorage.StormCache.ElementsByElementName.TryGetValue(name, out List<StormXElementValue>? cachStormXElementValues))
        {
            stormXElementValues ??= [];
            stormXElementValues.AddRange(cachStormXElementValues);
            success = true;
        }

        if (_stormStorage.StormMapCache.ElementsByElementName.TryGetValue(name, out cachStormXElementValues))
        {
            stormXElementValues ??= [];
            stormXElementValues.AddRange(cachStormXElementValues);
            success = true;
        }

        if (_stormStorage.StormCustomCache.ElementsByElementName.TryGetValue(name, out cachStormXElementValues))
        {
            stormXElementValues ??= [];
            stormXElementValues.AddRange(cachStormXElementValues);
            success = true;
        }

        return success;
    }

    void IHeroesData.SetHeroesLocalization(StormLocale stormLocale)
    {
        _heroesLocalization = stormLocale;
    }
}
