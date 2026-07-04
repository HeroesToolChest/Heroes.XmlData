using System.IO.Abstractions;

namespace Heroes.XmlData.Source;

internal sealed class FileHeroesSource : HeroesSource, IFileHeroesSource
{
    private readonly IFileSystem _fileSystem;

    public FileHeroesSource(IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IProgressReporter? backgroundWorkerEx)
        : this(new FileSystem(), stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath, backgroundWorkerEx)
    {
    }

    public FileHeroesSource(IFileSystem fileSystem, IStormStorage stormStorage, IStormModFactory stormModFactory, IDepotCacheFactory depotCacheFactory, string modsDirectoryPath, IProgressReporter? backgroundWorkerEx)
        : base(stormStorage, stormModFactory, depotCacheFactory, modsDirectoryPath, backgroundWorkerEx)
    {
        _fileSystem = fileSystem;
    }

    public override bool FileExists(string? path, string? mpqEntryPath = null)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (mpqEntryPath is null)
            return _fileSystem.File.Exists(GetValidatedPath(path));
        else
            return IsMpqFileEntryExists(path, mpqEntryPath);
    }

    public override bool FileExists(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return IsMpqFileEntryExists(stormFile.StormPath.StormModPath, stormFile.StormPath.Path);
        else
            return FileExists(stormFile.StormPath.Path);
    }

    public override Stream GetFile(string path, string? mpqEntryPath = null)
    {
        if (mpqEntryPath is null)
        {
            string validatedPath = GetValidatedPath(path);
            if (_fileSystem.File.Exists(validatedPath))
                return _fileSystem.File.OpenRead(validatedPath);
            else
                throw new FileNotFoundException("Could not find file", path);
        }
        else
        {
            return GetMpqFileEntry(path, mpqEntryPath);
        }
    }

    public override Stream GetFile(StormFile stormFile)
    {
        if (stormFile.StormPath.PathType == StormPathType.MPQ)
            return GetMpqFileEntry(stormFile.StormPath.StormModPath, stormFile.StormPath.Path);
        else
            return GetFile(stormFile.StormPath.Path);
    }

    protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType, IProgressReporter? progressReporter = null) => StormModFactory.CreateFileStormModInstance(this, directoryPath, stormModType);

    protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType) => StormModFactory.CreateFileMpqStormModInstance(this, name, directoryPath, stormModType);

    protected override IDepotCache GetDepotCache() => DepotCacheFactory.CreateFileDepotCache(this);

    protected override string GetValidatedPath(string path)
    {
        if (path.StartsWith(DefaultModsDirectory))
            path = string.Concat(ModsBaseDirectoryPath, path.AsSpan(DefaultModsDirectory.Length));
        else if (!path.StartsWith(ModsBaseDirectoryPath))
            path = Path.Join(ModsBaseDirectoryPath, path);

        return path;
    }

    protected override bool IsMpqFileEntryExists(string mpqPath, string entryPath)
    {
        if (!FileExists(mpqPath))
            return false;

        using MpqHeroesArchive mpqFile = MpqHeroesFile.Open(GetFile(mpqPath));
        return mpqFile.FileEntryExists(entryPath);
    }
}
