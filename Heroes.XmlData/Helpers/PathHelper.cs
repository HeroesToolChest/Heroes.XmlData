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

    /// <summary>
    /// Modifies the path to use the current platform's directory separator character and lowecases all characters.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    public static void NormalizePath(Span<char> filePath)
    {
        if (filePath.IsEmpty)
            return;

        bool platformIsBackslash = Path.DirectorySeparatorChar == '\\';

        for (var i = 0; i < filePath.Length; i++)
        {
            if (platformIsBackslash && filePath[i] == '/')
                filePath[i] = Path.DirectorySeparatorChar;
            else if (!platformIsBackslash && filePath[i] == '\\')
                filePath[i] = Path.DirectorySeparatorChar;
            else
                filePath[i] = char.ToLowerInvariant(filePath[i]);
        }
    }

    /// <summary>
    /// Returns a modified path to use the current platform's directory separator character and lowecases all characters. Will remove the 'mods' part as well.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    /// <param name="modsDirectory">The name of the mods directory</param>
    /// <returns>A file path</returns>
    public static string NormalizePath(ReadOnlySpan<char> filePath, string modsDirectory)
    {
        if (filePath.IsEmpty || filePath.IsWhiteSpace())
            return string.Empty;

        Span<char> buffer = stackalloc char[filePath.Length];
        filePath.CopyTo(buffer);

        NormalizePath(buffer);

        int indexOfMods = filePath.IndexOf(modsDirectory, StringComparison.OrdinalIgnoreCase);

        if (indexOfMods < 0)
            return buffer.ToString();

        // removing the "mods" part of the path
        return buffer[(indexOfMods + modsDirectory.Length)..].ToString();
    }
}
