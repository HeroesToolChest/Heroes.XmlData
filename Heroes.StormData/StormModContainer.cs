using System.Reflection.Metadata;
using System.Runtime.InteropServices;

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

    internal StormModContainer(SourceContainer sourceContainer, StormModType stormModType)
    {
        SourceContainer = sourceContainer;
        StormModType = stormModType;

        _specificDataStorage = SourceContainer.LayeredDataStorage.GetDataStorage(StormModType);
        _xmlParser = SourceContainer.StormXmlFileParser;
    }

    private SourceContainer SourceContainer { get; }

    private StormModType StormModType { get; }

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

    internal void AddFileNotFound(StormFile stormFile)
    {
        _notFoundFilesList.Add(stormFile);
       // _stormStorage.AddFileNotFound(StormModType, stormFile);
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

        if (!SourceContainer.LayeredDataStorage.TryGetFirstCatalogNameByElementName(element.ElementName, out string? existingCatalogName))
        {
            // TODO: more lookup
            existingCatalogName = string.Empty;
        }

        _specificDataStorage.AddElementWithId(element, existingCatalogName);
        
        // TODO: unitname attribute stuff
    }
}
