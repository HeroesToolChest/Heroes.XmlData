using Heroes.XmlData.CASC;

namespace Heroes.XmlData.Source;

internal interface ICASCHeroesSource : IHeroesSource
{
    ICASCHeroesStorage CASCHeroesStorage { get; }
}
