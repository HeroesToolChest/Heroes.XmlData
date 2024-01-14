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

    protected override IStormMod GetStormMod(string directoryPath) => CreateStormModInstance<CASCStormMod>(this, directoryPath);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath) => CreateStormModInstance<CASCMpqStormMod>(this, name, directoryPath);

    protected override IDepotCache GetDepotCache() => new CASCDepotCache(this);
}
