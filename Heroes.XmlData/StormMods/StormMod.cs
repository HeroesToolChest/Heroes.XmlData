namespace Heroes.XmlData.StormMods;

internal abstract class StormMod<T> : IStormMod
    where T : IHeroesSource
{
    private readonly T _heroesSource;
    private List<IStormMod> _includesStormModsCache = [];

    public StormMod(T heroesSource)
    {
        _heroesSource = heroesSource;
    }

    /// <summary>
    /// Gets the inner path, after the "mods" and before the "base" directory.
    /// </summary>
    protected abstract string? DirectoryPath { get; }

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

    protected T HeroesSource => _heroesSource;

    protected IHeroesData HeroesData => _heroesSource.HeroesData;

    public virtual void LoadStormData()
    {
        LoadGameDataXmlFile();
        LoadIncludesStormMods(ref _includesStormModsCache);

        LoadGameDataDirectory();
    }

    public virtual void LoadStormGameStrings(HeroesLocalization localization)
    {
        _includesStormModsCache.ForEach(x => x.LoadStormGameStrings(localization));
        LoadBaseStormGameStrings(localization);
    }

    protected static bool IsXmlFile(string xmlFilePath) => Path.GetExtension(xmlFilePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the gamestrings.txt file path.
    /// </summary>
    /// <param name="localization">The localization.</param>
    /// <returns>The path to the gamestrings file.</returns>
    protected string GetGameStringFilePath(HeroesLocalization localization)
    {
        return Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, localization.GetDescription(), HeroesSource.LocalizedDataDirectory, HeroesSource.GameStringFile);
    }

    /// <summary>
    /// Adds an xml file to the <see cref="HeroesData"/>. Checks first if it's an xml file and it exists.
    /// </summary>
    /// <param name="xmlFilePath">The path to the xml file.</param>
    protected abstract void AddXmlFile(string xmlFilePath);

    /// <summary>
    /// Validates an xml file, returning a value that indicates whether the file exists.
    /// </summary>
    /// <param name="xmlFilePath">The file path of the file.</param>
    /// <param name="document">When this method returns, contains an <see cref="XDocument"/> of the <paramref name="xmlFilePath"/>.</param>
    /// <param name="isRequired">If <see langword="true"/>, if file does not exist, then add the <paramref name="xmlFilePath"/> as a missing file.</param>
    /// <returns><see langword="true"/> if the xml file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool ValidateXmlFile(string xmlFilePath, [NotNullWhen(true)] out XDocument? document, bool isRequired = true);

    /// <summary>
    /// Validates an gamestring file, returning a value that indicates whether the file exists.
    /// </summary>
    /// <param name="localization">The <see cref="HeroesLocalization"/> for the file.</param>
    /// <param name="stream">When this method returns, contains the <see cref="Stream"/> of the gamestring file.</param>
    /// <param name="path">When this method returns, contains the file path to the gamestring file.</param>
    /// <returns><see langword="true"/> if the gamestring file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool ValidateGameStringFile(HeroesLocalization localization, [NotNullWhen(true)] out Stream? stream, out string path);

    /// <summary>
    /// Loads the xml files from the <see cref="GameDataDirectoryPath"/>.
    /// </summary>
    protected abstract void LoadGameDataDirectory();

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
                string xmlFilePath = Path.Join(HeroesSource.ModsDirectoryPath, DirectoryPath, HeroesSource.BaseStormDataDirectory, catalogPathValue);

                AddXmlFile(xmlFilePath);
            }
        }
    }

    /// <summary>
    /// Loads and adds the files from the includes.xml file. Caches the storm mods from the file.
    /// </summary>
    /// <param name="stormModsCache">Cache of the storm mods.</param>
    /// <exception cref="InvalidOperationException">The root element does not exist in the xml file.</exception>
    protected void LoadIncludesStormMods(ref List<IStormMod> stormModsCache)
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

            IStormMod stormMod = HeroesSource.CreateStormModInstance<FileStormModPathStormMod>(HeroesSource, path);

            stormModsCache.Add(stormMod);
            stormMod.LoadStormData();
        }
    }

    /// <summary>
    /// Loads and adds the gamestrings from the gamestrings.txt file.
    /// </summary>
    /// <param name="localization">The localization of the file to load.</param>
    protected virtual void LoadBaseStormGameStrings(HeroesLocalization localization)
    {
        if (!ValidateGameStringFile(localization, out Stream? stream, out string path))
            return;

        HeroesData.AddMainGameStringFile(stream, path);
    }
}
