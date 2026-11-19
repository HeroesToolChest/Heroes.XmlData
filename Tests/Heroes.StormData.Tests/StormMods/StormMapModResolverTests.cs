namespace Heroes.StormData.StormMods.Tests;

[TestClass]
internal sealed class StormMapModResolverTests
{
    private readonly Dictionary<string, IStormMod> _stormModsByLocalFile = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _createdLocalFiles = [];
    private readonly IStormMod _mapRootStormMod = CreateStormMod("mapsroot.s2ma");

    [TestMethod]
    public void Resolve_NoDependencies_ReturnsOnlyMapRoot()
    {
        // arrange
        StormMapModResolver resolver = CreateResolver();

        // act
        List<IStormMod> result = [.. resolver.Resolve(_mapRootStormMod, [])];

        // assert
        result.Should().ContainSingle().Which.Should().BeSameAs(_mapRootStormMod);
    }

    [TestMethod]
    public void Resolve_DirectDependencies_ReturnsDependenciesReversedThenMapRoot()
    {
        // arrange
        AddStormMod("a.stormmod");
        AddStormMod("b.stormmod");
        StormMapModResolver resolver = CreateResolver();

        // act
        List<IStormMod> result = [.. resolver.Resolve(_mapRootStormMod, [CreateDependency("a.stormmod"), CreateDependency("b.stormmod")])];

        // assert
        result.Select(x => x.DirectoryPath).Should().Equal("b.stormmod", "a.stormmod", "mapsroot.s2ma");
    }

    [TestMethod]
    public void Resolve_NestedDependencies_ReturnsNestedDependenciesBeforeTheirDependents()
    {
        // arrange
        AddStormMod("a.stormmod", "c.stormmod");
        AddStormMod("c.stormmod", "d.stormmod");
        AddStormMod("d.stormmod");
        StormMapModResolver resolver = CreateResolver();

        // act
        List<IStormMod> result = [.. resolver.Resolve(_mapRootStormMod, [CreateDependency("a.stormmod")])];

        // assert
        result.Select(x => x.DirectoryPath).Should().Equal("d.stormmod", "c.stormmod", "a.stormmod", "mapsroot.s2ma");
    }

    [TestMethod]
    public void Resolve_SharedDependency_KeepsOnlyFirstOccurrence()
    {
        // arrange
        AddStormMod("a.stormmod", "c.stormmod");
        AddStormMod("b.stormmod", "C.STORMMOD");
        AddStormMod("c.stormmod");
        StormMapModResolver resolver = CreateResolver();

        // act
        List<IStormMod> result = [.. resolver.Resolve(_mapRootStormMod, [CreateDependency("a.stormmod"), CreateDependency("b.stormmod")])];

        // assert
        result.Select(x => x.DirectoryPath).Should().Equal("c.stormmod", "b.stormmod", "a.stormmod", "mapsroot.s2ma");
    }

    [TestMethod]
    public void Resolve_HeroesDataDependency_IsExcluded()
    {
        // arrange
        string heroesDataLocalFile = Path.Combine("mods", SourceConstants.HeroesDataStormModDirectory);
        AddStormMod("a.stormmod", heroesDataLocalFile);
        AddStormMod(heroesDataLocalFile);
        StormMapModResolver resolver = CreateResolver();

        // act
        List<IStormMod> result = [.. resolver.Resolve(_mapRootStormMod, [CreateDependency("a.stormmod"), CreateDependency(heroesDataLocalFile)])];

        // assert
        result.Select(x => x.DirectoryPath).Should().Equal("a.stormmod", "mapsroot.s2ma");
    }

    [TestMethod]
    public void Resolve_Dependencies_CreatesStormModsFromLocalFile()
    {
        // arrange
        AddStormMod("a.stormmod", "c.stormmod");
        AddStormMod("c.stormmod");
        StormMapModResolver resolver = CreateResolver();

        // act
        _ = resolver.Resolve(_mapRootStormMod, [CreateDependency("a.stormmod")]).ToList();

        // assert
        _createdLocalFiles.Should().Equal("a.stormmod", "c.stormmod");
    }

    private static IStormMod CreateStormMod(string directoryPath, params string[] dependencyLocalFiles)
    {
        IStormMod stormMod = Substitute.For<IStormMod>();
        stormMod.DirectoryPath.Returns(directoryPath);
        stormMod.GetMapDependencies().Returns([.. dependencyLocalFiles.Select(CreateDependency)]);

        return stormMod;
    }

    private static MapDependency CreateDependency(string localFile) => new()
    {
        BnetName = string.Empty,
        BnetVersionMajor = 0,
        BnetVersionMinor = 0,
        BnetNamespace = 0,
        LocalFile = localFile,
    };

    private void AddStormMod(string localFile, params string[] dependencyLocalFiles) =>
        _stormModsByLocalFile.Add(localFile, CreateStormMod(localFile, dependencyLocalFiles));

    private StormMapModResolver CreateResolver() => new(localFile =>
    {
        _createdLocalFiles.Add(localFile);
        return _stormModsByLocalFile[localFile];
    });
}
