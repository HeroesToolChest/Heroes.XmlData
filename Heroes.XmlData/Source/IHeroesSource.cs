using Heroes.XmlData.StormMods;

namespace Heroes.XmlData.Source;

internal interface IHeroesSource
{
    int HotsBuild { get; }

    string ModsDirectoryPath { get; }

    string DefaultModsDirectory { get; }

    string GameDataDirectory { get; }

    string BaseStormDataDirectory { get; }

    string LocalizedDataDirectory { get; }

    string GameStringFile { get; }

    string GameDataXmlFile { get; }

    string IncludesXmlFile { get; }

    string CoreStormModDirectory { get; }

    string HeroesStormModDirectory { get; }

    string HeroesDataStormModDirectory { get; }

    public string HeroModsDirectory { get; }

    IHeroesData HeroesData { get; }

    IList<IStormMod> StormMods { get; }

    /// <summary>
    /// Creates an instance of an <see cref="IStormMod"/>.
    /// </summary>
    /// <typeparam name="T">A type of <see cref="IStormMod"/>.</typeparam>
    /// <param name="args">Additional arguments to be passed in a paramters to the constructor.</param>
    /// <returns>An instance of a <see cref="IStormMod"/>.</returns>
    IStormMod CreateStormModInstance<T>(params object?[]? args)
        where T : IStormMod;

    void LoadStormData();

    void LoadGamestrings(HeroesLocalization localization);
}
