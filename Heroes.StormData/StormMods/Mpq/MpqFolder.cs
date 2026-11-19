namespace Heroes.StormData.StormMods.Mpq;

internal sealed class MpqFolder
{
    private static readonly char[] _pathSeparators = ['/', '\\'];

    private readonly Dictionary<string, MpqFile> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MpqFolder> _folders = new(StringComparer.OrdinalIgnoreCase);

    public MpqFolder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyDictionary<string, MpqFile> Files => _files;

    public IReadOnlyDictionary<string, MpqFolder> Folders => _folders;

    /// <summary>
    /// Adds a file to the tree, creating any folders in its path that do not exist yet. An existing file with the same path is kept.
    /// </summary>
    /// <param name="filePath">The file path relative to this folder. Both '/' and '\' are treated as separators.</param>
    public void AddFile(string filePath)
    {
        string[] parts = SplitPath(filePath);
        if (parts.Length == 0)
            return;

        MpqFolder folder = this;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!folder._folders.TryGetValue(parts[i], out MpqFolder? childFolder))
            {
                childFolder = new MpqFolder(parts[i]);
                folder._folders.Add(parts[i], childFolder);
            }

            folder = childFolder;
        }

        folder._files.TryAdd(parts[^1], new MpqFile(parts[^1], filePath));
    }

    /// <summary>
    /// Gets the folder at the end of <paramref name="folderPath"/>. An empty path returns this folder.
    /// </summary>
    /// <param name="folderPath">The folder path relative to this folder. Both '/' and '\' are treated as separators.</param>
    /// <param name="folder">When this method returns, contains the found folder, or <see langword="null"/> if it was not found.</param>
    /// <returns><see langword="true"/> if the folder was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetFolder(string folderPath, [NotNullWhen(true)] out MpqFolder? folder)
    {
        folder = this;

        foreach (string part in SplitPath(folderPath))
        {
            if (!folder._folders.TryGetValue(part, out folder))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The file path relative to this folder. Both '/' and '\' are treated as separators.</param>
    /// <param name="file">When this method returns, contains the found file, or <see langword="null"/> if it was not found.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetFile(string filePath, [NotNullWhen(true)] out MpqFile? file)
    {
        file = null;

        string[] parts = SplitPath(filePath);
        if (parts.Length == 0)
            return false;

        MpqFolder? folder = this;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!folder._folders.TryGetValue(parts[i], out folder))
                return false;
        }

        return folder._files.TryGetValue(parts[^1], out file);
    }

    /// <summary>
    /// Enumerates all files in this folder and all of its subfolders.
    /// </summary>
    /// <returns>A collection of <see cref="MpqFile"/>.</returns>
    public IEnumerable<MpqFile> EnumerateFiles()
    {
        foreach (MpqFile file in _files.Values)
        {
            yield return file;
        }

        foreach (MpqFolder folder in _folders.Values)
        {
            foreach (MpqFile file in folder.EnumerateFiles())
            {
                yield return file;
            }
        }
    }

    private static string[] SplitPath(string path) => path.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);
}
