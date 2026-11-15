namespace Heroes.StormData.Paths;

internal sealed class PathNormalizer
{
    /// <summary>
    /// Returns a modified path to use the current platform's directory separator character and lowercase all characters.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    /// <returns>The modified file path.</returns>
    public static string NormalizePath(ReadOnlySpan<char> filePath)
    {
        if (filePath.IsWhiteSpace())
            return string.Empty;

        return string.Create(filePath.Length, filePath, static (buffer, source) =>
        {
            source.CopyTo(buffer);
            NormalizePath(buffer);
        });
    }

    /// <summary>
    /// Returns a modified path to use the current platform's directory separator character and lowercase all characters.
    /// If <paramref name="filePath"/> starts with the <paramref name="modsDirectory"/> directory, that directory is removed so the
    /// returned path is relative to it.
    /// </summary>
    /// <remarks>
    /// The <paramref name="modsDirectory"/> is only matched as whole directory segments at the start of <paramref name="filePath"/>.
    /// Matching is case-insensitive and treats '/' and '\' as equivalent.
    /// </remarks>
    /// <param name="filePath">A file path.</param>
    /// <param name="modsDirectory">The mods directory. May be a single directory name (e.g. "mods") or a full directory path.</param>
    /// <returns>The modified file path.</returns>
    public static string NormalizePath(ReadOnlySpan<char> filePath, string modsDirectory)
    {
        if (filePath.IsWhiteSpace())
            return string.Empty;

        return NormalizePath(filePath[GetDirectoryPrefixLength(filePath, modsDirectory)..]);
    }

    /// <summary>
    /// Modifies the path in place to use the current platform's directory separator character and lowercase all characters.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    private static void NormalizePath(Span<char> filePath)
    {
        for (int i = 0; i < filePath.Length; i++)
        {
            filePath[i] = NormalizeChar(filePath[i]);
        }
    }

    private static char NormalizeChar(char value) => IsSeparator(value) ? Path.DirectorySeparatorChar : char.ToLowerInvariant(value);

    private static bool IsSeparator(char value) => value is '/' or '\\';

    /// <summary>
    /// Gets the number of characters at the start of <paramref name="filePath"/> taken up by <paramref name="directory"/>, including any separators that follow it.
    /// </summary>
    /// <param name="filePath">A file path.</param>
    /// <param name="directory">The directory to match.</param>
    /// <returns>The length of the matched prefix, or 0 if <paramref name="filePath"/> does not start with <paramref name="directory"/>.</returns>
    private static int GetDirectoryPrefixLength(ReadOnlySpan<char> filePath, ReadOnlySpan<char> directory)
    {
        directory = directory.TrimEnd(['/', '\\']);

        if (directory.IsEmpty || filePath.Length < directory.Length)
            return 0;

        for (int i = 0; i < directory.Length; i++)
        {
            if (NormalizeChar(filePath[i]) != NormalizeChar(directory[i]))
                return 0;
        }

        int prefixLength = directory.Length;

        // must end on a directory boundary, e.g. "mods" should not match "modsdata"
        if (prefixLength < filePath.Length && !IsSeparator(filePath[prefixLength]))
            return 0;

        while (prefixLength < filePath.Length && IsSeparator(filePath[prefixLength]))
            prefixLength++;

        return prefixLength;
    }
}
