namespace Heroes.XmlData.Source;

internal class CASCHeroesSource : HeroesSource, ICASCHeroesSource
{
    private readonly CASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, CASCHeroesStorage cascHeroesStorage)
        : base(stormStorage, stormModFactory, depotCacheFactory)
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public CASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    protected override IStormMod GetStormMod(string directoryPath, bool isMapMod) => StormModFactory.CreateCASCStormModInstance(this, directoryPath, isMapMod);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, bool isMapMod) => StormModFactory.CreateCASCMpqStormModInstance(this, name, directoryPath, isMapMod);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateCASCDepotCache(this);
}
