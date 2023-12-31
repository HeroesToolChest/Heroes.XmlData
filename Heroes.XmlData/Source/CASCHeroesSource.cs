using Heroes.XmlData.StormMods;

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

    protected override void AddStormMods(IList<IStormMod> stormMods)
    {
        throw new NotImplementedException();
    }

    protected override void AddStormMaps()
    {
        throw new NotImplementedException();
    }
}
