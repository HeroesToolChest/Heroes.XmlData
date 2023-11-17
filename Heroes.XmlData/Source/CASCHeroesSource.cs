using Heroes.XmlData.StormMods;

namespace Heroes.XmlData.Source;

internal class CASCHeroesSource(HeroesData heroesData, CASCHeroesStorage cascHeroesStorage)
    : HeroesSource(heroesData, "mods"), ICASCHeroesSource
{
    public CASCHeroesStorage CASCHeroesStorage => cascHeroesStorage;

    protected override void AddStormMods(IList<IStormMod> stormMods)
    {
        throw new NotImplementedException();
    }
}
