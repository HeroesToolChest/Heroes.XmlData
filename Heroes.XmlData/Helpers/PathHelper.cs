using System.Diagnostics.CodeAnalysis;

namespace Heroes.XmlData.Helpers;

internal class PathHelper
{
    /// <summary>
    /// Returns a modified path using the current platform's directory separator character. If <paramref name="filePath"/> is <see langword="null"/>, returns <see langword="null"/>.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    /// <returns>A modified path.</returns>
    [return: NotNullIfNotNull(nameof(filePath))]
    public static string? GetFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return filePath;
        }

        if (Path.DirectorySeparatorChar != '\\')
        {
            filePath = filePath.Replace('\\', Path.DirectorySeparatorChar);
        }
        else if (Path.DirectorySeparatorChar == '\\')
        {
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
        }

        return filePath;
    }
}
