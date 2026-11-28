namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Gets the ability link indexes across the base, map, and custom data storages.
/// <para>
/// The index of an ability link is its position in the order the game first sees it while loading the XML data, which is the base data, then the map data, and then the custom data.
/// The indexes start at <c>1</c>. The default element (the element with no <c>id</c>) of each ability class also takes an index.
/// </para>
/// </summary>
internal sealed class LayeredAbilLinkCache
{
    // the index of the first ability link
    private const int FirstIndex = 1;

    private readonly AbilLinkCache[] _abilLinkCaches;

    // the ids in index order, the first one is index 1. An index that has no id is an empty string
    private string[] _ids = [];
    private Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> _indexById = new Dictionary<string, int>(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();
    private int _version = -1;

    public LayeredAbilLinkCache(AbilLinkCache baseAbilLinkCache, AbilLinkCache mapAbilLinkCache, AbilLinkCache customAbilLinkCache)
    {
        // in the order they are loaded
        _abilLinkCaches = [baseAbilLinkCache, mapAbilLinkCache, customAbilLinkCache];
    }

    /// <summary>
    /// Gets the <c>id</c>s of the ability links by their index.
    /// </summary>
    /// <returns>A new dictionary of the <c>id</c>s by their index, in index order. The first index is <c>1</c>. An index that has no <c>id</c> is an empty string.</returns>
    public Dictionary<int, string> GetAbilLinkIds()
    {
        Update();

        Dictionary<int, string> idsByIndex = new(_ids.Length);

        for (int i = 0; i < _ids.Length; i++)
            idsByIndex.Add(i + FirstIndex, _ids[i]);

        return idsByIndex;
    }

    /// <summary>
    /// Tries to get the <c>id</c> of an ability link from its index.
    /// </summary>
    /// <param name="abilLink">The index of the ability link.</param>
    /// <param name="id">When this method returns, contains the <c>id</c>, if the index has one; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the index has an <c>id</c>; otherwise, <see langword="false"/>.</returns>
    public bool TryGetAbilLinkId(int abilLink, [NotNullWhen(true)] out string? id)
    {
        Update();

        // int.MinValue - FirstIndex would overflow
        if (abilLink >= FirstIndex && abilLink - FirstIndex < _ids.Length && _ids[abilLink - FirstIndex].Length > 0)
        {
            id = _ids[abilLink - FirstIndex];
            return true;
        }

        id = null;
        return false;
    }

    /// <summary>
    /// Tries to get the index of an ability link from its <c>id</c>.
    /// </summary>
    /// <param name="id">The <c>id</c> of the ability link. This is case-sensitive.</param>
    /// <param name="abilLink">When this method returns, contains the index, if found; otherwise, <c>0</c>.</param>
    /// <returns><see langword="true"/> if the <c>id</c> has an index; otherwise, <see langword="false"/>.</returns>
    public bool TryGetAbilLink(ReadOnlySpan<char> id, out int abilLink)
    {
        Update();

        return _indexById.TryGetValue(id, out abilLink);
    }

    // each cache's version only goes up, so the sum changes whenever any of them does
    private int GetVersion()
    {
        int version = 0;

        foreach (AbilLinkCache abilLinkCache in _abilLinkCaches)
            version += abilLinkCache.Version;

        return version;
    }

    private void Update()
    {
        int version = GetVersion();

        if (version == _version)
            return;

        List<string> ids = [];
        Dictionary<string, int> indexById = new(StringComparer.Ordinal);
        HashSet<string> defaultElementNames = new(StringComparer.Ordinal);

        foreach (AbilLinkCache abilLinkCache in _abilLinkCaches)
        {
            foreach (AbilLinkEntry entry in abilLinkCache.Entries)
            {
                if (entry.IsDefaultElement)
                {
                    if (defaultElementNames.Add(entry.Value))
                        ids.Add(string.Empty);
                }
                else if (indexById.TryAdd(entry.Value, ids.Count + FirstIndex))
                {
                    ids.Add(entry.Value);
                }
            }
        }

        _ids = [.. ids];
        _indexById = indexById.GetAlternateLookup<ReadOnlySpan<char>>();
        _version = version;
    }
}
