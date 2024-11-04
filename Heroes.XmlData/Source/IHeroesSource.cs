namespace Heroes.XmlData.Source;

internal interface IHeroesSource
{
    IStormModFactory StormModFactory { get; }

    IBackgroundWorkerEx? BackgroundWorkerEx { get; }

    /// <summary>
    /// Gets the base directory path of the selected mods folder.
    /// </summary>
    string ModsBaseDirectoryPath { get; }

    /// <summary>
    /// Gets the default directory of the mods folder.
    /// </summary>
    string DefaultModsDirectory { get; }

    string GameDataDirectory { get; }

    string BaseStormDataDirectory { get; }

    string BaseStormAssetsDirectory { get; }

    string LocalizedDataDirectory { get; }

    string AssetsDirectory { get; }

    string GameStringFile { get; }

    string GameDataXmlFile { get; }

    string IncludesXmlFile { get; }

    string DocumentInfoFile { get; }

    string FontStyleFile { get; }

    string BuildIdFile { get; }

    string AssetsTextFile { get; }

    string DescIndexStormLayoutFile { get; }

    string CoreStormModDirectory { get; }

    string HeroesStormModDirectory { get; }

    string HeroesDataStormModDirectory { get; }

    string HeroModsDirectory { get; }

    string UIDirectory { get; }

    string LayoutDirectory { get; }

    string DepotCacheDirectory { get; }

    string BattleMapModsDirectory { get; }

    IStormStorage StormStorage { get; }

    IDepotCache DepotCache { get; }

    Dictionary<int, S2MVProperties> S2MVPropertiesByHashCode { get; }

    List<string> S2MVPaths { get; }

    List<S2MAProperties> S2MAProperties { get; }

    Dictionary<string, S2MAProperties> S2MAPropertiesByTitle { get; }

    List<string> S2MAPaths { get; }

    string GetLocaleStormDataDirectory(StormLocale stormLocale);

    void LoadStormData();

    void LoadGamestrings(StormLocale stormLocale);

    void LoadStormMapData(string mapTitle);

    void LoadCustomMod(IStormMod stormMod);

    void LoadCustomMod(string directoryPath);

    void LoadDepotCache();

    void UnloadCustomMods();

    IEnumerable<StormMapDependency> GetMapDependencies();

    bool IsMapMapLoaded();

    string? LoadedStormMapTitle();

    bool FileExists(string path, string? mpqPath = null);

    bool FileExists(StormFile stormFile);

    Stream GetFile(string path, string? mpqPath = null);

    Stream GetFile(StormFile stormFile);
}
