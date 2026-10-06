namespace Heroes.StormData.StormMods;

/// <summary>
/// Represents a base class for a <c>stormmod</c> directory.
/// </summary>
/// <typeparam name="T">The type of the <see cref="IHeroesSource"/> source.</typeparam>
internal abstract class StormModBase<T> : IStormMod
    where T : IHeroesSource
{
    protected const string XmlFileExtension = ".xml";

    private static readonly XmlReaderSettings _xmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = false,
    };

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
    }

    public string Name => Path.GetFileNameWithoutExtension(DirectoryPath);

    public string RelativeStormModPath => Path.GetRelativePath(HeroesSource.ModsDirectoryPath, DirectoryPath);

    public string DirectoryPath { get; }

    public StormModType ModType { get; }

    public StormModContainer ModContainer { get; }

    protected T HeroesSource { get; }

    /// <summary>
    /// Gets the path of the <c>GameData</c> directory.
    /// </summary>
    protected virtual string GameDataDirectoryPath => Path.Combine(DirectoryPath, SourceConstants.BaseStormDataDirectory, SourceConstants.GameDataDirectory);

    public virtual void LoadStormFiles()
    {
        LoadBuildIdFile();
        //LoadFontStyleFile();
        //LoadStormLayoutDirectory();
        //LoadAssetsDirectory();
        //LoadAssetsTextFile();

        LoadGameDataDirectory();
        //LoadGameDataXmlFile();

        //LoadIncludesStormMods();
    }

    public void LoadBuildIdFile()
    {
        //if (!TryGetFile(BuildIdFilePath, out Stream? stream))
        //    return;

        //StormModStorage.AddBuildIdFile(stream);
    }

    /// <summary>
    /// Loads the XML files from the <see cref="GameDataDirectoryPath"/>. Only this directory not the subdirectories.
    /// The subdirectories are loaded from <see cref="LoadGameDataXmlFile"/>.
    /// </summary>
    public abstract void LoadGameDataDirectory();

    protected void LoadGameDataFiles(IEnumerable<string> files)
    {
        foreach (string file in files)
        {
            AddXmlFile(file);
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
