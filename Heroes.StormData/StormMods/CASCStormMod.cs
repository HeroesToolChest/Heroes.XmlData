namespace Heroes.StormData.StormMods;

internal sealed class CASCStormMod : StormModBase<ICASCHeroesSource>
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
        : base(cascHeroesSource, directoryPath, stormModType)
    {
    }

    public override void LoadGameDataDirectory()
    {
        throw new NotImplementedException();
    }

    protected override bool IsFileExists(string filePath) => HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.FileExists(filePath);

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;

        if (!IsFileExists(path))
            return false;

        stream = HeroesSource.CASCHeroesStorage.CASCHandlerWrapper.OpenFile(path);

        if (stream is null)
            return false;

        return true;
    }
}
