using System.Text;

namespace Heroes.StormData;

/// <summary>
/// Represents a container for a specific <c>stormmod</c> and its associated files and elements.
/// </summary>
public class StormModContainer
{
    private readonly StormXmlFileParser _xmlParser;
    private readonly DataStorage _specificDataStorage;

    private readonly List<StormFile> _notFoundFilesList = [];
    private readonly List<StormFile> _loadedXmlFilesList = [];
    private readonly List<StormFile> _loadedAssetsTextFilesList = [];
    private readonly List<StormFile> _loadedAssetFilesList = [];
    private readonly List<StormFile> _loadedStormLayoutFilesList = [];

    internal StormModContainer(SourceContainer sourceContainer, StormModType stormModType)
    {
        SourceContainer = sourceContainer;
        StormModType = stormModType;

        _specificDataStorage = SourceContainer.LayeredDataStorage.GetDataStorage(StormModType);
        _xmlParser = SourceContainer.StormXmlFileParser;
    }

    public StormModType StormModType { get; }

    private SourceContainer SourceContainer { get; }

    internal void LoadXmlFile(XmlReader xmlReader, StormFile stormFile)
    {
        _loadedXmlFilesList.Add(stormFile);

        IEnumerable<StormXmlElement> elements = _xmlParser.Parse(xmlReader, stormFile);

        foreach (StormXmlElement element in elements)
        {
            if (element.ElementType == StormXmlElementType.Const)
                _specificDataStorage.AddConstElement((StormXmlConstElement)element);
            else
                AddElement(element);
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
        string fileName = Path.GetFileName(element.StormFiles[0].StormModPath);

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

        if (!SourceContainer.LayeredDataStorage.TryGetFirstCatalogNameByElementName(element.ElementName, out string? existingCatalogName))
        {
            // TODO: more lookup
            existingCatalogName = string.Empty;
        }

        _specificDataStorage.AddElementWithId(element, existingCatalogName);

        // TODO: unitname attribute stuff
    }
}
