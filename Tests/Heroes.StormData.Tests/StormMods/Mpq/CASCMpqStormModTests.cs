using Heroes.MpqTool;
using NSubstitute.Extensions;

namespace Heroes.StormData.StormMods.Mpq.Tests;

[TestClass]
public sealed class CASCMpqStormModTests
{
    private const string ModsDirectory = "mods";

    private static readonly string _archivePath = Path.Combine(ModsDirectory, "test.stormmod", "test.s2ma");
    private static readonly string _mapArchivePath = Path.Combine(ModsDirectory, "core.stormmod", "base.stormdata", "depotcache", "8d", "55", "8d554.s2ma");
    private static readonly byte[] _testArchiveData = File.ReadAllBytes(Path.Join("TestFiles", "Mpq", "test.s2ma"));
    private static readonly byte[] _mapArchiveData = File.ReadAllBytes(Path.Join("TestFiles", "DepotCache", "8d554.s2ma"));

    private readonly SourceContainer _sourceContainer = new();
    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly ICASCHeroesSource _heroesSource = Substitute.For<ICASCHeroesSource>();
    private readonly ICASCHeroesStorage _cascHeroesStorage = Substitute.For<ICASCHeroesStorage>();
    private readonly ICASCHandlerWrapper _cascHandlerWrapper = Substitute.For<ICASCHandlerWrapper>();
    private readonly List<MemoryStream> _openedStreams = [];

    public CASCMpqStormModTests()
    {
        _cascHeroesStorage.CASCHandlerWrapper.Returns(_cascHandlerWrapper);

        _heroesSource.ModsDirectoryPath.Returns(ModsDirectory);
        _heroesSource.SourceContainer.Returns(_sourceContainer);
        _heroesSource.StormModFactory.Returns(_stormModFactory);
        _heroesSource.CASCHeroesStorage.Returns(_cascHeroesStorage);

        // like the CASC storage, opening a missing file throws (CASCConfig.ThrowOnFileNotFound)
        _cascHandlerWrapper.OpenFile(Arg.Any<string>()).Returns(x => throw new FileNotFoundException("File not found in the CASC storage.", x.Arg<string>()));
    }

    [TestMethod]
    public void Constructor_ArchivePath_SetsProperties()
    {
        // act
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // assert
        cascMpqStormMod.Name.Should().Be("test");
        cascMpqStormMod.DirectoryPath.Should().Be(_archivePath);
        cascMpqStormMod.RelativeStormModPath.Should().Be(Path.Combine("test.stormmod", "test.s2ma"));
        cascMpqStormMod.ModType.Should().Be(StormModType.Base);
        cascMpqStormMod.ModContainer.StormModType.Should().Be(StormModType.Base);
    }

