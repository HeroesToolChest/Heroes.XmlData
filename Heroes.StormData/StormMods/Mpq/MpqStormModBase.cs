namespace Heroes.StormData.StormMods.Mpq;

internal abstract class MpqStormModBase<T> : StormModBase<T>
    where T : IHeroesSource
{
    protected MpqStormModBase(T heroesSource, string path, StormModType stormModType)
        : base(heroesSource, path, stormModType)
    {
    }

    public override void LoadGameDataDirectory()
    {

    }

    protected override bool IsFileExists(string filePath)
    {
        throw new NotImplementedException();
    }

    protected override bool TryGetFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        throw new NotImplementedException();
    }
}
