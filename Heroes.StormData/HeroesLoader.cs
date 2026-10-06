namespace Heroes.StormData;

public class HeroesLoader
{
    /// <summary>
    /// The product name for Heroes of the Storm.
    /// </summary>
    public const string ProductName = "hero";

    /// <summary>
    /// The product name for Heroes of the Storm PTR.
    /// </summary>
    public const string ProductPtrName = "herot";

    private readonly IHeroesSource _heroesSource;

    //private HeroesLoader()
    //{
    //}

    private HeroesLoader(string rootDirectory)
    {
        _heroesSource = new FileHeroesSource(rootDirectory, new StormModFactory());
    }

    private HeroesLoader(CASCHeroesStorage cascStorage, CASCConfig cascConfig, IProgressReporter? progressReporter)
    {
    }

    //public static HeroesLoader LoadWithEmpty(string? rootDirectory = null)
    //{
    //    return new HeroesLoader(rootDirectory ?? string.Empty);
    //}

    public static CASCConfig GetCASCConfig(string pathToHeroesDirectory, ILoggerOptions? loggerOptions = null)
    {
        return CASCConfig.LoadLocalStorageConfig(pathToHeroesDirectory, ProductName, loggerOptions ?? new HeroesLoggerOptions());
    }

    public static CASCConfig GetOnlineCASCConfig(HttpClient httpClient, bool isPtr = false, ILoggerOptions? loggerOptions = null)
    {
        return CASCConfig.LoadOnlineStorageConfig(isPtr ? ProductPtrName : ProductName, "us", httpClient, false, loggerOptions ?? new HeroesLoggerOptions());
    }

    public static HeroesLoader LoadWithFile(string rootDirectory)
    {
        return new HeroesLoader(rootDirectory);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cascConfig"></param>
    /// <param name="httpClient"></param>
    /// <param name="progressReporter"></param>
    /// <returns></returns>
    public static HeroesLoader LoadWithCASC(CASCConfig cascConfig, HttpClient httpClient, IProgressReporter? progressReporter = null)
    {
        return LoadAsCASCInternal(cascConfig, httpClient, progressReporter);
    }

    public HeroesLoader LoadStormMods()
    {
        //if (_baseStormModsLoaded is false)
        //{
            _heroesSource.LoadStormMods();
            //_heroesSource.LoadDepotCache();
        //}

        //_baseStormModsLoaded = true;

        return this;
    }

    private static HeroesLoader LoadAsCASCInternal(CASCConfig cascConfig, HttpClient httpClient, IProgressReporter? progressReporter = null)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;

        CASCHandler cascHandler = CASCHandler.OpenStorage(cascConfig, httpClient: httpClient, worker: (ProgressReporter?)progressReporter);
        cascHandler.Root.LoadListFile(string.Empty, (ProgressReporter?)progressReporter);

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

        return new HeroesLoader(new CASCHeroesStorage(cascHandler, cascFolderRoot), cascConfig, progressReporter);
    }
}
