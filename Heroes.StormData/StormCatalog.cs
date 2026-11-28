namespace Heroes.StormData;

/// <summary>
/// Gives read access to the loaded Heroes of the Storm data (the XML elements, gamestrings, styles, and files),
/// merged across the base, map, and custom <c>stormmods</c>.
/// </summary>
/// <remarks>
/// <para>
/// An instance is a live view of the data of the <see cref="HeroesLoader"/> that owns it,
/// so it reflects whatever is currently loaded and changes as the loader loads and unloads data.
/// </para>
/// <para>
/// This class is not thread-safe: looking up an element (and parsing a gamestring, which looks up elements) can add to an internal cache,
/// so its methods must not be called from more than one thread at the same time, or while the <see cref="HeroesLoader"/> is loading or unloading data.
/// </para>
/// </remarks>
public sealed class StormCatalog
{
    // the type letter that class element names start with in the XML (e.g. the C of CEffectDamage)
    private const char ClassElementDeclaration = 'C';
    private const string ClassElementDeclarationString = "C";

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
    /// This is the locale that the <c>ParseGameString</c> overloads without a locale, such as <see cref="ParseGameString(ReadOnlySpan{char})"/>, use.
    /// </para>
    /// </summary>
    public StormLocale? GameStringsLocale { get; internal set; }

    /// <summary>
    /// Parses an unparsed gamestring into a <see cref="GameStringText"/>, replacing its data tags (<c>&lt;d ... /&gt;</c>) with their values.
    /// Its locale is the <see cref="GameStringsLocale"/>, or <see cref="StormLocale.ENUS"/> if no gamestrings are loaded, and its font values are not extracted.
    /// <para>
    /// The values are calculated from the currently loaded XML data, so the same gamestring can parse to different text after a <c>mapmod</c> is loaded or unloaded.
    /// </para>
    /// </summary>
    /// <param name="gameString">The unparsed gamestring text (not its <c>id</c>). Can be from <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> or any other source.</param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    public GameStringText ParseGameString(ReadOnlySpan<char> gameString) => ParseGameStringCore(gameString, GetDefaultGameStringLocale(), false);

    /// <summary>
    /// Parses an unparsed gamestring into a <see cref="GameStringText"/>, replacing its data tags (<c>&lt;d ... /&gt;</c>) with their values.
    /// Its locale is the <see cref="GameStringsLocale"/>, or <see cref="StormLocale.ENUS"/> if no gamestrings are loaded.
    /// <para>
    /// The values are calculated from the currently loaded XML data, so the same gamestring can parse to different text after a <c>mapmod</c> is loaded or unloaded.
    /// </para>
    /// </summary>
    /// <param name="gameString">The unparsed gamestring text (not its <c>id</c>). Can be from <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> or any other source.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    public GameStringText ParseGameString(ReadOnlySpan<char> gameString, bool extractFontValues) => ParseGameStringCore(gameString, GetDefaultGameStringLocale(), extractFontValues);

    /// <summary>
    /// Parses an unparsed gamestring of a locale into a <see cref="GameStringText"/>, replacing its data tags (<c>&lt;d ... /&gt;</c>) with their values.
    /// Its font values are not extracted.
    /// <para>
    /// The values are calculated from the currently loaded XML data, so the same gamestring can parse to different text after a <c>mapmod</c> is loaded or unloaded.
    /// </para>
    /// </summary>
    /// <param name="gameString">The unparsed gamestring text (not its <c>id</c>). Can be from <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> or any other source.</param>
    /// <param name="locale">The locale of <paramref name="gameString"/>, used to format the numbers written into the text and for the scaling text.</param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="locale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    public GameStringText ParseGameString(ReadOnlySpan<char> gameString, StormLocale locale) => ParseGameStringCore(gameString, locale, false);

