namespace Heroes.XmlData.StormDepotCache;

internal interface IDepotCacheFactory
{
    IDepotCache CreateCASCDepotCache(ICASCHeroesSource heroesSource);

    IDepotCache CreateFileDepotCache(IFileHeroesSource heroesSource);
}