using Heroes.MpqTool;
using System.IO.Abstractions;

namespace Heroes.StormData.StormMods.Mpq.Tests;

[TestClass]
public sealed class FileMpqStormModTests
{
    private static readonly string _modsDirectoryPath = MockUnixSupport.Path(@"C:\Heroes\mods");
    private static readonly string _archivePath = Path.Combine(_modsDirectoryPath, "test.stormmod", "test.s2ma");
    private static readonly string _mapArchivePath = Path.Combine(_modsDirectoryPath, "heroesmapmods", "battlegroundmapmods", "volskaya.stormmap");

    private readonly MockFileSystem _fileSystem = new();
    private readonly SourceContainer _sourceContainer = new();
    private readonly IStormModFactory _stormModFactory = Substitute.For<IStormModFactory>();
    private readonly IFileHeroesSource _heroesSource = Substitute.For<IFileHeroesSource>();

    public FileMpqStormModTests()
    {
        _heroesSource.ModsDirectoryPath.Returns(_modsDirectoryPath);
        _heroesSource.SourceContainer.Returns(_sourceContainer);
        _heroesSource.StormModFactory.Returns(_stormModFactory);
    }

    [TestMethod]
    public void Constructor_ArchivePath_SetsProperties()
    {
        // act
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // assert
        fileMpqStormMod.Name.Should().Be("test");
        fileMpqStormMod.DirectoryPath.Should().Be(_archivePath);
        fileMpqStormMod.RelativeStormModPath.Should().Be(Path.Combine("test.stormmod", "test.s2ma"));
        fileMpqStormMod.ModType.Should().Be(StormModType.Base);
    }

    [TestMethod]
    public void Constructor_HasName_SetsNameInsteadOfFileName()
    {
        // act
        FileMpqStormMod fileMpqStormMod = new(_fileSystem, _heroesSource, _mapArchivePath, StormModType.Map, "Volskaya Foundry");

        // assert
        fileMpqStormMod.Name.Should().Be("Volskaya Foundry");
        fileMpqStormMod.DirectoryPath.Should().Be(_mapArchivePath);
    }

