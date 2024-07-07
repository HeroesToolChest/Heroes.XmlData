using System.IO.Abstractions;

namespace Heroes.XmlData.StormDepotCache;

internal class CASCDepotCache : DepotCache<ICASCHeroesSource>
{
    public CASCDepotCache(ICASCHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    public CASCDepotCache(IFileSystem fileSystem, ICASCHeroesSource heroesSource)
        : base(fileSystem, heroesSource)
    {
    }

    protected override void FindMapRootData()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(DepotCacheDirectoryPath, out CASCFolder? depotCacheFolder))
        {
            StormStorage.AddDirectoryNotFound(StormModType.Normal, new StormPath()
            {
                StormModDirectoryPath = string.Empty,
                StormModName = string.Empty,
                Path = DepotCacheDirectoryPath,
                PathType = StormPathType.CASC,
            });

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

                    if (LoadS2mvFile(HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(file.Value.FullName)))
                    {
                        HeroesSource.S2MVPaths.Add(file.Value.FullName);
                        HeroesSource.S2MVPropertiesByHashCode.Last().Value.DirectoryPath = PathHelper.NormalizePath(file.Value.FullName, HeroesSource.DefaultModsDirectory);
                    }
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

                    if (LoadS2maFile(HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(file.Value.FullName)))
                    {
                        HeroesSource.S2MAPaths.Add(file.Value.FullName);
                        HeroesSource.S2MAProperties.Last().DirectoryPath = PathHelper.NormalizePath(file.Value.FullName, HeroesSource.DefaultModsDirectory);
                    }
                }
            }
        }
    }
}
