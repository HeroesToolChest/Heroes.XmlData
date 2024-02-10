namespace Heroes.XmlData.StormDepotCache;

internal class CASCDepotCache : DepotCache<CASCHeroesSource>
{
    public CASCDepotCache(CASCHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    protected override void FindMapRootData()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(DepotCacheDirectoryPath, out CASCFolder? depotCacheFolder))
        {
            StormStorage.AddDirectoryNotFound(DepotCacheDirectoryPath, Name, string.Empty);
            return;
        }

        // find the s2mv files first
        foreach (KeyValuePair<string, CASCFolder> folder in depotCacheFolder.Folders)
        {
            foreach (KeyValuePair<string, CASCFolder> innerFolder in folder.Value.Folders)
            {
                foreach (KeyValuePair<string, CASCFile> file in innerFolder.Value.Files)
                {
                    if (!IsS2mvFile(file.Value.FullName))
                        continue;

                    if (LoadS2mvFile(HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file.Value.FullName)))
                        HeroesSource.S2MVPaths.Add(file.Value.FullName);
                }
            }
        }

        // then the s2ma files, which are the map stormmod files
        foreach (KeyValuePair<string, CASCFolder> folder in depotCacheFolder.Folders)
        {
            foreach (KeyValuePair<string, CASCFolder> innerFolder in folder.Value.Folders)
            {
                foreach (KeyValuePair<string, CASCFile> file in innerFolder.Value.Files)
                {
                    if (!IsS2maFile(file.Value.FullName))
                        continue;

                    if (LoadS2maFile(HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(file.Value.FullName)))
                    {
                        HeroesSource.S2MAPaths.Add(file.Value.FullName);
                        HeroesSource.S2MAProperties.Last().DirectoryPath = PathHelper.NormalizePath(file.Value.FullName, HeroesSource.DefaultModsDirectory);
                    }
                }
            }
        }
    }
}
