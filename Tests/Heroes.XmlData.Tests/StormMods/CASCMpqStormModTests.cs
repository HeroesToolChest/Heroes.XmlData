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
        CASCMpqStormMod cascMpqStormMod = new(cascHeroesSource, Path.Join("test.stormmod", "depotcache", "8d554.s2ma"), StormModType.Normal);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
                    XDocument.Parse(
                    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CGameUI default=""1"" />
</Catalog>
"),
                    TestHelpers.GetStormPath("GameUIData.xml"),
                    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
            XDocument.Parse(
            @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CTerrain default=""1"" />
</Catalog>
"),
            TestHelpers.GetStormPath("TerrainData.xml"),
            true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CLight default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("LightData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CBehavior default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("BehaviorData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CSound default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("SoundData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CActor default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("ActorData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CWater default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("WaterData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CUnit default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("UnitData.xml"),
    true);

        cascMpqStormMod.StormModStorage.AddXmlDataFile(
    XDocument.Parse(
    @"<?xml version=""1.0"" encoding=""us-ascii""?>
<Catalog>
  <CScoreResult default=""1"" id=""default"" />
</Catalog>
"),
    TestHelpers.GetStormPath("ScoreResultData.xml"),
    true);

        _cascHeroesStorage.CASCHandlerWrapper.OpenFile(Path.Join("mods", "test.stormmod", "depotcache", "8d554.s2ma")).Returns(File.OpenRead(Path.Join(TestFilesFolder, "8d554.s2ma")));

        // act
        cascMpqStormMod.LoadStormData();

        // assert
        cascMpqStormMod.StormModStorage.AddedXmlDataFilePaths.Should().HaveCount(18);
    }
}
