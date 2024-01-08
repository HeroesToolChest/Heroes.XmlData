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

    private static string[] EnumeratedStringPath(string directoryPath)
    {
        return directoryPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
    }
}
