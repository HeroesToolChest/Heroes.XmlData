namespace Heroes.XmlData.Source;

internal class CASCHeroesSource : HeroesSource, ICASCHeroesSource
{
    private readonly CASCHeroesStorage _cascHeroesStorage;

    public CASCHeroesSource(HeroesData heroesData, CASCHeroesStorage cascHeroesStorage)
        : base(heroesData, "mods")
    {
        _cascHeroesStorage = cascHeroesStorage;
    }

    public CASCHeroesStorage CASCHeroesStorage => _cascHeroesStorage;

    protected override IStormMod GetStormMod(string directoryPath) => CreateStormModInstance<CASCStormMod>(this, directoryPath);

    protected override IStormMod GetMpqStormMod(string directoryPath, string name) => CreateStormModInstance<CASCMpqStormMod>(this, directoryPath, name);

    protected override IDepotCache GetDepotCache() => new CASCDepotCache(this);
}
