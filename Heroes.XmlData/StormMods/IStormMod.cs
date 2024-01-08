namespace Heroes.XmlData.StormMods;

internal interface IStormMod
{
    string DirectoryPath { get; }

    void LoadStormData();

    void LoadStormGameStrings(HeroesLocalization localization);

    IEnumerable<IStormMod> GetStormMapMods(S2MAProperties s2maProperties);

    List<IStormMod> LoadDocumentInfo();
}
