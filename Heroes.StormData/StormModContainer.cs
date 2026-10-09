namespace Heroes.StormData;

/// <summary>
/// Represents a container for a specific <c>stormmod</c> and its associated files and elements.
/// </summary>
public class StormModContainer
{
    private readonly StormXmlFileParser _xmlParser;
    private readonly DataStorage _dataStorage;

    private readonly List<StormFile> _notFoundFilesList = [];
    private readonly List<StormFile> _loadedXmlFilesList = [];

    internal StormModContainer(SourceContainer sourceContainer, StormModType stormModType)
    {
        SourceContainer = sourceContainer;
        StormModType = stormModType;

        _dataStorage = SourceContainer.LayeredDataStorage.GetDataStorage(StormModType);
        _xmlParser = new StormXmlFileParser(SourceContainer.LayeredDataStorage);
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
                _dataStorage.AddConstElement((StormXmlConstElement)element);
            else
                _dataStorage.AddElement(element);
        }

    }

    internal void AddFileNotFound(StormFile stormFile)
    {
        _notFoundFilesList.Add(stormFile);
       // _stormStorage.AddFileNotFound(StormModType, stormFile);
    }
}
