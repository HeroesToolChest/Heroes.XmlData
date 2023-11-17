namespace Heroes.XmlData.Source;

internal interface ICASCHeroesSource : IHeroesSource
{
    CASCHeroesStorage CASCHeroesStorage { get; }
}
