using CASCLib;
using Heroes.XmlData.CASC;

namespace Heroes.XmlData.Tests.StormMods;

[TestClass]
public class CASCMpqStormModTests
{
    private const string TestFilesFolder = "TestFiles";

    private readonly IStormModFactory _stormModFactory;
    private readonly IDepotCacheFactory _depotCacheFactory;
    private readonly ICASCHeroesStorage _cascHeroesStorage;
    private readonly IProgressReporter _progressReporter;

    public CASCMpqStormModTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _progressReporter = Substitute.For<IProgressReporter>();
    }

    [TestMethod]
    public void LoadStormData_HasS2maFile_LoadsData()
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _progressReporter);
        CASCMpqStormMod cascMpqStormMod = new(cascHeroesSource, Path.Join("test.stormmod", "depotcache", "test.s2ma"), StormModType.Normal);

        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "test.stormmod", "depotcache", "test.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "test.s2ma")));

        // act
        cascMpqStormMod.LoadStormData();

        // assert
        cascMpqStormMod.StormModStorage.AddedXmlDataFilePaths.Should().ContainSingle();
        cascMpqStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(2);
        cascMpqStormMod.StormModStorage.AddedAssetsTextFilePaths.Should().ContainSingle();
        cascMpqStormMod.StormModStorage.FoundAssetFilePaths.Should().HaveCount(2);
    }
}
