namespace Heroes.StormData.DepotCache.Parsing;

/// <summary>
/// Parses the <c>.s2ma</c> files in the depot cache.
/// </summary>
internal static class S2MAFileParser
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

    /// <summary>
    /// Parses a <c>.s2ma</c> file. Only files that are a map stormmod file (an MPQ archive with a <c>BankList.xml</c>, <c>mapscript.galaxy</c> and <c>documentinfo</c>) are parsed.
    /// </summary>
    /// <param name="s2maFile">The stream of the file. Must be seekable.</param>
    /// <param name="s2maProperties">The parsed <see cref="S2MAProperties"/>, or <see langword="null"/> if the file is not a map stormmod file.</param>
    /// <returns><see langword="true"/> if the file was parsed; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(Stream s2maFile, [NotNullWhen(true)] out S2MAProperties? s2maProperties)
    {
        s2maProperties = null;

        if (s2maFile.Length < 3 ||
            s2maFile.ReadByte() != 'M' ||
            s2maFile.ReadByte() != 'P' ||
            s2maFile.ReadByte() != 'Q')
        {
            return false;
        }

        s2maFile.Position = 0;

        using MpqHeroesArchive mpqHeroesArchive = MpqHeroesFile.Open(s2maFile);

        if (!mpqHeroesArchive.FileEntryExists("BankList.xml") || !mpqHeroesArchive.TryGetEntry("mapscript.galaxy", out MpqHeroesArchiveEntry? mapScriptEntry) || !mpqHeroesArchive.TryGetEntry(SourceConstants.DocumentInfoFile, out MpqHeroesArchiveEntry? documentInfoEntry))
            return false;

        string? iconFile;
        List<MapDependency> mapDependencies;
        List<string> modifiableDependencies;

        using (Stream documentInfoStream = mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value))
        {
            if (!TryReadDocumentInfo(documentInfoStream, out iconFile, out mapDependencies, out modifiableDependencies))
                return false;
        }

        s2maProperties = new S2MAProperties()
        {
            DocInfoIconFile = PathNormalizer.NormalizePath(iconFile),
            MapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value),
            MapDependencies = mapDependencies,
            ModifiableDependencies = modifiableDependencies,
        };

        return true;
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
