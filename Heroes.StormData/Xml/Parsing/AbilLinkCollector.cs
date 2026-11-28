namespace Heroes.StormData.Xml.Parsing;

/// <summary>
/// Collects the ability links of the XML elements as they are parsed, in the order the game first sees them, and adds them to an <see cref="AbilLinkCache"/>.
/// </summary>
/// <remarks>
/// <para>
/// The game gives an ability link its index the first time it sees it while loading the XML data, in the order the files are loaded and the order within a file.
/// It is seen as the <c>id</c> of an ability element (e.g. <c>&lt;CAbilEffectTarget id="AlarakDiscordStrike"&gt;</c>), or as the value of a field
/// that is an ability link (e.g. <c>&lt;AbilArray Link="AlarakDiscordStrike" /&gt;</c>), whether or not the ability is defined.
/// The default element (the element with no <c>id</c>) of an ability class also takes an index.
/// </para>
/// <para>
/// A field of a <c>parent</c> that has a token (e.g. <c>&lt;Abil Link="##abil##" /&gt;</c>) is seen at each element that has that parent, with the element's token value.
/// </para>
/// </remarks>
internal sealed class AbilLinkCollector
{
    // the number of times the tokens in the token values are replaced, for a token value that has another token
    private const int TokenValueDepth = 3;

    private readonly AbilLinkCache _abilLinkCache;

    // the caches of the data storages that are loaded before this one, the last one loaded first
    private readonly AbilLinkCache[] _earlierAbilLinkCaches;

    // of the current root element
    private readonly Dictionary<string, string> _attributes = new(StringComparer.Ordinal);
    private readonly List<AbilLinkField> _fields = [];
    private readonly List<KeyValuePair<string, string>> _tokens = [];

    private readonly List<AbilLinkTemplate> _templates = [];
    private readonly HashSet<string> _parentIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _tokenIds = new(StringComparer.Ordinal);
    private readonly List<AbilLinkField> _tokenFields = [];
    private readonly Dictionary<string, string> _catalogNameByElementName = new(StringComparer.Ordinal);

    private Dictionary<string, string> _tokenValues = new(StringComparer.Ordinal);
    private Dictionary<string, string> _replacedTokenValues = new(StringComparer.Ordinal);
    private bool _hasTokenValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilLinkCollector"/> class.
    /// </summary>
    /// <param name="abilLinkCache">The cache to add the ability links to.</param>
    /// <param name="earlierAbilLinkCaches">The caches of the data storages that are loaded before this one, the last one loaded first. Their elements can be a <c>parent</c>.</param>
    public AbilLinkCollector(AbilLinkCache abilLinkCache, params AbilLinkCache[] earlierAbilLinkCaches)
    {
        _abilLinkCache = abilLinkCache;
        _earlierAbilLinkCaches = earlierAbilLinkCaches;
    }

    /// <summary>
    /// Starts a root element (e.g. <c>&lt;CUnit id="HeroAlarak"&gt;</c>).
    /// </summary>
    public void BeginElement()
    {
        _attributes.Clear();
        _fields.Clear();
        _tokens.Clear();
    }

    /// <summary>
    /// Adds an attribute of the root element.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute, as it is in the XML.</param>
    public void AddRootAttribute(string name, string value)
    {
        _attributes[name] = value;

        if (IsAbilLinkFieldName(name))
            _fields.Add(new AbilLinkField(null, name, value));
    }

    /// <summary>
    /// Adds an attribute of an element inside the root element.
    /// </summary>
    /// <param name="parentElementName">The name of the element that <paramref name="elementName"/> is in.</param>
    /// <param name="elementName">The name of the element the attribute is on.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute, as it is in the XML.</param>
    public void AddAttribute(string parentElementName, string elementName, string name, string value)
    {
        // <AbilLinkEnableArray value="X" /> is the field AbilLinkEnableArray, <AbilArray Link="X" /> is the field Link in AbilArray
        if (name.Equals(StormXmlConstants.ValueAttributeName, StringComparison.Ordinal))
        {
            if (IsAbilLinkFieldName(elementName))
                _fields.Add(new AbilLinkField(parentElementName, elementName, value));
        }
        else if (IsAbilLinkFieldName(name))
        {
            _fields.Add(new AbilLinkField(elementName, name, value));
        }
    }

