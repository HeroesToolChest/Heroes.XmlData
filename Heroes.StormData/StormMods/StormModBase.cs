namespace Heroes.StormData.StormMods;

/// <summary>
/// Represents a base class for a <c>stormmod</c> directory.
/// </summary>
/// <typeparam name="T">The type of the <see cref="IHeroesSource"/> source.</typeparam>
internal abstract class StormModBase<T> : IStormMod
    where T : IHeroesSource
{
    protected const string XmlFileExtension = ".xml";
    protected const string StormLayoutFileExtension = ".stormlayout";
    protected const string DDSFileExtension = ".dds";

    private static readonly XmlReaderSettings _xmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = false,
    };

    private readonly List<IStormMod> _includesStormModsCache = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StormModBase{T}"/> class.
    /// </summary>
    /// <param name="heroesSource">The source of the data.</param>
    /// <param name="directoryPath">The full path of the <c>stormmod</c> directory. No trailing directory separator.</param>
    /// <param name="stormModType">Type of the <c>stormmod</c>.</param>
    public StormModBase(T heroesSource, string directoryPath, StormModType stormModType)
    {
        DirectoryPath = directoryPath;
        ModType = stormModType;
        //StormPathType = stormPathType;

        HeroesSource = heroesSource;
        ModContainer = new StormModContainer(heroesSource.SourceContainer, ModType);
        //StormModStorage = heroesSource.StormStorage.CreateModStorage(this);

        Name = Path.GetFileNameWithoutExtension(DirectoryPath);
        RelativeStormModPath = Path.GetRelativePath(HeroesSource.ModsDirectoryPath, DirectoryPath);

        BaseStormDataPath = Path.Combine(DirectoryPath, SourceConstants.BaseStormDataDirectory);
        GameDataDirectoryPath = Path.Combine(BaseStormDataPath, SourceConstants.GameDataDirectory);
        GameDataXmlFilePath = Path.Combine(BaseStormDataPath, SourceConstants.GameDataXmlFile);
        IncludesXmlFilePath = Path.Combine(BaseStormDataPath, SourceConstants.IncludesXmlFile);
        AssetsTextFilePath = Path.Combine(GameDataDirectoryPath, SourceConstants.AssetsTextFile);
        LayoutDirectoryPath = Path.Combine(BaseStormDataPath, SourceConstants.UiDirectory, SourceConstants.LayoutDirectory);
        AssetsDirectoryPath = Path.Combine(BaseStormDataPath, SourceConstants.AssetsDirectory);
        FontStyleFilePath = Path.Combine(BaseStormDataPath, SourceConstants.UiDirectory, SourceConstants.FontStyleFile);
    }

    public string Name { get; }

    public string RelativeStormModPath { get; }

    public string DirectoryPath { get; }

    public StormModType ModType { get; }

    public StormModContainer ModContainer { get; }

    protected T HeroesSource { get; }

    /// <summary>
    /// Gets the path of the <c>GameData</c> directory.
    /// </summary>
    protected virtual string GameDataDirectoryPath { get; }

    /// <summary>
    /// Gets the path of the <c>base.stormdata</c> directory.
    /// </summary>
    protected string BaseStormDataPath { get; }

    /// <summary>
    /// Gets the <c>gamedata.xml</c> file path.
    /// </summary>
    protected virtual string GameDataXmlFilePath { get; }

    /// <summary>
    /// Gets the <c>includes.xml</c> file path.
    /// </summary>
    protected virtual string IncludesXmlFilePath { get; }

    /// <summary>
    /// Gets the <c>Assets.txt</c> file path.
    /// </summary>
    protected virtual string AssetsTextFilePath { get; }

    /// <summary>
    /// Gets the layout directory path.
    /// </summary>
    protected virtual string LayoutDirectoryPath { get; }

    /// <summary>
    /// Gets the Assets directory path.
    /// </summary>
    protected virtual string AssetsDirectoryPath { get; }

    /// <summary>
    /// Gets the <c>fontstyles.stormstyle</c> file path.
    /// </summary>
    protected virtual string FontStyleFilePath { get; }

    public virtual void LoadStormFiles()
    {
        LoadFontStyleFile();
        LoadStormLayoutDirectory();
        LoadAssetsDirectory();
        LoadAssetsTextFile();

        LoadGameDataDirectory();
        LoadGameDataXmlFile();

        LoadIncludesStormMods();
    }

    /// <summary>
    /// Loads the XML files from the <see cref="GameDataDirectoryPath"/>. Only this directory and not the subdirectories.
    /// The subdirectories are loaded from <see cref="LoadGameDataXmlFile"/>.
    /// </summary>
    protected abstract void LoadGameDataDirectory();

    /// <summary>
    /// Loads the assets files from the <see cref="AssetsDirectoryPath"/>. Include all subdirectories.
    /// </summary>
    protected abstract void LoadAssetsDirectory();

    /// <summary>
    /// Loads storm layout files paths (not through the desc index file) including subdirectories.
    /// </summary>
    protected abstract void LoadStormLayoutDirectory();

    protected void LoadGameDataFiles(IEnumerable<string> files)
    {
        foreach (string file in files)
        {
            AddXmlFile(file);
        }
    }

    protected void LoadStormLayoutFiles(IEnumerable<string> files)
    {
        foreach (string file in files)
        {
            AddStormLayoutFilePath(file);
        }
    }

    protected void LoadAssetFiles(IEnumerable<string> files)
    {
        foreach (string file in files)
        {
            AddAssetFilePath(file);
        }
    }

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="filePath">The file path of the file.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool IsFileExists(string filePath);

    /// <summary>
    /// Gets a file if it exists, returns a value that indicates whether the file exists.
    /// </summary>
    /// <param name="path">The file path of the file.</param>
    /// <param name="stream">When this method returns, contains a <see cref="Stream"/> of the file.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream);

    protected abstract IStormMod GetStormMod(string path, StormModType stormModType);

    private void LoadFontStyleFile()
    {
        if (!TryGetFile(FontStyleFilePath, out Stream? stream))
            return;

        //XDocument document = XDocument.Load(stream);

        //StormModStorage.AddXmlFontStyleFile(document, GetStormPath(FontStyleFilePath));
    }

    private void LoadAssetsTextFile()
    {
        if (!TryGetFile(AssetsTextFilePath, out Stream? stream))
            return;

        using (stream)
        {
            ModContainer.LoadAssetsTextFile(stream, new StormFile()
            {
                StormModName = Name,
                StormModPath = AssetsTextFilePath,
            });
        }
    }

    /// <summary>
    /// Loads and adds the files from the gamedata.xml file.
    /// </summary>
    private void LoadGameDataXmlFile()
    {
        if (!ValidateXmlFile(GameDataXmlFilePath, out XmlReader? reader, out _, isRequired: false))
            return;

        using XmlReader xmlReader = reader;

        if (!xmlReader.ReadToFollowing("Catalog"))
            return;

        do
        {
            if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "Catalog")
                continue;

            string? pathValue = xmlReader.GetAttribute("path");

            if (string.IsNullOrWhiteSpace(pathValue))
                continue;

            string path = PathNormalizer.NormalizePath(pathValue, SourceConstants.DefaultModsDirectory);

            if (path.StartsWith(SourceConstants.GameDataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                string xmlFilePath = Path.Combine(BaseStormDataPath, path);
                AddXmlFile(xmlFilePath);
            }
        }
        while (xmlReader.ReadToNextSibling("Catalog"));
    }

    /// <summary>
    /// Loads and adds the files from the <c>includes.xml</c> file. Caches the storm mods from the file.
    /// </summary>
    private void LoadIncludesStormMods()
    {
        if (!ValidateXmlFile(IncludesXmlFilePath, out XmlReader? reader, out _, isRequired: false))
            return;

        using XmlReader xmlReader = reader;

        if (!xmlReader.ReadToFollowing("Path"))
            return;

        do
        {
            if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "Path")
                continue;

            string? pathValue = xmlReader.GetAttribute("value");

            if (string.IsNullOrWhiteSpace(pathValue))
                continue;

            string path = Paths.PathNormalizer.NormalizePath(pathValue, SourceConstants.DefaultModsDirectory);

            IStormMod stormMod = GetStormMod(path, StormModType.Base);

            _includesStormModsCache.Add(stormMod);
            stormMod.LoadStormFiles();

            HeroesSource.SourceContainer.AddModContainer(stormMod.ModContainer);
        }
        while (xmlReader.ReadToNextSibling("Path"));
    }

    /// <summary>
    /// Adds an XML file to the <see cref="ModContainer"/>. Checks first if it's an XML file and it exists.
    /// </summary>
    /// <param name="path">The path to the XML file.</param>
    private void AddXmlFile(string path)
    {
        if (!ValidateXmlFile(path, out XmlReader? xmlReader, out StormFile stormFile))
            return;

        using (xmlReader)
            ModContainer.LoadXmlFile(xmlReader, stormFile);
    }

    private void AddStormLayoutFilePath(string layoutFilePath)
    {
        ReadOnlySpan<char> uiPath = layoutFilePath;

        int indexOfBase = layoutFilePath.IndexOf(SourceConstants.BaseStormDataDirectory, StringComparison.OrdinalIgnoreCase);

        if (indexOfBase > -1)
            uiPath = layoutFilePath.AsSpan()[(indexOfBase + SourceConstants.BaseStormDataDirectory.Length)..].TrimStart(Path.DirectorySeparatorChar);

        if (uiPath.StartsWith(SourceConstants.UiDirectory, StringComparison.OrdinalIgnoreCase))
        {
            ModContainer.AddStormLayoutFile(PathNormalizer.NormalizePath(uiPath), new StormFile()
            {
                StormModName = Name,
                StormModPath = layoutFilePath,
            });
        }
    }

    private void AddAssetFilePath(string assetFilePath)
    {
        ReadOnlySpan<char> assetsPath = assetFilePath;

        int indexOfBase = assetFilePath.IndexOf(SourceConstants.BaseStormAssetsDirectory, StringComparison.OrdinalIgnoreCase);

        if (indexOfBase > -1)
            assetsPath = assetFilePath.AsSpan()[(indexOfBase + SourceConstants.BaseStormAssetsDirectory.Length)..].TrimStart(Path.DirectorySeparatorChar);

        if (assetsPath.StartsWith(SourceConstants.AssetsDirectory, StringComparison.OrdinalIgnoreCase))
        {
            ModContainer.AddAssetFile(PathNormalizer.NormalizePath(assetsPath), new StormFile()
            {
                StormModName = Name,
                StormModPath = assetFilePath,
            });
        }
    }

    private bool ValidateXmlFile(string path, [NotNullWhen(true)] out XmlReader? reader, out StormFile stormFile, bool isRequired = true)
    {
        reader = null;
        stormFile = new StormFile()
        {
            StormModName = Name,
            StormModPath = path,
        };

        if (!TryGetFile(path, out Stream? stream))
        {
            if (isRequired)
                ModContainer.AddFileNotFound(stormFile);

            return false;
        }

        reader = XmlReader.Create(stream, _xmlReaderSettings);

        return true;
    }
}
