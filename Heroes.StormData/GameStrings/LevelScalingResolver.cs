namespace Heroes.StormData.GameStrings;

/// <summary>
/// Finds the per-level scaling of an element's field from the <c>LevelScalingArray</c> modifications, merged across the data storages.
/// Used by both the gamestring data references and <see cref="StormCatalog"/>, so they always agree.
/// </summary>
internal static class LevelScalingResolver
{
    /// <summary>
    /// Tries to get the scaling of a field, from the <c>LevelScalingArray</c> modification whose <c>Field</c> resolves to the same node as <paramref name="field"/>.
    /// <para>
    /// A modification matches if its <c>Field</c> is the same text as <paramref name="field"/>, or if both resolve to the same node of <paramref name="mergedStormXmlElement"/>,
    /// so different spellings of the same field (such as with and without an index) match. If more than one matches, the later one (higher data storage layer) is used.
    /// </para>
    /// </summary>
    /// <param name="layeredElementCache">The element data of the data storages.</param>
    /// <param name="mergedStormXmlElement">The merged element of <paramref name="catalog"/> and <paramref name="entry"/>, which the fields are resolved on.</param>
    /// <param name="catalog">The catalog name of the element (the modification's <c>Catalog</c>). This is case-insensitive.</param>
    /// <param name="entry">The <c>id</c> of the element (the modification's <c>Entry</c>). This is case-sensitive.</param>
    /// <param name="field">The field (e.g. <c>LifeMax</c> or <c>Modification.DamageDealtScaled[Spell]</c>).</param>
    /// <param name="fieldNode">The node <paramref name="field"/> resolves to on <paramref name="mergedStormXmlElement"/>, which can be empty if it doesn't resolve.</param>
    /// <param name="scaling">When this method returns, contains the scaling as written in the data (e.g. 0.04), if found; otherwise, 0.</param>
    /// <returns><see langword="true"/> if a matching modification was found and its <c>Value</c> is a number; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetScaling(
        LayeredElementCache layeredElementCache,
        StormXmlElement mergedStormXmlElement,
        ReadOnlySpan<char> catalog,
        ReadOnlySpan<char> entry,
        ReadOnlySpan<char> field,
        FieldNode fieldNode,
        out double scaling)
    {
        scaling = 0;

        IReadOnlyList<LevelScalingFieldValue>? scalingFieldValues = layeredElementCache.GetScalingFieldValuesById(entry, catalog);
        if (scalingFieldValues is null)
            return false;

        // from the end, so if two spellings resolve to the same node, the later (higher data storage layer) one wins
        for (int i = scalingFieldValues.Count - 1; i >= 0; i--)
        {
            LevelScalingFieldValue scalingFieldValue = scalingFieldValues[i];

            // fast path: same text, so no need to parse it.
            // both are parsed on the same merged element, so the node instances can be compared (two empty nodes are not the same node)
            bool isMatch = field.Equals(scalingFieldValue.Field, StringComparison.Ordinal) || DataRefFieldParser.ParseField(mergedStormXmlElement, scalingFieldValue.Field).IsSameNode(fieldNode);

            if (isMatch)
                return double.TryParse(scalingFieldValue.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out scaling);
        }

        return false;
    }
}
