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

    public override bool FileExists(string path, string? mpqPath = null)
    {
        if (mpqPath is null)
            return _fileSystem.File.Exists(GetValidatedPath(path));
        else
            return IsMpqFileEntryExists(GetValidatedPath(mpqPath), path);
    }

    public override bool FileExists(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return IsMpqFileEntryExists(GetValidatedPath(stormFile.StormPath.StormModPath), stormFile.StormPath.Path);
        else
            return FileExists(stormFile.StormPath.Path);
    }

    public override Stream GetFile(string path, string? mpqPath = null)
    {
        if (mpqPath is null)
        {
            string validatedPath = GetValidatedPath(path);
            if (_fileSystem.File.Exists(validatedPath))
                return _fileSystem.File.OpenRead(validatedPath);
            else
                throw new FileNotFoundException("Could not find file", path);
        }
        else
        {
            return GetMpqFileEntry(mpqPath, path);
        }
    }

    public override Stream GetFile(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return GetMpqFileEntry(stormFile.StormPath.StormModPath, stormFile.StormPath.Path);
        else
            return GetFile(stormFile.StormPath.Path);
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType, BackgroundWorkerEx? backgroundWorkerEx = null) => StormModFactory.CreateFileStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);
}
