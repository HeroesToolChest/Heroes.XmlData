# Heroes Xml Data
[![Build](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/HeroesToolChest/Heroes.XmlData/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/release/HeroesToolChest/Heroes.XmlData.svg)](https://github.com/HeroesToolChest/Heroes.XmlData/releases/latest) 
[![NuGet](https://img.shields.io/nuget/v/Heroes.XmlData.svg)](https://www.nuget.org/packages/Heroes.XmlData/)

Heroes Xml Data is a .NET library that parses the Heroes of the Storm CASC storage files through the use of [CascLib](https://github.com/WoW-Tools/CascLib).

## Usage
There are three sources of data Heroes of the Storm can be loaded from: a local Heroes of the Storm installation, online, or extracted data files.

Example of local installation:
```C#
HttpClient httpClient = new();

// get the casc configuration
CASCConfig cascConfig = HeroesXmlLoader.GetCASCConfig("Path\\to\\Heroes of the Storm");

// optional progress reporting
Progress<ProgressInfo> progress = new(p =>
{
    Console.WriteLine($"Progress: {p.Percentage}% - {p.Message}");
});
ProgressReporter progressReporter = new(progress);

// load from the local installation
HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithCASC(cascConfig, httpClient, progressReporter: progressReporter);
```

Example of online:
```C#
HttpClient httpClient = new();

// get the casc configuration
CASCConfig cascConfig = HeroesXmlLoader.GetOnlineCASCConfig(httpClient, isPtr: false);

// optional progress reporting
Progress<ProgressInfo> progress = new(p =>
{
    Console.WriteLine($"Progress: {p.Percentage}% - {p.Message}");
});
ProgressReporter progressReporter = new(progress);

// load from online
HeroesXmlLoader heroesXmlLoader = HeroesXmlLoader.LoadWithCASC(cascConfig, httpClient, progressReporter: progressReporter);
```

Example of extracted data files:
```C#
// optional progress reporting
Progress<ProgressInfo> progress = new(p =>
{
    Console.WriteLine($"Progress: {p.Percentage}% - {p.Message}");
});
ProgressReporter progressReporter = new(progress);

HeroesXmlLoader heroesXmlLoaderFile = HeroesXmlLoader.LoadWithFile("Path\\to\\mods", progressReporter: progressReporter);
```
## Developing
To build and compile the code, it is recommended to use the latest version of [Visual Studio 2026 or Visual Studio Code](https://visualstudio.microsoft.com/downloads/).

Another option is to use the dotnet CLI tools from the latest [.NET SDK](https://dotnet.microsoft.com/download).

## License
[MIT license](/LICENSE)
