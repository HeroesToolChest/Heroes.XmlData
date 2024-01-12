using System.Text.Json;

namespace Heroes.XmlData.StormDepotCache;

internal abstract class DepotCache<T> : IDepotCache
    where T : IHeroesSource
{
    private readonly T _heroesSource;

    public DepotCache(T heroesSource)
    {
        _heroesSource = heroesSource;
    }

    protected string DepotCacheDirectoryPath => Path.Join(HeroesSource.ModsDirectoryPath, HeroesSource.DepotCacheDirectory);

    protected T HeroesSource => _heroesSource;

    protected IHeroesData HeroesData => _heroesSource.HeroesData;

    public void LoadDepotCache()
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
            LoadingImage = PathHelper.NormalizePath(loadingElement.GetProperty("Image").GetString()),
            MapSize = new Point(mapInfoSizeElement.GetProperty("x").GetInt32(), mapInfoSizeElement.GetProperty("y").GetInt32()),
        };

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

        string? mapId = GetMapId(mpqHeroesArchive, mapScriptEntry.Value);

        XDocument document = XDocument.Load(mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value));
        XElement rootElement = document.Root!;

        IEnumerable<XElement> dependencies = rootElement.Element("Dependencies")!.Elements();
        IEnumerable<XElement> modifiableDependencies = rootElement.Element("ModifiableDependencies")!.Elements();

        S2MAProperties s2maProperties = new()
        {
            DocInfoIconFile = PathHelper.NormalizePath(rootElement.Element("Icon")?.Value),
            MapId = mapId,
            MapDependencies = MapDependency.GetMapDependencies(dependencies, HeroesSource.DefaultModsDirectory).ToList(),
            ModifiableDependencies = GetMapModifiableDependencies(modifiableDependencies, HeroesSource.DefaultModsDirectory).ToList(),
        };

        // find the s2mv file equivalent
        if (HeroesSource.S2MVPropertiesByHashCode.TryGetValue(s2maProperties.GetHashCode(), out S2MVProperties? s2mvProperties))
            s2maProperties.S2MVProperties = s2mvProperties;

        HeroesSource.S2MAProperties.Add(s2maProperties);

        if (s2maProperties.S2MVProperties is not null)
            HeroesSource.S2MAPropertiesByTitle.Add(s2maProperties.S2MVProperties.HeaderTitle, s2maProperties);

        return true;
    }

    private static string? GetMapId(MpqHeroesArchive mpqHeroesArchive, MpqHeroesArchiveEntry mapScriptEntry)
    {
        using StreamReader streamReader = new(mpqHeroesArchive.DecompressEntry(mapScriptEntry));

        while (!streamReader.EndOfStream)
        {
            ReadOnlySpan<char> line = streamReader.ReadLine();

            if (line.IsEmpty || line.IsWhiteSpace() || !line.Contains("mAPMapStringID", StringComparison.OrdinalIgnoreCase))
                continue;

            int equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0)
                continue;

            return line[(equalsIndex + 1)..].Trim().Trim(new char[] { '"', ';' }).ToString();
        }

        return null;
    }

    private static IEnumerable<string> GetMapModifiableDependencies(IEnumerable<XElement> modifiableDependencies, string modsDirectory)
    {
        foreach (XElement valueElement in modifiableDependencies)
        {
            yield return PathHelper.NormalizePath(valueElement.Value, modsDirectory);
        }
    }
}
