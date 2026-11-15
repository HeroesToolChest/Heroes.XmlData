namespace Heroes.StormData.StormMods;

internal sealed class CASCStormMod : StormModBase<ICASCHeroesSource>
{
    public CASCStormMod(ICASCHeroesSource cascHeroesSource, string directoryPath, StormModType stormModType)
        : base(cascHeroesSource, directoryPath, stormModType)
    {
    }

    protected override void LoadGameDataDirectory()
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

    protected override IStormMod GetStormMod(string path, StormModType stormModType)
    {
        throw new NotImplementedException();
    }

    protected override void LoadAssetsDirectory()
    {
        throw new NotImplementedException();
    }

    protected override void LoadStormLayoutDirectory()
    {
        throw new NotImplementedException();
    }
}
