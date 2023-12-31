namespace Heroes.XmlData.Source;

internal class FileHeroesSource : HeroesSource
{
    public FileHeroesSource(HeroesData heroesData, string modsDirectoryPath)
        : base(heroesData, modsDirectoryPath)
    {
    }

    protected override void AddStormMods(IList<IStormMod> stormMods)
    {
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, CoreStormModDirectory));
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, HeroesStormModDirectory));
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, HeroesDataStormModDirectory));
    }

    protected override void AddStormMaps()
    {
        DepotCache = new FileDepotCache(this);
    }
}
