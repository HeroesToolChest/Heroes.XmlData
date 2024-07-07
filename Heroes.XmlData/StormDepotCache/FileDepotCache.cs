using System.IO.Abstractions;

namespace Heroes.XmlData.StormDepotCache;

internal class FileDepotCache : DepotCache<IFileHeroesSource>
{
    public FileDepotCache(IFileHeroesSource heroesSource)
        : base(heroesSource)
    {
    }

    public FileDepotCache(IFileSystem fileSystem, IFileHeroesSource heroesSource)
        : base(fileSystem, heroesSource)
    {
    }

    protected override void FindMapRootData()
    {
        if (!FileSystem.Directory.Exists(DepotCacheDirectoryPath))
        {
            StormStorage.AddDirectoryNotFound(StormModType.Normal, new StormPath()
            {
                StormModDirectoryPath = string.Empty,
                StormModName = string.Empty,
                Path = DepotCacheDirectoryPath,
                PathType = StormPathType.File,
            });

            return;
        }

        IEnumerable<string> s2mvFiles = FileSystem.Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2mv", SearchOption.AllDirectories);
        IEnumerable<string> s2maFiles = FileSystem.Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2ma", SearchOption.AllDirectories);

        // find the s2mv files first
        foreach (string s2mvFile in s2mvFiles)
        {
            if (!IsS2mvFile(s2mvFile))
                continue;

            using FileSystemStream fileStream = FileSystem.File.OpenRead(s2mvFile);

            if (LoadS2mvFile(fileStream))
            {
                HeroesSource.S2MVPaths.Add(s2mvFile);
                HeroesSource.S2MVPropertiesByHashCode.Last().Value.DirectoryPath = PathHelper.NormalizePath(s2mvFile, HeroesSource.ModsBaseDirectoryPath);
            }
        }

        // then the s2ma files, which are the map stormmod files
        foreach (string s2maFile in s2maFiles)
        {
            if (!IsS2maFile(s2maFile))
                continue;

            using FileSystemStream fileStream = FileSystem.File.OpenRead(s2maFile);

            if (LoadS2maFile(fileStream))
            {
                HeroesSource.S2MAPaths.Add(s2maFile);
                HeroesSource.S2MAProperties.Last().DirectoryPath = PathHelper.NormalizePath(s2maFile, HeroesSource.ModsBaseDirectoryPath);
            }
        }
    }
}
