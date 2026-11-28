namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Represents a field of an element that can be an ability link, such as the <c>Link</c> of <c>&lt;AbilArray Link="HeroAttack" /&gt;</c>.
/// </summary>
/// <param name="ParentName">
/// The name of the element the field is in (e.g. <c>AbilArray</c>), or <see langword="null"/> if the field is an attribute of the root element.
/// </param>
/// <param name="Name">The name of the field (e.g. <c>Link</c>). For a <c>value</c> attribute it is the name of its element.</param>
/// <param name="Value">The value of the field, as it is in the XML.</param>
internal readonly record struct AbilLinkField(string? ParentName, string Name, string Value);
