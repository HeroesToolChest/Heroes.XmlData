namespace Heroes.XmlData.StormMods;

internal class CustomStormMod : IStormMod
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
            StormModDirectoryPath = CustomPath,
            Path = CustomPath,
            PathType = StormPathType.File,
        };
    }

    public string Name { get; }

    public string DirectoryPath => string.Empty;

    public StormModType StormModType => StormModType.Custom;

    public StormModStorage StormModStorage { get; }

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

        StormModStorage.UpdateConstantAttributes(_manualModLoader.Elements.DescendantsAndSelf());

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
