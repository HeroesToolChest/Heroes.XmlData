namespace Heroes.StormData.DepotCache.Parsing;

/// <summary>
/// Parses the <c>.s2mv</c> files in the depot cache.
/// </summary>
internal static class S2MVFileParser
{
    /// <summary>
    /// Parses a <c>.s2mv</c> file. Only files that are a map root file (json with a <c>MapInfo</c> property) are parsed.
    /// </summary>
    /// <param name="s2mvFile">The stream of the file. Must be seekable.</param>
    /// <param name="s2mvProperties">The parsed <see cref="S2MVProperties"/>, or <see langword="null"/> if the file is not a map root file.</param>
    /// <returns><see langword="true"/> if the file was parsed; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(Stream s2mvFile, [NotNullWhen(true)] out S2MVProperties? s2mvProperties)
    {
        s2mvProperties = null;

        // a map's s2mv file is json, most of the others are images
        if (s2mvFile.Length <= 0 || s2mvFile.ReadByte() != '{')
            return false;

        s2mvFile.Position = 0;

        int length = (int)s2mvFile.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            s2mvFile.ReadExactly(buffer, 0, length);

            ReadOnlyMemory<byte> json = buffer.AsMemory(0, length);

            // json files that are not maps don't have a MapInfo property
            if (json.Span.IndexOf("\"MapInfo\""u8) < 0)
                return false;

            s2mvProperties = ParseJson(json);

            return s2mvProperties is not null;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static S2MVProperties? ParseJson(ReadOnlyMemory<byte> json)
    {
        using JsonDocument? jsonDocument = TryParseJson(json);
        if (jsonDocument is null)
            return null;

        JsonElement rootElement = jsonDocument.RootElement;

        if (!rootElement.TryGetProperty("MapInfo", out JsonElement mapInfoElement) ||
            !mapInfoElement.TryGetProperty("Properties", out JsonElement propertiesElement) ||
            !propertiesElement.TryGetProperty("Loading", out JsonElement loadingElement) ||
            !loadingElement.TryGetProperty("MapLink", out JsonElement mapLinkElement))
        {
            return null;
        }

        if (!rootElement.TryGetProperty("DocInfo", out JsonElement docInfoElement) ||
            !docInfoElement.TryGetProperty("Info", out JsonElement infoElement) ||
            !docInfoElement.TryGetProperty("Dependencies", out JsonElement dependenciesElement) ||
            !docInfoElement.TryGetProperty("ModifiableDependencies", out JsonElement modifiableDependenciesElement))
        {
            return null;
        }

        if (!rootElement.TryGetProperty("UsedStrings", out JsonElement usedStringsElement) ||
            !usedStringsElement.TryGetProperty("DocInfo/Name", out JsonElement docInfoNameElement))
        {
            return null;
        }

        JsonElement mapInfoSizeElement = propertiesElement.GetProperty("Size");

        S2MVProperties s2mvProperties = new()
        {
            MapLink = mapLinkElement.GetString() ?? string.Empty,
            HeaderTitle = docInfoNameElement.GetProperty("enUS").GetString() ?? string.Empty,
            LoadingImage = PathNormalizer.NormalizePath(loadingElement.GetProperty("Image").GetString()),
            MapSize = new MapSize(mapInfoSizeElement.GetProperty("x").GetDouble(), mapInfoSizeElement.GetProperty("y").GetDouble()),
            PreviewLargeImage = PathNormalizer.NormalizePath(propertiesElement.GetProperty("PreviewLargeImage").GetString()),
            CustomLayout = PathNormalizer.NormalizePath(loadingElement.GetProperty("CustomLayout").GetString()),
            CustomFrame = PathNormalizer.NormalizePath(loadingElement.GetProperty("CustomFrame").GetString()),
            DocInfoIconFile = PathNormalizer.NormalizePath(infoElement.GetProperty("IconFile").GetString()),
        };

        foreach (JsonProperty docInfoNameProperty in docInfoNameElement.EnumerateObject())
        {
            if (Enum.TryParse(docInfoNameProperty.Name, true, out StormLocale stormLocale))
                s2mvProperties.NameByStormLocale.TryAdd(stormLocale, docInfoNameProperty.Value.GetString() ?? string.Empty);
        }

        foreach (JsonElement dependencyElement in dependenciesElement.EnumerateArray())
        {
            s2mvProperties.MapDependencies.Add(new MapDependency()
            {
                BnetName = dependencyElement.GetProperty("BnetName").GetString() ?? string.Empty,
                BnetVersionMajor = dependencyElement.GetProperty("BnetVersionMajor").GetInt32(),
                BnetVersionMinor = dependencyElement.GetProperty("BnetVersionMinor").GetInt32(),
                BnetNamespace = dependencyElement.GetProperty("BnetNamespace").GetInt32(),
                LocalFile = PathNormalizer.NormalizePath(dependencyElement.GetProperty("LocalFile").GetString(), SourceConstants.DefaultModsDirectory),
            });
        }

        foreach (JsonElement dependencyElement in modifiableDependenciesElement.EnumerateArray())
        {
            s2mvProperties.ModifiableDependencies.Add(PathNormalizer.NormalizePath(dependencyElement.GetString(), SourceConstants.DefaultModsDirectory));
        }

        return s2mvProperties;
    }

    private static JsonDocument? TryParseJson(ReadOnlyMemory<byte> json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
