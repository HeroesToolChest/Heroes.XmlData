namespace Heroes.XmlData.StormDepotCache;

internal class DepotCacheFactory : IDepotCacheFactory
{
    public IDepotCache CreateFileDepotCache(IFileHeroesSource heroesSource)
    {
        return new FileDepotCache(heroesSource);
    }

    public IDepotCache CreateCASCDepotCache(ICASCHeroesSource heroesSource)
    {
        return new CASCDepotCache(heroesSource);
    }
}
