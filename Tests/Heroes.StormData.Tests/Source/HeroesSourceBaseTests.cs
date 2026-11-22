using Heroes.MpqTool;
using System.Diagnostics.CodeAnalysis;

namespace Heroes.StormData.Source.Tests;

[TestClass]
public sealed class HeroesSourceBaseTests
{
    private const string MapTitle = "Volskaya Foundry";

    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
    private static readonly string _mapRelativePath = Path.Combine("core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma");
    private static readonly string _dependencyLocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod");

    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly IDepotCache _depotCache = Substitute.For<IDepotCache>();
    private readonly DepotCacheData _depotCacheData = new();
    private readonly TestHeroesSource _heroesSource;

    public HeroesSourceBaseTests()
    {
        _depotCache.Data.Returns(_depotCacheData);
        _heroesSource = new TestHeroesSource(_stormModFactory, _depotCache);
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_CreatesMapRootWithTitleAndFullPath()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        _heroesSource.LoadStormMapMod(MapTitle);

        // assert
        _heroesSource.MpqStormModRequests.Should().ContainSingle().Which.Should().Be((MapTitle, Path.Join(_modsDirectoryPath, _mapRelativePath), StormModType.Map));
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_LoadsDependenciesBeforeMapRoot()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        _heroesSource.LoadStormMapMod(MapTitle);

        // assert
        IStormMod dependencyStormMod = _heroesSource.StormModsByDirectoryPath[Path.Join(_modsDirectoryPath, _dependencyLocalFile)];
        IStormMod mapRootStormMod = _heroesSource.MapRootStormMod!;

        Received.InOrder(() =>
        {
            dependencyStormMod.LoadStormFiles();
            mapRootStormMod.LoadStormFiles();
        });

        _heroesSource.SourceContainer.StormModContainers.Should().Equal(dependencyStormMod.ModContainer, mapRootStormMod.ModContainer);
    }

    [TestMethod]
    public void StormMapMods_MapLoaded_ReturnsDependenciesThenMapRoot()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        _heroesSource.LoadStormMapMod(MapTitle);

        // assert
        IStormMod dependencyStormMod = _heroesSource.StormModsByDirectoryPath[Path.Join(_modsDirectoryPath, _dependencyLocalFile)];

