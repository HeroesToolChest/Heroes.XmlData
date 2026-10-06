namespace Heroes.StormData.StormMods.Mpq;

internal sealed class CASCMpqStormMod : MpqStormModBase<ICASCHeroesSource>
{
    public CASCMpqStormMod(ICASCHeroesSource heroesSource, string path, StormModType stormModType)
        : base(heroesSource, path, stormModType)
    {
    }
}
