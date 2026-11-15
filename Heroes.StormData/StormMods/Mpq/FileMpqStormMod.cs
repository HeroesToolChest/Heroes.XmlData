namespace Heroes.StormData.StormMods.Mpq;

internal sealed class FileMpqStormMod : MpqStormModBase<IFileHeroesSource>
{
    private readonly IFileSystem _fileSystem;

    public FileMpqStormMod(IFileSystem fileSystem, IFileHeroesSource heroesSource, string path, StormModType stormModType)
        : base(heroesSource, path, stormModType)
    {
        _fileSystem = fileSystem;
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
