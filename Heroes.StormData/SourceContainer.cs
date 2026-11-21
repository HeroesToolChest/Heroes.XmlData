namespace Heroes.StormData;

/// <summary>
/// Holds the data across all the <c>stormmods</c>.
/// </summary>
internal class SourceContainer
{
    private int _loadedNormalMods;
    private int _loadedMapMods;
    private int _loadedCustomMods;

    public SourceContainer()
    {
        LayeredDataStorage = new LayeredDataStorage();
        StormXmlFileParser = new StormXmlFileParser(LayeredDataStorage.Elements, LayeredDataStorage.Texts);

        AddDefaults();
    }

    /// <summary>
    /// Gets the storage for the base, map, and custom data.
    /// </summary>
    public LayeredDataStorage LayeredDataStorage { get; }

    /// <summary>
    /// Gets the parser used to parse the XML files.
    /// </summary>
    public StormXmlFileParser StormXmlFileParser { get; }

    public List<StormModContainer> StormModContainers { get; } = [];

    private bool HasLoadedMapMods => _loadedMapMods > 0;

    private bool HasLoadedCustomMods => _loadedCustomMods > 0;

    public void AddModContainer(StormModContainer stormModContainer)
    {
        if (stormModContainer.StormModType == StormModType.Base)
        {
            if (HasLoadedMapMods)
            {
                int insertIndex = StormModContainers.FindIndex(x => x.StormModType == StormModType.Map);
                StormModContainers.Insert(insertIndex, stormModContainer);
            }
            else if (HasLoadedCustomMods)
            {
                int insertIndex = StormModContainers.FindIndex(x => x.StormModType == StormModType.Custom);
                StormModContainers.Insert(insertIndex, stormModContainer);
            }
            else
            {
                StormModContainers.Add(stormModContainer);
            }

            _loadedNormalMods++;
        }
        else if (stormModContainer.StormModType == StormModType.Map)
        {
            if (HasLoadedCustomMods)
            {
                int insertIndex = StormModContainers.FindIndex(x => x.StormModType == StormModType.Custom);
                StormModContainers.Insert(insertIndex, stormModContainer);
            }
            else
            {
                StormModContainers.Add(stormModContainer);
            }

            _loadedMapMods++;
        }
        else if (stormModContainer.StormModType == StormModType.Custom)
        {
            StormModContainers.Add(stormModContainer);

            _loadedCustomMods++;
        }
    }

    /// <summary>
    /// Removes the gamestrings from all the <see cref="StormModContainers"/> and from the base, map, and custom <see cref="DataStorage"/>.
    /// </summary>
    public void ClearGamestrings()
    {
        foreach (StormModContainer stormModContainer in StormModContainers)
            stormModContainer.ClearGameStrings();

        LayeredDataStorage.ClearGameStrings();
    }

    /// <summary>
    /// Removes the <see cref="StormModType.Map"/> containers and all the data loaded into the map <see cref="DataStorage"/>.
    /// </summary>
    public void ClearStormMapMods()
    {
        ClearStormMapContainers();
        LayeredDataStorage.ClearDataStorage(StormModType.Map);
    }

    private void AddDefaults()
    {
        using XmlReader reader = XmlReader.Create(new StringReader(StormDefaultData.DefaultXml));

        foreach (StormXmlElement element in StormXmlFileParser.Parse(reader, StormDefaultData.StormFile))
            LayeredDataStorage.GetDataStorage(StormModType.Base).AddElementWithNoId(element);
    }

    private void ClearStormMapContainers()
    {
        StormModContainers.RemoveAll(x => x.StormModType == StormModType.Map);
        _loadedMapMods = 0;
    }
}
