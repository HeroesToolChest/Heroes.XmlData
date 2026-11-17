namespace Heroes.StormData.DepotCache;

internal abstract class DepotCacheBase<T> : IDepotCache
    where T : IHeroesSource
{
    private const string BnetPrefix = "bnet:";
    private const string FilePrefix = "file:";

    private static readonly XmlReaderSettings _xmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        CloseInput = true,
    };

    public DepotCacheBase(T heroesSource)
        : this(new FileSystem(), heroesSource)
    {
    }

    public DepotCacheBase(IFileSystem fileSystem, T heroesSource)
    {
        FileSystem = fileSystem;
        HeroesSource = heroesSource;

        DepotCacheDirectoryPath = Path.Combine(HeroesSource.ModsDirectoryPath, SourceConstants.CoreStormModDirectory, SourceConstants.BaseStormDataDirectory, SourceConstants.DepotCacheDirectory);
    }

    /// <inheritdoc/>
    public DepotCacheData Data { get; } = new();

    protected string DepotCacheDirectoryPath { get; }

    protected IFileSystem FileSystem { get; }

    protected T HeroesSource { get; }

    /// <inheritdoc/>
    public void LoadDepotCache()
    {
        Data.Clear();

        FindMapRootData();
    }

    protected static bool IsS2MVFile(string filePath) => Path.GetExtension(filePath).Equals(".s2mv", StringComparison.OrdinalIgnoreCase);

    protected static bool IsS2MAFile(string filePath) => Path.GetExtension(filePath).Equals(".s2ma", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Finds the <c>.s2mv</c> and <c>.s2ma</c> files. All the <c>.s2mv</c> files must be added (<see cref="AddS2MVFile"/>) before the <c>.s2ma</c> files (<see cref="AddS2maFile"/>).
    /// </summary>
    protected abstract void FindMapRootData();

    /// <summary>
    /// Loads a <c>.s2mv</c> file and adds it to <see cref="Data"/> if it is a map root file.
    /// </summary>
    /// <param name="s2mvFile">The stream of the file.</param>
    /// <param name="filePath">The path of the file.</param>
    protected void AddS2MVFile(Stream s2mvFile, string filePath)
    {
        S2MVProperties? s2mvProperties = LoadS2MVFile(s2mvFile);
        if (s2mvProperties is null)
            return;

        s2mvProperties.DirectoryPath = PathNormalizer.NormalizePath(filePath, HeroesSource.ModsDirectoryPath);

        Data.AddS2MV(filePath, s2mvProperties);
    }

    /// <summary>
    /// Loads a <c>.s2ma</c> file and adds it to <see cref="Data"/> if it is a map stormmod file.
    /// </summary>
    /// <param name="s2maFile">The stream of the file.</param>
    /// <param name="filePath">The path of the file.</param>
    protected void AddS2maFile(Stream s2maFile, string filePath)
    {
        S2MAProperties? s2maProperties = LoadS2MAFile(s2maFile);
        if (s2maProperties is null)
            return;

        s2maProperties.DirectoryPath = PathNormalizer.NormalizePath(filePath, HeroesSource.ModsDirectoryPath);

        Data.AddS2MA(filePath, s2maProperties);
    }

    private static S2MVProperties? LoadS2MVFile(Stream s2mvFile)
    {
        if (s2mvFile.Length <= 0 || s2mvFile.ReadByte() != '{')
            return null;

        s2mvFile.Position = 0;

        using JsonDocument? jsonDocument = TryParseJson(s2mvFile);
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

    private static JsonDocument? TryParseJson(Stream stream)
    {
        try
        {
            return JsonDocument.Parse(stream);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static S2MAProperties? LoadS2MAFile(Stream s2maFile)
    {
        if (s2maFile.Length < 3 ||
            s2maFile.ReadByte() != 'M' ||
            s2maFile.ReadByte() != 'P' ||
            s2maFile.ReadByte() != 'Q')
        {
            return null;
        }

        s2maFile.Position = 0;

        using MpqHeroesArchive mpqHeroesArchive = MpqHeroesFile.Open(s2maFile);

        if (!mpqHeroesArchive.FileEntryExists("BankList.xml") || !mpqHeroesArchive.TryGetEntry("mapscript.galaxy", out MpqHeroesArchiveEntry? mapScriptEntry) || !mpqHeroesArchive.TryGetEntry(SourceConstants.DocumentInfoFile, out MpqHeroesArchiveEntry? documentInfoEntry))
            return null;

        XElement? documentInfoRoot;

        using (Stream documentInfoStream = mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value))
        {
            documentInfoRoot = TryLoadXml(documentInfoStream)?.Root;
        }

        if (documentInfoRoot is null)
            return null;

        List<MapDependency> mapDependencies = [];

        foreach (XElement dependencyElement in documentInfoRoot.Element("Dependencies")?.Elements() ?? [])
        {
            // a dependency that can't be parsed would change the dependency key, so the map would not match its s2mv file
            if (!TryParseMapDependency(dependencyElement.Value, SourceConstants.DefaultModsDirectory, out MapDependency? mapDependency))
                return null;

            mapDependencies.Add(mapDependency);
        }

        IEnumerable<XElement> modifiableDependencyElements = documentInfoRoot.Element("ModifiableDependencies")?.Elements() ?? [];

        return new S2MAProperties()
        {
            DocInfoIconFile = PathNormalizer.NormalizePath(documentInfoRoot.Element("Icon")?.Value),
            MapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value),
            MapDependencies = mapDependencies,
            ModifiableDependencies = [.. modifiableDependencyElements.Select(x => PathNormalizer.NormalizePath(x.Value, SourceConstants.DefaultModsDirectory))],
        };
    }

    /// <summary>
    /// Parses a dependency value in the format <c>bnet:&lt;name&gt;/&lt;major&gt;.&lt;minor&gt;/&lt;namespace&gt;,file:&lt;path&gt;</c>.
    /// </summary>
    /// <param name="value">The dependency value.</param>
    /// <param name="modsDirectory">The mods directory, which is removed from the start of the file path.</param>
    /// <param name="mapDependency">The parsed <see cref="MapDependency"/>, or <see langword="null"/> if <paramref name="value"/> is not in the expected format.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was parsed; otherwise <see langword="false"/>.</returns>
    private static bool TryParseMapDependency(ReadOnlySpan<char> value, string modsDirectory, [NotNullWhen(true)] out MapDependency? mapDependency)
    {
        mapDependency = null;

        // bnet:<name>/<major>.<minor>/<namespace>,file:<path>
        Span<Range> valueParts = stackalloc Range[2];
        if (value.Split(valueParts, ',') != 2)
            return false;

        ReadOnlySpan<char> bnetSpan = value[valueParts[0]];
        ReadOnlySpan<char> fileSpan = value[valueParts[1]];

        if (!bnetSpan.StartsWith(BnetPrefix, StringComparison.OrdinalIgnoreCase) || !fileSpan.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        bnetSpan = bnetSpan[BnetPrefix.Length..];

        // <name>/<major>.<minor>/<namespace>
        Span<Range> bnetParts = stackalloc Range[4];
        if (bnetSpan.Split(bnetParts, '/') != 3)
            return false;

        ReadOnlySpan<char> bnetName = bnetSpan[bnetParts[0]];
        ReadOnlySpan<char> bnetVersion = bnetSpan[bnetParts[1]];
        int indexOfVersionDot = bnetVersion.IndexOf('.');

        if (bnetName.IsEmpty ||
            indexOfVersionDot < 0 ||
            !int.TryParse(bnetVersion[..indexOfVersionDot], NumberStyles.Integer, CultureInfo.InvariantCulture, out int bnetVersionMajor) ||
            !int.TryParse(bnetVersion[(indexOfVersionDot + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int bnetVersionMinor) ||
            !int.TryParse(bnetSpan[bnetParts[2]], NumberStyles.Integer, CultureInfo.InvariantCulture, out int bnetNamespace))
        {
            return false;
        }

        mapDependency = new MapDependency()
        {
            BnetName = bnetName.ToString(),
            BnetVersionMajor = bnetVersionMajor,
            BnetVersionMinor = bnetVersionMinor,
            BnetNamespace = bnetNamespace,
            LocalFile = PathNormalizer.NormalizePath(fileSpan[FilePrefix.Length..], modsDirectory),
        };

        return true;
    }

    private static XDocument? TryLoadXml(Stream stream)
    {
        try
        {
            using XmlReader xmlReader = XmlReader.Create(stream, _xmlReaderSettings);

            return XDocument.Load(xmlReader, LoadOptions.None);
        }
        catch (XmlException)
        {
            return null;
        }
    }

    // finds the value assigned to the mAPMapStringID variable, e.g. libCore_gv_mAPMapStringID = "BraxisHoldout";
    private static string? GetMapId(MpqHeroesArchive mpqHeroesArchive, MpqHeroesArchiveEntry mapScriptEntry)
    {
        using Stream stream = mpqHeroesArchive.DecompressEntry(mapScriptEntry);
        using StreamReader reader = new(stream, Encoding.UTF8);

        while (reader.ReadLine() is string line)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan().TrimStart();
            if (lineSpan.StartsWith("//", StringComparison.Ordinal))
                continue;

            int indexOfEquals = lineSpan.IndexOf('=');

            // skip lines without an assignment, and comparisons such as ==, !=, <= and >=
            if (indexOfEquals < 1 ||
                (indexOfEquals + 1 < lineSpan.Length && lineSpan[indexOfEquals + 1] == '=') ||
                lineSpan[indexOfEquals - 1] is '!' or '<' or '>')
            {
                continue;
            }

            if (!lineSpan[..indexOfEquals].TrimEnd().EndsWith("mAPMapStringID", StringComparison.OrdinalIgnoreCase))
                continue;

            return lineSpan[(indexOfEquals + 1)..].Trim([' ', '\t', '"', ';']).ToString();
        }

        return null;
    }
}
