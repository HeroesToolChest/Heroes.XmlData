namespace Heroes.StormData.DepotCache;

internal sealed class FileDepotCache : DepotCacheBase<IFileHeroesSource>
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
            return;

        // sorted, so the first of any duplicates (which is the one kept) is the same on every platform
        IEnumerable<string> s2mvFilePaths = FileSystem.Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2mv", SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase);
        IEnumerable<string> s2maFilePaths = FileSystem.Directory.EnumerateFiles(DepotCacheDirectoryPath, "*.s2ma", SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase);

        // find the s2mv files first
        foreach (string s2mvFilePath in s2mvFilePaths)
        {
            using FileSystemStream fileStream = FileSystem.File.OpenRead(s2mvFilePath);

            AddS2MVFile(fileStream, s2mvFilePath);
        }

        // then the s2ma files, which are the map stormmod files
        foreach (string s2maFilePath in s2maFilePaths)
        {
            using FileSystemStream fileStream = FileSystem.File.OpenRead(s2maFilePath);

            AddS2maFile(fileStream, s2maFilePath);
        }
    }
}
