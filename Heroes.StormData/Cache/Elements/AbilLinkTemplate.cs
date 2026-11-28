namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents what an element passes on to its children (the elements that have it as their <c>parent</c>) so their ability links can be found.
/// <para>
/// An ability link field of an element can have a token in its value instead of an ability (e.g. <c>&lt;Abil Link="##abil##" /&gt;</c>).
/// The element itself does not have an ability link there. Each child does, once the token is replaced with the child's value for it
/// (e.g. <c>&lt;CActorRange id="AlarakRange" parent="RangeAbil" abil="AlarakDiscordStrike" /&gt;</c> has the ability link <c>AlarakDiscordStrike</c>).
/// So the template keeps those fields, the element's tokens with their default values, and its own <c>parent</c>,
/// since a child also gets the fields and tokens of the parents above its parent.
/// </para>
/// <para>
/// A template can have no tokens and no fields. It is then kept only for its <see cref="Parent"/>, so a child still gets the fields and tokens of the parents above it.
/// </para>
/// </summary>
internal sealed class AbilLinkTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbilLinkTemplate"/> class.
    /// </summary>
    /// <param name="parent">The <c>parent</c> of the element, or <see langword="null"/> if it has none.</param>
    /// <param name="tokens">The tokens of the element. The list is kept, not copied, so it must not be changed afterwards.</param>
    /// <param name="tokenFields">The ability link fields of the element that have a token. The list is kept, not copied, so it must not be changed afterwards.</param>
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
    /// Gets the tokens of the element (e.g. <c>&lt;?token id="abil" type="CAbilLink"?&gt;</c>), each as its <c>id</c> and default value, in the order they were defined.
    /// <para>
    /// The same <c>id</c> can be in here more than once, from different definitions of the element. The later one is used.
    /// </para>
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> Tokens { get; }

    /// <summary>
    /// Gets the ability link fields of the element that have a token in their value (e.g. <c>&lt;Abil Link="##abil##" /&gt;</c>), in the order they were defined.
    /// </summary>
    public IReadOnlyList<AbilLinkField> TokenFields { get; }

    /// <summary>
    /// Creates a template that also has the tokens and fields of a later definition of the same element, the same way the definitions of an element are merged.
    /// </summary>
    /// <param name="parent">The <c>parent</c> of the later definition, which replaces <see cref="Parent"/>, or <see langword="null"/> if it does not set one, which keeps it.</param>
    /// <param name="tokens">The tokens of the later definition.</param>
    /// <param name="tokenFields">The fields of the later definition.</param>
    /// <returns>A new <see cref="AbilLinkTemplate"/>, or this instance if nothing changes.</returns>
    public AbilLinkTemplate With(string? parent, IReadOnlyList<KeyValuePair<string, string>> tokens, IReadOnlyList<AbilLinkField> tokenFields)
    {
        string? newParent = parent ?? Parent;

        if (tokens.Count == 0 && tokenFields.Count == 0 && string.Equals(newParent, Parent, StringComparison.Ordinal))
            return this;

        return new AbilLinkTemplate(newParent, [.. Tokens, .. tokens], [.. TokenFields, .. tokenFields]);
    }
}
