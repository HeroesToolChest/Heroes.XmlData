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

    private static readonly SearchValues<string> _mapStringIdSearchValues = SearchValues.Create(["mAPMapStringID"], StringComparison.OrdinalIgnoreCase);

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
        // a map's s2mv file is json, most of the others are images
        if (s2mvFile.Length <= 0 || s2mvFile.ReadByte() != '{')
            return null;

        s2mvFile.Position = 0;

        int length = (int)s2mvFile.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            s2mvFile.ReadExactly(buffer, 0, length);

            ReadOnlyMemory<byte> json = buffer.AsMemory(0, length);

            // json files that are not maps don't have a MapInfo property, so they can be rejected without parsing them.
            if (json.Span.IndexOf("\"MapInfo\""u8) < 0)
                return null;

            return ParseS2MVJson(json);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static S2MVProperties? ParseS2MVJson(ReadOnlyMemory<byte> json)
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

    // the returned JsonDocument uses json directly, so json must not be changed or returned to a pool until the JsonDocument is disposed
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

        string? iconFile;
        List<MapDependency> mapDependencies;
        List<string> modifiableDependencies;

        using (Stream documentInfoStream = mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value))
        {
            if (!TryReadDocumentInfo(documentInfoStream, out iconFile, out mapDependencies, out modifiableDependencies))
                return null;
        }

        return new S2MAProperties()
        {
            DocInfoIconFile = PathNormalizer.NormalizePath(iconFile),
            MapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value),
            MapDependencies = mapDependencies,
            ModifiableDependencies = modifiableDependencies,
        };
    }

    /// <summary>
    /// Reads the <c>Icon</c>, <c>Dependencies</c> and <c>ModifiableDependencies</c> elements of a <c>documentinfo</c> file.
    /// Only the first of each element under the root is used.
    /// </summary>
    /// <param name="stream">The stream of the <c>documentinfo</c> file.</param>
    /// <param name="iconFile">The value of the <c>Icon</c> element, or <see langword="null"/> if there is none.</param>
    /// <param name="mapDependencies">The parsed values of the <c>Dependencies</c> child elements.</param>
    /// <param name="modifiableDependencies">The normalized values of the <c>ModifiableDependencies</c> child elements.</param>
    /// <returns><see langword="false"/> if the file is not valid xml or a dependency could not be parsed; otherwise <see langword="true"/>.</returns>
    private static bool TryReadDocumentInfo(Stream stream, out string? iconFile, out List<MapDependency> mapDependencies, out List<string> modifiableDependencies)
    {
        iconFile = null;
        mapDependencies = [];
        modifiableDependencies = [];

        bool hasIcon = false;
        bool hasDependencies = false;
        bool hasModifiableDependencies = false;

        try
        {
            using XmlReader reader = XmlReader.Create(stream, _xmlReaderSettings);

            if (reader.MoveToContent() != XmlNodeType.Element)
                return false;

            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.Read();

                // the direct children of the root element
                while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        reader.Skip();
                    }
                    else if (!hasIcon && reader.Name == "Icon")
                    {
                        hasIcon = true;
                        iconFile = ReadElementValue(reader);
                    }
                    else if (!hasDependencies && reader.Name == "Dependencies")
                    {
                        hasDependencies = true;

                        foreach (string value in ReadChildElementValues(reader))
                        {
                            // a dependency that can't be parsed would change the dependency key, so the map would not match its s2mv file
                            if (!TryParseMapDependency(value, SourceConstants.DefaultModsDirectory, out MapDependency? mapDependency))
                                return false;

                            mapDependencies.Add(mapDependency);
                        }
                    }
                    else if (!hasModifiableDependencies && reader.Name == "ModifiableDependencies")
                    {
                        hasModifiableDependencies = true;

                        foreach (string value in ReadChildElementValues(reader))
                        {
                            modifiableDependencies.Add(PathNormalizer.NormalizePath(value, SourceConstants.DefaultModsDirectory));
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }

            // read the rest of the document so that malformed xml after the root element is still rejected
            while (reader.Read())
            {
            }

            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    // reader must be on the start of an element; returns the values of its child elements and moves past its end element
    private static List<string> ReadChildElementValues(XmlReader reader)
    {
        List<string> values = [];

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return values;
        }

        reader.Read();

        while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element)
                values.Add(ReadElementValue(reader));
            else
                reader.Skip();
        }

        reader.Read();

        return values;
    }

    // reader must be on the start of an element; returns all of its descendant text (like XElement.Value) and moves past its end element
    private static string ReadElementValue(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return string.Empty;
        }

        int depth = reader.Depth;
        string? value = null;
        StringBuilder? stringBuilder = null;

        while (reader.Read() && reader.Depth > depth)
        {
            if (reader.NodeType is not (XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.SignificantWhitespace))
                continue;

            if (value is null)
            {
                value = reader.Value;
            }
            else
            {
                stringBuilder ??= new StringBuilder(value);
                stringBuilder.Append(reader.Value);
            }
        }

        // move past the end element
        reader.Read();

        return stringBuilder?.ToString() ?? value ?? string.Empty;
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

    // finds the value assigned to the mAPMapStringID variable, e.g. libCore_gv_mAPMapStringID = "BraxisHoldout";
    private static string? GetMapId(MpqHeroesArchive mpqHeroesArchive, MpqHeroesArchiveEntry mapScriptEntry)
    {
        int byteCount = (int)mapScriptEntry.FileSize;
        byte[] byteBuffer = ArrayPool<byte>.Shared.Rent(byteCount);
        char[]? charBuffer = null;

        try
        {
            Span<byte> bytes = byteBuffer.AsSpan(0, byteCount);
            mpqHeroesArchive.DecompressEntry(mapScriptEntry, bytes);

            if (bytes.StartsWith(Encoding.UTF8.Preamble))
                bytes = bytes[Encoding.UTF8.Preamble.Length..];

            charBuffer = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(bytes.Length));
            int charCount = Encoding.UTF8.GetChars(bytes, charBuffer);

            return FindMapId(charBuffer.AsSpan(0, charCount));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(byteBuffer);

            if (charBuffer is not null)
                ArrayPool<char>.Shared.Return(charBuffer);
        }
    }

    // only the lines containing mAPMapStringID are checked, instead of reading every line of the script
    private static string? FindMapId(ReadOnlySpan<char> script)
    {
        int searchStart = 0;

        while (searchStart < script.Length)
        {
            int index = script[searchStart..].IndexOfAny(_mapStringIdSearchValues);
            if (index < 0)
                return null;

            index += searchStart;

            int lineStart = script[..index].LastIndexOfAny('\r', '\n') + 1;
            int lineEnd = script[index..].IndexOfAny('\r', '\n');
            lineEnd = lineEnd < 0 ? script.Length : index + lineEnd;

            if (TryGetMapIdFromLine(script[lineStart..lineEnd], out string? mapId))
                return mapId;

            searchStart = lineEnd;
        }

        return null;
    }

    private static bool TryGetMapIdFromLine(ReadOnlySpan<char> line, [NotNullWhen(true)] out string? mapId)
    {
        mapId = null;

        line = line.TrimStart();
        if (line.StartsWith("//", StringComparison.Ordinal))
            return false;

        int indexOfEquals = line.IndexOf('=');

        // skip lines without an assignment, and comparisons such as ==, !=, <= and >=
        if (indexOfEquals < 1 ||
            (indexOfEquals + 1 < line.Length && line[indexOfEquals + 1] == '=') ||
            line[indexOfEquals - 1] is '!' or '<' or '>')
        {
            return false;
        }

        if (!line[..indexOfEquals].TrimEnd().EndsWith("mAPMapStringID", StringComparison.OrdinalIgnoreCase))
            return false;

        mapId = line[(indexOfEquals + 1)..].Trim([' ', '\t', '"', ';']).ToString();

        return true;
    }
}
