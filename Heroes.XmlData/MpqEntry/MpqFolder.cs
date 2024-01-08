namespace Heroes.XmlData.MpqEntry;

internal class MpqFolder
{
    public MpqFolder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public Dictionary<string, MpqFile> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, MpqFolder> Folders { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetLastDirectory(string directoryPath, [NotNullWhen(true)] out MpqFolder? mpqFolder)
    {
        mpqFolder = null;

        MpqFolder currentFolder = this;

        foreach (string directory in EnumeratedStringPath(directoryPath))
        {
            MpqFolder? foundFolder = currentFolder.GetFolder(directory);
            if (foundFolder is null)
                return false;

            currentFolder = foundFolder;
        }

        mpqFolder = currentFolder;

        return true;
    }

    public bool TryGetFile(string filePath, [NotNullWhen(true)] out MpqFile? mpqFile)
    {
        mpqFile = null;

        MpqFolder currentFolder = this;

        string[] paths = EnumeratedStringPath(filePath);

        for (int i = 0; i < paths.Length; i++)
        {
            bool isFile = i == paths.Length - 1;

            string pathPart = paths[i];

            if (isFile)
            {
                if (Files.TryGetValue(pathPart, out MpqFile? value))
                {
                    mpqFile = value;
                    return true;
                }
            }
            else
            {
                MpqFolder? foundFolder = currentFolder.GetFolder(pathPart);
                if (foundFolder is null)
                    return false;

                currentFolder = foundFolder;
            }
        }

        return false;
    }

    public MpqFolder? GetFolder(string name)
    {
        Folders.TryGetValue(name, out MpqFolder? folder);
        return folder;
    }

    private static string[] EnumeratedStringPath(string path)
    {
        return path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
    }
}
