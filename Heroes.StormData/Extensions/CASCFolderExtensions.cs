namespace Heroes.StormData.Extensions;

internal static class CASCFolderExtensions
{
    // the same separators CASCLib splits on when it builds its folder tree
    private static readonly char[] _pathSeparators = ['/', '\\'];

    /// <summary>
    /// Gets the last folder of <paramref name="directoryPath"/>. The lookup is case-insensitive.
    /// </summary>
    /// <param name="folder">The folder to start the lookup from.</param>
    /// <param name="directoryPath">The directory path, relative to <paramref name="folder"/>. Either '/' or '\' may be used as the separator.</param>
    /// <param name="cascFolder">When this method returns, contains the last folder of <paramref name="directoryPath"/>, if found; otherwise, <see langword="null"/>.
    /// If <paramref name="directoryPath"/> has no directories, contains <paramref name="folder"/>.</param>
    /// <returns><see langword="true"/> if every directory of <paramref name="directoryPath"/> was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetLastDirectory(this CASCFolder folder, string directoryPath, [NotNullWhen(true)] out CASCFolder? cascFolder)
    {
        cascFolder = null;

        CASCFolder currentFolder = folder;

        foreach (string directory in SplitPath(directoryPath))
        {
            CASCFolder? foundFolder = currentFolder.GetFolder(directory);
            if (foundFolder is null)
                return false;

            currentFolder = foundFolder;
        }

        cascFolder = currentFolder;

        return true;
    }

    /// <summary>
    /// Adds the directories of <paramref name="directoryPath"/> that do not already exist.
    /// </summary>
    /// <param name="folder">The folder to add the directories to.</param>
    /// <param name="directoryPath">The directory path, relative to <paramref name="folder"/>. Either '/' or '\' may be used as the separator.</param>
    public static void AddDirectory(this CASCFolder folder, string directoryPath)
    {
        CASCFolder currentFolder = folder;

        foreach (string directory in SplitPath(directoryPath))
        {
            currentFolder = GetOrAddFolder(currentFolder, directory);
        }
    }

    /// <summary>
    /// Adds a file, and any of its directories that do not already exist. An existing file with the same name is replaced.
    /// </summary>
    /// <param name="folder">The folder to add the file to.</param>
    /// <param name="filePath">The file path, relative to <paramref name="folder"/>. Either '/' or '\' may be used as the separator.</param>
    public static void AddFile(this CASCFolder folder, string filePath)
    {
        string[] paths = SplitPath(filePath);
        if (paths.Length == 0)
            return;

        CASCFolder currentFolder = folder;

        for (int i = 0; i < paths.Length - 1; i++)
        {
            currentFolder = GetOrAddFolder(currentFolder, paths[i]);
        }

        currentFolder.Files[paths[^1]] = new CASCFile(ComputeHash(filePath), filePath);
    }

    private static CASCFolder GetOrAddFolder(CASCFolder folder, string name)
    {
        CASCFolder? foundFolder = folder.GetFolder(name);
        if (foundFolder is not null)
            return foundFolder;

        CASCFolder newFolder = new(name);
        folder.Folders[name] = newFolder;

        return newFolder;
    }

    // the same hash CASC uses for a file name; Jenkins96 keeps state, so it is not shared
    private static ulong ComputeHash(string filePath)
    {
        using Jenkins96 hasher = new();

        return hasher.ComputeHash(filePath);
    }

    private static string[] SplitPath(string path) => path.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);
}
