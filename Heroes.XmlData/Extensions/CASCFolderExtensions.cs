namespace Heroes.XmlData.Extensions;

internal static class CASCFolderExtensions
{
    public static bool TryGetLastDirectory(this CASCFolder folder, string directoryPath, [NotNullWhen(true)] out CASCFolder? cascFolder)
    {
        cascFolder = null;

        CASCFolder currentFolder = folder;

        foreach (string directory in EnumeratedStringPath(directoryPath))
        {
            CASCFolder? foundFolder = currentFolder.GetFolder(directory);
            if (foundFolder is null)
                return false;

            currentFolder = foundFolder;
        }

        cascFolder = currentFolder;

        return true;
    }

    public static void AddDirectory(this CASCFolder folder, string directoryPath)
    {
        CASCFolder currentFolder = folder;

        string[] directories = EnumeratedStringPath(directoryPath);

        for (int i = 0; i < directories.Length; i++)
        {
            currentFolder.Folders.TryAdd(directories[i], new CASCFolder(directories[i]));
            currentFolder = currentFolder.GetFolder(directories[i]);
        }
    }

    public static void AddFile(this CASCFolder folder, string filePath)
    {
        CASCFolder currentFolder = folder;

        string[] paths = EnumeratedStringPath(filePath);

        for (int i = 0; i < paths.Length; i++)
        {
            if (i == paths.Length - 1)
                currentFolder.Files[paths[i]] = new CASCFile((ulong)Random.Shared.NextInt64(1_000_000), filePath);
            else
                currentFolder.Folders.TryAdd(paths[i], new CASCFolder(paths[i]));

            currentFolder = currentFolder.GetFolder(paths[i]);
        }
    }

    private static string[] EnumeratedStringPath(string directoryPath)
    {
        return directoryPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
    }
}
