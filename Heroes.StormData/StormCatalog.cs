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
    private readonly GameStringParser _gameStringParser;

    internal StormCatalog(LayeredDataStorage layeredDataStorage)
    {
        ArgumentNullException.ThrowIfNull(layeredDataStorage);

        _layeredDataStorage = layeredDataStorage;

        // holds no per-call state (the locale is passed to each call), so one instance is used for the lifetime of the catalog
        _gameStringParser = new GameStringParser(layeredDataStorage);
    }

    /// <summary>
    /// Gets the locale of the currently loaded gamestrings, or <see langword="null"/> if no gamestrings are loaded.
    /// <para>
    /// This is the locale that <see cref="ParseGameString(ReadOnlySpan{char}, StormLocale?, bool)"/> uses when no locale is given.
    /// </para>
    /// </summary>
    public StormLocale? GameStringsLocale { get; internal set; }

    /// <summary>
    /// Parses an unparsed gamestring into a <see cref="GameStringText"/>, replacing its data tags (<c>&lt;d ... /&gt;</c>) with their values.
    /// <para>
    /// The values are calculated from the currently loaded XML data, so the same gamestring can parse to different text after a <c>mapmod</c> is loaded or unloaded.
    /// </para>
    /// </summary>
    /// <param name="gameString">The unparsed gamestring text (not its <c>id</c>). Can be from <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> or any other source.</param>
    /// <param name="locale">
    /// The locale of <paramref name="gameString"/>, used to format the numbers written into the text and for the scaling text.
    /// If <see langword="null"/>, the <see cref="GameStringsLocale"/> is used, or <see cref="StormLocale.ENUS"/> if no gamestrings are loaded.
    /// </param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="locale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    public GameStringText ParseGameString(ReadOnlySpan<char> gameString, StormLocale? locale = null, bool extractFontValues = false)
    {
        StormLocale stormLocale = locale ?? GameStringsLocale ?? StormLocale.ENUS;

        if (!Enum.IsDefined(stormLocale))
            throw new ArgumentOutOfRangeException(nameof(locale), stormLocale, $"The value is not a defined {nameof(StormLocale)}.");

        return new GameStringText(_gameStringParser.ParseText(gameString, stormLocale), stormLocale, extractFontValues);
    }

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>, if it exists.
    /// <para>
    /// This is the same as <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> followed by <see cref="ParseGameString(ReadOnlySpan{char}, StormLocale?, bool)"/>
    /// with the <see cref="GameStringsLocale"/>, which is the locale of the gamestring.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <param name="gameStringText">When this method returns, contains the parsed <see cref="GameStringText"/>, if the gamestring was found; otherwise, <see langword="null"/>.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <returns><see langword="true"/> if the gamestring was found; otherwise <see langword="false"/>, in the same cases as <see cref="GameStringExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetParsedGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out GameStringText? gameStringText, bool extractFontValues = false)
    {
        if (!TryGetGameString(id, out string? gameString))
        {
            gameStringText = null;
            return false;
        }

        gameStringText = ParseGameString(gameString, null, extractFontValues);
        return true;
    }

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>.
    /// <para>
    /// This is the same as <see cref="GetGameString(ReadOnlySpan{char})"/> followed by <see cref="ParseGameString(ReadOnlySpan{char}, StormLocale?, bool)"/>
    /// with the <see cref="GameStringsLocale"/>, which is the locale of the gamestring.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The gamestring was not found (in the same cases that <see cref="GameStringExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public GameStringText GetParsedGameString(ReadOnlySpan<char> id, bool extractFontValues = false)
    {
        if (TryGetParsedGameString(id, out GameStringText? gameStringText, extractFontValues))
            return gameStringText;

        throw new KeyNotFoundException($"Could not find the gamestring: {id}");
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

    /// <summary>
    /// Determines if a <c>.stormlayout</c> file exists.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormdata</c> directory, which starts with <c>ui</c> (e.g. <c>ui/layout/hero/herotalents.stormlayout</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="relativePath"/> being empty.</returns>
    public bool StormLayoutFileExists(ReadOnlySpan<char> relativePath) => _layeredDataStorage.Files.TryGetFirstStormLayoutFileByRelativePath(relativePath, out _);

    /// <summary>
    /// Gets a <c>.stormlayout</c> file, if it exists.
    /// <para>
    /// If more than one <c>stormmod</c> has the file, the one from the highest layer is used (custom, then map, then base).
    /// To open it, use <see cref="HeroesLoader.GetFile(StormFile)"/> or <see cref="HeroesLoader.TryGetFile(StormFile?, out Stream?)"/>.
    /// </para>
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormdata</c> directory, which starts with <c>ui</c> (e.g. <c>ui/layout/hero/herotalents.stormlayout</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>, in the same cases as <see cref="StormLayoutFileExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetStormLayoutFile(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile) =>
        _layeredDataStorage.Files.TryGetFirstStormLayoutFileByRelativePath(relativePath, out stormFile);

    /// <summary>
    /// Gets a <c>.stormlayout</c> file.
    /// <para>
    /// If more than one <c>stormmod</c> has the file, the one from the highest layer is used (custom, then map, then base).
    /// To open it, use <see cref="HeroesLoader.GetFile(StormFile)"/> or <see cref="HeroesLoader.TryGetFile(StormFile?, out Stream?)"/>.
    /// </para>
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormdata</c> directory, which starts with <c>ui</c> (e.g. <c>ui/layout/hero/herotalents.stormlayout</c>).
    /// This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <returns>The <see cref="StormFile"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The file was not found (in the same cases that <see cref="StormLayoutFileExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormFile GetStormLayoutFile(ReadOnlySpan<char> relativePath)
    {
        if (TryGetStormLayoutFile(relativePath, out StormFile? stormFile))
            return stormFile;

        throw new KeyNotFoundException($"Could not find the stormlayout file: {relativePath}");
    }
}
