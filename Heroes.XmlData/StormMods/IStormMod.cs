namespace Heroes.XmlData.StormMods;

internal interface IStormMod
{
    void LoadStormData();

    void LoadStormGameStrings(HeroesLocalization localization);
}
