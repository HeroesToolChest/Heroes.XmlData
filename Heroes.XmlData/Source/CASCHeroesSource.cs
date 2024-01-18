namespace Heroes.XmlData.Source;

internal class CASCHeroesSource : HeroesSource, ICASCHeroesSource
{
    private readonly CASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(IStormStorage stormStorage, CASCHeroesStorage cascHeroesStorage)
        : base(stormStorage, "mods")
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public CASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    protected override IStormMod GetStormMod(string directoryPath, bool isMapMod) => CreateStormModInstance<CASCStormMod>(this, directoryPath, isMapMod);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, bool isMapMod) => CreateStormModInstance<CASCMpqStormMod>(this, name, directoryPath, isMapMod);

    protected override IDepotCache GetDepotCache() => new CASCDepotCache(this);
}
