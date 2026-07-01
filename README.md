# Heroes Xml Data
[![Build](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/release/HeroesToolChest/Heroes.XmlData.svg)](https://github.com/HeroesToolChest/Heroes.XmlData/releases/latest) 
[![NuGet](https://img.shields.io/nuget/v/Heroes.XmlData.svg)](https://www.nuget.org/packages/Heroes.XmlData/)

Heroes Xml Data is a .NET library that parses the Heroes of the Storm CASC storage files through the use of [CascLib](https://github.com/WoW-Tools/CascLib) and provides an API to access the files and data.

## Usage
### Loading Storage
Heroes of the Storm data can be loaded from three sources: a local Heroes of the Storm installation, online, or already extracted data files.

To extract the data files, use a tool such as [Heroes Data Parser](https://github.com/HeroesToolChest/HeroesDataParser), [CASCExplorer](https://github.com/WoW-Tools/CASCExplorer), or [CascView](http://www.zezula.net/en/casc/main.html).

Example of loading from local installation:
```C#
// get the casc configuration, specifying the local storage
CASCConfig cascConfig = HeroesXmlLoader.GetCASCConfig("Path\to\Heroes of the Storm");

// load from the local installation
HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithCASC(cascConfig, new HttpClient());
```

Example of loading from online:
```C#
HttpClient httpClient = new();

// get the casc configuration, specifying online storage
CASCConfig cascConfig = HeroesXmlLoader.GetOnlineCASCConfig(httpClient, isPtr: false);

// load from online
HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithCASC(cascConfig, httpClient);
```

Example of loading from extracted data files:
```C#
// specify the mods root directory
// if on linux or macos, ensure all directories and files are in lowercase characters
HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithFile("Path\to\mods");
```

There is also an optional `ProgressReporter` that may be passed in to the `LoadWithCASC` and `LoadWithFile` methods:
```C#
// optional progress reporting
Progress<ProgressInfo> progress = new(p =>
{
    Console.WriteLine($"Progress: {p.Percentage}% - {p.Message}");
});
ProgressReporter progressReporter = new(progress);

// HeroesXmlLoader.LoadWithCASC(cascConfig, httpClient, progressReporter: progressReporter);
// HeroesXmlLoader.LoadWithFile("Path\to\mods, progressReporter: progressReporter);
```

### HeroesXmlLoader
After loading the storage, the `HeroesXmlLoader` instance also provides an API to access file-related methods and properties, such as `FileExists` and `GetFile`.
```C#
string filePath = @"mods\heromods\alarak.stormmod\base.stormdata\gamedata.xml";
bool exists = heroesXmlLoader.FileExists(filePath);
Stream fileStream = heroesXmlLoader.GetFile(filePath);
```

If the file is an MPQ file, the second argument can be provided to look up an entry:
```C#
string filePath = @"mods\core.stormmod\base.stormdata\depotcache\7b\7a\7b7a83395746a818eb7f842a7ff0a27699c93b5a9d170d1c0993a3cc0058e1d0.s2ma";
string mpqEntry = @"enus.stormdata\localizeddata\gamestrings.txt";

bool exists2 = heroesXmlLoader.FileExists(filePath, mpqEntry);
Stream mpqFileStream = heroesXmlLoader.GetFile(filePath, mpqEntry);
```
### Loading StormMods
Use the `HeroesXmlLoader` instance to load the stormmods. `LoadMapMod` and `LoadGameStrings` may be called as many times as needed to switch between different maps and locales.

```C#
// loads the base stormmods
heroesXmlLoader.LoadStormMods();

// optionally load a map mod
heroesXmlLoader.LoadMapMod("Warhead Junction");

// optionally load a locale, defaults to enUS
heroesXmlLoader.LoadGameStrings(StormLocale.ENUS);
```

### HeroesData
Get the `HeroesData` from the `HeroesXmlLoader` instance. This can be used to obtain data directly from the xml data, gamestrings, layout files, etc.

Visit the [wiki](https://github.com/HeroesToolChest/Heroes.XmlData/wiki) for examples.

## Developing
To build the code, it is recommended to use the latest version of [Visual Studio 2026 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/).

Another option is to use the dotnet CLI tools from the latest [.NET SDK](https://dotnet.microsoft.com/download).

## License
[MIT license](/LICENSE)