        _heroesSource.StormMapMods.Should().Equal(dependencyStormMod, _heroesSource.MapRootStormMod!);
    }

    [TestMethod]
    public void StormMapMods_MapUnloaded_ReturnsEmpty()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();
        _heroesSource.LoadStormMapMod(MapTitle);

        // act
        _heroesSource.UnloadStormMapMod();

        // assert
        _heroesSource.StormMapMods.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleNotFound_LoadsNothing()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        _heroesSource.LoadStormMapMod("Unknown Map");

        // assert
        _heroesSource.MpqStormModRequests.Should().BeEmpty();
        _heroesSource.SourceContainer.StormModContainers.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormMapMod_DepotCacheNotLoaded_LoadsNothing()
    {
        // arrange
        AddVolskayaMap();

        // act
        _heroesSource.LoadStormMapMod(MapTitle);

        // assert
        _heroesSource.MpqStormModRequests.Should().BeEmpty();
        _heroesSource.SourceContainer.StormModContainers.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleFound_ReturnsTrue()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        bool result = _heroesSource.LoadStormMapMod(MapTitle);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleNotFound_ReturnsFalse()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        bool result = _heroesSource.LoadStormMapMod("Unknown Map");

        // assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormMapMod_MapTitleNotFoundWithMapLoaded_KeepsLoadedMap()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();
        _heroesSource.LoadStormMapMod(MapTitle);

        List<StormModContainer> loadedContainers = [.. _heroesSource.SourceContainer.StormModContainers];

        // act
        _heroesSource.LoadStormMapMod("Unknown Map");

        // assert
        _heroesSource.SourceContainer.StormModContainers.Should().Equal(loadedContainers);
    }

    [TestMethod]
    public void UnloadStormMapMod_MapLoaded_RemovesMapContainers()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();
        _heroesSource.LoadStormMapMod(MapTitle);

        // act
        _heroesSource.UnloadStormMapMod();

        // assert
        _heroesSource.SourceContainer.StormModContainers.Should().BeEmpty();
    }

    [TestMethod]
    public void GetMapTitles_DepotCacheLoaded_ReturnsTitles()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        IEnumerable<string> result = _heroesSource.GetMapTitles();

        // assert
        result.Should().ContainSingle().Which.Should().Be(MapTitle);
    }

    [TestMethod]
    public void GetMapTitles_DepotCacheNotLoaded_ReturnsEmpty()
    {
        // arrange
        AddVolskayaMap();

        // act
        IEnumerable<string> result = _heroesSource.GetMapTitles();

        // assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void TryGetS2MAProperties_DepotCacheLoadedDifferentCase_ReturnsProperties()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();

        // act
        bool result = _heroesSource.TryGetS2MAProperties(MapTitle.ToUpperInvariant(), out S2MAProperties? s2maProperties);

        // assert
        result.Should().BeTrue();
        s2maProperties.Should().BeSameAs(_depotCacheData.S2MAPropertiesByTitle[MapTitle]);
    }

    [TestMethod]
    public void TryGetS2MAProperties_DepotCacheNotLoaded_ReturnsFalse()
    {
        // arrange
        AddVolskayaMap();

        // act
        bool result = _heroesSource.TryGetS2MAProperties(MapTitle, out S2MAProperties? s2maProperties);

        // assert
        result.Should().BeFalse();
        s2maProperties.Should().BeNull();
    }

    [TestMethod]
    public void LoadGamestrings_HasBaseAndMapStormMods_LoadsGameStringsOfBaseThenMapStormMods()
    {
        // arrange
        AddVolskayaMap();
        _heroesSource.LoadDepotCache();
        _heroesSource.LoadStormMapMod(MapTitle);

        IStormMod coreStormMod = _heroesSource.StormModsByDirectoryPath[Path.Combine(_modsDirectoryPath, "core.stormmod")];
        IStormMod heroesStormMod = _heroesSource.StormModsByDirectoryPath[Path.Combine(_modsDirectoryPath, "heroes.stormmod")];
        IStormMod heroesDataStormMod = _heroesSource.StormModsByDirectoryPath[Path.Combine(_modsDirectoryPath, "heroesdata.stormmod")];
        IStormMod dependencyStormMod = _heroesSource.StormModsByDirectoryPath[Path.Join(_modsDirectoryPath, _dependencyLocalFile)];
        IStormMod mapRootStormMod = _heroesSource.MapRootStormMod!;

        // act
        _heroesSource.LoadGamestrings(StormLocale.DEDE);

        // assert
        Received.InOrder(() =>
        {
            coreStormMod.LoadStormGameStrings(StormLocale.DEDE);
            heroesStormMod.LoadStormGameStrings(StormLocale.DEDE);
            heroesDataStormMod.LoadStormGameStrings(StormLocale.DEDE);
            dependencyStormMod.LoadStormGameStrings(StormLocale.DEDE);
            mapRootStormMod.LoadStormGameStrings(StormLocale.DEDE);
        });
    }

    [TestMethod]
    public void LoadGamestrings_HasLoadedGameStrings_ClearsThemFirst()
    {
        // arrange
        TextCache textCache = _heroesSource.SourceContainer.LayeredDataStorage.GetDataStorage(StormModType.Base).TextCache;
        textCache.GameStringsById.Dictionary["id1"] = new StringValue("value1", StormFileHelper.CreateStormFile());

        // act
        _heroesSource.LoadGamestrings(StormLocale.ENUS);

        // assert
        textCache.GameStringsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(16)]
    [DataRow(64)]
    public void FileExists_MpqEntryInFileThatIsNotMpqArchive_ReturnsFalseAndClosesFile(int fileLength)
    {
        // arrange
        MemoryStream file = new(new byte[fileLength]);
        _heroesSource.OpenedFile = file;

        // act
        bool result = _heroesSource.FileExists("file.s2ma", "entry.xml");

        // assert
        result.Should().BeFalse();
        file.CanRead.Should().BeFalse();
    }

    [TestMethod]
    public void GetFile_MpqEntryInFileThatIsNotMpqArchive_ThrowsExceptionAndClosesFile()
    {
        // arrange
        MemoryStream file = new(new byte[64]);
        _heroesSource.OpenedFile = file;

        // act
        Action act = () => _heroesSource.GetFile("file.s2ma", "entry.xml");

        // assert
        act.Should().Throw<MpqHeroesToolException>();
        file.CanRead.Should().BeFalse();
    }

    private void AddVolskayaMap()
    {
        _depotCacheData.S2MAPropertiesByTitle.Add(MapTitle, new S2MAProperties()
        {
            DirectoryPath = _mapRelativePath,
            MapDependencies =
            [
                new MapDependency()
                {
                    BnetName = "Volskaya Sound",
                    BnetVersionMajor = 0,
                    BnetVersionMinor = 0,
                    BnetNamespace = 1146,
                    LocalFile = _dependencyLocalFile,
                },
            ],
        });
    }

    /// <summary>
    /// A <see cref="HeroesSourceBase"/> that creates substitute <c>stormmods</c> and records what it was asked to create.
    /// </summary>
    private sealed class TestHeroesSource : HeroesSourceBase
    {
        private readonly IDepotCache _depotCache;

        public TestHeroesSource(IStormModFactory stormModFactory, IDepotCache depotCache)
            : base(_modsDirectoryPath, stormModFactory)
        {
            _depotCache = depotCache;
        }

        public Dictionary<string, IStormMod> StormModsByDirectoryPath { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<(string Name, string DirectoryPath, StormModType StormModType)> MpqStormModRequests { get; } = [];

        public IStormMod? MapRootStormMod { get; private set; }

        /// <summary>
        /// Gets or sets the stream returned for any file that is opened, or <see langword="null"/> for no file.
        /// </summary>
        public Stream? OpenedFile { get; set; }

        protected override IStormMod GetStormMod(string directoryPath, StormModType stormModType)
        {
            // also called by the base constructor, which runs after this class's property initializers
            IStormMod stormMod = CreateStormMod(directoryPath, stormModType);
            StormModsByDirectoryPath[directoryPath] = stormMod;

            return stormMod;
        }

        protected override IStormMod GetMpqStormMod(string name, string directoryPath, StormModType stormModType)
        {
            MpqStormModRequests.Add((name, directoryPath, stormModType));
            MapRootStormMod = CreateStormMod(directoryPath, stormModType);

            return MapRootStormMod;
        }

        protected override IDepotCache CreateDepotCache() => _depotCache;

        protected override bool FileExistsCore(string fullPath) => false;

        protected override bool TryOpenFile(string fullPath, [NotNullWhen(true)] out Stream? stream)
        {
            stream = OpenedFile;
            return stream is not null;
        }

        private IStormMod CreateStormMod(string directoryPath, StormModType stormModType)
        {
            IStormMod stormMod = Substitute.For<IStormMod>();
            stormMod.DirectoryPath.Returns(directoryPath);
            stormMod.ModContainer.Returns(new StormModContainer(SourceContainer, stormModType));
            stormMod.GetMapDependencies().Returns([]);

            return stormMod;
        }
    }
}