    /// <summary>
    /// Adds a token of the root element (e.g. <c>&lt;?token id="abil" type="CAbilLink"?&gt;</c>).
    /// </summary>
    /// <param name="id">The <c>id</c> of the token.</param>
    /// <param name="value">The default value of the token.</param>
    public void AddToken(string id, string value)
    {
        _tokens.Add(new KeyValuePair<string, string>(id, value));
    }

    /// <summary>
    /// Ends the root element, which adds its ability links to the <see cref="AbilLinkCache"/>.
    /// </summary>
    /// <param name="elementName">The name of the root element (e.g. <c>CUnit</c>).</param>
    public void EndElement(string elementName)
    {
        if (elementName.Equals(StormXmlConstants.ConstElementName, StringComparison.Ordinal))
            return;

        _attributes.TryGetValue(StormXmlConstants.IdAttributeName, out string? id);
        _attributes.TryGetValue(StormXmlConstants.ParentAttributeName, out string? parent);

        bool hasId = !string.IsNullOrEmpty(id);
        bool hasParent = !string.IsNullOrEmpty(parent);

        if (elementName.StartsWith(StormXmlConstants.AbilElementNamePrefix, StringComparison.Ordinal))
        {
            if (hasId)
                AddAbilLink(id!, StormXmlConstants.IdAttributeName);
            else
                _abilLinkCache.AddDefaultElement(elementName);
        }

        // most elements, nothing in it can be an ability link
        if (_fields.Count == 0 && _tokens.Count == 0 && !hasParent)
            return;

        string catalogName = GetCatalogName(elementName);

        _templates.Clear();

        if (hasParent)
            AddParentTemplates(catalogName, parent!);

        _tokenIds.Clear();
        _hasTokenValues = false;

        if (_templates.Count > 0 || _tokens.Count > 0)
            SetTokenValues(id);

        // the fields of the parents first, then the fields of this element
        foreach (AbilLinkTemplate template in _templates)
        {
            foreach (AbilLinkField tokenField in template.TokenFields)
            {
                AddAbilLinkOfField(elementName, tokenField, ReplaceTokens(tokenField.Value, _tokenValues));
            }
        }

        _tokenFields.Clear();

        foreach (AbilLinkField field in _fields)
        {
            // an attribute of the root element that is the value of a token, it is not a field
            if (field.ParentName is null && _tokenIds.Contains(field.Name))
                continue;

            if (field.Value.Contains(StormXmlConstants.TokenDelimiter, StringComparison.Ordinal))
            {
                // ##id## is always a token
                if (!_hasTokenValues)
                    SetTokenValues(id);

                _tokenFields.Add(field);

                AddAbilLinkOfField(elementName, field, ReplaceTokens(field.Value, _tokenValues));
            }
            else
            {
                AddAbilLinkOfField(elementName, field, field.Value);
            }
        }

        if (hasId)
            SetTemplate(catalogName, id!, hasParent ? parent : null);
    }

    private static bool IsAbilLinkFieldName(string name) =>
        StormXmlConstants.AbilLinkFieldNames.Contains(name) || name.Equals(StormXmlConstants.LinkFieldName, StringComparison.Ordinal) || name.Equals(StormXmlConstants.ReferenceFieldName, StringComparison.Ordinal);

