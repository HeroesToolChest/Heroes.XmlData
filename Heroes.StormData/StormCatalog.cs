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
    /// Determines if a default element exists. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// </summary>
    /// <param name="elementName">
    /// The element name as written in the XML, with its leading <c>C</c> (e.g. <c>CEffectDamage</c>), the same as <see cref="StormXmlElement.ElementNameWithDeclaration"/>.
    /// This is case-insensitive.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if any of the loaded <c>stormmods</c> has the default element; otherwise <see langword="false"/>, which includes <paramref name="elementName"/>
    /// not starting with <c>C</c> (such as <c>EffectDamage</c>) or being empty.
    /// </returns>
    public bool DefaultElementExists(ReadOnlySpan<char> elementName) => TryGetDefaultElement(elementName, out _);

    /// <summary>
    /// Gets a default element, if it exists. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom).
    /// The same instance can be returned on later calls until the loaded data changes. Its public members are read-only.
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

        element = _layeredDataStorage.Elements.GetStormXmlElementByClassElement(storedElementName);
        return element is not null;
    }

    /// <summary>
    /// Gets a default element. A default element is the element with no <c>id</c> (e.g. <c>&lt;CEffectDamage default="1"&gt;</c>).
    /// <para>
    /// It is merged from all the loaded <c>stormmods</c> (base, then map, then custom).
    /// The same instance can be returned on later calls until the loaded data changes. Its public members are read-only.
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
}
