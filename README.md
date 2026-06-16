# Heroes Xml Data
[![Build](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/release/HeroesToolChest/Heroes.XmlData.svg)](https://github.com/HeroesToolChest/Heroes.XmlData/releases/latest) 
[![NuGet](https://img.shields.io/nuget/v/Heroes.XmlData.svg)](https://www.nuget.org/packages/Heroes.XmlData/)

Heroes Xml Data is a .NET library that parses the Heroes of the Storm CASC storage files through the use of [CascLib](https://github.com/WoW-Tools/CascLib).

## Usage
### Loading Storage
There are three sources of storage Heroes of the Storm can be loaded from: a local Heroes of the Storm installation, online, or already extracted data files.

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
HeroesXmlLoader heroesXmlLoaderFile = HeroesXmlLoader.LoadWithFile("Path\to\mods");
```

There is also an optional `ProgressReporter` that may be passed in to the `LoadWithCASC` and `LoadWithFile` method

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

## Developing
To build the code, it is recommended to use the latest version of [Visual Studio 2026 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/).

Another option is to use the dotnet CLI tools from the latest [.NET SDK](https://dotnet.microsoft.com/download).

## License
[MIT license](/LICENSE)