    /// <summary>
    /// Parses an unparsed gamestring of a locale into a <see cref="GameStringText"/>, replacing its data tags (<c>&lt;d ... /&gt;</c>) with their values.
    /// <para>
    /// The values are calculated from the currently loaded XML data, so the same gamestring can parse to different text after a <c>mapmod</c> is loaded or unloaded.
    /// </para>
    /// </summary>
    /// <param name="gameString">The unparsed gamestring text (not its <c>id</c>). Can be from <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> or any other source.</param>
    /// <param name="locale">The locale of <paramref name="gameString"/>, used to format the numbers written into the text and for the scaling text.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="locale"/> is not a defined <see cref="StormLocale"/> value.</exception>
    public GameStringText ParseGameString(ReadOnlySpan<char> gameString, StormLocale locale, bool extractFontValues) => ParseGameStringCore(gameString, locale, extractFontValues);

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>, if it exists.
    /// <para>
    /// This is the same as <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> followed by <see cref="ParseGameString(ReadOnlySpan{char})"/>
    /// with the <see cref="GameStringsLocale"/>, which is the locale of the gamestring.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <param name="gameStringText">
    /// When this method returns, contains the parsed <see cref="GameStringText"/>, if the gamestring was found; otherwise, <see langword="null"/>.
    /// Its font values are not extracted (see <see cref="TryGetParsedGameString(ReadOnlySpan{char}, bool, out GameStringText?)"/>).
    /// </param>
    /// <returns><see langword="true"/> if the gamestring was found; otherwise <see langword="false"/>, in the same cases as <see cref="GameStringExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetParsedGameString(ReadOnlySpan<char> id, [NotNullWhen(true)] out GameStringText? gameStringText) => TryGetParsedGameString(id, false, out gameStringText);

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>, if it exists.
    /// <para>
    /// This is the same as <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/> followed by <see cref="ParseGameString(ReadOnlySpan{char}, bool)"/>
    /// with the <see cref="GameStringsLocale"/>, which is the locale of the gamestring.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <param name="extractFontValues">
    /// If <see langword="true"/>, the <c>val</c> values of the font style and constant tags are saved in <see cref="GameStringText.FontStyleValues"/> and <see cref="GameStringText.FontConstantValues"/>.
    /// If the output with color tags is not needed, set to <see langword="false"/> for faster parsing.
    /// </param>
    /// <param name="gameStringText">When this method returns, contains the parsed <see cref="GameStringText"/>, if the gamestring was found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the gamestring was found; otherwise <see langword="false"/>, in the same cases as <see cref="GameStringExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetParsedGameString(ReadOnlySpan<char> id, bool extractFontValues, [NotNullWhen(true)] out GameStringText? gameStringText)
    {
        if (!TryGetGameString(id, out string? gameString))
        {
            gameStringText = null;
            return false;
        }

        gameStringText = ParseGameString(gameString, extractFontValues);
        return true;
    }

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>. Its font values are not extracted.
    /// <para>
    /// This is the same as <see cref="GetGameString(ReadOnlySpan{char})"/> followed by <see cref="ParseGameString(ReadOnlySpan{char})"/>
    /// with the <see cref="GameStringsLocale"/>, which is the locale of the gamestring.
    /// </para>
    /// </summary>
    /// <param name="id">The <c>id</c> of the gamestring (e.g. <c>Button/Name/AlarakDeadlyChargeActivate</c>). This is case-sensitive.</param>
    /// <returns>The parsed <see cref="GameStringText"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The gamestring was not found (in the same cases that <see cref="GameStringExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public GameStringText GetParsedGameString(ReadOnlySpan<char> id) => GetParsedGameString(id, false);

