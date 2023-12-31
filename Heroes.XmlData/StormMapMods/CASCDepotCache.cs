using CASCLib;

namespace Heroes.XmlData.StormMapMods;

internal class CASCDepotCache : DepotCache<CASCHeroesSource>
{
    public CASCDepotCache(CASCHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    protected override void LoadMapDependencyData()
    {
        CASCFolder depotCacheFolder = HeroesSource.CASCHeroesStorage.CASCFolderRoot.GetFolder(DepotCacheDirectoryPath);

        if (depotCacheFolder is null)
        {
            HeroesData.AddDirectoryNotFound(DepotCacheDirectoryPath);
            return;
        }

        foreach (KeyValuePair<string, CASCFolder> folder in depotCacheFolder.Folders)
        {
            foreach (KeyValuePair<string, CASCFolder> innerFolder in folder.Value.Folders)
            {
                foreach (KeyValuePair<string, CASCFile> file in innerFolder.Value.Files)
                {
                    if (IsS2mvFile(file.Value.FullName))
                    {
                        // find the s2mv files first
                        if (LoadS2mvFile(HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file.Value.FullName)))
                            HeroesSource.S2MVPaths.Add(file.Value.FullName);
                    }
                    else if (IsS2maFile(file.Value.FullName))
                    {
                        // then the s2ma files, which are the map stormmod files
                        if (LoadS2maFile(HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file.Value.FullName)))
                            HeroesSource.S2MAPaths.Add(file.Value.FullName);
                    }
                }
            }
        }
    }
}
