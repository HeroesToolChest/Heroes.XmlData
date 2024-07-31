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
    private readonly IBackgroundWorkerEx _backgroundWorkerEx;

    public CASCMpqStormModTests()
    {
        _stormModFactory = Substitute.For<IStormModFactory>();
        _depotCacheFactory = Substitute.For<IDepotCacheFactory>();
        _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
        _backgroundWorkerEx = Substitute.For<IBackgroundWorkerEx>();
    }

    [TestMethod]
    public void LoadStormData_HasS2maFile_LoadsData()
    {
        // arrange
        StormStorage stormStorage = new(false);

        CASCHeroesSource cascHeroesSource = new(stormStorage, _stormModFactory, _depotCacheFactory, _cascHeroesStorage, _backgroundWorkerEx);
        CASCMpqStormMod cascMpqStormMod = new(cascHeroesSource, Path.Join("test.stormmod", "depotcache", "test.s2ma"), StormModType.Normal);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CActor default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("ActorData.xml"),
    true);

        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "test.stormmod", "depotcache", "test.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "test.s2ma")));

        // act
        cascMpqStormMod.LoadStormData();

        // assert
        cascMpqStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(3);
        cascMpqStormMod.StormModStorage.FoundLayoutFilePaths.Should().HaveCount(2);
        cascMpqStormMod.StormModStorage.AddedAssetsFilePaths.Should().ContainSingle();
    }
}
