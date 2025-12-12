namespace Heroes.XmlData.StormMods;

internal sealed class CustomStormMod : IStormMod
{
    private readonly IHeroesSource _heroesSource;
    private readonly ManualModLoader _manualModLoader;

    private readonly StormPath _stormPath;

    public CustomStormMod(IHeroesSource heroesSource, ManualModLoader manualModLoader)
    {
        Name = manualModLoader.Name;
        _heroesSource = heroesSource;
        _manualModLoader = manualModLoader;

        StormModStorage = heroesSource.StormStorage.CreateModStorage(this);

        _stormPath = new StormPath()
        {
            StormModName = $"custom-{Name}",
            StormModPath = DirectoryPath,
            Path = CustomPath,
            PathType = StormPathType.File,
        };
    }

    public string Name { get; }

    public string DirectoryPath => string.Empty;

    public StormModType StormModType => StormModType.Custom;

    public IStormModStorage StormModStorage { get; }

    internal string CustomPath => Path.Join(HxdConstants.Name, "custom", Name);

    public IEnumerable<IStormMod> GetStormMapMods(S2MAProperties s2maProperties)
    {
        throw new NotImplementedException();
    }

    public List<IStormMod> LoadDocumentInfoFile()
    {
        throw new NotImplementedException();
    }

    public void LoadStormData()
    {
        foreach (XElement constantXElement in _manualModLoader.ConstantXElements)
        {
            _heroesSource.StormStorage.AddConstantXElement(StormModType, constantXElement, _stormPath);
        }

        foreach (var items in _manualModLoader.ElementNamesByDataObjectType)
        {
            foreach (string elementName in items.Value)
            {
                _heroesSource.StormStorage.AddBaseElementTypes(StormModType, items.Key, elementName);
            }
        }

        StormModStorage.UpdateAttributes(_manualModLoader.Elements.DescendantsAndSelf());

        foreach (XElement element in _manualModLoader.Elements)
        {
            _heroesSource.StormStorage.AddElement(StormModType, element, _stormPath);
        }

        foreach (XElement element in _manualModLoader.LevelScalingArrayElements)
        {
            _heroesSource.StormStorage.AddLevelScalingArrayElement(StormModType, element, _stormPath);
        }

        foreach (XElement element in _manualModLoader.StormStyleElements)
        {
            _heroesSource.StormStorage.AddStormStyleElement(StormModType, element, _stormPath);
        }

        foreach (string filePath in _manualModLoader.AssetFilePaths)
        {
            _heroesSource.StormStorage.AddAssetFilePath(StormModType, filePath, new StormPath()
            {
                Path = filePath,
                PathType = StormPathType.File,
                StormModName = $"custom-{Name}",
                StormModPath = DirectoryPath,
            });
        }

        foreach (StormMap stormMap in _manualModLoader.StormMaps)
        {
            _heroesSource.S2MAPropertiesByTitle.TryAdd(stormMap.Name, new S2MAProperties()
            {
                MapId = stormMap.MapId,
                DirectoryPath = stormMap.S2MAFilePath,
                S2MVProperties = new S2MVProperties()
                {
                    MapLink = stormMap.MapLink,
                    MapSize = new Point(stormMap.MapSize.X, stormMap.MapSize.Y),
                    NameByStormLocale = new Dictionary<StormLocale, string>(stormMap.NameByLocale),
                    LoadingImage = stormMap.LoadingScreenImagePath,
                    PreviewLargeImage = stormMap.ReplayPreviewImagePath,
                    CustomLayout = stormMap.LayoutFilePath,
                    CustomFrame = stormMap.LayoutLoadingScreenFrame,
                    DirectoryPath = stormMap.S2MAFilePath,
                },
            });
        }
    }

    public void LoadStormGameStrings(StormLocale stormLocale)
    {
        if (_manualModLoader.GameStringsByLocale.TryGetValue(stormLocale, out List<string>? gameStrings))
        {
            foreach (string gameString in gameStrings)
            {
                var gamestring = _heroesSource.StormStorage.GetGameStringWithId(gameString, _stormPath);

                if (gamestring is not null)
                    StormModStorage.AddGameString(gamestring.Value.Id, gamestring.Value.GameStringText);
            }
        }
    }
}
