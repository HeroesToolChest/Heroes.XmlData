using System.IO;

namespace Heroes.XmlData.StormMods;

internal class CustomStormMod : IStormMod
{
    private const string _customPath = "custom";
    private readonly IHeroesSource _heroesSource;
    private readonly ManualModLoader _manualModLoader;

    public CustomStormMod(IHeroesSource heroesSource, ManualModLoader manualModLoader)
    {
        Name = manualModLoader.Name;
        _heroesSource = heroesSource;
        _manualModLoader = manualModLoader;

        StormModStorage = heroesSource.StormStorage.CreateModStorage(this, _heroesSource.ModsBaseDirectoryPath);
    }

    public string Name { get; }

    public string DirectoryPath => string.Empty;

    public StormModType StormModType => StormModType.Custom;

    public StormModStorage StormModStorage { get; }

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
        throw new NotImplementedException();
    }

    public void LoadStormGameStrings(StormLocale stormLocale)
    {
        if (_manualModLoader.GameStringsByLocale.TryGetValue(stormLocale, out List<string>? gameStrings))
        {
            foreach (string gameString in gameStrings)
            {
                var gamestring = _heroesSource.StormStorage.GetGameStringWithId(gameString, _customPath);

                if (gamestring is not null)
                    StormModStorage.AddGameString(gamestring.Value.Id, gamestring.Value.GameStringText);
            }
        }
    }
}
