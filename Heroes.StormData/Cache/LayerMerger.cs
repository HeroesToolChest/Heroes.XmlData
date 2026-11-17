namespace Heroes.StormData.Cache;

/// <summary>
/// Provides helpers for merging elements found in the base, map, and custom data storage layers.
/// </summary>
internal static class LayerMerger
{
    /// <summary>
    /// Merges an element from a data storage layer into the result of the previous layers.
    /// <para>
    /// The first element found is kept as is. It is only cloned (and <paramref name="isCopy"/> set to <see langword="true"/>) when a later layer has to be merged into it,
    /// so the stored instance is never modified.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <param name="stormXmlElement">The result of the previous layers, or <see langword="null"/> if none of them had the element.</param>
    /// <param name="isCopy"><see langword="true"/> if <paramref name="stormXmlElement"/> is already a copy that can be modified.</param>
    /// <param name="layerElement">The element from the current layer, or <see langword="null"/> if the layer does not have it.</param>
    public static void MergeInto<T>(ref T? stormXmlElement, ref bool isCopy, T? layerElement)
        where T : StormXmlElement
    {
        if (layerElement is null)
            return;

        // first layer found: keep the cached instance for now, it is only copied if another layer has to be merged into it
        if (stormXmlElement is null)
        {
            stormXmlElement = layerElement;
            return;
        }

        if (!isCopy)
        {
            // need to merge, so clone the first layer to avoid modifying the cached instance
            stormXmlElement = (T)stormXmlElement.Clone();
            isCopy = true;
        }

        stormXmlElement.Merge(layerElement);
    }
}
