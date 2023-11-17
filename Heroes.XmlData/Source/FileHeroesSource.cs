using Heroes.XmlData.StormMods;

namespace Heroes.XmlData.Source;

internal class FileHeroesSource(HeroesData heroesData, string modsDirectoryPath)
    : HeroesSource(heroesData, modsDirectoryPath)
{
    protected override void AddStormMods(IList<IStormMod> stormMods)
    {
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, CoreStormModDirectory));
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, HeroesStormModDirectory));
        stormMods.Add(CreateStormModInstance<FileStormModPathStormMod>(this, HeroesDataStormModDirectory));
        //stormMods.Add(new FileCoreStormMod(this));
        //stormMods.Add(new FileHeroesStormMod(this));
        //stormMods.Add(new FileHeroesDataStormMod(this));
    }
}
