namespace Heroes.XmlData.Source;

internal interface IHeroesSource
{
    int HotsBuild { get; }

    string ModsDirectoryPath { get; }

    string DefaultModsDirectory { get; }

    string GameDataDirectory { get; }

    string BaseStormDataDirectory { get; }

    string LocalizedDataDirectory { get; }

    string GameStringFile { get; }

    string GameDataXmlFile { get; }

    string IncludesXmlFile { get; }

    string DocumentInfoFile { get; }

    string CoreStormModDirectory { get; }

    string HeroesStormModDirectory { get; }

    string HeroesDataStormModDirectory { get; }

    string HeroModsDirectory { get; }

    string DepotCacheDirectory { get; }

    string BattleMapModsDirectory { get; }

    IHeroesData HeroesData { get; }

    IDepotCache DepotCache { get; }

    Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; }

    List<string> S2MVPaths { get; }

    List<S2MAProperties> S2MAProperties { get; }

    Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; }

    List<string> S2MAPaths { get; }

    /// <summary>
    /// Creates an instance of an <see cref="IStormMod"/>.
    /// </summary>
    /// <typeparam name="T">A type of <see cref="IStormMod"/>.</typeparam>
    /// <param name="args">Additional arguments to be passed in a paramaters to the constructor.</param>
    /// <returns>An instance of a <see cref="IStormMod"/>.</returns>
    IStormMod CreateStormModInstance<T>(params object?[]? args)
        where T : IStormMod;

    void LoadStormData();

    void LoadGamestrings(HeroesLocalization localization);

    bool LoadStormMapData(string mapTitle);
}
