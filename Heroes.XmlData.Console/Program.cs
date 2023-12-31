using CASCLib;
using Heroes.XmlData;
using System.ComponentModel;
using System.Xml.Linq;

Console.WriteLine("Hello, World!");

HeroesXmlFileLoader heroesXmlFileLoader = new(@"F:\heroes\heroes_91418\mods_91418");
HeroesData heroesData = heroesXmlFileLoader.HeroesData;

heroesXmlFileLoader.LoadStormMods();
heroesXmlFileLoader.LoadMapMod("any");
heroesXmlFileLoader.LoadGameStrings(HeroesLocalization.ENUS);
heroesXmlFileLoader.LoadGameStrings(HeroesLocalization.DEDE);

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
Console.ReadKey();