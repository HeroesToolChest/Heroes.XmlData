namespace Heroes.StormData.DepotCache;

internal sealed class CASCDepotCache : DepotCacheBase<ICASCHeroesSource>
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
            return;

        // find the s2mv files first
        foreach (CASCFile file in GetDepotCacheFiles(depotCacheFolder))
        {
            if (!IsS2MVFile(file.FullName))
                continue;

            using Stream s2mvFile = OpenFile(file.FullName);

            AddS2MVFile(s2mvFile, file.FullName);
        }

        // then the s2ma files, which are the map stormmod files
        foreach (CASCFile file in GetDepotCacheFiles(depotCacheFolder))
        {
            if (!IsS2MAFile(file.FullName))
                continue;

            using Stream s2maFile = OpenFile(file.FullName);

            AddS2maFile(s2maFile, file.FullName);
        }
    }

    // the files are two folders deep, e.g. depotcache/<xx>/<xx>/<hash>.s2ma
    private static IEnumerable<CASCFile> GetDepotCacheFiles(CASCFolder depotCacheFolder)
    {
        foreach (CASCFolder folder in depotCacheFolder.Folders.Values)
        {
            foreach (CASCFolder innerFolder in folder.Folders.Values)
            {
                foreach (CASCFile file in innerFolder.Files.Values)
                {
                    yield return file;
                }
            }
        }
    }

    private Stream OpenFile(string fileName) =>
        HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(fileName) ?? throw new HeroesStormDataException($"Could not open depot cache file: {fileName}");
}
