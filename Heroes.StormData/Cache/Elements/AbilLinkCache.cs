namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents a cache of the ability links of a data storage, in the order they were first seen while its XML files were loaded.
/// </summary>
internal sealed class AbilLinkCache
{
    private readonly List<AbilLinkEntry> _entries = [];

    // case-sensitive for ids
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _ids = new HashSet<string>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
    private readonly HashSet<string> _defaultElementNames = new(StringComparer.Ordinal);

    private readonly Dictionary<(string CatalogName, string Id), AbilLinkTemplate> _templatesById = [];

    /// <summary>
    /// Gets the entries in the order they were first seen. An entry is only in here once.
    /// </summary>
    public IReadOnlyList<AbilLinkEntry> Entries => _entries;

    /// <summary>
    /// Gets a number that changes whenever the <see cref="Entries"/> change.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Adds the <c>id</c> of an ability, if it was not already added.
    /// </summary>
    /// <param name="id">The <c>id</c> of the ability. This is case-sensitive.</param>
    public void AddId(ReadOnlySpan<char> id)
    {
        if (!_ids.Add(id))
            return;

        _ids.TryGetValue(id, out string? value);

        _entries.Add(new AbilLinkEntry(value!, false));
        Version++;
    }

    /// <summary>
    /// Adds the default element (the element with no <c>id</c>) of an ability class, if it was not already added.
    /// </summary>
    /// <param name="elementName">The name of the element (e.g. <c>CAbilEffectTarget</c>). This is case-sensitive.</param>
    public void AddDefaultElement(string elementName)
    {
        if (!_defaultElementNames.Add(elementName))
            return;

        _entries.Add(new AbilLinkEntry(elementName, true));
        Version++;
    }

    public bool TryGetTemplate(string catalogName, string id, [NotNullWhen(true)] out AbilLinkTemplate? template) => _templatesById.TryGetValue((catalogName, id), out template);

    public void SetTemplate(string catalogName, string id, AbilLinkTemplate template) => _templatesById[(catalogName, id)] = template;

    /// <summary>
    /// Removes all the entries and templates.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        _ids.Set.Clear();
        _defaultElementNames.Clear();
        _templatesById.Clear();

        // the version only goes up (never reset), so the sum across the data storages can't land on an earlier value.
        Version++;
    }
}
