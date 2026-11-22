namespace Heroes.StormData.StormMods;

internal sealed class CASCStormMod : StormModBase<ICASCHeroesSource>
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
        : base(cascHeroesSource, directoryPath, stormModType)
    {
    }

    protected override void LoadGameDataDirectory()
    {
        if (!TryGetCASCFolder(GameDataDirectoryPath, out CASCFolder? gameDataFolder))
            return;

        // only the files in the directory, not the subdirectories
        IEnumerable<string> files = gameDataFolder.Files.Keys
            .Where(x => HasExtension(x, XmlFileExtension))
            .Select(x => Path.Join(GameDataDirectoryPath, x))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        LoadGameDataFiles(files);
    }

    protected override void LoadStormLayoutDirectory()
    {
        if (!TryGetCASCFolder(LayoutDirectoryPath, out CASCFolder? layoutFolder))
            return;

        LoadStormLayoutFiles(GetFilePaths(layoutFolder, LayoutDirectoryPath, StormLayoutFileExtension));
    }

    protected override void LoadAssetsDirectory()
    {
        if (!TryGetCASCFolder(AssetsDirectoryPath, out CASCFolder? assetsFolder))
            return;

        LoadAssetFiles(GetFilePaths(assetsFolder, AssetsDirectoryPath, DDSFileExtension));
    }

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        ICASCHandlerWrapper cascHandlerWrapper = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper;

        // check first, otherwise OpenFile throws a FileNotFoundException for a missing file (CASCConfig.ThrowOnFileNotFound)
        if (!cascHandlerWrapper.FileExists(path))
            return false;

        stream = cascHandlerWrapper.OpenFile(path);

        return stream is not null;
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormMod(HeroesSource, path, stormModType);

    private static bool HasExtension(string fileName, string extension) => Path.GetExtension(fileName.AsSpan()).Equals(extension, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the full paths of the files with an extension in a folder, including all subfolders.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="folderPath">The full path of <paramref name="folder"/>.</param>
    /// <param name="extension">The file extension to match.</param>
    /// <returns>The full paths, in order.</returns>
    private static IEnumerable<string> GetFilePaths(CASCFolder folder, string folderPath, string extension) =>
        EnumerateFilePaths(folder, folderPath)
            .Where(x => HasExtension(x, extension))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

    // the paths are built from folderPath instead of using CASCFile.FullName, so they always start with the DirectoryPath and use the platform's separator
    private static IEnumerable<string> EnumerateFilePaths(CASCFolder folder, string folderPath)
    {
        foreach (string fileName in folder.Files.Keys)
        {
            yield return Path.Join(folderPath, fileName);
        }

        foreach (KeyValuePair<string, CASCFolder> subfolder in folder.Folders)
        {
            foreach (string filePath in EnumerateFilePaths(subfolder.Value, Path.Join(folderPath, subfolder.Key)))
            {
                yield return filePath;
            }
        }
    }

    private bool TryGetCASCFolder(string directoryPath, [NotNullWhen(true)] out CASCFolder? cascFolder) =>
        HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(directoryPath, out cascFolder);
}
