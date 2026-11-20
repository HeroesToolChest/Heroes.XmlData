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
