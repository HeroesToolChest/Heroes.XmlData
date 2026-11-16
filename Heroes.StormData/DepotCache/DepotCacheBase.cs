namespace Heroes.StormData.DepotCache;

internal abstract class DepotCacheBase<T> : IDepotCache
    where T : IHeroesSource
{
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
        S2MVProperties? s2mvProperties = LoadS2mvFile(s2mvFile);
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
        S2MAProperties? s2maProperties = LoadS2maFile(s2maFile);
        if (s2maProperties is null)
            return;

        s2maProperties.DirectoryPath = PathNormalizer.NormalizePath(filePath, HeroesSource.ModsDirectoryPath);

        Data.AddS2MA(filePath, s2maProperties);
    }

    private static S2MVProperties? LoadS2mvFile(Stream s2mvFile)
    {
        if (s2mvFile.Length <= 0 || s2mvFile.ReadByte() != '{')
            return null;

        s2mvFile.Position = 0;

        using JsonDocument jsonDocument = JsonDocument.Parse(s2mvFile);
        JsonElement rootElement = jsonDocument.RootElement;

        if (!rootElement.TryGetProperty("MapInfo", out JsonElement mapInfoElement) ||
            !mapInfoElement.TryGetProperty("Properties", out JsonElement propertiesElement) ||
            !propertiesElement.TryGetProperty("Loading", out JsonElement loadingElement) ||
            !loadingElement.TryGetProperty("MapLink", out JsonElement mapLinkElement))
        {
            return null;
        }

        if (!rootElement.TryGetProperty("DocInfo", out JsonElement docInfoElememt) ||
            !docInfoElememt.TryGetProperty("Info", out JsonElement infoElement) ||
            !docInfoElememt.TryGetProperty("Dependencies", out JsonElement dependenciesElement) ||
            !docInfoElememt.TryGetProperty("ModifiableDependencies", out JsonElement modifiableDependenciesElement))
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
            MapSize = new MapSize(mapInfoSizeElement.GetProperty("x").GetInt32(), mapInfoSizeElement.GetProperty("y").GetInt32()),
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

    private static S2MAProperties? LoadS2maFile(Stream s2maFile)
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

        string? mapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value).Result;

        using Stream documentInfoStream = mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value);
        using XmlReader xmlReader = XmlReader.Create(documentInfoStream, _xmlReaderSettings);

        XDocument document = XDocument.Load(xmlReader, LoadOptions.None);

        IEnumerable<XElement> dependencies = document.Root?.Element("Dependencies")!.Elements() ?? [];
        IEnumerable<XElement> modifiableDependencies = document.Root?.Element("ModifiableDependencies")!.Elements() ?? [];

        return new S2MAProperties()
        {
            DocInfoIconFile = PathNormalizer.NormalizePath(document.Root?.Element("Icon")?.Value),
            MapId = mapId,
            MapDependencies = [.. MapDependency.GetMapDependencies(dependencies, SourceConstants.DefaultModsDirectory)],
            ModifiableDependencies = [.. GetMapModifiableDependencies(modifiableDependencies, SourceConstants.DefaultModsDirectory)],
        };
    }

    private static async Task<string?> GetMapId(MpqHeroesArchive mpqHeroesArchive, MpqHeroesArchiveEntry mapScriptEntry)
    {
        using Stream stream = mpqHeroesArchive.DecompressEntry(mapScriptEntry);

        PipeReader reader = PipeReader.Create(stream);

        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                SequenceReader<byte> sequenceReader = new(line);

                if (!sequenceReader.TryReadTo(out ReadOnlySequence<byte> keyBytes, (byte)'='))
                    continue;

                ReadOnlySequence<byte> valueBytes = line.Slice(sequenceReader.Position);

                string variable = Encoding.UTF8.GetString(keyBytes);
                if (!variable.Contains("mAPMapStringID", StringComparison.OrdinalIgnoreCase))
                    continue;

                string value = Encoding.UTF8.GetString(valueBytes);

                return value.Trim([' ', '"', ';']).ToString();
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();

        return null;
    }

    private static IEnumerable<string> GetMapModifiableDependencies(IEnumerable<XElement> modifiableDependencies, string modsDirectory)
    {
        foreach (XElement valueElement in modifiableDependencies)
        {
            yield return PathNormalizer.NormalizePath(valueElement.Value, modsDirectory);
        }
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        SequencePosition? position = buffer.PositionOf((byte)'\n');

        if (position is null)
        {
            line = default;
            return false;
        }

        line = buffer.Slice(0, position.Value);

        if (!line.IsEmpty && line.Slice(line.Length - 1, 1).FirstSpan[0] == (byte)'\r')
        {
            line = line.Slice(0, line.Length - 1);
        }

        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

        return true;
    }
}
