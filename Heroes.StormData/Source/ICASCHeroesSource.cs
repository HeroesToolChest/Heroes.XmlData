namespace Heroes.StormData.Source;

internal interface ICASCHeroesSource : IHeroesSource
{
    ICASCHeroesStorage CASCHeroesStorage { get; }

    CASCFolder GetCASCFolder(string? directory = null);
}
