namespace Heroes.XmlData.StormMods;

internal abstract class StormMod<T> : IStormMod
    where T : IHeroesSource
{
    private readonly T _heroesSource;
    private readonly List<IStormMod> _includesStormModsCache = [];
    private readonly XmlStorage _xmlStorage = new();

    public StormMod(T heroesSource)
    {
        _heroesSource = heroesSource;
    }

    /// <summary>
    /// Gets the inner path, after the "mods" and before the "base" directory.
    /// </summary>
    public abstract string DirectoryPath { get; }

    /// <summary>
    /// Gets the name of this stormmod. Not necessarily the file name.
    /// </summary>
    public virtual string Name => Path.GetFileNameWithoutExtension(DirectoryPath);

    /// <summary>
    /// Gets the storage object to keep track of all the loaded xml and gamestrings.
    /// </summary>
    protected XmlStorage XmlStorage => _xmlStorage;

    /// <summary>
    /// Gets the GameData directory path.
    /// </summary>
    protected virtual string GameDataDirectoryPath => Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataDirectory);

    /// <summary>
    /// Gets the gamedata.xml file path.
    /// </summary>
    protected virtual string GameDataFilePath => Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataXmlFile);

    /// <summary>
    /// Gets the includes.xml file path.
    /// </summary>
    protected virtual string IncludesFilePath => Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, HeroesSource.IncludesXmlFile);

    /// <summary>
    /// Gets the DocumentInfo file path.
    /// </summary>
    protected virtual string DocumentInfoPath => Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.DocumentInfoFile);

    /// <summary>
    /// Gets the FontStyles.StormStyle file path.
    /// </summary>
    protected virtual string FontStyleFilePath => Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, HeroesSource.UIDirectory, HeroesSource.FontStyleFile);

    protected T HeroesSource => _heroesSource;

    protected IHeroesData HeroesData => _heroesSource.HeroesData;

    public virtual void LoadStormData()
    {
        XmlStorage.StormModName = Name;
        XmlStorage.StormModDirectoryPath = DirectoryPath;
        HeroesData.SetCurrentStormMod(Name, DirectoryPath);

        LoadGameDataXmlFile();
        LoadGameDataDirectory();
        LoadFontStyleFile();

        HeroesData.AddXmlStorage(XmlStorage);

        LoadIncludesStormMods();
    }

    public virtual void LoadStormGameStrings(HeroesLocalization localization)
    {
        _includesStormModsCache.ForEach(x => x.LoadStormGameStrings(localization));
        LoadBaseStormGameStrings(localization);
    }

    public IEnumerable<IStormMod> GetStormMapMods(S2MAProperties s2maProperties)
    {
        return BuildMapDependencyTree(s2maProperties);
    }

    public List<IStormMod> LoadDocumentInfoFile()
    {
        if (!TryGetFile(DocumentInfoPath, out Stream? stream))
            return [];

        XDocument document = XDocument.Load(stream);
        XElement rootElement = document.Root!;

        IEnumerable<XElement> dependencies = rootElement.Element("Dependencies")!.Elements();
        IEnumerable<MapDependency> mapDependencies = MapDependency.GetMapDependencies(dependencies, HeroesSource.DefaultModsDirectory);

        return GetStormMapModDependencies(mapDependencies);
    }

    public void LoadFontStyleFile()
    {
        if (!TryGetFile(FontStyleFilePath, out Stream? stream))
            return;

        XDocument document = XDocument.Load(stream);

        XmlStorage.AddXmlFontStyleFile(document, FontStyleFilePath);
    }

    protected static bool IsXmlFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the gamestrings.txt file path.
    /// </summary>
    /// <param name="localization">The localization.</param>
    /// <returns>The path to the gamestrings file.</returns>
    protected virtual string GetGameStringFilePath(HeroesLocalization localization)
    {
        return Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, localization.GetDescription(), HeroesSource.LocalizedDataDirectory, HeroesSource.GameStringFile);
    }

    /// <summary>
    /// Adds an xml file to the <see cref="HeroesData"/>. Checks first if it's an xml file and it exists.
    /// </summary>
    /// <param name="xmlFilePath">The path to the xml file.</param>
    protected void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        XmlStorage.AddXmlDataFile(document, xmlFilePath);
    }

    /// <summary>
    /// Validates an xml file, returning a value that indicates whether the file exists.
    /// </summary>
    /// <param name="xmlFilePath">The file path of the file.</param>
    /// <param name="document">When this method returns, contains an <see cref="XDocument"/> of the <paramref name="xmlFilePath"/>.</param>
    /// <param name="isRequired">If <see langword="true"/>, if file does not exist, then add the <paramref name="xmlFilePath"/> as a missing file.</param>
    /// <returns><see langword="true"/> if the xml file was found; otherwise <see langword="false"/>.</returns>
    protected bool ValidateXmlFile(string xmlFilePath, [NotNullWhen(true)] out XDocument? document, bool isRequired = true)
    {
        document = null;

        if (!IsXmlFile(xmlFilePath))
            return false;

        if (!TryGetFile(xmlFilePath, out Stream? stream))
        {
            if (isRequired)
                HeroesData.AddFileNotFound(xmlFilePath);

            return false;
        }

        using Stream fileStream = stream;
        document = XDocument.Load(fileStream);

        return true;
    }

    /// <summary>
    /// Validates an gamestring file, returning a value that indicates whether the file exists.
    /// </summary>
    /// <param name="localization">The <see cref="HeroesLocalization"/> for the file.</param>
    /// <param name="stream">When this method returns, contains the <see cref="Stream"/> of the gamestring file.</param>
    /// <param name="path">When this method returns, contains the file path to the gamestring file.</param>
    /// <returns><see langword="true"/> if the gamestring file was found; otherwise <see langword="false"/>.</returns>
    protected bool ValidateGameStringFile(HeroesLocalization localization, [NotNullWhen(true)] out Stream? stream, out string path)
    {
        path = GetGameStringFilePath(localization);

        if (!TryGetFile(path, out stream))
        {
            HeroesData.AddFileNotFound(path);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a file if it exists, return a value that indicates whether the file exists.
    /// </summary>
    /// <param name="filePath">The file path of the file.</param>
    /// <param name="stream">When this method returns, contains a <see cref="Stream"/> of the file.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream);

    /// <summary>
    /// Loads the xml files from the <see cref="GameDataDirectoryPath"/>.
    /// </summary>
    protected abstract void LoadGameDataDirectory();

    protected abstract IStormMod GetStormMod(string path);

    /// <summary>
    /// Loads and adds the files from the gamedata.xml file.
    /// </summary>
    /// <exception cref="InvalidOperationException">The root element does not exist in the xml file.</exception>
    protected void LoadGameDataXmlFile()
    {
        if (!ValidateXmlFile(GameDataFilePath, out XDocument? document, isRequired: false))
            return;

        if (document.Root is null)
            throw new InvalidOperationException();

        IEnumerable<XElement> catalogElements = document.Root.Elements("Catalog");

        foreach (XElement catalogElement in catalogElements)
        {
            ReadOnlySpan<char> catalogPathValue = catalogElement.Attribute("path")?.Value;

            if (catalogPathValue.IsEmpty || catalogPathValue.IsWhiteSpace())
                continue;

            string path = PathHelper.NormalizePath(catalogPathValue, HeroesSource.DefaultModsDirectory);

            if (path.StartsWith(HeroesSource.GameDataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                string xmlFilePath = Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, path);

                AddXmlFile(xmlFilePath);
            }
        }
    }

    /// <summary>
    /// Loads and adds the files from the includes.xml file. Caches the storm mods from the file.
    /// </summary>
    /// <exception cref="InvalidOperationException">The root element does not exist in the xml file.</exception>
    protected void LoadIncludesStormMods()
    {
        if (!ValidateXmlFile(IncludesFilePath, out XDocument? document, isRequired: false))
            return;

        if (document.Root is null)
            throw new InvalidOperationException();

        IEnumerable<XElement> pathElements = document.Root.Elements("Path");

        foreach (XElement pathElement in pathElements)
        {
            ReadOnlySpan<char> pathValuePath = pathElement.Attribute("value")?.Value;

            if (pathValuePath.IsEmpty || pathValuePath.IsWhiteSpace())
                continue;

            string path = PathHelper.NormalizePath(pathValuePath, HeroesSource.DefaultModsDirectory);
            IStormMod stormMod = GetStormMod(path);

            _includesStormModsCache.Add(stormMod);
            stormMod.LoadStormData();
        }
    }

    /// <summary>
    /// Loads and adds the gamestrings from the gamestrings.txt file.
    /// </summary>
    /// <param name="localization">The localization of the file to load.</param>
    protected void LoadBaseStormGameStrings(HeroesLocalization localization)
    {
        if (!ValidateGameStringFile(localization, out Stream? stream, out string path))
            return;

        XmlStorage.AddGameStringFile(stream, path);
    }

    private IEnumerable<IStormMod> BuildMapDependencyTree(S2MAProperties s2maProperties)
    {
        List<IStormMod> mapModDependecies = GetStormMapModDependencies(s2maProperties.MapDependencies);
        mapModDependecies.Add(this); // add this mod last

        // remove the heroesdata.stormmod references and remove any earlier occurences of duplicated stormmods
        return mapModDependecies.Where(x => !x.DirectoryPath.EndsWith(HeroesSource.HeroesDataStormModDirectory, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(x => x.DirectoryPath, StringComparer.OrdinalIgnoreCase);
    }

    private List<IStormMod> GetStormMapModDependencies(IEnumerable<MapDependency> mapDependencies)
    {
        List<IStormMod> mapModDependecies = [];
        List<IStormMod> currentDependencies = [];

        foreach (MapDependency mapDependency in mapDependencies)
        {
            IStormMod stormMod = GetStormMod(mapDependency.LocalFile);
            currentDependencies.Add(stormMod);

            mapModDependecies.AddRange(stormMod.LoadDocumentInfoFile());
        }

        currentDependencies.Reverse(); // flip-flop
        mapModDependecies.AddRange(currentDependencies);

        return mapModDependecies;
    }
}
