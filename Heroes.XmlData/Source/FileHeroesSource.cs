using System.IO.Abstractions;

namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource, IFileHeroesSource
{
    private readonly IFileSystem _fileSystem;

    public FileHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IBackgroundWorkerEx? backgroundWorkerEx)
        : this(new FileSystem(), stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath, backgroundWorkerEx)
    {
    }

    public FileHeroesSource(IFileSystem fileSystem, IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IBackgroundWorkerEx? backgroundWorkerEx)
        : base(stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath, backgroundWorkerEx)
    {
        _fileSystem = fileSystem;
    }

    public override bool FileExists(string path) => _fileSystem.File.Exists(GetValidatedPath(path));

    public override Stream? GetFile(string path)
    {
        path = GetValidatedPath(path);

        if (_fileSystem.File.Exists(path))
            return _fileSystem.File.OpenRead(path);
        else
            return null;
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType, BackgroundWorkerEx? backgroundWorkerEx = null) => StormModFactory.CreateFileStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);
}