    [TestMethod]
    public void LoadStormFiles_HasName_StormFilesHaveName()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = new(_fileSystem, _heroesSource, _archivePath, StormModType.Base, "Test Map");

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        fileMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.StormModName.Should().Be("Test Map");
    }

    [TestMethod]
    public void LoadStormFiles_ArchiveDoesNotExist_LoadsNothing()
    {
        // arrange
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        StormModContainer modContainer = fileMpqStormMod.ModContainer;
        modContainer.LoadedXmlFiles.Should().BeEmpty();
        modContainer.LoadedAssetsTextFiles.Should().BeEmpty();
        modContainer.LoadedAssetFiles.Should().BeEmpty();
        modContainer.LoadedStormLayoutFiles.Should().BeEmpty();
        _stormModFactory.ReceivedCalls().Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_LoadsXmlFilesInGameDataDirectory()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        string expectedFilePath = Path.Combine(_archivePath, "Base.StormData", "GameData", "ActorData.xml");

        fileMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Which.Should().BeEquivalentTo(ExpectedStormFile(expectedFilePath));
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_StormFilesHaveMpqFileAndEntryPaths()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        StormFile stormFile = fileMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle().Subject;

        stormFile.IsInMpqArchive.Should().BeTrue();
        stormFile.MpqFilePath.Should().Be(_archivePath);
        stormFile.MpqEntryPath.Should().Be(Path.Combine("Base.StormData", "GameData", "ActorData.xml"));
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_EveryStormFileHasMpqFileAndEntryPaths()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        StormModContainer modContainer = fileMpqStormMod.ModContainer;
        List<StormFile> stormFiles = [.. modContainer.LoadedXmlFiles, .. modContainer.LoadedAssetsTextFiles, .. modContainer.LoadedStormLayoutFiles, .. modContainer.LoadedAssetFiles];

        stormFiles.Should().HaveCount(6).And.AllSatisfy(stormFile =>
        {
            stormFile.MpqFilePath.Should().Be(_archivePath);
            stormFile.MpqEntryPath.Should().Be(stormFile.FullPath[(_archivePath.Length + 1)..]);
        });
    }

    [TestMethod]
    public void LoadStormFiles_GameDataSubdirectoryAndNoGameDataXmlFile_DoesNotLoadSubdirectoryFiles()
    {
        // arrange
        // the archive has Base.StormData\GameData\Other\BehaviorData.xml, but no gamedata.xml file that lists it
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        fileMpqStormMod.ModContainer.LoadedXmlFiles.Should().NotContain(x => x.FullPath.Contains("BehaviorData", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void LoadStormFiles_MapStormModType_LoadsIntoMapDataStorageOnly()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath, StormModType.Map);

        // act
        fileMpqStormMod.LoadStormFiles();

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
    [DataRow(nameof(FileMpqStormMod.LoadStormFiles))]
    [DataRow(nameof(FileMpqStormMod.LoadStormGameStrings))]
    [DataRow(nameof(FileMpqStormMod.GetMapDependencies))]
    public void Method_HasArchive_ClosesArchiveFileWhenDone(string methodName)
    {
        // arrange
        // the mock file system does not track open files, so a real file is used
        DirectoryInfo modsDirectory = Directory.CreateTempSubdirectory(nameof(FileMpqStormModTests));

        try
        {
            string archivePath = Path.Combine(modsDirectory.FullName, "test.s2ma");
            File.Copy(Path.Join("TestFiles", "Mpq", "test.s2ma"), archivePath);

            _heroesSource.ModsDirectoryPath.Returns(modsDirectory.FullName);
            FileMpqStormMod fileMpqStormMod = new(new FileSystem(), _heroesSource, archivePath, StormModType.Base);

            // act
            switch (methodName)
            {
                case nameof(FileMpqStormMod.LoadStormFiles):
                    fileMpqStormMod.LoadStormFiles();
                    break;
                case nameof(FileMpqStormMod.LoadStormGameStrings):
                    fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);
                    break;
                default:
                    fileMpqStormMod.GetMapDependencies();
                    break;
            }

            // assert
            // opening the file without sharing fails if the archive still has it open
            Action act = () => File.Open(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose();
            act.Should().NotThrow();
        }
        finally
        {
            modsDirectory.Delete(true);
        }
    }

    [TestMethod]
    public void LoadStormFiles_FileIsNotMpqArchive_ThrowsExceptionAndClosesFile()
    {
        // arrange
        // the mock file system does not track open files, so a real file is used
        DirectoryInfo modsDirectory = Directory.CreateTempSubdirectory(nameof(FileMpqStormModTests));

        try
        {
            string archivePath = Path.Combine(modsDirectory.FullName, "test.s2ma");
            File.WriteAllBytes(archivePath, new byte[64]);

            _heroesSource.ModsDirectoryPath.Returns(modsDirectory.FullName);
            FileMpqStormMod fileMpqStormMod = new(new FileSystem(), _heroesSource, archivePath, StormModType.Base);

            // act
            Action act = fileMpqStormMod.LoadStormFiles;

            // assert
            act.Should().Throw<MpqHeroesToolException>();

            Action openFile = () => File.Open(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose();
            openFile.Should().NotThrow();
        }
        finally
        {
            modsDirectory.Delete(true);
        }
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_AddsAssetTexts()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        fileMpqStormMod.ModContainer.LoadedAssetsTextFiles.Should().ContainSingle();
        GetDataStorage(StormModType.Base).TextCache.AssetTextsById.Dictionary.Keys.Should().BeEquivalentTo("id1");
    }

    [TestMethod]
    public void LoadStormFiles_HasArchive_AddsRelativeUiPathsOfStormLayoutFiles()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

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
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        string texturesDirectoryPath = Path.Combine(_archivePath, "Base.StormAssets", "Assets", "Textures");

        var assetFiles = GetDataStorage(StormModType.Base).FileCache.AssetFilesByRelativePath.Dictionary;
        assetFiles.Keys.Should().BeEquivalentTo(
            Path.Combine("assets", "textures", "075.dds"),
            Path.Combine("assets", "textures", "33.dds"));
        assetFiles[Path.Combine("assets", "textures", "075.dds")].Should().BeEquivalentTo(
            ExpectedStormFile(Path.Combine(texturesDirectoryPath, "075.dds")));
        assetFiles[Path.Combine("assets", "textures", "33.dds")].Should().BeEquivalentTo(
            ExpectedStormFile(Path.Combine(texturesDirectoryPath, "33.dds")));
    }

    [TestMethod]
    public void LoadStormFiles_CalledAfterGetMapDependencies_ReopensArchiveAndLoadsFiles()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);
        fileMpqStormMod.GetMapDependencies();

        // act
        fileMpqStormMod.LoadStormFiles();

        // assert
        fileMpqStormMod.ModContainer.LoadedXmlFiles.Should().ContainSingle();
        fileMpqStormMod.ModContainer.LoadedStormLayoutFiles.Should().HaveCount(2);
        fileMpqStormMod.ModContainer.LoadedAssetFiles.Should().HaveCount(2);
    }

    [TestMethod]
    public void LoadStormGameStrings_MapArchiveHasGameStrings_OpensArchiveAndAddsGameStrings()
    {
        // arrange
        _fileSystem.AddFile(_mapArchivePath, new MockFileData(File.ReadAllBytes(Path.Join("TestFiles", "DepotCache", "8d554.s2ma"))));
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        string expectedFilePath = Path.Combine(_mapArchivePath, "enus.stormdata", "localizeddata", "gamestrings.txt");

        fileMpqStormMod.ModContainer.LoadedGameStringFiles.Should().ContainSingle()
            .Which.FullPath.Should().Be(expectedFilePath);
        GetDataStorage(StormModType.Map).TextCache.GameStringsById.Dictionary.Should().NotBeEmpty();
        GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadStormGameStrings_CalledAfterLoadStormFiles_ReopensArchiveAndAddsGameStringsWithMpqPaths()
    {
        // arrange
        AddTestArchive();
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_archivePath);
        fileMpqStormMod.LoadStormFiles();

        // act
        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        StormFile stormFile = fileMpqStormMod.ModContainer.LoadedGameStringFiles.Should().ContainSingle().Subject;

        stormFile.FullPath.Should().Be(Path.Combine(_archivePath, "enus.stormdata", "localizeddata", "gamestrings.txt"));
        stormFile.MpqFilePath.Should().Be(_archivePath);
        stormFile.MpqEntryPath.Should().Be(Path.Combine("enus.stormdata", "localizeddata", "gamestrings.txt"));
        GetDataStorage(StormModType.Base).TextCache.GameStringsById.Dictionary.Should().ContainKey("DocInfo/Name");
    }

    [TestMethod]
    public void LoadStormGameStrings_ArchiveDoesNotExist_LoadsNothing()
    {
        // arrange
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        fileMpqStormMod.LoadStormGameStrings(StormLocale.ENUS);

        // assert
        fileMpqStormMod.ModContainer.LoadedGameStringFiles.Should().BeEmpty();
        GetDataStorage(StormModType.Map).TextCache.GameStringsById.Dictionary.Should().BeEmpty();
    }

    [TestMethod]
    public void GetMapDependencies_MapArchiveHasDocumentInfo_ReturnsDependencies()
    {
        // arrange
        _fileSystem.AddFile(_mapArchivePath, new MockFileData(File.ReadAllBytes(Path.Join("TestFiles", "DepotCache", "8d554.s2ma"))));
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        IReadOnlyList<MapDependency> mapDependencies = fileMpqStormMod.GetMapDependencies();

        // assert
        mapDependencies.Should().ContainSingle().Which.LocalFile.Should().Be(Path.Combine("heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
    }

    [TestMethod]
    public void GetMapDependencies_ArchiveDoesNotExist_ReturnsEmpty()
    {
        // arrange
        FileMpqStormMod fileMpqStormMod = CreateFileMpqStormMod(_mapArchivePath, StormModType.Map);

        // act
        IReadOnlyList<MapDependency> mapDependencies = fileMpqStormMod.GetMapDependencies();

        // assert
        mapDependencies.Should().BeEmpty();
    }

    /// <summary>
    /// Gets the expected <see cref="StormFile"/> values for a file in the <c>test.s2ma</c> archive.
    /// </summary>
    /// <param name="fullPath">The full path of the file.</param>
    /// <returns>An object with the expected <see cref="StormFile"/> values.</returns>
    private static object ExpectedStormFile(string fullPath) => new
    {
        StormModName = "test",
        RelativePath = fullPath[(_modsDirectoryPath.Length + 1)..],
        FullPath = fullPath,
    };

    private void AddTestArchive() =>
        _fileSystem.AddFile(_archivePath, new MockFileData(File.ReadAllBytes(Path.Join("TestFiles", "Mpq", "test.s2ma"))));

    private FileMpqStormMod CreateFileMpqStormMod(string archivePath, StormModType stormModType = StormModType.Base) =>
        new(_fileSystem, _heroesSource, archivePath, stormModType);

    private DataStorage GetDataStorage(StormModType stormModType) => _sourceContainer.LayeredDataStorage.GetDataStorage(stormModType);
}
