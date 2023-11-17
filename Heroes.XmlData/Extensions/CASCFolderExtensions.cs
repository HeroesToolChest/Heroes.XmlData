using CASCLib;

namespace Heroes.XmlData.Extensions;

public static class CASCFolderExtensions
{
    //public static CASCFolder GetDirectory(this CASCFolder cascFolder, string folderPath)
    //{
    //    CASCFolder currentFolder = cascFolder;

    //    foreach (string directory in EnumeratedStringPath(folderPath))
    //    {
    //        currentFolder.
    //        currentFolder = (CASCFolder)currentFolder.GetEntry(directory);
    //    }

    //    return currentFolder;
    //}

    //public static bool DirectoryExists(this CASCFolder cascFolder, string folderPath)
    //{
    //    CASCFolder currentFolder = cascFolder;

    //    foreach (string directory in EnumeratedStringPath(folderPath))
    //    {
    //        if ((CASCFolder)currentFolder.GetEntry(directory) == null)
    //            return false;
    //    }

    //    return true;
    //}

    private static string[] EnumeratedStringPath(string filePath)
    {
        return filePath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
    }
}
