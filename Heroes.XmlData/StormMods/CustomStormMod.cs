using U8Xml;

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
            using XmlObject xmlObject = XmlParser.Parse(constantXElement.ToString());
            _heroesSource.StormStorage.AddConstantElement(StormModType, xmlObject.Root, _stormPath);
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
            using XmlObject xmlObject = XmlParser.Parse(element.ToString());
            _heroesSource.StormStorage.AddElement(StormModType, xmlObject.Root, _stormPath);
        }

        foreach (XElement element in _manualModLoader.LevelScalingArrayElements)
        {
            using XmlObject xmlObject = XmlParser.Parse(element.ToString());
            _heroesSource.StormStorage.AddLevelScalingArrayElement(StormModType, xmlObject.Root, _stormPath);
        }

        foreach (XElement element in _manualModLoader.StormStyleElements)
        {
            using XmlObject xmlObject = XmlParser.Parse(element.ToString());
            _heroesSource.StormStorage.AddStormStyleElement(StormModType, xmlObject.Root, _stormPath);
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