    [TestMethod]
    public void Constructor_HasName_SetsNameInsteadOfFileName()
    {
        // act
        CASCMpqStormMod cascMpqStormMod = new(_heroesSource, _mapArchivePath, StormModType.Map, "Volskaya Foundry");

        // assert
        cascMpqStormMod.Name.Should().Be("Volskaya Foundry");
        cascMpqStormMod.DirectoryPath.Should().Be(_mapArchivePath);
        cascMpqStormMod.ModType.Should().Be(StormModType.Map);
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_LoadsXmlFilesInGameDataDirectory()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        // the archive also has Base.StormData\GameData\Other\BehaviorData.xml, but no gamedata.xml file that lists it
        cascMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_archivePath, "Base.StormData", "GameData", "ActorData.xml")));
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_AddsAssetTexts()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        cascMpqStormMod.ModContainer.LoadedAssetsTextFiles.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(ExpectedStormFile(Path.Combine(_archivePath, "base.stormdata", "gamedata", "assets.txt")));
        GetDataStorage(StormModType.Base).TextCache.AssetTextsById.Dictionary.Keys.Should().Equal("id1");
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_AddsRelativeUiPathsOfStormLayoutFiles()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        string layoutDirectoryPath = Path.Combine(_archivePath, "Base.StormData", "UI", "Layout");

        var layoutFiles = GetDataStorage(StormModType.Base).FileCache.StormLayoutFilesByRelativePath.Dictionary;
        layoutFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("ui", "layout", "descindex.stormlayout"),
            Path.Combine("ui", "layout", "glue", "addfrienddialog.stormlayout"));
        layoutFiles[Path.Combine("ui", "layout", "descindex.stormlayout")].Should().BeEquivalentTo(
            ExpectedStormFile(Path.Combine(layoutDirectoryPath, "DescIndex.StormLayout")));
        layoutFiles[Path.Combine("ui", "layout", "glue", "addfrienddialog.stormlayout")].Should().BeEquivalentTo(
            ExpectedStormFile(Path.Combine(layoutDirectoryPath, "Glue", "AddFriendDialog.StormLayout")));
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_AddsRelativeAssetsPathsOfDdsFiles()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        string texturesDirectoryPath = Path.Combine(_archivePath, "Base.StormAssets", "Assets", "Textures");

        cascMpqStormMod.ModContainer.LoadedAssetFiles.Select(x => x.FullPath).Should().Equal(
            Path.Combine(texturesDirectoryPath, "075.dds"),
            Path.Combine(texturesDirectoryPath, "33.dds"));
        GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary.Keys.Should().BeEquivalentTo(
            Path.Combine("assets", "textures", "075.dds"),
            Path.Combine("assets", "textures", "33.dds"));
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_EveryStormFileHasMpqFileAndEntryPaths()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        StormModContainer modContainer = cascMpqStormMod.ModContainer;
        List<StormFile> stormFiles = [.. modContainer.LoadedXmlFiles, .. modContainer.LoadedAssetsTextFiles, .. modContainer.LoadedStormLayoutFiles, .. modContainer.LoadedAssetFiles];

        stormFiles.Should().HaveCount(6).And.AllSatisfy(stormFile =>
        {
            stormFile.IsInMpqArchive.Should().BeTrue();
            stormFile.MpqFilePath.Should().Be(_archivePath);
            stormFile.MpqEntryPath.Should().Be(stormFile.FullPath[(_archivePath.Length + 1)..]);
        });
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_OpensArchiveOnceAndClosesItWhenDone()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        _cascHandlerWrapper.Received(1).OpenFile(_archivePath);
        _openedStreams.Should().ContainSingle().Which.CanRead.Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormFiles_MapStormModType_LoadsIntoMapDataStorageOnly()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath, StormModType.Map);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        DataStorage mapDataStorage = GetDataStorage(StormModType.Map);
        mapDataStorage.TextCache.AssetTextsById.Dictionary.Should().ContainKey("id1");
        mapDataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().HaveCount(2);
        mapDataStorage.FileCache.AssetFilesByRelativePath.Dictionary.Should().HaveCount(2);

        DataStorage baseDataStorage = GetDataStorage(StormModType.Base);
        baseDataStorage.TextCache.AssetTextsById.Dictionary.Should().BeEmpty();
        baseDataStorage.FileCache.StormLayoutFilesByRelativePath.Dictionary.Should().BeEmpty();
        baseDataStorage.FileCache.AssetFilesByRelativePath.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_HasName_StormFilesHaveName()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = new(_heroesSource, _archivePath, StormModType.Base, "Test Map");

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        cascMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.StormModName.Should().Be("Test Map");
    }

    [TestMethod]
    public void LoadStormFiles_ArchiveNotInCASC_LoadsNothingAndDoesNotThrow()
    {
        // arrange
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        Action act = cascMpqStormMod.LoadStormFiles;

        // assert
        act.Should().NotThrow();

        StormModContainer modContainer = cascMpqStormMod.ModContainer;
        modContainer.LoadedXmlFiles.Should().BeEmpty();
        modContainer.LoadedAssetsTextFiles.Should().BeEmpty();
        modContainer.LoadedAssetFiles.Should().BeEmpty();
        modContainer.LoadedStormLayoutFiles.Should().BeEmpty();
        _cascHandlerWrapper.DidNotReceiveWithAnyArgs().OpenFile(default!);
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_ArchiveExistsButCannotBeOpened_LoadsNothing()
    {
        // arrange
        _cascHandlerWrapper.FileExists(_archivePath).Returns(true);
        _cascHandlerWrapper.Configure().OpenFile(_archivePath).Returns((Stream?)null);

        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        cascMpqStormMod.ModContainer.LoadedXmlFiles.Should().BeEmpty();
        cascMpqStormMod.ModContainer.LoadedAssetFiles.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_FileIsNotMpqArchive_ThrowsExceptionAndClosesFile()
    {
        // arrange
        AddCASCFile(_archivePath, new byte[64]);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);

        // act
        Action act = cascMpqStormMod.LoadStormFiles;

        // assert
        act.Should().Throw<MpqHeroesToolException>();
        _openedStreams.Should().ContainSingle().Which.CanRead.Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormFiles_CalledAfterGetMapDependencies_ReopensArchiveAndLoadsFiles()
    {
        // arrange
        AddCASCFile(_archivePath, _testArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_archivePath);
        cascMpqStormMod.GetMapDependencies();

        // act
        cascMpqStormMod.LoadStormFiles();

        // assert
        cascMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle();
        cascMpqStormMod.ModContainer.LoadedStormLayoutFiles.Should().HaveCount(2);
        cascMpqStormMod.ModContainer.LoadedAssetFiles.Should().HaveCount(2);
        _openedStreams.Should().HaveCount(2).And.OnlyContain(x => !x.CanRead);
    }

    [TestMethod]
    public void LoadStormGameStrings_MapArchiveHasGameStrings_AddsGameStringsWithMpqPaths()
    {
        // arrange
        AddCASCFile(_mapArchivePath, _mapArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        cascMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        StormFile stormFile = cascMpqStormMod.ModContainer.LoadedGameStringFiles.Should().ContainSingle().Subject;

        stormFile.FullPath.Should().Be(Path.Combine(_mapArchivePath, "enus.stormdata", "localizeddata", "gamestrings.txt"));
        stormFile.MpqFilePath.Should().Be(_mapArchivePath);
        stormFile.MpqEntryPath.Should().Be(Path.Combine("enus.stormdata", "localizeddata", "gamestrings.txt"));

        GetDataStorage(StormModType.Map).TextCache.GameStringsById.Dictionary.Should().NotBeEmpty();
        GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
        _openedStreams.Should().ContainSingle().Which.CanRead.Should().BeFalse();
    }

    [TestMethod]
    public void LoadStormGameStrings_ArchiveNotInCASC_LoadsNothingAndDoesNotThrow()
    {
        // arrange
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        Action act = () => cascMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        act.Should().NotThrow();
        cascMpqStormMod.ModContainer.LoadedGameStringFiles.Should().BeEmpty();
        GetDataStorage(StormModType.Map).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void GetMapDependencies_MapArchiveHasDocumentInfo_ReturnsDependencies()
    {
        // arrange
        AddCASCFile(_mapArchivePath, _mapArchiveData);
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = cascMpqStormMod.GetMapDependencies();

        // assert
        result.Should().Equal(new MapDependency()
        {
            BnetName = "Volskaya Sound",
            BnetVersionMajor = 0,
            BnetVersionMinor = 0,
            BnetNamespace = 1146,
            LocalFile = Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"),
        });
        _openedStreams.Should().ContainSingle().Which.CanRead.Should().BeFalse();
    }

    [TestMethod]
    public void GetMapDependencies_ArchiveNotInCASC_ReturnsEmptyAndDoesNotThrow()
    {
        // arrange
        CASCMpqStormMod cascMpqStormMod = CreateCASCMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        IReadOnlyList<MapDependency> result = cascMpqStormMod.GetMapDependencies();

        // assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Gets the expected <see cref="StormFile"/> values for a file in the <c>test.s2ma</c> archive.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns>An object with the expected <see cref="StormFile"/> values.</returns>
    private static object ExpectedStormFile(string fullPath) => new
    {
        StormModName = "test",
        RelativePath = fullPath[(ModsDirectory.Length + 1)..],
        FullPath = fullPath,
    };

    private CASCMpqStormMod CreateCASCMpqStormMod(string archivePath, StormModType stormModType = StormModType.Base) =>
        new(_heroesSource, archivePath, stormModType);

    private DataStorage GetDataStorage(StormModType stormModType) => _sourceContainer.LayeredDataStorage.GetDataStorage(stormModType);

    /// <summary>
    /// Adds a file to the CASC storage. Each time it is opened, a new stream is returned and recorded in <see cref="_openedStreams"/>.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    /// <param name="data">The content of the file.</param>
    private void AddCASCFile(string filePath, byte[] data)
    {
        _cascHandlerWrapper.FileExists(filePath).Returns(true);

        // Configure() so the throwing OpenFile set up in the constructor is not called while setting this up
        _cascHandlerWrapper.Configure().OpenFile(filePath).Returns(_ =>
        {
            MemoryStream stream = new(data, writable: false);
            _openedStreams.Add(stream);

            return stream;
        });
    }
}
