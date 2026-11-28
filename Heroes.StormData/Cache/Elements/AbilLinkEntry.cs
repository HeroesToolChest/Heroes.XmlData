namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents an entry that takes an ability link index.
/// </summary>
/// <param name="Value">
/// The <c>id</c> of the ability, or the element name (e.g. <c>CAbilEffectTarget</c>) if <paramref name="IsDefaultElement"/> is <see langword="true"/>.
/// </param>
/// <param name="IsDefaultElement">Whether the entry is the default element (the element with no <c>id</c>) of an ability class.</param>
internal readonly record struct AbilLinkEntry(string Value, bool IsDefaultElement);
