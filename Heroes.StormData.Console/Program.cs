using CASCLib;
using Heroes.StormData;
using Heroes.StormData.Source;

Console.WriteLine("Hello, World!");

HttpClient httpClient = new();

CASCConfig cascConfig = HeroesLoader.GetCASCConfig("E:\\Games\\Heroes of the Storm");
//CASCConfig cascConfig = HeroesLoader.GetOnlineCASCConfig(httpClient, isPtr: false);

Progress<ProgressInfo> progress = new(p =>
{
    Console.WriteLine($"Progress: {p.Percentage}% - {p.Message}");
});


HeroesLoader heroesLoader = HeroesLoader.LoadWithFile("F:\\heroes\\heroes_98304\\mods");
//HeroesLoader heroesLoader = HeroesLoader.LoadWithCASC(cascConfig, httpClient);

heroesLoader.LoadStormMods();