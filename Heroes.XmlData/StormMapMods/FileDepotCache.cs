namespace Heroes.XmlData.StormMapMods;

internal class FileDepotCache : DepotCache<FileHeroesSource>
{
    public FileDepotCache(FileHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    protected override void LoadMapDependencyData()
    {
        if (!Directory.Exists(DepotCacheDirectoryPath))
        {
            HeroesData.AddDirectoryNotFound(DepotCacheDirectoryPath);
            return;
        }

        IEnumerable<string> s2mvFiles = Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2mv", SearchOption.AllDirectories);
        IEnumerable<string> s2maFiles = Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2ma", SearchOption.AllDirectories);

        // find the s2mv files first
        foreach (string s2mvFile in s2mvFiles)
        {
            if (!IsS2mvFile(s2mvFile))
                continue;

            using FileStream fileStream = File.OpenRead(s2mvFile);

            if (LoadS2mvFile(fileStream))
                HeroesSource.S2MVPaths.Add(s2mvFile);
        }

        // then the s2ma files, which are the map stormmod files
        foreach (string s2maFile in s2maFiles)
        {
            if (!IsS2maFile(s2maFile))
                continue;

            using FileStream fileStream = File.OpenRead(s2maFile);

            if (LoadS2maFile(fileStream))
                HeroesSource.S2MAPaths.Add(s2maFile);
        }
    }
}
