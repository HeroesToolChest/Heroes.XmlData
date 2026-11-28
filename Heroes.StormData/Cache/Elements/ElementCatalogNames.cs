namespace Heroes.StormData.Cache.Elements;

/// <summary>
/// Gets the catalog name of a class element from its name (e.g. <c>Effect</c> for <c>EffectDamage</c>), with the casing the game data uses.
/// </summary>
/// <remarks>
/// A class element's name starts with the name of its catalog (e.g. <c>CEffectDamage</c> is in the <c>Effect</c> catalog), so the catalog is the longest
/// catalog name that the element name starts with (e.g. <c>CSoundtrack</c> is <c>Soundtrack</c>, not <c>Sound</c>).
/// The one exception is the requirement nodes (e.g. <c>CRequirementAnd</c>), which are in the <c>RequirementNode</c> catalog, while <c>CRequirement</c> itself is in the <c>Requirement</c> catalog.
/// </remarks>
internal static class ElementCatalogNames
{
    // the catalog names (StormXmlConstants.CatalogNames) longest first, so the first one that matches is the longest
    private static readonly string[] _catalogNamesByLength = [.. StormXmlConstants.CatalogNames.OrderByDescending(x => x.Length).ThenBy(x => x, StringComparer.Ordinal)];

    /// <summary>
    /// Tries to get the catalog name of a class element.
    /// </summary>
    /// <param name="elementName">The name of the class element, without its leading <c>C</c> (e.g. <c>EffectDamage</c>). This is case-insensitive.</param>
    /// <param name="catalogName">When this method returns, contains the catalog name (e.g. <c>Effect</c>), if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="elementName"/> starts with a catalog name; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetCatalogName(ReadOnlySpan<char> elementName, [NotNullWhen(true)] out string? catalogName)
    {
        // every requirement class other than CRequirement itself is a requirement node (CRequirementAnd, CRequirementEq, and CRequirementNode)
        if (elementName.Length > StormXmlConstants.RequirementCatalogName.Length && elementName.StartsWith(StormXmlConstants.RequirementCatalogName, StringComparison.OrdinalIgnoreCase))
        {
            catalogName = StormXmlConstants.RequirementNodeCatalogName;
            return true;
        }

        foreach (string name in _catalogNamesByLength)
        {
            if (elementName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                catalogName = name;
                return true;
            }
        }

        catalogName = null;
        return false;
    }
}
