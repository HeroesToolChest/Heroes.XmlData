﻿namespace Heroes.XmlData.StormMods;

internal class CASCStormMod : StormMod<ICASCHeroesSource>, IStormMod
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
        : base(cascHeroesSource, directoryPath, stormModType)
    {
    }

    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string name, string directoryPath, StormModType stormModType)
    : base(cascHeroesSource, name, directoryPath, stormModType)
    {
    }

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!HeroesSource.CASCHeroesStorage.CASCHandler.FileExists(filePath))
            return false;

        stream = HeroesSource.CASCHeroesStorage.CASCHandler.OpenFile(filePath);

        return true;
    }

    protected override void LoadGameDataDirectory()
    {
        if (!HeroesSource.CASCHeroesStorage.CASCFolderRoot.TryGetLastDirectory(GameDataDirectoryPath, out CASCFolder? gameDataFolder))
        {
            StormModStorage.AddDirectoryNotFound(new StormFile()
            {
                Path = GameDataDirectoryPath,
                StormModDirectoryPath = DirectoryPath,
                StormModName = Name,
            });

            return;
        }

        foreach (KeyValuePair<string, CASCFile> file in gameDataFolder.Files)
        {
            AddXmlFile(file.Value.FullName);
        }
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType) => HeroesSource.StormModFactory.CreateCASCStormModInstance(HeroesSource, path, stormModType);
}