    /// <summary>
    /// Gets a gamestring (from a <c>gamestrings.txt</c> file) by its <c>id</c> and parses it into a <see cref="GameStringText"/>.
    /// <para>
    /// This is the same as <see cref="GetGameString(ReadOnlySpan{char})"/> followed by <see cref="ParseGameString(ReadOnlySpan{char}, bool)"/>
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
    public GameStringText GetParsedGameString(ReadOnlySpan<char> id, bool extractFontValues)
    {
        if (TryGetParsedGameString(id, extractFontValues, out GameStringText? gameStringText))
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
    /// Gets all the loaded gamestrings (from the <c>gamestrings.txt</c> files), unparsed, by their <c>id</c>.
    /// <para>
    /// If more than one <c>stormmod</c> has a gamestring, the value from the highest layer is used (custom, then map, then base),
    /// the same as <see cref="TryGetGameString(ReadOnlySpan{char}, out string?)"/>. They are in the <see cref="GameStringsLocale"/>.
    /// To parse one, use <see cref="ParseGameString(ReadOnlySpan{char})"/>.
    /// </para>
    /// </summary>
    /// <returns>
    /// A new dictionary of the unparsed gamestrings by their <c>id</c> (case-sensitive), or an empty one if no gamestrings are loaded.
    /// Loading or unloading afterwards does not change it.
    /// </returns>
    public IReadOnlyDictionary<string, string> GetGameStrings() => _layeredDataStorage.Texts.GetGameStrings();

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

    /// <summary>
    /// Determines if an asset file (such as a <c>.dds</c> image) exists.
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormassets</c> directory, which starts with <c>assets</c> (e.g. <c>assets/textures/storm_ui_icon_abathur.dds</c>),
    /// the same as the asset paths in the XML and <c>assets.txt</c> files. This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <returns><see langword="true"/> if the file exists; otherwise <see langword="false"/>, which includes <paramref name="relativePath"/> being empty.</returns>
    public bool AssetFileExists(ReadOnlySpan<char> relativePath) => _layeredDataStorage.Files.TryGetFirstAssetFileByRelativePath(relativePath, out _);

    /// <summary>
    /// Gets an asset file (such as a <c>.dds</c> image), if it exists.
    /// <para>
    /// If more than one <c>stormmod</c> has the file, the one from the highest layer is used (custom, then map, then base).
    /// To open it, use <see cref="HeroesLoader.GetFile(StormFile)"/> or <see cref="HeroesLoader.TryGetFile(StormFile?, out Stream?)"/>.
    /// </para>
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormassets</c> directory, which starts with <c>assets</c> (e.g. <c>assets/textures/storm_ui_icon_abathur.dds</c>),
    /// the same as the asset paths in the XML and <c>assets.txt</c> files. This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <param name="stormFile">When this method returns, contains the <see cref="StormFile"/>, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the file was found; otherwise <see langword="false"/>, in the same cases as <see cref="AssetFileExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetAssetFile(ReadOnlySpan<char> relativePath, [NotNullWhen(true)] out StormFile? stormFile) =>
        _layeredDataStorage.Files.TryGetFirstAssetFileByRelativePath(relativePath, out stormFile);

    /// <summary>
    /// Gets an asset file (such as a <c>.dds</c> image).
    /// <para>
    /// If more than one <c>stormmod</c> has the file, the one from the highest layer is used (custom, then map, then base).
    /// To open it, use <see cref="HeroesLoader.GetFile(StormFile)"/> or <see cref="HeroesLoader.TryGetFile(StormFile?, out Stream?)"/>.
    /// </para>
    /// </summary>
    /// <param name="relativePath">
    /// The path of the file relative to the <c>base.stormassets</c> directory, which starts with <c>assets</c> (e.g. <c>assets/textures/storm_ui_icon_abathur.dds</c>),
    /// the same as the asset paths in the XML and <c>assets.txt</c> files. This is case-insensitive and either <c>/</c> or <c>\</c> can be used as the directory separator.
    /// </param>
    /// <returns>The <see cref="StormFile"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The file was not found (in the same cases that <see cref="AssetFileExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormFile GetAssetFile(ReadOnlySpan<char> relativePath)
    {
        if (TryGetAssetFile(relativePath, out StormFile? stormFile))
            return stormFile;

        throw new KeyNotFoundException($"Could not find the asset file: {relativePath}");
    }

    /// <summary>
    /// Gets the catalog name (e.g. <c>Effect</c>) of an element name (e.g. <c>CEffectDamage</c>), if it is known.
    /// <para>
    /// If more than one <c>stormmod</c> has the element name, the catalog name from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <param name="catalogName">When this method returns, contains the catalog name, if found; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the catalog name was found; otherwise <see langword="false"/>, which includes <paramref name="elementName"/> not starting with <c>C</c>
    /// (such as <c>EffectDamage</c>) or being empty.
    /// </returns>
    public bool TryGetCatalogName(ReadOnlySpan<char> elementName, [NotNullWhen(true)] out string? catalogName)
    {
        if (!TryRemoveClassElementDeclaration(elementName, out ReadOnlySpan<char> storedElementName))
        {
            catalogName = null;
            return false;
        }

        return _layeredDataStorage.Elements.TryGetFirstCatalogNameByElementName(storedElementName, out catalogName);
    }

    /// <summary>
    /// Gets the catalog name (e.g. <c>Effect</c>) of an element name (e.g. <c>CEffectDamage</c>).
    /// <para>
    /// If more than one <c>stormmod</c> has the element name, the catalog name from the highest layer is used (custom, then map, then base).
    /// </para>
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <returns>The catalog name.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The catalog name was not found (in the same cases that <see cref="TryGetCatalogName(ReadOnlySpan{char}, out string?)"/> returns <see langword="false"/>).
    /// </exception>
    public string GetCatalogName(ReadOnlySpan<char> elementName)
    {
        if (TryGetCatalogName(elementName, out string? catalogName))
            return catalogName;

        throw new KeyNotFoundException($"Could not find the catalog name of the element: {elementName}");
    }

    /// <summary>
    /// Gets the element names (e.g. <c>CEffectDamage</c>) of a catalog name (e.g. <c>Effect</c>), from all the loaded <c>stormmods</c>.
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <returns>
    /// A new list of the element names as written in the XML, with their leading <c>C</c> (the form that <see cref="TryGetCatalogName(ReadOnlySpan{char}, out string?)"/> takes),
    /// sorted by <see cref="StringComparer.OrdinalIgnoreCase"/>. It is empty if the catalog name is not found.
    /// </returns>
    public IReadOnlyList<string> GetElementNames(ReadOnlySpan<char> catalogName)
    {
        HashSet<string> elementNames = _layeredDataStorage.Elements.GetClassElementNamesByCatalogName(catalogName);

        List<string> elementNamesWithDeclaration = new(elementNames.Count);

        foreach (string elementName in elementNames)
            elementNamesWithDeclaration.Add(string.Concat(ClassElementDeclarationString, elementName));

        elementNamesWithDeclaration.Sort(StringComparer.OrdinalIgnoreCase);

        return elementNamesWithDeclaration;
    }

    /// <summary>
    /// Determines if an element with an <c>id</c> exists in a catalog.
    /// <para>
    /// Only the element's own <c>id</c> is checked, nothing is merged, so this is cheaper than <see cref="TryGetElement(ReadOnlySpan{char}, ReadOnlySpan{char}, out StormXmlElement?)"/>.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <param name="id">The <c>id</c> of the element (e.g. <c>GuldanHorrifyDamage</c>). This is case-sensitive.</param>
    /// <returns><see langword="true"/> if any of the loaded <c>stormmods</c> has the element; otherwise <see langword="false"/>, which includes <paramref name="id"/> being empty.</returns>
    public bool ElementExists(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> id) => _layeredDataStorage.Elements.ContainsStormXmlElementById(id, catalogName);

    /// <summary>
    /// Gets an element with an <c>id</c> from a catalog, if it exists, merged into the complete element that the game uses.
    /// <para>
    /// The element is merged from all the loaded <c>stormmods</c> (base, then map, then custom), its <c>parent</c> elements, and its default element (the element with no <c>id</c>),
    /// so values it inherits are included.
    /// </para>
    /// <para>
    /// The merged element is cached, so the same instance can be returned on later calls until the loaded data changes. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <param name="id">The <c>id</c> of the element (e.g. <c>GuldanHorrifyDamage</c>). This is case-sensitive.</param>
    /// <param name="element">When this method returns, contains the merged element, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the element was found; otherwise <see langword="false"/>, in the same cases as <see cref="ElementExists(ReadOnlySpan{char}, ReadOnlySpan{char})"/>.</returns>
    public bool TryGetElement(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> id, [NotNullWhen(true)] out StormXmlElement? element)
    {
        // checked first, so a missing id is not merged or cached
        if (!ElementExists(catalogName, id))
        {
            element = null;
            return false;
        }

        element = _layeredDataStorage.GetMergedStormXmlElementById(id, catalogName);
        return element is not null;
    }

    /// <summary>
    /// Gets an element with an <c>id</c> from a catalog, merged into the complete element that the game uses.
    /// <para>
    /// The element is merged from all the loaded <c>stormmods</c> (base, then map, then custom), its <c>parent</c> elements, and its default element (the element with no <c>id</c>),
    /// so values it inherits are included.
    /// </para>
    /// <para>
    /// The merged element is cached, so the same instance can be returned on later calls until the loaded data changes. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Effect</c>). This is case-insensitive.</param>
    /// <param name="id">The <c>id</c> of the element (e.g. <c>GuldanHorrifyDamage</c>). This is case-sensitive.</param>
    /// <returns>The merged element.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The element was not found (in the same cases that <see cref="ElementExists(ReadOnlySpan{char}, ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormXmlElement GetElement(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> id)
    {
        if (TryGetElement(catalogName, id, out StormXmlElement? element))
            return element;

        throw new KeyNotFoundException($"Could not find the element: {catalogName},{id}");
    }

    /// <summary>
    /// Gets the <c>id</c>s of the elements of a catalog (e.g. every <c>Unit</c>), from all the loaded <c>stormmods</c>.
    /// <para>
    /// Only elements with an <c>id</c> are included, not default elements. Each <c>id</c> can be passed to
    /// <see cref="TryGetElement(ReadOnlySpan{char}, ReadOnlySpan{char}, out StormXmlElement?)"/> with the same <paramref name="catalogName"/>.
    /// To get them from one layer, use <see cref="GetElementIds(ReadOnlySpan{char}, StormModType)"/>.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Unit</c>). This is case-insensitive.</param>
    /// <returns>
    /// A new list of the <c>id</c>s, each once, sorted by <see cref="StringComparer.Ordinal"/>, or an empty list if the catalog has no elements.
    /// Loading or unloading afterwards does not change it.
    /// </returns>
    public IReadOnlyList<string> GetElementIds(ReadOnlySpan<char> catalogName) => GetElementIdsCore(catalogName, null);

    /// <summary>
    /// Gets the <c>id</c>s of the elements of a catalog (e.g. every <c>Unit</c>) from one layer, such as <see cref="StormModType.Map"/> for the elements a <c>mapmod</c> adds or overrides.
    /// <para>
    /// Only elements with an <c>id</c> are included, not default elements. Each <c>id</c> can be passed to
    /// <see cref="TryGetElement(ReadOnlySpan{char}, ReadOnlySpan{char}, out StormXmlElement?)"/> with the same <paramref name="catalogName"/>.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name (e.g. <c>Unit</c>). This is case-insensitive.</param>
    /// <param name="stormModType">The layer to get the <c>id</c>s from.</param>
    /// <returns>
    /// A new list of the <c>id</c>s, each once, sorted by <see cref="StringComparer.Ordinal"/>, or an empty list if the layer has no elements of the catalog.
    /// Loading or unloading afterwards does not change it.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stormModType"/> is not a defined <see cref="StormModType"/> value.</exception>
    public IReadOnlyList<string> GetElementIds(ReadOnlySpan<char> catalogName, StormModType stormModType)
    {
        if (!Enum.IsDefined(stormModType))
            throw new ArgumentOutOfRangeException(nameof(stormModType), stormModType, $"The value is not a defined {nameof(StormModType)}.");

        return GetElementIdsCore(catalogName, stormModType);
    }

    /// <summary>
    /// Gets the <c>id</c> of the element of a catalog that is for a unit, by its <c>unitName</c> attribute, if there is one.
    /// For example, the actor of the <c>HeroMuradin</c> unit is <c>&lt;CActorUnit id="HeroMuradin" unitName="HeroMuradin"&gt;</c>.
    /// In the game data, the actors (<c>CActorUnit</c> and <c>CActorMissile</c>) are the elements that have a <c>unitName</c>.
    /// <para>
    /// If more than one element has the unit name, the one from the highest layer is used (custom, then map, then base), and within a layer the one loaded last.
    /// Only an element's own <c>unitName</c> attributes are used, not one it inherits from its <c>parent</c>.
    /// An element whose <c>unitName</c> a later definition changed (e.g. a map that renames it) is only found by its new unit name.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Actor</c>). This is case-insensitive.</param>
    /// <param name="unitName">The <c>id</c> of the unit (e.g. <c>HeroMuradin</c>), the value of the <c>unitName</c> attribute. This is case-sensitive.</param>
    /// <param name="id">When this method returns, contains the <c>id</c> of the element, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an element has the unit name; otherwise <see langword="false"/>, which includes <paramref name="unitName"/> being empty.</returns>
    public bool TryGetElementIdByUnitName(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> unitName, [NotNullWhen(true)] out string? id)
    {
        // highest layer first. A layer has the unit name a definition of the element had, so the merged element is checked to still have it,
        // as a later definition can have changed it (then a lower layer's element can still be the one for the unit)
        foreach (StormModType layer in (ReadOnlySpan<StormModType>)[StormModType.Custom, StormModType.Map, StormModType.Base])
        {
            if (_layeredDataStorage.Elements.TryGetIdByUnitNameInLayer(layer, unitName, catalogName, out id) && HasUnitName(catalogName, id, unitName))
                return true;
        }

        id = null;
        return false;
    }

    /// <summary>
    /// Gets the <c>id</c> of the element of a catalog that is for a unit, by its <c>unitName</c> attribute.
    /// For example, the actor of the <c>HeroMuradin</c> unit is <c>&lt;CActorUnit id="HeroMuradin" unitName="HeroMuradin"&gt;</c>.
    /// In the game data, the actors (<c>CActorUnit</c> and <c>CActorMissile</c>) are the elements that have a <c>unitName</c>.
    /// <para>
    /// If more than one element has the unit name, the one from the highest layer is used (custom, then map, then base), and within a layer the one loaded last.
    /// Only an element's own <c>unitName</c> attributes are used, not one it inherits from its <c>parent</c>.
    /// An element whose <c>unitName</c> a later definition changed (e.g. a map that renames it) is only found by its new unit name.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Actor</c>). This is case-insensitive.</param>
    /// <param name="unitName">The <c>id</c> of the unit (e.g. <c>HeroMuradin</c>), the value of the <c>unitName</c> attribute. This is case-sensitive.</param>
    /// <returns>The <c>id</c> of the element.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No element has the unit name (in the same cases that <see cref="TryGetElementIdByUnitName(ReadOnlySpan{char}, ReadOnlySpan{char}, out string?)"/> returns <see langword="false"/>).
    /// </exception>
    public string GetElementIdByUnitName(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> unitName)
    {
        if (TryGetElementIdByUnitName(catalogName, unitName, out string? id))
            return id;

        throw new KeyNotFoundException($"Could not find an element with the unit name: {catalogName},{unitName}");
    }

    /// <summary>
    /// Determines if a default element exists. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if any of the loaded <c>stormmods</c> has the default element; otherwise <see langword="false"/>, which includes <paramref name="elementName"/>
    /// not starting with <c>C</c> (such as <c>EffectDamage</c>) or being empty. Nothing is merged, so this is cheaper than
    /// <see cref="TryGetDefaultElement(ReadOnlySpan{char}, out StormXmlElement?)"/>.
    /// </returns>
    public bool DefaultElementExists(ReadOnlySpan<char> elementName) =>
        TryRemoveClassElementDeclaration(elementName, out ReadOnlySpan<char> storedElementName) && _layeredDataStorage.Elements.ContainsStormXmlElementByClassElement(storedElementName);

    /// <summary>
    /// Gets a default element, if it exists. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom), then over the default elements of the classes it derives from:
    /// its catalog's base class and each class in between (e.g. <c>CValidator</c> and <c>CValidatorUnit</c> for <c>CValidatorUnitAbil</c>), so the defaults it inherits are included.
    /// This is the same default element that <see cref="TryGetElement(ReadOnlySpan{char}, ReadOnlySpan{char}, out StormXmlElement?)"/> merges an element into.
    /// </para>
    /// <para>
    /// It is not cached: it can be the stored instance or a new merged copy, so later calls don't always return the same instance. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <param name="element">When this method returns, contains the default element, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the default element was found; otherwise <see langword="false"/>, in the same cases as <see cref="DefaultElementExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetDefaultElement(ReadOnlySpan<char> elementName, [NotNullWhen(true)] out StormXmlElement? element)
    {
        if (!TryRemoveClassElementDeclaration(elementName, out ReadOnlySpan<char> storedElementName))
        {
            element = null;
            return false;
        }

        element = _layeredDataStorage.GetMergedDefaultStormXmlElement(storedElementName);
        return element is not null;
    }

    /// <summary>
    /// Gets a default element. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom), then over the default elements of the classes it derives from:
    /// its catalog's base class and each class in between (e.g. <c>CValidator</c> and <c>CValidatorUnit</c> for <c>CValidatorUnitAbil</c>), so the defaults it inherits are included.
    /// This is the same default element that <see cref="TryGetElement(ReadOnlySpan{char}, ReadOnlySpan{char}, out StormXmlElement?)"/> merges an element into.
    /// </para>
    /// <para>
    /// It is not cached: it can be the stored instance or a new merged copy, so later calls don't always return the same instance. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <returns>The default element.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The default element was not found (in the same cases that <see cref="DefaultElementExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormXmlElement GetDefaultElement(ReadOnlySpan<char> elementName)
    {
        if (TryGetDefaultElement(elementName, out StormXmlElement? element))
            return element;

        throw new KeyNotFoundException($"Could not find the default element: {elementName}");
    }

    /// <summary>
    /// Gets the per-level scaling of an element's field (from a <c>LevelScalingArray</c> modification), if it has one.
    /// For example, a hero unit's <c>LifeMax</c> with a scaling of 0.04 increases by 4% per level.
    /// <para>
    /// A modification matches if its <c>Field</c> is the same text as <paramref name="field"/>, or if both refer to the same value of the merged element,
    /// so different spellings of the same field (such as with and without an index) match.
    /// If more than one <c>stormmod</c> has a matching modification, the one from the highest layer is used (custom, then map, then base).
    /// This is the same scaling that the gamestrings use for the field's data references.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Unit</c>). This is case-insensitive.</param>
    /// <param name="id">The <c>id</c> of the element (e.g. <c>HeroMuradin</c>). This is case-sensitive.</param>
    /// <param name="field">The field (e.g. <c>LifeMax</c> or <c>Modification.DamageDealtScaled[Spell]</c>), the same as in a data reference.</param>
    /// <param name="scaling">
    /// When this method returns, contains the scaling, if found; otherwise, the default value.
    /// Its <see cref="StormNumber.Value"/> is as written in the data (e.g. 0.04) and its <see cref="StormNumber.FixedValue"/> is what the game stores (e.g. 0.0400390625).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the field has a scaling; otherwise <see langword="false"/>, which includes the element not being found
    /// and the modification's <c>Value</c> not being a number.
    /// </returns>
    public bool TryGetScaling(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> id, ReadOnlySpan<char> field, out StormNumber scaling)
    {
        // the fields are resolved on the merged element, as in a data reference
        if (!TryGetElement(catalogName, id, out StormXmlElement? element))
        {
            scaling = default;
            return false;
        }

        FieldNode fieldNode = DataRefFieldParser.ParseField(element, field);

        if (!LevelScalingResolver.TryGetScaling(_layeredDataStorage.Elements, element, catalogName, id, field, fieldNode, out double value))
        {
            scaling = default;
            return false;
        }

        scaling = StormNumber.FromDataValue(value);
        return true;
    }

    /// <summary>
    /// Gets the per-level scaling of an element's field (from a <c>LevelScalingArray</c> modification).
    /// For example, a hero unit's <c>LifeMax</c> with a scaling of 0.04 increases by 4% per level.
    /// <para>
    /// A modification matches if its <c>Field</c> is the same text as <paramref name="field"/>, or if both refer to the same value of the merged element,
    /// so different spellings of the same field (such as with and without an index) match.
    /// If more than one <c>stormmod</c> has a matching modification, the one from the highest layer is used (custom, then map, then base).
    /// This is the same scaling that the gamestrings use for the field's data references.
    /// </para>
    /// </summary>
    /// <param name="catalogName">The catalog name of the element (e.g. <c>Unit</c>). This is case-insensitive.</param>
    /// <param name="id">The <c>id</c> of the element (e.g. <c>HeroMuradin</c>). This is case-sensitive.</param>
    /// <param name="field">The field (e.g. <c>LifeMax</c> or <c>Modification.DamageDealtScaled[Spell]</c>), the same as in a data reference.</param>
    /// <returns>
    /// The scaling. Its <see cref="StormNumber.Value"/> is as written in the data (e.g. 0.04) and its <see cref="StormNumber.FixedValue"/> is what the game stores (e.g. 0.0400390625).
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// The field has no scaling (in the same cases that <see cref="TryGetScaling(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, out StormNumber)"/> returns <see langword="false"/>).
    /// </exception>
    public StormNumber GetScaling(ReadOnlySpan<char> catalogName, ReadOnlySpan<char> id, ReadOnlySpan<char> field)
    {
        if (TryGetScaling(catalogName, id, field, out StormNumber scaling))
            return scaling;

        throw new KeyNotFoundException($"Could not find the scaling: {catalogName},{id},{field}");
    }

    /// <summary>
    /// Determines if a style constant (a <c>Constant</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Constant name="ColorGray" val="a7a7a7" /&gt;</c>) exists.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant, without the leading <c>#</c> used to reference it (e.g. <c>ColorGray</c>). This is case-sensitive.</param>
    /// <returns><see langword="true"/> if any of the loaded <c>stormmods</c> has the style constant; otherwise <see langword="false"/>, which includes <paramref name="name"/> being empty.</returns>
    public bool StyleConstantExists(ReadOnlySpan<char> name) => _layeredDataStorage.Styles.ContainsStormXmlStyleConstantElementByName(name);

    /// <summary>
    /// Gets a style constant (a <c>Constant</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Constant name="ColorGray" val="a7a7a7" /&gt;</c>), if it exists.
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom). Its <see cref="StormXmlStyleConstantElement.Value"/> is as written,
    /// so it can be a reference to another constant (e.g. <c>#ColorChatLobby</c>). To follow the references to the final value, use
    /// <see cref="TryGetStyleConstantValue(ReadOnlySpan{char}, out string?)"/>.
    /// It is not cached: it can be the stored instance or a new merged copy, so later calls don't always return the same instance. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant, without the leading <c>#</c> used to reference it (e.g. <c>ColorGray</c>). This is case-sensitive.</param>
    /// <param name="constant">When this method returns, contains the style constant, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the style constant was found; otherwise <see langword="false"/>, in the same cases as <see cref="StyleConstantExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetStyleConstant(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlStyleConstantElement? constant)
    {
        constant = _layeredDataStorage.Styles.GetStormXmlStyleConstantElementByName(name);
        return constant is not null;
    }

