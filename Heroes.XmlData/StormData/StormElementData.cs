namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data that represents an <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData
{
    private static readonly HashSet<string> _otherElementArrays = new(StringComparer.OrdinalIgnoreCase)
    {
        "CatalogModifications",
        "CardLayouts",
        "ConditionalEvents",
        "Cost",
        "LayoutButtons",
        "On",
        "TooltipAppender",
    };

    private static readonly HashSet<string> _singleMergeArrays = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cost",
    };

    private string? _value;

    internal StormElementData(XElement rootElement)
    {
#if NET9_0_OR_GREATER
        ElementDataPairsAltLookup = ElementDataPairs.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, bool isArray = false)
    {
#if NET9_0_OR_GREATER
        ElementDataPairsAltLookup = ElementDataPairs.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

        Parent = parent;

        if (!string.IsNullOrWhiteSpace(parent.Field))
        {
            if (isArray)
                Field = $"{parent.Field}[{field}]";
            else
                Field = $"{parent.Field}.{field}";
        }
        else
        {
            Field = field;
        }
    }

    internal StormElementData(StormElementData parent, string field, XElement rootElement)
        : this(parent, field)
    {
        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, string value, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        _value = value;
    }

    internal StormElementData(StormElementData parent, string field, XElement rootElement, bool isInnerArray = false, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        Parse(rootElement, isInnerArray);
    }

    internal StormElementData(StormElementData parent, string field, XElement element, string index, bool isInnerArray = false)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, element, isInnerArray, true);
    }

    internal StormElementData(StormElementData parent, string field, XAttribute attribute, string index)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, attribute.Value, true);
    }

    /// <summary>
    /// Gets the parent data.
    /// </summary>
    public StormElementData? Parent { get; }

    /// <summary>
    /// Gets the representation of the current data reference field.
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    public bool HasValue => !Value.IsNull;

    /// <summary>
    /// Gets the original value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? RawValue
    {
        get
        {
            if (_value is not null)
            {
                return _value;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    return data.RawValue;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    if (firstElementData.HasValue)
                        return firstElementData.RawValue;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the evaluated value of <see cref="RawValue"/>. For example, if it contains an element replacement (##name##), it will be updated with the new value.
    /// To get the original value, use <see cref="RawValue"/>.
    /// </summary>
    public StormElementValue Value
    {
        get
        {
            ReadOnlySpan<char> returnValue = null;
            bool isNull = false;

            if (_value is not null)
            {
                returnValue = _value;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    isNull = data.Value.IsNull;
                    returnValue = data.Value.Value;
                }
                else if (HasTextIndex)
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    isNull = firstElementData.Value.IsNull;
                    returnValue = firstElementData.Value.Value;
                }
            }

            return new StormElementValue(this)
            {
                Value = returnValue,
                IsNull = isNull,
            };
        }
    }

    /// <summary>
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of numerical keys.
    /// </summary>
    public bool HasNumericalIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of text keys.
    /// </summary>
    public bool HasTextIndex { get; init; }

    /// <summary>
    /// Gets the amount of elements.
    /// </summary>
    public int ElementDataCount => ElementDataPairs.Count;

    /// <summary>
    /// Gets a value indicating whether <see cref="HxdScaleValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(HxdScaleValue))]
    internal bool HasHxdScale => ElementDataPairs.Count == 1 && ElementDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    /// <summary>
    /// Gets the scaling value.
    /// </summary>
    internal StormElementValue HxdScaleValue
    {
        get
        {
            if (HasHxdScale)
            {
                return new StormElementValue(this)
                {
                    Value = ElementDataPairs[ScaleValueParser.ScaleAttributeName].RawValue,
                    IsNull = false,
                };
            }

            return new StormElementValue(this)
            {
                IsNull = true,
            };
        }
    }

    /// <summary>
    /// Gets the inner data.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal Dictionary<string, StormElementData> ElementDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

#if NET9_0_OR_GREATER
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal Dictionary<string, StormElementData>.AlternateLookup<ReadOnlySpan<char>> ElementDataPairsAltLookup { get; }
#endif

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Field))
                return $"{{ROOT, Elements = {ElementDataCount}}}";
            else if (!string.IsNullOrWhiteSpace(Value.GetString()))
                return $"{{\"{Field}\", Value = \"{Value.GetString()}\", Elements = {ElementDataCount}}}";
            else
                return $"{{\"{Field}\", Elements = {ElementDataCount}}}";
        }
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData this[string index]
    {
        get
        {
            return GetElementDataAt(index);
        }
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData this[ReadOnlySpan<char> index]
    {
        get
        {
            return GetElementDataAt(index);
        }
    }

    /// <summary>
    /// Gets a collection of the inner data indexes.
    /// </summary>
    /// <returns>A collection of the inner data indexes.</returns>
    public IEnumerable<string> GetElementDataIndexes()
    {
        return ElementDataPairs.Keys;
    }

    /// <summary>
    /// Gets a collection of the inner data elements.
    /// </summary>
    /// <returns>A collection of <see cref="KeyValuePair"/>s representing the inner data elements.</returns>
    public IEnumerable<KeyValuePair<string, StormElementData>> GetElementData()
    {
        return ElementDataPairs.AsReadOnly();
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(ReadOnlySpan<char> index)
    {
#if NET9_0_OR_GREATER
        if (ElementDataPairsAltLookup.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException($"Value '{index}' was not found.");
#else
        return GetElementDataAt(index.ToString());
#endif
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        if (ElementDataPairs.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException($"Value '{index}' was not found.");
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetElementDataAt(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
#if NET9_0_OR_GREATER
        return ElementDataPairsAltLookup.TryGetValue(index, out stormElementData);
#else
        return TryGetElementDataAt(index.ToString(), out stormElementData);
#endif
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    public bool TryGetElementDataAt(string index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.TryGetValue(index, out stormElementData);
    }

    /// <summary>
    /// Determines whether the inner data contains the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    public bool ContainsIndex(ReadOnlySpan<char> index)
    {
#if NET9_0_OR_GREATER
        return ElementDataPairsAltLookup.ContainsKey(index);
#else
        return ContainsIndex(index.ToString());
#endif
    }

    /// <summary>
    /// Determines whether the inner data contains the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    public bool ContainsIndex(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.ContainsKey(index);
    }

    internal IEnumerable<StormElementData> GetElements()
    {
        foreach (StormElementData data in ElementDataPairs.Values)
        {
            if (data.ElementDataPairs.Count == 0)
            {
                yield return data;
            }
            else
            {
                foreach (StormElementData innerData in data.GetElements())
                {
                    yield return innerData;
                }
            }
        }
    }

    internal void AddXElement(XElement element, bool isInnerArray = false)
    {
        Parse(element, isInnerArray);
    }

    private void Parse(XElement rootElement, bool isInnerArray = false)
    {
        IEnumerable<XAttribute> attributes = rootElement.Attributes();
        IEnumerable<XElement> elements = rootElement.Elements();

        foreach (XAttribute attribute in attributes)
        {
            if (attribute.Name.LocalName.Equals("index", StringComparison.OrdinalIgnoreCase))
                continue;

            if (attribute.Name.LocalName.Equals("value", StringComparison.OrdinalIgnoreCase))
            {
                _value = attribute.Value;
                continue;
            }

            if (isInnerArray)
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute, "0")
                {
                    HasNumericalIndex = true,
                };
            }
            else if (attribute.Name.LocalName.Equals("parent", StringComparison.OrdinalIgnoreCase) &&
                ElementDataPairs.TryGetValue("id", out StormElementData? existingIdData) && !string.IsNullOrEmpty(existingIdData.RawValue) &&
                existingIdData.RawValue.Equals(attribute.Value))
            {
                // skip the parent attribute if it's the same as the existing id attribute.
                continue;
            }
            else
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute.Value);
            }
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;
            string? valueAtt = element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value;

            if (!string.IsNullOrEmpty(indexAtt))
            {
                if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingElementData))
                {
                    if (existingElementData.ElementDataPairs.TryGetValue(indexAtt, out StormElementData? existingIndexedData))
                    {
                        existingIndexedData.AddXElement(element, true);
                    }
                    else
                    {
                        existingElementData.ElementDataPairs[indexAtt] = new StormElementData(existingElementData, indexAtt, element, true, true);
                    }
                }
                else
                {
                    bool numericalIndex = int.TryParse(indexAtt, out _);

                    ElementDataPairs[elementName] = new StormElementData(this, elementName, element, indexAtt, true)
                    {
                        HasNumericalIndex = numericalIndex,
                        HasTextIndex = !numericalIndex,
                    };
                }
            }
            else if (isInnerArray || elementName.AsSpan().EndsWith("array", StringComparison.OrdinalIgnoreCase) || (_otherElementArrays.Contains(elementName) && !_singleMergeArrays.Contains(elementName)))
            {
                if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
                {
                    string nextIndex;

                    if (existingData.HasNumericalIndex)
                        nextIndex = (existingData.ElementDataPairs.Keys.Max(int.Parse) + 1).ToString();
                    else
                        nextIndex = existingData.ElementDataPairs.Keys.Count.ToString();

                    existingData.ElementDataPairs[nextIndex] = new StormElementData(existingData, nextIndex, element, true, true);
                }
                else
                {
                    ElementDataPairs[elementName] = new StormElementData(this, elementName, element, "0", true)
                    {
                        HasNumericalIndex = true,
                    };
                }
            }
            else if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
            {
                existingData.AddXElement(element);
            }
            else if (string.IsNullOrEmpty(valueAtt))
            {
                ElementDataPairs[elementName] = new StormElementData(this, elementName, element);
            }
            else
            {
                ElementDataPairs[elementName] = new StormElementData(this, elementName, valueAtt);
            }
        }
    }
}