    // whether a field is an ability link, Link is one of many kinds of links so it depends on what it is in
    private static bool IsAbilLinkField(string elementName, AbilLinkField field)
    {
        if (!field.Name.Equals(StormXmlConstants.LinkFieldName, StringComparison.Ordinal))
            return StormXmlConstants.AbilLinkFieldNames.Contains(field.Name);

        return field.ParentName switch
        {
            StormXmlConstants.AbilArrayFieldName or StormXmlConstants.AbilFieldName => true,
            StormXmlConstants.CountFieldName => elementName.Equals(StormXmlConstants.RequirementCountAbilElementName, StringComparison.Ordinal),
            _ => elementName.Equals(StormXmlConstants.RequirementAllowAbilElementName, StringComparison.Ordinal),
        };
    }

    /// <summary>
    /// Replaces the tokens in a value (e.g. <c>##abil##</c>) with their values. A token that has no value is kept as it is.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="tokenValues">The values of the tokens by their <c>id</c>.</param>
    /// <returns>The value with its tokens replaced.</returns>
    private static string ReplaceTokens(string value, Dictionary<string, string> tokenValues)
    {
        int start = value.IndexOf(StormXmlConstants.TokenDelimiter, StringComparison.Ordinal);

        if (start < 0)
            return value;

        Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> tokenValuesLookup = tokenValues.GetAlternateLookup<ReadOnlySpan<char>>();
        StringBuilder? stringBuilder = null;
        int position = 0;

        while (start >= 0)
        {
            int idStart = start + StormXmlConstants.TokenDelimiter.Length;
            int idEnd = value.IndexOf('#', idStart);

            // the id is at least one character and ends with the delimiter
            if (idEnd > idStart && idEnd + 1 < value.Length && value[idEnd + 1] == '#')
            {
                int end = idEnd + StormXmlConstants.TokenDelimiter.Length;

                if (tokenValuesLookup.TryGetValue(value.AsSpan(idStart, idEnd - idStart), out string? tokenValue))
                {
                    stringBuilder ??= new StringBuilder(value.Length);
                    stringBuilder.Append(value, position, start - position).Append(tokenValue);

                    position = end;
                }

                start = value.IndexOf(StormXmlConstants.TokenDelimiter, end, StringComparison.Ordinal);
            }
            else
            {
                start = value.IndexOf(StormXmlConstants.TokenDelimiter, start + 1, StringComparison.Ordinal);
            }
        }

        if (stringBuilder is null)
            return value;

        return stringBuilder.Append(value, position, value.Length - position).ToString();
    }

    private void AddAbilLinkOfField(string elementName, AbilLinkField field, string value)
    {
        if (field.Name.Equals(StormXmlConstants.ReferenceFieldName, StringComparison.Ordinal))
        {
            // a catalog reference, Abil,<id>,<field> (e.g. Abil,AlarakDiscordStrike,Cost[0].Cooldown.TimeUse)
            if (value.StartsWith(StormXmlConstants.AbilReferencePrefix, StringComparison.Ordinal))
                AddAbilLink(value.AsSpan(StormXmlConstants.AbilReferencePrefix.Length), field.Name);
        }
        else if (IsAbilLinkField(elementName, field))
        {
            AddAbilLink(value, field.Name);
        }
    }

    private void AddAbilLink(ReadOnlySpan<char> value, string fieldName)
    {
        // an ability command is the ability link and its command (e.g. AlarakDiscordStrike,Execute)
        if (!fieldName.Equals(StormXmlConstants.SubmenuAbilStateFieldName, StringComparison.Ordinal))
        {
            int commandIndex = value.IndexOf(',');

            if (commandIndex >= 0)
                value = value[..commandIndex];
        }

        value = value.Trim();

        // no value, or it has a token that has no value
        if (value.IsEmpty || value.Contains(StormXmlConstants.TokenDelimiter, StringComparison.Ordinal))
            return;

        _abilLinkCache.AddId(value);
    }

    // the catalog of the element (e.g. Actor for CActorRange), the parent of an element is in the same catalog
    private string GetCatalogName(string elementName)
    {
        if (_catalogNameByElementName.TryGetValue(elementName, out string? catalogName))
            return catalogName;

        // without the type letter declaration
        if (elementName.Length < 2 || !ElementCatalogNames.TryGetCatalogName(elementName.AsSpan(1), out catalogName))
            catalogName = elementName;

        _catalogNameByElementName[elementName] = catalogName;

        return catalogName;
    }

