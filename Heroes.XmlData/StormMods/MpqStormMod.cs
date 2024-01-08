﻿namespace Heroes.XmlData.StormMods;

internal abstract class MpqStormMod<T> : StormMod<T>
    where T : IHeroesSource
{
    private MpqHeroesArchive? _mpqHeroesArchive;
    private MpqFolder? _mpqFolderRoot;

    public MpqStormMod(T heroesSource)
        : base(heroesSource)
    {
    }

    protected abstract string MpqDirectoryPath { get; }

    protected override string GameDataDirectoryPath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataDirectory);

    protected override string GameDataFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataXmlFile);

    protected override string IncludesFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.IncludesXmlFile);

    protected override string DocumentInfoPath => HeroesSource.DocumentInfoFile;

    public override void LoadStormData()
    {
        using MpqHeroesArchive mpqHeroesArchive = GetMpqHeroesArchive();

        CreateMpqFileTree();
        base.LoadStormData();
    }

    public override void LoadStormGameStrings(HeroesLocalization localization)
    {
        using MpqHeroesArchive mpqHeroesArchive = GetMpqHeroesArchive();

        base.LoadStormGameStrings(localization);
    }

    protected override string GetGameStringFilePath(HeroesLocalization localization)
    {
        return Path.Join(localization.GetDescription(), HeroesSource.LocalizedDataDirectory, HeroesSource.GameStringFile);
    }

    protected override void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        XmlStorage.AddXmlFile(document, xmlFilePath);
    }

    protected override bool ValidateXmlFile(string xmlFilePath, [NotNullWhen(true)] out XDocument? document, bool isRequired = true)
    {
        document = null;

        if (!IsXmlFile(xmlFilePath))
            return false;

        if (_mpqHeroesArchive is null || !_mpqHeroesArchive.TryGetEntry(xmlFilePath, out MpqHeroesArchiveEntry? entry))
        {
            if (isRequired)
                HeroesData.AddFileNotFound(xmlFilePath);

            return false;
        }

        document = XDocument.Load(_mpqHeroesArchive.DecompressEntry(entry.Value));

        return true;
    }

    protected override bool ValidateGameStringFile(HeroesLocalization localization, [NotNullWhen(true)] out Stream? stream, out string path)
    {
        stream = null;
        path = GetGameStringFilePath(localization);

        if (_mpqHeroesArchive is null || !_mpqHeroesArchive.TryGetEntry(path, out MpqHeroesArchiveEntry? entry))
        {
            HeroesData.AddFileNotFound(path);
            return false;
        }

        stream = _mpqHeroesArchive.DecompressEntry(entry.Value);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        if (_mpqFolderRoot is null || !_mpqFolderRoot.TryGetLastDirectory(GameDataDirectoryPath, out MpqFolder? gameDataFolder))
        {
            HeroesData.AddDirectoryNotFound(GameDataDirectoryPath);
            return;
        }

        foreach (KeyValuePair<string, MpqFile> file in gameDataFolder.Files)
        {
            AddXmlFile(file.Value.FullName);
        }
    }

    protected abstract Stream GetMpqFile(string file);

    private static void CreateFilePaths(MpqFolder root, string file)
    {
        string[] parts = file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        MpqFolder folder = root;

        for (int i = 0; i < parts.Length; i++)
        {
            bool isFile = i == parts.Length - 1;

            string pathPart = parts[i];
            if (isFile && !folder.Files.TryGetValue(pathPart, out _))
            {
                folder.Files[pathPart] = new MpqFile(file);
            }
            else
            {
                if (!folder.Folders.TryGetValue(pathPart, out MpqFolder? existingFolder))
                {
                    existingFolder = new MpqFolder(pathPart);
                    folder.Folders[pathPart] = existingFolder;
                }

                folder = existingFolder;
            }
        }
    }

    private MpqHeroesArchive GetMpqHeroesArchive()
    {
        MpqHeroesArchive mpqHeroesArchive = MpqHeroesFile.Open(GetMpqFile(MpqDirectoryPath));
        _mpqHeroesArchive = mpqHeroesArchive;
        return mpqHeroesArchive;
    }

    private void CreateMpqFileTree()
    {
        _mpqFolderRoot = new MpqFolder("root");

        if (_mpqHeroesArchive is null)
            return;

        foreach (MpqHeroesArchiveEntry entry in _mpqHeroesArchive.MpqArchiveEntries)
        {
            string? file = entry.FileName;
            if (string.IsNullOrWhiteSpace(file))
                continue;

            if (file.Contains(Path.DirectorySeparatorChar))
            {
                CreateFilePaths(_mpqFolderRoot, file);
            }
            else
            {
                _mpqFolderRoot.Files[file] = new MpqFile(file);
            }
        }
    }
}