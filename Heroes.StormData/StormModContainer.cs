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

        _dataStorage = SourceContainer.Storage.GetDataStorage(StormModType);
        _xmlParser = new StormXmlFileParser(_dataStorage.ElementCache);
    }

    private SourceContainer SourceContainer { get; }

    private StormModType StormModType { get; }

    internal void LoadXmlFile(XmlReader xmlReader, StormFile stormFile)
    {
        _loadedXmlFilesList.Add(stormFile);

        IEnumerable<StormXmlElement> elements = _xmlParser.Parse(xmlReader, stormFile);

        foreach (StormXmlElement element in elements)
        {
            SourceContainer.AddElement(element, _dataStorage);
        }

    }

    internal void AddFileNotFound(StormFile stormFile)
    {
        _notFoundFilesList.Add(stormFile);
       // _stormStorage.AddFileNotFound(StormModType, stormFile);
    }

}
