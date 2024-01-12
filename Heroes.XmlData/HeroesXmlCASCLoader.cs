namespace Heroes.XmlData;

public class HeroesXmlCASCLoader
{
    public HeroesXmlCASCLoader(CASCHeroesStorage cascHeroesStorage)
    {

    }

    public void Test(string path)
    {
        CASCConfig.ThrowOnFileNotFound = true;
        CASCConfig.ThrowOnMissingDecryptionKey = true;
        CASCConfig config = CASCConfig.LoadLocalStorageConfig("E:\\Games\\Heroes of the Storm", "hero");
        CASCHandler cascHandler = CASCHandler.OpenStorage(config);

        cascHandler.Root.LoadListFile(Path.Combine(Environment.CurrentDirectory, "listfile.txt"));

        CASCFolder cascFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);


        HeroesData heroesData = new HeroesData();
        CASCHeroesSource cASCHeroesSource = new CASCHeroesSource(heroesData, new(cascHandler, cascFolderRoot));

        cASCHeroesSource.LoadStormData();
        cASCHeroesSource.LoadDepotCache();

        var a = cASCHeroesSource.S2MAPropertiesByTitle.Select(x => x.Key).Order();

        cASCHeroesSource.LoadStormMapData("Volskaya Foundry");

        cASCHeroesSource.LoadGamestrings(HeroesLocalization.ENUS);
    }
}
