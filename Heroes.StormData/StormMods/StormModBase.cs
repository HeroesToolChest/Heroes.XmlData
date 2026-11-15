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
        CloseInput = true,
    };

    private static readonly char[] _directorySeparatorChars = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

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
        AssetsDirectoryPath = Path.Combine(DirectoryPath, SourceConstants.BaseStormAssetsDirectory, SourceConstants.AssetsDirectory);
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
    /// Gets a file if it exists, returns a value that indicates whether the file exists.
    /// </summary>
    /// <param name="path">The file path of the file.</param>
    /// <param name="stream">When this method returns, contains a <see cref="Stream"/> of the file.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>.</returns>
    protected abstract bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream);

    protected abstract IStormMod GetStormMod(string path, StormModType stormModType);

    /// <summary>
    /// Reads the value of an attribute from each sibling element with the specified name, starting from the first one found.
    /// Elements without the attribute, or with an empty value, are skipped.
    /// </summary>
    /// <param name="xmlReader">The <see cref="XmlReader"/> to read from.</param>
    /// <param name="elementName">The name of the elements.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute values.</returns>
    private static IEnumerable<string> ReadAttributeValues(XmlReader xmlReader, string elementName, string attributeName)
    {
        if (!xmlReader.ReadToFollowing(elementName))
            yield break;

        do
        {
            string? value = xmlReader.GetAttribute(attributeName);

            if (!string.IsNullOrWhiteSpace(value))
                yield return value;
        }
        while (xmlReader.ReadToNextSibling(elementName));
    }

    /// <summary>
    /// Checks if a path starts with a directory, as a whole directory name (e.g. <c>gamedata</c> does not match <c>gamedata.xml</c>).
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="directory">The directory name.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> starts with <paramref name="directory"/>; otherwise <see langword="false"/>.</returns>
    private static bool StartsWithDirectory(ReadOnlySpan<char> path, string directory) =>
        path.Length > directory.Length &&
        path.StartsWith(directory, StringComparison.OrdinalIgnoreCase) &&
        IsDirectorySeparator(path[directory.Length]);

    private static bool IsDirectorySeparator(char value) => value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar;

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
            ModContainer.LoadAssetsTextFile(stream, CreateStormFile(AssetsTextFilePath));
    }

    /// <summary>
    /// Loads and adds the XML files listed in the <c>gamedata.xml</c> file. Only the files in the <c>gamedata</c> directory are added.
    /// </summary>
    private void LoadGameDataXmlFile()
    {
        if (!TryCreateXmlReader(GameDataXmlFilePath, out XmlReader? xmlReader))
            return;

        using (xmlReader)
        {
            foreach (string pathValue in ReadAttributeValues(xmlReader, "Catalog", "path"))
            {
                string path = PathNormalizer.NormalizePath(pathValue, SourceConstants.DefaultModsDirectory);

                if (StartsWithDirectory(path, SourceConstants.GameDataDirectory))
                    AddXmlFile(Path.Join(BaseStormDataPath, path));
            }
        }
    }

    /// <summary>
    /// Loads the <c>stormmods</c> listed in the <c>includes.xml</c> file and adds their containers to the <see cref="SourceContainer"/>.
    /// </summary>
    private void LoadIncludesStormMods()
    {
        if (!TryCreateXmlReader(IncludesXmlFilePath, out XmlReader? xmlReader))
            return;

        using (xmlReader)
        {
            foreach (string pathValue in ReadAttributeValues(xmlReader, "Path", "value"))
            {
                string path = Path.Join(HeroesSource.ModsDirectoryPath, PathNormalizer.NormalizePath(pathValue, SourceConstants.DefaultModsDirectory));

                IStormMod stormMod = GetStormMod(path, StormModType.Base);

                _includesStormModsCache.Add(stormMod);
                stormMod.LoadStormFiles();

                HeroesSource.SourceContainer.AddModContainer(stormMod.ModContainer);
            }
        }
    }

    /// <summary>
    /// Loads an XML file into the <see cref="ModContainer"/>. If the file does not exist, it is added as a not found file.
    /// </summary>
    /// <param name="path">The full path of the XML file.</param>
    private void AddXmlFile(string path)
    {
        StormFile stormFile = CreateStormFile(path);

        if (!TryCreateXmlReader(path, out XmlReader? xmlReader))
        {
            ModContainer.AddFileNotFound(stormFile);
            return;
        }

        using (xmlReader)
            ModContainer.LoadXmlFile(xmlReader, stormFile);
    }

    /// <summary>
    /// Adds a <c>.stormlayout</c> file to the <see cref="ModContainer"/> by its path relative to the <c>base.stormdata</c> directory (starts with <c>ui</c>).
    /// </summary>
    /// <param name="layoutFilePath">The full path of the <c>.stormlayout</c> file.</param>
    private void AddStormLayoutFilePath(string layoutFilePath)
    {
        if (TryGetPathAfterDirectory(layoutFilePath, SourceConstants.BaseStormDataDirectory, SourceConstants.UiDirectory, out ReadOnlySpan<char> uiPath))
            ModContainer.AddStormLayoutFile(PathNormalizer.NormalizePath(uiPath), CreateStormFile(layoutFilePath));
    }

    /// <summary>
    /// Adds an asset file to the <see cref="ModContainer"/> by its path relative to the <c>base.stormassets</c> directory (starts with <c>assets</c>).
    /// </summary>
    /// <param name="assetFilePath">The full path of the asset file.</param>
    private void AddAssetFilePath(string assetFilePath)
    {
        if (TryGetPathAfterDirectory(assetFilePath, SourceConstants.BaseStormAssetsDirectory, SourceConstants.AssetsDirectory, out ReadOnlySpan<char> assetsPath))
            ModContainer.AddAssetFile(PathNormalizer.NormalizePath(assetsPath), CreateStormFile(assetFilePath));
    }

    /// <summary>
    /// Gets the part of a file path that comes after a directory (e.g. <c>base.stormdata</c>), if that part starts with another directory (e.g. <c>ui</c>).
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <param name="directory">The directory to remove from the start of the path, along with everything before it.</param>
    /// <param name="startDirectory">The directory that the remaining path must start with.</param>
    /// <param name="path">When this method returns, contains the part of <paramref name="filePath"/> after <paramref name="directory"/>.</param>
    /// <returns><see langword="true"/> if the remaining path starts with <paramref name="startDirectory"/>; otherwise <see langword="false"/>.</returns>
    private bool TryGetPathAfterDirectory(string filePath, string directory, string startDirectory, out ReadOnlySpan<char> path)
    {
        path = filePath;

        // only search after the stormmod directory, so a parent directory with the same name is not matched
        int searchStart = filePath.StartsWith(DirectoryPath, StringComparison.OrdinalIgnoreCase) ? DirectoryPath.Length : 0;
        int index = filePath.IndexOf(directory, searchStart, StringComparison.OrdinalIgnoreCase);

        if (index > -1)
            path = path[(index + directory.Length)..].TrimStart(_directorySeparatorChars);

        return StartsWithDirectory(path, startDirectory);
    }

    /// <summary>
    /// Opens a file and creates an <see cref="XmlReader"/> for it. Disposing the reader also closes the file.
    /// </summary>
    /// <param name="path">The full path of the XML file.</param>
    /// <param name="xmlReader">When this method returns, contains the <see cref="XmlReader"/> of the file, if it exists; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>.</returns>
    private bool TryCreateXmlReader(string path, [NotNullWhen(true)] out XmlReader? xmlReader)
    {
        if (!TryGetFile(path, out Stream? stream))
        {
            xmlReader = null;
            return false;
        }

        xmlReader = XmlReader.Create(stream, _xmlReaderSettings);
        return true;
    }

    /// <summary>
    /// Creates a <see cref="StormFile"/> for a file in this <c>stormmod</c>.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>The <see cref="StormFile"/> with a path relative to the <c>mods</c> directory and the full path.</returns>
    private StormFile CreateStormFile(string filePath) => new()
    {
        StormModName = Name,
        RelativePath = GetRelativePath(filePath),
        FullPath = filePath,
    };

    /// <summary>
    /// Gets the path of a file relative to the <c>mods</c> directory.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>The path relative to the <c>mods</c> directory.</returns>
    private string GetRelativePath(string filePath)
    {
        // the files are almost always in this stormmod's directory, and this is a lot faster than Path.GetRelativePath
        if (filePath.Length > DirectoryPath.Length &&
            filePath.StartsWith(DirectoryPath, StringComparison.Ordinal) &&
            IsDirectorySeparator(filePath[DirectoryPath.Length]))
        {
            return string.Concat(RelativeStormModPath, filePath.AsSpan(DirectoryPath.Length));
        }

        return Path.GetRelativePath(HeroesSource.ModsDirectoryPath, filePath);
    }
}
