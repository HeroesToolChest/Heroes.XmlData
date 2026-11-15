namespace Heroes.StormData.StormMods.Mpq;

internal sealed class CASCMpqStormMod : MpqStormModBase<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string path, StormModType stormModType)
        : base(heroesSource, path, stormModType)
    {
    }

    protected override void LoadStormLayoutDirectory()
    {
        throw new NotImplementedException();
    }

    protected override IStormMod GetStormMod(string path, StormModType stormModType)
    {
        throw new NotImplementedException();
    }

    protected override void LoadAssetsDirectory()
    {
        throw new NotImplementedException();
    }
}
