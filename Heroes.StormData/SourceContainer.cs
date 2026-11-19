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

    public void ClearStormMapMods()
    {
        ClearStormMapContainers();
        //StormMapCache.Clear();
    }

    private void AddDefaults()
    {
        using XmlReader reader = XmlReader.Create(new StringReader(StormDefaultData.DefaultXml));

        foreach (StormXmlElement element in StormXmlFileParser.Parse(reader, StormDefaultData.StormFile))
            LayeredDataStorage.GetDataStorage(StormModType.Base).AddElementWithNoId(element);
    }

    private void ClearStormMapContainers()
    {
        //// stormmap mods are always at the end (or before customs) and grouped together
        //for (int i = StormModStorages.Count - 1; i >= 0; i--)
        //{
        //    if (_loadedMapMods < 1)
        //        break;

        //    if (StormModStorages[i].StormModType == StormModType.Map)
        //    {
        //        StormModStorages.RemoveAt(i);
        //        _loadedMapMods--;
        //    }
        //}
    }
}
