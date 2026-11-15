using System.Text;

namespace Heroes.StormData;

/// <summary>
/// Represents a container for a specific <c>stormmod</c> and its associated files and elements.
/// </summary>
public class StormModContainer
{
    private readonly SourceContainer _sourceContainer;

    private readonly StormXmlFileParser _xmlParser;
    private readonly DataStorage _specificDataStorage;

    private readonly List<StormFile> _notFoundFilesList = [];
    private readonly List<StormFile> _loadedXmlFilesList = [];
    private readonly List<StormFile> _loadedFontStylesFilesList = [];
    private readonly List<StormFile> _loadedAssetsTextFilesList = [];
    private readonly List<StormFile> _loadedAssetFilesList = [];
    private readonly List<StormFile> _loadedStormLayoutFilesList = [];

    internal StormModContainer(SourceContainer sourceContainer, StormModType stormModType)
    {
        _sourceContainer = sourceContainer;
        StormModType = stormModType;

        _specificDataStorage = _sourceContainer.LayeredDataStorage.GetDataStorage(StormModType);
        _xmlParser = _sourceContainer.StormXmlFileParser;

        NotFoundFiles = _notFoundFilesList.AsReadOnly();
        LoadedXmlFiles = _loadedXmlFilesList.AsReadOnly();
        LoadedAssetsTextFiles = _loadedAssetsTextFilesList.AsReadOnly();
        LoadedAssetFiles = _loadedAssetFilesList.AsReadOnly();
        LoadedStormLayoutFiles = _loadedStormLayoutFilesList.AsReadOnly();
    }

    /// <summary>
    /// Gets the type of the <c>stormmod</c>, which is the data layer (<see cref="StormModType.Base"/>, <see cref="StormModType.Map"/>,
    /// or <see cref="StormModType.Custom"/>) that this container loads its data into.
    /// </summary>
    public StormModType StormModType { get; }

    /// <summary>
    /// Gets the required files that were not found, such as the XML files listed in a <c>gamedata.xml</c> file.
    /// </summary>
    public IReadOnlyList<StormFile> NotFoundFiles { get; }

    /// <summary>
    /// Gets the XML files that were loaded, in the order they were loaded.
    /// </summary>
    public IReadOnlyList<StormFile> LoadedXmlFiles { get; }

    /// <summary>
    /// Gets the <c>assets.txt</c> files that were loaded.
    /// </summary>
    public IReadOnlyList<StormFile> LoadedAssetsTextFiles { get; }

    /// <summary>
    /// Gets the asset (<c>.dds</c>) files that were added.
    /// </summary>
    public IReadOnlyList<StormFile> LoadedAssetFiles { get; }

    /// <summary>
    /// Gets the <c>.stormlayout</c> files that were added.
    /// </summary>
    public IReadOnlyList<StormFile> LoadedStormLayoutFiles { get; }

    internal void LoadXmlFile(XmlReader xmlReader, StormFile stormFile)
    {
        _loadedXmlFilesList.Add(stormFile);

        IEnumerable<StormXmlElement> elements = _xmlParser.Parse(xmlReader, stormFile);

        foreach (StormXmlElement element in elements)
        {
            if (element is StormXmlConstElement constElement)
                _specificDataStorage.AddConstElement(constElement);
            else
                AddElement(element);
        }
    }

    internal void LoadFontStylesFile(XmlReader xmlReader, StormFile stormFile)
    {
        _loadedFontStylesFilesList.Add(stormFile);

        IEnumerable<StormXmlElement> elements = _xmlParser.ParseStyleFile(xmlReader, stormFile);

        foreach (StormXmlElement element in elements)
        {
            if (element is StormXmlStyleConstantElement styleConstantElement)
                _specificDataStorage.AddStyleConstantElement(styleConstantElement);
            else if (element is StormXmlStyleElement styleElement)
                _specificDataStorage.AddStyleElement(styleElement);
        }
    }

    // assets.txt
    internal void LoadAssetsTextFile(Stream stream, StormFile stormFile)
    {
        _loadedAssetsTextFilesList.Add(stormFile);

        using KeyValueTextReader reader = new(stream);

        while (reader.TryRead(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value))
        {
            string id = Encoding.UTF8.GetString(key);
            string text = value.IsEmpty ? string.Empty : Encoding.UTF8.GetString(value);

            _specificDataStorage.TextCache.AssetTextsById.Dictionary[id] = new StringValue(text, stormFile);
        }
    }

    internal void AddAssetFile(string relativePath, StormFile stormFile)
    {
        _loadedAssetFilesList.Add(stormFile);
        _specificDataStorage.FileCache.AssetFilesByRelativePath.Dictionary[relativePath] = stormFile;
    }

    internal void AddStormLayoutFile(string relativePath, StormFile stormFile)
    {
        _loadedStormLayoutFilesList.Add(stormFile);
        _specificDataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary[relativePath] = stormFile;
    }

    internal void AddFileNotFound(StormFile stormFile)
    {
        _notFoundFilesList.Add(stormFile);
    }

    private static string? GetCatalogNameFromFileName(StormXmlElement element)
    {
        string fileName = Path.GetFileName(element.StormFiles[0].RelativePath);

        int index = fileName.LastIndexOf("data.xml", StringComparison.OrdinalIgnoreCase);
        if (index > 1)
        {
            return fileName[..index];
        }

        return null;
    }

    private void AddElement(StormXmlElement element)
    {
        if (element.ElementType != StormXmlElementType.Class)
            return;

        string? catalogName = GetCatalogNameFromFileName(element);

        // base class catalog name check
        if (!string.IsNullOrWhiteSpace(catalogName) && element.ElementName.AsSpan(1).StartsWith(catalogName, StringComparison.OrdinalIgnoreCase))
        {
            _specificDataStorage.AddBaseClassCatalogName(element, catalogName);
        }

        // element with no id check
        if (string.IsNullOrEmpty(element.Id))
        {
            _specificDataStorage.AddElementWithNoId(element);
            return;
        }

        if (element.Members.TryGetMember("LevelScalingArray", out StormXmlArray? levelScalingArray))
            _specificDataStorage.AddLevelScalingElementValue(levelScalingArray);

        if (!_sourceContainer.LayeredDataStorage.TryGetFirstCatalogNameByElementName(element.ElementName, out string? existingCatalogName))
        {
            // TODO: more lookup
            existingCatalogName = string.Empty;
        }

        _specificDataStorage.AddElementWithId(element, existingCatalogName);

        // TODO: unitname attribute stuff
    }
}
