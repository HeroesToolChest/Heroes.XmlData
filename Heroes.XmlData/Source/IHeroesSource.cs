namespace Heroes.XmlData.Source;

internal interface IHeroesSource
{
    IStormModFactory StormModFactory { get; }

    string ModsDirectoryPath { get; }

    string DefaultModsDirectory { get; }

    string GameDataDirectory { get; }

    string BaseStormDataDirectory { get; }

    string LocalizedDataDirectory { get; }

    string GameStringFile { get; }

    string GameDataXmlFile { get; }

    string IncludesXmlFile { get; }

    string DocumentInfoFile { get; }

    string FontStyleFile { get; }

    string BuildIdFile { get; }

    string CoreStormModDirectory { get; }

    string HeroesStormModDirectory { get; }

    string HeroesDataStormModDirectory { get; }

    string HeroModsDirectory { get; }

    string UIDirectory { get; }

    string DepotCacheDirectory { get; }

    string BattleMapModsDirectory { get; }

    IStormStorage StormStorage { get; }

    IDepotCache DepotCache { get; }

    Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; }

    List<string> S2MVPaths { get; }

    List<S2MAProperties> S2MAProperties { get; }

    Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; }

    List<string> S2MAPaths { get; }

    void LoadStormData();

    void LoadGamestrings(StormLocale stormLocale);

    void LoadStormMapData(string mapTitle);

    void LoadDepotCache();
}
