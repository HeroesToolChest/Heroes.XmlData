namespace Heroes.StormData;

/// <summary>
/// Gives read access to the loaded Heroes of the Storm data (the xml elements, gamestrings, styles, and files),
/// merged across the base, map, and custom <c>stormmods</c>.
/// </summary>
/// <remarks>
/// An instance is a live view of the data of the <see cref="HeroesLoader"/> that owns it,
/// so it reflects whatever is currently loaded and changes as the loader loads and unloads data.
/// </remarks>
public sealed class StormCatalog
{
    private readonly LayeredDataStorage _layeredDataStorage;

    internal StormCatalog(LayeredDataStorage layeredDataStorage)
    {
        ArgumentNullException.ThrowIfNull(layeredDataStorage);

        _layeredDataStorage = layeredDataStorage;
    }
}
