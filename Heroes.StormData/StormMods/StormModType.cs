namespace Heroes.StormData.StormMods;

/// <summary>
/// Specifies the layer a <c>stormmod</c> loads its data into.
/// <para>
/// When the same data exists in more than one layer, a higher layer overrides a lower one,
/// from lowest to highest: <see cref="Base"/>, <see cref="Map"/>, then <see cref="Custom"/>.
/// </para>
/// </summary>
public enum StormModType
{
    /// <summary>
    /// The game's base <c>stormmods</c> (<c>core.stormmod</c>, <c>heroes.stormmod</c>, <c>heroesdata.stormmod</c>) and their dependencies.
    /// This is the lowest priority layer.
    /// </summary>
    Base,

    /// <summary>
    /// A map's <c>stormmod</c> (e.g. a battleground <c>.stormmap</c>), which overrides the <see cref="Base"/> layer.
    /// </summary>
    Map,

    /// <summary>
    /// A user-supplied <c>stormmod</c>, which overrides both the <see cref="Base"/> and <see cref="Map"/> layers.
    /// This is the highest priority layer.
    /// </summary>
    Custom,
}
