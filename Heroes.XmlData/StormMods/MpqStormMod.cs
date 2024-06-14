using System.Buffers;

namespace Heroes.XmlData.StormMods;

internal abstract class MpqStormMod<T> : StormMod<T>
    where T : IHeroesSource
{
    private static readonly char _forwardSlashChar = '/';
    private static readonly char _backSlashChar = '\\';
    private static readonly char[] _pathSeperators = string.Join(string.Empty, _forwardSlashChar, _backSlashChar).ToCharArray();
    private static readonly SearchValues<char> _forwardSlashSeperatorSearchValue = SearchValues.Create(_forwardSlashChar.ToString());
    private static readonly SearchValues<char> _backSlashSeperatorSearchValue = SearchValues.Create(_backSlashChar.ToString());

    private MpqHeroesArchive? _mpqHeroesArchive;
    private MpqFolder? _mpqFolderRoot;

    protected MpqStormMod(T heroesSource, string directoryPath, StormModType stormModType)
        : base(heroesSource, directoryPath, stormModType, StormPathType.MPQ)
    {
    }

    protected MpqStormMod(T heroesSource, string name, string directoryPath, StormModType stormModType)
        : base(heroesSource, name, directoryPath, stormModType, StormPathType.MPQ)
    {
    }

    protected string MpqDirectoryPath => Path.Join(HeroesSource.ModsBaseDirectoryPath, DirectoryPath);

    protected override string GameDataDirectoryPath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataDirectory);

    protected override string GameDataFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.GameDataXmlFile);

    protected override string IncludesFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.IncludesXmlFile);

    protected override string DocumentInfoPath => HeroesSource.DocumentInfoFile;

    protected override string FontStyleFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.FontStyleFile);

    protected override string BuildIdFilePath => Path.Join(HeroesSource.BaseStormDataDirectory, HeroesSource.BuildIdFile);

    public override void LoadStormData()
    {
        using MpqHeroesArchive mpqHeroesArchive = GetMpqHeroesArchive();

        CreateMpqFileTree();
        base.LoadStormData();
    }

    public override void LoadStormGameStrings(StormLocale stormLocale)
    {
        using MpqHeroesArchive mpqHeroesArchive = GetMpqHeroesArchive();

        base.LoadStormGameStrings(stormLocale);
    }

    protected override string GetGameStringFilePath(StormLocale stormLocale)
    {
        return Path.Join(StormLocaleData.GetStormDataFileName(stormLocale), HeroesSource.LocalizedDataDirectory, HeroesSource.GameStringFile);
    }

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (_mpqHeroesArchive is null || !TryGetEntry(filePath, out MpqHeroesArchiveEntry? entry))
        {
            return false;
        }

        stream = _mpqHeroesArchive.DecompressEntry(entry.Value);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        if (_mpqFolderRoot is null || !_mpqFolderRoot.TryGetLastDirectory(GameDataDirectoryPath, out MpqFolder? gameDataFolder))
        {
            StormModStorage.AddDirectoryNotFound(new StormPath()
            {
                StormModDirectoryPath = DirectoryPath,
                StormModName = Name,
                Path = GameDataDirectoryPath,
                PathType = StormPathType.MPQ,
            });

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
        string[] parts = file.Split(_pathSeperators, StringSplitOptions.RemoveEmptyEntries);

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

            string normalizedFile = PathHelper.NormalizePath(file);

            if (normalizedFile.Contains(Path.DirectorySeparatorChar))
            {
                CreateFilePaths(_mpqFolderRoot, file);
            }
            else
            {
                _mpqFolderRoot.Files[file] = new MpqFile(file);
            }
        }
    }

    private bool TryGetEntry(string path, [NotNullWhen(true)] out MpqHeroesArchiveEntry? mpqHeroesArchiveEntry)
    {
        if (_mpqHeroesArchive is null)
        {
            mpqHeroesArchiveEntry = null;
            return false;
        }

        if (_mpqHeroesArchive.TryGetEntry(path, out mpqHeroesArchiveEntry))
        {
            return true;
        }
        else
        {
            ReadOnlySpan<char> pathSpan = path;
            Span<char> buffer = stackalloc char[path.Length];

            if (pathSpan.IndexOfAny(_backSlashSeperatorSearchValue) >= 0)
            {
                pathSpan.Replace(buffer, _backSlashChar, _forwardSlashChar);
                if (_mpqHeroesArchive.TryGetEntry(buffer, out mpqHeroesArchiveEntry))
                {
                    return true;
                }
            }
            else if (pathSpan.IndexOfAny(_forwardSlashSeperatorSearchValue) >= 0)
            {
                pathSpan.Replace(buffer, _forwardSlashChar, _backSlashChar);
                if (_mpqHeroesArchive.TryGetEntry(buffer, out mpqHeroesArchiveEntry))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
