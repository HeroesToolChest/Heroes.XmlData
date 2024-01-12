namespace Heroes.XmlData.StormDepotCache;

internal record MapDependency
{
    public required string BnetName { get; init; } = string.Empty;

    public required int BnetVersionMajor { get; init; }

    public required int BnetVersionMinor { get; init; }

    public required int BnetNamespace { get; init; }

    public required string LocalFile { get; init; } = string.Empty;

    public static IEnumerable<MapDependency> GetMapDependencies(IEnumerable<XElement> dependencies, string modsDirectory)
    {
        foreach (XElement valueElement in dependencies)
        {
            yield return GetMapDependency(valueElement, modsDirectory);
        }
    }

    private static MapDependency GetMapDependency(XElement valueElement, string modsDirectory)
    {
        Span<Range> valueParts = stackalloc Range[2];
        Span<Range> bnetParts = stackalloc Range[3];

        ReadOnlySpan<char> value = valueElement.Value;

        value.Split(valueParts, ',');

        // bnet:<file name>/<major>.<minor>/<namespace>
        ReadOnlySpan<char> bnetSpan = value[valueParts[0]];

        // file:<filePath>
        ReadOnlySpan<char> filePathSpan = value[valueParts[1]];
        int indexOfFilePath = filePathSpan.IndexOf(':');

        // split the bnetSpan into parts
        bnetSpan.Split(bnetParts, '/');

        // get the file name part of the bnetParts -> bnet:<file name>
        ReadOnlySpan<char> bnetFileName = bnetSpan[bnetParts[0]];
        int indexOfBnetFileName = bnetFileName.IndexOf(':');

        // get the version part -> <major>.<minor>
        ReadOnlySpan<char> bnetVersion = bnetSpan[bnetParts[1]];
        int indexOfBnetVersion = bnetVersion.IndexOf('.');

        return new()
        {
            BnetName = bnetSpan[bnetParts[0]][(indexOfBnetFileName + 1)..].ToString(),
            BnetVersionMajor = int.Parse(bnetSpan[bnetParts[1]][..indexOfBnetVersion]),
            BnetVersionMinor = int.Parse(bnetSpan[bnetParts[1]][(indexOfBnetVersion + 1)..]),
            BnetNamespace = int.Parse(bnetSpan[bnetParts[2]]),
            LocalFile = PathHelper.NormalizePath(filePathSpan[(indexOfFilePath + 1)..], modsDirectory),
        };
    }
}
