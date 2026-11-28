namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents the parts of an element that the elements that have it as their <c>parent</c> need to get their ability links,
/// which are its tokens and its ability link fields that have a token (e.g. <c>&lt;Abil Link="##abil##" /&gt;</c>).
/// </summary>
internal sealed class AbilLinkTemplate
{
    public AbilLinkTemplate(string? parent, IReadOnlyList<KeyValuePair<string, string>> tokens, IReadOnlyList<AbilLinkField> tokenFields)
    {
        Parent = parent;
        Tokens = tokens;
        TokenFields = tokenFields;
    }

    /// <summary>
    /// Gets the <c>parent</c> of the element, or <see langword="null"/> if it has none.
    /// </summary>
    public string? Parent { get; }

    /// <summary>
    /// Gets the tokens of the element, the token <c>id</c> with its default value.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> Tokens { get; }

    /// <summary>
    /// Gets the fields of the element that can be an ability link and have a token in their value.
    /// </summary>
    public IReadOnlyList<AbilLinkField> TokenFields { get; }

    /// <summary>
    /// Creates a template that also has the tokens and fields of a later definition of the same element.
    /// </summary>
    /// <param name="tokens">The tokens of the later definition.</param>
    /// <param name="tokenFields">The fields of the later definition.</param>
    /// <returns>A new <see cref="AbilLinkTemplate"/>, or this instance if there is nothing to add.</returns>
    public AbilLinkTemplate With(IReadOnlyList<KeyValuePair<string, string>> tokens, IReadOnlyList<AbilLinkField> tokenFields)
    {
        if (tokens.Count == 0 && tokenFields.Count == 0)
            return this;

        return new AbilLinkTemplate(Parent, [.. Tokens, .. tokens], [.. TokenFields, .. tokenFields]);
    }
}
