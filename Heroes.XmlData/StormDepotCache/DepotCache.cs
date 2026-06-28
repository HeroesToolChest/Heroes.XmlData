using System.IO.Abstractions;
using System.Text;
using System.Xml;

namespace Heroes.XmlData.StormDepotCache;

internal abstract class DepotCache<T> : IDepotCache
    where T : IHeroesSource
{
    private static readonly XmlReaderSettings _xmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    public DepotCache(T heroesSource)
        : this(new FileSystem(), heroesSource)
    {
        HeroesSource = heroesSource;
    }

    public DepotCache(IFileSystem fileSystem, T heroesSource)
    {
        FileSystem = fileSystem;

        HeroesSource = heroesSource;
    }

    protected string DepotCacheDirectoryPath => Path.Join(HeroesSource.ModsBaseDirectoryPath, HeroesSource.DepotCacheDirectory);

    protected T HeroesSource { get; }

    protected IStormStorage StormStorage => HeroesSource.StormStorage;

    protected IFileSystem FileSystem { get; }

    public virtual void LoadDepotCache()
    {
        FindMapRootData();
    }

    protected static bool IsS2mvFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".s2mv", StringComparison.OrdinalIgnoreCase);

    protected static bool IsS2maFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".s2ma", StringComparison.OrdinalIgnoreCase);

    protected abstract void FindMapRootData();

    // find the root s2mv files
    protected bool LoadS2mvFile(Stream s2mvFile)
    {
        if (s2mvFile.Length <= 0 || s2mvFile.ReadByte() != '{')
            return false;

        s2mvFile.Position = 0;

        using JsonDocument jsonDocument = JsonDocument.Parse(s2mvFile);
        JsonElement rootElement = jsonDocument.RootElement;

        if (!rootElement.TryGetProperty("MapInfo", out JsonElement mapInfoElement) ||
            !mapInfoElement.TryGetProperty("Properties", out JsonElement propertiesElement) ||
            !propertiesElement.TryGetProperty("Loading", out JsonElement loadingElement) ||
            !loadingElement.TryGetProperty("MapLink", out JsonElement mapLinkElement))
        {
            return false;
        }

        if (!rootElement.TryGetProperty("DocInfo", out JsonElement docInfoElememt) ||
            !docInfoElememt.TryGetProperty("Info", out JsonElement infoElement) ||
            !docInfoElememt.TryGetProperty("Dependencies", out JsonElement dependenciesElement) ||
            !docInfoElememt.TryGetProperty("ModifiableDependencies", out JsonElement modifiableDependenciesElement))
        {
            return false;
        }

        if (!rootElement.TryGetProperty("UsedStrings", out JsonElement usedStringsElement) ||
            !usedStringsElement.TryGetProperty("DocInfo/Name", out JsonElement docInfoNameElement))
        {
            return false;
        }

        JsonElement mapInfoSizeElement = propertiesElement.GetProperty("Size");

        S2MVProperties s2mvProperties = new()
        {
            MapLink = mapLinkElement.GetString() ?? string.Empty,
            HeaderTitle = docInfoNameElement.GetProperty("enUS").GetString() ?? string.Empty,
            LoadingImage = PathHelper.NormalizePath(loadingElement.GetProperty("Image").GetString() ?? string.Empty),
            MapSize = new Point(mapInfoSizeElement.GetProperty("x").GetInt32(), mapInfoSizeElement.GetProperty("y").GetInt32()),
            PreviewLargeImage = PathHelper.NormalizePath(propertiesElement.GetProperty("PreviewLargeImage").GetString() ?? string.Empty),
            CustomLayout = PathHelper.NormalizePath(loadingElement.GetProperty("CustomLayout").GetString() ?? string.Empty),
            CustomFrame = PathHelper.NormalizePath(loadingElement.GetProperty("CustomFrame").GetString() ?? string.Empty),
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
                LocalFile = PathHelper.NormalizePath(dependencyElement.GetProperty("LocalFile").GetString(), HeroesSource.DefaultModsDirectory),
            });
        }

        foreach (JsonElement dependencyElement in modifiableDependenciesElement.EnumerateArray())
        {
            s2mvProperties.ModifiableDependencies.Add(PathHelper.NormalizePath(dependencyElement.GetString(), HeroesSource.DefaultModsDirectory));
        }

        s2mvProperties.DocInfoIconFile = PathHelper.NormalizePath(infoElement.GetProperty("IconFile").GetString());

        HeroesSource.S2MVPropertiesByHashCode.Add(s2mvProperties.GetHashCode(), s2mvProperties);

        return true;
    }

    protected bool LoadS2maFile(Stream s2maFile)
    {
        if (s2maFile.Length < 3 ||
            s2maFile.ReadByte() != 'M' ||
            s2maFile.ReadByte() != 'P' ||
            s2maFile.ReadByte() != 'Q')
        {
            return false;
        }

        s2maFile.Position = 0;

        using MpqHeroesArchive mpqHeroesArchive = MpqHeroesFile.Open(s2maFile);

        if (!mpqHeroesArchive.FileEntryExists("BankList.xml") || !mpqHeroesArchive.TryGetEntry("mapscript.galaxy", out MpqHeroesArchiveEntry? mapScriptEntry) || !mpqHeroesArchive.TryGetEntry(HeroesSource.DocumentInfoFile, out MpqHeroesArchiveEntry? documentInfoEntry))
            return false;

        string? mapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value).Result;

        using Stream documentInfoStream = mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value);
        using XmlReader xmlReader = XmlReader.Create(documentInfoStream, _xmlReaderSettings);

        XDocument document = XDocument.Load(xmlReader, LoadOptions.None);

        IEnumerable<XElement> dependencies = document.Root?.Element("Dependencies")!.Elements() ?? [];
        IEnumerable<XElement> modifiableDependencies = document.Root?.Element("ModifiableDependencies")!.Elements() ?? [];

        S2MAProperties s2maProperties = new()
        {
            DocInfoIconFile = PathHelper.NormalizePath(document.Root?.Element("Icon")?.Value),
            MapId = mapId,
            MapDependencies = [.. MapDependency.GetMapDependencies(dependencies, HeroesSource.DefaultModsDirectory)],
            ModifiableDependencies = [.. GetMapModifiableDependencies(modifiableDependencies, HeroesSource.DefaultModsDirectory)],
        };

        // find the s2mv file equivalent
        if (HeroesSource.S2MVPropertiesByHashCode.TryGetValue(s2maProperties.GetHashCode(), out S2MVProperties? s2mvProperties))
            s2maProperties.S2MVProperties = s2mvProperties;

        HeroesSource.S2MAProperties.Add(s2maProperties);

        if (s2maProperties.S2MVProperties is not null)
            HeroesSource.S2MAPropertiesByTitle.Add(s2maProperties.S2MVProperties.HeaderTitle, s2maProperties);

        return true;
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
            yield return PathHelper.NormalizePath(valueElement.Value, modsDirectory);
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
