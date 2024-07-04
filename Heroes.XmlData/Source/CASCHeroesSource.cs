using Heroes.XmlData.CASC;

namespace Heroes.XmlData.Source;

internal class CASCHeroesSource : HeroesSource, ICASCHeroesSource
{
    private readonly ICASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, ICASCHeroesStorage cascHeroesStorage)
        : base(stormStorage, stormModFactory, depotCacheFactory)
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public ICASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType) => StormModFactory.CreateCASCStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateCASCMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateCASCDepotCache(this);
}