    private bool TryGetTemplate(string catalogName, string id, [NotNullWhen(true)] out AbilLinkTemplate? template)
    {
        if (_abilLinkCache.TryGetTemplate(catalogName, id, out template))
            return true;

        return TryGetEarlierTemplate(catalogName, id, out template);
    }

    private bool TryGetEarlierTemplate(string catalogName, string id, [NotNullWhen(true)] out AbilLinkTemplate? template)
    {
        foreach (AbilLinkCache abilLinkCache in _earlierAbilLinkCaches)
        {
            if (abilLinkCache.TryGetTemplate(catalogName, id, out template))
                return true;
        }

        template = null;
        return false;
    }

    // sets the templates of the parent, and its parents, the first parent last
    private void AddParentTemplates(string catalogName, string parent)
    {
        _parentIds.Clear();

        string? parentId = parent;

        while (!string.IsNullOrEmpty(parentId) && _parentIds.Add(parentId) && TryGetTemplate(catalogName, parentId, out AbilLinkTemplate? template))
        {
            _templates.Add(template);
            parentId = template.Parent;
        }

        _templates.Reverse();
    }

    // sets the values of the tokens of this element, which are the default values of the tokens of its parents and itself, and its attributes that are a token
    private void SetTokenValues(string? id)
    {
        _hasTokenValues = true;

        _tokenIds.Clear();
        _tokenValues.Clear();

        foreach (AbilLinkTemplate template in _templates)
        {
            foreach (KeyValuePair<string, string> token in template.Tokens)
            {
                _tokenIds.Add(token.Key);
                _tokenValues[token.Key] = token.Value;
            }
        }

        foreach (KeyValuePair<string, string> token in _tokens)
        {
            _tokenIds.Add(token.Key);
            _tokenValues[token.Key] = token.Value;
        }

        foreach (string tokenId in _tokenIds)
        {
            if (_attributes.TryGetValue(tokenId, out string? value))
                _tokenValues[tokenId] = value;
        }

        _tokenValues[StormXmlConstants.IdAttributeName] = id ?? string.Empty;

        // a token value can have another token (e.g. abil="##id##Cast")
        for (int i = 0; i < TokenValueDepth; i++)
        {
            bool hasToken = false;

            _replacedTokenValues.Clear();

            foreach (KeyValuePair<string, string> tokenValue in _tokenValues)
            {
                string replacedValue = ReplaceTokens(tokenValue.Value, _tokenValues);

                hasToken |= !ReferenceEquals(replacedValue, tokenValue.Value);

                _replacedTokenValues[tokenValue.Key] = replacedValue;
            }

            if (!hasToken)
                break;

            (_tokenValues, _replacedTokenValues) = (_replacedTokenValues, _tokenValues);
        }
    }

    // sets the template of this element for the elements that have it as their parent,
    // it is only kept if it, an earlier definition of it, or one of its parents, has something for them
    private void SetTemplate(string catalogName, string id, string? parent)
    {
        // a later definition of an element adds to the element (in this layer or an earlier one), the same way the element's definitions are merged,
        // and a parent it sets replaces the earlier one
        if (TryGetTemplate(catalogName, id, out AbilLinkTemplate? template))
        {
            AbilLinkTemplate mergedTemplate = template.With(parent, [.. _tokens], [.. _tokenFields]);

            // unchanged, so the lookup already finds it
            if (!ReferenceEquals(mergedTemplate, template))
                _abilLinkCache.SetTemplate(catalogName, id, mergedTemplate);

            return;
        }

        if (_tokens.Count > 0 || _tokenFields.Count > 0 || _templates.Count > 0)
            _abilLinkCache.SetTemplate(catalogName, id, new AbilLinkTemplate(parent, [.. _tokens], [.. _tokenFields]));
    }
}
