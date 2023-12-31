using System.Text.Json;

namespace Heroes.XmlData.StormMapMods;

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
        LoadMapDependencyData();

    }

    protected static bool IsS2mvFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".s2mv", StringComparison.OrdinalIgnoreCase);

    protected static bool IsS2maFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".s2ma", StringComparison.OrdinalIgnoreCase);

    protected abstract void LoadMapDependencyData();

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
            LoadingImage = loadingElement.GetProperty("Image").GetString() ?? string.Empty,
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

        s2mvProperties.DocInfoIconFile = PathHelper.NormalizePath(infoElement.GetProperty("IconFile").GetString(), HeroesSource.DefaultModsDirectory);

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

        if (!mpqHeroesArchive.FileEntryExists("BankList.xml") || !mpqHeroesArchive.FileEntryExists("mapscript.galaxy") || !mpqHeroesArchive.TryGetEntry("DocumentInfo", out MpqHeroesArchiveEntry? documentInfoEntry))
            return false;

        XDocument document = XDocument.Load(mpqHeroesArchive.DecompressEntry(documentInfoEntry.Value));
        XElement rootElement = document.Root!;

        S2MAProperties s2maProperties = new()
        {
            DocInfoIconFile = PathHelper.NormalizePath(rootElement.Element("Icon")?.Value, HeroesSource.DefaultModsDirectory),
        };

        IEnumerable<XElement> dependencies = rootElement.Element("Dependencies")!.Elements();
        IEnumerable<XElement> modifiableDependencies = rootElement.Element("ModifiableDependencies")!.Elements();

        AddMapDependencies(s2maProperties, dependencies);
        AddMapModifiableDependencies(s2maProperties, modifiableDependencies);

        // find the s2mv file equivalent
        if (HeroesSource.S2MVPropertiesByHashCode.TryGetValue(s2maProperties.GetHashCode(), out S2MVProperties? s2mvProperties))
            s2maProperties.S2MVProperties = s2mvProperties;

        HeroesSource.S2MAProperties.Add(s2maProperties);

        return true;
    }

    private void AddMapDependencies(S2MAProperties s2maProperties, IEnumerable<XElement> dependencies)
    {
        Span<Range> valueParts = stackalloc Range[2];
        Span<Range> bnetParts = stackalloc Range[3];

        foreach (XElement valueElement in dependencies)
        {
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

            s2maProperties.MapDependencies.Add(new MapDependency()
            {
                BnetName = bnetSpan[bnetParts[0]][(indexOfBnetFileName + 1)..].ToString(),
                BnetVersionMajor = int.Parse(bnetSpan[bnetParts[1]][..indexOfBnetVersion]),
                BnetVersionMinor = int.Parse(bnetSpan[bnetParts[1]][(indexOfBnetVersion + 1)..]),
                BnetNamespace = int.Parse(bnetSpan[bnetParts[2]]),
                LocalFile = PathHelper.NormalizePath(filePathSpan[(indexOfFilePath + 1)..], HeroesSource.DefaultModsDirectory),
            });
        }
    }

    private void AddMapModifiableDependencies(S2MAProperties s2maProperties, IEnumerable<XElement> modifiableDependencies)
    {
        foreach (XElement valueElement in modifiableDependencies)
        {
            s2maProperties.ModifiableDependencies.Add(PathHelper.NormalizePath(valueElement.Value, HeroesSource.DefaultModsDirectory));
        }
    }
}
