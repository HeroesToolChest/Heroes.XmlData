using Heroes.LocaleText;
using Heroes.XmlData;

Console.WriteLine("Hello, World!");

Console.WriteLine(Environment.OSVersion);
var aa = Environment.OSVersion;

HeroesXmlLoader xmlLoader = HeroesXmlLoader.LoadAsFile("F:\\heroes\\heroes_91418\\mods_all_91418");
//HeroesXmlFileLoader heroesXmlFileLoader = new("/home/koliva/mods_all_91418");
//HeroesXmlFileLoader heroesXmlFileLoader = new("F:\\heroes\\heroes_91418\\mods_all_91418");
//HeroesData heroesData = heroesXmlFileLoader.HeroesData;
var hero = xmlLoader.HeroesData;
var a = xmlLoader.GetMapTitles();
xmlLoader.LoadStormMods();
var b = xmlLoader.GetMapTitles();
xmlLoader.LoadMapMod("Volskaya Foundry");
xmlLoader.LoadMapMod("");
xmlLoader.LoadGameStrings(StormLocale.ENUS);
xmlLoader.LoadMapMod("Alterac Pass");
//fileLoader.LoadGameStrings(StormLocale.DEDE);

//Heroes.XmlData.StormData.GameStringText? a1 = hero.GetGameString("");
//var a2 = hero.GetGameString("Unit/Name/AllianceCavalry");
//var a3 = hero.GetGameString("Button/Tooltip/LiLiShakeItOffTalent");

var bxcvxcv = hero.GetElements("CAbilEffectInstant");

//var a333333333333 = hero.GetBuildNumber();
var hero2 = xmlLoader.HeroesData;

//xmlLoader.LoadMapMod("Volskaya Foundry");
//heroesXmlFileLoader.LoadGameStrings(HeroesLocalization.DEDE);

//XDocument xDocument = new XDocument();

//XDocument docFirst = XDocument.Load(@"F:\heroes\heroes_91093\mods_91093\core.stormmod\base.stormdata\gamedata\abildata.xml");
//docFirst.Root.SetAttributeValue("HXD-FilePath", "F:\\heroes\\heroes_91093\\mods_91093\\core.stormmod\\base.stormdata\\gamedata\\abildata.xml");

//xDocument.Declaration = docFirst.Declaration;
//xDocument.Add(new XElement("HXD"));


//xDocument.Root.Add(docFirst.Root);


//XDocument doc = XDocument.Load(@"F:\heroes\heroes_91093\mods_91093\core.stormmod\base.stormdata\gamedata\accumulatordata.xml");
//doc.Root.SetAttributeValue("HXD-FilePath", "F:\\heroes\\heroes_91093\\mods_91093\\core.stormmod\\base.stormdata\\gamedata\\accumulatordata.xml");
//xDocument.Root.Add(doc.Root);
//CASCConfig.ThrowOnFileNotFound = true;
//CASCConfig.ThrowOnMissingDecryptionKey = true;
//CASCConfig config = CASCConfig.LoadLocalStorageConfig("E:\\Games\\Heroes of the Storm", "hero");
//CASCHandler cascHandler = CASCHandler.OpenStorage(config);

//cascHandler.Root.LoadListFile(Path.Combine(Environment.CurrentDirectory, "listfile.txt"));

//CASCFolder CASCFolderRoot = cascHandler.Root.SetFlags(LocaleFlags.All);

//HeroesXmlCASCLoader heroesXmlCASCLoader = new HeroesXmlCASCLoader(null);
//heroesXmlCASCLoader.Test("E:\\Games\\Heroes of the Storm");
Console.ReadKey();
//
//
// XmlContainer
// HeroesContainer
// HeroesDataContainer