    /// <summary>
    /// Gets a style constant (a <c>Constant</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Constant name="ColorGray" val="a7a7a7" /&gt;</c>).
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom). Its <see cref="StormXmlStyleConstantElement.Value"/> is as written,
    /// so it can be a reference to another constant (e.g. <c>#ColorChatLobby</c>). To follow the references to the final value, use
    /// <see cref="TryGetStyleConstantValue(ReadOnlySpan{char}, out string?)"/>.
    /// It is not cached: it can be the stored instance or a new merged copy, so later calls don't always return the same instance. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant, without the leading <c>#</c> used to reference it (e.g. <c>ColorGray</c>). This is case-sensitive.</param>
    /// <returns>The style constant.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The style constant was not found (in the same cases that <see cref="StyleConstantExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormXmlStyleConstantElement GetStyleConstant(ReadOnlySpan<char> name)
    {
        if (TryGetStyleConstant(name, out StormXmlStyleConstantElement? constant))
            return constant;

        throw new KeyNotFoundException($"Could not find the style constant: {name}");
    }

    /// <summary>
    /// Gets the final value of a style constant, following its references to other constants (a value that starts with <c>#</c>),
    /// e.g. <c>ColorInGameAll</c> is <c>#ColorChatLobby</c>, so its value is <c>ColorChatLobby</c>'s value.
    /// <para>
    /// This is also how to get the value of a reference in a style, such as a <c>textcolor</c> of <c>#ColorGray</c>: pass the name without the <c>#</c>.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style constant, without the leading <c>#</c> used to reference it (e.g. <c>ColorGray</c>). This is case-sensitive.</param>
    /// <param name="value">When this method returns, contains the final value (it does not start with <c>#</c>), if found; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the style constant and every constant it references were found; otherwise <see langword="false"/>,
    /// which includes a reference to a constant that does not exist and references that loop back to an earlier constant.
    /// </returns>
    public bool TryGetStyleConstantValue(ReadOnlySpan<char> name, [NotNullWhen(true)] out string? value)
    {
        value = null;

        if (!TryGetStyleConstant(name, out StormXmlStyleConstantElement? constant))
            return false;

        string currentValue = constant.Value;
        HashSet<string>? referencedNames = null;

        while (currentValue.StartsWith('#'))
        {
            // most constants are not references, so the set is only made for the ones that are
            referencedNames ??= new HashSet<string>(StringComparer.Ordinal) { constant.Name };

            string referencedName = currentValue[1..];

            if (!referencedNames.Add(referencedName) || !TryGetStyleConstant(referencedName, out constant))
                return false;

            currentValue = constant.Value;
        }

        value = currentValue;
        return true;
    }

    /// <summary>
    /// Determines if a style (a <c>Style</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Style name="TooltipSubscript" template="StandardTooltip" /&gt;</c>) exists.
    /// </summary>
    /// <param name="name">The <c>name</c> of the style (e.g. <c>TooltipSubscript</c>). This is case-sensitive.</param>
    /// <returns><see langword="true"/> if any of the loaded <c>stormmods</c> has the style; otherwise <see langword="false"/>, which includes <paramref name="name"/> being empty.</returns>
    public bool StyleExists(ReadOnlySpan<char> name) => _layeredDataStorage.Styles.ContainsStormXmlStyleElementByName(name);

    /// <summary>
    /// Gets a style (a <c>Style</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Style name="TooltipSubscript" template="StandardTooltip" /&gt;</c>), if it exists,
    /// merged with its <c>template</c> chain so the attributes it inherits are included.
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom), then over its template, that template's template, and so on,
    /// so a style that has no <c>textcolor</c> of its own has its template's. The style's own attributes override its template's, and it keeps its own <c>name</c> and <c>template</c>.
    /// </para>
    /// <para>
    /// Its attribute values are as written, so a color can be a reference to a style constant (e.g. <c>#ColorGray</c>),
    /// which <see cref="TryGetStyleConstantValue(ReadOnlySpan{char}, out string?)"/> gets the value of. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style (e.g. <c>TooltipSubscript</c>). This is case-sensitive.</param>
    /// <param name="style">When this method returns, contains the merged style, if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the style was found; otherwise <see langword="false"/>, in the same cases as <see cref="StyleExists(ReadOnlySpan{char})"/>.</returns>
    public bool TryGetStyle(ReadOnlySpan<char> name, [NotNullWhen(true)] out StormXmlStyleElement? style)
    {
        style = _layeredDataStorage.Styles.GetMergedStormXmlStyleElementByName(name);
        return style is not null;
    }

    /// <summary>
    /// Gets a style (a <c>Style</c> element from a <c>.stormstyle</c> file, e.g. <c>&lt;Style name="TooltipSubscript" template="StandardTooltip" /&gt;</c>),
    /// merged with its <c>template</c> chain so the attributes it inherits are included.
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom), then over its template, that template's template, and so on,
    /// so a style that has no <c>textcolor</c> of its own has its template's. The style's own attributes override its template's, and it keeps its own <c>name</c> and <c>template</c>.
    /// </para>
    /// <para>
    /// Its attribute values are as written, so a color can be a reference to a style constant (e.g. <c>#ColorGray</c>),
    /// which <see cref="TryGetStyleConstantValue(ReadOnlySpan{char}, out string?)"/> gets the value of. Its public members are read-only.
    /// </para>
    /// </summary>
    /// <param name="name">The <c>name</c> of the style (e.g. <c>TooltipSubscript</c>). This is case-sensitive.</param>
    /// <returns>The merged style.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The style was not found (in the same cases that <see cref="StyleExists(ReadOnlySpan{char})"/> returns <see langword="false"/>).
    /// </exception>
    public StormXmlStyleElement GetStyle(ReadOnlySpan<char> name)
    {
        if (TryGetStyle(name, out StormXmlStyleElement? style))
            return style;

        throw new KeyNotFoundException($"Could not find the style: {name}");
    }

    /// <summary>
    /// Gets the <c>id</c>s of the ability links by their index, which is the index the game uses for an ability (e.g. the <c>m_abilLink</c> of a replay's game events).
    /// <para>
    /// The game gives an ability link its index the first time it sees it while loading the XML data, as the <c>id</c> of an ability element
    /// (e.g. <c>&lt;CAbilEffectTarget id="AlarakDiscordStrike"&gt;</c>) or as the value of an ability link of another element (e.g. <c>&lt;AbilArray Link="AlarakDiscordStrike" /&gt;</c>).
    /// So the indexes are from all the loaded <c>stormmods</c> (base, then map, then custom), and the indexes of a map's abilities come after the base ones.
    /// </para>
    /// <para>
    /// Not every <c>id</c> is an ability that is defined: an ability link can be to an ability that does not exist,
    /// and a few are an ability with its command (e.g. <c>SamuroMirrorImage,Execute</c>).
    /// </para>
    /// </summary>
    /// <returns>
    /// A new dictionary of the <c>id</c>s by their index, in index order. The first index is <c>1</c> and no index is skipped.
    /// An index that has no <c>id</c> is an empty string, which is the default element (the element with no <c>id</c>) of each ability class (e.g. <c>CAbilEffectTarget</c>).
    /// Loading or unloading afterwards does not change it.
    /// </returns>
    public IReadOnlyDictionary<int, string> GetAbilLinkIds() => _layeredDataStorage.AbilLinks.GetAbilLinkIds();

    /// <summary>
    /// Gets the <c>id</c> of an ability link from its index (e.g. the <c>m_abilLink</c> of a replay's game events), if the index has one.
    /// See <see cref="GetAbilLinkIds"/> for how the indexes are given.
    /// </summary>
    /// <param name="abilLink">The index of the ability link.</param>
    /// <param name="id">When this method returns, contains the <c>id</c> of the ability link, if found; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the index has an <c>id</c>; otherwise <see langword="false"/>, which is when the index is out of range (the first index is <c>1</c>)
    /// or has no <c>id</c> (the default element of each ability class).
    /// </returns>
    public bool TryGetAbilLinkId(int abilLink, [NotNullWhen(true)] out string? id) => _layeredDataStorage.AbilLinks.TryGetAbilLinkId(abilLink, out id);

    /// <summary>
    /// Gets the <c>id</c> of an ability link from its index (e.g. the <c>m_abilLink</c> of a replay's game events).
    /// See <see cref="GetAbilLinkIds"/> for how the indexes are given.
    /// </summary>
    /// <param name="abilLink">The index of the ability link.</param>
    /// <returns>The <c>id</c> of the ability link.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The index has no <c>id</c> (in the same cases that <see cref="TryGetAbilLinkId(int, out string?)"/> returns <see langword="false"/>).
    /// </exception>
    public string GetAbilLinkId(int abilLink)
    {
        if (TryGetAbilLinkId(abilLink, out string? id))
            return id;

        throw new KeyNotFoundException($"Could not find the ability link: {abilLink}");
    }

    /// <summary>
    /// Gets the index of an ability link from its <c>id</c>, if it has one.
    /// See <see cref="GetAbilLinkIds"/> for how the indexes are given.
    /// </summary>
    /// <param name="id">The <c>id</c> of the ability link (e.g. <c>AlarakDiscordStrike</c>). This is case-sensitive.</param>
    /// <param name="abilLink">When this method returns, contains the index of the ability link, if found; otherwise, <c>0</c>.</param>
    /// <returns><see langword="true"/> if the <c>id</c> has an index; otherwise <see langword="false"/>.</returns>
    public bool TryGetAbilLink(ReadOnlySpan<char> id, out int abilLink) => _layeredDataStorage.AbilLinks.TryGetAbilLink(id, out abilLink);

    /// <summary>
    /// Gets the index of an ability link from its <c>id</c>.
    /// See <see cref="GetAbilLinkIds"/> for how the indexes are given.
    /// </summary>
    /// <param name="id">The <c>id</c> of the ability link (e.g. <c>AlarakDiscordStrike</c>). This is case-sensitive.</param>
    /// <returns>The index of the ability link.</returns>
    /// <exception cref="KeyNotFoundException">The <c>id</c> has no index.</exception>
    public int GetAbilLink(ReadOnlySpan<char> id)
    {
        if (TryGetAbilLink(id, out int abilLink))
            return abilLink;

        throw new KeyNotFoundException($"Could not find the ability link: {id}");
    }

    // the element names are stored without their leading C, so it is removed before a lookup. A name without it is not found.
    private static bool TryRemoveClassElementDeclaration(ReadOnlySpan<char> elementName, out ReadOnlySpan<char> storedElementName)
    {
        if (elementName.Length < 2 || char.ToUpperInvariant(elementName[0]) != ClassElementDeclaration)
        {
            storedElementName = default;
            return false;
        }

        storedElementName = elementName[1..];
        return true;
    }

    // the ids of one layer, or of all the layers if it is null
    private List<string> GetElementIdsCore(ReadOnlySpan<char> catalogName, StormModType? stormModType)
    {
        List<string> ids = [.. _layeredDataStorage.Elements.GetStormXmlElementIdsByCatalogName(catalogName, stormModType)];
        ids.Sort(StringComparer.Ordinal);

        return ids;
    }

    // the locale of a gamestring when none is given: the loaded gamestrings' locale
    private StormLocale GetDefaultGameStringLocale() => GameStringsLocale ?? StormLocale.ENUS;

    private GameStringText ParseGameStringCore(ReadOnlySpan<char> gameString, StormLocale locale, bool extractFontValues)
    {
        if (!Enum.IsDefined(locale))
            throw new ArgumentOutOfRangeException(nameof(locale), locale, $"The value is not a defined {nameof(StormLocale)}.");

        return new GameStringText(_gameStringParser.ParseText(gameString, locale), locale, extractFontValues);
    }

    // whether the merged element still has the unit name, which a later definition can have changed
    private bool HasUnitName(ReadOnlySpan<char> catalogName, string id, ReadOnlySpan<char> unitName) =>
        TryGetElement(catalogName, id, out StormXmlElement? element) &&
        element.Members.TryGetMember(StormXmlConstants.UnitNameAttributeName, out StormXmlArray? elementUnitName) &&
        elementUnitName.Value.HasValue &&
        unitName.Equals(elementUnitName.Value.RawValue, StringComparison.Ordinal);
}
