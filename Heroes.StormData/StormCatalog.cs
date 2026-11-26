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

    /// <summary>
    /// Determines if a gamestring (from a <c>gamestrings.txt</c> file) exists in the loaded gamestrings.
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <returns>
    /// <see langword="true"/> if the gamestring exists; otherwise <see langword="false"/>, which includes <paramref name="id"/> being empty
    /// and no gamestrings being loaded.
    /// </returns>
    public bool GameStringExists(ReadOnlySpan<char> id) => _layeredDataStorage.Texts.TryGetFirstGameStringById(id, out _);

    /// <summary>
    /// Gets an unparsed gamestring (from a <c>gamestrings.txt</c> file), if it exists.
    /// <para>
    /// If more than one <c>stormmod</c> has the gamestring, the value from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <param name="gameString">
    /// When this method returns, contains the unparsed gamestring, if found; otherwise, <see langword="null"/>.
    /// It can be <see cref="string.Empty"/> if the gamestring has no value.
    /// </param>
    /// <returns><see langword="true"/> if the gamestring was found; otherwise <see langword="false"/>, in the same cases as <see cref="GameStringExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? gameString) => _layeredDataStorage.Texts.TryGetFirstGameStringById(id, out gameString);

    /// <summary>
    /// Gets an unparsed gamestring (from a <c>gamestrings.txt</c> file).
    /// <para>
    /// If more than one <c>stormmod</c> has the gamestring, the value from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <returns>The unparsed gamestring. It can be <see cref="string.Empty"/> if the gamestring has no value.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The gamestring was not found (in the same cases that <see cref="GameStringExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public string GetGameString(ReadOnlySpan<char> id)
    {
        if (TryGetGameString(id, out string? gameString))
            return gameString;

        throw new KeyNotFoundException($"Could not find the gamestring: {id}");
    }

    /// <summary>
    /// Determines if an asset string (from an <c>assets.txt</c> file) exists.
    /// </summary>
    /// <param name="id">
    /// The <c>id</c> of the asset string (e.g. <c>UI/HeroIcons/Alarak</c>), without the leading <c>@</c> that is used to reference it in XML. This is case-sensitive.
    /// </param>
    /// <returns><see langword="true"/> if the asset string exists; otherwise <see langword="false"/>, which includes <paramref name="id"/> being empty.</returns>
    public bool AssetStringExists(ReadOnlySpan<char> id) => _layeredDataStorage.Texts.TryGetFirstAssetTextById(id, out _);

    /// <summary>
    /// Gets an asset string (from an <c>assets.txt</c> file), if it exists.
    /// <para>
    /// If more than one <c>stormmod</c> has the asset string, the value from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="id">
    /// The <c>id</c> of the asset string (e.g. <c>UI/HeroIcons/Alarak</c>), without the leading <c>@</c> that is used to reference it in XML. This is case-sensitive.
    /// </param>
    /// <param name="assetString">
    /// When this method returns, contains the asset string, if found; otherwise, <see langword="null"/>.
    /// It can be <see cref="string.Empty"/> if the asset string has no value.
    /// </param>
    /// <returns><see langword="true"/> if the asset string was found; otherwise <see langword="false"/>, in the same cases as <see cref="AssetStringExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetAssetString(ReadOnlySpan<char> id, [NotNullWhen(true)] out string? assetString) => _layeredDataStorage.Texts.TryGetFirstAssetTextById(id, out assetString);

    /// <summary>
    /// Gets an asset string (from an <c>assets.txt</c> file).
    /// <para>
    /// If more than one <c>stormmod</c> has the asset string, the value from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="id">
    /// The <c>id</c> of the asset string (e.g. <c>UI/HeroIcons/Alarak</c>), without the leading <c>@</c> that is used to reference it in XML. This is case-sensitive.
    /// </param>
    /// <returns>The asset string. It can be <see cref="string.Empty"/> if the asset string has no value.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The asset string was not found (in the same cases that <see cref="AssetStringExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public string GetAssetString(ReadOnlySpan<char> id)
    {
        if (TryGetAssetString(id, out string? assetString))
            return assetString;

        throw new KeyNotFoundException($"Could not find the asset string: {id}");
    }
}
