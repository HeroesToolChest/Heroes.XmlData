namespace Heroes.StormData;

/// <summary>
/// Describes what a <see cref="HeroesLoader"/> should have loaded. Used by <see cref="HeroesLoader.Load(HeroesLoaderOptions)"/>,
/// which loads everything in the correct order and loads the gamestrings only once.
/// </summary>
public sealed class HeroesLoaderOptions
{
    /// <summary>
    /// <para>Gets the title (enUS) of the map mod to load. Is case-insensitive. Can be found from <see cref="HeroesLoader.GetMapTitles"/>.</para>
    /// <para>If <see langword="null"/>, the currently loaded map mod (if any) is left unchanged.</para>
    /// </summary>
    public string? MapTitle { get; init; }

    /// <summary>
    /// <para>Gets the locale of the gamestrings to load.</para>
    /// <para>If <see langword="null"/>, the currently loaded gamestrings locale (if any) is kept.</para>
    /// </summary>
    public StormLocale? GameStringsLocale { get; init; }
}
