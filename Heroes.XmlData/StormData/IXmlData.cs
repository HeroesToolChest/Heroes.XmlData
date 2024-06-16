namespace Heroes.XmlData.StormData;

internal interface IXmlData
{
    /// <summary>
    /// Gets the amount of inner elements and attributes.
    /// </summary>
    int XmlDataCount { get; }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    StormElementData GetXmlData(ReadOnlySpan<char> index);

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    StormElementData GetXmlData(string index);

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    bool TryGetXmlData(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData);

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    bool TryGetXmlData(string index, [NotNullWhen(true)] out StormElementData? stormElementData);

    /// <summary>
    /// Gets a collection of the inner elements and attributes.
    /// </summary>
    /// <returns>A collection of the inner xml data as <see cref="StormElementData"/>.</returns>
    IEnumerable<StormElementData> GetXmlData();
}
